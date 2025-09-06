using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DungeonOwner.Core;
using DungeonOwner.Managers;

namespace DungeonOwner.UI
{
    /// <summary>
    /// リソース表示UI（縦画面レイアウト最適化）
    /// 要件15.1, 15.2に対応
    /// </summary>
    public class ResourceDisplayUI : MonoBehaviour
    {
        public static ResourceDisplayUI Instance { get; private set; }

        [Header("Resource Display")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI floorText;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Progress Bars")]
        [SerializeField] private Slider experienceBar;
        [SerializeField] private Slider timeProgressBar;
        [SerializeField] private Image experienceBarFill;
        [SerializeField] private Image timeProgressBarFill;

        [Header("Status Icons")]
        [SerializeField] private Image goldIcon;
        [SerializeField] private Image floorIcon;
        [SerializeField] private Image timeIcon;
        [SerializeField] private Image levelIcon;

        [Header("Animation Settings")]
        [SerializeField] private float goldChangeAnimationDuration = 0.5f;
        [SerializeField] private float iconPulseScale = 1.2f;
        [SerializeField] private Color positiveChangeColor = Color.green;
        [SerializeField] private Color negativeChangeColor = Color.red;

        [Header("Portrait Layout")]
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private float compactHeight = 80f;
        [SerializeField] private float expandedHeight = 120f;
        [SerializeField] private bool autoCompactInCombat = true;

        // 内部状態
        private int currentGold = 0;
        private int displayedGold = 0;
        private int currentFloor = 1;
        private float currentTime = 0f;
        private int currentLevel = 1;
        private float currentExperience = 0f;
        private float maxExperience = 100f;

        // アニメーション用
        private Coroutine goldAnimationCoroutine;
        private bool isCompactMode = false;

        // イベント
        public System.Action<int> OnGoldChanged;
        public System.Action<int> OnFloorChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeResourceDisplay();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SubscribeToEvents();
            UpdateAllDisplays();
            ConfigurePortraitLayout();
        }

        private void Update()
        {
            UpdateTimeDisplay();
            CheckCompactMode();
        }

        /// <summary>
        /// リソース表示の初期化
        /// </summary>
        private void InitializeResourceDisplay()
        {
            // 初期値の設定
            currentGold = 1000; // 初期金貨
            displayedGold = currentGold;
            currentFloor = 1;
            currentTime = 0f;
            currentLevel = 1;
            currentExperience = 0f;
            maxExperience = 100f;
        }

        /// <summary>
        /// 縦画面レイアウトの設定
        /// </summary>
        private void ConfigurePortraitLayout()
        {
            if (topPanel == null) return;

            // 縦画面に最適化されたレイアウト設定
            topPanel.sizeDelta = new Vector2(0, expandedHeight);
            
            // リソース要素を横並びに配置
            ConfigureHorizontalLayout();
        }

        /// <summary>
        /// 横並びレイアウトの設定
        /// </summary>
        private void ConfigureHorizontalLayout()
        {
            // 金貨表示を左端に配置
            if (goldText != null)
            {
                RectTransform goldRect = goldText.GetComponent<RectTransform>();
                SetAnchorPosition(goldRect, new Vector2(0.1f, 0.5f), new Vector2(-100, 0));
            }

            // 階層表示を左中央に配置
            if (floorText != null)
            {
                RectTransform floorRect = floorText.GetComponent<RectTransform>();
                SetAnchorPosition(floorRect, new Vector2(0.35f, 0.5f), new Vector2(0, 0));
            }

            // 時間表示を右中央に配置
            if (timeText != null)
            {
                RectTransform timeRect = timeText.GetComponent<RectTransform>();
                SetAnchorPosition(timeRect, new Vector2(0.65f, 0.5f), new Vector2(0, 0));
            }

            // レベル表示を右端に配置
            if (levelText != null)
            {
                RectTransform levelRect = levelText.GetComponent<RectTransform>();
                SetAnchorPosition(levelRect, new Vector2(0.9f, 0.5f), new Vector2(100, 0));
            }
        }

        /// <summary>
        /// アンカー位置の設定
        /// </summary>
        private void SetAnchorPosition(RectTransform rectTransform, Vector2 anchor, Vector2 offset)
        {
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;
            rectTransform.anchoredPosition = offset;
        }

        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            // ResourceManagerのイベント
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnGoldChanged += OnGoldValueChanged;
            }

