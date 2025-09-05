using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Managers;
using DungeonOwner.Core;

namespace DungeonOwner.Core
{
    public class MonsterPlacementTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private bool testInitialPlacement = true;
        [SerializeField] private bool testCapacityLimits = true;
        [SerializeField] private bool testMonsterRemoval = true;
        [SerializeField] private bool testMonsterMovement = true;

        [Header("Test Data")]
        [SerializeField] private MonsterType testMonsterType = MonsterType.Slime;
        [SerializeField] private int testFloor = 1;

        private void Start()
        {
            if (runTestOnStart)
            {
                // 少し遅延してテストを実行（他のシステムの初期化を待つ）
                Invoke(nameof(RunTests), 1f);
            }
        }

        private void Update()
        {
            // デバッグキー
            if (Input.GetKeyDown(KeyCode.T))
            {
                RunTests();
            }
            
            if (Input.GetKeyDown(KeyCode.I))
            {
                TestInitialMonsterPlacement();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                TestCapacityLimits();
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                TestMonsterRemoval();
            }
            
            if (Input.GetKeyDown(KeyCode.M))
            {
                TestMonsterMovement();
            }
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                PrintSystemInfo();
            }
        }

        public void RunTests()
        {
            Debug.Log("=== Starting Monster Placement System Tests ===");

            if (testInitialPlacement)
            {
                TestInitialMonsterPlacement();
            }

            if (testCapacityLimits)
            {
                TestCapacityLimits();
            }

            if (testMonsterRemoval)
            {
                TestMonsterRemoval();
            }

            if (testMonsterMovement)
            {
                TestMonsterMovement();
            }

            Debug.Log("=== Monster Placement System Tests Completed ===");
        }

        private void TestInitialMonsterPlacement()
        {
            Debug.Log("--- Testing Initial Monster Placement ---");

            if (MonsterPlacementManager.Instance == null)
            {
                Debug.LogError("MonsterPlacementManager not found!");
                return;
            }

            // 初期モンスター配置をテスト
            MonsterPlacementManager.Instance.SetupInitialMonsters(testFloor);

            int monsterCount = MonsterPlacementManager.Instance.GetMonsterCountOnFloor(testFloor);
            Debug.Log($"Initial monsters placed: {monsterCount}/15");

            // 各モンスタータイプの配置確認
            var monsters = MonsterPlacementManager.Instance.GetMonstersOnFloor(testFloor);
            var typeCounts = new System.Collections.Generic.Dictionary<MonsterType, int>();

            foreach (var monster in monsters)
            {
                if (typeCounts.ContainsKey(monster.Type))
                {
                    typeCounts[monster.Type]++;
                }
                else
                {
                    typeCounts[monster.Type] = 1;
                }
            }

            foreach (var kvp in typeCounts)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value} placed");
            }
        }

        private void TestCapacityLimits()
        {
            Debug.Log("--- Testing Capacity Limits ---");

            if (MonsterPlacementManager.Instance == null)
            {
                Debug.LogError("MonsterPlacementManager not found!");
                return;
            }

            int initialCount = MonsterPlacementManager.Instance.GetMonsterCountOnFloor(testFloor);
            Debug.Log($"Initial monster count: {initialCount}");

            // 容量限界まで配置を試行
            int attempts = 0;
            int successful = 0;

            for (int i = 0; i < 20; i++) // 15体制限を超えて試行
            {
                Vector2 position = new Vector2(
                    Random.Range(-5f, 5f),
                    Random.Range(-5f, 5f)
                );

                attempts++;
                var monster = MonsterPlacementManager.Instance.PlaceMonster(testFloor, testMonsterType, position);
                
                if (monster != null)
                {
                    successful++;
                }

                int currentCount = MonsterPlacementManager.Instance.GetMonsterCountOnFloor(testFloor);
                if (currentCount >= 15)
                {
                    Debug.Log($"Reached capacity limit: {currentCount}/15");
                    break;
                }
            }

            Debug.Log($"Placement attempts: {attempts}, Successful: {successful}");
            
            int finalCount = MonsterPlacementManager.Instance.GetMonsterCountOnFloor(testFloor);
            bool capacityRespected = finalCount <= 15;
            Debug.Log($"Final count: {finalCount}/15, Capacity respected: {capacityRespected}");
        }

        private void TestMonsterRemoval()
        {
            Debug.Log("--- Testing Monster Removal ---");

            if (MonsterPlacementManager.Instance == null)
            {
                Debug.LogError("MonsterPlacementManager not found!");
                return;
            }

            var monsters = MonsterPlacementManager.Instance.GetMonstersOnFloor(testFloor);
            int initialCount = monsters.Count;
            Debug.Log($"Initial monster count: {initialCount}");

            if (monsters.Count > 0)
            {
                // 最初のモンスターを除去
                var monsterToRemove = monsters[0];
                bool removed = MonsterPlacementManager.Instance.RemoveMonster(monsterToRemove, testFloor);
                
                int newCount = MonsterPlacementManager.Instance.GetMonsterCountOnFloor(testFloor);
                Debug.Log($"Removal successful: {removed}, New count: {newCount}");
                
                if (removed && newCount == initialCount - 1)
                {
                    Debug.Log("Monster removal test PASSED");
                }
                else
                {
                    Debug.LogError("Monster removal test FAILED");
                }
            }
            else
            {
                Debug.Log("No monsters to remove");
            }
        }

        private void TestMonsterMovement()
        {
            Debug.Log("--- Testing Monster Movement ---");

            if (MonsterPlacementManager.Instance == null || FloorSystem.Instance == null)
            {
                Debug.LogError("Required managers not found!");
                return;
            }

            // 2階層が存在するかチェック
            if (FloorSystem.Instance.CurrentFloorCount < 2)
            {
                Debug.Log("Creating second floor for movement test");
                FloorSystem.Instance.ExpandFloor();
            }

            var monsters = MonsterPlacementManager.Instance.GetMonstersOnFloor(testFloor);
            if (monsters.Count > 0)
            {
                var monsterToMove = monsters[0];
                Vector2 originalPosition = monsterToMove.Position;
                int targetFloor = testFloor == 1 ? 2 : 1;
                Vector2 newPosition = new Vector2(0, 0);

                Debug.Log($"Moving monster from floor {testFloor} to floor {targetFloor}");
                
                bool moved = MonsterPlacementManager.Instance.MoveMonster(
                    monsterToMove, testFloor, targetFloor, newPosition);

                if (moved)
                {
                    Debug.Log("Monster movement test PASSED");
                    
                    // 元の位置に戻す
                    MonsterPlacementManager.Instance.MoveMonster(
                        monsterToMove, targetFloor, testFloor, originalPosition);
                }
                else
                {
                    Debug.LogError("Monster movement test FAILED");
                }
            }
            else
            {
                Debug.Log("No monsters to move");
            }
        }

        private void PrintSystemInfo()
        {
            Debug.Log("=== Monster Placement System Info ===");

            if (MonsterPlacementManager.Instance != null)
            {
                MonsterPlacementManager.Instance.DebugPrintFloorInfo();
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.DebugPrintFloorInfo();
            }

            // 解放されたモンスター情報
            if (MonsterPlacementManager.Instance != null)
            {
                var unlockedMonsters = MonsterPlacementManager.Instance.GetUnlockedMonsters();
                Debug.Log($"Unlocked monsters: {unlockedMonsters.Count}");
                foreach (var monster in unlockedMonsters)
                {
                    Debug.Log($"  - {monster.type} (Cost: {monster.goldCost}, Unlock Floor: {monster.unlockFloor})");
                }
            }
        }

        // 手動テスト用メソッド
        [ContextMenu("Place Test Monster")]
        public void PlaceTestMonster()
        {
            if (MonsterPlacementManager.Instance != null)
            {
                Vector2 randomPosition = new Vector2(
                    Random.Range(-3f, 3f),
                    Random.Range(-3f, 3f)
                );

                var monster = MonsterPlacementManager.Instance.PlaceMonster(
                    testFloor, testMonsterType, randomPosition);

                if (monster != null)
                {
                    Debug.Log($"Placed {testMonsterType} at {randomPosition}");
                }
                else
                {
                    Debug.Log($"Failed to place {testMonsterType} at {randomPosition}");
                }
            }
        }

        [ContextMenu("Clear Test Floor")]
        public void ClearTestFloor()
        {
            if (MonsterPlacementManager.Instance != null)
            {
                MonsterPlacementManager.Instance.ClearFloor(testFloor);
                Debug.Log($"Cleared floor {testFloor}");
            }
        }

        [ContextMenu("Setup Initial Monsters")]
        public void SetupInitialMonsters()
        {
            if (MonsterPlacementManager.Instance != null)
            {
                MonsterPlacementManager.Instance.SetupInitialMonsters(testFloor);
                Debug.Log($"Setup initial monsters on floor {testFloor}");
            }
        }
    }
}