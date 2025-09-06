using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Monsters;
using DungeonOwner.Core;

namespace DungeonOwner.Managers
{
    public class MonsterPlacementManager : MonoBehaviour
    {
        public static MonsterPlacementManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private int maxMonstersPerFloor = 15;
        [SerializeField] private Transform monsterContainer;
        [SerializeField] private LayerMask placementLayerMask = -1;

        [Header("Monster Data")]
        [SerializeField] private List<MonsterData> availableMonsters = new List<MonsterData>();

        // イベント
        public System.Action<IMonster, int> OnMonsterPlaced;
        public System.Action<IMonster, int> OnMonsterRemoved;
        public System.Action<int, int> OnFloorMonsterCountChanged;

        // 内部状態
        private Dictionary<int, List<IMonster>> floorMonsters = new Dictionary<int, List<IMonster>>();
        private List<MonsterData> unlockedMonsters = new List<MonsterData>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeManager()
        {
            if (monsterContainer == null)
            {
                GameObject container = new GameObject("MonsterContainer");
                monsterContainer = container.transform;
                monsterContainer.SetParent(transform);
            }

            // 初期解放モンスターを設定
            UpdateUnlockedMonsters(1);

            Debug.Log("MonsterPlacementManager initialized");
        }

        public void UpdateUnlockedMonsters(int currentFloor)
        {
            unlockedMonsters.Clear();
            
            foreach (var monsterData in availableMonsters)
            {
                if (monsterData.unlockFloor <= currentFloor)
                {
                    unlockedMonsters.Add(monsterData);
                }
            }

            Debug.Log($"Unlocked {unlockedMonsters.Count} monster types for floor {currentFloor}");
        }

        public bool CanPlaceMonster(int floorIndex, Vector2 position, MonsterType monsterType)
        {
            // 階層の存在確認
            if (!FloorSystem.Instance.GetFloor(floorIndex))
            {
                Debug.LogWarning($"Floor {floorIndex} does not exist");
                return false;
            }

            // モンスター数制限チェック
            if (GetMonsterCountOnFloor(floorIndex) >= maxMonstersPerFloor)
            {
                Debug.LogWarning($"Floor {floorIndex} has reached maximum monster capacity ({maxMonstersPerFloor})");
                return false;
            }

            // 位置の有効性チェック
            if (!FloorSystem.Instance.CanPlaceMonster(floorIndex, position))
            {
                Debug.LogWarning($"Cannot place monster at position {position} on floor {floorIndex}");
                return false;
            }

            // モンスタータイプの解放状況チェック
            MonsterData monsterData = GetMonsterData(monsterType);
            if (monsterData == null || !unlockedMonsters.Contains(monsterData))
            {
                Debug.LogWarning($"Monster type {monsterType} is not unlocked");
                return false;
            }

            return true;
        }

        public IMonster PlaceMonster(int floorIndex, MonsterType monsterType, Vector2 position, int level = 1)
        {
            if (!CanPlaceMonster(floorIndex, position, monsterType))
            {
                return null;
            }

            MonsterData monsterData = GetMonsterData(monsterType);
            if (monsterData == null || monsterData.prefab == null)
            {
                Debug.LogError($"Monster data or prefab not found for {monsterType}");
                return null;
            }

            // モンスターオブジェクトを生成
            GameObject monsterObj = Instantiate(monsterData.prefab, monsterContainer);
            monsterObj.transform.position = new Vector3(position.x, position.y, 0);
            monsterObj.name = $"{monsterType}_{floorIndex}_{GetMonsterCountOnFloor(floorIndex)}";

            // モンスターコンポーネントを取得・設定
            IMonster monster = monsterObj.GetComponent<IMonster>();
            if (monster == null)
            {
                Debug.LogError($"Monster prefab for {monsterType} does not have IMonster component");
                Destroy(monsterObj);
                return null;
            }

            // BaseMonsterコンポーネントにデータを設定
            BaseMonster baseMonster = monsterObj.GetComponent<BaseMonster>();
            if (baseMonster != null)
            {
                baseMonster.SetMonsterData(monsterData);
                baseMonster.Level = level;
            }

            // 階層システムに登録
            if (!FloorSystem.Instance.PlaceMonster(floorIndex, monsterObj, position))
            {
                Debug.LogError($"Failed to place monster in FloorSystem");
                Destroy(monsterObj);
                return null;
            }

            // 内部管理に追加
            if (!floorMonsters.ContainsKey(floorIndex))
            {
                floorMonsters[floorIndex] = new List<IMonster>();
            }
            floorMonsters[floorIndex].Add(monster);

            // イベント発火
            OnMonsterPlaced?.Invoke(monster, floorIndex);
            OnFloorMonsterCountChanged?.Invoke(floorIndex, GetMonsterCountOnFloor(floorIndex));

            Debug.Log($"Placed {monsterType} on floor {floorIndex} at {position}");
            return monster;
        }

