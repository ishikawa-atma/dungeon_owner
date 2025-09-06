using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.UI;
using System.Collections;

namespace DungeonOwner.Core
{
    /// <summary>
    /// チュートリアルシステムのテスター
    /// 要件20.1-20.4の動作確認用
    /// </summary>
    public class TutorialSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool autoStartTest = false;
        [SerializeField] private float testDelay = 2f;
        [SerializeField] private bool verboseLogging = true;

        [Header("Test UI")]
        [SerializeField] private Button startTutorialButton;
        [SerializeField] private Button skipTutorialButton;
        [SerializeField] private Button resetTutorialButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI logText;

        [Header("Mock UI Elements")]
        [SerializeField] private Button mockMonsterPlacementButton;
        [SerializeField] private Button mockSpeedControlButton;
        [SerializeField] private GameObject mockResourceDisplay;

        // テスト状態
        private bool isTestRunning = false;
        private int testStepCount = 0;
        private string testLog = "";

        private void Start()
        {
            InitializeTest();
            
            if (autoStartTest)
            {
                StartCoroutine(AutoStartTestDelayed());
            }
        }

        /// <summary>
        /// テスト初期化
        /// </summary>
        private void InitializeTest()
        {
            SetupTestUI();
            SetupMockElements();
            SubscribeToEvents();
            
            LogTest("Tutorial System Tester initialized");
            UpdateStatus("Ready for testing");
        }

        /// <summary>
        /// テストUI設定
        /// </summary>
        private void SetupTestUI()
        {
            if (startTutorialButton != null)
            {
                startTutorialButton.onClick.AddListener(StartTutorialTest);
            }

            if (skipTutorialButton != null)
            {
                skipTutorialButton.onClick.AddListener(SkipTutorialTest);
            }

            if (resetTutorialButton != null)
            {
                resetTutorialButton.onClick.AddListener(ResetTutorialTest);
            }
        }

