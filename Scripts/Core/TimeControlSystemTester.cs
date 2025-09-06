using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.UI;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 時間制御システムのテスター
    /// 要件14.1, 14.2, 14.5のテスト
    /// </summary>
    public class TimeControlSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableAutoTest = false;
        [SerializeField] private float testInterval = 3.0f;
        [SerializeField] private bool logDetailedInfo = true;

        [Header("UI References")]
        [SerializeField] private SpeedControlUI speedControlUI;

        private float testTimer = 0f;
        private int testCycleCount = 0;
        private TestTimeScalableComponent testComponent;

        private void Start()
        {
            InitializeTest();
        }

        private void Update()
        {
            if (enableAutoTest)
            {
                RunAutoTest();
            }

            // キーボードショートカットでのテスト
            HandleKeyboardInput();
        }

        private void InitializeTest()
        {
            Debug.Log("=== Time Control System Test Started ===");
            
            // テスト用のTimeScalableコンポーネントを作成
            testComponent = gameObject.AddComponent<TestTimeScalableComponent>();
            
            // TimeManagerに登録
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.RegisterTimeScalable(testComponent);
                TimeManager.Instance.OnSpeedChanged += OnSpeedChanged;
                TimeManager.Instance.OnSpeedTransitionStarted += OnSpeedTransitionStarted;
            }

            // SpeedControlUIの参照を取得
            if (speedControlUI == null)
            {
                speedControlUI = FindObjectOfType<SpeedControlUI>();
            }

            LogSystemStatus();
        }

        private void RunAutoTest()
        {
            testTimer += Time.unscaledDeltaTime;
            
            if (testTimer >= testInterval)
            {
                testTimer = 0f;
                testCycleCount++;
                
                // 速度をサイクル切り替え
                if (TimeManager.Instance != null)
                {
                    TimeManager.Instance.CycleGameSpeed();
                    
                    if (logDetailedInfo)
                    {
                        Debug.Log($"Auto test cycle {testCycleCount}: Speed changed to {TimeManager.Instance.CurrentSpeedMultiplier:F1}x");
                    }
                }
            }
        }

        private void HandleKeyboardInput()
        {
            // 数字キーで直接速度設定
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                TestSetSpeed(1.0f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                TestSetSpeed(1.5f);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                TestSetSpeed(2.0f);
            }
            
            // スペースキーで速度サイクル
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TestCycleSpeed();
            }
            
            // Pキーで一時停止/再開
            if (Input.GetKeyDown(KeyCode.P))
            {
                TestPauseResume();
            }
            
            // Lキーでログ出力
            if (Input.GetKeyDown(KeyCode.L))
            {
                LogSystemStatus();
            }
        }

        [ContextMenu("Test Set Speed 1x")]
        public void TestSetSpeed1x()
        {
            TestSetSpeed(1.0f);
        }

        [ContextMenu("Test Set Speed 1.5x")]
        public void TestSetSpeed15x()
        {
            TestSetSpeed(1.5f);
        }

        [ContextMenu("Test Set Speed 2x")]
        public void TestSetSpeed2x()
        {
            TestSetSpeed(2.0f);
        }

        [ContextMenu("Test Cycle Speed")]
        public void TestCycleSpeed()
        {
            if (TimeManager.Instance != null)
            {
                float previousSpeed = TimeManager.Instance.CurrentSpeedMultiplier;
                TimeManager.Instance.CycleGameSpeed();
                
                Debug.Log($"Speed cycled from {previousSpeed:F1}x to {TimeManager.Instance.CurrentSpeedMultiplier:F1}x");
            }
        }

        [ContextMenu("Test Pause/Resume")]
        public void TestPauseResume()
        {
            if (GameManager.Instance != null)
            {
                if (GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.PauseGame();
                    Debug.Log("Game paused");
                }
                else if (GameManager.Instance.CurrentState == GameState.Paused)
                {
                    GameManager.Instance.ResumeGame();
                    Debug.Log("Game resumed");
                }
            }
        }

        [ContextMenu("Log System Status")]
        public void LogSystemStatus()
        {
            if (TimeManager.Instance != null)
            {
                Debug.Log($"=== Time Control System Status ===");
                Debug.Log($"Current Speed: {TimeManager.Instance.CurrentSpeedMultiplier:F1}x");
                Debug.Log($"Target Speed: {TimeManager.Instance.TargetSpeedMultiplier:F1}x");
                Debug.Log($"Speed Control Enabled: {TimeManager.Instance.IsSpeedControlEnabled}");
                Debug.Log($"Available Speeds: {string.Join(", ", System.Array.ConvertAll(TimeManager.Instance.AvailableSpeedMultipliers, x => x.ToString("F1") + "x"))}");
                Debug.Log($"Current Day: {TimeManager.Instance.CurrentDay}");
                Debug.Log($"Day Progress: {(TimeManager.Instance.CurrentDayTime / 300f * 100):F1}%");
            }

            if (GameManager.Instance != null)
            {
                Debug.Log($"Game State: {GameManager.Instance.CurrentState}");
                Debug.Log($"Time Scale: {Time.timeScale}");
            }
        }

        private void TestSetSpeed(float speed)
        {
            if (TimeManager.Instance != null)
            {
                float previousSpeed = TimeManager.Instance.CurrentSpeedMultiplier;
                TimeManager.Instance.SetGameSpeed(speed);
                
                Debug.Log($"Speed set from {previousSpeed:F1}x to {speed:F1}x");
            }
        }

        private void OnSpeedChanged(float newSpeed)
        {
            if (logDetailedInfo)
            {
                Debug.Log($"Speed changed event: {newSpeed:F1}x");
            }
        }

        private void OnSpeedTransitionStarted(float fromSpeed, float toSpeed)
        {
            if (logDetailedInfo)
            {
                Debug.Log($"Speed transition started: {fromSpeed:F1}x → {toSpeed:F1}x");
            }
        }

        private void OnDestroy()
        {
            // イベント購読を解除
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnSpeedChanged -= OnSpeedChanged;
                TimeManager.Instance.OnSpeedTransitionStarted -= OnSpeedTransitionStarted;
                
                if (testComponent != null)
                {
                    TimeManager.Instance.UnregisterTimeScalable(testComponent);
                }
            }
        }
    }

    /// <summary>
    /// テスト用のTimeScalableコンポーネント
    /// </summary>
    public class TestTimeScalableComponent : MonoBehaviour, ITimeScalable
    {
        private float totalScaledTime = 0f;
        private float lastLogTime = 0f;
        private const float LOG_INTERVAL = 5f;

        public void UpdateWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            totalScaledTime += scaledDeltaTime;
            
            // 定期的にログ出力
            if (totalScaledTime - lastLogTime >= LOG_INTERVAL)
            {
                lastLogTime = totalScaledTime;
                Debug.Log($"TestTimeScalable: Total scaled time = {totalScaledTime:F1}s, Current scale = {timeScale:F1}x");
            }
        }

        public void OnTimeScaleChanged(float newTimeScale)
        {
            Debug.Log($"TestTimeScalable: Time scale changed to {newTimeScale:F1}x");
        }
    }
}