        public bool RemoveMonster(IMonster monster, int floorIndex)
        {
            if (monster == null)
            {
                return false;
            }

            GameObject monsterObj = (monster as MonoBehaviour)?.gameObject;
            if (monsterObj == null)
            {
                return false;
            }

            // 階層システムから除去
            FloorSystem.Instance.RemoveMonster(floorIndex, monsterObj);

            // 内部管理から除去
            if (floorMonsters.ContainsKey(floorIndex))
            {
                floorMonsters[floorIndex].Remove(monster);
            }

            // オブジェクト破棄
            Destroy(monsterObj);

            // イベント発火
            OnMonsterRemoved?.Invoke(monster, floorIndex);
            OnFloorMonsterCountChanged?.Invoke(floorIndex, GetMonsterCountOnFloor(floorIndex));

            Debug.Log($"Removed monster from floor {floorIndex}");
            return true;
        }

        /// <summary>
        /// 退避スポットシステム用: モンスターを階層から除去（オブジェクトは破棄しない）
        /// </summary>
        public bool RemoveMonster(IMonster monster)
        {
            if (monster == null)
            {
                return false;
            }

            GameObject monsterObj = (monster as MonoBehaviour)?.gameObject;
            if (monsterObj == null)
            {
                return false;
            }

            // どの階層にいるかを検索
            int floorIndex = -1;
            foreach (var kvp in floorMonsters)
            {
                if (kvp.Value.Contains(monster))
                {
                    floorIndex = kvp.Key;
                    break;
                }
            }

            if (floorIndex == -1)
            {
                Debug.LogWarning("Monster not found in any floor");
                return false;
            }

            // 階層システムから除去
            FloorSystem.Instance.RemoveMonster(floorIndex, monsterObj);

            // 内部管理から除去
            floorMonsters[floorIndex].Remove(monster);

            // イベント発火（オブジェクトは破棄しない）
            OnMonsterRemoved?.Invoke(monster, floorIndex);
            OnFloorMonsterCountChanged?.Invoke(floorIndex, GetMonsterCountOnFloor(floorIndex));

            Debug.Log($"Removed monster from floor {floorIndex} (for shelter)");
            return true;
        }

        /// <summary>
        /// 退避スポットシステム用: モンスターを階層に配置（既存オブジェクトを使用）
        /// </summary>
        public bool PlaceMonster(IMonster monster, int floorIndex, Vector2 position)
        {
            if (monster == null)
            {
                return false;
            }

            GameObject monsterObj = (monster as MonoBehaviour)?.gameObject;
            if (monsterObj == null)
            {
                return false;
            }

            // 配置可能かチェック
            if (!CanPlaceMonster(floorIndex, position, monster.Type))
            {
                return false;
            }

            // 位置を設定
            monsterObj.transform.position = new Vector3(position.x, position.y, 0);
            monster.Position = position;

            // 階層システムに登録
            if (!FloorSystem.Instance.PlaceMonster(floorIndex, monsterObj, position))
            {
                Debug.LogError($"Failed to place monster in FloorSystem");
                return false;
            }

            // 内部管理に追加
            if (!floorMonsters.ContainsKey(floorIndex))
            {
                floorMonsters[floorIndex] = new List<IMonster>();
            }
            floorMonsters[floorIndex].Add(monster);

            // イベント発火
            OnMonsterPlaced?.Invoke(monster, floorIndex);
            OnFloorMonsterCountChanged?.Invoke(floorIndex, GetMonsterCountOnFloor(floorIndex));

            Debug.Log($"Placed monster {monster.Type} on floor {floorIndex} at {position} (from shelter)");
            return true;
        }

