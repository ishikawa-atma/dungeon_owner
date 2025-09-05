using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Data;
using DungeonOwner.Monsters;
using DungeonOwner.PlayerCharacters;

namespace DungeonOwner.Core
{
    /// <summary>
    /// ゲーム開始時の初期モンスター配置を管理するクラス
    /// 要件3.1, 3.2に基づいて1階層に自キャラクター1体+モンスター14体を配置
    /// </summary>
    public class InitialMonsterPlacer : MonoBehaviour
    {
        [Header("Initial Placement Configuration")]
        [SerializeField] private PlayerCharacterType selectedPlayerCharacterType = PlayerCharacterType.Warrior;
        [SerializeField] private bool placeOnStart = false; // 手動制御用
        
        [Header("Monster Composition (Total: 14)")]
        [SerializeField] private int slimeCount = 5;
        [SerializeField] private int lesserSkeletonCount = 3;
        [SerializeField] private int lesserGhostCount = 3;
        [SerializeField] private int lesserGolemCount = 1;
        [SerializeField] private int goblinCount = 1;
        [SerializeField] private int lesserWolfCount = 1;
        
        [Header("Placement Settings")]
        [SerializeField] private float placementRadius = 3f;
        [SerializeField] private Vector2 centerPosition = Vector2.zero;
        [SerializeField] private bool avoidStairPositions = true;
        
        private List<GameObject> placedMonsters = new List<GameObject>();
        private GameObject placedPlayerCharacter;
        
        public bool IsPlacementComplete { get; private set; } = false;
        
        // イベント
        public System.Action OnPlacementComplete;
        public System.Action<GameObject> OnMonsterPlaced;
        public System.Action<GameObject> OnPlayerCharacterPlaced;

        private void Start()
        {
            if (placeOnStart)
            {
                PlaceInitialMonsters();
            }
        }

        /// <summary>
        /// 初期モンスター配置を実行
        /// </summary>
        public void PlaceInitialMonsters()
        {
            if (IsPlacementComplete)
            {
                Debug.LogWarning("Initial monsters already placed");
                return;
            }

            // 必要なシステムの確認
            if (!ValidateRequiredSystems())
            {
                Debug.LogError("Required systems not available for monster placement");
                return;
            }

            Debug.Log("Starting initial monster placement...");

            // 配置数の検証
            if (!ValidateMonsterCounts())
            {
                Debug.LogError("Invalid monster counts for initial placement");
                return;
            }

            // 1階層への配置
            Floor firstFloor = FloorSystem.Instance.GetFloor(1);
            if (firstFloor == null)
            {
                Debug.LogError("First floor not found");
                return;
            }

            // 配置位置の生成
            List<Vector2> placementPositions = GeneratePlacementPositions(15); // 自キャラ1 + モンスター14

            if (placementPositions.Count < 15)
            {
                Debug.LogError($"Not enough valid placement positions. Required: 15, Available: {placementPositions.Count}");
                return;
            }

            // プレイヤーキャラクターを配置
            PlacePlayerCharacter(placementPositions[0]);

            // モンスターを配置
            int positionIndex = 1;
            positionIndex = PlaceMonstersByType(MonsterType.Slime, slimeCount, placementPositions, positionIndex);
            positionIndex = PlaceMonstersByType(MonsterType.LesserSkeleton, lesserSkeletonCount, placementPositions, positionIndex);
            positionIndex = PlaceMonstersByType(MonsterType.LesserGhost, lesserGhostCount, placementPositions, positionIndex);
            positionIndex = PlaceMonstersByType(MonsterType.LesserGolem, lesserGolemCount, placementPositions, positionIndex);
            positionIndex = PlaceMonstersByType(MonsterType.Goblin, goblinCount, placementPositions, positionIndex);
            positionIndex = PlaceMonstersByType(MonsterType.LesserWolf, lesserWolfCount, placementPositions, positionIndex);

            IsPlacementComplete = true;
            OnPlacementComplete?.Invoke();

            Debug.Log($"Initial placement complete! Placed {placedMonsters.Count} monsters and 1 player character on floor 1");
            PrintPlacementSummary();
        }

