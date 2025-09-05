using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Managers;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 侵入者ランダム出現システムのテストセットアップ
    /// 要件18.1-18.4のテスト環境を構築
    /// </summary>
    public class InvaderSpawnerTestSetup : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool startGameAutomatically = true;
        [SerializeField] private int initialFloors = 5;

        [Header("Spawner Settings")]
        [SerializeField] private float testSpawnInterval = 8f;
        [SerializeField] private int testMaxConcurrentInvaders = 15;
        [SerializeField] private float testFrequencyScaling = 0.15f;

        [Header("Test Objects")]
        [SerializeField] private GameObject spawnerPrefab;
        [SerializeField] private GameObject testerPrefab;

        private void Start()
        {
            if (autoSetup)
            {
                SetupTestEnvironment();
            }
        }

        [ContextMenu("Setup Test Environment")]
        public void SetupTestEnvironment()
        {
            Debug.Log("Setting up Invader Spawner test environment...");

            // 必要なマネージャーの確認・作成
            EnsureRequiredManagers();

            // FloorSystemの初期化
            SetupFloorSystem();

            // InvaderSpawnerの設定
            SetupInvaderSpawner();

            // テスターの設定
            SetupTester();

            // ゲーム開始
            if (startGameAutomatically)
            {
                StartGame();
            }

            Debug.Log("Test environment setup completed!");
        }

        private void EnsureRequiredManagers()
        {
            // GameManager
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
                Debug.Log("Created GameManager");
            }

            // DataManager
            if (DataManager.Instance == null)
            {
                GameObject dataManagerObj = new GameObject("DataManager");
                dataManagerObj.AddComponent<DataManager>();
                Debug.Log("Created DataManager");
            }

            // FloorSystem
            if (FloorSystem.Instance == null)
            {
                GameObject floorSystemObj = new GameObject("FloorSystem");
                floorSystemObj.AddComponent<FloorSystem>();
                Debug.Log("Created FloorSystem");
            }

            // ResourceManager
            if (ResourceManager.Instance == null)
            {
                GameObject resourceManagerObj = new GameObject("ResourceManager");
                resourceManagerObj.AddComponent<ResourceManager>();
                Debug.Log("Created ResourceManager");
            }

            // TimeManager
            if (TimeManager.Instance == null)
            {
                GameObject timeManagerObj = new GameObject("TimeManager");
                timeManagerObj.AddComponent<TimeManager>();
                Debug.Log("Created TimeManager");
            }
        }

        private void SetupFloorSystem()
        {
            if (FloorSystem.Instance == null) return;

            // 初期階層数を設定（テスト用に多めに）
            for (int i = FloorSystem.Instance.CurrentFloorCount + 1; i <= initialFloors; i++)
            {
                if (FloorSystem.Instance.CanExpandFloor())
                {
                    FloorSystem.Instance.ExpandFloor();
                }
            }

            Debug.Log($"FloorSystem setup with {FloorSystem.Instance.CurrentFloorCount} floors");
        }

        private void SetupInvaderSpawner()
        {
            // InvaderSpawnerが存在しない場合は作成
            if (InvaderSpawner.Instance == null)
            {
                GameObject spawnerObj;
                if (spawnerPrefab != null)
                {
                    spawnerObj = Instantiate(spawnerPrefab);
                }
                else
                {
                    spawnerObj = new GameObject("InvaderSpawner");
                    spawnerObj.AddComponent<InvaderSpawner>();
                }
                Debug.Log("Created InvaderSpawner");
            }

            // テスト用設定を適用
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.SetSpawnInterval(testSpawnInterval);
                InvaderSpawner.Instance.SetMaxConcurrentInvaders(testMaxConcurrentInvaders);
                InvaderSpawner.Instance.SetFrequencyScaling(testFrequencyScaling);
                InvaderSpawner.Instance.SetPartySpawnSettings(3, 0.4f); // 3階層からパーティ出現、40%確率

                Debug.Log("InvaderSpawner configured for testing");
            }
        }

        private void SetupTester()
        {
            // InvaderSpawnerTesterが存在しない場合は作成
            InvaderSpawnerTester existingTester = FindObjectOfType<InvaderSpawnerTester>();
            if (existingTester == null)
            {
                GameObject testerObj;
                if (testerPrefab != null)
                {
                    testerObj = Instantiate(testerPrefab);
                }
                else
                {
                    testerObj = new GameObject("InvaderSpawnerTester");
                    testerObj.AddComponent<InvaderSpawnerTester>();
                }
                Debug.Log("Created InvaderSpawnerTester");
            }
        }

        private void StartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartGame();
                Debug.Log("Game started for testing");
            }
        }

        /// <summary>
        /// 階層深度テスト用の階層拡張
        /// </summary>
        [ContextMenu("Expand Floors for Testing")]
        public void ExpandFloorsForTesting()
        {
            if (FloorSystem.Instance == null) return;

            int targetFloors = 10;
            while (FloorSystem.Instance.CurrentFloorCount < targetFloors && FloorSystem.Instance.CanExpandFloor())
            {
                FloorSystem.Instance.ExpandFloor();
            }

            Debug.Log($"Expanded to {FloorSystem.Instance.CurrentFloorCount} floors for depth testing");
            
            // InvaderSpawnerの情報を表示
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.DebugPrintSpawnerInfo();
            }
        }

        /// <summary>
        /// 出現頻度テスト
        /// </summary>
        [ContextMenu("Test Spawn Frequency")]
        public void TestSpawnFrequency()
        {
            Debug.Log("=== Testing Spawn Frequency ===");
            
            if (InvaderSpawner.Instance == null)
            {
                Debug.LogError("InvaderSpawner not found");
                return;
            }

            // 現在の設定を表示
            InvaderSpawner.Instance.DebugPrintSpawnerInfo();

            // 階層を段階的に拡張して頻度変化をテスト
            for (int floor = 1; floor <= 8; floor++)
            {
                if (FloorSystem.Instance != null && FloorSystem.Instance.CurrentFloorCount < floor)
                {
                    if (FloorSystem.Instance.CanExpandFloor())
                    {
                        FloorSystem.Instance.ExpandFloor();
                    }
                }

                Debug.Log($"Floor {floor}: Testing spawn frequency...");
                
                // 強制出現テスト
                for (int i = 0; i < 3; i++)
                {
                    InvaderSpawner.Instance.SpawnRandomInvader();
                }
            }
        }

        /// <summary>
        /// パーティ出現テスト
        /// </summary>
        [ContextMenu("Test Party Spawning")]
        public void TestPartySpawning()
        {
            Debug.Log("=== Testing Party Spawning ===");
            
            if (InvaderSpawner.Instance == null)
            {
                Debug.LogError("InvaderSpawner not found");
                return;
            }

            // 階層を拡張してパーティ出現条件を満たす
            ExpandFloorsForTesting();

            // パーティ出現テスト
            for (int i = 0; i < 5; i++)
            {
                InvaderSpawner.Instance.SpawnRandomInvaderParty();
                Debug.Log($"Party spawn test {i + 1}");
            }
        }

        /// <summary>
        /// 連続出現防止テスト
        /// </summary>
        [ContextMenu("Test Consecutive Prevention")]
        public void TestConsecutivePrevention()
        {
            Debug.Log("=== Testing Consecutive Spawn Prevention ===");
            
            if (InvaderSpawner.Instance == null)
            {
                Debug.LogError("InvaderSpawner not found");
                return;
            }

            // 連続出現防止時間を短く設定
            InvaderSpawner.Instance.SetPreventConsecutiveSpawnTime(2f);

            // 連続出現を試行
            for (int i = 0; i < 10; i++)
            {
                bool spawned = false;
                try
                {
                    InvaderSpawner.Instance.SpawnRandomInvader();
                    spawned = true;
                }
                catch
                {
                    spawned = false;
                }

                Debug.Log($"Consecutive spawn attempt {i + 1}: {(spawned ? "SUCCESS" : "PREVENTED")}");
                
                // 短い間隔で試行
                System.Threading.Thread.Sleep(500);
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(Screen.width - 200, 10, 190, 150));
            GUILayout.Label("=== Test Controls ===");
            
            if (GUILayout.Button("Setup Environment"))
            {
                SetupTestEnvironment();
            }
            
            if (GUILayout.Button("Expand Floors"))
            {
                ExpandFloorsForTesting();
            }
            
            if (GUILayout.Button("Test Frequency"))
            {
                TestSpawnFrequency();
            }
            
            if (GUILayout.Button("Test Parties"))
            {
                TestPartySpawning();
            }
            
            if (GUILayout.Button("Test Prevention"))
            {
                TestConsecutivePrevention();
            }
            
            GUILayout.EndArea();
        }
    }
}