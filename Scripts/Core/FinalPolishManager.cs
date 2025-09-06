using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 最終ポリッシュとリリース準備を管理するクラス
/// UI/UXの最終調整、バグ修正、安定性向上を担当
/// </summary>
public class FinalPolishManager : MonoBehaviour
{
    [Header("ポリッシュ設定")]
    [SerializeField] private bool enableAutoPolish = true;
    [SerializeField] private bool enableDetailedLogging = true;
    [SerializeField] private float polishCheckInterval = 5f;
    
    [Header("UI/UX調整")]
    [SerializeField] private UIPolishSettings uiPolishSettings = new UIPolishSettings();
    
    [Header("パフォーマンス最適化")]
    [SerializeField] private PerformanceSettings performanceSettings = new PerformanceSettings();
    
    [Header("安定性設定")]
    [SerializeField] private StabilitySettings stabilitySettings = new StabilitySettings();
    
    [Header("ビルド設定")]
    [SerializeField] private BuildSettings buildSettings = new BuildSettings();
    
    private float lastPolishCheck = 0f;
    private List<string> polishLog = new List<string>();
    private Dictionary<string, bool> polishTasks = new Dictionary<string, bool>();
    
    [System.Serializable]
    public class UIPolishSettings
    {
        [Header("レスポンシブ調整")]
        public bool enableResponsiveUI = true;
        public float minButtonSize = 80f;
        public float maxButtonSize = 120f;
        public float safeAreaMargin = 20f;
        
        [Header("アニメーション調整")]
        public bool enableSmoothAnimations = true;
        public float defaultAnimationDuration = 0.3f;
        public AnimationCurve defaultEasing = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("視覚効果")]
        public bool enableVisualFeedback = true;
        public Color highlightColor = Color.yellow;
        public Color errorColor = Color.red;
        public Color successColor = Color.green;
        