        private bool ValidateRequiredSystems()
        {
            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem not found");
                return false;
            }

            if (DataManager.Instance == null)
            {
                Debug.LogError("DataManager not found");
                return false;
            }

            return true;
        }

        private bool ValidateMonsterCounts()
        {
            int totalMonsters = slimeCount + lesserSkeletonCount + lesserGhostCount + 
                              lesserGolemCount + goblinCount + lesserWolfCount;
            
            if (totalMonsters != 14)
            {
                Debug.LogError($"Total monster count must be 14, but got {totalMonsters}");
                return false;
            }

            return true;
        }

        private List<Vector2> GeneratePlacementPositions(int count)
        {
            List<Vector2> positions = new List<Vector2>();
            Floor firstFloor = FloorSystem.Instance.GetFloor(1);
            
            // 階段位置を取得（回避用）
            Vector2 upStairPos = firstFloor.upStairPosition;
            Vector2 downStairPos = firstFloor.downStairPosition;
            
            int attempts = 0;
            int maxAttempts = count * 10; // 十分な試行回数
            
            while (positions.Count < count && attempts < maxAttempts)
            {
                attempts++;
                
                // ランダムな位置を生成
                Vector2 candidatePos = GenerateRandomPosition();
                
                // 階段位置との距離チェック
                if (avoidStairPositions)
                {
                    if (Vector2.Distance(candidatePos, upStairPos) < 1f ||
                        Vector2.Distance(candidatePos, downStairPos) < 1f)
                    {
                        continue;
                    }
                }
                
                // 他の配置位置との距離チェック
                bool tooClose = false;
                foreach (var existingPos in positions)
                {
                    if (Vector2.Distance(candidatePos, existingPos) < 0.8f)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    positions.Add(candidatePos);
                }
            }
            
            return positions;
        }

        private Vector2 GenerateRandomPosition()
        {
            // 中心位置を基準に円形範囲内でランダム配置
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(0.5f, placementRadius);
            
            Vector2 offset = new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );
            
