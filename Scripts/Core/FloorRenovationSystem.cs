using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 階層改装システム
    /// 空階層での改装モード有効化、階段経路確保の制約システム、改装UIとレイアウト保存機能を提供
    /// </summary>
    public class FloorRenovationSystem : MonoBehaviour
    {
        public static FloorRenovationSystem Instance { get; private set; }

        [Header("Renovation Configuration")]
        [SerializeField] private bool isRenovationMode = false;
        [SerializeField] private int currentRenovationFloor = -1;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private Material renovationHighlightMaterial;
        [SerializeField] private LayerMask renovationLayer = 1 << 8; // Renovation layer

        [Header("Path Validation")]
        [SerializeField] private bool validatePathOnEdit = true;
        [SerializeField] private float pathfindingTimeout = 1.0f;

        // 改装状態
        private List<Vector2> temporaryWalls = new List<Vector2>();
        private List<Vector2> originalWalls = new List<Vector2>();
        private Dictionary<Vector2, GameObject> wallObjects = new Dictionary<Vector2, GameObject>();

        // イベント
        public System.Action<int> OnRenovationModeStarted;
        public System.Action<int> OnRenovationModeEnded;
        public System.Action<int, List<Vector2>> OnLayoutSaved;
        public System.Action<Vector2> OnWallPlaced;
        public System.Action<Vector2> OnWallRemoved;
        public System.Action<string> OnRenovationError;

        // プロパティ
        public bool IsRenovationMode => isRenovationMode;
        public int CurrentRenovationFloor => currentRenovationFloor;
        public List<Vector2> TemporaryWalls => new List<Vector2>(temporaryWalls);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeRenovationSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeRenovationSystem()
        {
            // FloorSystemのイベントを購読
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged += OnFloorChanged;
            }

            Debug.Log("FloorRenovationSystem initialized");
        }

        private void OnDestroy()
        {
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged -= OnFloorChanged;
            }
        }

        /// <summary>
        /// 指定階層で改装モードを開始できるかチェック
        /// 要件11.1: 階層に侵入者とモンスターが存在しない場合のみ改装モード有効化
        /// </summary>
        public bool CanStartRenovation(int floorIndex)
        {
            if (FloorSystem.Instance == null)
            {
                return false;
            }

            Floor floor = FloorSystem.Instance.GetFloor(floorIndex);
            if (floor == null)
            {
                return false;
            }

            // 階層が空かどうかチェック
            bool isEmpty = floor.IsEmpty();
            
            if (!isEmpty)
            {
                Debug.Log($"Cannot start renovation on floor {floorIndex}: floor is not empty");
            }

            return isEmpty;
        }

        /// <summary>
        /// 改装モードを開始
        /// </summary>
        public bool StartRenovation(int floorIndex)
        {
            if (isRenovationMode)
            {
                Debug.LogWarning("Renovation mode is already active");
                return false;
            }

            if (!CanStartRenovation(floorIndex))
            {
                OnRenovationError?.Invoke($"階層{floorIndex}は改装できません。モンスターまたは侵入者が存在します。");
                return false;
            }

            Floor floor = FloorSystem.Instance.GetFloor(floorIndex);
            if (floor == null)
            {
                OnRenovationError?.Invoke($"階層{floorIndex}が見つかりません。");
                return false;
            }

            // 改装モード開始
            isRenovationMode = true;
            currentRenovationFloor = floorIndex;

            // 現在の壁配置を保存
            originalWalls.Clear();
            originalWalls.AddRange(floor.wallPositions);

            // 一時的な壁配置を初期化
            temporaryWalls.Clear();
            temporaryWalls.AddRange(floor.wallPositions);

            // 壁オブジェクトを生成
            CreateWallObjects();

            // 改装可能エリアをハイライト
            HighlightRenovationArea();

            OnRenovationModeStarted?.Invoke(floorIndex);
            Debug.Log($"Started renovation mode on floor {floorIndex}");

            return true;
        }

        /// <summary>
        /// 改装モードを終了
        /// </summary>
        public void EndRenovation(bool saveChanges = true)
        {
            if (!isRenovationMode)
            {
                return;
            }

            if (saveChanges)
            {
                SaveRenovationChanges();
            }
            else
            {
                // 変更を破棄
                temporaryWalls.Clear();
                temporaryWalls.AddRange(originalWalls);
            }

            // 改装モード終了
            isRenovationMode = false;
            int renovatedFloor = currentRenovationFloor;
            currentRenovationFloor = -1;

            // 壁オブジェクトを削除
            DestroyWallObjects();

            // ハイライトを削除
            RemoveRenovationHighlight();

            OnRenovationModeEnded?.Invoke(renovatedFloor);
            Debug.Log($"Ended renovation mode on floor {renovatedFloor}");
        }

        /// <summary>
        /// 壁を配置
        /// </summary>
        public bool PlaceWall(Vector2 position)
        {
            if (!isRenovationMode)
            {
                OnRenovationError?.Invoke("改装モードが有効ではありません。");
                return false;
            }

            // 階段位置チェック
            if (!CanPlaceWallAtPosition(position))
            {
                return false;
            }

            // 既に壁がある場合はスキップ
            if (temporaryWalls.Contains(position))
            {
                return false;
            }

            // 一時的に壁を追加
            temporaryWalls.Add(position);

            // 経路確保チェック
            if (validatePathOnEdit && !ValidateStairPath())
            {
                // 経路が確保できない場合は元に戻す
                temporaryWalls.Remove(position);
                OnRenovationError?.Invoke("階段への経路が確保できません。");
                return false;
            }

            // 壁オブジェクトを生成
            CreateWallObject(position);

            OnWallPlaced?.Invoke(position);
            Debug.Log($"Placed wall at {position}");

            return true;
        }

        /// <summary>
        /// 壁を除去
        /// </summary>
        public bool RemoveWall(Vector2 position)
        {
            if (!isRenovationMode)
            {
                OnRenovationError?.Invoke("改装モードが有効ではありません。");
                return false;
            }

            if (!temporaryWalls.Contains(position))
            {
                return false;
            }

            // 壁を除去
            temporaryWalls.Remove(position);

            // 壁オブジェクトを削除
            DestroyWallObject(position);

            OnWallRemoved?.Invoke(position);
            Debug.Log($"Removed wall at {position}");

            return true;
        }

        /// <summary>
        /// 指定位置に壁を配置できるかチェック
        /// 要件11.3: 階段を塞ぐ改装を無効化
        /// </summary>
        private bool CanPlaceWallAtPosition(Vector2 position)
        {
            Floor floor = FloorSystem.Instance.GetFloor(currentRenovationFloor);
            if (floor == null)
            {
                return false;
            }

            // 階段位置との重複チェック
            if (Vector2.Distance(position, floor.upStairPosition) < 1f)
            {
                OnRenovationError?.Invoke("上り階段の位置には壁を配置できません。");
                return false;
            }

            if (Vector2.Distance(position, floor.downStairPosition) < 1f)
            {
                OnRenovationError?.Invoke("下り階段の位置には壁を配置できません。");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 階段間の経路が確保されているかチェック
        /// 要件11.2: 上り階段と下り階段の経路を確保
        /// </summary>
        private bool ValidateStairPath()
        {
            Floor floor = FloorSystem.Instance.GetFloor(currentRenovationFloor);
            if (floor == null)
            {
                return false;
            }

            // 1階層の場合は下り階段のみチェック
            if (currentRenovationFloor == 1)
            {
                return true; // 上り階段がないので常にOK
            }

            // 最深部の場合は上り階段のみチェック
            if (floor.hasCore)
            {
                return true; // 下り階段がないので常にOK
            }

            // 上り階段から下り階段への経路をチェック
            return HasPathBetweenStairs(floor.upStairPosition, floor.downStairPosition);
        }

        /// <summary>
        /// 2点間に経路があるかチェック（簡易パスファインディング）
        /// </summary>
        private bool HasPathBetweenStairs(Vector2 start, Vector2 end)
        {
            // 簡易的なA*アルゴリズムを使用
            HashSet<Vector2> obstacles = new HashSet<Vector2>(temporaryWalls);
            Queue<Vector2> queue = new Queue<Vector2>();
            HashSet<Vector2> visited = new HashSet<Vector2>();

            queue.Enqueue(start);
            visited.Add(start);

            Vector2[] directions = {
                Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                new Vector2(1, 1), new Vector2(1, -1), new Vector2(-1, 1), new Vector2(-1, -1)
            };

            int maxIterations = 1000; // タイムアウト防止
            int iterations = 0;

            while (queue.Count > 0 && iterations < maxIterations)
            {
                iterations++;
                Vector2 current = queue.Dequeue();

                // 目標に到達
                if (Vector2.Distance(current, end) < 1f)
                {
                    return true;
                }

                // 隣接セルをチェック
                foreach (Vector2 direction in directions)
                {
                    Vector2 next = current + direction;

                    // 範囲外チェック
                    if (next.x < -10 || next.x > 10 || next.y < -10 || next.y > 10)
                    {
                        continue;
                    }

                    // 訪問済みまたは障害物チェック
                    if (visited.Contains(next) || obstacles.Contains(next))
                    {
                        continue;
                    }

                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }

            return false; // 経路が見つからない
        }

        /// <summary>
        /// 改装変更を保存
        /// 要件11.4: 新しいレイアウトを保存
        /// </summary>
        private void SaveRenovationChanges()
        {
            Floor floor = FloorSystem.Instance.GetFloor(currentRenovationFloor);
            if (floor == null)
            {
                return;
            }

            // 壁配置を更新
            floor.wallPositions.Clear();
            floor.wallPositions.AddRange(temporaryWalls);

            OnLayoutSaved?.Invoke(currentRenovationFloor, new List<Vector2>(temporaryWalls));
            Debug.Log($"Saved renovation changes for floor {currentRenovationFloor}");
        }

        /// <summary>
        /// 階層変更時の処理
        /// 要件11.5: 改装中に侵入者またはモンスターが出現した場合は改装モード終了
        /// </summary>
        private void OnFloorChanged(int newFloor)
        {
            if (isRenovationMode && newFloor != currentRenovationFloor)
            {
                // 他の階層に移動した場合は改装モードを終了
                EndRenovation(true);
            }
        }

        /// <summary>
        /// 改装中に侵入者やモンスターが出現した場合の処理
        /// </summary>
        public void OnCharacterSpawned(int floorIndex)
        {
            if (isRenovationMode && floorIndex == currentRenovationFloor)
            {
                Debug.Log($"Character spawned on renovation floor {floorIndex}, ending renovation mode");
                EndRenovation(true);
            }
        }

        /// <summary>
        /// 壁オブジェクトを生成
        /// </summary>
        private void CreateWallObjects()
        {
            foreach (Vector2 wallPos in temporaryWalls)
            {
                CreateWallObject(wallPos);
            }
        }

        /// <summary>
        /// 単一の壁オブジェクトを生成
        /// </summary>
        private void CreateWallObject(Vector2 position)
        {
            if (wallPrefab == null || wallObjects.ContainsKey(position))
            {
                return;
            }

            GameObject wallObj = Instantiate(wallPrefab);
            wallObj.transform.position = new Vector3(position.x, position.y, 0);
            wallObj.name = $"RenovationWall_{position.x}_{position.y}";
            wallObj.layer = Mathf.RoundToInt(Mathf.Log(renovationLayer.value, 2));

            wallObjects[position] = wallObj;
        }

        /// <summary>
        /// 壁オブジェクトを削除
        /// </summary>
        private void DestroyWallObjects()
        {
            foreach (var wallObj in wallObjects.Values)
            {
                if (wallObj != null)
                {
                    DestroyImmediate(wallObj);
                }
            }
            wallObjects.Clear();
        }

        /// <summary>
        /// 単一の壁オブジェクトを削除
        /// </summary>
        private void DestroyWallObject(Vector2 position)
        {
            if (wallObjects.TryGetValue(position, out GameObject wallObj))
            {
                if (wallObj != null)
                {
                    DestroyImmediate(wallObj);
                }
                wallObjects.Remove(position);
            }
        }

        /// <summary>
        /// 改装可能エリアをハイライト
        /// 要件15.5: 改装可能エリアをハイライト表示
        /// </summary>
        private void HighlightRenovationArea()
        {
            // TODO: 改装可能エリアのハイライト表示を実装
            Debug.Log("Highlighting renovation area");
        }

        /// <summary>
        /// 改装ハイライトを削除
        /// </summary>
        private void RemoveRenovationHighlight()
        {
            // TODO: ハイライト削除を実装
            Debug.Log("Removing renovation highlight");
        }

        /// <summary>
        /// 現在の改装状態をリセット
        /// </summary>
        public void ResetRenovation()
        {
            if (isRenovationMode)
            {
                EndRenovation(false);
            }

            temporaryWalls.Clear();
            originalWalls.Clear();
        }

        /// <summary>
        /// デバッグ情報を出力
        /// </summary>
        public void DebugPrintRenovationInfo()
        {
            Debug.Log($"=== Renovation System Info ===");
            Debug.Log($"Renovation Mode: {isRenovationMode}");
            Debug.Log($"Current Floor: {currentRenovationFloor}");
            Debug.Log($"Temporary Walls: {temporaryWalls.Count}");
            Debug.Log($"Original Walls: {originalWalls.Count}");
            Debug.Log($"Wall Objects: {wallObjects.Count}");
        }
    }
}