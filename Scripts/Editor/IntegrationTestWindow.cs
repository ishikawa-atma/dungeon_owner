using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 統合テストとバランス調整の実行・管理を行うエディタウィンドウ
/// 最終ポリッシュ機能を統合
/// </summary>
public class IntegrationTestWindow : EditorWindow
{
    private IntegrationTestRunner testRunner;
    private IntegrationTestManager testManager;
    private MobilePlatformTester platformTester;
    private GameBalanceManager balanceManager;
    private FinalPolishManager polishManager;
    private RequirementValidator requirementValidator;
    private BuildPreparationManager buildPreparationManager;
    
    private Vector2 scrollPosition;
    private bool showTestResults = true;
    private bool showBalanceSettings = true;
    private bool showPlatformInfo = true;
    private bool showPolishSettings = true;
    private bool showRequirementValidation = true;
    private bool showBuildPreparation = true;
    
    private string lastTestReport = "";
    private string lastPolishReport = "";
    private string lastValidationReport = "";
    
    [MenuItem("DungeonOwner/Integration Test Window")]
    public static void ShowWindow()
    {
        GetWindow<IntegrationTestWindow>("統合テスト管理");
    }
    
    private void OnEnable()
    {
        FindTestComponents();
    }
    
    private void FindTestComponents()
    {
        testRunner = FindObjectOfType<IntegrationTestRunner>();
        testManager = FindObjectOfType<IntegrationTestManager>();
        platformTester = FindObjectOfType<MobilePlatformTester>();
        balanceManager = FindObjectOfType<GameBalanceManager>();
        polishManager = FindObjectOfType<FinalPolishManager>();
        requirementValidator = FindObjectOfType<RequirementValidator>();
        buildPreparationManager = FindObjectOfType<BuildPreparationManager>();
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        EditorGUILayout.LabelField("統合テスト管理", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // テストコンポーネントの状態表示
        DrawComponentStatus();
        EditorGUILayout.Space();
        
        // テスト実行ボタン
        DrawTestExecutionButtons();
        EditorGUILayout.Space();
        
        // 最終ポリッシュ設定
        if (showPolishSettings)
        {
            DrawPolishSettings();
            EditorGUILayout.Space();
        }
        
        // 要件検証
        if (showRequirementValidation)
        {
            DrawRequirementValidation();
            EditorGUILayout.Space();
        }
        
        // ビルド準備
        if (showBuildPreparation)
        {
            DrawBuildPreparation();
            EditorGUILayout.Space();
        }
        
        // バランス調整設定
        if (showBalanceSettings)
        {
            DrawBalanceSettings();
            EditorGUILayout.Space();
        }
        
        // プラットフォーム情報
        if (showPlatformInfo)
        {
            DrawPlatformInfo();
            EditorGUILayout.Space();
        }
        
        // テスト結果表示
        if (showTestResults)
        {
            DrawTestResults();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void DrawComponentStatus()
    {
        EditorGUILayout.LabelField("コンポーネント状態", EditorStyles.boldLabel);
        
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Toggle("IntegrationTestRunner", testRunner != null);
        EditorGUILayout.Toggle("IntegrationTestManager", testManager != null);
        EditorGUILayout.Toggle("MobilePlatformTester", platformTester != null);
        EditorGUILayout.Toggle("GameBalanceManager", balanceManager != null);
        EditorGUILayout.Toggle("FinalPolishManager", polishManager != null);
        EditorGUILayout.Toggle("RequirementValidator", requirementValidator != null);
        EditorGUILayout.Toggle("BuildPreparationManager", buildPreparationManager != null);
        EditorGUI.EndDisabledGroup();
        
        if (GUILayout.Button("コンポーネントを再検索"))
        {
            FindTestComponents();
        }
        
        if (GUILayout.Button("テストコンポーネントを作成"))
        {
            CreateTestComponents();
        }
    }
    
    private void DrawTestExecutionButtons()
    {
        EditorGUILayout.LabelField("テスト実行", EditorStyles.boldLabel);
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        
        if (GUILayout.Button("完全統合テスト実行", GUILayout.Height(30)))
        {
            if (testRunner != null)
            {
                testRunner.RunFullIntegrationTest();
            }
            else
            {
                Debug.LogError("IntegrationTestRunnerが見つかりません");
            }
        }
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("プラットフォームテスト"))
        {
            if (testRunner != null)
            {
                testRunner.RunPlatformTestsOnly();
            }
            else if (platformTester != null)
            {
                platformTester.RunPlatformTests();
            }
        }
        
        if (GUILayout.Button("統合テスト"))
        {
            if (testRunner != null)
            {
                testRunner.RunIntegrationTestsOnly();
            }
            else if (testManager != null)
            {
                testManager.RunIntegrationTests();
            }
        }
        
        if (GUILayout.Button("バランステスト"))
        {
            if (testRunner != null)
            {
                testRunner.RunBalanceTestsOnly();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.EndDisabledGroup();
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("テストを実行するにはプレイモードに入ってください", MessageType.Info);
        }
    }
    
    private void DrawBalanceSettings()
    {
        showBalanceSettings = EditorGUILayout.Foldout(showBalanceSettings, "バランス調整設定");
        
        if (!showBalanceSettings) return;
        
        if (balanceManager == null)
        {
            EditorGUILayout.HelpBox("GameBalanceManagerが見つかりません", MessageType.Warning);
            return;
        }
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        
        if (GUILayout.Button("バランス設定をリセット"))
        {
            balanceManager.ResetBalanceSettings();
        }
        
        if (GUILayout.Button("バランス設定を保存"))
        {
            balanceManager.SaveBalanceSettings();
        }
        
        EditorGUI.EndDisabledGroup();
        
        if (Application.isPlaying && balanceManager != null)
        {
            var metrics = balanceManager.GetMetrics();
            
            EditorGUILayout.LabelField("現在のメトリクス", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"総戦闘数: {metrics.totalCombats}");
            EditorGUILayout.LabelField($"勝率: {metrics.playerWinRate:P2}");
            EditorGUILayout.LabelField($"平均戦闘時間: {metrics.averageCombatDuration:F2}秒");
            EditorGUILayout.LabelField($"金貨獲得率: {metrics.averageGoldPerMinute:F1}/分");
            EditorGUILayout.LabelField($"階層進行度: {metrics.averageFloorProgression:F1}");
        }
    }
    
    private void DrawPlatformInfo()
    {
        showPlatformInfo = EditorGUILayout.Foldout(showPlatformInfo, "プラットフォーム情報");
        
        if (!showPlatformInfo) return;
        
        EditorGUILayout.LabelField("システム情報", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"プラットフォーム: {Application.platform}");
        EditorGUILayout.LabelField($"Unity バージョン: {Application.unityVersion}");
        EditorGUILayout.LabelField($"デバイスモデル: {SystemInfo.deviceModel}");
        EditorGUILayout.LabelField($"OS: {SystemInfo.operatingSystem}");
        EditorGUILayout.LabelField($"プロセッサ: {SystemInfo.processorType}");
        EditorGUILayout.LabelField($"プロセッサ数: {SystemInfo.processorCount}");
        EditorGUILayout.LabelField($"システムメモリ: {SystemInfo.systemMemorySize}MB");
        EditorGUILayout.LabelField($"グラフィック: {SystemInfo.graphicsDeviceName}");
        EditorGUILayout.LabelField($"グラフィックメモリ: {SystemInfo.graphicsMemorySize}MB");
        EditorGUILayout.LabelField($"解像度: {Screen.width}x{Screen.height}");
        EditorGUILayout.LabelField($"DPI: {Screen.dpi}");
        
        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("リアルタイム情報", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"FPS: {1f / Time.deltaTime:F1}");
            EditorGUILayout.LabelField($"メモリ使用量: {System.GC.GetTotalMemory(false) / (1024 * 1024)}MB");
            EditorGUILayout.LabelField($"バッテリー: {SystemInfo.batteryLevel:P0}");
        }
    }
    
    private void DrawTestResults()
    {
        showTestResults = EditorGUILayout.Foldout(showTestResults, "テスト結果");
        
        if (!showTestResults) return;
        
        if (GUILayout.Button("テストレポートを読み込み"))
        {
            LoadLatestTestReport();
        }
        
        if (GUILayout.Button("テストレポートフォルダを開く"))
        {
            string reportPath = Application.persistentDataPath;
            EditorUtility.RevealInFinder(reportPath);
        }
        
        if (!string.IsNullOrEmpty(lastTestReport))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("最新のテストレポート", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.TextArea(lastTestReport, GUILayout.Height(200));
            EditorGUILayout.EndVertical();
        }
        
        // リアルタイムテスト状況
        if (Application.isPlaying)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("リアルタイムテスト状況", EditorStyles.boldLabel);
            
            if (testManager != null)
            {
                EditorGUILayout.LabelField($"統合テスト実行中: {(testManager.isTestRunning ? "はい" : "いいえ")}");
                EditorGUILayout.LabelField($"全テスト成功: {(testManager.allTestsPassed ? "はい" : "いいえ")}");
            }
            
            if (platformTester != null)
            {
                EditorGUILayout.LabelField($"プラットフォームテスト実行中: {(platformTester.isTestRunning ? "はい" : "いいえ")}");
                EditorGUILayout.LabelField($"全テスト成功: {(platformTester.allTestsPassed ? "はい" : "いいえ")}");
            }
        }
    }
    
    private void CreateTestComponents()
    {
        if (testRunner == null)
        {
            GameObject runnerObj = new GameObject("IntegrationTestRunner");
            testRunner = runnerObj.AddComponent<IntegrationTestRunner>();
            Debug.Log("IntegrationTestRunnerを作成しました");
        }
        
        if (testManager == null)
        {
            GameObject managerObj = new GameObject("IntegrationTestManager");
            testManager = managerObj.AddComponent<IntegrationTestManager>();
            Debug.Log("IntegrationTestManagerを作成しました");
        }
        
        if (platformTester == null)
        {
            GameObject testerObj = new GameObject("MobilePlatformTester");
            platformTester = testerObj.AddComponent<MobilePlatformTester>();
            Debug.Log("MobilePlatformTesterを作成しました");
        }
        
        if (balanceManager == null)
        {
            GameObject balanceObj = new GameObject("GameBalanceManager");
            balanceManager = balanceObj.AddComponent<GameBalanceManager>();
            Debug.Log("GameBalanceManagerを作成しました");
        }
        
        if (polishManager == null)
        {
            GameObject polishObj = new GameObject("FinalPolishManager");
            polishManager = polishObj.AddComponent<FinalPolishManager>();
            Debug.Log("FinalPolishManagerを作成しました");
        }
        
        if (requirementValidator == null)
        {
            GameObject validatorObj = new GameObject("RequirementValidator");
            requirementValidator = validatorObj.AddComponent<RequirementValidator>();
            Debug.Log("RequirementValidatorを作成しました");
        }
        
        if (buildPreparationManager == null)
        {
            GameObject buildObj = new GameObject("BuildPreparationManager");
            buildPreparationManager = buildObj.AddComponent<BuildPreparationManager>();
            Debug.Log("BuildPreparationManagerを作成しました");
        }
    }
    
    private void LoadLatestTestReport()
    {
        string reportPath = Application.persistentDataPath;
        string[] reportFiles = Directory.GetFiles(reportPath, "integration_test_report_*.txt");
        
        if (reportFiles.Length == 0)
        {
            lastTestReport = "テストレポートが見つかりません";
            return;
        }
        
        // 最新のファイルを取得
        string latestFile = reportFiles[0];
        System.DateTime latestTime = File.GetLastWriteTime(latestFile);
        
        foreach (string file in reportFiles)
        {
            System.DateTime fileTime = File.GetLastWriteTime(file);
            if (fileTime > latestTime)
            {
                latestFile = file;
                latestTime = fileTime;
            }
        }
        
        try
        {
            lastTestReport = File.ReadAllText(latestFile);
            Debug.Log($"テストレポートを読み込みました: {Path.GetFileName(latestFile)}");
        }
        catch (System.Exception e)
        {
            lastTestReport = $"テストレポートの読み込みに失敗しました: {e.Message}";
            Debug.LogError(lastTestReport);
        }
    }
    
    private void DrawPolishSettings()
    {
        showPolishSettings = EditorGUILayout.Foldout(showPolishSettings, "最終ポリッシュ設定");
        
        if (!showPolishSettings) return;
        
        if (polishManager == null)
        {
            EditorGUILayout.HelpBox("FinalPolishManagerが見つかりません", MessageType.Warning);
            return;
        }
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        
        if (GUILayout.Button("最終ポリッシュ実行", GUILayout.Height(25)))
        {
            polishManager.ExecuteManualPolish();
        }
        
        EditorGUI.EndDisabledGroup();
        
        if (Application.isPlaying && polishManager != null)
        {
            var polishStatus = polishManager.GetPolishStatus();
            
            EditorGUILayout.LabelField("ポリッシュ進捗", EditorStyles.boldLabel);
            
            int completedTasks = 0;
            int totalTasks = polishStatus.Count;
            
            foreach (var task in polishStatus)
            {
                string status = task.Value ? "完了" : "未完了";
                EditorGUILayout.LabelField($"{task.Key}: {status}");
                if (task.Value) completedTasks++;
            }
            
            float progress = totalTasks > 0 ? (float)completedTasks / totalTasks : 0f;
            EditorGUILayout.LabelField($"全体進捗: {completedTasks}/{totalTasks} ({progress:P0})");
            
            EditorGUILayout.Space();
            if (GUILayout.Button("ポリッシュレポートを読み込み"))
            {
                LoadLatestPolishReport();
            }
        }
        
        if (!string.IsNullOrEmpty(lastPolishReport))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("最新のポリッシュレポート", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.TextArea(lastPolishReport, GUILayout.Height(150));
            EditorGUILayout.EndVertical();
        }
    }
    
    private void DrawRequirementValidation()
    {
        showRequirementValidation = EditorGUILayout.Foldout(showRequirementValidation, "要件検証");
        
        if (!showRequirementValidation) return;
        
        if (requirementValidator == null)
        {
            EditorGUILayout.HelpBox("RequirementValidatorが見つかりません", MessageType.Warning);
            return;
        }
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        
        if (GUILayout.Button("全要件検証実行", GUILayout.Height(25)))
        {
            requirementValidator.ExecuteManualValidation();
        }
        
        EditorGUI.EndDisabledGroup();
        
        if (Application.isPlaying && requirementValidator != null)
        {
            float implementationRate = requirementValidator.GetImplementationRate();
            EditorGUILayout.LabelField($"実装率: {implementationRate:F1}%");
            
            EditorGUILayout.Space();
            if (GUILayout.Button("検証レポートを読み込み"))
            {
                LoadLatestValidationReport();
            }
        }
        
        if (!string.IsNullOrEmpty(lastValidationReport))
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("最新の検証レポート", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.TextArea(lastValidationReport, GUILayout.Height(150));
            EditorGUILayout.EndVertical();
        }
    }
    
    private void DrawBuildPreparation()
    {
        showBuildPreparation = EditorGUILayout.Foldout(showBuildPreparation, "ビルド準備");
        
        if (!showBuildPreparation) return;
        
        if (buildPreparationManager == null)
        {
            EditorGUILayout.HelpBox("BuildPreparationManagerが見つかりません", MessageType.Warning);
            return;
        }
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        
        if (GUILayout.Button("ビルド準備実行", GUILayout.Height(25)))
        {
            buildPreparationManager.ExecuteBuildPreparation();
        }
        
        EditorGUI.EndDisabledGroup();
        
        if (Application.isPlaying && buildPreparationManager != null)
        {
            var buildChecklist = buildPreparationManager.GetBuildChecklist();
            bool isReadyForRelease = buildPreparationManager.IsReadyForRelease();
            
            EditorGUILayout.LabelField("ビルドチェックリスト", EditorStyles.boldLabel);
            
            foreach (var item in buildChecklist)
            {
                string status = item.Value ? "完了" : "未完了";
                EditorGUILayout.LabelField($"{item.Key}: {status}");
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"リリース準備状況: {(isReadyForRelease ? "準備完了" : "準備中")}");
            
            if (isReadyForRelease)
            {
                EditorGUILayout.HelpBox("リリース準備が完了しました！", MessageType.Info);
            }
        }
    }
    
    private void LoadLatestPolishReport()
    {
        string reportPath = Application.persistentDataPath;
        string[] reportFiles = Directory.GetFiles(reportPath, "polish_report_*.txt");
        
        if (reportFiles.Length == 0)
        {
            lastPolishReport = "ポリッシュレポートが見つかりません";
            return;
        }
        
        string latestFile = GetLatestFile(reportFiles);
        
        try
        {
            lastPolishReport = File.ReadAllText(latestFile);
            Debug.Log($"ポリッシュレポートを読み込みました: {Path.GetFileName(latestFile)}");
        }
        catch (System.Exception e)
        {
            lastPolishReport = $"ポリッシュレポートの読み込みに失敗しました: {e.Message}";
            Debug.LogError(lastPolishReport);
        }
    }
    
    private void LoadLatestValidationReport()
    {
        string reportPath = Application.persistentDataPath;
        string[] reportFiles = Directory.GetFiles(reportPath, "requirement_validation_report_*.txt");
        
        if (reportFiles.Length == 0)
        {
            lastValidationReport = "検証レポートが見つかりません";
            return;
        }
        
        string latestFile = GetLatestFile(reportFiles);
        
        try
        {
            lastValidationReport = File.ReadAllText(latestFile);
            Debug.Log($"検証レポートを読み込みました: {Path.GetFileName(latestFile)}");
        }
        catch (System.Exception e)
        {
            lastValidationReport = $"検証レポートの読み込みに失敗しました: {e.Message}";
            Debug.LogError(lastValidationReport);
        }
    }
    
    private string GetLatestFile(string[] files)
    {
        string latestFile = files[0];
        System.DateTime latestTime = File.GetLastWriteTime(latestFile);
        
        foreach (string file in files)
        {
            System.DateTime fileTime = File.GetLastWriteTime(file);
            if (fileTime > latestTime)
            {
                latestFile = file;
                latestTime = fileTime;
            }
        }
        
        return latestFile;
    }
    
    private void OnInspectorUpdate()
    {
        // エディタウィンドウを定期的に更新
        if (Application.isPlaying)
        {
            Repaint();
        }
    }
}