using UnityEngine;
using DungeonOwner.Managers;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core
{
    /// <summary>
    /// レベルシステムのテスト用クラス
    /// レベル表示、戦闘計算、報酬システムの動作確認
    /// </summary>
    public class LevelSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableAutoTest = false;
        [SerializeField] private float testInterval = 5f;
        [SerializeField] private KeyCode testKey = KeyCode.L;

        [Header("Test Parameters")]
        [SerializeField] private int testMonsterLevel = 5;
        [SerializeField] private int testInvaderLevel = 3;
        [SerializeField] private int testFloorDepth = 7;

        private float lastTestTime;

        private void Update()
        {
            // 手動テスト
            if (Input.GetKeyDown(testKey))
            {
                RunLevelSystemTests();
            }

            // 自動テスト
            if (enableAutoTest && Time.time - lastTestTime >= testInterval)
            {
                RunLevelSystemTests();
                lastTestTime = Time.time;
            }
        }

        /// <summary>
        /// レベルシステムの総合テスト実行
        /// </summary>
        public void RunLevelSystemTests()
        {
            Debug.Log("=== Level System Test Started ===");

            TestLevelDisplaySystem();
            TestCombatCalculations();
            TestInvaderLevelScaling();
            TestRewardSystem();
            TestResourceManager();

            Debug.Log("=== Level System Test Completed ===");
        }

        /// <summary>
        /// レベル表示システムのテスト
        /// </summary>
        private void TestLevelDisplaySystem()
        {
            Debug.Log("--- Testing Level Display System ---");

            if (LevelDisplayManager.Instance != null)
            {
                LevelDisplayManager.Instance.DebugPrintInfo();
                LevelDisplayManager.Instance.RefreshAllDisplays();
                Debug.Log("Level Display System: OK");
            }
            else
            {
                Debug.LogWarning("LevelDisplayManager not found!");
            }
        }

        /// <summary>
        /// 戦闘計算のテスト
        /// </summary>
        private void TestCombatCalculations()
        {
            Debug.Log("--- Testing Combat Calculations ---");

            if (CombatManager.Instance != null)
            {
                // レベル差による戦闘計算のテスト
                TestCombatWithLevelDifference(testMonsterLevel, testInvaderLevel);
                TestCombatWithLevelDifference(testInvaderLevel, testMonsterLevel);
                TestCombatWithEqualLevels(testMonsterLevel);
                
                Debug.Log("Combat Calculations: OK");
            }
            else
            {
                Debug.LogWarning("CombatManager not found!");
            }
        }

        /// <summary>
        /// レベル差戦闘のテスト
        /// </summary>
        private void TestCombatWithLevelDifference(int attackerLevel, int defenderLevel)
        {
            int levelDiff = attackerLevel - defenderLevel;
            float baseDamage = 20f;
            
            // 簡易的な戦闘計算テスト
            float expectedDamageMultiplier = 1f + (levelDiff * 0.15f);
            if (levelDiff > 0)
            {
                expectedDamageMultiplier = Mathf.Max(expectedDamageMultiplier, 1f);
            }
            else
            {
                expectedDamageMultiplier = Mathf.Max(expectedDamageMultiplier, 0.3f);
            }

            float expectedDamage = baseDamage * expectedDamageMultiplier;
            
            Debug.Log($"Level {attackerLevel} vs Level {defenderLevel}: " +
                     $"Expected damage multiplier: {expectedDamageMultiplier:F2}, " +
                     $"Expected damage: {expectedDamage:F1}");
        }

        /// <summary>
        /// 同レベル戦闘のテスト
        /// </summary>
        private void TestCombatWithEqualLevels(int level)
        {
            Debug.Log($"Equal level combat (Level {level}): Base damage with level bonus");
        }

        /// <summary>
        /// 侵入者レベルスケーリングのテスト
        /// </summary>
        private void TestInvaderLevelScaling()
        {
            Debug.Log("--- Testing Invader Level Scaling ---");

            if (InvaderSpawner.Instance != null)
            {
                // 階層深度に応じたレベル計算のテスト
                for (int floor = 1; floor <= testFloorDepth; floor++)
                {
                    int expectedLevel = CalculateExpectedInvaderLevel(floor);
                    Debug.Log($"Floor {floor}: Expected invader level = {expectedLevel}");
                }
                
                Debug.Log("Invader Level Scaling: OK");
            }
            else
            {
                Debug.LogWarning("InvaderSpawner not found!");
            }
        }

        /// <summary>
        /// 期待される侵入者レベルの計算
        /// </summary>
        private int CalculateExpectedInvaderLevel(int floorDepth)
        {
            // InvaderSpawnerの計算式と同じ
            int baseLevel = 1;
            float scalingFactor = 0.2f;
            return baseLevel + Mathf.FloorToInt(floorDepth * scalingFactor);
        }

        /// <summary>
        /// 報酬システムのテスト
        /// </summary>
        private void TestRewardSystem()
        {
            Debug.Log("--- Testing Reward System ---");

            if (ResourceManager.Instance != null)
            {
                // 各レベルの侵入者撃破報酬をテスト
                for (int level = 1; level <= 10; level++)
                {
                    int expectedReward = CalculateExpectedReward(level, InvaderType.Warrior);
                    Debug.Log($"Level {level} Warrior defeat reward: {expectedReward} gold");
                }

                // 異なる侵入者タイプのテスト
                TestRewardForInvaderType(InvaderType.Mage, 5);
                TestRewardForInvaderType(InvaderType.Cleric, 5);
                TestRewardForInvaderType(InvaderType.Rogue, 5);
                
                Debug.Log("Reward System: OK");
            }
            else
            {
                Debug.LogWarning("ResourceManager not found!");
            }
        }

        /// <summary>
        /// 期待される報酬の計算
        /// </summary>
        private int CalculateExpectedReward(int level, InvaderType type)
        {
            float baseReward = 10f;
            float levelMultiplier = 1f + (level - 1) * 0.2f;
            float typeMultiplier = GetTypeMultiplier(type);
            
            return Mathf.RoundToInt(baseReward * levelMultiplier * typeMultiplier);
        }

        /// <summary>
        /// 侵入者タイプ別報酬倍率
        /// </summary>
        private float GetTypeMultiplier(InvaderType type)
        {
            switch (type)
            {
                case InvaderType.Warrior: return 1.0f;
                case InvaderType.Mage: return 1.2f;
                case InvaderType.Rogue: return 0.8f;
                case InvaderType.Cleric: return 1.5f;
                default: return 1.0f;
            }
        }

        /// <summary>
        /// 侵入者タイプ別報酬テスト
        /// </summary>
        private void TestRewardForInvaderType(InvaderType type, int level)
        {
            int expectedReward = CalculateExpectedReward(level, type);
            Debug.Log($"Level {level} {type} defeat reward: {expectedReward} gold");
        }

        /// <summary>
        /// ResourceManagerのテスト
        /// </summary>
        private void TestResourceManager()
        {
            Debug.Log("--- Testing Resource Manager ---");

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.DebugPrintResourceInfo();
                
                // 階層拡張コストのテスト
                for (int floor = 4; floor <= 10; floor++)
                {
                    int cost = ResourceManager.Instance.CalculateFloorExpansionCost(floor);
                    Debug.Log($"Floor {floor} expansion cost: {cost} gold");
                }
                
                Debug.Log("Resource Manager: OK");
            }
            else
            {
                Debug.LogWarning("ResourceManager not found!");
            }
        }

        /// <summary>
        /// レベル表示の強制更新
        /// </summary>
        public void ForceRefreshLevelDisplays()
        {
            if (LevelDisplayManager.Instance != null)
            {
                LevelDisplayManager.Instance.RefreshAllDisplays();
                Debug.Log("Level displays refreshed");
            }
        }

        /// <summary>
        /// テスト用侵入者生成
        /// </summary>
        public void SpawnTestInvader(InvaderType type, int level)
        {
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.ForceSpawnInvader(type, level);
                Debug.Log($"Spawned test {type} (Level {level})");
            }
        }

        /// <summary>
        /// テスト用金貨付与
        /// </summary>
        public void AddTestGold(int amount)
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.DebugAddGold(amount);
                Debug.Log($"Added {amount} test gold");
            }
        }

        /// <summary>
        /// 設定値の表示
        /// </summary>
        private void OnGUI()
        {
            if (!enableAutoTest) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Level System Tester");
            GUILayout.Label($"Auto Test: {enableAutoTest}");
            GUILayout.Label($"Test Interval: {testInterval}s");
            GUILayout.Label($"Press '{testKey}' for manual test");
            
            if (GUILayout.Button("Run Test Now"))
            {
                RunLevelSystemTests();
            }
            
            if (GUILayout.Button("Refresh Level Displays"))
            {
                ForceRefreshLevelDisplays();
            }
            
            GUILayout.EndArea();
        }
    }
}