using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Core;
using System.Collections;

namespace DungeonOwner.UI
{
    /// <summary>
    /// チュートリアルUI管理
    /// 要件20.1-20.4に対応したUI表示とインタラクション
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private GameObject tutorialOverlay;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Visual Effects")]
        [SerializeField] private GameObject highlightPrefab;
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private float highlightPulseSpeed = 2f;
        [SerializeField] private float arrowAnimationSpeed = 1f;

        [Header("Animation Settings")]
        [SerializeField] private float panelFadeInDuration = 0.5f;
        [SerializeField] private float panelFadeOutDuration = 0.3f;
        [SerializeField] private AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        // 内部状態
        private CanvasGroup tutorialCanvasGroup;
        private GameObject currentHighlight;
        private GameObject currentArrow;
        private Coroutine fadeCoroutine;
        private Coroutine highlightCoroutine;

        // イベント
        public System.Action OnNextClicked;
        public System.Action OnSkipClicked;

        private void Awake()
        {
            InitializeUI();
            SetupEventListeners();
        }

        private void Start()
        {
            // 初期状態では非表示
            SetTutorialVisible(false, false);
        }

        /// <summary>
        /// UI初期化
        /// </summary>
        private void InitializeUI()
        {
            // CanvasGroupの設定
            if (tutorialOverlay != null)
            {
                tutorialCanvasGroup = tutorialOverlay.GetComponent<CanvasGroup>();
                if (tutorialCanvasGroup == null)
                {
                    tutorialCanvasGroup = tutorialOverlay.AddComponent<CanvasGroup>();
                }
            }

            // プログレスバーの初期化
            if (progressBar != null)
            {
                progressBar.fillAmount = 0f;
            }
        }