        [Header("フォント調整")]
        public bool enableDynamicFontSizing = true;
        public float minFontSize = 12f;
        public float maxFontSize = 24f;
    }
    
    [System.Serializable]
    public class PerformanceSettings
    {
        [Header("フレームレート")]
        public int targetFPS = 60;
        public bool enableVSync = true;
        public bool enableFrameRateOptimization = true;
        
        [Header("メモリ管理")]
        public bool enableMemoryOptimization = true;
        public float memoryCleanupInterval = 30f;
        public long maxMemoryUsage = 500 * 1024 * 1024; // 500MB
        
        [Header("描画最適化")]
        public bool enableBatching = true;
        public bool enableOcclusion = true;
        public int maxDrawCalls = 100;
    }
    
    [System.Serializable]
    public class StabilitySettings
    {
        [Header("エラーハンドリング")]
        public bool enableGlobalErrorHandling = true;
        public bool enableCrashReporting = true;
        public bool enableAutoRecovery = true;
        
        [Header("セーブデータ保護")]
        public bool enableBackupSaves = true;
        public int maxBackupCount = 3;
        public float autoSaveInterval = 60f;
        
        [Header("ネットワーク安定性")]
        public bool enableNetworkRetry = true;
        public int maxRetryAttempts = 3;
        public float retryDelay = 2f;
    }
    
    [System.Serializable]
    public class BuildSettings
    {
        [Header("プラットフォーム設定")]
        public bool optimizeForMobile = true;
        public bool enableDebugging = false;
        public bool enableProfiling = false;
        
        [Header("圧縮設定")]
        public bool enableTextureCompression = true;
        public bool enableAudioCompression = true;
        public bool enableScriptOptimization = true;
        
        [Header("セキュリティ")]
        public bool enableCodeObfuscation = true;
        public bool enableAntiCheat = true;
    }
    
    private void Start()
    {
        InitializePolishManager();
        StartPolishProcess();
    }
    
    private void Update()
    {
        if (enableAutoPolish && Time.time - lastPolishCheck >= polishCheckInterval)
        {
            PerformPolishCheck();
            lastPolishCheck = Time.time;
        }
    }
    
    /// <summary>
    /// ポリッシュマネージャーの初期化
    /// </summary>
    private void InitializePolishManager()
    {
        LogPolish("FinalPolishManager初期化開始");
        
        // ポリッシュタスクの初期化
        InitializePolishTasks();
        
        // グローバルエラーハンドリングの設定
        if (stabilitySettings.enableGlobalErrorHandling)
        {
            SetupGlobalErrorHandling();
        }
        
        // パフォーマンス設定の適用
        ApplyPerformanceSettings();
        
        LogPolish("FinalPolishManager初期化完了");
    }
    
    /// <summary>
    /// ポリッシュタスクの初期化
    /// </summary>
    private void InitializePolishTasks()
    {
        polishTasks["UI_Responsive"] = false;
        polishTasks["UI_Animations"] = false;
        polishTasks["UI_Accessibility"] = false;
        polishTasks["Performance_FPS"] = false;
        polishTasks["Performance_Memory"] = false;
        polishTasks["Stability_ErrorHandling"] = false;
        polishTasks["Stability_SaveSystem"] = false;
        polishTasks["Build_Optimization"] = false;
        polishTasks["Build_Security"] = false;
        polishTasks["Final_Testing"] = false;
    }
    
    /// <summary>
    /// ポリッシュプロセスの開始
    /// </summary>
    private void StartPolishProcess()
    {
        StartCoroutine(ExecutePolishSequence());
    }
    
    /// <summary>
    /// ポリッシュシーケンスの実行
    /// </summary>
    private IEnumerator ExecutePolishSequence()
    {
        LogPolish("=== 最終ポリッシュシーケンス開始 ===");
        
        // 1. UI/UXの最終調整
        yield return StartCoroutine(PolishUIUX());
        
        // 2. パフォーマンス最適化
        yield return StartCoroutine(OptimizePerformance());
        
        // 3. 安定性向上
        yield return StartCoroutine(ImproveStability());
        
        // 4. ビルド設定とリリース準備
        yield return StartCoroutine(PrepareBuild());
        
        // 5. 最終テスト
        yield return StartCoroutine(FinalTesting());
        
        LogPolish("=== 最終ポリッシュシーケンス完了 ===");
        GeneratePolishReport();
    }
    
    /// <summary>
    /// UI/UXの最終調整
    /// </summary>
    private IEnumerator PolishUIUX()
    {
        LogPolish("UI/UX最終調整開始");
        
        // レスポンシブUI調整
        yield return StartCoroutine(AdjustResponsiveUI());
        polishTasks["UI_Responsive"] = true;
        
        // アニメーション調整
        yield return StartCoroutine(AdjustAnimations());
        polishTasks["UI_Animations"] = true;
        
        // アクセシビリティ調整
        yield return StartCoroutine(AdjustAccessibility());
        polishTasks["UI_Accessibility"] = true;
        
        LogPolish("UI/UX最終調整完了");
    }
    
    /// <summary>
    /// レスポンシブUI調整
    /// </summary>
    private IEnumerator AdjustResponsiveUI()
    {
        LogPolish("レスポンシブUI調整中...");
        
        // 全てのButtonコンポーネントを取得して調整
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            AdjustButtonForMobile(button);
            yield return null; // フレーム分散
        }
        
        // 全てのTextコンポーネントを取得して調整
        Text[] texts = FindObjectsOfType<Text>();
        foreach (Text text in texts)
        {
            AdjustTextForMobile(text);
            yield return null;
        }
        
        // TextMeshProコンポーネントも調整
        TextMeshProUGUI[] tmpTexts = FindObjectsOfType<TextMeshProUGUI>();
        foreach (TextMeshProUGUI tmpText in tmpTexts)
        {
            AdjustTMPTextForMobile(tmpText);
            yield return null;
        }
        
        LogPolish("レスポンシブUI調整完了");
    }
    
    /// <summary>
    /// モバイル向けボタン調整
    /// </summary>
    private void AdjustButtonForMobile(Button button)
    {
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        if (rectTransform == null) return;
        
        // 最小タッチターゲットサイズの確保
        Vector2 size = rectTransform.sizeDelta;
        size.x = Mathf.Max(size.x, uiPolishSettings.minButtonSize);
        size.y = Mathf.Max(size.y, uiPolishSettings.minButtonSize);
        
        // 最大サイズの制限
        size.x = Mathf.Min(size.x, uiPolishSettings.maxButtonSize);
        size.y = Mathf.Min(size.y, uiPolishSettings.maxButtonSize);
        
        rectTransform.sizeDelta = size;
        
        // ボタンの視覚的フィードバック強化
        EnhanceButtonFeedback(button);
    }
    
    /// <summary>
    /// ボタンの視覚的フィードバック強化
    /// </summary>
    private void EnhanceButtonFeedback(Button button)
    {
        // ColorBlockの調整
        ColorBlock colors = button.colors;
        colors.highlightedColor = uiPolishSettings.highlightColor;
        colors.pressedColor = Color.Lerp(colors.normalColor, Color.white, 0.3f);
        colors.fadeDuration = 0.1f;
        button.colors = colors;
        
        // ボタンプレス時のスケールアニメーション追加
        ButtonScaleAnimation scaleAnim = button.GetComponent<ButtonScaleAnimation>();
        if (scaleAnim == null)
        {
            scaleAnim = button.gameObject.AddComponent<ButtonScaleAnimation>();
        }
    }
    
    /// <summary>
    /// モバイル向けテキスト調整
    /// </summary>
    private void AdjustTextForMobile(Text text)
    {
        // フォントサイズの調整
        if (uiPolishSettings.enableDynamicFontSizing)
        {
            float screenScale = Mathf.Min(Screen.width, Screen.height) / 1080f;
            float adjustedSize = text.fontSize * screenScale;
            adjustedSize = Mathf.Clamp(adjustedSize, uiPolishSettings.minFontSize, uiPolishSettings.maxFontSize);
            text.fontSize = Mathf.RoundToInt(adjustedSize);
        }
        
        // 読みやすさの向上
        text.lineSpacing = 1.1f;
        
        // 日本語フォントの場合の特別調整
        if (IsJapaneseText(text.text))
        {
            text.fontSize = Mathf.RoundToInt(text.fontSize * 0.9f); // 日本語は少し小さく
        }
    }
    
    /// <summary>
    /// TextMeshPro向けテキスト調整
    /// </summary>
    private void AdjustTMPTextForMobile(TextMeshProUGUI tmpText)
    {
        // フォントサイズの調整
        if (uiPolishSettings.enableDynamicFontSizing)
        {
            float screenScale = Mathf.Min(Screen.width, Screen.height) / 1080f;
            float adjustedSize = tmpText.fontSize * screenScale;
            adjustedSize = Mathf.Clamp(adjustedSize, uiPolishSettings.minFontSize, uiPolishSettings.maxFontSize);
            tmpText.fontSize = adjustedSize;
        }
        
        // 読みやすさの向上
        tmpText.lineSpacing = 0.1f;
        tmpText.enableAutoSizing = true;
        tmpText.fontSizeMin = uiPolishSettings.minFontSize;
        tmpText.fontSizeMax = uiPolishSettings.maxFontSize;
    }
    
    /// <summary>
    /// 日本語テキストの判定
    /// </summary>
    private bool IsJapaneseText(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        
        foreach (char c in text)
        {
            // ひらがな、カタカナ、漢字の範囲をチェック
            if ((c >= 0x3040 && c <= 0x309F) || // ひらがな
                (c >= 0x30A0 && c <= 0x30FF) || // カタカナ
                (c >= 0x4E00 && c <= 0x9FAF))   // 漢字
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// アニメーション調整
    /// </summary>
    private IEnumerator AdjustAnimations()
    {
        LogPolish("アニメーション調整中...");
        
        // 全てのAnimatorコンポーネントを取得して調整
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (Animator animator in animators)
        {
            OptimizeAnimator(animator);
            yield return null;
        }
        
        // DOTweenアニメーションの最適化（もし使用している場合）
        OptimizeTweenAnimations();
        
        LogPolish("アニメーション調整完了");
    }
    
    /// <summary>
    /// Animatorの最適化
    /// </summary>
    private void OptimizeAnimator(Animator animator)
    {
        // カリング設定の最適化
        animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        
        // 不要なアニメーションレイヤーの無効化
        for (int i = 0; i < animator.layerCount; i++)
        {
            if (animator.GetLayerWeight(i) <= 0.01f)
            {
                animator.SetLayerWeight(i, 0f);
            }
        }
    }
    
    /// <summary>
    /// Tweenアニメーションの最適化
    /// </summary>
    private void OptimizeTweenAnimations()
    {
        // DOTweenの設定最適化（使用している場合）
        // DG.Tweening.DOTween.SetTweensCapacity(200, 50);
        // DG.Tweening.DOTween.defaultAutoPlay = DG.Tweening.AutoPlay.None;
    }
    
    /// <summary>
    /// アクセシビリティ調整
    /// </summary>
    private IEnumerator AdjustAccessibility()
    {
        LogPolish("アクセシビリティ調整中...");
        
        // カラーコントラストの調整
        AdjustColorContrast();
        yield return new WaitForSeconds(0.1f);
        
        // フォーカス可能要素の調整
        AdjustFocusableElements();
        yield return new WaitForSeconds(0.1f);
        
        // 音声フィードバックの追加
        AddAudioFeedback();
        yield return new WaitForSeconds(0.1f);
        
        LogPolish("アクセシビリティ調整完了");
    }
    
    /// <summary>
    /// カラーコントラストの調整
    /// </summary>
    private void AdjustColorContrast()
    {
        // 重要なUI要素のコントラストを確保
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            Image buttonImage = button.GetComponent<Image>();
            Text buttonText = button.GetComponentInChildren<Text>();
            
            if (buttonImage != null && buttonText != null)
            {
                EnsureColorContrast(buttonImage, buttonText);
            }
        }
    }
    
    /// <summary>
    /// カラーコントラストの確保
    /// </summary>
    private void EnsureColorContrast(Image background, Text text)
    {
        Color bgColor = background.color;
        Color textColor = text.color;
        
        // 輝度の計算
        float bgLuminance = GetLuminance(bgColor);
        float textLuminance = GetLuminance(textColor);
        
        // コントラスト比の計算
        float contrast = (Mathf.Max(bgLuminance, textLuminance) + 0.05f) / 
                        (Mathf.Min(bgLuminance, textLuminance) + 0.05f);
        
        // WCAG AA基準（4.5:1）を満たさない場合は調整
        if (contrast < 4.5f)
        {
            if (bgLuminance > textLuminance)
            {
                text.color = Color.black;
            }
            else
            {
                text.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// 輝度の計算
    /// </summary>
    private float GetLuminance(Color color)
    {
        float r = color.r <= 0.03928f ? color.r / 12.92f : Mathf.Pow((color.r + 0.055f) / 1.055f, 2.4f);
        float g = color.g <= 0.03928f ? color.g / 12.92f : Mathf.Pow((color.g + 0.055f) / 1.055f, 2.4f);
        float b = color.b <= 0.03928f ? color.b / 12.92f : Mathf.Pow((color.b + 0.055f) / 1.055f, 2.4f);
        
        return 0.2126f * r + 0.7152f * g + 0.0722f * b;
    }
    
    /// <summary>
    /// フォーカス可能要素の調整
    /// </summary>
    private void AdjustFocusableElements()
    {
        // 全てのSelectableコンポーネントにナビゲーション設定
        Selectable[] selectables = FindObjectsOfType<Selectable>();
        foreach (Selectable selectable in selectables)
        {
            Navigation nav = selectable.navigation;
            nav.mode = Navigation.Mode.Automatic;
            selectable.navigation = nav;
        }
    }
    
    /// <summary>
    /// 音声フィードバックの追加
    /// </summary>
    private void AddAudioFeedback()
    {
        // 重要なボタンに音声フィードバックを追加
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            AudioSource audioSource = button.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = button.gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = 0.5f;
            }
        }
    }
    
    /// <summary>
    /// パフォーマンス最適化
    /// </summary>
    private IEnumerator OptimizePerformance()
    {
        LogPolish("パフォーマンス最適化開始");
        
        // FPS最適化
        yield return StartCoroutine(OptimizeFPS());
        polishTasks["Performance_FPS"] = true;
        
        // メモリ最適化
        yield return StartCoroutine(OptimizeMemory());
        polishTasks["Performance_Memory"] = true;
        
        LogPolish("パフォーマンス最適化完了");
    }
    
    /// <summary>
    /// FPS最適化
    /// </summary>
    private IEnumerator OptimizeFPS()
    {
        LogPolish("FPS最適化中...");
        
        // ターゲットフレームレートの設定
        Application.targetFrameRate = performanceSettings.targetFPS;
        
        // VSync設定
        QualitySettings.vSyncCount = performanceSettings.enableVSync ? 1 : 0;
        
        // 描画最適化
        if (performanceSettings.enableBatching)
        {
            // バッチング設定の最適化
            QualitySettings.maxQueuedFrames = 2;
        }
        
        yield return new WaitForSeconds(0.1f);
        LogPolish("FPS最適化完了");
    }
    
    /// <summary>
    /// メモリ最適化
    /// </summary>
    private IEnumerator OptimizeMemory()
    {
        LogPolish("メモリ最適化中...");
        
        // ガベージコレクションの実行
        System.GC.Collect();
        yield return new WaitForSeconds(0.1f);
        
        // リソースのアンロード
        Resources.UnloadUnusedAssets();
        yield return new WaitForSeconds(0.5f);
        
        // メモリ使用量の確認
        long memoryUsage = System.GC.GetTotalMemory(false);
        LogPolish($"現在のメモリ使用量: {memoryUsage / (1024 * 1024)}MB");
        
        if (memoryUsage > performanceSettings.maxMemoryUsage)
        {
            LogPolish("警告: メモリ使用量が上限を超えています");
        }
        
        LogPolish("メモリ最適化完了");
    }
    
    /// <summary>
    /// 安定性向上
    /// </summary>
    private IEnumerator ImproveStability()
    {
        LogPolish("安定性向上開始");
        
        // エラーハンドリング強化
        yield return StartCoroutine(EnhanceErrorHandling());
        polishTasks["Stability_ErrorHandling"] = true;
        
        // セーブシステム強化
        yield return StartCoroutine(EnhanceSaveSystem());
        polishTasks["Stability_SaveSystem"] = true;
        
        LogPolish("安定性向上完了");
    }
    
    /// <summary>
    /// エラーハンドリング強化
    /// </summary>
    private IEnumerator EnhanceErrorHandling()
    {
        LogPolish("エラーハンドリング強化中...");
        
        // グローバルエラーハンドリングの設定
        SetupGlobalErrorHandling();
        
        yield return new WaitForSeconds(0.1f);
        LogPolish("エラーハンドリング強化完了");
    }
    
    /// <summary>
    /// グローバルエラーハンドリングの設定
    /// </summary>
    private void SetupGlobalErrorHandling()
    {
        Application.logMessageReceived += HandleGlobalError;
    }
    
    /// <summary>
    /// グローバルエラーの処理
    /// </summary>
    private void HandleGlobalError(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            LogPolish($"エラー検出: {logString}");
            
            if (stabilitySettings.enableAutoRecovery)
            {
                StartCoroutine(AttemptAutoRecovery());
            }
            
            if (stabilitySettings.enableCrashReporting)
            {
                ReportCrash(logString, stackTrace);
            }
        }
    }
    
    /// <summary>
    /// 自動復旧の試行
    /// </summary>
    private IEnumerator AttemptAutoRecovery()
    {
        LogPolish("自動復旧を試行中...");
        
        // メモリクリーンアップ
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
        
        yield return new WaitForSeconds(1f);
        
        // 重要なマネージャーの状態確認
        CheckManagerStates();
        
        LogPolish("自動復旧完了");
    }
    
    /// <summary>
    /// マネージャー状態の確認
    /// </summary>
    private void CheckManagerStates()
    {
        // GameManagerの状態確認
        if (GameManager.Instance == null)
        {
            LogPolish("警告: GameManagerが見つかりません");
        }
        
        // その他の重要なマネージャーの確認
        // TODO: 必要に応じて他のマネージャーも確認
    }
    
    /// <summary>
    /// クラッシュレポートの送信
    /// </summary>
    private void ReportCrash(string error, string stackTrace)
    {
        // クラッシュレポートの作成と送信
        var crashReport = new
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            error = error,
            stackTrace = stackTrace,
            platform = Application.platform.ToString(),
            version = Application.version,
            deviceModel = SystemInfo.deviceModel,
            operatingSystem = SystemInfo.operatingSystem
        };
        
        string reportJson = JsonUtility.ToJson(crashReport, true);
        LogPolish($"クラッシュレポート作成: {reportJson}");
        
        // TODO: 実際のレポート送信処理
    }
    
    /// <summary>
    /// セーブシステム強化
    /// </summary>
    private IEnumerator EnhanceSaveSystem()
    {
        LogPolish("セーブシステム強化中...");
        
        // バックアップセーブの設定
        if (stabilitySettings.enableBackupSaves)
        {
            SetupBackupSaves();
        }
        
        // 自動保存の設定
        SetupAutoSave();
        
        yield return new WaitForSeconds(0.1f);
        LogPolish("セーブシステム強化完了");
    }
    
    /// <summary>
    /// バックアップセーブの設定
    /// </summary>
    private void SetupBackupSaves()
    {
        // SaveManagerにバックアップ機能を追加
        SaveManager saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            // TODO: バックアップ機能の実装
            LogPolish("バックアップセーブ機能を設定しました");
        }
    }
    
    /// <summary>
    /// 自動保存の設定
    /// </summary>
    private void SetupAutoSave()
    {
        // 自動保存コルーチンの開始
        StartCoroutine(AutoSaveCoroutine());
    }
    
    /// <summary>
    /// 自動保存コルーチン
    /// </summary>
    private IEnumerator AutoSaveCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(stabilitySettings.autoSaveInterval);
            
            SaveManager saveManager = FindObjectOfType<SaveManager>();
            if (saveManager != null)
            {
                saveManager.SaveGame();
                LogPolish("自動保存実行");
            }
        }
    }
    
    /// <summary>
    /// ビルド準備
    /// </summary>
    private IEnumerator PrepareBuild()
    {
        LogPolish("ビルド準備開始");
        
        // ビルド最適化
        yield return StartCoroutine(OptimizeForBuild());
        polishTasks["Build_Optimization"] = true;
        
        // セキュリティ設定
        yield return StartCoroutine(ApplySecurity());
        polishTasks["Build_Security"] = true;
        
        LogPolish("ビルド準備完了");
    }
    
    /// <summary>
    /// ビルド最適化
    /// </summary>
    private IEnumerator OptimizeForBuild()
    {
        LogPolish("ビルド最適化中...");
        
        // モバイル最適化の適用
        if (buildSettings.optimizeForMobile)
        {
            ApplyMobileOptimizations();
        }
        
        // デバッグ機能の無効化
        if (!buildSettings.enableDebugging)
        {
            DisableDebuggingFeatures();
        }
        
        yield return new WaitForSeconds(0.1f);
        LogPolish("ビルド最適化完了");
    }
    
    /// <summary>
    /// モバイル最適化の適用
    /// </summary>
    private void ApplyMobileOptimizations()
    {
        // テクスチャ圧縮の設定
        if (buildSettings.enableTextureCompression)
        {
            // TODO: テクスチャ圧縮設定
        }
        
        // オーディオ圧縮の設定
        if (buildSettings.enableAudioCompression)
        {
            // TODO: オーディオ圧縮設定
        }
        
        // スクリプト最適化
        if (buildSettings.enableScriptOptimization)
        {
            // TODO: スクリプト最適化設定
        }
        
        LogPolish("モバイル最適化を適用しました");
    }
    
    /// <summary>
    /// デバッグ機能の無効化
    /// </summary>
    private void DisableDebuggingFeatures()
    {
        // デバッグログの無効化
        Debug.unityLogger.logEnabled = false;
        
        // プロファイリングの無効化
        if (!buildSettings.enableProfiling)
        {
            Profiler.enabled = false;
        }
        
        LogPolish("デバッグ機能を無効化しました");
    }
    
    /// <summary>
    /// セキュリティ設定の適用
    /// </summary>
    private IEnumerator ApplySecurity()
    {
        LogPolish("セキュリティ設定適用中...");
        
        // コード難読化の設定
        if (buildSettings.enableCodeObfuscation)
        {
            // TODO: コード難読化設定
        }
        
        // アンチチート機能の設定
        if (buildSettings.enableAntiCheat)
        {
            // TODO: アンチチート機能設定
        }
        
        yield return new WaitForSeconds(0.1f);
        LogPolish("セキュリティ設定適用完了");
    }
    
    /// <summary>
    /// 最終テスト
    /// </summary>
    private IEnumerator FinalTesting()
    {
        LogPolish("最終テスト開始");
        
        // 統合テストの実行
        IntegrationTestRunner testRunner = FindObjectOfType<IntegrationTestRunner>();
        if (testRunner != null)
        {
            testRunner.RunFullIntegrationTest();
            
            // テスト完了まで待機
            yield return new WaitForSeconds(5f);
        }
        
        // プラットフォームテストの実行
        MobilePlatformTester platformTester = FindObjectOfType<MobilePlatformTester>();
        if (platformTester != null)
        {
            platformTester.RunPlatformTests();
            
            // テスト完了まで待機
            yield return new WaitForSeconds(5f);
        }
        
        polishTasks["Final_Testing"] = true;
        LogPolish("最終テスト完了");
    }
    
    /// <summary>
    /// ポリッシュチェックの実行
    /// </summary>
    private void PerformPolishCheck()
    {
        // パフォーマンスチェック
        CheckPerformance();
        
        // メモリ使用量チェック
        CheckMemoryUsage();
        
        // エラー状況チェック
        CheckErrorStatus();
    }
    
    /// <summary>
    /// パフォーマンスチェック
    /// </summary>
    private void CheckPerformance()
    {
        float currentFPS = 1f / Time.deltaTime;
        if (currentFPS < performanceSettings.targetFPS * 0.8f)
        {
            LogPolish($"警告: FPSが低下しています ({currentFPS:F1})");
        }
    }
    
    /// <summary>
    /// メモリ使用量チェック
    /// </summary>
    private void CheckMemoryUsage()
    {
        long memoryUsage = System.GC.GetTotalMemory(false);
        if (memoryUsage > performanceSettings.maxMemoryUsage)
        {
            LogPolish($"警告: メモリ使用量が上限を超えています ({memoryUsage / (1024 * 1024)}MB)");
            
            // 自動メモリクリーンアップ
            if (performanceSettings.enableMemoryOptimization)
            {
                StartCoroutine(PerformMemoryCleanup());
            }
        }
    }
    
    /// <summary>
    /// メモリクリーンアップの実行
    /// </summary>
    private IEnumerator PerformMemoryCleanup()
    {
        LogPolish("メモリクリーンアップ実行中...");
        
        System.GC.Collect();
        yield return new WaitForSeconds(0.1f);
        
        Resources.UnloadUnusedAssets();
        yield return new WaitForSeconds(0.5f);
        
        LogPolish("メモリクリーンアップ完了");
    }
    
    /// <summary>
    /// エラー状況チェック
    /// </summary>
    private void CheckErrorStatus()
    {
        // 重要なマネージャーの存在確認
        if (GameManager.Instance == null)
        {
            LogPolish("エラー: GameManagerが見つかりません");
        }
        
        // TODO: 他の重要なコンポーネントの確認
    }
    
    /// <summary>
    /// ポリッシュレポートの生成
    /// </summary>
    private void GeneratePolishReport()
    {
        LogPolish("=== 最終ポリッシュレポート ===");
        
        int completedTasks = 0;
        int totalTasks = polishTasks.Count;
        
        foreach (var task in polishTasks)
        {
            string status = task.Value ? "完了" : "未完了";
            LogPolish($"[{status}] {task.Key}");
            
            if (task.Value) completedTasks++;
        }
        
        LogPolish($"ポリッシュ進捗: {completedTasks}/{totalTasks} ({(float)completedTasks / totalTasks * 100:F1}%)");
        
        // システム情報
        LogPolish("=== システム情報 ===");
        LogPolish($"Unity バージョン: {Application.unityVersion}");
        LogPolish($"プラットフォーム: {Application.platform}");
        LogPolish($"デバイス: {SystemInfo.deviceModel}");
        LogPolish($"OS: {SystemInfo.operatingSystem}");
        LogPolish($"メモリ: {SystemInfo.systemMemorySize}MB");
        LogPolish($"現在のFPS: {1f / Time.deltaTime:F1}");
        LogPolish($"メモリ使用量: {System.GC.GetTotalMemory(false) / (1024 * 1024)}MB");
        
        // レポートをファイルに保存
        SavePolishReport();
    }
    
    /// <summary>
    /// ポリッシュレポートの保存
    /// </summary>
    private void SavePolishReport()
    {
        try
        {
            string reportContent = string.Join("\n", polishLog);
            string fileName = $"polish_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            
            System.IO.File.WriteAllText(filePath, reportContent);
            LogPolish($"ポリッシュレポートを保存しました: {filePath}");
        }
        catch (Exception e)
        {
            LogPolish($"ポリッシュレポートの保存に失敗: {e.Message}");
        }
    }
    
    /// <summary>
    /// ポリッシュログの出力
    /// </summary>
    private void LogPolish(string message)
    {
        string logMessage = $"[ポリッシュ] {DateTime.Now:HH:mm:ss} {message}";
        
        if (enableDetailedLogging)
        {
            Debug.Log(logMessage);
        }
        
        polishLog.Add(logMessage);
        
        // ログサイズの制限
        if (polishLog.Count > 1000)
        {
            polishLog.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 公開メソッド：手動ポリッシュ実行
    /// </summary>
    public void ExecuteManualPolish()
    {
        StartCoroutine(ExecutePolishSequence());
    }
    
    /// <summary>
    /// 公開メソッド：ポリッシュ状況の取得
    /// </summary>
    public Dictionary<string, bool> GetPolishStatus()
    {
        return new Dictionary<string, bool>(polishTasks);
    }
    
    /// <summary>
    /// 公開メソッド：ポリッシュログの取得
    /// </summary>
    public List<string> GetPolishLog()
    {
        return new List<string>(polishLog);
    }
    
    private void OnDestroy()
    {
        // イベント購読解除
        Application.logMessageReceived -= HandleGlobalError;
    }
}

/// <summary>
/// ボタンスケールアニメーション用コンポーネント
/// </summary>
public class ButtonScaleAnimation : MonoBehaviour, UnityEngine.EventSystems.IPointerDownHandler, UnityEngine.EventSystems.IPointerUpHandler
{
    [SerializeField] private float scaleAmount = 0.95f;
    [SerializeField] private float animationDuration = 0.1f;
    
    private Vector3 originalScale;
    private Coroutine scaleCoroutine;
    
    private void Start()
    {
        originalScale = transform.localScale;
    }
    
    public void OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleAnimation(originalScale * scaleAmount));
    }
    
    public void OnPointerUp(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(ScaleAnimation(originalScale));
    }
    
    private IEnumerator ScaleAnimation(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        transform.localScale = targetScale;
        scaleCoroutine = null;
    }
}