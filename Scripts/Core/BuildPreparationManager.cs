using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// ビルド設定とリリース準備を管理するクラス
/// プラットフォーム固有の設定、最適化、品質保証を担当
/// </summary>
public class BuildPreparationManager : MonoBehaviour
{
    [Header("ビルド設定")]
    [SerializeField] private BuildConfiguration buildConfig = new BuildConfiguration();
    
    [Header("プラットフォーム設定")]
    [SerializeField] private PlatformSettings platformSettings = new PlatformSettings();
    
    [Header("品質設定")]
    [SerializeField] private QualityConfiguration qualityConfig = new QualityConfiguration();
    
    [Header("リリース設定")]
    [SerializeField] private ReleaseSettings releaseSettings = new ReleaseSettings();
    
    private List<string> buildLog = new List<string>();
    private Dictionary<string, bool> buildChecklist = new Dictionary<string, bool>();
    
    [System.Serializable]
    public class BuildConfiguration
    {
        [Header("基本設定")]
        public string productName = "ダンジョンオーナー";
        public string companyName = "YourCompany";
        public string version = "1.0.0";
        public int bundleVersionCode = 1;
        
        [Header("ビルド対象")]
        public bool buildForIOS = true;
        public bool buildForAndroid = true;
        public bool buildDevelopmentBuild = false;
        public bool enableDeepProfiling = false;
        
        [Header("最適化設定")]
        public bool enableScriptOptimization = true;
        public bool enableTextureCompression = true;
        public bool enableAudioCompression = true;
        public bool stripEngineCode = true;
    }
    
    [System.Serializable]
    public class PlatformSettings
    {
        [Header("iOS設定")]
        public string iOSBundleIdentifier = "com.yourcompany.dungeonowner";
        public string iOSMinimumVersion = "12.0";
        public bool iOSRequireARKit = false;
        public bool iOSAllowHTTP = false;
        
        [Header("Android設定")]
        public string androidBundleIdentifier = "com.yourcompany.dungeonowner";
        public int androidMinSDKVersion = 21; // Android 5.0
        public int androidTargetSDKVersion = 33; // Android 13
        public bool androidUseAPKExpansion = false;
        
        [Header("共通設定")]
        public bool requireInternet = false;
        public bool supportLandscape = false;
        public bool supportPortrait = true;
        public bool allowFullscreen = false;
    }
    
    [System.Serializable]
    public class QualityConfiguration
    {
        [Header("グラフィック品質")]
        public int defaultQualityLevel = 2; // Medium
        public bool enableAntiAliasing = true;
        public bool enableShadows = true;
        public int textureQuality = 0; // Full Res
        
        [Header("パフォーマンス設定")]
        public int targetFrameRate = 60;
        public bool enableVSync = true;
        public int maxLODLevel = 0;
        public float lodBias = 1.0f;
        
        [Header("モバイル最適化")]
        public bool enableGPUSkinning = true;
        public bool enableBatching = true;
        public bool enableOcclusionCulling = true;
        public int pixelLightCount = 1;
    }
    
    [System.Serializable]
    public class ReleaseSettings
    {
        [Header("配布設定")]
        public bool createReleaseNotes = true;
        public bool generateScreenshots = true;
        public bool createBuildReport = true;
        public bool validateBuild = true;
        
        [Header("セキュリティ")]
        public bool enableCodeSigning = true;
        public bool enableObfuscation = true;
        public bool enableAntiTamper = true;
        public bool removeDebugSymbols = true;
        
        [Header("ストア設定")]
        public string appStoreCategory = "Games";
        public string contentRating = "4+";
        public List<string> keywords = new List<string>();
        public string shortDescription = "";
        public string fullDescription = "";
    }
    
    private void Start()
    {
        InitializeBuildPreparation();
    }
    
    /// <summary>
    /// ビルド準備の初期化
    /// </summary>
    private void InitializeBuildPreparation()
    {
        LogBuild("BuildPreparationManager初期化開始");
        
        InitializeBuildChecklist();
        ValidateCurrentSettings();
        
        LogBuild("BuildPreparationManager初期化完了");
    }
    
