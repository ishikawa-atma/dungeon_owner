using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Core;
using DungeonOwner.UI;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 時間制御システムのデモンストレーション
    /// 要件14.1, 14.2, 14.5の動作確認用
    /// </summary>
    public class TimeControlSystemDemo : MonoBehaviour
    {
        [Header("Demo UI")]
        [SerializeField] private Button speedButton;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Slider timeProgressSlider;
        [SerializeField] private TextMeshProUGUI timeText;

        [Header("Demo Objects")]
        [SerializeField] private GameObject[] movingObjects;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float moveRange = 5f;

        private Vector3[] originalPositions;
        private float[] moveDirections;

        private void Start()
        {
            InitializeDemo();
            SetupUI();
        }

        private void InitializeDemo()
        {
            // 移動オブジェクトの初期化
            if (movingObjects != null && movingObjects.Length > 0)
            {
                originalPositions = new Vector3[movingObjects.Length];
                moveDirections = new float[movingObjects.Length];

                for (int i = 0; i < movingObjects.Length; i++)
                {
                    if (movingObjects[i] != null)
                    {
                        originalPositions[i] = movingObjects[i].transform.position;
                        moveDirections[i] = Random.Range(-1f, 1f);
                    }
                }
            }

            Debug.Log("Time Control System Demo initialized");
        }

        private void SetupUI()
        {
            // 速度ボタンの設定
            if (speedButton != null)
            {
                speedButton.onClick.AddListener(OnSpeedButtonClicked);
            }

            // TimeManagerのイベントに登録
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnSpeedChanged += UpdateSpeedDisplay;
                TimeManager.Instance.OnDayTimeChanged += UpdateTimeDisplay;
            }

            // 初期表示を更新
            UpdateSpeedDisplay(TimeManager.Instance?.CurrentSpeedMultiplier ?? 1.0f);
        }

        private void Update()
        {
            UpdateMovingObjects();
            UpdateStatusDisplay();
        }

        private void UpdateMovingObjects()
        {
            if (movingObjects == null || originalPositions == null) return;

            // 時間制御システムを考慮した移動
            float deltaTime = Time.deltaTime;
            if (TimeManager.Instance != null && TimeManager.Instance.IsSpeedControlEnabled)
            {
                deltaTime = Time.unscaledDeltaTime * TimeManager.Instance.CurrentSpeedMultiplier;
            }

            for (int i = 0; i < movingObjects.Length; i++)
            {
                if (movingObjects[i] == null) continue;

                // 往復移動
                Vector3 currentPos = movingObjects[i].transform.position;
                float offset = Mathf.Sin(Time.time * moveSpeed + i) * moveRange;
                
                Vector3 targetPos = originalPositions[i] + Vector3.right * offset;
                movingObjects[i].transform.position = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * deltaTime);
            }
        }

        private void UpdateStatusDisplay()
        {
            if (statusText == null) return;

            string status = "=== Time Control System Status ===\n";
            
            if (TimeManager.Instance != null)
            {
                status += $"Current Speed: {TimeManager.Instance.CurrentSpeedMultiplier:F1}x\n";
                status += $"Target Speed: {TimeManager.Instance.TargetSpeedMultiplier:F1}x\n";
                status += $"Speed Control: {(TimeManager.Instance.IsSpeedControlEnabled ? "Enabled" : "Disabled")}\n";
                status += $"Day: {TimeManager.Instance.CurrentDay}\n";
                status += $"Time: {TimeManager.Instance.GetFormattedTime()}\n";
            }

            if (GameManager.Instance != null)
            {
                status += $"Game State: {GameManager.Instance.CurrentState}\n";
                status += $"Unity Time Scale: {Time.timeScale:F2}\n";
            }

            status += $"FPS: {(1f / Time.unscaledDeltaTime):F0}";

            statusText.text = status;
        }

        private void OnSpeedButtonClicked()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.CycleGameSpeed();
            }
        }

        private void UpdateSpeedDisplay(float speed)
        {
            if (speedText != null)
            {
                speedText.text = $"Speed: {speed:F1}x";
            }
        }

        private void UpdateTimeDisplay(float progress)
        {
            if (timeProgressSlider != null)
            {
                timeProgressSlider.value = progress;
            }

            if (timeText != null && TimeManager.Instance != null)
            {
                timeText.text = $"Day {TimeManager.Instance.CurrentDay} - {TimeManager.Instance.GetFormattedTime()}";
            }
        }

        // デモ用のコントロールメソッド
        [ContextMenu("Set Speed 1x")]
        public void SetSpeed1x()
        {
            TimeManager.Instance?.SetGameSpeed(1.0f);
        }

        [ContextMenu("Set Speed 1.5x")]
        public void SetSpeed15x()
        {
            TimeManager.Instance?.SetGameSpeed(1.5f);
        }

        [ContextMenu("Set Speed 2x")]
        public void SetSpeed2x()
        {
            TimeManager.Instance?.SetGameSpeed(2.0f);
        }

        [ContextMenu("Toggle Speed Control")]
        public void ToggleSpeedControl()
        {
            if (TimeManager.Instance != null)
            {
                bool currentState = TimeManager.Instance.IsSpeedControlEnabled;
                TimeManager.Instance.SetSpeedControlEnabled(!currentState);
                Debug.Log($"Speed control {(currentState ? "disabled" : "enabled")}");
            }
        }

        [ContextMenu("Pause Game")]
        public void PauseGame()
        {
            GameManager.Instance?.PauseGame();
        }

        [ContextMenu("Resume Game")]
        public void ResumeGame()
        {
            GameManager.Instance?.ResumeGame();
        }

        private void OnDestroy()
        {
            // イベント購読を解除
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnSpeedChanged -= UpdateSpeedDisplay;
                TimeManager.Instance.OnDayTimeChanged -= UpdateTimeDisplay;
            }

            if (speedButton != null)
            {
                speedButton.onClick.RemoveListener(OnSpeedButtonClicked);
            }
        }

        private void OnDrawGizmos()
        {
            // 移動範囲を可視化
            if (originalPositions != null)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < originalPositions.Length; i++)
                {
                    Vector3 start = originalPositions[i] + Vector3.left * moveRange;
                    Vector3 end = originalPositions[i] + Vector3.right * moveRange;
                    Gizmos.DrawLine(start, end);
                }
            }
        }
    }
}