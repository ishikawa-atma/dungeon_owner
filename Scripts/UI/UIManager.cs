using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Core;
using DungeonOwner.Managers;
using System.Collections.Generic;

namespace DungeonOwner.UI
{
    /// <summary>
    /// 縦画面レイアウトと片手操作に最適化されたUIマネージャー
    /// 要件15.1, 15.2に対応
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Main UI Panels")]
        [SerializeField] private RectTransform mainUIPanel;
        [SerializeField] private RectTransform topPanel;
        [SerializeField] private RectTransform bottomPanel;
        [SerializeField] private RectTransform sidePanel;

        [Header("Portrait Layout Settings")]
        [SerializeField] private float topPanelHeight = 120f;
        [SerializeField] private float bottomPanelHeight = 200f;
        [SerializeField] private float sidePanelWidth = 80f;
        [SerializeField] private float safeAreaMargin = 20f;

        [Header("One-Hand Operation Settings")]
        [SerializeField] private bool enableOneHandMode = true;
        [SerializeField] private float thumbReachRadius = 300f;
        [SerializeField] private Vector2 thumbPosition = new Vector2(0, -200);

        [Header("UI Components")]
        [SerializeField] private MonsterPlacementUI monsterPlacementUI;
        [SerializeField] private SpeedControlUI speedControlUI;
        [SerializeField] private CombatVisualUI combatVisualUI;
        [SerializeField] private ResourceDisplayUI resourceDisplayUI;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject placementGhostPrefab;
        [SerializeField] private Material ghostMaterial;
        [SerializeField] private Color validPlacementColor = Color.green;
        [SerializeField] private Color invalidPlacementColor = Color.red;

        // 内部状態
        private Canvas mainCanvas;
        private CanvasScaler canvasScaler;
        private bool isPortraitMode = true;
        private Vector2 screenSize;
        private Dictionary<string, RectTransform> uiElements = new Dictionary<string, RectTransform>();

