using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Core;

namespace DungeonOwner.UI
{
    /// <summary>
    /// ゲーム速度制御UI
    /// 要件14.1, 14.2, 14.5に対応
    /// </summary>
    public class SpeedControlUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button speedButton;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private Image speedIcon;
        
        [Header("Speed Display Settings")]
        [SerializeField] private Color normalSpeedColor = Color.white;
        [SerializeField] private Color fastSpeedColor = Color.yellow;
        [SerializeField] private Color veryFastSpeedColor = Color.red;
        
        [Header("Speed Icons")]
        [SerializeField] private Sprite normalSpeedSprite;
        [SerializeField] private Sprite fastSpeedSprite;
        [SerializeField] private Sprite veryFastSpeedSprite;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void InitializeUI()
        {
            if (speedButton != null)
            {
                speedButton.onClick.AddListener(OnSpeedButtonClicked);
            }
            
            // 初期表示を設定
            UpdateSpeedDisplay(1.0f);
        }

        private void SubscribeToEvents()
        {
            // TimeManagerの速度変更イベントに登録
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnSpeedChanged += UpdateSpeedDisplay;
            }
            
            // GameManagerの速度変更イベントにも登録
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameSpeedChanged += UpdateSpeedDisplay;
            }
        }

        private void OnSpeedButtonClicked()
        {
            // TimeManagerの速度サイクル機能を使用
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.CycleGameSpeed();
            }
        }

        /// <summary>
        /// 速度表示を更新
        /// 要件14.5: 速度表示UIとコントロール
        /// </summary>
        /// <param name="speedMultiplier">現在の速度倍率</param>
        private void UpdateSpeedDisplay(float speedMultiplier)
        {
            // テキスト表示を更新
            if (speedText != null)
            {
                speedText.text = $"{speedMultiplier:F1}x";
                
                // 速度に応じて色を変更
                speedText.color = GetSpeedColor(speedMultiplier);
            }
            
            // アイコン表示を更新
            if (speedIcon != null)
            {
                speedIcon.sprite = GetSpeedSprite(speedMultiplier);
                speedIcon.color = GetSpeedColor(speedMultiplier);
            }
            
            // ボタンの視覚的フィードバック
            UpdateButtonVisuals(speedMultiplier);
        }

        /// <summary>
        /// 速度に応じた色を取得
        /// </summary>
        private Color GetSpeedColor(float speedMultiplier)
        {
            if (speedMultiplier <= 1.0f)
                return normalSpeedColor;
            else if (speedMultiplier <= 1.5f)
                return fastSpeedColor;
            else
                return veryFastSpeedColor;
        }

        /// <summary>
        /// 速度に応じたスプライトを取得
        /// </summary>
        private Sprite GetSpeedSprite(float speedMultiplier)
        {
            if (speedMultiplier <= 1.0f)
                return normalSpeedSprite;
            else if (speedMultiplier <= 1.5f)
                return fastSpeedSprite;
            else
                return veryFastSpeedSprite;
        }

        /// <summary>
        /// ボタンの視覚的表現を更新
        /// </summary>
        private void UpdateButtonVisuals(float speedMultiplier)
        {
            if (speedButton != null)
            {
                // ボタンの色を速度に応じて変更
                ColorBlock colors = speedButton.colors;
                colors.normalColor = GetSpeedColor(speedMultiplier);
                speedButton.colors = colors;
                
                // ボタンのスケールアニメーション（オプション）
                StartCoroutine(ButtonPulseAnimation());
            }
        }

        /// <summary>
        /// ボタンのパルスアニメーション
        /// </summary>
        private System.Collections.IEnumerator ButtonPulseAnimation()
        {
            Vector3 originalScale = speedButton.transform.localScale;
            Vector3 targetScale = originalScale * 1.1f;
            
            float duration = 0.1f;
            float elapsed = 0f;
            
            // 拡大
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                speedButton.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }
            
            elapsed = 0f;
            
            // 縮小
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                speedButton.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            
            speedButton.transform.localScale = originalScale;
        }

        /// <summary>
        /// 速度制御の有効/無効を切り替え
        /// </summary>
        public void SetSpeedControlEnabled(bool enabled)
        {
            if (speedButton != null)
            {
                speedButton.interactable = enabled;
            }
        }

        /// <summary>
        /// 特定の速度に直接設定
        /// </summary>
        public void SetSpeed(float speedMultiplier)
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.SetGameSpeed(speedMultiplier);
            }
        }

        private void OnDestroy()
        {
            // イベント購読を解除
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnSpeedChanged -= UpdateSpeedDisplay;
            }
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameSpeedChanged -= UpdateSpeedDisplay;
            }
            
            if (speedButton != null)
            {
                speedButton.onClick.RemoveListener(OnSpeedButtonClicked);
            }
        }
    }
}