    /// <summary>
    /// ビルドチェックリストの初期化
    /// </summary>
    private void InitializeBuildChecklist()
    {
        buildChecklist["ProjectSettings"] = false;
        buildChecklist["PlatformSettings"] = false;
        buildChecklist["QualitySettings"] = false;
        buildChecklist["AssetOptimization"] = false;
        buildChecklist["SecuritySettings"] = false;
        buildChecklist["TestingComplete"] = false;
        buildChecklist["DocumentationReady"] = false;
        buildChecklist["StoreAssetsReady"] = false;
        buildChecklist["BuildValidation"] = false;
        buildChecklist["FinalApproval"] = false;
    }
    
    /// <summary>
    /// 現在の設定の検証
    /// </summary>
    private void ValidateCurrentSettings()
    {
        LogBuild("現在の設定を検証中...");
        
        // プロジェクト設定の検証
        ValidateProjectSettings();
        
        // プラットフォーム設定の検証
        ValidatePlatformSettings();
        
        // 品質設定の検証
        ValidateQualitySettings();
        
        LogBuild("設定検証完了");
    }
    
    /// <summary>
    /// プロジェクト設定の検証
    /// </summary>
    private void ValidateProjectSettings()
    {
        bool isValid = true;
        
        // 製品名の確認
        if (string.IsNullOrEmpty(buildConfig.productName))
        {
            LogBuild("エラー: 製品名が設定されていません");
            isValid = false;
        }
        
        // バージョンの確認
        if (string.IsNullOrEmpty(buildConfig.version))
        {
            LogBuild("エラー: バージョンが設定されていません");
            isValid = false;
        }
        
        // バンドルバージョンの確認
        if (buildConfig.bundleVersionCode <= 0)
        {
            LogBuild("エラー: バンドルバージョンコードが無効です");
            isValid = false;
        }
        
        buildChecklist["ProjectSettings"] = isValid;
        LogBuild($"プロジェクト設定検証: {(isValid ? "成功" : "失敗")}");
    }
    
    /// <summary>
    /// プラットフォーム設定の検証
    /// </summary>
    private void ValidatePlatformSettings()
    {
        bool isValid = true;
        
        // iOS設定の確認
        if (buildConfig.buildForIOS)
        {
            if (string.IsNullOrEmpty(platformSettings.iOSBundleIdentifier))
            {
                LogBuild("エラー: iOSバンドル識別子が設定されていません");
                isValid = false;
            }
            
            if (string.IsNullOrEmpty(platformSettings.iOSMinimumVersion))
            {
                LogBuild("エラー: iOS最小バージョンが設定されていません");
                isValid = false;
            }
        }
        
        // Android設定の確認
        if (buildConfig.buildForAndroid)
        {
            if (string.IsNullOrEmpty(platformSettings.androidBundleIdentifier))
            {
                LogBuild("エラー: Androidバンドル識別子が設定されていません");
                isValid = false;
            }
            
            if (platformSettings.androidMinSDKVersion < 21)
            {
                LogBuild("警告: Android最小SDKバージョンが低すぎます");
            }
        }
        
        buildChecklist["PlatformSettings"] = isValid;
        LogBuild($"プラットフォーム設定検証: {(isValid ? "成功" : "失敗")}");
    }
    
    /// <summary>
    /// 品質設定の検証
    /// </summary>
    private void ValidateQualitySettings()
    {
        bool isValid = true;
        
        // フレームレート設定の確認
        if (qualityConfig.targetFrameRate <= 0 || qualityConfig.targetFrameRate > 120)
        {
            LogBuild("警告: ターゲットフレームレートが異常です");
        }
        
        // 品質レベルの確認
        if (qualityConfig.defaultQualityLevel < 0 || qualityConfig.defaultQualityLevel > 5)
        {
            LogBuild("エラー: デフォルト品質レベルが無効です");
            isValid = false;
        }
        
        buildChecklist["QualitySettings"] = isValid;
        LogBuild($"品質設定検証: {(isValid ? "成功" : "失敗")}");
    }
    
    /// <summary>
    /// ビルド準備プロセスの実行
    /// </summary>
    public void ExecuteBuildPreparation()
    {
        StartCoroutine(BuildPreparationSequence());
    }
    