        /// <summary>
        /// モック要素設定
        /// </summary>
        private void SetupMockElements()
        {
            // UIManagerにモック要素を登録
            if (UIManager.Instance != null)
            {
                if (mockResourceDisplay != null)
                {
                    UIManager.Instance.RegisterUIElement("ResourceDisplay", mockResourceDisplay.GetComponent<RectTransform>());
                }

                if (mockMonsterPlacementButton != null)
                {
                    UIManager.Instance.RegisterUIElement("MonsterPlacementButton", mockMonsterPlacementButton.GetComponent<RectTransform>());
                }

                if (mockSpeedControlButton != null)
                {
                    UIManager.Instance.RegisterUIElement("SpeedControlButton", mockSpeedControlButton.GetComponent<RectTransform>());
                }
            }

            // モックボタンの動作設定
            if (mockMonsterPlacementButton != null)
            {
                mockMonsterPlacementButton.onClick.AddListener(OnMockMonsterPlacement);
            }

            if (mockSpeedControlButton != null)
            {
                mockSpeedControlButton.onClick.AddListener(OnMockSpeedControl);
            }
        }

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnTutorialStarted += OnTutorialStarted;
                TutorialManager.Instance.OnTutorialCompleted += OnTutorialCompleted;
                TutorialManager.Instance.OnTutorialSkipped += OnTutorialSkipped;
                TutorialManager.Instance.OnStepCompleted += OnStepCompleted;
            }
        }

        /// <summary>
        /// 自動テスト開始（遅延）
        /// </summary>
        private IEnumerator AutoStartTestDelayed()
        {
            yield return new WaitForSeconds(testDelay);
            StartTutorialTest();
        }

        /// <summary>
        /// チュートリアルテスト開始
        /// </summary>
        [ContextMenu("Start Tutorial Test")]
        public void StartTutorialTest()
        {
            if (isTestRunning)
            {
                LogTest("Test already running");
                return;
            }

            LogTest("=== Starting Tutorial Test ===");
            isTestRunning = true;
            testStepCount = 0;
            
            UpdateStatus("Testing tutorial start...");

            // チュートリアル完了フラグをリセット
            PlayerPrefs.DeleteKey("TutorialCompleted");
            
            // TutorialManagerが存在することを確認
            if (TutorialManager.Instance == null)
            {
                LogTest("ERROR: TutorialManager not found!");
                UpdateStatus("ERROR: TutorialManager missing");
                isTestRunning = false;
                return;
            }

            // チュートリアル開始
            TutorialManager.Instance.StartTutorial();
        }

        /// <summary>
        /// チュートリアルスキップテスト
        /// </summary>
        [ContextMenu("Skip Tutorial Test")]
        public void SkipTutorialTest()
        {
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                LogTest("Testing tutorial skip...");
                TutorialManager.Instance.SkipTutorial();
            }
            else
            {
                LogTest("No active tutorial to skip");
            }
        }

        /// <summary>
        /// チュートリアルリセットテスト
        /// </summary>
        [ContextMenu("Reset Tutorial Test")]
        public void ResetTutorialTest()
        {
            LogTest("=== Resetting Tutorial ===");
            
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.ResetTutorial();
            }
            
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
            
            isTestRunning = false;
            testStepCount = 0;
            
            UpdateStatus("Tutorial reset - ready for testing");
            LogTest("Tutorial reset completed");
        }

        /// <summary>
        /// チュートリアル開始イベント
        /// </summary>
        private void OnTutorialStarted()
        {
            LogTest("✓ Tutorial started successfully");
            UpdateStatus("Tutorial running...");
            
            // 要件20.1のテスト: 初回起動時のチュートリアルモード
            TestRequirement201();
        }

        /// <summary>
        /// チュートリアル完了イベント
        /// </summary>
        private void OnTutorialCompleted()
        {
            LogTest("✓ Tutorial completed successfully");
            UpdateStatus("Tutorial completed");
            
            isTestRunning = false;
            
            // 要件20.4のテスト: 通常ゲームモードに移行
            TestRequirement204();
            
            LogTest("=== Tutorial Test Completed ===");
        }

        /// <summary>
        /// チュートリアルスキップイベント
        /// </summary>
        private void OnTutorialSkipped()
        {
            LogTest("✓ Tutorial skipped successfully");
            UpdateStatus("Tutorial skipped");
            
            isTestRunning = false;
            
            // 要件20.5のテスト: スキップ機能
            TestRequirement205();
            
            LogTest("=== Tutorial Skip Test Completed ===");
        }

        /// <summary>
        /// ステップ完了イベント
        /// </summary>
        private void OnStepCompleted(int stepIndex)
        {
            testStepCount++;
            LogTest($"✓ Step {stepIndex + 1} completed");
            UpdateStatus($"Step {stepIndex + 1} completed");
            
            // 要件20.2のテスト: 段階的な操作説明
            TestRequirement202(stepIndex);
            
            // 要件20.3のテスト: 次のステップに進行
            TestRequirement203(stepIndex);
        }

        /// <summary>
        /// モックモンスター配置
        /// </summary>
        private void OnMockMonsterPlacement()
        {
            LogTest("Mock monster placement triggered");
            
            // MonsterPlacementManagerのモック動作
            if (MonsterPlacementManager.Instance != null)
            {
                // 実際の配置処理をシミュレート
                LogTest("Simulating monster placement...");
            }
        }

        /// <summary>
        /// モック速度制御
        /// </summary>
        private void OnMockSpeedControl()
        {
            LogTest("Mock speed control triggered");
            
            // GameManagerの速度変更をシミュレート
            if (GameManager.Instance != null)
            {
                float newSpeed = GameManager.Instance.GameSpeed == 1.0f ? 1.5f : 1.0f;
                GameManager.Instance.SetGameSpeed(newSpeed);
                LogTest($"Game speed changed to {newSpeed}x");
            }
        }

        /// <summary>
        /// 要件20.1のテスト: 初回起動時のチュートリアルモード
        /// </summary>
        private void TestRequirement201()
        {
            bool passed = true;
            string testName = "Requirement 20.1: Tutorial mode on first launch";
            
            // GameStateがTutorialになっているかチェック
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentState != GameState.Tutorial)
                {
                    LogTest($"❌ {testName} - Game state is not Tutorial");
                    passed = false;
                }
            }
            else
            {
                LogTest($"❌ {testName} - GameManager not found");
                passed = false;
            }
            
            // TutorialManagerがアクティブかチェック
            if (TutorialManager.Instance == null || !TutorialManager.Instance.IsTutorialActive())
            {
                LogTest($"❌ {testName} - Tutorial not active");
                passed = false;
            }
            
            if (passed)
            {
                LogTest($"✅ {testName} - PASSED");
            }
        }

        /// <summary>
        /// 要件20.2のテスト: 段階的な操作説明
        /// </summary>
        private void TestRequirement202(int stepIndex)
        {
            string testName = $"Requirement 20.2: Step-by-step instruction (Step {stepIndex + 1})";
            
            // ステップが順次進行しているかチェック
            if (stepIndex == testStepCount - 1)
            {
                LogTest($"✅ {testName} - PASSED");
            }
            else
            {
                LogTest($"❌ {testName} - Step order mismatch");
            }
        }

        /// <summary>
        /// 要件20.3のテスト: 次のステップに進行
        /// </summary>
        private void TestRequirement203(int stepIndex)
        {
            string testName = $"Requirement 20.3: Progress to next step (Step {stepIndex + 1})";
            
            // ステップが完了後に次に進むかチェック
            StartCoroutine(CheckStepProgression(stepIndex, testName));
        }

        /// <summary>
        /// ステップ進行チェック
        /// </summary>
        private IEnumerator CheckStepProgression(int completedStep, string testName)
        {
            yield return new WaitForSeconds(1f);
            
            // 次のステップが開始されているかチェック
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                LogTest($"✅ {testName} - PASSED");
            }
            else if (completedStep >= 6) // 最後のステップの場合
            {
                LogTest($"✅ {testName} - PASSED (Tutorial completed)");
            }
            else
            {
                LogTest($"❌ {testName} - Next step not started");
            }
        }

        /// <summary>
        /// 要件20.4のテスト: 通常ゲームモードに移行
        /// </summary>
        private void TestRequirement204()
        {
            bool passed = true;
            string testName = "Requirement 20.4: Transition to normal game mode";
            
            // GameStateがPlayingになっているかチェック
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentState != GameState.Playing)
                {
                    LogTest($"❌ {testName} - Game state is not Playing");
                    passed = false;
                }
            }
            else
            {
                LogTest($"❌ {testName} - GameManager not found");
                passed = false;
            }
            
            // チュートリアル完了フラグがセットされているかチェック
            if (PlayerPrefs.GetInt("TutorialCompleted", 0) != 1)
            {
                LogTest($"❌ {testName} - Tutorial completion flag not set");
                passed = false;
            }
            
            if (passed)
            {
                LogTest($"✅ {testName} - PASSED");
            }
        }

        /// <summary>
        /// 要件20.5のテスト: スキップ機能
        /// </summary>
        private void TestRequirement205()
        {
            bool passed = true;
            string testName = "Requirement 20.5: Skip functionality";
            
            // チュートリアルが終了しているかチェック
            if (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                LogTest($"❌ {testName} - Tutorial still active after skip");
                passed = false;
            }
            
            // GameStateがPlayingになっているかチェック
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentState != GameState.Playing)
                {
                    LogTest($"❌ {testName} - Game state is not Playing after skip");
                    passed = false;
                }
            }
            
            if (passed)
            {
                LogTest($"✅ {testName} - PASSED");
            }
        }

        /// <summary>
        /// ログ出力
        /// </summary>
        private void LogTest(string message)
        {
            if (verboseLogging)
            {
                Debug.Log($"[TutorialTest] {message}");
            }
            
            testLog += $"{System.DateTime.Now:HH:mm:ss} - {message}\n";
            
            if (logText != null)
            {
                logText.text = testLog;
                
                // ログが長くなりすぎた場合は古い部分を削除
                if (testLog.Length > 2000)
                {
                    int cutIndex = testLog.IndexOf('\n', 500);
                    if (cutIndex > 0)
                    {
                        testLog = testLog.Substring(cutIndex + 1);
                        logText.text = testLog;
                    }
                }
            }
        }

        /// <summary>
        /// ステータス更新
        /// </summary>
        private void UpdateStatus(string status)
        {
            if (statusText != null)
            {
                statusText.text = $"Status: {status}";
            }
        }

        /// <summary>
        /// 全要件テスト実行
        /// </summary>
        [ContextMenu("Run All Requirements Tests")]
        public void RunAllRequirementsTests()
        {
            LogTest("=== Running All Requirements Tests ===");
            
            // 各要件の個別テスト
            StartCoroutine(RunSequentialTests());
        }

        /// <summary>
        /// 順次テスト実行
        /// </summary>
        private IEnumerator RunSequentialTests()
        {
            // テスト1: 通常のチュートリアル完了
            LogTest("Test 1: Normal tutorial completion");
            ResetTutorialTest();
            yield return new WaitForSeconds(1f);
            StartTutorialTest();
            
            // チュートリアル完了まで待機
            while (TutorialManager.Instance != null && TutorialManager.Instance.IsTutorialActive())
            {
                yield return new WaitForSeconds(0.5f);
            }
            
            yield return new WaitForSeconds(2f);
            
            // テスト2: スキップ機能
            LogTest("Test 2: Skip functionality");
            ResetTutorialTest();
            yield return new WaitForSeconds(1f);
            StartTutorialTest();
            yield return new WaitForSeconds(2f);
            SkipTutorialTest();
            
            yield return new WaitForSeconds(2f);
            
            LogTest("=== All Requirements Tests Completed ===");
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnTutorialStarted -= OnTutorialStarted;
                TutorialManager.Instance.OnTutorialCompleted -= OnTutorialCompleted;
                TutorialManager.Instance.OnTutorialSkipped -= OnTutorialSkipped;
                TutorialManager.Instance.OnStepCompleted -= OnStepCompleted;
            }
        }
    }
}