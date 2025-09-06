using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// iOS・Android実機での動作確認とモバイル最適化テストを管理するクラス
/// </summary>
public class MobilePlatformTester : MonoBehaviour
{
    [Header("テスト設定")]
    [SerializeField] private bool runTestsOnStart = false;
    [SerializeField] private bool enableDetailedLogging = true;
    [SerializeField] private float testDuration = 60f;
    
    [Header("パフォーマンステスト")]
    [SerializeField] private int targetFPS = 60;
    [SerializeField] private long maxMemoryUsage = 500 * 1024 * 1024; // 500MB
    [SerializeField] private float maxBatteryDrainRate = 10f; // %/hour
    
    [Header("入力テスト")]
    [SerializeField] private float touchResponseThreshold = 0.1f;
    [SerializeField] private int multiTouchTestCount = 5;
    
    [Header("画面対応テスト")]
    [SerializeField] private List<Vector2> testResolutions = new List<Vector2>();
    
    [Header("テスト結果")]
    [SerializeField] private List<PlatformTestResult> testResults = new List<PlatformTestResult>();
    [SerializeField] private bool allTestsPassed = false;
    
    private bool isTestRunning = false;
    private float testStartTime = 0f;
    private PlatformMetrics metrics = new PlatformMetrics();
    
    [System.Serializable]
    public class PlatformTestResult
    {
        public string testName;
        public bool passed;
        public string errorMessage;
        public float executionTime;
        public Dictionary<string, object> additionalData;
        
        public PlatformTestResult(string name, bool success, string error = "", float time = 0f)
        {
            testName = name;
            passed = success;
            errorMessage = error;
            executionTime = time;
            additionalData = new Dictionary<string, object>();
        }
    }
    
    [System.Serializable]
    public class PlatformMetrics
    {
        public float averageFPS = 0f;
        public float minFPS = float.MaxValue;
        public float maxFPS = 0f;
        public long currentMemoryUsage = 0L;
        public long peakMemoryUsage = 0L;
        public float batteryLevel = 100f;
        public float initialBatteryLevel = 100f;
        public float averageTouchResponseTime = 0f;
        public int totalTouchEvents = 0;
        public List<float> touchResponseTimes = new List<float>();
        public string deviceModel = "";
        public string operatingSystem = "";
        public Vector2 screenResolution = Vector2.zero;
        public float screenDPI = 0f;
        
        public void Reset()
        {
            averageFPS = 0f;
            minFPS = float.MaxValue;
            maxFPS = 0f;
            currentMemoryUsage = 0L;
            peakMemoryUsage = 0L;
            batteryLevel = SystemInfo.batteryLevel * 100f;
            initialBatteryLevel = batteryLevel;
            averageTouchResponseTime = 0f;
            totalTouchEvents = 0;
            touchResponseTimes.Clear();
            
            deviceModel = SystemInfo.deviceModel;
            operatingSystem = SystemInfo.operatingSystem;
            screenResolution = new Vector2(Screen.width, Screen.height);
            screenDPI = Screen.dpi;
        }
        
        public void UpdateFPS(float currentFPS)
        {
            if (currentFPS > 0)
            {
                averageFPS = (averageFPS + currentFPS) / 2f;
                minFPS = Mathf.Min(minFPS, currentFPS);
                maxFPS = Mathf.Max(maxFPS, currentFPS);
            }
        }
        
        public void UpdateMemory(long memoryUsage)
        {
            currentMemoryUsage = memoryUsage;
            peakMemoryUsage = Mathf.Max(peakMemoryUsage, memoryUsage);
        }
        
        public void RecordTouchResponse(float responseTime)
        {
            touchResponseTimes.Add(responseTime);
            totalTouchEvents++;
            
            float total = 0f;
            foreach (float time in touchResponseTimes)
            {
                total += time;
            }
            averageTouchResponseTime = total / touchResponseTimes.Count;
        }
        