        // イベント
        public System.Action<bool> OnOrientationChanged;
        public System.Action<Vector2> OnScreenSizeChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeUI();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetupPortraitLayout();
            ConfigureOneHandOperation();
            SubscribeToEvents();
        }

        private void Update()
        {
            CheckScreenOrientation();
            UpdateUILayout();
        }

        /// <summary>
        /// UI初期化
        /// </summary>
        private void InitializeUI()
        {
            mainCanvas = GetComponent<Canvas>();
            if (mainCanvas == null)
            {
                mainCanvas = gameObject.AddComponent<Canvas>();
            }

            canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler == null)
            {
                canvasScaler = gameObject.AddComponent<CanvasScaler>();
            }

            // Canvas設定
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;

            // CanvasScaler設定（縦画面最適化）
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920); // 縦画面基準
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // バランス調整

            screenSize = new Vector2(Screen.width, Screen.height);
        }

        /// <summary>
        /// 縦画面レイアウトの設定
        /// 要件15.1: 縦画面レイアウト
        /// </summary>
        private void SetupPortraitLayout()
        {
            if (mainUIPanel == null) return;

            // メインパネルをフルスクリーンに設定
            SetFullScreen(mainUIPanel);

            // トップパネル設定（リソース表示など）
            if (topPanel != null)
            {
                SetAnchor(topPanel, AnchorPresets.TopStretch);
                topPanel.sizeDelta = new Vector2(0, topPanelHeight);
                topPanel.anchoredPosition = Vector2.zero;
            }

            // ボトムパネル設定（主要操作ボタン）
            if (bottomPanel != null)
            {
                SetAnchor(bottomPanel, AnchorPresets.BottomStretch);
                bottomPanel.sizeDelta = new Vector2(0, bottomPanelHeight);
                bottomPanel.anchoredPosition = Vector2.zero;
            }

            // サイドパネル設定（補助機能）
            if (sidePanel != null)
            {
                SetAnchor(sidePanel, AnchorPresets.MiddleRight);
                sidePanel.sizeDelta = new Vector2(sidePanelWidth, 0);
                sidePanel.anchoredPosition = new Vector2(-safeAreaMargin, 0);
            }

            Debug.Log("Portrait layout configured");
        }

        /// <summary>
        /// 片手操作の設定
        /// 要件15.2: 片手操作UI
        /// </summary>
        private void ConfigureOneHandOperation()
        {
            if (!enableOneHandMode) return;

            // 重要なボタンを親指の届く範囲に配置
            ConfigureThumbReachableArea();

            // ボタンサイズを片手操作に適したサイズに調整
            ConfigureButtonSizes();

            Debug.Log("One-hand operation configured");
        }

        /// <summary>
        /// 親指の届く範囲の設定
        /// </summary>
        private void ConfigureThumbReachableArea()
        {
            // 画面下部を基準とした親指の届く範囲を計算
            Vector2 thumbCenter = new Vector2(0, thumbPosition.y);
            
            // 重要なUIエレメントをこの範囲内に配置
            if (bottomPanel != null)
            {
                // ボトムパネル内のボタン配置を調整
                ConfigureBottomPanelLayout(thumbCenter);
            }
        }

        /// <summary>
        /// ボトムパネルのレイアウト設定
        /// </summary>
        private void ConfigureBottomPanelLayout(Vector2 thumbCenter)
        {
            // モンスター配置ボタンを親指の届きやすい位置に配置
            if (monsterPlacementUI != null)
            {
                RectTransform placementButton = monsterPlacementUI.GetComponent<RectTransform>();
                if (placementButton != null)
                {
                    SetAnchor(placementButton, AnchorPresets.BottomCenter);
                    placementButton.anchoredPosition = new Vector2(0, 100);
                }
            }

            // 速度制御ボタンを右下に配置
            if (speedControlUI != null)
            {
                RectTransform speedButton = speedControlUI.GetComponent<RectTransform>();
                if (speedButton != null)
                {
                    SetAnchor(speedButton, AnchorPresets.BottomRight);
                    speedButton.anchoredPosition = new Vector2(-50, 50);
                }
            }
        }

        /// <summary>
        /// ボタンサイズの調整
        /// </summary>
        private void ConfigureButtonSizes()
        {
            float minButtonSize = 80f; // 最小タッチターゲットサイズ
            
            // 全てのボタンコンポーネントを取得して調整
            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    Vector2 size = buttonRect.sizeDelta;
                    size.x = Mathf.Max(size.x, minButtonSize);
                    size.y = Mathf.Max(size.y, minButtonSize);
                    buttonRect.sizeDelta = size;
                }
            }
        }

        /// <summary>
        /// 画面向きの確認
        /// </summary>
        private void CheckScreenOrientation()
        {
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            
            if (currentScreenSize != screenSize)
            {
                screenSize = currentScreenSize;
                bool newIsPortrait = Screen.height > Screen.width;
                
                if (newIsPortrait != isPortraitMode)
                {
                    isPortraitMode = newIsPortrait;
                    OnOrientationChanged?.Invoke(isPortraitMode);
                    
                    if (isPortraitMode)
                    {
                        SetupPortraitLayout();
                    }
                    else
                    {
                        SetupLandscapeLayout();
                    }
                }
                
                OnScreenSizeChanged?.Invoke(screenSize);
            }
        }

        /// <summary>
        /// 横画面レイアウトの設定（フォールバック）
        /// </summary>
        private void SetupLandscapeLayout()
        {
            Debug.LogWarning("Landscape mode detected. This game is optimized for portrait mode.");
            
            // 横画面でも最低限動作するようにレイアウト調整
            if (topPanel != null)
            {
                topPanel.sizeDelta = new Vector2(0, topPanelHeight * 0.7f);
            }
            
            if (bottomPanel != null)
            {
                bottomPanel.sizeDelta = new Vector2(0, bottomPanelHeight * 0.7f);
            }
        }

        /// <summary>
        /// UIレイアウトの更新
        /// </summary>
        private void UpdateUILayout()
        {
            // セーフエリアの考慮
            ApplySafeArea();
            
            // 動的レイアウト調整
            AdjustDynamicElements();
        }

        /// <summary>
        /// セーフエリアの適用
        /// </summary>
        private void ApplySafeArea()
        {
            Rect safeArea = Screen.safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            
            if (mainUIPanel != null)
            {
                mainUIPanel.anchorMin = anchorMin;
                mainUIPanel.anchorMax = anchorMax;
            }
        }

        /// <summary>
        /// 動的要素の調整
        /// </summary>
        private void AdjustDynamicElements()
        {
            // 戦闘中は重要でないUIを縮小
            if (combatVisualUI != null && combatVisualUI.IsInCombat())
            {
                MinimizeNonEssentialUI();
            }
            else
            {
                RestoreNormalUI();
            }
        }

        /// <summary>
        /// 非必須UIの最小化
        /// </summary>
        private void MinimizeNonEssentialUI()
        {
            if (sidePanel != null)
            {
                sidePanel.localScale = Vector3.one * 0.8f;
            }
        }

        /// <summary>
        /// 通常UIの復元
        /// </summary>
        private void RestoreNormalUI()
        {
            if (sidePanel != null)
            {
                sidePanel.localScale = Vector3.one;
            }
        }

        /// <summary>
        /// イベント購読
        /// </summary>
        private void SubscribeToEvents()
        {
            // GameManagerのイベント
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }
        }

        /// <summary>
        /// ゲーム状態変更時の処理
        /// </summary>
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    ShowGameplayUI();
                    break;
                case GameState.Paused:
                    ShowPauseOverlay();
                    break;
                case GameState.GameOver:
                    ShowGameOverUI();
                    break;
            }
        }

        /// <summary>
        /// ゲームプレイUIの表示
        /// </summary>
        private void ShowGameplayUI()
        {
            if (mainUIPanel != null)
            {
                mainUIPanel.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// ポーズオーバーレイの表示
        /// </summary>
        private void ShowPauseOverlay()
        {
            // ポーズ用のオーバーレイ表示
            // TODO: ポーズUIの実装
        }

        /// <summary>
        /// ゲームオーバーUIの表示
        /// </summary>
        private void ShowGameOverUI()
        {
            // ゲームオーバー用のUI表示
            // TODO: ゲームオーバーUIの実装
        }

        /// <summary>
        /// アンカープリセットの設定
        /// </summary>
        private void SetAnchor(RectTransform rectTransform, AnchorPresets preset)
        {
            switch (preset)
            {
                case AnchorPresets.TopStretch:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    break;
                case AnchorPresets.BottomStretch:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    break;
                case AnchorPresets.MiddleRight:
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    break;
                case AnchorPresets.BottomCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 0);
                    rectTransform.anchorMax = new Vector2(0.5f, 0);
                    break;
                case AnchorPresets.BottomRight:
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    break;
            }
        }

        /// <summary>
        /// フルスクリーン設定
        /// </summary>
        private void SetFullScreen(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }

        /// <summary>
        /// 公開メソッド：UIエレメントの登録
        /// </summary>
        public void RegisterUIElement(string key, RectTransform element)
        {
            uiElements[key] = element;
        }

        /// <summary>
        /// 公開メソッド：UIエレメントの取得
        /// </summary>
        public RectTransform GetUIElement(string key)
        {
            return uiElements.ContainsKey(key) ? uiElements[key] : null;
        }

        /// <summary>
        /// 公開メソッド：片手操作モードの切り替え
        /// </summary>
        public void SetOneHandMode(bool enabled)
        {
            enableOneHandMode = enabled;
            if (enabled)
            {
                ConfigureOneHandOperation();
            }
        }
    }

    /// <summary>
    /// アンカープリセット列挙型
    /// </summary>
    public enum AnchorPresets
    {
        TopStretch,
        BottomStretch,
        MiddleRight,
        BottomCenter,
        BottomRight
    }
}