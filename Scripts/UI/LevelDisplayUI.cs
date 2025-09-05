using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Interfaces;

namespace DungeonOwner.UI
{
    /// <summary>
    /// キャラクターのレベルを表示するUIコンポーネント
    /// モンスターと侵入者の上にレベルを表示する
    /// </summary>
    public class LevelDisplayUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Canvas levelCanvas;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image backgroundImage;

        [Header("Display Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 1.2f, 0);
        [SerializeField] private float fadeDistance = 8f; // カメラからの距離でフェード
        [SerializeField] private bool alwaysVisible = false;

        [Header("Style Settings")]
        [SerializeField] private Color monsterLevelColor = Color.green;
        [SerializeField] private Color invaderLevelColor = Color.red;
        [SerializeField] private Color playerCharacterColor = Color.blue;
        [SerializeField] private Color backgroundColorNormal = new Color(0, 0, 0, 0.7f);
        [SerializeField] private Color backgroundColorHighLevel = new Color(1, 0.5f, 0, 0.8f);

        private Transform targetTransform;
        private ICharacterBase targetCharacter;
        private Camera mainCamera;
        private CanvasGroup canvasGroup;
        private int currentLevel = 1;
        private bool isInitialized = false;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        private void Update()
        {
            if (isInitialized && targetTransform != null)
            {
                UpdatePosition();
                UpdateVisibility();
                UpdateLevel();
            }
        }

        /// <summary>
        /// コンポーネントの初期化
        /// </summary>
        private void InitializeComponents()
        {
            // Canvas設定
            if (levelCanvas == null)
            {
                GameObject canvasObj = new GameObject("LevelCanvas");
                canvasObj.transform.SetParent(transform);
                levelCanvas = canvasObj.AddComponent<Canvas>();
                levelCanvas.renderMode = RenderMode.WorldSpace;
                levelCanvas.worldCamera = Camera.main;
                levelCanvas.sortingOrder = 100;

                // CanvasScaler追加
                var scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                scaler.scaleFactor = 1f;
            }

            // CanvasGroup追加
            canvasGroup = levelCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = levelCanvas.gameObject.AddComponent<CanvasGroup>();
            }

            // Background Image設定
            if (backgroundImage == null)
            {
                GameObject bgObj = new GameObject("Background");
                bgObj.transform.SetParent(levelCanvas.transform);
                backgroundImage = bgObj.AddComponent<Image>();
                backgroundImage.color = backgroundColorNormal;
                
                // RectTransform設定
                var bgRect = backgroundImage.rectTransform;
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
            }

            // Level Text設定
            if (levelText == null)
            {
                GameObject textObj = new GameObject("LevelText");
                textObj.transform.SetParent(levelCanvas.transform);
                levelText = textObj.AddComponent<TextMeshProUGUI>();
                levelText.text = "Lv.1";
                levelText.fontSize = 14f;
                levelText.color = Color.white;
                levelText.alignment = TextAlignmentOptions.Center;
                levelText.fontStyle = FontStyles.Bold;

                // RectTransform設定
                var textRect = levelText.rectTransform;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(2, 2);
                textRect.offsetMax = new Vector2(-2, -2);
            }

            // Canvas サイズ設定
            var canvasRect = levelCanvas.GetComponent<RectTransform>();
            canvasRect.sizeDelta = new Vector2(60, 25);

            isInitialized = true;
        }

        /// <summary>
        /// 表示対象を設定
        /// </summary>
        public void SetTarget(Transform target, ICharacterBase character)
        {
            targetTransform = target;
            targetCharacter = character;

            if (character != null)
            {
                UpdateLevelDisplay();
                UpdateColorScheme();
            }
        }

        /// <summary>
        /// 位置更新
        /// </summary>
        private void UpdatePosition()
        {
            if (targetTransform != null)
            {
                Vector3 worldPosition = targetTransform.position + offset;
                transform.position = worldPosition;

                // カメラに向ける（ビルボード効果）
                if (mainCamera != null)
                {
                    Vector3 lookDirection = mainCamera.transform.position - transform.position;
                    lookDirection.z = 0; // 2Dゲームなので Z軸回転のみ
                    if (lookDirection != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.up);
                    }
                }
            }
        }

        /// <summary>
        /// 可視性更新
        /// </summary>
        private void UpdateVisibility()
        {
            if (!alwaysVisible && mainCamera != null)
            {
                float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
                float alpha = Mathf.Clamp01(1f - (distance / fadeDistance));
                
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }
            }
            else if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// レベル更新チェック
        /// </summary>
        private void UpdateLevel()
        {
            if (targetCharacter != null)
            {
                int newLevel = GetCharacterLevel();
                if (newLevel != currentLevel)
                {
                    currentLevel = newLevel;
                    UpdateLevelDisplay();
                }
            }
        }

        /// <summary>
        /// レベル表示更新
        /// </summary>
        private void UpdateLevelDisplay()
        {
            if (levelText != null)
            {
                levelText.text = $"Lv.{currentLevel}";
            }

            // 高レベル時の背景色変更
            if (backgroundImage != null)
            {
                if (currentLevel >= 10)
                {
                    backgroundImage.color = backgroundColorHighLevel;
                }
                else
                {
                    backgroundImage.color = backgroundColorNormal;
                }
            }
        }

        /// <summary>
        /// 色スキーム更新
        /// </summary>
        private void UpdateColorScheme()
        {
            if (levelText == null) return;

            if (targetCharacter is IMonster)
            {
                levelText.color = monsterLevelColor;
            }
            else if (targetCharacter is IInvader)
            {
                levelText.color = invaderLevelColor;
            }
            else
            {
                levelText.color = playerCharacterColor;
            }
        }

        /// <summary>
        /// キャラクターレベル取得
        /// </summary>
        private int GetCharacterLevel()
        {
            if (targetCharacter is IMonster monster)
            {
                return monster.Level;
            }
            else if (targetCharacter is IInvader invader)
            {
                return invader.Level;
            }

            return 1;
        }

        /// <summary>
        /// 表示設定
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (levelCanvas != null)
            {
                levelCanvas.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// オフセット設定
        /// </summary>
        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
        }

        /// <summary>
        /// 常時表示設定
        /// </summary>
        public void SetAlwaysVisible(bool always)
        {
            alwaysVisible = always;
        }

        /// <summary>
        /// 色設定
        /// </summary>
        public void SetColors(Color textColor, Color backgroundColor)
        {
            if (levelText != null)
            {
                levelText.color = textColor;
            }
            if (backgroundImage != null)
            {
                backgroundImage.color = backgroundColor;
            }
        }

        private void OnDestroy()
        {
            // クリーンアップ
            targetTransform = null;
            targetCharacter = null;
        }
    }
}