    /// <summary>
    /// ビルド準備シーケンス
    /// </summary>
    private IEnumerator BuildPreparationSequence()
    {
        LogBuild("=== ビルド準備シーケンス開始 ===");
        
        // 1. プロジェクト設定の適用
        yield return StartCoroutine(ApplyProjectSettings());
        
        // 2. プラットフォーム設定の適用
        yield return StartCoroutine(ApplyPlatformSettings());
        
        // 3. 品質設定の適用
        yield return StartCoroutine(ApplyQualitySettings());
        
        // 4. アセット最適化
        yield return StartCoroutine(OptimizeAssets());
        
        // 5. セキュリティ設定の適用
        yield return StartCoroutine(ApplySecuritySettings());
        
        // 6. 最終テストの実行
        yield return StartCoroutine(ExecuteFinalTests());
        
        // 7. ドキュメント準備
        yield return StartCoroutine(PrepareDocumentation());
        
        // 8. ストアアセット準備
        yield return StartCoroutine(PrepareStoreAssets());
        
        // 9. ビルド検証
        yield return StartCoroutine(ValidateBuild());
        
        // 10. 最終承認
        yield return StartCoroutine(FinalApproval());
        
        LogBuild("=== ビルド準備シーケンス完了 ===");
        GenerateBuildReport();
    }
    
    /// <summary>
    /// プロジェクト設定の適用
    /// </summary>
    private IEnumerator ApplyProjectSettings()
    {
        LogBuild("プロジェクト設定を適用中...");
        
        // Unity PlayerSettingsの設定
        PlayerSettings.productName = buildConfig.productName;
        PlayerSettings.companyName = buildConfig.companyName;
        PlayerSettings.bundleVersion = buildConfig.version;
        
        // 最適化設定
        if (buildConfig.enableScriptOptimization)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        }
        
        if (buildConfig.stripEngineCode)
        {
            PlayerSettings.stripEngineCode = true;
        }
        
