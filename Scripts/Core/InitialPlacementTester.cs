using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 初期配置システムのテスト用コンポーネント
    /// タスク5の動作確認用
    /// </summary>
    public class InitialPlacementTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestOnStart = false;
        [SerializeField] private PlayerCharacterType testPlayerCharacter = PlayerCharacterType.Warrior;
        
        [Header("Test Results")]
        [SerializeField] private bool testPassed = false;
        [SerializeField] private string testResults = "";

        private void Start()
        {
            if (runTestOnStart)
            {
                Invoke(nameof(RunPlacementTest), 1f); // 1秒後に実行（初期化完了を待つ）
            }
        }

        [ContextMenu("Run Placement Test")]
        public void RunPlacementTest()
        {
            Debug.Log("=== Starting Initial Placement Test ===");
            
            testPassed = false;
            testResults = "";

            // 必要なシステムの確認
            if (!ValidateRequiredSystems())
            {
                testResults = "Required systems not available";
                Debug.LogError(testResults);
                return;
            }

            // DungeonInitializerの確認
            DungeonInitializer initializer = FindObjectOfType<DungeonInitializer>();
            if (initializer == null)
            {
                testResults = "DungeonInitializer not found";
                Debug.LogError(testResults);
                return;
            }

            // プレイヤーキャラクタータイプを設定
            initializer.SetPlayerCharacterType(testPlayerCharacter);

            // 初期化実行
            if (!initializer.IsInitialized)
            {
                initializer.InitializeDungeon();
            }

            // 結果検証
            ValidateResults(initializer);
        }

        private bool ValidateRequiredSystems()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found");
                return false;
            }

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

        private void ValidateResults(DungeonInitializer initializer)
        {
            string results = "";
            bool allTestsPassed = true;

            // 1. 初期化完了の確認
            if (!initializer.IsInitialized)
            {
                results += "❌ Initialization not completed\n";
                allTestsPassed = false;
            }
            else
            {
                results += "✅ Initialization completed\n";
            }

            // 2. モンスター配置完了の確認
            if (!initializer.IsMonsterPlacementComplete)
            {
                results += "❌ Monster placement not completed\n";
                allTestsPassed = false;
            }
            else
            {
                results += "✅ Monster placement completed\n";
            }

            // 3. 1階層の確認
            Floor firstFloor = FloorSystem.Instance.GetFloor(1);
            if (firstFloor == null)
            {
                results += "❌ First floor not found\n";
                allTestsPassed = false;
            }
            else
            {
                results += "✅ First floor exists\n";

                // 4. 配置されたオブジェクト数の確認
                int placedCount = firstFloor.placedMonsters?.Count ?? 0;
                if (placedCount == 15) // プレイヤーキャラクター1 + モンスター14
                {
                    results += $"✅ Correct number of entities placed: {placedCount}\n";
                }
                else
                {
                    results += $"❌ Incorrect number of entities placed: {placedCount} (expected: 15)\n";
                    allTestsPassed = false;
                }
            }

            // 5. InitialMonsterPlacerの確認
            InitialMonsterPlacer placer = FindObjectOfType<InitialMonsterPlacer>();
            if (placer != null && placer.IsPlacementComplete)
            {
                results += "✅ InitialMonsterPlacer completed successfully\n";
                
                var placedMonsters = placer.GetPlacedMonsters();
                var playerCharacter = placer.GetPlacedPlayerCharacter();
                
                results += $"   - Monsters placed: {placedMonsters.Count}\n";
                results += $"   - Player character: {(playerCharacter != null ? "Yes" : "No")}\n";
            }
            else
            {
                results += "❌ InitialMonsterPlacer not found or not completed\n";
                allTestsPassed = false;
            }

            // 6. ゲーム開始準備の確認
            if (initializer.IsReadyToStart())
            {
                results += "✅ Ready to start game\n";
            }
            else
            {
                results += "❌ Not ready to start game\n";
                allTestsPassed = false;
            }

            testPassed = allTestsPassed;
            testResults = results;

            Debug.Log("=== Test Results ===");
            Debug.Log(results);
            Debug.Log($"Overall Test Result: {(testPassed ? "PASSED" : "FAILED")}");
        }

        [ContextMenu("Clear Test Results")]
        public void ClearTestResults()
        {
            testPassed = false;
            testResults = "";
            
            // 配置されたモンスターをクリア
            DungeonInitializer initializer = FindObjectOfType<DungeonInitializer>();
            if (initializer != null)
            {
                initializer.ResetInitialization();
            }
            
            Debug.Log("Test results cleared and initialization reset");
        }

        [ContextMenu("Test Different Player Characters")]
        public void TestAllPlayerCharacters()
        {
            PlayerCharacterType[] types = { 
                PlayerCharacterType.Warrior, 
                PlayerCharacterType.Mage, 
                PlayerCharacterType.Rogue, 
                PlayerCharacterType.Cleric 
            };

            foreach (var type in types)
            {
                Debug.Log($"\n=== Testing Player Character: {type} ===");
                
                // リセット
                ClearTestResults();
                
                // テスト実行
                testPlayerCharacter = type;
                RunPlacementTest();
                
                if (!testPassed)
                {
                    Debug.LogError($"Test failed for {type}");
                    break;
                }
            }
        }

        // ゲッター
        public bool IsTestPassed()
        {
            return testPassed;
        }

        public string GetTestResults()
        {
            return testResults;
        }
    }
}