        public bool MoveMonster(IMonster monster, int fromFloor, int toFloor, Vector2 newPosition)
        {
            if (monster == null)
            {
                return false;
            }

            // 移動先に配置可能かチェック
            if (!CanPlaceMonster(toFloor, newPosition, monster.Type))
            {
                return false;
            }

            // 移動元から除去（オブジェクトは破棄しない）
            GameObject monsterObj = (monster as MonoBehaviour)?.gameObject;
            if (monsterObj == null)
            {
                return false;
            }

            FloorSystem.Instance.RemoveMonster(fromFloor, monsterObj);
            if (floorMonsters.ContainsKey(fromFloor))
            {
                floorMonsters[fromFloor].Remove(monster);
            }

            // 移動先に配置
            monster.Position = newPosition;
            if (!FloorSystem.Instance.PlaceMonster(toFloor, monsterObj, newPosition))
            {
                // 失敗した場合は元に戻す
                FloorSystem.Instance.PlaceMonster(fromFloor, monsterObj, monster.Position);
                if (!floorMonsters.ContainsKey(fromFloor))
                {
                    floorMonsters[fromFloor] = new List<IMonster>();
                }
                floorMonsters[fromFloor].Add(monster);
                return false;
            }

            // 移動先の管理に追加
            if (!floorMonsters.ContainsKey(toFloor))
            {
                floorMonsters[toFloor] = new List<IMonster>();
            }
            floorMonsters[toFloor].Add(monster);

            // イベント発火
            OnMonsterRemoved?.Invoke(monster, fromFloor);
            OnMonsterPlaced?.Invoke(monster, toFloor);
            OnFloorMonsterCountChanged?.Invoke(fromFloor, GetMonsterCountOnFloor(fromFloor));
            OnFloorMonsterCountChanged?.Invoke(toFloor, GetMonsterCountOnFloor(toFloor));

            Debug.Log($"Moved monster from floor {fromFloor} to floor {toFloor}");
            return true;
        }

        public int GetMonsterCountOnFloor(int floorIndex)
        {
            if (!floorMonsters.ContainsKey(floorIndex))
            {
                return 0;
            }

            // null参照を除去してカウント
            floorMonsters[floorIndex].RemoveAll(m => m == null || (m as MonoBehaviour) == null);
            return floorMonsters[floorIndex].Count;
        }

        public List<IMonster> GetMonstersOnFloor(int floorIndex)
        {
            if (!floorMonsters.ContainsKey(floorIndex))
            {
                return new List<IMonster>();
            }

            // null参照を除去
            floorMonsters[floorIndex].RemoveAll(m => m == null || (m as MonoBehaviour) == null);
            return new List<IMonster>(floorMonsters[floorIndex]);
        }

        public bool IsFloorFull(int floorIndex)
        {
            return GetMonsterCountOnFloor(floorIndex) >= maxMonstersPerFloor;
        }

        public int GetRemainingCapacity(int floorIndex)
        {
            return maxMonstersPerFloor - GetMonsterCountOnFloor(floorIndex);
        }

        public MonsterData GetMonsterData(MonsterType monsterType)
        {
            return availableMonsters.FirstOrDefault(m => m.type == monsterType);
        }

        public List<MonsterData> GetUnlockedMonsters()
        {
            return new List<MonsterData>(unlockedMonsters);
        }

        public bool IsMonsterUnlocked(MonsterType monsterType)
        {
            MonsterData data = GetMonsterData(monsterType);
            return data != null && unlockedMonsters.Contains(data);
        }

        public void SetupInitialMonsters(int floorIndex)
        {
            // 要件3.2に従って初期モンスター配置
            var initialSetup = new Dictionary<MonsterType, int>
            {
                { MonsterType.Slime, 5 },
                { MonsterType.LesserSkeleton, 3 },
                { MonsterType.LesserGhost, 3 },
                { MonsterType.LesserGolem, 1 },
                { MonsterType.Goblin, 1 },
                { MonsterType.LesserWolf, 1 }
            };

            Vector2 basePosition = new Vector2(-3f, -3f);
            int placedCount = 0;

            foreach (var setup in initialSetup)
            {
                for (int i = 0; i < setup.Value; i++)
                {
                    Vector2 position = basePosition + new Vector2(
                        (placedCount % 5) * 1.5f,
                        (placedCount / 5) * 1.5f
                    );

                    if (CanPlaceMonster(floorIndex, position, setup.Key))
                    {
                        PlaceMonster(floorIndex, setup.Key, position, 1);
                        placedCount++;
                    }
                }
            }

            Debug.Log($"Placed {placedCount} initial monsters on floor {floorIndex}");
        }

