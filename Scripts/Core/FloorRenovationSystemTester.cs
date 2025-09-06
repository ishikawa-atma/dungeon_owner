using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Core;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 階層改装システムのテスト用クラス
    /// 改装機能の動作確認とデバッグ用機能を提供
    /// </summary>
    public class FloorRenovationSystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private int testFloorIndex = 2;
        [SerializeField] private bool enableDebugLogs = true;

        [Header("Test Scenarios")]
        [SerializeField] private bool testBasicRenovation = true;
        [SerializeField] private bool testPathValidation = true;
        [SerializeField] private bool testErrorHandling = true;
        [SerializeField] private bool testLayoutSaving = true;

        [Header("Manual Test Controls")]
        [SerializeField] private KeyCode startRenovationKey = KeyCode.R;
        [SerializeField] private KeyCode endRenovationKey = KeyCode.E;
        [SerializeField] private KeyCode placeWallKey = KeyCode.P;
        [SerializeField] private KeyCode removeWallKey = KeyCode.X;
        [SerializeField] private KeyCode debugInfoKey = KeyCode.D;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }

        private void Update()
        {
            HandleManualTestInput();
        }

        /// <summary>
        /// 手動テスト用の入力処理
        /// </summary>
        private void HandleManualTestInput()
        {
            if (Input.GetKeyDown(startRenovationKey))
            {
                TestStartRenovation();
            }

            if (Input.GetKeyDown(endRenovationKey))
            {
                TestEndRenovation();
            }

            if (Input.GetKeyDown(placeWallKey))
            {
                TestPlaceWall();
            }

            if (Input.GetKeyDown(removeWallKey))
            {
                TestRemoveWall();
            }

            if (Input.GetKeyDown(debugInfoKey))
            {
                PrintDebugInfo();
            }
        }

        /// <summary>
        /// 全テストを実行
        /// </summary>
        private System.Collections.IEnumerator RunAllTests()
        {
            LogTest("=== Starting Floor Renovation System Tests ===");

            yield return new WaitForSeconds(1f);

            if (testBasicRenovation)
            {
                yield return StartCoroutine(TestBasicRenovationFlow());
                yield return new WaitForSeconds(1f);
            }

            if (testPathValidation)
            {
                yield return StartCoroutine(TestPathValidationFlow());
                yield return new WaitForSeconds(1f);
            }

            if (testErrorHandling)
            {
                yield return StartCoroutine(TestErrorHandlingFlow());
                yield return new WaitForSeconds(1f);
            }

            if (testLayoutSaving)
            {
                yield return StartCoroutine(TestLayoutSavingFlow());
                yield return new WaitForSeconds(1f);
            }

            LogTest("=== Floor Renovation System Tests Completed ===");
        }

        /// <summary>
        /// 基本的な改装フローのテスト
        /// </summary>
        private System.Collections.IEnumerator TestBasicRenovationFlow()
        {
            LogTest("--- Testing Basic Renovation Flow ---");

            // 改装開始テスト
            bool canStart = FloorRenovationSystem.Instance.CanStartRenovation(testFloorIndex);
            LogTest($"Can start renovation on floor {testFloorIndex}: {canStart}");

            if (canStart)
            {
                bool started = FloorRenovationSystem.Instance.StartRenovation(testFloorIndex);
                LogTest($"Started renovation: {started}");

                yield return new WaitForSeconds(0.5f);

                // 壁配置テスト
                Vector2 testWallPos = new Vector2(2, 2);
                bool wallPlaced = FloorRenovationSystem.Instance.PlaceWall(testWallPos);
                LogTest($"Placed wall at {testWallPos}: {wallPlaced}");

                yield return new WaitForSeconds(0.5f);

                // 壁除去テスト
                bool wallRemoved = FloorRenovationSystem.Instance.RemoveWall(testWallPos);
                LogTest($"Removed wall at {testWallPos}: {wallRemoved}");

                yield return new WaitForSeconds(0.5f);

                // 改装終了テスト
                FloorRenovationSystem.Instance.EndRenovation(true);
                LogTest("Ended renovation with save");
            }

            LogTest("--- Basic Renovation Flow Test Completed ---");
        }

        /// <summary>
        /// 経路検証フローのテスト
        /// </summary>
        private System.Collections.IEnumerator TestPathValidationFlow()
        {
            LogTest("--- Testing Path Validation Flow ---");

            if (FloorRenovationSystem.Instance.CanStartRenovation(testFloorIndex))
            {
                FloorRenovationSystem.Instance.StartRenovation(testFloorIndex);

                yield return new WaitForSeconds(0.5f);

                // 階段位置に壁を配置しようとする（失敗するはず）
                Floor floor = FloorSystem.Instance.GetFloor(testFloorIndex);
                if (floor != null)
                {
                    bool blockedStair = FloorRenovationSystem.Instance.PlaceWall(floor.upStairPosition);
                    LogTest($"Tried to place wall on up stair: {blockedStair} (should be false)");

                    yield return new WaitForSeconds(0.5f);

                    // 経路を塞ぐ壁の配置テスト
                    Vector2 blockingPos = new Vector2(0, 0);
                    bool blockingWall = FloorRenovationSystem.Instance.PlaceWall(blockingPos);
                    LogTest($"Placed potentially blocking wall: {blockingWall}");
                }

                yield return new WaitForSeconds(0.5f);

                FloorRenovationSystem.Instance.EndRenovation(false);
                LogTest("Ended renovation without save");
            }

            LogTest("--- Path Validation Flow Test Completed ---");
        }

        /// <summary>
        /// エラーハンドリングフローのテスト
        /// </summary>
        private System.Collections.IEnumerator TestErrorHandlingFlow()
        {
            LogTest("--- Testing Error Handling Flow ---");

            // 改装モードでない状態で壁配置を試行
            bool wallWithoutMode = FloorRenovationSystem.Instance.PlaceWall(Vector2.zero);
            LogTest($"Tried to place wall without renovation mode: {wallWithoutMode} (should be false)");

            yield return new WaitForSeconds(0.5f);

            // 存在しない階層で改装開始を試行
            bool invalidFloor = FloorRenovationSystem.Instance.StartRenovation(999);
            LogTest($"Tried to start renovation on invalid floor: {invalidFloor} (should be false)");

            yield return new WaitForSeconds(0.5f);

            // 重複する改装モード開始を試行
            if (FloorRenovationSystem.Instance.CanStartRenovation(testFloorIndex))
            {
                FloorRenovationSystem.Instance.StartRenovation(testFloorIndex);
                bool duplicateStart = FloorRenovationSystem.Instance.StartRenovation(testFloorIndex);
                LogTest($"Tried to start renovation twice: {duplicateStart} (should be false)");

                FloorRenovationSystem.Instance.EndRenovation(false);
            }

            LogTest("--- Error Handling Flow Test Completed ---");
        }

        /// <summary>
        /// レイアウト保存フローのテスト
        /// </summary>
        private System.Collections.IEnumerator TestLayoutSavingFlow()
        {
            LogTest("--- Testing Layout Saving Flow ---");

            if (FloorRenovationSystem.Instance.CanStartRenovation(testFloorIndex))
            {
                FloorRenovationSystem.Instance.StartRenovation(testFloorIndex);

                yield return new WaitForSeconds(0.5f);

                // 複数の壁を配置
                List<Vector2> testWalls = new List<Vector2>
                {
                    new Vector2(1, 1),
                    new Vector2(2, 1),
                    new Vector2(3, 1)
                };

                foreach (Vector2 wallPos in testWalls)
                {
                    bool placed = FloorRenovationSystem.Instance.PlaceWall(wallPos);
                    LogTest($"Placed wall at {wallPos}: {placed}");
                    yield return new WaitForSeconds(0.2f);
                }

                // 保存して終了
                FloorRenovationSystem.Instance.EndRenovation(true);
                LogTest("Saved layout and ended renovation");

                yield return new WaitForSeconds(0.5f);

                // 保存されたレイアウトを確認
                Floor floor = FloorSystem.Instance.GetFloor(testFloorIndex);
                if (floor != null)
                {
                    LogTest($"Saved wall count: {floor.wallPositions.Count}");
                    foreach (Vector2 wallPos in floor.wallPositions)
                    {
                        LogTest($"Saved wall at: {wallPos}");
                    }
                }
            }

            LogTest("--- Layout Saving Flow Test Completed ---");
        }

        /// <summary>
        /// 改装開始テスト
        /// </summary>
        public void TestStartRenovation()
        {
            if (FloorSystem.Instance == null)
            {
                LogTest("FloorSystem not found");
                return;
            }

            int currentFloor = FloorSystem.Instance.CurrentViewFloor;
            bool result = FloorRenovationSystem.Instance.StartRenovation(currentFloor);
            LogTest($"Start renovation on floor {currentFloor}: {result}");
        }

        /// <summary>
        /// 改装終了テスト
        /// </summary>
        public void TestEndRenovation()
        {
            FloorRenovationSystem.Instance.EndRenovation(true);
            LogTest("End renovation with save");
        }

        /// <summary>
        /// 壁配置テスト
        /// </summary>
        public void TestPlaceWall()
        {
            Vector2 randomPos = new Vector2(
                Random.Range(-3, 4),
                Random.Range(-3, 4)
            );

            bool result = FloorRenovationSystem.Instance.PlaceWall(randomPos);
            LogTest($"Place wall at {randomPos}: {result}");
        }

        /// <summary>
        /// 壁除去テスト
        /// </summary>
        public void TestRemoveWall()
        {
            List<Vector2> currentWalls = FloorRenovationSystem.Instance.TemporaryWalls;
            if (currentWalls.Count > 0)
            {
                Vector2 wallToRemove = currentWalls[Random.Range(0, currentWalls.Count)];
                bool result = FloorRenovationSystem.Instance.RemoveWall(wallToRemove);
                LogTest($"Remove wall at {wallToRemove}: {result}");
            }
            else
            {
                LogTest("No walls to remove");
            }
        }

        /// <summary>
        /// デバッグ情報を出力
        /// </summary>
        public void PrintDebugInfo()
        {
            FloorRenovationSystem.Instance.DebugPrintRenovationInfo();
            
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.DebugPrintFloorInfo();
            }
        }

        /// <summary>
        /// テストログを出力
        /// </summary>
        private void LogTest(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[FloorRenovationTest] {message}");
            }
        }

        /// <summary>
        /// 改装システムの状態をリセット
        /// </summary>
        [ContextMenu("Reset Renovation System")]
        public void ResetRenovationSystem()
        {
            FloorRenovationSystem.Instance.ResetRenovation();
            LogTest("Renovation system reset");
        }

        /// <summary>
        /// 指定階層を空にする（テスト用）
        /// </summary>
        [ContextMenu("Clear Test Floor")]
        public void ClearTestFloor()
        {
            Floor floor = FloorSystem.Instance.GetFloor(testFloorIndex);
            if (floor != null)
            {
                // モンスターを全て除去
                for (int i = floor.placedMonsters.Count - 1; i >= 0; i--)
                {
                    if (floor.placedMonsters[i] != null)
                    {
                        DestroyImmediate(floor.placedMonsters[i]);
                    }
                }
                floor.placedMonsters.Clear();

                // 侵入者を全て除去
                for (int i = floor.activeInvaders.Count - 1; i >= 0; i--)
                {
                    if (floor.activeInvaders[i] != null)
                    {
                        DestroyImmediate(floor.activeInvaders[i]);
                    }
                }
                floor.activeInvaders.Clear();

                LogTest($"Cleared floor {testFloorIndex}");
            }
        }
    }
}