using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Managers;
using DungeonOwner.Data;
using System.Collections.Generic;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 階層拡張システムのテスタークラス
    /// 要件10.4, 10.5, 12.4の動作確認用
    /// </summary>
    public class FloorExpansionSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Test Controls")]
        [SerializeField] private KeyCode testExpansionKey = KeyCode.E;
        [SerializeField] private KeyCode addGoldKey = KeyCode.G;
        [SerializeField] private KeyCode printInfoKey = KeyCode.I;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunTestsAfterDelay());
            }
        }

        private void Update()
        {
            HandleTestInputs();
        }

        private void HandleTestInputs()
        {
            if (Input.GetKeyDown(testExpansionKey))
            {
                TestFloorExpansion();
            }

            if (Input.GetKeyDown(addGoldKey))
            {
                AddTestGold();
            }

            if (Input.GetKeyDown(printInfoKey))
            {
                PrintSystemInfo();
            }
        }

        private System.Collections.IEnumerator RunTestsAfterDelay()
        {
            // システムの初期化を待つ
            yield return new WaitForSeconds(1f);

            LogTest("=== Floor Expansion System Tests ===");

            // テスト1: 基本機能テスト
            TestBasicFunctionality();

            yield return new WaitForSeconds(0.5f);

            // テスト2: コスト計算テスト
            TestCostCalculation();

            yield return new WaitForSeconds(0.5f);

            // テスト3: モンスター解放テスト
            TestMonsterUnlocking();

            yield return new WaitForSeconds(0.5f);

            // テスト4: 拡張実行テスト
            TestExpansionExecution();

            LogTest("=== All Tests Completed ===");
        }

        /// <summary>
        /// 基本機能テスト
        /// </summary>
        private void TestBasicFunctionality()
        {
            LogTest("--- Test 1: Basic Functionality ---");

            if (FloorExpansionSystem.Instance == null)
            {
                LogError("FloorExpansionSystem.Instance is null!");
                return;
            }

            if (FloorSystem.Instance == null)
            {
                LogError("FloorSystem.Instance is null!");
                return;
            }

            // 拡張可能性チェック
            bool canExpand = FloorExpansionSystem.Instance.CanExpandFloor();
            LogTest($"Can expand floor: {canExpand}");

            // 現在の階層数
            int currentFloors = FloorSystem.Instance.CurrentFloorCount;
            LogTest($"Current floor count: {currentFloors}");

            // 利用可能モンスター
            var availableMonsters = FloorExpansionSystem.Instance.GetAvailableMonsters();
            LogTest($"Available monsters: {availableMonsters.Count}");

            LogTest("Basic functionality test completed");
        }

        /// <summary>
        /// コスト計算テスト
        /// </summary>
        private void TestCostCalculation()
        {
            LogTest("--- Test 2: Cost Calculation ---");

            if (FloorExpansionSystem.Instance == null) return;

            // 各階層のコストを計算
            for (int floor = 1; floor <= 10; floor++)
            {
                int cost = FloorExpansionSystem.Instance.CalculateExpansionCost(floor);
                LogTest($"Floor {floor} expansion cost: {cost}");
            }

            LogTest("Cost calculation test completed");
        }

        /// <summary>
        /// モンスター解放テスト
        /// </summary>
        private void TestMonsterUnlocking()
        {
            LogTest("--- Test 3: Monster Unlocking ---");

            if (FloorExpansionSystem.Instance == null) return;

            // 各階層での解放モンスターをチェック
            for (int floor = 1; floor <= 30; floor += 5)
            {
                var unlockedMonsters = FloorExpansionSystem.Instance.GetUnlockedMonstersAtFloor(floor);
                if (unlockedMonsters.Count > 0)
                {
                    string monsterNames = string.Join(", ", unlockedMonsters);
                    LogTest($"Floor {floor} unlocks: {monsterNames}");
                }
            }

            // 次の解放予定モンスター
            var nextUnlockable = FloorExpansionSystem.Instance.GetNextUnlockableMonsters();
            if (nextUnlockable.Count > 0)
            {
                string nextMonsters = string.Join(", ", nextUnlockable);
                LogTest($"Next unlockable monsters: {nextMonsters}");
            }

            LogTest("Monster unlocking test completed");
        }

        /// <summary>
        /// 拡張実行テスト
        /// </summary>
        private void TestExpansionExecution()
        {
            LogTest("--- Test 4: Expansion Execution ---");

            if (FloorExpansionSystem.Instance == null || ResourceManager.Instance == null) return;

            // 現在の状態を記録
            int initialFloors = FloorSystem.Instance.CurrentFloorCount;
            int initialGold = ResourceManager.Instance.Gold;

            LogTest($"Initial state - Floors: {initialFloors}, Gold: {initialGold}");

            // 十分な金貨を追加
            ResourceManager.Instance.AddGold(10000);
            LogTest($"Added test gold. Current gold: {ResourceManager.Instance.Gold}");

            // 拡張を試行
            bool expansionResult = FloorExpansionSystem.Instance.TryExpandFloor();
            LogTest($"Expansion result: {expansionResult}");

            if (expansionResult)
            {
                int newFloors = FloorSystem.Instance.CurrentFloorCount;
                int newGold = ResourceManager.Instance.Gold;
                LogTest($"After expansion - Floors: {newFloors}, Gold: {newGold}");
                LogTest($"Floors increased: {newFloors - initialFloors}");
                LogTest($"Gold spent: {initialGold + 10000 - newGold}");
            }

            LogTest("Expansion execution test completed");
        }

        /// <summary>
        /// 手動階層拡張テスト
        /// </summary>
        private void TestFloorExpansion()
        {
            LogTest("--- Manual Floor Expansion Test ---");

            if (FloorExpansionSystem.Instance == null)
            {
                LogError("FloorExpansionSystem not available");
                return;
            }

            bool result = FloorExpansionSystem.Instance.TryExpandFloor();
            LogTest($"Manual expansion result: {result}");

            if (!result)
            {
                if (ResourceManager.Instance != null)
                {
                    int currentGold = ResourceManager.Instance.Gold;
                    int requiredCost = FloorExpansionSystem.Instance.CalculateExpansionCost(
                        FloorSystem.Instance.CurrentFloorCount + 1);
                    LogTest($"Current gold: {currentGold}, Required: {requiredCost}");
                }
            }
        }

        /// <summary>
        /// テスト用金貨追加
        /// </summary>
        private void AddTestGold()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.AddGold(1000);
                LogTest($"Added 1000 gold. Current total: {ResourceManager.Instance.Gold}");
            }
        }

        /// <summary>
        /// システム情報出力
        /// </summary>
        private void PrintSystemInfo()
        {
            LogTest("--- System Information ---");

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.DebugPrintFloorInfo();
            }

            if (FloorExpansionSystem.Instance != null)
            {
                FloorExpansionSystem.Instance.DebugPrintExpansionInfo();
            }

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.DebugPrintResourceInfo();
            }
        }

        /// <summary>
        /// イベントハンドラー登録
        /// </summary>
        private void OnEnable()
        {
            if (FloorExpansionSystem.Instance != null)
            {
                FloorExpansionSystem.Instance.OnFloorExpanded += OnFloorExpanded;
                FloorExpansionSystem.Instance.OnNewMonstersUnlocked += OnNewMonstersUnlocked;
            }
        }

        /// <summary>
        /// イベントハンドラー解除
        /// </summary>
        private void OnDisable()
        {
            if (FloorExpansionSystem.Instance != null)
            {
                FloorExpansionSystem.Instance.OnFloorExpanded -= OnFloorExpanded;
                FloorExpansionSystem.Instance.OnNewMonstersUnlocked -= OnNewMonstersUnlocked;
            }
        }

        /// <summary>
        /// 階層拡張イベントハンドラー
        /// </summary>
        private void OnFloorExpanded(int newFloor)
        {
            LogTest($"[EVENT] Floor expanded to: {newFloor}");
        }

        /// <summary>
        /// モンスター解放イベントハンドラー
        /// </summary>
        private void OnNewMonstersUnlocked(List<MonsterType> monsters)
        {
            string monsterNames = string.Join(", ", monsters);
            LogTest($"[EVENT] New monsters unlocked: {monsterNames}");
        }

        /// <summary>
        /// テストログ出力
        /// </summary>
        private void LogTest(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[FloorExpansionTest] {message}");
            }
        }

        /// <summary>
        /// エラーログ出力
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[FloorExpansionTest] {message}");
        }

        /// <summary>
        /// GUI表示（デバッグ用）
        /// </summary>
        private void OnGUI()
        {
            if (!enableDebugLogs) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Floor Expansion System Tester");

            if (FloorSystem.Instance != null)
            {
                GUILayout.Label($"Current Floors: {FloorSystem.Instance.CurrentFloorCount}");
            }

            if (ResourceManager.Instance != null)
            {
                GUILayout.Label($"Gold: {ResourceManager.Instance.Gold}");
            }

            if (FloorExpansionSystem.Instance != null)
            {
                bool canExpand = FloorExpansionSystem.Instance.CanExpandFloor();
                GUILayout.Label($"Can Expand: {canExpand}");

                if (FloorSystem.Instance != null)
                {
                    int nextFloor = FloorSystem.Instance.CurrentFloorCount + 1;
                    int cost = FloorExpansionSystem.Instance.CalculateExpansionCost(nextFloor);
                    GUILayout.Label($"Next Cost: {cost}");
                }
            }

            GUILayout.Label($"Controls:");
            GUILayout.Label($"E - Test Expansion");
            GUILayout.Label($"G - Add 1000 Gold");
            GUILayout.Label($"I - Print Info");

            GUILayout.EndArea();
        }
    }
}