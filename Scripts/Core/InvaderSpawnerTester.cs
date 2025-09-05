using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Managers;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 侵入者ランダム出現システムのテスター
    /// 要件18.1-18.4の動作確認用
    /// </summary>
    public class InvaderSpawnerTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableAutoTest = false;
        [SerializeField] private float testDuration = 60f; // テスト実行時間（秒）
        [SerializeField] private bool logDetailedInfo = true;

        [Header("Test Controls")]
        [SerializeField] private KeyCode forceSpawnKey = KeyCode.S;
        [SerializeField] private KeyCode forcePartySpawnKey = KeyCode.P;
        [SerializeField] private KeyCode printInfoKey = KeyCode.I;
        [SerializeField] private KeyCode toggleSpawningKey = KeyCode.T;

        [Header("Test Results")]
        [SerializeField] private int totalSpawned = 0;
        [SerializeField] private int partiesSpawned = 0;
        [SerializeField] private float averageSpawnInterval = 0f;
        [SerializeField] private float lastSpawnTime = 0f;

        private bool testRunning = false;
        private float testStartTime = 0f;
        private float lastRecordedSpawnTime = 0f;
        private int spawnCount = 0;

        private void Start()
        {
            if (enableAutoTest)
            {
                StartTest();
            }

            // InvaderSpawnerのイベントに登録
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.OnInvaderSpawned += OnInvaderSpawned;
                InvaderSpawner.Instance.OnInvaderDefeated += OnInvaderDefeated;
            }
        }

        private void OnDestroy()
        {
            // イベント購読を解除
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.OnInvaderSpawned -= OnInvaderSpawned;
                InvaderSpawner.Instance.OnInvaderDefeated -= OnInvaderDefeated;
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateTestProgress();
        }

        private void HandleInput()
        {
            // 強制出現テスト
            if (Input.GetKeyDown(forceSpawnKey))
            {
                ForceSpawnInvader();
            }

            // 強制パーティ出現テスト
            if (Input.GetKeyDown(forcePartySpawnKey))
            {
                ForceSpawnParty();
            }

            // 情報表示
            if (Input.GetKeyDown(printInfoKey))
            {
                PrintTestInfo();
            }

            // スポーン切り替え
            if (Input.GetKeyDown(toggleSpawningKey))
            {
                ToggleSpawning();
            }
        }

        private void UpdateTestProgress()
        {
            if (!testRunning) return;

            // テスト時間終了チェック
            if (Time.time - testStartTime >= testDuration)
            {
                EndTest();
            }
        }

        public void StartTest()
        {
            if (testRunning)
            {
                Debug.LogWarning("Test is already running");
                return;
            }

            testRunning = true;
            testStartTime = Time.time;
            totalSpawned = 0;
            partiesSpawned = 0;
            spawnCount = 0;
            lastRecordedSpawnTime = Time.time;

            Debug.Log($"=== Invader Spawner Test Started ===");
            Debug.Log($"Test Duration: {testDuration} seconds");
            Debug.Log($"Current Floor Count: {(FloorSystem.Instance != null ? FloorSystem.Instance.CurrentFloorCount : 0)}");

            // ゲーム状態を確認
            if (GameManager.Instance != null)
            {
                Debug.Log($"Game State: {GameManager.Instance.CurrentState}");
                if (GameManager.Instance.CurrentState != GameState.Playing)
                {
                    Debug.LogWarning("Game is not in Playing state. Starting game...");
                    GameManager.Instance.StartGame();
                }
            }
        }

        public void EndTest()
        {
            if (!testRunning)
            {
                Debug.LogWarning("No test is running");
                return;
            }

            testRunning = false;
            float testTime = Time.time - testStartTime;

            Debug.Log($"=== Invader Spawner Test Completed ===");
            Debug.Log($"Test Duration: {testTime:F2} seconds");
            Debug.Log($"Total Invaders Spawned: {totalSpawned}");
            Debug.Log($"Parties Spawned: {partiesSpawned}");
            Debug.Log($"Average Spawn Interval: {(spawnCount > 1 ? testTime / (spawnCount - 1) : 0):F2} seconds");
            Debug.Log($"Spawn Rate: {totalSpawned / testTime:F2} invaders/second");

            PrintTestInfo();
        }

        private void OnInvaderSpawned(GameObject invader)
        {
            if (!testRunning) return;

            totalSpawned++;
            spawnCount++;

            // パーティかどうかチェック
            if (invader.CompareTag("InvaderParty"))
            {
                partiesSpawned++;
            }

            // 出現間隔を記録
            float currentTime = Time.time;
            if (lastRecordedSpawnTime > 0)
            {
                float interval = currentTime - lastRecordedSpawnTime;
                averageSpawnInterval = ((averageSpawnInterval * (spawnCount - 2)) + interval) / (spawnCount - 1);
            }
            lastRecordedSpawnTime = currentTime;
            lastSpawnTime = currentTime;

            if (logDetailedInfo)
            {
                Debug.Log($"Invader spawned: {invader.name} (Total: {totalSpawned})");
            }
        }

        private void OnInvaderDefeated(GameObject invader)
        {
            if (logDetailedInfo)
            {
                Debug.Log($"Invader defeated: {invader.name}");
            }
        }

        private void ForceSpawnInvader()
        {
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.SpawnRandomInvader();
                Debug.Log("Forced invader spawn");
            }
            else
            {
                Debug.LogError("InvaderSpawner.Instance is null");
            }
        }

        private void ForceSpawnParty()
        {
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.SpawnRandomInvaderParty();
                Debug.Log("Forced party spawn");
            }
            else
            {
                Debug.LogError("InvaderSpawner.Instance is null");
            }
        }

        private void ToggleSpawning()
        {
            if (InvaderSpawner.Instance != null)
            {
                if (GameManager.Instance != null)
                {
                    if (GameManager.Instance.CurrentState == GameState.Playing)
                    {
                        GameManager.Instance.PauseGame();
                        Debug.Log("Spawning paused");
                    }
                    else
                    {
                        GameManager.Instance.ResumeGame();
                        Debug.Log("Spawning resumed");
                    }
                }
            }
        }

        private void PrintTestInfo()
        {
            Debug.Log($"=== Current Test Status ===");
            Debug.Log($"Test Running: {testRunning}");
            Debug.Log($"Test Time: {(testRunning ? Time.time - testStartTime : 0):F2}s");
            Debug.Log($"Total Spawned: {totalSpawned}");
            Debug.Log($"Parties Spawned: {partiesSpawned}");
            Debug.Log($"Average Interval: {averageSpawnInterval:F2}s");
            Debug.Log($"Last Spawn: {(lastSpawnTime > 0 ? Time.time - lastSpawnTime : 0):F2}s ago");

            // InvaderSpawnerの詳細情報
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.DebugPrintSpawnerInfo();
            }

            // FloorSystemの情報
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.DebugPrintFloorInfo();
            }
        }

        /// <summary>
        /// 階層拡張テスト
        /// 階層深度に応じた出現頻度調整をテスト
        /// </summary>
        [ContextMenu("Test Floor Expansion")]
        public void TestFloorExpansion()
        {
            if (FloorSystem.Instance != null)
            {
                Debug.Log("Testing floor expansion effects on spawn frequency...");
                
                // 現在の状態を記録
                PrintTestInfo();
                
                // 階層を拡張
                for (int i = 0; i < 3; i++)
                {
                    if (FloorSystem.Instance.CanExpandFloor())
                    {
                        FloorSystem.Instance.ExpandFloor();
                        Debug.Log($"Expanded to floor {FloorSystem.Instance.CurrentFloorCount}");
                    }
                }
                
                // 拡張後の状態を表示
                PrintTestInfo();
            }
        }

        /// <summary>
        /// 連続出現防止テスト
        /// </summary>
        [ContextMenu("Test Consecutive Spawn Prevention")]
        public void TestConsecutiveSpawnPrevention()
        {
            Debug.Log("Testing consecutive spawn prevention...");
            
            // 連続で出現を試行
            for (int i = 0; i < 5; i++)
            {
                ForceSpawnInvader();
                Debug.Log($"Spawn attempt {i + 1}");
            }
        }

        /// <summary>
        /// パーティ出現テスト
        /// </summary>
        [ContextMenu("Test Party Spawning")]
        public void TestPartySpawning()
        {
            Debug.Log("Testing party spawning...");
            
            // 複数回パーティ出現を試行
            for (int i = 0; i < 3; i++)
            {
                ForceSpawnParty();
                Debug.Log($"Party spawn attempt {i + 1}");
            }
        }

        // GUI表示用
        private void OnGUI()
        {
            if (!logDetailedInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== Invader Spawner Test ===");
            GUILayout.Label($"Test Running: {testRunning}");
            GUILayout.Label($"Total Spawned: {totalSpawned}");
            GUILayout.Label($"Parties: {partiesSpawned}");
            GUILayout.Label($"Avg Interval: {averageSpawnInterval:F1}s");
            
            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label($"[{forceSpawnKey}] Force Spawn");
            GUILayout.Label($"[{forcePartySpawnKey}] Force Party");
            GUILayout.Label($"[{printInfoKey}] Print Info");
            GUILayout.Label($"[{toggleSpawningKey}] Toggle Spawning");
            
            GUILayout.EndArea();
        }
    }
}