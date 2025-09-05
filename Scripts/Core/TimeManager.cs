using UnityEngine;
using System;
using DungeonOwner.Managers;

namespace DungeonOwner.Core
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField] private float dayDuration = 300f; // 5分 = 1日
        [SerializeField] private float[] speedMultipliers = { 1.0f, 1.5f, 2.0f };

        private float currentDayTime;
        private int currentDay;
        private int currentSpeedIndex = 0;

        public float CurrentDayTime => currentDayTime;
        public int CurrentDay => currentDay;
        public float CurrentSpeedMultiplier => speedMultipliers[currentSpeedIndex];
        public bool IsDayComplete => currentDayTime >= dayDuration;

        public event Action OnDayCompleted;
        public event Action<float> OnDayTimeChanged;
        public event Action<float> OnSpeedChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeTime();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }
        }

        private void Update()
        {
            if (GameManager.Instance?.CurrentState == GameState.Playing)
            {
                UpdateDayTime();
            }
        }

        private void InitializeTime()
        {
            currentDayTime = 0f;
            currentDay = 1;
            currentSpeedIndex = 0;
        }

        private void UpdateDayTime()
        {
            float deltaTime = Time.unscaledDeltaTime * CurrentSpeedMultiplier;
            currentDayTime += deltaTime;
            
            OnDayTimeChanged?.Invoke(currentDayTime / dayDuration);

            if (IsDayComplete)
            {
                CompletDay();
            }
        }

        private void CompletDay()
        {
            currentDay++;
            currentDayTime = 0f;
            
            // 日次報酬処理
            ProcessDailyReward();
            
            OnDayCompleted?.Invoke();
            Debug.Log($"Day {currentDay - 1} completed. Starting day {currentDay}");
        }

        /// <summary>
        /// 日次報酬処理
        /// </summary>
        private void ProcessDailyReward()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.CheckDailyReward();
                Debug.Log($"Daily reward processed for day {currentDay}");
            }
        }

        public void CycleGameSpeed()
        {
            currentSpeedIndex = (currentSpeedIndex + 1) % speedMultipliers.Length;
            float newSpeed = CurrentSpeedMultiplier;
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameSpeed(newSpeed);
            }
            
            OnSpeedChanged?.Invoke(newSpeed);
        }

        public void SetGameSpeed(float multiplier)
        {
            // 最も近い有効な速度を見つける
            float closestSpeed = speedMultipliers[0];
            int closestIndex = 0;
            
            for (int i = 0; i < speedMultipliers.Length; i++)
            {
                if (Mathf.Abs(speedMultipliers[i] - multiplier) < Mathf.Abs(closestSpeed - multiplier))
                {
                    closestSpeed = speedMultipliers[i];
                    closestIndex = i;
                }
            }
            
            currentSpeedIndex = closestIndex;
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameSpeed(closestSpeed);
            }
            
            OnSpeedChanged?.Invoke(closestSpeed);
        }

        public float GetDayProgress()
        {
            return currentDayTime / dayDuration;
        }

        public string GetFormattedTime()
        {
            float progress = GetDayProgress();
            int hours = Mathf.FloorToInt(progress * 24);
            int minutes = Mathf.FloorToInt((progress * 24 - hours) * 60);
            return $"{hours:00}:{minutes:00}";
        }

        private void OnGameStateChanged(GameState newState)
        {
            // ゲーム状態に応じた時間制御の調整
            switch (newState)
            {
                case GameState.Paused:
                case GameState.GameOver:
                case GameState.MainMenu:
                    // 時間停止は GameManager で Time.timeScale を制御
                    break;
                case GameState.Playing:
                    // ゲーム再開時の処理
                    break;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }
    }
}