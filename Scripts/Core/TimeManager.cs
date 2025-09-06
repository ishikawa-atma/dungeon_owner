using UnityEngine;
using System;
using System.Collections.Generic;
using DungeonOwner.Managers;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 時間制御システム
    /// 要件14.1, 14.2, 14.5に対応
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField] private float dayDuration = 300f; // 5分 = 1日
        [SerializeField] private float[] speedMultipliers = { 1.0f, 1.5f, 2.0f };

        [Header("Speed Control Settings")]
        [SerializeField] private bool enableSpeedControl = true;
        [SerializeField] private float speedTransitionDuration = 0.2f;

        private float currentDayTime;
        private int currentDay;
        private int currentSpeedIndex = 0;
        private float targetSpeedMultiplier;
        private float currentSpeedMultiplier;
        private bool isTransitioning = false;

        // 速度制御対象のコンポーネントを管理
        private List<ITimeScalable> timeScalableComponents = new List<ITimeScalable>();

        public float CurrentDayTime => currentDayTime;
        public int CurrentDay => currentDay;
        public float CurrentSpeedMultiplier => currentSpeedMultiplier;
        public float TargetSpeedMultiplier => targetSpeedMultiplier;
        public bool IsDayComplete => currentDayTime >= dayDuration;
        public bool IsSpeedControlEnabled => enableSpeedControl;
        public float[] AvailableSpeedMultipliers => speedMultipliers;

        public event Action OnDayCompleted;
        public event Action<float> OnDayTimeChanged;
        public event Action<float> OnSpeedChanged;
        public event Action<float, float> OnSpeedTransitionStarted;

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
                UpdateSpeedTransition();
                UpdateDayTime();
                UpdateTimeScalableComponents();
            }
        }

        private void InitializeTime()
        {
            currentDayTime = 0f;
            currentDay = 1;
            currentSpeedIndex = 0;
            currentSpeedMultiplier = speedMultipliers[0];
            targetSpeedMultiplier = currentSpeedMultiplier;
            isTransitioning = false;
        }

        /// <summary>
        /// 速度遷移の更新
        /// </summary>
        private void UpdateSpeedTransition()
        {
            if (isTransitioning)
            {
                float transitionSpeed = 1.0f / speedTransitionDuration;
                currentSpeedMultiplier = Mathf.MoveTowards(
                    currentSpeedMultiplier, 
                    targetSpeedMultiplier, 
                    transitionSpeed * Time.unscaledDeltaTime
                );

                if (Mathf.Approximately(currentSpeedMultiplier, targetSpeedMultiplier))
                {
                    currentSpeedMultiplier = targetSpeedMultiplier;
                    isTransitioning = false;
                }
            }
        }

        /// <summary>
        /// 日時の更新
        /// 要件14.2: 全ゲーム要素への速度適用
        /// </summary>
        private void UpdateDayTime()
        {
            float deltaTime = Time.unscaledDeltaTime * currentSpeedMultiplier;
            currentDayTime += deltaTime;
            
            OnDayTimeChanged?.Invoke(currentDayTime / dayDuration);

            if (IsDayComplete)
            {
                CompleteDay();
            }
        }

        /// <summary>
        /// 時間スケール対応コンポーネントの更新
        /// 要件14.2: 全ゲーム要素への速度適用システム
        /// </summary>
        private void UpdateTimeScalableComponents()
        {
            float deltaTime = Time.unscaledDeltaTime * currentSpeedMultiplier;
            
            for (int i = timeScalableComponents.Count - 1; i >= 0; i--)
            {
                if (timeScalableComponents[i] == null)
                {
                    timeScalableComponents.RemoveAt(i);
                    continue;
                }
                
                timeScalableComponents[i].UpdateWithTimeScale(deltaTime, currentSpeedMultiplier);
            }
        }

        private void CompleteDay()
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

        /// <summary>
        /// ゲーム速度をサイクル切り替え
        /// 要件14.1: ゲーム速度切り替え（1x/1.5x/2x）機能
        /// </summary>
        public void CycleGameSpeed()
        {
            if (!enableSpeedControl) return;

            currentSpeedIndex = (currentSpeedIndex + 1) % speedMultipliers.Length;
            float newSpeed = speedMultipliers[currentSpeedIndex];
            
            SetGameSpeed(newSpeed);
        }

        /// <summary>
        /// ゲーム速度を設定
        /// 要件14.1, 14.2: 速度切り替えと全要素への適用
        /// </summary>
        public void SetGameSpeed(float multiplier)
        {
            if (!enableSpeedControl) return;

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
            float previousSpeed = targetSpeedMultiplier;
            targetSpeedMultiplier = closestSpeed;
            
            // スムーズな遷移を開始
            if (!Mathf.Approximately(currentSpeedMultiplier, targetSpeedMultiplier))
            {
                isTransitioning = true;
                OnSpeedTransitionStarted?.Invoke(previousSpeed, targetSpeedMultiplier);
            }
            
            // GameManagerにも通知
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameSpeed(closestSpeed);
            }
            
            OnSpeedChanged?.Invoke(closestSpeed);
            
            Debug.Log($"Game speed changed from {previousSpeed:F1}x to {closestSpeed:F1}x");
        }

        /// <summary>
        /// 速度制御の有効/無効を設定
        /// </summary>
        public void SetSpeedControlEnabled(bool enabled)
        {
            enableSpeedControl = enabled;
            
            if (!enabled)
            {
                // 速度制御が無効の場合は通常速度に戻す
                SetGameSpeed(1.0f);
            }
        }

        /// <summary>
        /// 時間スケール対応コンポーネントを登録
        /// 要件14.2: 全ゲーム要素への速度適用システム
        /// </summary>
        public void RegisterTimeScalable(ITimeScalable component)
        {
            if (component != null && !timeScalableComponents.Contains(component))
            {
                timeScalableComponents.Add(component);
                Debug.Log($"Registered time scalable component: {component.GetType().Name}");
            }
        }

        /// <summary>
        /// 時間スケール対応コンポーネントの登録を解除
        /// </summary>
        public void UnregisterTimeScalable(ITimeScalable component)
        {
            if (component != null && timeScalableComponents.Contains(component))
            {
                timeScalableComponents.Remove(component);
                Debug.Log($"Unregistered time scalable component: {component.GetType().Name}");
            }
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