using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 簡単なビルドテストを実行するエディタスクリプト
/// </summary>
public class QuickBuildTest : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> testResults = new List<string>();
    private bool isTestRunning = false;

    [MenuItem("Tools/Quick Build Test")]
    public static void ShowWindow()
    {
        GetWindow<QuickBuildTest>("Quick Build Test");
    }

    private void OnGUI()
    {
        GUILayout.Label("Quick Build Test", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Run Compile Test", GUILayout.Height(30)))
        {
            RunCompileTest();
        }

        if (GUILayout.Button("Run Basic Functionality Test", GUILayout.Height(30)))
        {
            RunBasicFunctionalityTest();
        }

        if (GUILayout.Button("Run Full Integration Test", GUILayout.Height(30)))
        {
            RunFullIntegrationTest();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Clear Results"))
        {
            testResults.Clear();
        }

        GUILayout.Space(10);
        GUILayout.Label("Test Results:", EditorStyles.boldLabel);

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        foreach (string result in testResults)
        {
            GUILayout.Label(result, EditorStyles.wordWrappedLabel);
        }
        GUILayout.EndScrollView();
    }

    private void RunCompileTest()
    {
        testResults.Clear();
        AddResult("=== コンパイルテスト開始 ===");

        // スクリプトの再コンパイルを強制
        AssetDatabase.Refresh();
        
        // コンパイルエラーをチェック
        var compilationMessages = UnityEditor.Compilation.CompilationPipeline.GetAssemblies();
        
        AddResult($"アセンブリ数: {compilationMessages.Length}");
        AddResult("コンパイルテスト完了 - エラーがないことを確認してください");
        
        // 重要なクラスの存在確認
        CheckClassExists("DungeonOwner.Core.GameManager");
        CheckClassExists("DungeonOwner.Core.FloorSystem");
        CheckClassExists("DungeonOwner.Core.TimeManager");
        CheckClassExists("DungeonOwner.Data.SaveData");

        AddResult("=== コンパイルテスト完了 ===");
    }

    private void RunBasicFunctionalityTest()
    {
        testResults.Clear();
        AddResult("=== 基本機能テスト開始 ===");

        // GameManagerの確認
        var gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            AddResult("✅ GameManager: 存在確認");
            AddResult($"   現在の状態: {gameManager.CurrentState}");
            AddResult($"   現在の階層: {gameManager.CurrentFloor}");
        }
        else
        {
            AddResult("❌ GameManager: 見つかりません");
        }

        // FloorSystemの確認
        var floorSystem = FindObjectOfType<FloorSystem>();
        if (floorSystem != null)
        {
            AddResult("✅ FloorSystem: 存在確認");
            AddResult($"   階層数: {floorSystem.CurrentFloorCount}");
            AddResult($"   最大階層: {floorSystem.MaxFloors}");
        }
        else
        {
            AddResult("❌ FloorSystem: 見つかりません");
        }

        // TimeManagerの確認
        var timeManager = FindObjectOfType<TimeManager>();
        if (timeManager != null)
        {
            AddResult("✅ TimeManager: 存在確認");
            AddResult($"   現在の日: {timeManager.CurrentDay}");
            AddResult($"   速度倍率: {timeManager.CurrentSpeedMultiplier}");
        }
        else
        {
            AddResult("❌ TimeManager: 見つかりません");
        }

        AddResult("=== 基本機能テスト完了 ===");
    }

    private void RunFullIntegrationTest()
    {
        testResults.Clear();
        AddResult("=== 統合テスト開始 ===");

        // IntegrationTestRunnerを探すか作成
        var testRunner = FindObjectOfType<IntegrationTestRunner>();
        if (testRunner == null)
        {
            GameObject testRunnerObj = new GameObject("IntegrationTestRunner");
            testRunner = testRunnerObj.AddComponent<IntegrationTestRunner>();
            AddResult("IntegrationTestRunnerを作成しました");
        }

        if (testRunner != null)
        {
            testRunner.RunFullIntegrationTest();
            AddResult("統合テストを開始しました");
            AddResult("詳細な結果はConsoleウィンドウを確認してください");
        }
        else
        {
            AddResult("❌ IntegrationTestRunnerの作成に失敗しました");
        }

        AddResult("=== 統合テスト完了 ===");
    }

    private void CheckClassExists(string className)
    {
        var type = System.Type.GetType(className);
        if (type != null)
        {
            AddResult($"✅ クラス存在確認: {className}");
        }
        else
        {
            AddResult($"❌ クラス見つからず: {className}");
        }
    }

    private void AddResult(string result)
    {
        testResults.Add($"[{System.DateTime.Now:HH:mm:ss}] {result}");
        Debug.Log(result);
    }
}