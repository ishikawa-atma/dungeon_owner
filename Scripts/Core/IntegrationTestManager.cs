using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 全システムの統合テストを管理するクラス
/// 各システムテスターを統合し、全体的なゲームフローをテストする
/// </summary>
public class IntegrationTestManager : MonoBehaviour
{
    [Header("テスト設定")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private bool enableDetailedLogging = true;
    [SerializeField] private float testTimeout = 30f;
    
    [Header("システムテスター参照")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private FloorSystemTester floorSystemTester;
    [SerializeField] private CombatSystemTester combatSystemTester;
    [SerializeField] private EconomySystemTester economySystemTester;
    [SerializeField] private PartySystemTester partySystemTester;
    [SerializeField] private BossSystemTester bossSystemTester;
    [SerializeField] private SaveSystemTester saveSystemTester;
    [SerializeField] private TutorialSystemTester tutorialSystemTester;
    
    [Header("テスト結果")]
    [SerializeField] private List<TestResult> testResults = new List<TestResult>();
    [SerializeField] private bool allTestsPassed = false;
    
    private bool isTestRunning = false;
    
    [System.Serializable]
    public class TestResult
    {
        public string testName;
        public bool passed;
        public string errorMessage;
        public float executionTime;
        
        public TestResult(string name, bool success, string error = "", float time = 0f)
        {
            testName = name;
            passed = success;
            errorMessage = error;
            executionTime = time;
        }
    }
    
    private void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllIntegrationTests());
        }
    }
    
    /// <summary>
    /// 全統合テストを実行
    /// </summary>
    public void RunIntegrationTests()
    {
        if (!isTestRunning)
        {
            StartCoroutine(RunAllIntegrationTests());
        }
    }
    
    private IEnumerator RunAllIntegrationTests()
    {
        isTestRunning = true;
        testResults.Clear();
        allTestsPassed = true;
        
        LogTest("=== 統合テスト開始 ===");
        
        // 1. 基本システム初期化テスト
        yield return StartCoroutine(TestSystemInitialization());
        
        // 2. ゲームフロー統合テスト
        yield return StartCoroutine(TestGameFlowIntegration());
        
        // 3. 戦闘システム統合テスト
        yield return StartCoroutine(TestCombatIntegration());
        
        // 4. 経済システム統合テスト
        yield return StartCoroutine(TestEconomyIntegration());
        
        // 5. パーティシステム統合テスト
        yield return StartCoroutine(TestPartyIntegration());
        
        // 6. セーブ・ロード統合テスト
        yield return StartCoroutine(TestSaveLoadIntegration());
        
        // 7. パフォーマンステスト
        yield return StartCoroutine(TestPerformanceIntegration());
        
        // 8. UI統合テスト
        yield return StartCoroutine(TestUIIntegration());
        
        // テスト結果の集計
        LogTest("=== 統合テスト完了 ===");
        LogTestResults();
        
        isTestRunning = false;
    }
    
    private IEnumerator TestSystemInitialization()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("システム初期化テスト開始");
            
            // GameManagerの初期化確認
            if (gameManager == null || !gameManager.IsInitialized)
            {
                testPassed = false;
                errorMsg = "GameManagerが初期化されていません";
            }
            
            // 各システムの初期化確認
            if (testPassed && !ValidateSystemInitialization())
            {
                testPassed = false;
                errorMsg = "一部システムの初期化に失敗しました";
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new TestResult("システム初期化", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestGameFlowIntegration()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("ゲームフロー統合テスト開始");
            
            // 初期状態の確認
            if (!gameManager.IsGameReady())
            {
                testPassed = false;
                errorMsg = "ゲームが開始可能状態ではありません";
            }
            
            // ゲーム開始フローのテスト
            if (testPassed)
            {
                gameManager.StartGame();
                yield return new WaitForSeconds(1f);
                
                if (gameManager.CurrentState != GameState.Playing)
                {
                    testPassed = false;
                    errorMsg = "ゲーム開始後の状態が正しくありません";
                }
            }
            
            // 時間制御システムのテスト
            if (testPassed)
            {
                gameManager.SetGameSpeed(2f);
                yield return new WaitForSeconds(0.5f);
                
                if (Mathf.Abs(gameManager.GameSpeed - 2f) > 0.1f)
                {
                    testPassed = false;
                    errorMsg = "時間制御システムが正常に動作していません";
                }
            }
            
            // ゲーム一時停止・再開のテスト
            if (testPassed)
            {
                gameManager.PauseGame();
                yield return new WaitForSeconds(0.2f);
                
                if (gameManager.CurrentState != GameState.Paused)
                {
                    testPassed = false;
                    errorMsg = "ゲーム一時停止が正常に動作していません";
                }
                
                gameManager.ResumeGame();
                yield return new WaitForSeconds(0.2f);
                
                if (gameManager.CurrentState != GameState.Playing)
                {
                    testPassed = false;
                    errorMsg = "ゲーム再開が正常に動作していません";
                }
            }
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new TestResult("ゲームフロー統合", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestCombatIntegration()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("戦闘システム統合テスト開始");
            
            if (combatSystemTester != null)
            {
                // 戦闘システムテスターを実行
                yield return StartCoroutine(combatSystemTester.RunAllTests());
                
                if (!combatSystemTester.AllTestsPassed)
                {
                    testPassed = false;
                    errorMsg = "戦闘システムテストに失敗しました";
                }
            }
            else
            {
                testPassed = false;
                errorMsg = "CombatSystemTesterが見つかりません";
            }
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new TestResult("戦闘システム統合", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestEconomyIntegration()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("経済システム統合テスト開始");
            
            if (economySystemTester != null)
            {
                // 経済システムテスターを実行
                yield return StartCoroutine(economySystemTester.RunAllTests());
                
                if (!economySystemTester.AllTestsPassed)
                {
                    testPassed = false;
                    errorMsg = "経済システムテストに失敗しました";
                }
            }
            else
            {
                testPassed = false;
                errorMsg = "EconomySystemTesterが見つかりません";
            }
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new TestResult("経済システム統合", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestPartyIntegration()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("パーティシステム統合テスト開始");
            
            if (partySystemTester != null)
            {
                // パーティシステムテスターを実行
                yield return StartCoroutine(partySystemTester.RunAllTests());
                
                if (!partySystemTester.AllTestsPassed)
                {
                    testPassed = false;
                    errorMsg = "パーティシステムテストに失敗しました";
                }
            }
            else
            {
                testPassed = false;
                errorMsg = "PartySystemTesterが見つかりません";
            }
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new TestResult("パーティシステム統合", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestSaveLoadIntegration()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("セーブ・ロード統合テスト開始");
            
            if (saveSystemTester != null)
            {
                // セーブシステムテスターを実行
                yield return StartCoroutine(saveSystemTester.RunAllTests());
                
                if (!saveSystemTester.AllTestsPassed)
                {
                    testPassed = false;
                    errorMsg = "セーブ・ロードシステムテストに失敗しました";
                }
            }
            else
            {
                testPassed = false;
                errorMsg = "SaveSystemTesterが見つかりません";
            }
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new TestResult("セーブ・ロード統合", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestPerformanceIntegration()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("パフォーマンス統合テスト開始");
            
            // フレームレートテスト
            float averageFPS = PerformanceMonitor.Instance.GetAverageFPS();
            if (averageFPS < 50f)
            {
                testPassed = false;
                errorMsg = $"フレームレートが低すぎます: {averageFPS:F1} FPS";
            }
            
            // メモリ使用量テスト
            long memoryUsage = MemoryOptimizer.Instance.GetCurrentMemoryUsage();
            if (memoryUsage > 500 * 1024 * 1024) // 500MB制限
            {
                testPassed = false;
                errorMsg = $"メモリ使用量が多すぎます: {memoryUsage / (1024 * 1024)} MB";
            }
            
            yield return new WaitForSeconds(1f);
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new TestResult("パフォーマンス統合", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestUIIntegration()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("UI統合テスト開始");
            
            // UIManagerの確認
            UIManager uiManager = FindObjectOfType<UIManager>();
            if (uiManager == null)
            {
                testPassed = false;
                errorMsg = "UIManagerが見つかりません";
            }
            
            // 主要UIの存在確認
            if (testPassed)
            {
                if (!ValidateUIComponents())
                {
                    testPassed = false;
                    errorMsg = "必要なUIコンポーネントが不足しています";
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new TestResult("UI統合", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private bool ValidateSystemInitialization()
    {
        // 各システムの初期化状態を確認
        return FindObjectOfType<FloorSystem>() != null &&
               FindObjectOfType<ResourceManager>() != null &&
               FindObjectOfType<InvaderSpawner>() != null &&
               FindObjectOfType<PartyManager>() != null;
    }
    
    private bool ValidateUIComponents()
    {
        // 主要UIコンポーネントの存在確認
        return FindObjectOfType<ResourceDisplayUI>() != null &&
               FindObjectOfType<MonsterPlacementUI>() != null &&
               FindObjectOfType<SpeedControlUI>() != null;
    }
    
    private void LogTest(string message)
    {
        if (enableDetailedLogging)
        {
            Debug.Log($"[統合テスト] {message}");
        }
    }
    
    private void LogTestResults()
    {
        Debug.Log("=== 統合テスト結果 ===");
        
        int passedTests = 0;
        int totalTests = testResults.Count;
        
        foreach (var result in testResults)
        {
            string status = result.passed ? "PASS" : "FAIL";
            Debug.Log($"[{status}] {result.testName} ({result.executionTime:F2}s)");
            
            if (!result.passed && !string.IsNullOrEmpty(result.errorMessage))
            {
                Debug.LogError($"  エラー: {result.errorMessage}");
            }
            
            if (result.passed) passedTests++;
        }
        
        Debug.Log($"統合テスト完了: {passedTests}/{totalTests} 成功");
        Debug.Log($"全体結果: {(allTestsPassed ? "成功" : "失敗")}");
    }
    
    /// <summary>
    /// テスト結果をJSON形式で出力
    /// </summary>
    public string GetTestResultsAsJson()
    {
        var results = new
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            allTestsPassed = allTestsPassed,
            totalTests = testResults.Count,
            passedTests = testResults.FindAll(r => r.passed).Count,
            results = testResults
        };
        
        return JsonUtility.ToJson(results, true);
    }
}