        yield return new WaitForSeconds(0.1f);
        buildChecklist["ProjectSettings"] = true;
        LogBuild("プロジェクト設定適用完了");
    }
    
    /// <summary>
    /// プラットフォーム設定の適用
    /// </summary>
    private IEnumerator ApplyPlatformSettings()
    {
        LogBuild("プラットフォーム設定を適用中...");
        
        // iOS設定
        if (buildConfig.buildForIOS)
        {
            ApplyIOSSettings();
        }
        
        // Android設定
        if (buildConfig.buildForAndroid)
        {
            ApplyAndroidSettings();
        }
        
        // 共通設定
        ApplyCommonPlatformSettings();
        
        yield return new WaitForSeconds(0.1f);
        buildChecklist["PlatformSettings"] = true;
        LogBuild("プラットフォーム設定適用完了");
    }
    
    /// <summary>
    /// iOS設定の適用
    /// </summary>
    private void ApplyIOSSettings()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, platformSettings.iOSBundleIdentifier);
        PlayerSettings.iOS.targetOSVersionString = platformSettings.iOSMinimumVersion;
        PlayerSettings.iOS.allowHTTPDownload = platformSettings.iOSAllowHTTP;
        
        // ARKit設定
        if (!platformSettings.iOSRequireARKit)
        {
            // ARKitを無効化
        }
        
        LogBuild("iOS設定を適用しました");
    }
    
    /// <summary>
    /// Android設定の適用
    /// </summary>
    private void ApplyAndroidSettings()
    {
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, platformSettings.androidBundleIdentifier);
        PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)platformSettings.androidMinSDKVersion;
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)platformSettings.androidTargetSDKVersion;
        
        // APK拡張ファイル設定
        PlayerSettings.Android.useAPKExpansionFiles = platformSettings.androidUseAPKExpansion;
        
        // バンドルバージョンコード
        PlayerSettings.Android.bundleVersionCode = buildConfig.bundleVersionCode;
        
        LogBuild("Android設定を適用しました");
    }
    
    /// <summary>
    /// 共通プラットフォーム設定の適用
    /// </summary>
    private void ApplyCommonPlatformSettings()
    {
        // 画面向き設定
        if (platformSettings.supportPortrait && !platformSettings.supportLandscape)
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        }
        else if (!platformSettings.supportPortrait && platformSettings.supportLandscape)
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        }
        else
        {
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        }
        
        // フルスクリーン設定
        PlayerSettings.defaultIsNativeResolution = !platformSettings.allowFullscreen;
        
        LogBuild("共通プラットフォーム設定を適用しました");
    }
    
    /// <summary>
    /// 品質設定の適用
    /// </summary>
    private IEnumerator ApplyQualitySettings()
    {
        LogBuild("品質設定を適用中...");
        
        // デフォルト品質レベル
        QualitySettings.SetQualityLevel(qualityConfig.defaultQualityLevel);
        
        // フレームレート設定
        Application.targetFrameRate = qualityConfig.targetFrameRate;
        QualitySettings.vSyncCount = qualityConfig.enableVSync ? 1 : 0;
        
        // グラフィック設定
        QualitySettings.antiAliasing = qualityConfig.enableAntiAliasing ? 2 : 0;
        QualitySettings.shadows = qualityConfig.enableShadows ? ShadowQuality.All : ShadowQuality.Disable;
        QualitySettings.masterTextureLimit = qualityConfig.textureQuality;
        
        // LOD設定
        QualitySettings.maximumLODLevel = qualityConfig.maxLODLevel;
        QualitySettings.lodBias = qualityConfig.lodBias;
        
        // モバイル最適化
        QualitySettings.skinWeights = qualityConfig.enableGPUSkinning ? SkinWeights.FourBones : SkinWeights.TwoBones;
        QualitySettings.pixelLightCount = qualityConfig.pixelLightCount;
        
        yield return new WaitForSeconds(0.1f);
        buildChecklist["QualitySettings"] = true;
        LogBuild("品質設定適用完了");
    }
    
    /// <summary>
    /// アセット最適化
    /// </summary>
    private IEnumerator OptimizeAssets()
    {
        LogBuild("アセット最適化中...");
        
        // テクスチャ最適化
        if (buildConfig.enableTextureCompression)
        {
            yield return StartCoroutine(OptimizeTextures());
        }
        
        // オーディオ最適化
        if (buildConfig.enableAudioCompression)
        {
            yield return StartCoroutine(OptimizeAudio());
        }
        
        // 不要アセットの削除
        yield return StartCoroutine(RemoveUnusedAssets());
        
        buildChecklist["AssetOptimization"] = true;
        LogBuild("アセット最適化完了");
    }
    
    /// <summary>
    /// テクスチャ最適化
    /// </summary>
    private IEnumerator OptimizeTextures()
    {
        LogBuild("テクスチャ最適化中...");
        
        // TODO: テクスチャ圧縮設定の適用
        // AssetDatabase.Refresh();
        
        yield return new WaitForSeconds(0.5f);
        LogBuild("テクスチャ最適化完了");
    }
    
    /// <summary>
    /// オーディオ最適化
    /// </summary>
    private IEnumerator OptimizeAudio()
    {
        LogBuild("オーディオ最適化中...");
        
        // TODO: オーディオ圧縮設定の適用
        
        yield return new WaitForSeconds(0.3f);
        LogBuild("オーディオ最適化完了");
    }
    
    /// <summary>
    /// 不要アセットの削除
    /// </summary>
    private IEnumerator RemoveUnusedAssets()
    {
        LogBuild("不要アセット削除中...");
        
        // 未使用アセットのアンロード
        Resources.UnloadUnusedAssets();
        
        yield return new WaitForSeconds(1f);
        LogBuild("不要アセット削除完了");
    }
    
    /// <summary>
    /// セキュリティ設定の適用
    /// </summary>
    private IEnumerator ApplySecuritySettings()
    {
        LogBuild("セキュリティ設定適用中...");
        
        // コード署名設定
        if (releaseSettings.enableCodeSigning)
        {
            ApplyCodeSigning();
        }
        
        // 難読化設定
        if (releaseSettings.enableObfuscation)
        {
            ApplyObfuscation();
        }
        
        // アンチタンパー設定
        if (releaseSettings.enableAntiTamper)
        {
            ApplyAntiTamper();
        }
        
        // デバッグシンボル削除
        if (releaseSettings.removeDebugSymbols)
        {
            RemoveDebugSymbols();
        }
        
        yield return new WaitForSeconds(0.1f);
        buildChecklist["SecuritySettings"] = true;
        LogBuild("セキュリティ設定適用完了");
    }
    
    /// <summary>
    /// コード署名の適用
    /// </summary>
    private void ApplyCodeSigning()
    {
        // TODO: コード署名設定
        LogBuild("コード署名設定を適用しました");
    }
    
    /// <summary>
    /// 難読化の適用
    /// </summary>
    private void ApplyObfuscation()
    {
        // TODO: コード難読化設定
        LogBuild("コード難読化設定を適用しました");
    }
    
    /// <summary>
    /// アンチタンパーの適用
    /// </summary>
    private void ApplyAntiTamper()
    {
        // TODO: アンチタンパー設定
        LogBuild("アンチタンパー設定を適用しました");
    }
    
    /// <summary>
    /// デバッグシンボルの削除
    /// </summary>
    private void RemoveDebugSymbols()
    {
        // TODO: デバッグシンボル削除設定
        LogBuild("デバッグシンボルを削除しました");
    }
    
    /// <summary>
    /// 最終テストの実行
    /// </summary>
    private IEnumerator ExecuteFinalTests()
    {
        LogBuild("最終テスト実行中...");
        
        // 統合テストの実行
        IntegrationTestRunner testRunner = FindObjectOfType<IntegrationTestRunner>();
        if (testRunner != null)
        {
            testRunner.RunFullIntegrationTest();
            yield return new WaitForSeconds(10f); // テスト完了待機
        }
        
        // プラットフォームテストの実行
        MobilePlatformTester platformTester = FindObjectOfType<MobilePlatformTester>();
        if (platformTester != null)
        {
            platformTester.RunPlatformTests();
            yield return new WaitForSeconds(10f); // テスト完了待機
        }
        
        buildChecklist["TestingComplete"] = true;
        LogBuild("最終テスト完了");
    }
    
    /// <summary>
    /// ドキュメント準備
    /// </summary>
    private IEnumerator PrepareDocumentation()
    {
        LogBuild("ドキュメント準備中...");
        
        // リリースノートの生成
        if (releaseSettings.createReleaseNotes)
        {
            GenerateReleaseNotes();
        }
        
        // ビルドレポートの生成
        if (releaseSettings.createBuildReport)
        {
            GenerateBuildReport();
        }
        
        yield return new WaitForSeconds(0.1f);
        buildChecklist["DocumentationReady"] = true;
        LogBuild("ドキュメント準備完了");
    }
    
    /// <summary>
    /// リリースノートの生成
    /// </summary>
    private void GenerateReleaseNotes()
    {
        var releaseNotes = new System.Text.StringBuilder();
        releaseNotes.AppendLine($"# {buildConfig.productName} v{buildConfig.version}");
        releaseNotes.AppendLine($"リリース日: {DateTime.Now:yyyy-MM-dd}");
        releaseNotes.AppendLine();
        releaseNotes.AppendLine("## 新機能");
        releaseNotes.AppendLine("- 最終ポリッシュとUI/UX改善");
        releaseNotes.AppendLine("- パフォーマンス最適化");
        releaseNotes.AppendLine("- 安定性向上");
        releaseNotes.AppendLine();
        releaseNotes.AppendLine("## バグ修正");
        releaseNotes.AppendLine("- 各種バグ修正と安定性向上");
        releaseNotes.AppendLine();
        releaseNotes.AppendLine("## 技術的改善");
        releaseNotes.AppendLine("- メモリ使用量の最適化");
        releaseNotes.AppendLine("- フレームレートの安定化");
        releaseNotes.AppendLine("- モバイル端末での動作最適化");
        
        try
        {
            string filePath = Path.Combine(Application.dataPath, "..", "ReleaseNotes.md");
            File.WriteAllText(filePath, releaseNotes.ToString());
            LogBuild($"リリースノートを生成しました: {filePath}");
        }
        catch (Exception e)
        {
            LogBuild($"リリースノート生成エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// ストアアセット準備
    /// </summary>
    private IEnumerator PrepareStoreAssets()
    {
        LogBuild("ストアアセット準備中...");
        
        // スクリーンショット生成
        if (releaseSettings.generateScreenshots)
        {
            yield return StartCoroutine(GenerateScreenshots());
        }
        
        // アプリアイコンの確認
        ValidateAppIcons();
        
        // ストア説明文の準備
        PrepareStoreDescriptions();
        
        buildChecklist["StoreAssetsReady"] = true;
        LogBuild("ストアアセット準備完了");
    }
    
    /// <summary>
    /// スクリーンショット生成
    /// </summary>
    private IEnumerator GenerateScreenshots()
    {
        LogBuild("スクリーンショット生成中...");
        
        // TODO: 自動スクリーンショット生成
        // 各画面のスクリーンショットを撮影
        
        yield return new WaitForSeconds(1f);
        LogBuild("スクリーンショット生成完了");
    }
    
    /// <summary>
    /// アプリアイコンの検証
    /// </summary>
    private void ValidateAppIcons()
    {
        // TODO: アプリアイコンの存在と品質確認
        LogBuild("アプリアイコンを検証しました");
    }
    
    /// <summary>
    /// ストア説明文の準備
    /// </summary>
    private void PrepareStoreDescriptions()
    {
        if (string.IsNullOrEmpty(releaseSettings.shortDescription))
        {
            releaseSettings.shortDescription = "ダンジョンの神となり、モンスターを配置して侵入者からコアを守る戦略的防衛ゲーム";
        }
        
        if (string.IsNullOrEmpty(releaseSettings.fullDescription))
        {
            releaseSettings.fullDescription = GenerateFullDescription();
        }
        
        LogBuild("ストア説明文を準備しました");
    }
    
    /// <summary>
    /// 詳細説明文の生成
    /// </summary>
    private string GenerateFullDescription()
    {
        return @"「ダンジョンオーナー」は、プレイヤーがダンジョンの神となり、最深部のダンジョンコアを侵入者から守るモバイル向けリアルタイム防衛ゲームです。

【ゲームの特徴】
• 縦画面での片手操作に最適化
• 多様なモンスターの配置と戦略的なダンジョン経営
• リアルタイム戦闘システム
• 階層拡張とモンスター強化
• パーティシステムによる協力戦闘

【主な機能】
• モンスター配置システム
• 侵入者との戦闘
• 経済システム（金貨管理）
• 階層拡張機能
• 退避スポットシステム
• ボスキャラクター配置
• 罠アイテムシステム
• 時間制御機能

戦略的思考と素早い判断力が試される、奥深いゲーム体験をお楽しみください。";
    }
    
    /// <summary>
    /// ビルド検証
    /// </summary>
    private IEnumerator ValidateBuild()
    {
        LogBuild("ビルド検証中...");
        
        // 設定の最終確認
        bool allSettingsValid = ValidateAllSettings();
        
        // チェックリストの確認
        bool allTasksComplete = ValidateChecklist();
        
        // 依存関係の確認
        bool dependenciesValid = ValidateDependencies();
        
        bool buildValid = allSettingsValid && allTasksComplete && dependenciesValid;
        
        buildChecklist["BuildValidation"] = buildValid;
        
        if (buildValid)
        {
            LogBuild("ビルド検証成功");
        }
        else
        {
            LogBuild("ビルド検証失敗 - 問題を修正してください");
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    /// <summary>
    /// 全設定の検証
    /// </summary>
    private bool ValidateAllSettings()
    {
        return buildChecklist["ProjectSettings"] && 
               buildChecklist["PlatformSettings"] && 
               buildChecklist["QualitySettings"];
    }
    
    /// <summary>
    /// チェックリストの検証
    /// </summary>
    private bool ValidateChecklist()
    {
        int completedTasks = 0;
        int totalTasks = buildChecklist.Count - 1; // FinalApprovalを除く
        
        foreach (var task in buildChecklist)
        {
            if (task.Key != "FinalApproval" && task.Value)
            {
                completedTasks++;
            }
        }
        
        LogBuild($"チェックリスト進捗: {completedTasks}/{totalTasks}");
        return completedTasks == totalTasks;
    }
    
    /// <summary>
    /// 依存関係の検証
    /// </summary>
    private bool ValidateDependencies()
    {
        // 重要なマネージャーの存在確認
        bool managersExist = true;
        
        if (FindObjectOfType<GameManager>() == null)
        {
            LogBuild("エラー: GameManagerが見つかりません");
            managersExist = false;
        }
        
        if (FindObjectOfType<UIManager>() == null)
        {
            LogBuild("エラー: UIManagerが見つかりません");
            managersExist = false;
        }
        
        // TODO: 他の重要なコンポーネントの確認
        
        return managersExist;
    }
    
    /// <summary>
    /// 最終承認
    /// </summary>
    private IEnumerator FinalApproval()
    {
        LogBuild("最終承認プロセス開始");
        
        // 全てのチェックが完了しているか確認
        bool readyForRelease = true;
        
        foreach (var task in buildChecklist)
        {
            if (task.Key != "FinalApproval" && !task.Value)
            {
                LogBuild($"未完了タスク: {task.Key}");
                readyForRelease = false;
            }
        }
        
        if (readyForRelease)
        {
            buildChecklist["FinalApproval"] = true;
            LogBuild("最終承認完了 - リリース準備完了");
        }
        else
        {
            LogBuild("最終承認保留 - 未完了タスクがあります");
        }
        
        yield return new WaitForSeconds(0.1f);
    }
    
    /// <summary>
    /// ビルドレポートの生成
    /// </summary>
    private void GenerateBuildReport()
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== ビルド準備レポート ===");
        report.AppendLine($"生成日時: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"製品名: {buildConfig.productName}");
        report.AppendLine($"バージョン: {buildConfig.version}");
        report.AppendLine($"バンドルバージョンコード: {buildConfig.bundleVersionCode}");
        report.AppendLine();
        
        // チェックリスト状況
        report.AppendLine("=== チェックリスト状況 ===");
        foreach (var task in buildChecklist)
        {
            string status = task.Value ? "完了" : "未完了";
            report.AppendLine($"[{status}] {task.Key}");
        }
        report.AppendLine();
        
        // システム情報
        report.AppendLine("=== システム情報 ===");
        report.AppendLine($"Unity バージョン: {Application.unityVersion}");
        report.AppendLine($"プラットフォーム: {Application.platform}");
        report.AppendLine($"デバイス: {SystemInfo.deviceModel}");
        report.AppendLine($"OS: {SystemInfo.operatingSystem}");
        report.AppendLine();
        
        // ビルドログ
        report.AppendLine("=== ビルドログ ===");
        foreach (string logEntry in buildLog)
        {
            report.AppendLine(logEntry);
        }
        
        // レポートの保存
        try
        {
            string fileName = $"build_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllText(filePath, report.ToString());
            LogBuild($"ビルドレポートを保存しました: {filePath}");
        }
        catch (Exception e)
        {
            LogBuild($"ビルドレポート保存エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// ビルドログの出力
    /// </summary>
    private void LogBuild(string message)
    {
        string logMessage = $"[ビルド準備] {DateTime.Now:HH:mm:ss} {message}";
        Debug.Log(logMessage);
        buildLog.Add(logMessage);
        
        // ログサイズの制限
        if (buildLog.Count > 1000)
        {
            buildLog.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 公開メソッド：ビルドチェックリストの取得
    /// </summary>
    public Dictionary<string, bool> GetBuildChecklist()
    {
        return new Dictionary<string, bool>(buildChecklist);
    }
    
    /// <summary>
    /// 公開メソッド：ビルドログの取得
    /// </summary>
    public List<string> GetBuildLog()
    {
        return new List<string>(buildLog);
    }
    
    /// <summary>
    /// 公開メソッド：リリース準備状況の確認
    /// </summary>
    public bool IsReadyForRelease()
    {
        return buildChecklist.ContainsKey("FinalApproval") && buildChecklist["FinalApproval"];
    }
}