            // FloorSystemのイベント
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged += OnFloorValueChanged;
                FloorSystem.Instance.OnFloorExpanded += OnFloorExpanded;
            }

            // TimeManagerのイベント
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged += OnTimeValueChanged;
                TimeManager.Instance.OnDayCompleted += OnDayCompleted;
            }

            // LevelDisplayManagerのイベント
            if (LevelDisplayManager.Instance != null)
            {
                LevelDisplayManager.Instance.OnLevelChanged += OnLevelValueChanged;
                LevelDisplayManager.Instance.OnExperienceChanged += OnExperienceValueChanged;
            }

            // CombatVisualUIのイベント
            if (CombatVisualUI.Instance != null)
            {
                CombatVisualUI.Instance.OnCombatStateChanged += OnCombatStateChanged;
            }
        }

        /// <summary>
        /// 全表示の更新
        /// </summary>
        private void UpdateAllDisplays()
        {
            UpdateGoldDisplay();
            UpdateFloorDisplay();
            UpdateTimeDisplay();
            UpdateLevelDisplay();
            UpdateExperienceDisplay();
        }

        /// <summary>
        /// 金貨表示の更新
        /// </summary>
        private void UpdateGoldDisplay()
        {
            if (goldText != null)
            {
                goldText.text = $"{displayedGold:N0}G";
            }
        }

        /// <summary>
        /// 階層表示の更新
        /// </summary>
        private void UpdateFloorDisplay()
        {
            if (floorText != null)
            {
                floorText.text = $"階層 {currentFloor}";
            }
        }

        /// <summary>
        /// 時間表示の更新
        /// </summary>
        private void UpdateTimeDisplay()
        {
            if (TimeManager.Instance != null)
            {
                currentTime = TimeManager.Instance.GetCurrentTime();
            }

            if (timeText != null)
            {
                int hours = Mathf.FloorToInt(currentTime);
                int minutes = Mathf.FloorToInt((currentTime - hours) * 60);
                timeText.text = $"{hours:D2}:{minutes:D2}";
            }

            // 時間プログレスバーの更新
            if (timeProgressBar != null)
            {
                float dayProgress = (currentTime % 24f) / 24f;
                timeProgressBar.value = dayProgress;
                
                if (timeProgressBarFill != null)
                {
                    // 時間帯に応じて色を変更
                    Color timeColor = GetTimeColor(currentTime % 24f);
                    timeProgressBarFill.color = timeColor;
                }
            }
        }

        /// <summary>
        /// レベル表示の更新
        /// </summary>
        private void UpdateLevelDisplay()
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{currentLevel}";
            }
        }

        /// <summary>
        /// 経験値表示の更新
        /// </summary>
        private void UpdateExperienceDisplay()
        {
            if (experienceBar != null)
            {
                experienceBar.value = currentExperience / maxExperience;
            }

            if (experienceBarFill != null)
            {
                // 経験値の割合に応じて色を変更
                float expRatio = currentExperience / maxExperience;
                experienceBarFill.color = Color.Lerp(Color.blue, Color.gold, expRatio);
            }
        }

        /// <summary>
        /// 時間帯に応じた色の取得
        /// </summary>
        private Color GetTimeColor(float timeOfDay)
        {
            if (timeOfDay >= 6f && timeOfDay < 18f)
            {
                // 昼間（6:00-18:00）
                return Color.yellow;
            }
            else if (timeOfDay >= 18f && timeOfDay < 22f)
            {
                // 夕方（18:00-22:00）
                return Color.Lerp(Color.yellow, Color.red, (timeOfDay - 18f) / 4f);
            }
            else
            {
                // 夜間（22:00-6:00）
                return Color.blue;
            }
        }

        /// <summary>
        /// コンパクトモードのチェック
        /// </summary>
        private void CheckCompactMode()
        {
            if (!autoCompactInCombat) return;

            bool shouldBeCompact = CombatVisualUI.Instance != null && CombatVisualUI.Instance.IsInCombat();
            
            if (shouldBeCompact != isCompactMode)
            {
                SetCompactMode(shouldBeCompact);
            }
        }

        /// <summary>
        /// コンパクトモードの設定
        /// </summary>
        private void SetCompactMode(bool compact)
        {
            isCompactMode = compact;
            
            if (topPanel != null)
            {
                float targetHeight = compact ? compactHeight : expandedHeight;
                StartCoroutine(AnimateHeight(topPanel, targetHeight, 0.3f));
            }

            // コンパクトモード時は一部の情報を非表示
            if (experienceBar != null)
            {
                experienceBar.gameObject.SetActive(!compact);
            }
            
            if (timeProgressBar != null)
            {
                timeProgressBar.gameObject.SetActive(!compact);
            }
        }

        /// <summary>
        /// 高さアニメーション
        /// </summary>
        private IEnumerator AnimateHeight(RectTransform rectTransform, float targetHeight, float duration)
        {
            float startHeight = rectTransform.sizeDelta.y;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                Vector2 sizeDelta = rectTransform.sizeDelta;
                sizeDelta.y = Mathf.Lerp(startHeight, targetHeight, t);
                rectTransform.sizeDelta = sizeDelta;
                
                yield return null;
            }

            Vector2 finalSize = rectTransform.sizeDelta;
            finalSize.y = targetHeight;
            rectTransform.sizeDelta = finalSize;
        }

        /// <summary>
        /// 金貨変更アニメーション
        /// </summary>
        private IEnumerator AnimateGoldChange(int startValue, int endValue)
        {
            float elapsed = 0f;
            bool isIncrease = endValue > startValue;
            
            // アイコンのパルスアニメーション
            if (goldIcon != null)
            {
                StartCoroutine(PulseIcon(goldIcon, isIncrease ? positiveChangeColor : negativeChangeColor));
            }

            while (elapsed < goldChangeAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / goldChangeAnimationDuration;
                
                displayedGold = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
                UpdateGoldDisplay();
                
                yield return null;
            }

            displayedGold = endValue;
            UpdateGoldDisplay();
            goldAnimationCoroutine = null;
        }

        /// <summary>
        /// アイコンのパルスアニメーション
        /// </summary>
        private IEnumerator PulseIcon(Image icon, Color pulseColor)
        {
            Vector3 originalScale = icon.transform.localScale;
            Color originalColor = icon.color;
            
            // 拡大とカラー変更
            float elapsed = 0f;
            float pulseDuration = 0.2f;
            
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                
                icon.transform.localScale = Vector3.Lerp(originalScale, originalScale * iconPulseScale, t);
                icon.color = Color.Lerp(originalColor, pulseColor, t);
                
                yield return null;
            }
            
            // 縮小と元の色に戻す
            elapsed = 0f;
            while (elapsed < pulseDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pulseDuration;
                
                icon.transform.localScale = Vector3.Lerp(originalScale * iconPulseScale, originalScale, t);
                icon.color = Color.Lerp(pulseColor, originalColor, t);
                
                yield return null;
            }
            
            icon.transform.localScale = originalScale;
            icon.color = originalColor;
        }

        // イベントハンドラー
        private void OnGoldValueChanged(int newGold)
        {
            int oldGold = currentGold;
            currentGold = newGold;
            
            if (goldAnimationCoroutine != null)
            {
                StopCoroutine(goldAnimationCoroutine);
            }
            
            goldAnimationCoroutine = StartCoroutine(AnimateGoldChange(displayedGold, newGold));
            OnGoldChanged?.Invoke(newGold);
        }

        private void OnFloorValueChanged(int newFloor)
        {
            currentFloor = newFloor;
            UpdateFloorDisplay();
            OnFloorChanged?.Invoke(newFloor);
            
            // 階層変更時のアイコンパルス
            if (floorIcon != null)
            {
                StartCoroutine(PulseIcon(floorIcon, positiveChangeColor));
            }
        }

        private void OnFloorExpanded(int newFloorIndex)
        {
            // 階層拡張時の特別なエフェクト
            if (floorIcon != null)
            {
                StartCoroutine(PulseIcon(floorIcon, Color.gold));
            }
        }

        private void OnTimeValueChanged(float newTime)
        {
            currentTime = newTime;
            // UpdateTimeDisplayはUpdateで呼ばれるので、ここでは何もしない
        }

        private void OnDayCompleted()
        {
            // 日完了時のエフェクト
            if (timeIcon != null)
            {
                StartCoroutine(PulseIcon(timeIcon, Color.cyan));
            }
        }

        private void OnLevelValueChanged(int newLevel)
        {
            currentLevel = newLevel;
            UpdateLevelDisplay();
            
            // レベルアップ時のエフェクト
            if (levelIcon != null)
            {
                StartCoroutine(PulseIcon(levelIcon, Color.gold));
            }
        }

        private void OnExperienceValueChanged(float newExperience, float newMaxExperience)
        {
            currentExperience = newExperience;
            maxExperience = newMaxExperience;
            UpdateExperienceDisplay();
        }

        private void OnCombatStateChanged(bool inCombat)
        {
            // 戦闘状態の変化は CheckCompactMode で処理される
        }

        /// <summary>
        /// 公開メソッド：手動でのリソース更新
        /// </summary>
        public void RefreshAllDisplays()
        {
            // 各マネージャーから最新の値を取得
            if (ResourceManager.Instance != null)
            {
                currentGold = ResourceManager.Instance.Gold;
                displayedGold = currentGold;
            }

            if (FloorSystem.Instance != null)
            {
                currentFloor = FloorSystem.Instance.CurrentViewFloor;
            }

            if (LevelDisplayManager.Instance != null)
            {
                // TODO: レベル情報の取得
            }

            UpdateAllDisplays();
        }

        /// <summary>
        /// 公開メソッド：コンパクトモードの手動設定
        /// </summary>
        public void SetCompactModeManual(bool compact)
        {
            autoCompactInCombat = false;
            SetCompactMode(compact);
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnGoldChanged -= OnGoldValueChanged;
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged -= OnFloorValueChanged;
                FloorSystem.Instance.OnFloorExpanded -= OnFloorExpanded;
            }

            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnTimeChanged -= OnTimeValueChanged;
                TimeManager.Instance.OnDayCompleted -= OnDayCompleted;
            }

            if (LevelDisplayManager.Instance != null)
            {
                LevelDisplayManager.Instance.OnLevelChanged -= OnLevelValueChanged;
                LevelDisplayManager.Instance.OnExperienceChanged -= OnExperienceValueChanged;
            }

            if (CombatVisualUI.Instance != null)
            {
                CombatVisualUI.Instance.OnCombatStateChanged -= OnCombatStateChanged;
            }
        }
    }
}