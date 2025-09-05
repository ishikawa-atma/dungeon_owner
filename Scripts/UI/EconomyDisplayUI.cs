using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Managers;
using DungeonOwner.Core;

namespace DungeonOwner.UI
{
    /// <summary>
    /// 経済システムの表示UI
    /// 金貨残高、日次報酬、時間表示などを管理
    /// </summary>
    public class EconomyDisplayUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI dayText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private Button speedButton;
        [SerializeField] private Button dailyRewardButton;

        [Header("アニメーション設定")]
        [SerializeField] private float goldChangeAnimationDuration = 0.5f;
        [SerializeField] private Color goldIncreaseColor = Color.green;
        [SerializeField] private Color goldDecreaseColor = Color.red;

        private int lastDisplayedGold = 0;
        private Coroutine goldAnimationCoroutine;

        private void Start()
        {
            InitializeUI();
            RegisterEvents();
            UpdateAllDisplays();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        /// <summary>
        /// UI初期化
        /// </summary>
        private void InitializeUI()
        {
            // ボタンイベント設定
            if (speedButton != null)
            {
                speedButton.onClick.AddListener(OnSpeedButtonClicked);
            }

            if (dailyRewardButton != null)
            {
                dailyRewardButton.onClick.AddListener(OnDailyRewardButtonClicked);
            }

            // 初期表示設定
            if (goldText != null)
            {
                goldText.text = "0";
            }

            if (dayText != null)
            {
                dayText.text = "Day 1";
            }

            if (timeText != null)
            {
                timeText.text = "00:00";
            }

            if (speedText != null)
            {
                speedText.text = "1.0x";
            }
        }

        /// <summary>
        /// イベント登録
        /// </summary>
        private void RegisterEvents()
        {
            // ResourceManagerのイベント
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnGoldChanged += OnGoldChanged;
                ResourceManager.Instance.OnGoldEarned += OnGoldEarned;
                ResourceManager.Instance.OnGoldSpent += OnGoldSpent;
            }

            // TimeManagerのイベント
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayCompleted += OnDayCompleted;
                TimeManager.Instance.OnDayTimeChanged += OnDayTimeChanged;
                TimeManager.Instance.OnSpeedChanged += OnSpeedChanged;
            }
        }

        /// <summary>
        /// イベント登録解除
        /// </summary>
        private void UnregisterEvents()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnGoldChanged -= OnGoldChanged;
                ResourceManager.Instance.OnGoldEarned -= OnGoldEarned;
                ResourceManager.Instance.OnGoldSpent -= OnGoldSpent;
            }

            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayCompleted -= OnDayCompleted;
                TimeManager.Instance.OnDayTimeChanged -= OnDayTimeChanged;
                TimeManager.Instance.OnSpeedChanged -= OnSpeedChanged;
            }
        }

        /// <summary>
        /// 全表示更新
        /// </summary>
        private void UpdateAllDisplays()
        {
            UpdateGoldDisplay();
            UpdateDayDisplay();
            UpdateTimeDisplay();
            UpdateSpeedDisplay();
        }

        /// <summary>
        /// 金貨表示更新
        /// </summary>
        private void UpdateGoldDisplay()
        {
            if (ResourceManager.Instance != null && goldText != null)
            {
                int currentGold = ResourceManager.Instance.Gold;
                goldText.text = $"{currentGold:N0}";
                lastDisplayedGold = currentGold;
            }
        }

        /// <summary>
        /// 日数表示更新
        /// </summary>
        private void UpdateDayDisplay()
        {
            if (TimeManager.Instance != null && dayText != null)
            {
                dayText.text = $"Day {TimeManager.Instance.CurrentDay}";
            }
        }

        /// <summary>
        /// 時間表示更新
        /// </summary>
        private void UpdateTimeDisplay()
        {
            if (TimeManager.Instance != null && timeText != null)
            {
                timeText.text = TimeManager.Instance.GetFormattedTime();
            }
        }

        /// <summary>
        /// 速度表示更新
        /// </summary>
        private void UpdateSpeedDisplay()
        {
            if (TimeManager.Instance != null && speedText != null)
            {
                speedText.text = $"{TimeManager.Instance.CurrentSpeedMultiplier:F1}x";
            }
        }

        /// <summary>
        /// 金貨変更時の処理
        /// </summary>
        private void OnGoldChanged(int newGold)
        {
            if (goldText != null)
            {
                // アニメーション付きで金貨表示を更新
                if (goldAnimationCoroutine != null)
                {
                    StopCoroutine(goldAnimationCoroutine);
                }
                goldAnimationCoroutine = StartCoroutine(AnimateGoldChange(lastDisplayedGold, newGold));
            }
        }

        /// <summary>
        /// 金貨獲得時の処理
        /// </summary>
        private void OnGoldEarned(int amount)
        {
            // 獲得エフェクト表示
            ShowGoldChangeEffect(amount, goldIncreaseColor, "+");
        }

        /// <summary>
        /// 金貨消費時の処理
        /// </summary>
        private void OnGoldSpent(int amount)
        {
            // 消費エフェクト表示
            ShowGoldChangeEffect(amount, goldDecreaseColor, "-");
        }

        /// <summary>
        /// 日完了時の処理
        /// </summary>
        private void OnDayCompleted()
        {
            UpdateDayDisplay();
            
            // 日次報酬通知（簡易版）
            Debug.Log("新しい日が始まりました！日次報酬を獲得しました。");
        }

        /// <summary>
        /// 日時間変更時の処理
        /// </summary>
        private void OnDayTimeChanged(float progress)
        {
            UpdateTimeDisplay();
        }

        /// <summary>
        /// 速度変更時の処理
        /// </summary>
        private void OnSpeedChanged(float newSpeed)
        {
            UpdateSpeedDisplay();
        }

        /// <summary>
        /// 速度ボタンクリック時の処理
        /// </summary>
        private void OnSpeedButtonClicked()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.CycleGameSpeed();
            }
        }

        /// <summary>
        /// 日次報酬ボタンクリック時の処理（デバッグ用）
        /// </summary>
        private void OnDailyRewardButtonClicked()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.ForceDailyReward();
            }
        }

        /// <summary>
        /// 金貨変更アニメーション
        /// </summary>
        private System.Collections.IEnumerator AnimateGoldChange(int fromGold, int toGold)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < goldChangeAnimationDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float progress = elapsedTime / goldChangeAnimationDuration;
                
                int currentDisplayGold = Mathf.RoundToInt(Mathf.Lerp(fromGold, toGold, progress));
                goldText.text = $"{currentDisplayGold:N0}";
                
                yield return null;
            }
            
            goldText.text = $"{toGold:N0}";
            lastDisplayedGold = toGold;
            goldAnimationCoroutine = null;
        }

        /// <summary>
        /// 金貨変更エフェクト表示
        /// </summary>
        private void ShowGoldChangeEffect(int amount, Color color, string prefix)
        {
            // 簡易版: コンソールログ出力
            Debug.Log($"{prefix}{amount} Gold!");
            
            // TODO: 実際のエフェクト表示（パーティクル、フローティングテキストなど）
        }

        /// <summary>
        /// Update処理（フレーム毎の更新が必要な要素）
        /// </summary>
        private void Update()
        {
            // 時間表示は頻繁に更新
            if (TimeManager.Instance != null)
            {
                UpdateTimeDisplay();
            }
        }

        /// <summary>
        /// デバッグ用メソッド
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugAddGold(int amount)
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.DebugAddGold(amount);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugSpendGold(int amount)
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.SpendGold(amount);
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public void DebugForceDailyReward()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.ForceDailyReward();
            }
        }
    }
}