            return centerPosition + offset;
        }

        private void PlacePlayerCharacter(Vector2 position)
        {
            PlayerCharacterData characterData = DataManager.Instance.GetPlayerCharacterData(selectedPlayerCharacterType);
            if (characterData == null)
            {
                Debug.LogError($"PlayerCharacterData not found for type: {selectedPlayerCharacterType}");
                return;
            }

            if (characterData.prefab == null)
            {
                Debug.LogError($"Prefab not assigned for player character: {selectedPlayerCharacterType}");
                return;
            }

            // プレイヤーキャラクターを生成
            GameObject playerCharacterObj = Instantiate(characterData.prefab);
            playerCharacterObj.transform.position = new Vector3(position.x, position.y, 0);
            playerCharacterObj.name = $"PlayerCharacter_{selectedPlayerCharacterType}";

            // BasePlayerCharacterコンポーネントの設定
            BasePlayerCharacter playerCharacter = playerCharacterObj.GetComponent<BasePlayerCharacter>();
            if (playerCharacter != null)
            {
                playerCharacter.SetCharacterData(characterData);
            }

            // FloorSystemに登録
            bool placed = FloorSystem.Instance.PlaceMonster(1, playerCharacterObj, position);
            if (placed)
            {
                placedPlayerCharacter = playerCharacterObj;
                OnPlayerCharacterPlaced?.Invoke(playerCharacterObj);
                Debug.Log($"Placed player character {selectedPlayerCharacterType} at position {position}");
            }
            else
            {
                Debug.LogError($"Failed to place player character at position {position}");
                Destroy(playerCharacterObj);
            }
        }

        private int PlaceMonstersByType(MonsterType monsterType, int count, List<Vector2> positions, int startIndex)
        {
            MonsterData monsterData = DataManager.Instance.GetMonsterData(monsterType);
            if (monsterData == null)
            {
                Debug.LogError($"MonsterData not found for type: {monsterType}");
                return startIndex;
            }

            if (monsterData.prefab == null)
            {
                Debug.LogError($"Prefab not assigned for monster: {monsterType}");
                return startIndex;
            }

            for (int i = 0; i < count; i++)
            {
                if (startIndex >= positions.Count)
                {
                    Debug.LogError($"Not enough positions for monster placement. Index: {startIndex}, Available: {positions.Count}");
                    break;
                }

                Vector2 position = positions[startIndex];
                
                // モンスターを生成
                GameObject monsterObj = Instantiate(monsterData.prefab);
                monsterObj.transform.position = new Vector3(position.x, position.y, 0);
                monsterObj.name = $"{monsterType}_{i + 1}";

                // BaseMonsterコンポーネントの設定
                BaseMonster monster = monsterObj.GetComponent<BaseMonster>();
                if (monster != null)
                {
                    monster.SetMonsterData(monsterData);
                }

                // FloorSystemに登録
                bool placed = FloorSystem.Instance.PlaceMonster(1, monsterObj, position);
                if (placed)
                {
                    placedMonsters.Add(monsterObj);
                    OnMonsterPlaced?.Invoke(monsterObj);
                    Debug.Log($"Placed {monsterType} at position {position}");
                }
                else
                {
                    Debug.LogError($"Failed to place {monsterType} at position {position}");
                    Destroy(monsterObj);
                }

                startIndex++;
            }

            return startIndex;
        }

        private void PrintPlacementSummary()
        {
            Debug.Log("=== Initial Placement Summary ===");
            Debug.Log($"Player Character: {selectedPlayerCharacterType}");
            Debug.Log($"Total Monsters Placed: {placedMonsters.Count}");
            Debug.Log($"Slimes: {slimeCount}");
            Debug.Log($"Lesser Skeletons: {lesserSkeletonCount}");
            Debug.Log($"Lesser Ghosts: {lesserGhostCount}");
            Debug.Log($"Lesser Golems: {lesserGolemCount}");
            Debug.Log($"Goblins: {goblinCount}");
            Debug.Log($"Lesser Wolves: {lesserWolfCount}");
        }

        /// <summary>
        /// 配置されたモンスターをクリア（リセット用）
        /// </summary>
        public void ClearPlacedMonsters()
        {
            foreach (var monster in placedMonsters)
            {
                if (monster != null)
                {
                    FloorSystem.Instance.RemoveMonster(1, monster);
                    Destroy(monster);
                }
            }
            placedMonsters.Clear();

            if (placedPlayerCharacter != null)
            {
                FloorSystem.Instance.RemoveMonster(1, placedPlayerCharacter);
                Destroy(placedPlayerCharacter);
                placedPlayerCharacter = null;
            }

            IsPlacementComplete = false;
            Debug.Log("Cleared all placed monsters and player character");
        }

        /// <summary>
        /// プレイヤーキャラクタータイプを設定
        /// </summary>
        public void SetPlayerCharacterType(PlayerCharacterType characterType)
        {
            selectedPlayerCharacterType = characterType;
            Debug.Log($"Selected player character type: {characterType}");
        }

        /// <summary>
        /// 配置されたモンスターのリストを取得
        /// </summary>
        public List<GameObject> GetPlacedMonsters()
        {
            return new List<GameObject>(placedMonsters);
        }

        /// <summary>
        /// 配置されたプレイヤーキャラクターを取得
        /// </summary>
        public GameObject GetPlacedPlayerCharacter()
        {
            return placedPlayerCharacter;
        }

        // デバッグ用メソッド
        [ContextMenu("Place Initial Monsters")]
        public void DebugPlaceMonsters()
        {
            PlaceInitialMonsters();
        }

        [ContextMenu("Clear Placed Monsters")]
        public void DebugClearMonsters()
        {
            ClearPlacedMonsters();
        }

        private void OnDrawGizmosSelected()
        {
            // 配置範囲を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPosition, placementRadius);
            
            // 中心位置を表示
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centerPosition, 0.2f);
        }
    }
}