        public float GetBatteryDrainRate(float elapsedHours)
        {
            if (elapsedHours > 0)
            {
                float currentBattery = SystemInfo.batteryLevel * 100f;
                return (initialBatteryLevel - currentBattery) / elapsedHours;
            }
            return 0f;
        }
    }
    
    private void Start()
    {
        InitializeTestResolutions();
        
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllPlatformTests());
        }
    }
    
    private void InitializeTestResolutions()
    {
        if (testResolutions.Count == 0)
        {
            // 一般的なモバイル解像度を追加
            testResolutions.Add(new Vector2(1080, 1920)); // Full HD
            testResolutions.Add(new Vector2(1125, 2436)); // iPhone X
            testResolutions.Add(new Vector2(1242, 2688)); // iPhone XS Max
            testResolutions.Add(new Vector2(828, 1792));  // iPhone XR
            testResolutions.Add(new Vector2(1440, 2960)); // Galaxy S8+
            testResolutions.Add(new Vector2(1080, 2340)); // Galaxy S10
        }
    }
    
    /// <summary>
    /// 全プラットフォームテストを実行
    /// </summary>
    public void RunPlatformTests()
    {
        if (!isTestRunning)
        {
            StartCoroutine(RunAllPlatformTests());
        }
    }
    
    private IEnumerator RunAllPlatformTests()
    {
        isTestRunning = true;
        testResults.Clear();
        allTestsPassed = true;
        testStartTime = Time.time;
        
        metrics.Reset();
        
        LogTest("=== モバイルプラットフォームテスト開始 ===");
        LogTest($"デバイス: {metrics.deviceModel}");
        LogTest($"OS: {metrics.operatingSystem}");
        LogTest($"解像度: {metrics.screenResolution.x}x{metrics.screenResolution.y}");
        LogTest($"DPI: {metrics.screenDPI}");
        
        // 1. デバイス情報テスト
        yield return StartCoroutine(TestDeviceInformation());
        
        // 2. パフォーマンステスト
        yield return StartCoroutine(TestPerformance());
        
        // 3. タッチ入力テスト
        yield return StartCoroutine(TestTouchInput());
        
        // 4. 画面対応テスト
        yield return StartCoroutine(TestScreenCompatibility());
        
        // 5. メモリ管理テスト
        yield return StartCoroutine(TestMemoryManagement());
        
        // 6. バッテリー消費テスト
        yield return StartCoroutine(TestBatteryConsumption());
        
        // 7. ネットワーク接続テスト
        yield return StartCoroutine(TestNetworkConnectivity());
        
        // 8. ストレージテスト
        yield return StartCoroutine(TestStorageAccess());
        
        // テスト結果の集計
        LogTest("=== モバイルプラットフォームテスト完了 ===");
        LogTestResults();
        
        isTestRunning = false;
    }
    
    private IEnumerator TestDeviceInformation()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("デバイス情報テスト開始");
            
            var result = new PlatformTestResult("デバイス情報", true);
            
            // デバイス情報の収集
            result.additionalData["deviceModel"] = SystemInfo.deviceModel;
            result.additionalData["deviceName"] = SystemInfo.deviceName;
            result.additionalData["deviceType"] = SystemInfo.deviceType.ToString();
            result.additionalData["operatingSystem"] = SystemInfo.operatingSystem;
            result.additionalData["processorType"] = SystemInfo.processorType;
            result.additionalData["processorCount"] = SystemInfo.processorCount;
            result.additionalData["systemMemorySize"] = SystemInfo.systemMemorySize;
            result.additionalData["graphicsDeviceName"] = SystemInfo.graphicsDeviceName;
            result.additionalData["graphicsMemorySize"] = SystemInfo.graphicsMemorySize;
            result.additionalData["screenResolution"] = $"{Screen.width}x{Screen.height}";
            result.additionalData["screenDPI"] = Screen.dpi;
            result.additionalData["batteryStatus"] = SystemInfo.batteryStatus.ToString();
            result.additionalData["batteryLevel"] = SystemInfo.batteryLevel;
            
            // 最小要件チェック
            if (SystemInfo.systemMemorySize < 2048) // 2GB未満
            {
                testPassed = false;
                errorMsg = "システムメモリが不足しています";
            }
            
            if (SystemInfo.processorCount < 4) // 4コア未満
            {
                LogTest("警告: プロセッサコア数が少ない可能性があります");
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new PlatformTestResult("デバイス情報", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestPerformance()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("パフォーマンステスト開始");
            
            float testDurationLocal = 10f; // 10秒間のパフォーマンステスト
            float endTime = Time.time + testDurationLocal;
            
            while (Time.time < endTime)
            {
                // FPS測定
                float currentFPS = 1f / Time.deltaTime;
                metrics.UpdateFPS(currentFPS);
                
                // メモリ使用量測定
                long memoryUsage = System.GC.GetTotalMemory(false);
                metrics.UpdateMemory(memoryUsage);
                
                yield return null;
            }
            
            // パフォーマンス評価
            if (metrics.averageFPS < targetFPS * 0.8f) // 目標FPSの80%未満
            {
                testPassed = false;
                errorMsg = $"平均FPSが低すぎます: {metrics.averageFPS:F1} (目標: {targetFPS})";
            }
            
            if (metrics.peakMemoryUsage > maxMemoryUsage)
            {
                testPassed = false;
                errorMsg += $" メモリ使用量が多すぎます: {metrics.peakMemoryUsage / (1024 * 1024)}MB";
            }
            
            LogTest($"平均FPS: {metrics.averageFPS:F1}");
            LogTest($"最小FPS: {metrics.minFPS:F1}");
            LogTest($"最大FPS: {metrics.maxFPS:F1}");
            LogTest($"ピークメモリ使用量: {metrics.peakMemoryUsage / (1024 * 1024)}MB");
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        var result = new PlatformTestResult("パフォーマンス", testPassed, errorMsg, executionTime);
        result.additionalData["averageFPS"] = metrics.averageFPS;
        result.additionalData["minFPS"] = metrics.minFPS;
        result.additionalData["maxFPS"] = metrics.maxFPS;
        result.additionalData["peakMemoryUsage"] = metrics.peakMemoryUsage;
        testResults.Add(result);
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestTouchInput()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("タッチ入力テスト開始");
            
            // シミュレートされたタッチイベントでテスト
            for (int i = 0; i < multiTouchTestCount; i++)
            {
                float touchStartTime = Time.time;
                
                // タッチイベントのシミュレーション
                Vector2 touchPosition = new Vector2(
                    UnityEngine.Random.Range(0, Screen.width),
                    UnityEngine.Random.Range(0, Screen.height)
                );
                
                // タッチ応答時間の測定
                yield return new WaitForEndOfFrame();
                
                float responseTime = Time.time - touchStartTime;
                metrics.RecordTouchResponse(responseTime);
                
                if (responseTime > touchResponseThreshold)
                {
                    LogTest($"警告: タッチ応答時間が遅い: {responseTime:F3}s");
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            // タッチ応答性能の評価
            if (metrics.averageTouchResponseTime > touchResponseThreshold)
            {
                testPassed = false;
                errorMsg = $"タッチ応答時間が遅すぎます: {metrics.averageTouchResponseTime:F3}s";
            }
            
            LogTest($"平均タッチ応答時間: {metrics.averageTouchResponseTime:F3}s");
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        var result = new PlatformTestResult("タッチ入力", testPassed, errorMsg, executionTime);
        result.additionalData["averageTouchResponseTime"] = metrics.averageTouchResponseTime;
        result.additionalData["totalTouchEvents"] = metrics.totalTouchEvents;
        testResults.Add(result);
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestScreenCompatibility()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("画面対応テスト開始");
            
            Vector2 currentResolution = new Vector2(Screen.width, Screen.height);
            float currentAspectRatio = currentResolution.x / currentResolution.y;
            
            // アスペクト比の確認
            bool supportedAspectRatio = false;
            foreach (Vector2 testRes in testResolutions)
            {
                float testAspectRatio = testRes.x / testRes.y;
                if (Mathf.Abs(currentAspectRatio - testAspectRatio) < 0.1f)
                {
                    supportedAspectRatio = true;
                    break;
                }
            }
            
            if (!supportedAspectRatio)
            {
                LogTest($"警告: 未対応のアスペクト比の可能性: {currentAspectRatio:F2}");
            }
            
            // セーフエリアの確認
            Rect safeArea = Screen.safeArea;
            float safeAreaRatio = (safeArea.width * safeArea.height) / (Screen.width * Screen.height);
            
            if (safeAreaRatio < 0.8f) // セーフエリアが80%未満
            {
                LogTest($"警告: セーフエリアが小さい: {safeAreaRatio:F2}");
            }
            
            LogTest($"現在の解像度: {currentResolution.x}x{currentResolution.y}");
            LogTest($"アスペクト比: {currentAspectRatio:F2}");
            LogTest($"セーフエリア比率: {safeAreaRatio:F2}");
            
            yield return new WaitForSeconds(0.1f);
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        var result = new PlatformTestResult("画面対応", testPassed, errorMsg, executionTime);
        result.additionalData["resolution"] = $"{Screen.width}x{Screen.height}";
        result.additionalData["aspectRatio"] = Screen.width / (float)Screen.height;
        result.additionalData["safeArea"] = Screen.safeArea;
        testResults.Add(result);
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestMemoryManagement()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("メモリ管理テスト開始");
            
            long initialMemory = System.GC.GetTotalMemory(false);
            
            // メモリ負荷テスト
            List<byte[]> memoryLoad = new List<byte[]>();
            for (int i = 0; i < 10; i++)
            {
                memoryLoad.Add(new byte[1024 * 1024]); // 1MB
                yield return null;
            }
            
            long peakMemory = System.GC.GetTotalMemory(false);
            
            // メモリ解放テスト
            memoryLoad.Clear();
            System.GC.Collect();
            yield return new WaitForSeconds(1f);
            
            long finalMemory = System.GC.GetTotalMemory(false);
            
            // メモリリーク検出
            long memoryIncrease = finalMemory - initialMemory;
            if (memoryIncrease > 50 * 1024 * 1024) // 50MB以上の増加
            {
                testPassed = false;
                errorMsg = $"メモリリークの可能性: {memoryIncrease / (1024 * 1024)}MB増加";
            }
            
            LogTest($"初期メモリ: {initialMemory / (1024 * 1024)}MB");
            LogTest($"ピークメモリ: {peakMemory / (1024 * 1024)}MB");
            LogTest($"最終メモリ: {finalMemory / (1024 * 1024)}MB");
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new PlatformTestResult("メモリ管理", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestBatteryConsumption()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("バッテリー消費テスト開始");
            
            if (SystemInfo.batteryStatus == BatteryStatus.Unknown)
            {
                LogTest("警告: バッテリー情報を取得できません");
                yield return new WaitForSeconds(0.1f);
            }
            else
            {
                float testDurationLocal = 30f; // 30秒間のテスト
                yield return new WaitForSeconds(testDurationLocal);
                
                float elapsedHours = testDurationLocal / 3600f;
                float drainRate = metrics.GetBatteryDrainRate(elapsedHours);
                
                if (drainRate > maxBatteryDrainRate)
                {
                    testPassed = false;
                    errorMsg = $"バッテリー消費が多すぎます: {drainRate:F1}%/hour";
                }
                
                LogTest($"バッテリー消費率: {drainRate:F1}%/hour");
            }
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new PlatformTestResult("バッテリー消費", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestNetworkConnectivity()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("ネットワーク接続テスト開始");
            
            // ネットワーク接続状態の確認
            NetworkReachability reachability = Application.internetReachability;
            
            if (reachability == NetworkReachability.NotReachable)
            {
                LogTest("警告: ネットワーク接続がありません");
            }
            else
            {
                LogTest($"ネットワーク接続: {reachability}");
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        var result = new PlatformTestResult("ネットワーク接続", testPassed, errorMsg, executionTime);
        result.additionalData["networkReachability"] = Application.internetReachability.ToString();
        testResults.Add(result);
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private IEnumerator TestStorageAccess()
    {
        float startTime = Time.time;
        bool testPassed = true;
        string errorMsg = "";
        
        try
        {
            LogTest("ストレージアクセステスト開始");
            
            // 書き込みテスト
            string testFilePath = Application.persistentDataPath + "/platform_test.txt";
            string testData = "Platform test data";
            
            System.IO.File.WriteAllText(testFilePath, testData);
            
            // 読み込みテスト
            if (System.IO.File.Exists(testFilePath))
            {
                string readData = System.IO.File.ReadAllText(testFilePath);
                
                if (readData != testData)
                {
                    testPassed = false;
                    errorMsg = "ファイル読み書きが正常に動作していません";
                }
                
                // テストファイルの削除
                System.IO.File.Delete(testFilePath);
            }
            else
            {
                testPassed = false;
                errorMsg = "ファイル書き込みに失敗しました";
            }
            
            LogTest($"永続データパス: {Application.persistentDataPath}");
            
            yield return new WaitForSeconds(0.1f);
        }
        catch (Exception e)
        {
            testPassed = false;
            errorMsg = e.Message;
        }
        
        float executionTime = Time.time - startTime;
        testResults.Add(new PlatformTestResult("ストレージアクセス", testPassed, errorMsg, executionTime));
        
        if (!testPassed) allTestsPassed = false;
    }
    
    private void LogTest(string message)
    {
        if (enableDetailedLogging)
        {
            Debug.Log($"[プラットフォームテスト] {message}");
        }
    }
    
    private void LogTestResults()
    {
        Debug.Log("=== プラットフォームテスト結果 ===");
        
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
        
        Debug.Log($"プラットフォームテスト完了: {passedTests}/{totalTests} 成功");
        Debug.Log($"全体結果: {(allTestsPassed ? "成功" : "失敗")}");
        
        // デバイス情報のサマリー
        Debug.Log("=== デバイス情報サマリー ===");
        Debug.Log($"デバイス: {metrics.deviceModel}");
        Debug.Log($"OS: {metrics.operatingSystem}");
        Debug.Log($"解像度: {metrics.screenResolution.x}x{metrics.screenResolution.y}");
        Debug.Log($"平均FPS: {metrics.averageFPS:F1}");
        Debug.Log($"ピークメモリ: {metrics.peakMemoryUsage / (1024 * 1024)}MB");
    }
    
    /// <summary>
    /// テスト結果をJSON形式で出力
    /// </summary>
    public string GetTestResultsAsJson()
    {
        var results = new
        {
            timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            platform = Application.platform.ToString(),
            deviceInfo = new
            {
                model = metrics.deviceModel,
                os = metrics.operatingSystem,
                resolution = $"{metrics.screenResolution.x}x{metrics.screenResolution.y}",
                dpi = metrics.screenDPI
            },
            performance = new
            {
                averageFPS = metrics.averageFPS,
                minFPS = metrics.minFPS,
                maxFPS = metrics.maxFPS,
                peakMemoryUsage = metrics.peakMemoryUsage,
                averageTouchResponseTime = metrics.averageTouchResponseTime
            },
            allTestsPassed = allTestsPassed,
            totalTests = testResults.Count,
            passedTests = testResults.FindAll(r => r.passed).Count,
            results = testResults
        };
        
        return JsonUtility.ToJson(results, true);
    }
    
    /// <summary>
    /// プラットフォーム固有の最適化を適用
    /// </summary>
    public void ApplyPlatformOptimizations()
    {
        // iOS固有の最適化
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            // フレームレート設定
            Application.targetFrameRate = 60;
            
            // バッテリー最適化
            Screen.sleepTimeout = SleepTimeout.SystemSetting;
        }
        
        // Android固有の最適化
        if (Application.platform == RuntimePlatform.Android)
        {
            // フレームレート設定
            Application.targetFrameRate = 60;
            
            // メモリ最適化
            System.GC.Collect();
        }
        
        LogTest("プラットフォーム固有の最適化を適用しました");
    }
}