        /// <summary>
        /// イベントリスナー設定
        /// </summary>
        private void SetupEventListeners()
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(() => OnNextClicked?.Invoke());
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(() => OnSkipClicked?.Invoke());
            }
        }

        /// <summary>
        /// チュートリアル表示/非表示
        /// </summary>
        public void SetTutorialVisible(bool visible, bool animated = true)
        {
            if (tutorialOverlay == null) return;

            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (animated)
            {
                fadeCoroutine = StartCoroutine(FadeTutorial(visible));
            }
            else
            {
                tutorialOverlay.SetActive(visible);
                if (tutorialCanvasGroup != null)
                {
                    tutorialCanvasGroup.alpha = visible ? 1f : 0f;
                }
            }
        }

        /// <summary>
        /// チュートリアルフェードアニメーション
        /// </summary>
        private IEnumerator FadeTutorial(bool fadeIn)
        {
            if (fadeIn)
            {
                tutorialOverlay.SetActive(true);
            }

            float duration = fadeIn ? panelFadeInDuration : panelFadeOutDuration;
            AnimationCurve curve = fadeIn ? fadeInCurve : fadeOutCurve;
            float startAlpha = tutorialCanvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                float curveValue = curve.Evaluate(progress);
                
                tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
                yield return null;
            }

            tutorialCanvasGroup.alpha = targetAlpha;

            if (!fadeIn)
            {
                tutorialOverlay.SetActive(false);
            }

            fadeCoroutine = null;
        }

        /// <summary>
        /// ステップ情報更新
        /// </summary>
        public void UpdateStepInfo(string title, string instruction, int currentStep, int totalSteps, bool showNext, bool showSkip)
        {
            // タイトル更新
            if (titleText != null)
            {
                titleText.text = title;
            }

            // 説明文更新
            if (instructionText != null)
            {
                instructionText.text = instruction;
            }

            // プログレス更新
            UpdateProgress(currentStep, totalSteps);

            // ボタン表示制御
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(showNext);
            }

            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(showSkip);
            }
        }

        /// <summary>
        /// プログレス更新
        /// </summary>
        private void UpdateProgress(int currentStep, int totalSteps)
        {
            float progress = totalSteps > 0 ? (float)currentStep / totalSteps : 0f;

            if (progressBar != null)
            {
                progressBar.fillAmount = progress;
            }

            if (progressText != null)
            {
                progressText.text = $"{currentStep} / {totalSteps}";
            }
        }

        /// <summary>
        /// UI要素のハイライト表示
        /// </summary>
        public void HighlightUIElement(RectTransform target, bool showArrow = false)
        {
            // 既存のハイライトを削除
            ClearHighlight();

            if (target == null || highlightPrefab == null) return;

            // ハイライト作成
            currentHighlight = Instantiate(highlightPrefab, transform);
            RectTransform highlightRect = currentHighlight.GetComponent<RectTransform>();

            if (highlightRect != null)
            {
                // ワールド座標での位置合わせ
                Vector3 worldPos = target.TransformPoint(Vector3.zero);
                highlightRect.position = worldPos;
                highlightRect.sizeDelta = target.sizeDelta * 1.3f;
            }

            // ハイライトアニメーション開始
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }
            highlightCoroutine = StartCoroutine(AnimateHighlight());

            // 矢印表示
            if (showArrow && arrowPrefab != null)
            {
                ShowArrow(target);
            }
        }

        /// <summary>
        /// ハイライトアニメーション
        /// </summary>
        private IEnumerator AnimateHighlight()
        {
            if (currentHighlight == null) yield break;

            Image highlightImage = currentHighlight.GetComponent<Image>();
            if (highlightImage == null) yield break;

            Vector3 originalScale = currentHighlight.transform.localScale;
            Color originalColor = highlightImage.color;

            while (currentHighlight != null)
            {
                float time = Time.unscaledTime * highlightPulseSpeed;
                
                // スケールアニメーション
                float scaleMultiplier = 1f + 0.1f * Mathf.Sin(time);
                currentHighlight.transform.localScale = originalScale * scaleMultiplier;

                // カラーアニメーション
                float alpha = 0.5f + 0.3f * Mathf.Sin(time * 1.5f);
                Color newColor = originalColor;
                newColor.a = alpha;
                highlightImage.color = newColor;

                yield return null;
            }
        }

        /// <summary>
        /// 矢印表示
        /// </summary>
        private void ShowArrow(RectTransform target)
        {
            if (arrowPrefab == null || target == null) return;

            currentArrow = Instantiate(arrowPrefab, transform);
            RectTransform arrowRect = currentArrow.GetComponent<RectTransform>();

            if (arrowRect != null)
            {
                // 矢印を対象の上に配置
                Vector3 worldPos = target.TransformPoint(Vector3.zero);
                worldPos.y += target.sizeDelta.y * 0.8f;
                arrowRect.position = worldPos;

                // 矢印アニメーション開始
                StartCoroutine(AnimateArrow());
            }
        }

        /// <summary>
        /// 矢印アニメーション
        /// </summary>
        private IEnumerator AnimateArrow()
        {
            if (currentArrow == null) yield break;

            Vector3 originalPos = currentArrow.transform.position;

            while (currentArrow != null)
            {
                float time = Time.unscaledTime * arrowAnimationSpeed;
                float offset = 10f * Mathf.Sin(time);
                
                Vector3 newPos = originalPos;
                newPos.y += offset;
                currentArrow.transform.position = newPos;

                yield return null;
            }
        }

        /// <summary>
        /// ハイライトクリア
        /// </summary>
        public void ClearHighlight()
        {
            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
                highlightCoroutine = null;
            }

            if (currentHighlight != null)
            {
                Destroy(currentHighlight);
                currentHighlight = null;
            }

            if (currentArrow != null)
            {
                Destroy(currentArrow);
                currentArrow = null;
            }
        }

        /// <summary>
        /// 画面全体のマスク表示（特定エリア以外を暗くする）
        /// </summary>
        public void ShowMask(RectTransform excludeArea = null)
        {
            // マスク機能の実装
            // 特定のUI要素以外を暗くして注目を集める
            if (excludeArea != null)
            {
                // TODO: マスク実装
                Debug.Log($"Showing mask excluding area: {excludeArea.name}");
            }
        }

        /// <summary>
        /// マスク非表示
        /// </summary>
        public void HideMask()
        {
            // マスク非表示の実装
            Debug.Log("Hiding mask");
        }

        /// <summary>
        /// チュートリアルパネルの位置調整
        /// </summary>
        public void AdjustPanelPosition(Vector2 targetPosition)
        {
            if (tutorialPanel == null) return;

            RectTransform panelRect = tutorialPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // 画面内に収まるように調整
                Vector2 adjustedPosition = ClampToScreen(targetPosition, panelRect.sizeDelta);
                panelRect.anchoredPosition = adjustedPosition;
            }
        }

        /// <summary>
        /// 画面内に位置をクランプ
        /// </summary>
        private Vector2 ClampToScreen(Vector2 position, Vector2 size)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return position;

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;

            float halfWidth = size.x * 0.5f;
            float halfHeight = size.y * 0.5f;

            position.x = Mathf.Clamp(position.x, -canvasSize.x * 0.5f + halfWidth, canvasSize.x * 0.5f - halfWidth);
            position.y = Mathf.Clamp(position.y, -canvasSize.y * 0.5f + halfHeight, canvasSize.y * 0.5f - halfHeight);

            return position;
        }

        /// <summary>
        /// アニメーション付きテキスト表示
        /// </summary>
        public void ShowTextAnimated(string text, float duration = 1f)
        {
            if (instructionText != null)
            {
                StartCoroutine(AnimateText(text, duration));
            }
        }

        /// <summary>
        /// テキストアニメーション
        /// </summary>
        private IEnumerator AnimateText(string targetText, float duration)
        {
            if (instructionText == null) yield break;

            instructionText.text = "";
            
            float charDelay = duration / targetText.Length;
            
            for (int i = 0; i <= targetText.Length; i++)
            {
                instructionText.text = targetText.Substring(0, i);
                yield return new WaitForSecondsRealtime(charDelay);
            }
        }

        /// <summary>
        /// ボタンの有効/無効切り替え
        /// </summary>
        public void SetButtonsInteractable(bool interactable)
        {
            if (nextButton != null)
            {
                nextButton.interactable = interactable;
            }

            if (skipButton != null)
            {
                skipButton.interactable = interactable;
            }
        }

        /// <summary>
        /// チュートリアル完了エフェクト
        /// </summary>
        public void ShowCompletionEffect()
        {
            StartCoroutine(PlayCompletionEffect());
        }

        /// <summary>
        /// 完了エフェクトの再生
        /// </summary>
        private IEnumerator PlayCompletionEffect()
        {
            // 完了エフェクトの実装
            if (tutorialPanel != null)
            {
                // パネルを少し拡大してから縮小
                Vector3 originalScale = tutorialPanel.transform.localScale;
                
                // 拡大
                float expandDuration = 0.2f;
                float elapsed = 0f;
                while (elapsed < expandDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float scale = Mathf.Lerp(1f, 1.1f, elapsed / expandDuration);
                    tutorialPanel.transform.localScale = originalScale * scale;
                    yield return null;
                }

                // 縮小
                float shrinkDuration = 0.3f;
                elapsed = 0f;
                while (elapsed < shrinkDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float scale = Mathf.Lerp(1.1f, 1f, elapsed / shrinkDuration);
                    tutorialPanel.transform.localScale = originalScale * scale;
                    yield return null;
                }

                tutorialPanel.transform.localScale = originalScale;
            }

            Debug.Log("Tutorial completion effect played");
        }

        private void OnDestroy()
        {
            // コルーチン停止
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            if (highlightCoroutine != null)
            {
                StopCoroutine(highlightCoroutine);
            }

            // ハイライト削除
            ClearHighlight();
        }
    }
}