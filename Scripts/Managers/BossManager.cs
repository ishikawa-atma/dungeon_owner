using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Monsters;
using DungeonOwner.Core;

namespace DungeonOwner.Managers
{
    /// <summary>
    /// ボスキャラクターシステムの管理クラス
    /// 5階層ごとのボス配置、選択式UI、リポップ管理を担当
    /// </summary>
    public class BossManager : MonoBehaviour
    {
        public static BossManager Instance { get; private set; }

        [Header("Boss Configuration")]
        [SerializeField] private List<BossData> availableBosses = new List<BossData>();
        [SerializeField] private Transform bossContainer;
        [SerializeField] private int bossFloorInterval = 5; // 5階層ごと

        [Header("Current State")]
        [SerializeField] private Dictionary<int, IBoss> activeBosses = new Dictionary<int, IBoss>();
        [SerializeField] private Dictionary<int, BossType> floorBossTypes = new Dictionary<int, BossType>();

        // イベント
        public System.Action<IBoss, int> OnBossPlaced;
        public System.Action<IBoss, int> OnBossDefeated;
        public System.Action<IBoss, int> OnBossRespawned;
        public System.Action<int> OnBossFloorAvailable;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeBossManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // FloorSystemのイベントに登録
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorExpanded += OnFloorExpanded;
            }
        }

        private void InitializeBossManager()
        {
            if (bossContainer == null)
            {
                GameObject container = new GameObject("BossContainer");
                bossContainer = container.transform;
                bossContainer.SetParent(transform);
            }

            Debug.Log("BossManager initialized");
        }

        /// <summary>
        /// 指定階層がボス階層かどうかチェック
        /// 要件6.1: 5階層ごとにボスキャラ配置オプションを表示
        /// </summary>
        public bool IsBossFloor(int floorIndex)
        {
            return floorIndex > 0 && floorIndex % bossFloorInterval == 0;
        }

        /// <summary>
        /// 指定階層にボスが配置可能かチェック
        /// </summary>
        public bool CanPlaceBoss(int floorIndex)
        {
            // ボス階層でない場合は配置不可
            if (!IsBossFloor(floorIndex))
            {
                return false;
            }

            // 既にボスが配置されている場合は配置不可
            if (HasBossOnFloor(floorIndex))
            {
                return false;
            }

            // 階層が存在するかチェック
            if (FloorSystem.Instance == null || FloorSystem.Instance.GetFloor(floorIndex) == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 指定階層にボスが配置されているかチェック
        /// </summary>
        public bool HasBossOnFloor(int floorIndex)
        {
            return activeBosses.ContainsKey(floorIndex) && activeBosses[floorIndex] != null;
        }

        /// <summary>
        /// 指定階層のボスを取得
        /// </summary>
        public IBoss GetBossOnFloor(int floorIndex)
        {
            return activeBosses.ContainsKey(floorIndex) ? activeBosses[floorIndex] : null;
        }

        /// <summary>
        /// 配置可能なボスタイプのリストを取得
        /// 要件6.2: 選択式のボスキャラリストを表示
        /// </summary>
        public List<BossData> GetAvailableBossesForFloor(int floorIndex)
        {
            return availableBosses.Where(boss => boss.CanPlaceOnFloor(floorIndex)).ToList();
        }

        /// <summary>
        /// ボスを配置
        /// 要件6.2: プレイヤーがボスキャラを選択
        /// </summary>
        public IBoss PlaceBoss(int floorIndex, BossType bossType, Vector2 position, int level = 1)
        {
            if (!CanPlaceBoss(floorIndex))
            {
                Debug.LogWarning($"Cannot place boss on floor {floorIndex}");
                return null;
            }

            BossData bossData = availableBosses.FirstOrDefault(b => b.type == bossType);
            if (bossData == null || bossData.prefab == null)
            {
                Debug.LogError($"Boss data or prefab not found for {bossType}");
                return null;
            }

            // ボスオブジェクトを生成
            GameObject bossObj = Instantiate(bossData.prefab, bossContainer);
            bossObj.transform.position = new Vector3(position.x, position.y, 0);
            bossObj.name = $"Boss_{bossType}_{floorIndex}";

            // ボスコンポーネントを取得・設定
            IBoss boss = bossObj.GetComponent<IBoss>();
            if (boss == null)
            {
                Debug.LogError($"Boss prefab for {bossType} does not have IBoss component");
                Destroy(bossObj);
                return null;
            }

            // BaseBossコンポーネントにデータを設定
            BaseBoss baseBoss = bossObj.GetComponent<BaseBoss>();
            if (baseBoss != null)
            {
                baseBoss.SetBossData(bossData);
                baseBoss.Level = level;

                // イベント登録
                baseBoss.OnBossDefeated += (defeatedBoss) => HandleBossDefeated(defeatedBoss, floorIndex);
                baseBoss.OnBossRespawned += (respawnedBoss) => HandleBossRespawned(respawnedBoss, floorIndex);
            }

            // 階層システムに登録
            if (!FloorSystem.Instance.PlaceMonster(floorIndex, bossObj, position))
            {
                Debug.LogError($"Failed to place boss in FloorSystem");
                Destroy(bossObj);
                return null;
            }

            // 階層にボス情報を設定
            Floor floor = FloorSystem.Instance.GetFloor(floorIndex);
            if (floor != null)
            {
                floor.SetBoss(bossType, level);
            }

            // 内部管理に追加
            activeBosses[floorIndex] = boss;
            floorBossTypes[floorIndex] = bossType;

            // イベント発火
            OnBossPlaced?.Invoke(boss, floorIndex);

            Debug.Log($"Placed boss {bossType} on floor {floorIndex} at level {level}");
            return boss;
        }

        /// <summary>
        /// ボスを除去
        /// </summary>
        public bool RemoveBoss(int floorIndex)
        {
            if (!HasBossOnFloor(floorIndex))
            {
                return false;
            }

            IBoss boss = activeBosses[floorIndex];
            GameObject bossObj = (boss as MonoBehaviour)?.gameObject;

            if (bossObj != null)
            {
                // 階層システムから除去
                FloorSystem.Instance.RemoveMonster(floorIndex, bossObj);

                // 階層からボス情報を除去
                Floor floor = FloorSystem.Instance.GetFloor(floorIndex);
                if (floor != null)
                {
                    floor.RemoveBoss();
                }

                // オブジェクト破棄
                Destroy(bossObj);
            }

            // 内部管理から除去
            activeBosses.Remove(floorIndex);
            floorBossTypes.Remove(floorIndex);

            Debug.Log($"Removed boss from floor {floorIndex}");
            return true;
        }

        /// <summary>
        /// ボス撃破時の処理
        /// 要件6.4: 撃破後の再リポップ
        /// </summary>
        private void HandleBossDefeated(IBoss boss, int floorIndex)
        {
            // 報酬処理
            if (ResourceManager.Instance != null)
            {
                BossData bossData = GetBossData(boss.BossType);
                if (bossData != null)
                {
                    int reward = bossData.GetGoldReward(boss.Level);
                    ResourceManager.Instance.AddGold(reward);
                }
            }

            OnBossDefeated?.Invoke(boss, floorIndex);

            Debug.Log($"Boss {boss.BossType} defeated on floor {floorIndex} (Level {boss.Level})");
        }

        /// <summary>
        /// ボスリポップ時の処理
        /// 要件6.5: レベル引き継ぎ
        /// </summary>
        private void HandleBossRespawned(IBoss boss, int floorIndex)
        {
            // 階層のボス情報を更新
            Floor floor = FloorSystem.Instance.GetFloor(floorIndex);
            if (floor != null)
            {
                floor.SetBoss(boss.BossType, boss.Level);
            }

            OnBossRespawned?.Invoke(boss, floorIndex);

            Debug.Log($"Boss {boss.BossType} respawned on floor {floorIndex} (Level {boss.Level})");
        }

        /// <summary>
        /// 階層拡張時の処理
        /// </summary>
        private void OnFloorExpanded(int newFloorIndex)
        {
            if (IsBossFloor(newFloorIndex))
            {
                OnBossFloorAvailable?.Invoke(newFloorIndex);
                Debug.Log($"Boss floor {newFloorIndex} is now available");
            }
        }

        /// <summary>
        /// ボスデータを取得
        /// </summary>
        public BossData GetBossData(BossType bossType)
        {
            return availableBosses.FirstOrDefault(b => b.type == bossType);
        }

        /// <summary>
        /// 全ボスデータを取得
        /// </summary>
        public List<BossData> GetAllBossData()
        {
            return new List<BossData>(availableBosses);
        }

        /// <summary>
        /// アクティブなボスのリストを取得
        /// </summary>
        public List<IBoss> GetActiveBosses()
        {
            return activeBosses.Values.Where(b => b != null).ToList();
        }

        /// <summary>
        /// 指定階層のボスタイプを取得
        /// </summary>
        public BossType? GetBossTypeOnFloor(int floorIndex)
        {
            return floorBossTypes.ContainsKey(floorIndex) ? floorBossTypes[floorIndex] : (BossType?)null;
        }

        /// <summary>
        /// リポップ中のボスがあるかチェック
        /// </summary>
        public bool HasRespawningBoss()
        {
            return activeBosses.Values.Any(boss => boss != null && boss.IsRespawning);
        }

        /// <summary>
        /// 指定階層のボスがリポップ中かチェック
        /// </summary>
        public bool IsBossRespawning(int floorIndex)
        {
            IBoss boss = GetBossOnFloor(floorIndex);
            return boss != null && boss.IsRespawning;
        }

        /// <summary>
        /// 指定階層のボスリポップ進行度を取得
        /// </summary>
        public float GetBossRespawnProgress(int floorIndex)
        {
            IBoss boss = GetBossOnFloor(floorIndex);
            return boss?.RespawnProgress ?? 0f;
        }

        /// <summary>
        /// セーブデータ用: ボス配置情報を取得
        /// </summary>
        public Dictionary<int, (BossType type, int level, bool isRespawning, float progress)> GetBossSaveData()
        {
            var saveData = new Dictionary<int, (BossType, int, bool, float)>();

            foreach (var kvp in activeBosses)
            {
                int floorIndex = kvp.Key;
                IBoss boss = kvp.Value;

                if (boss != null)
                {
                    saveData[floorIndex] = (boss.BossType, boss.Level, boss.IsRespawning, boss.RespawnProgress);
                }
            }

            return saveData;
        }

        /// <summary>
        /// セーブデータ用: ボス配置情報を復元
        /// </summary>
        public void LoadBossSaveData(Dictionary<int, (BossType type, int level, bool isRespawning, float progress)> saveData)
        {
            foreach (var kvp in saveData)
            {
                int floorIndex = kvp.Key;
                var (type, level, isRespawning, progress) = kvp.Value;

                // ボス階層でない場合はスキップ
                if (!IsBossFloor(floorIndex))
                {
                    continue;
                }

                // 階層が存在しない場合はスキップ
                Floor floor = FloorSystem.Instance?.GetFloor(floorIndex);
                if (floor == null)
                {
                    continue;
                }

                // ボスを配置
                Vector2 position = new Vector2(0, 0); // デフォルト位置
                IBoss boss = PlaceBoss(floorIndex, type, position, level);

                if (boss != null && isRespawning)
                {
                    // リポップ状態を復元
                    boss.StartRespawn();
                    // 進行度は内部で管理されるため、ここでは設定しない
                }
            }
        }

        /// <summary>
        /// デバッグ用メソッド
        /// </summary>
        public void DebugPrintBossInfo()
        {
            Debug.Log("=== Boss Manager Info ===");
            Debug.Log($"Active Bosses: {activeBosses.Count}");

            foreach (var kvp in activeBosses)
            {
                int floorIndex = kvp.Key;
                IBoss boss = kvp.Value;

                if (boss != null)
                {
                    string status = boss.IsRespawning ? $"Respawning ({boss.RespawnProgress:P})" : "Active";
                    Debug.Log($"Floor {floorIndex}: {boss.BossType} (Level {boss.Level}) - {status}");
                }
            }
        }

        /// <summary>
        /// デバッグ用: 全ボスを強制リポップ
        /// </summary>
        public void DebugForceRespawnAllBosses()
        {
            foreach (var boss in activeBosses.Values)
            {
                if (boss is BaseBoss baseBoss)
                {
                    baseBoss.DebugForceRespawn();
                }
            }
        }

        private void OnDestroy()
        {
            // イベント登録解除
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorExpanded -= OnFloorExpanded;
            }
        }
    }
}