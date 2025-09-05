using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonOwner.Data;
using DungeonOwner.Managers;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 階層拡張システム
    /// 要件10.4, 10.5, 12.4に対応
    /// </summary>
    public class FloorExpansionSystem : MonoBehaviour
    {
        public static FloorExpansionSystem Instance { get; private set; }

        [Header("Expansion Settings")]
        [SerializeField] private int baseCost = 200;
        [SerializeField] private float costMultiplier = 1.5f;
        [SerializeField] private int maxFloors = 100;

        [Header("Monster Unlock Settings")]
        [SerializeField] private List<MonsterUnlockData> monsterUnlocks = new List<MonsterUnlockData>();

        // イベント
        public System.Action<int> OnFloorExpanded;
        public System.Action<int, int> OnExpansionCostCalculated; // floor, cost
        public System.Action<List<MonsterType>> OnNewMonstersUnlocked;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeExpansionSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeExpansionSystem()
        {
            // デフォルトのモンスター解放設定
            if (monsterUnlocks.Count == 0)
            {
                SetupDefaultMonsterUnlocks();
            }

            Debug.Log("FloorExpansionSystem initialized");
        }

        /// <summary>
        /// デフォルトのモンスター解放設定
        /// </summary>
        private void SetupDefaultMonsterUnlocks()
        {
            monsterUnlocks.Add(new MonsterUnlockData { floorLevel = 5, unlockedMonsters = new List<MonsterType> { MonsterType.GreaterSlime } });
            monsterUnlocks.Add(new MonsterUnlockData { floorLevel = 8, unlockedMonsters = new List<MonsterType> { MonsterType.Skeleton } });
            monsterUnlocks.Add(new MonsterUnlockData { floorLevel = 10, unlockedMonsters = new List<MonsterType> { MonsterType.Ghost, MonsterType.Golem } });
            monsterUnlocks.Add(new MonsterUnlockData { floorLevel = 15, unlockedMonsters = new List<MonsterType> { MonsterType.Orc, MonsterType.Wolf } });
            monsterUnlocks.Add(new MonsterUnlockData { floorLevel = 20, unlockedMonsters = new List<MonsterType> { MonsterType.Dragon } });
            monsterUnlocks.Add(new MonsterUnlockData { floorLevel = 25, unlockedMonsters = new List<MonsterType> { MonsterType.Lich } });
            monsterUnlocks.Add(new MonsterUnlockData { floorLevel = 30, unlockedMonsters = new List<MonsterType> { MonsterType.Demon } });
        }

        /// <summary>
        /// 階層拡張が可能かチェック
        /// </summary>
        public bool CanExpandFloor()
        {
            if (FloorSystem.Instance == null)
            {
                return false;
            }

            int currentFloorCount = FloorSystem.Instance.CurrentFloorCount;
            return currentFloorCount < maxFloors;
        }

        /// <summary>
        /// 階層拡張コストを計算
        /// 要件12.4: 階層数に応じてコストが増加
        /// </summary>
        public int CalculateExpansionCost(int targetFloor)
        {
            if (targetFloor <= 3)
            {
                return 0; // 初期3階層は無料
            }

            // 指数的にコストが増加
            float cost = baseCost * Mathf.Pow(costMultiplier, targetFloor - 3);
            int finalCost = Mathf.RoundToInt(cost);

            OnExpansionCostCalculated?.Invoke(targetFloor, finalCost);
            return finalCost;
        }

        /// <summary>
        /// 階層拡張を実行
        /// 要件10.4, 10.5: 新階層生成と階段配置
        /// </summary>
        public bool TryExpandFloor()
        {
            if (!CanExpandFloor())
            {
                Debug.LogWarning("Cannot expand floor: maximum floors reached or FloorSystem not available");
                return false;
            }

            if (FloorSystem.Instance == null || ResourceManager.Instance == null)
            {
                Debug.LogError("Required managers not available for floor expansion");
                return false;
            }

            int currentFloorCount = FloorSystem.Instance.CurrentFloorCount;
            int targetFloor = currentFloorCount + 1;
            int expansionCost = CalculateExpansionCost(targetFloor);

            // コストチェック
            if (!ResourceManager.Instance.CanAfford(expansionCost))
            {
                Debug.Log($"Cannot afford floor expansion. Cost: {expansionCost}, Available: {ResourceManager.Instance.Gold}");
                return false;
            }

            // 金貨を消費
            if (!ResourceManager.Instance.SpendGold(expansionCost))
            {
                Debug.LogError("Failed to spend gold for floor expansion");
                return false;
            }

            // 階層を拡張
            bool expansionSuccess = FloorSystem.Instance.ExpandFloor();
            if (!expansionSuccess)
            {
                // 失敗した場合は金貨を返還
                ResourceManager.Instance.AddGold(expansionCost);
                Debug.LogError("Floor expansion failed, gold refunded");
                return false;
            }

            // 新モンスター解放チェック
            CheckAndUnlockNewMonsters(targetFloor);

            OnFloorExpanded?.Invoke(targetFloor);
            Debug.Log($"Floor expanded to {targetFloor} for {expansionCost} gold");

            return true;
        }

        /// <summary>
        /// 新モンスター解放チェック
        /// 拡張時の新モンスター解放機能
        /// </summary>
        private void CheckAndUnlockNewMonsters(int newFloorLevel)
        {
            var unlockData = monsterUnlocks.FirstOrDefault(u => u.floorLevel == newFloorLevel);
            if (unlockData != null && unlockData.unlockedMonsters.Count > 0)
            {
                OnNewMonstersUnlocked?.Invoke(unlockData.unlockedMonsters);
                
                string monsterNames = string.Join(", ", unlockData.unlockedMonsters.Select(m => m.ToString()));
                Debug.Log($"New monsters unlocked at floor {newFloorLevel}: {monsterNames}");
            }
        }

        /// <summary>
        /// 指定階層で解放されるモンスターリストを取得
        /// </summary>
        public List<MonsterType> GetUnlockedMonstersAtFloor(int floorLevel)
        {
            var unlockData = monsterUnlocks.FirstOrDefault(u => u.floorLevel == floorLevel);
            return unlockData?.unlockedMonsters ?? new List<MonsterType>();
        }

        /// <summary>
        /// 現在利用可能なモンスターリストを取得
        /// </summary>
        public List<MonsterType> GetAvailableMonsters()
        {
            if (FloorSystem.Instance == null)
            {
                return new List<MonsterType>();
            }

            int currentFloorCount = FloorSystem.Instance.CurrentFloorCount;
            var availableMonsters = new List<MonsterType>();

            // 基本モンスター（常に利用可能）
            availableMonsters.AddRange(new[]
            {
                MonsterType.Slime,
                MonsterType.LesserSkeleton,
                MonsterType.LesserGhost,
                MonsterType.LesserGolem,
                MonsterType.Goblin,
                MonsterType.LesserWolf
            });

            // 階層に応じて解放されたモンスターを追加
            foreach (var unlockData in monsterUnlocks)
            {
                if (currentFloorCount >= unlockData.floorLevel)
                {
                    availableMonsters.AddRange(unlockData.unlockedMonsters);
                }
            }

            return availableMonsters.Distinct().ToList();
        }

        /// <summary>
        /// 次の階層拡張で解放されるモンスターを取得
        /// </summary>
        public List<MonsterType> GetNextUnlockableMonsters()
        {
            if (FloorSystem.Instance == null)
            {
                return new List<MonsterType>();
            }

            int nextFloorLevel = FloorSystem.Instance.CurrentFloorCount + 1;
            return GetUnlockedMonstersAtFloor(nextFloorLevel);
        }

        /// <summary>
        /// モンスター解放設定を追加
        /// </summary>
        public void AddMonsterUnlock(int floorLevel, List<MonsterType> monsters)
        {
            var existingUnlock = monsterUnlocks.FirstOrDefault(u => u.floorLevel == floorLevel);
            if (existingUnlock != null)
            {
                existingUnlock.unlockedMonsters.AddRange(monsters);
                existingUnlock.unlockedMonsters = existingUnlock.unlockedMonsters.Distinct().ToList();
            }
            else
            {
                monsterUnlocks.Add(new MonsterUnlockData
                {
                    floorLevel = floorLevel,
                    unlockedMonsters = new List<MonsterType>(monsters)
                });
            }

            // 階層順でソート
            monsterUnlocks = monsterUnlocks.OrderBy(u => u.floorLevel).ToList();
        }

        /// <summary>
        /// 拡張設定を変更
        /// </summary>
        public void SetExpansionSettings(int newBaseCost, float newCostMultiplier, int newMaxFloors)
        {
            baseCost = newBaseCost;
            costMultiplier = newCostMultiplier;
            maxFloors = newMaxFloors;
        }

        /// <summary>
        /// デバッグ用メソッド
        /// </summary>
        public void DebugPrintExpansionInfo()
        {
            Debug.Log($"=== Floor Expansion System Info ===");
            Debug.Log($"Base Cost: {baseCost}");
            Debug.Log($"Cost Multiplier: {costMultiplier}");
            Debug.Log($"Max Floors: {maxFloors}");
            Debug.Log($"Monster Unlocks: {monsterUnlocks.Count}");

            foreach (var unlock in monsterUnlocks)
            {
                string monsters = string.Join(", ", unlock.unlockedMonsters.Select(m => m.ToString()));
                Debug.Log($"Floor {unlock.floorLevel}: {monsters}");
            }
        }
    }

    /// <summary>
    /// モンスター解放データ
    /// </summary>
    [System.Serializable]
    public class MonsterUnlockData
    {
        public int floorLevel;
        public List<MonsterType> unlockedMonsters = new List<MonsterType>();
    }
}