        public void ClearFloor(int floorIndex)
        {
            if (!floorMonsters.ContainsKey(floorIndex))
            {
                return;
            }

            var monsters = new List<IMonster>(floorMonsters[floorIndex]);
            foreach (var monster in monsters)
            {
                RemoveMonster(monster, floorIndex);
            }

            Debug.Log($"Cleared all monsters from floor {floorIndex}");
        }

        /// <summary>
        /// 全ての配置されたモンスターを取得（セーブ用）
        /// </summary>
        public List<IMonster> GetAllPlacedMonsters()
        {
            List<IMonster> allMonsters = new List<IMonster>();
            
            foreach (var kvp in floorMonsters)
            {
                // null参照を除去
                kvp.Value.RemoveAll(m => m == null || (m as MonoBehaviour) == null);
                allMonsters.AddRange(kvp.Value);
            }
            
            return allMonsters;
        }

        /// <summary>
        /// モンスターの階層インデックスを取得
        /// </summary>
        public int GetMonsterFloorIndex(IMonster monster)
        {
            foreach (var kvp in floorMonsters)
            {
                if (kvp.Value.Contains(monster))
                {
                    return kvp.Key;
                }
            }
            return -1;
        }

        /// <summary>
        /// セーブデータからモンスターを復元
        /// </summary>
        public void RestoreMonster(MonsterSaveData monsterData)
        {
            if (monsterData == null || monsterData.isInShelter) return;

            MonsterData data = GetMonsterData(monsterData.type);
            if (data == null || data.prefab == null)
            {
                Debug.LogError($"Cannot restore monster: data not found for {monsterData.type}");
                return;
            }

            // モンスターオブジェクトを生成
            GameObject monsterObj = Instantiate(data.prefab, monsterContainer);
            monsterObj.transform.position = new Vector3(monsterData.position.x, monsterData.position.y, 0);
            monsterObj.name = $"{monsterData.type}_{monsterData.floorIndex}_restored";

            // モンスターコンポーネントを取得・設定
            IMonster monster = monsterObj.GetComponent<IMonster>();
            if (monster == null)
            {
                Debug.LogError($"Restored monster prefab for {monsterData.type} does not have IMonster component");
                Destroy(monsterObj);
                return;
            }

            // BaseMonsterコンポーネントにデータを設定
            BaseMonster baseMonster = monsterObj.GetComponent<BaseMonster>();
            if (baseMonster != null)
            {
                baseMonster.SetMonsterData(data);
                baseMonster.Level = monsterData.level;
                baseMonster.SetHealth(monsterData.currentHealth);
                baseMonster.SetMana(monsterData.currentMana);
            }

            // 階層システムに登録
            if (FloorSystem.Instance.PlaceMonster(monsterData.floorIndex, monsterObj, monsterData.position))
            {
                // 内部管理に追加
                if (!floorMonsters.ContainsKey(monsterData.floorIndex))
                {
                    floorMonsters[monsterData.floorIndex] = new List<IMonster>();
                }
                floorMonsters[monsterData.floorIndex].Add(monster);

                Debug.Log($"Restored {monsterData.type} on floor {monsterData.floorIndex}");
            }
            else
            {
                Debug.LogError($"Failed to place restored monster in FloorSystem");
                Destroy(monsterObj);
            }
        }

        // デバッグ用メソッド
        public void DebugPrintFloorInfo()
        {
            Debug.Log("=== Monster Placement Manager Info ===");
            foreach (var kvp in floorMonsters)
            {
                int floorIndex = kvp.Key;
                int count = GetMonsterCountOnFloor(floorIndex);
                Debug.Log($"Floor {floorIndex}: {count}/{maxMonstersPerFloor} monsters");
                
                foreach (var monster in kvp.Value)
                {
                    if (monster != null)
                    {
                        Debug.Log($"  - {monster.Type} (Level {monster.Level}, HP: {monster.Health}/{monster.MaxHealth})");
                    }
                }
            }
        }
    }
}