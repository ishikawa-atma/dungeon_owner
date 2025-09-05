using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Managers;

namespace DungeonOwner.Core
{
    public class InvaderSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableTesting = true;
        [SerializeField] private KeyCode spawnWarriorKey = KeyCode.Alpha1;
        [SerializeField] private KeyCode spawnMageKey = KeyCode.Alpha2;
        [SerializeField] private KeyCode spawnRogueKey = KeyCode.Alpha3;
        [SerializeField] private KeyCode spawnClericKey = KeyCode.Alpha4;
        [SerializeField] private KeyCode spawnRandomKey = KeyCode.R;
        [SerializeField] private KeyCode toggleSpawningKey = KeyCode.T;

        [Header("Test Parameters")]
        [SerializeField] private int testLevel = 1;
        [SerializeField] private bool showDebugInfo = true;

        private bool isSpawningActive = false;

        private void Update()
        {
            if (!enableTesting) return;

            HandleTestInput();
            
            if (showDebugInfo)
            {
                DisplayDebugInfo();
            }
        }

        private void HandleTestInput()
        {
            // 個別侵入者の生成
            if (Input.GetKeyDown(spawnWarriorKey))
            {
                SpawnTestInvader(InvaderType.Warrior);
            }
            else if (Input.GetKeyDown(spawnMageKey))
            {
                SpawnTestInvader(InvaderType.Mage);
            }
            else if (Input.GetKeyDown(spawnRogueKey))
            {
                SpawnTestInvader(InvaderType.Rogue);
            }
            else if (Input.GetKeyDown(spawnClericKey))
            {
                SpawnTestInvader(InvaderType.Cleric);
            }
            else if (Input.GetKeyDown(spawnRandomKey))
            {
                SpawnRandomTestInvader();
            }
            else if (Input.GetKeyDown(toggleSpawningKey))
            {
                ToggleAutoSpawning();
            }
        }

        private void SpawnTestInvader(InvaderType type)
        {
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.ForceSpawnInvader(type, testLevel);
                Debug.Log($"Test spawned: {type} (Level {testLevel})");
            }
            else
            {
                Debug.LogWarning("InvaderSpawner not found!");
            }
        }

        private void SpawnRandomTestInvader()
        {
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.SpawnRandomInvader();
                Debug.Log("Test spawned: Random Invader");
            }
            else
            {
                Debug.LogWarning("InvaderSpawner not found!");
            }
        }

        private void ToggleAutoSpawning()
        {
            if (InvaderSpawner.Instance != null)
            {
                if (isSpawningActive)
                {
                    InvaderSpawner.Instance.StopSpawning();
                    isSpawningActive = false;
                    Debug.Log("Auto spawning stopped");
                }
                else
                {
                    InvaderSpawner.Instance.StartSpawning();
                    isSpawningActive = true;
                    Debug.Log("Auto spawning started");
                }
            }
        }

        private void DisplayDebugInfo()
        {
            // スクリーン上にデバッグ情報を表示
            if (InvaderSpawner.Instance != null)
            {
                // GUI表示は OnGUI で行う
            }
        }

        private void OnGUI()
        {
            if (!enableTesting || !showDebugInfo) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== Invader System Tester ===");
            GUILayout.Label($"1: Spawn Warrior (Level {testLevel})");
            GUILayout.Label($"2: Spawn Mage (Level {testLevel})");
            GUILayout.Label($"3: Spawn Rogue (Level {testLevel})");
            GUILayout.Label($"4: Spawn Cleric (Level {testLevel})");
            GUILayout.Label("R: Spawn Random Invader");
            GUILayout.Label($"T: Toggle Auto Spawning ({(isSpawningActive ? "ON" : "OFF")})");
            
            GUILayout.Space(10);
            
            if (InvaderSpawner.Instance != null)
            {
                GUILayout.Label("=== Spawner Status ===");
                // スポーナーの状態情報を表示
            }
            
            GUILayout.EndArea();
        }

        // テスト用メソッド
        [ContextMenu("Test Spawn All Types")]
        public void TestSpawnAllTypes()
        {
            SpawnTestInvader(InvaderType.Warrior);
            SpawnTestInvader(InvaderType.Mage);
            SpawnTestInvader(InvaderType.Rogue);
            SpawnTestInvader(InvaderType.Cleric);
        }

        [ContextMenu("Clear All Invaders")]
        public void ClearAllInvaders()
        {
            GameObject[] invaders = GameObject.FindGameObjectsWithTag("Invader");
            foreach (var invader in invaders)
            {
                DestroyImmediate(invader);
            }
            Debug.Log($"Cleared {invaders.Length} invaders");
        }

        [ContextMenu("Print Invader Data")]
        public void PrintInvaderData()
        {
            if (DataManager.Instance != null)
            {
                Debug.Log("=== Available Invader Data ===");
                var warriors = DataManager.Instance.GetInvaderData(InvaderType.Warrior);
                var mages = DataManager.Instance.GetInvaderData(InvaderType.Mage);
                var rogues = DataManager.Instance.GetInvaderData(InvaderType.Rogue);
                var clerics = DataManager.Instance.GetInvaderData(InvaderType.Cleric);

                Debug.Log($"Warrior: {(warriors != null ? warriors.displayName : "Not Found")}");
                Debug.Log($"Mage: {(mages != null ? mages.displayName : "Not Found")}");
                Debug.Log($"Rogue: {(rogues != null ? rogues.displayName : "Not Found")}");
                Debug.Log($"Cleric: {(clerics != null ? clerics.displayName : "Not Found")}");
            }
        }

        // 設定メソッド
        public void SetTestLevel(int level)
        {
            testLevel = Mathf.Max(1, level);
        }

        public void SetSpawnInterval(float interval)
        {
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.SetSpawnInterval(interval);
            }
        }

        public void SetMaxConcurrentInvaders(int max)
        {
            if (InvaderSpawner.Instance != null)
            {
                InvaderSpawner.Instance.SetMaxConcurrentInvaders(max);
            }
        }
    }
}