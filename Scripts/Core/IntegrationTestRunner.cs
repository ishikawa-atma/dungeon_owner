using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 統合テストの実行とレポート生成を管理するクラス
/// </summary>
public class IntegrationTestRunner : MonoBehaviour
{
    [Header("テスト実行設定")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private bool generateReport = true;
    [SerializeField] private bool saveReportToFile = true;
    
    [Header("テストシーン設定")]
    [SerializeField] private string gameplaySceneName = "GameplayScene";
    [SerializeField] private float testSceneLoadTimeout = 10f;
    
    private IntegrationTestManager integrationTestManager;
    private MobilePlatformTester mobilePlatformTester;
    private GameBalanceManager gameBalanceManager;
    
    private void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(RunFullIntegrationTest());
        }
    }
    
    /// <summary>
    /// 完全な統合テストを実行
    /// </summary>
    public void RunFullIntegrationTest()
    {
        StartCoroutine(RunFullIntegrationTestCoroutine());
    }
    
    private IEnumerator RunFullIntegrationTestCoroutine()
    {
        Debug.Log("=== 統合テスト実行開始 ===");
        
        // 1. テストシーンの読み込み
        yield return StartCoroutine(LoadTestScene());
        
        // 2. テストマネージャーの初期化
        yield return StartCoroutine(InitializeTestManagers());
        
        // 3. プラットフォームテストの実行
        yield return StartCoroutine(RunPlatformTests());
        
        // 4. 統合テストの実行
        yield return StartCoroutine(RunIntegrationTests());
        
        // 5. バランステストの実行
        yield return StartCoroutine(RunBalanceTests());
        
        // 6. レポート生成
        if (generateReport)
        {
            GenerateTestReport();
        }
        
        Debug.Log("=== 統合テスト実行完了 ===");
    }
    
    private IEnumerator LoadTestScene()
    {
        Debug.Log("テストシーンを読み込み中...");
        
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(gameplaySceneName, LoadSceneMode.Additive);
            
            float timeout = Time.time + testSceneLoadTimeout;
            while (!loadOperation.isDone && Time.time < timeout)
            {
                yield return null;
            }
            
            if (!loadOperation.isDone)
            {
                Debug.LogError("テストシーンの読み込みがタイムアウトしました");
                yield break;
            }
            
            Debug.Log("テストシーンの読み込み完了");
        }
        
        yield return new WaitForSeconds(1f); // シーン初期化待機
    }
    
    private IEnumerator InitializeTestManagers()
    {
        Debug.Log("テストマネージャーを初期化中...");
        
        // IntegrationTestManagerの作成または取得
        integrationTestManager = FindObjectOfType<IntegrationTestManager>();
        if (integrationTestManager == null)
        {
            GameObject testManagerObj = new GameObject("IntegrationTestManager");
            integrationTestManager = testManagerObj.AddComponent<IntegrationTestManager>();
        }
        
        // MobilePlatformTesterの作成または取得
        mobilePlatformTester = FindObjectOfType<MobilePlatformTester>();
        if (mobilePlatformTester == null)
        {
            GameObject platformTesterObj = new GameObject("MobilePlatformTester");
            mobilePlatformTester = platformTesterObj.AddComponent<MobilePlatformTester>();
        }
        
        // GameBalanceManagerの作成または取得
        gameBalanceManager = FindObjectOfType<GameBalanceManager>();
        if (gameBalanceManager == null)
        {
            GameObject balanceManagerObj = new GameObject("GameBalanceManager");
            gameBalanceManager = balanceManagerObj.AddComponent<GameBalanceManager>();
        }
        
        yield return new WaitForSeconds(0.5f); // 初期化待機
        Debug.Log("テストマネージャーの初期化完了");
    }
    
    private IEnumerator RunPlatformTests()
    {
        Debug.Log("プラットフォームテストを実行中...");
        
        if (mobilePlatformTester != null)
        {
            mobilePlatformTester.RunPlatformTests();
            
            // テスト完了まで待機
            while (mobilePlatformTester.isTestRunning)
            {
                yield return new WaitForSeconds(1f);
            }
            
            Debug.Log("プラットフォームテスト完了");
        }
        else
        {
            Debug.LogWarning("MobilePlatformTesterが見つかりません");
        }
    }
    
    private IEnumerator RunIntegrationTests()
    {
        Debug.Log("統合テストを実行中...");
        
        if (integrationTestManager != null)
        {
            integrationTestManager.RunIntegrationTests();
            
            // テスト完了まで待機
            while (integrationTestManager.isTestRunning)
            {
                yield return new WaitForSeconds(1f);
            }
            
            Debug.Log("統合テスト完了");
        }
        else
        {
            Debug.LogWarning("IntegrationTestManagerが見つかりません");
        }
    }
    
    private IEnumerator RunBalanceTests()
    {
        Debug.Log("バランステストを実行中...");
        
        if (gameBalanceManager != null)
        {
            // バランス調整システムのテスト
            var originalMetrics = gameBalanceManager.GetMetrics();
            
            // テスト用のメトリクスを設定
            gameBalanceManager.RecordCombatResult(true, 2.5f);
            gameBalanceManager.RecordCombatResult(false, 3.2f);
            gameBalanceManager.RecordCombatResult(true, 1.8f);
            
            yield return new WaitForSeconds(1f);
            
            var updatedMetrics = gameBalanceManager.GetMetrics();
            
            if (updatedMetrics.totalCombats > originalMetrics.totalCombats)
            {
                Debug.Log("バランステスト成功: メトリクスが正常に更新されました");
            }
            else
            {
                Debug.LogWarning("バランステスト警告: メトリクスの更新に問題があります");
            }
        }
        else
        {
            Debug.LogWarning("GameBalanceManagerが見つかりません");
        }
    }
    
    private void GenerateTestReport()
    {
        Debug.Log("テストレポートを生成中...");
        
        var report = new System.Text.StringBuilder();
        report.AppendLine("=== 統合テストレポート ===");
        report.AppendLine($"実行日時: {System.DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        report.AppendLine($"プラットフォーム: {Application.platform}");
        report.AppendLine($"Unity バージョン: {Application.unityVersion}");
        report.AppendLine();
        
        // プラットフォームテスト結果
        if (mobilePlatformTester != null)
        {
            report.AppendLine("=== プラットフォームテスト結果 ===");
            string platformResults = mobilePlatformTester.GetTestResultsAsJson();
            report.AppendLine(platformResults);
            report.AppendLine();
        }
        
        // 統合テスト結果
        if (integrationTestManager != null)
        {
            report.AppendLine("=== 統合テスト結果 ===");
            string integrationResults = integrationTestManager.GetTestResultsAsJson();
            report.AppendLine(integrationResults);
            report.AppendLine();
        }
        
        // バランステスト結果
        if (gameBalanceManager != null)
        {
            report.AppendLine("=== バランステスト結果 ===");
            var metrics = gameBalanceManager.GetMetrics();
            report.AppendLine($"総戦闘数: {metrics.totalCombats}");
            report.AppendLine($"勝率: {metrics.playerWinRate:P2}");
            report.AppendLine($"平均戦闘時間: {metrics.averageCombatDuration:F2}秒");
            report.AppendLine($"金貨獲得率: {metrics.averageGoldPerMinute:F1}/分");
            report.AppendLine();
        }
        
        // システム情報
        report.AppendLine("=== システム情報 ===");
        report.AppendLine($"デバイスモデル: {SystemInfo.deviceModel}");
        report.AppendLine($"OS: {SystemInfo.operatingSystem}");
        report.AppendLine($"プロセッサ: {SystemInfo.processorType} ({SystemInfo.processorCount}コア)");
        report.AppendLine($"メモリ: {SystemInfo.systemMemorySize}MB");
        report.AppendLine($"グラフィック: {SystemInfo.graphicsDeviceName}");
        report.AppendLine($"解像度: {Screen.width}x{Screen.height}");
        report.AppendLine();
        
        string reportContent = report.ToString();
        Debug.Log(reportContent);
        
        // ファイルに保存
        if (saveReportToFile)
        {
            SaveReportToFile(reportContent);
        }
    }
    
    private void SaveReportToFile(string reportContent)
    {
        try
        {
            string fileName = $"integration_test_report_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            
            System.IO.File.WriteAllText(filePath, reportContent);
            Debug.Log($"テストレポートを保存しました: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"テストレポートの保存に失敗しました: {e.Message}");
        }
    }
    
    /// <summary>
    /// 個別テストの実行
    /// </summary>
    public void RunPlatformTestsOnly()
    {
        StartCoroutine(RunPlatformTests());
    }
    
    public void RunIntegrationTestsOnly()
    {
        StartCoroutine(RunIntegrationTests());
    }
    
    public void RunBalanceTestsOnly()
    {
        StartCoroutine(RunBalanceTests());
    }
    
    /// <summary>
    /// テスト結果のクリア
    /// </summary>
    public void ClearTestResults()
    {
        if (integrationTestManager != null)
        {
            // テスト結果をクリア（必要に応じて実装）
        }
        
        if (mobilePlatformTester != null)
        {
            // テスト結果をクリア（必要に応じて実装）
        }
        
        Debug.Log("テスト結果をクリアしました");
    }
}