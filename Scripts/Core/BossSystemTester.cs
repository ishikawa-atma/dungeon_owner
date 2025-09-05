using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Managers;
using DungeonOwner.Core;

namespace DungeonOwner.Core
{
    /// <summary>
    /// ボスキャラクターシステムのテスタークラス
    /// 要件6.1, 6.2, 6.4, 6.5の動作確認を行う
    /// </summary>
    public class BossSystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private KeyCode testKey = KeyCode.B;
        [SerializeField] private KeyCode respawnTestKey = KeyCode.R;
        [SerializeField] private KeyCode statusTestKey = KeyCode.S;

        [Header("Test Settings")]
        [SerializeField] private int testFloorIndex = 5;
        [SerializeField] private BossType testBossType = BossType.DragonLord;
        [SerializeField] private Vector2 testPosition = Vector2.zero;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunTestsAfterDelay());
            }
        }

        private System.Collections.IEnumerator RunTestsAfterDelay()
        {
            // システムの初期化を待つ
            yield return new WaitForSeconds(1f);
            
            RunAllTests();
        }

        private void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                RunAllTests();
            }

            if (Input.GetKeyDown(respawnTestKey))
            {
                TestBossRespawn();
            }

            if (Input.GetKeyDown(statusTestKey))
            {
                TestBossStatus();
            }
        }

        /// <summary>
        /// 全テストを実行
        /// </summary>
        public void RunAllTests()
        {
            DebugLog("=== Boss System Tests Started ===");

            TestBossFloorDetection();
            TestBossPlacement();
            TestBossAvailability();
            TestBossManagement();

            DebugLog("=== Boss System Tests Completed ===");
        }

        /// <summary>
        /// 要件6.1: 5階層ごとのボスキャラ配置システムのテスト
        /// </summary>
        private void TestBossFloorDetection()
        {
            DebugLog("--- Testing Boss Floor Detection (Requirement 6.1) ---");

            if (BossManager.Instance == null)
            {
                DebugLog("ERROR: BossManager not found");
                return;
            }

            // 5階層ごとのボス階層判定をテスト
            for (int floor = 1; floor <= 20; floor++)
            {
                bool isBossFloor = BossManager.Instance.IsBossFloor(floor);
                bool expected = floor % 5 == 0;

                if (isBossFloor == expected)
                {
                    DebugLog($"✓ Floor {floor}: Boss floor detection correct ({isBossFloor})");
                }
                else
                {
                    DebugLog($"✗ Floor {floor}: Boss floor detection failed. Expected: {expected}, Got: {isBossFloor}");
                }
            }
        }

        /// <summary>
        /// 要件6.2: 選択式のボスキャラリスト表示のテスト
        /// </summary>
        private void TestBossAvailability()
        {
            DebugLog("--- Testing Boss Availability (Requirement 6.2) ---");

            if (BossManager.Instance == null)
            {
                DebugLog("ERROR: BossManager not found");
                return;
            }

            // 各ボス階層で利用可能なボスをテスト
            int[] testFloors = { 5, 10, 15, 20 };

            foreach (int floor in testFloors)
            {
                List<BossData> availableBosses = BossManager.Instance.GetAvailableBossesForFloor(floor);
                DebugLog($"Floor {floor}: {availableBosses.Count} bosses available");

                foreach (BossData boss in availableBosses)
                {
                    DebugLog($"  - {boss.type}: {boss.displayName}");
                }
            }
        }

        /// <summary>
        /// ボス配置のテスト
        /// </summary>
        private void TestBossPlacement()
        {
            DebugLog("--- Testing Boss Placement ---");

            if (BossManager.Instance == null || FloorSystem.Instance == null)
            {
                DebugLog("ERROR: Required managers not found");
                return;
            }

            // テスト用階層を確保
            while (FloorSystem.Instance.CurrentFloorCount < testFloorIndex)
            {
                FloorSystem.Instance.ExpandFloor();
            }

            // ボス配置可能かテスト
            bool canPlace = BossManager.Instance.CanPlaceBoss(testFloorIndex);
            DebugLog($"Can place boss on floor {testFloorIndex}: {canPlace}");

            if (canPlace)
            {
                // ボスを配置
                IBoss placedBoss = BossManager.Instance.PlaceBoss(testFloorIndex, testBossType, testPosition, 1);

                if (placedBoss != null)
                {
                    DebugLog($"✓ Successfully placed {testBossType} on floor {testFloorIndex}");
                    DebugLog($"  Boss Level: {placedBoss.Level}");
                    DebugLog($"  Boss Health: {placedBoss.Health}/{placedBoss.MaxHealth}");
                    DebugLog($"  Boss Mana: {placedBoss.Mana}/{placedBoss.MaxMana}");
                }
                else
                {
                    DebugLog($"✗ Failed to place {testBossType} on floor {testFloorIndex}");
                }
            }
        }

        /// <summary>
        /// 要件6.4, 6.5: ボスリポップとレベル引き継ぎのテスト
        /// </summary>
        private void TestBossRespawn()
        {
            DebugLog("--- Testing Boss Respawn (Requirements 6.4, 6.5) ---");

            if (BossManager.Instance == null)
            {
                DebugLog("ERROR: BossManager not found");
                return;
            }

            IBoss boss = BossManager.Instance.GetBossOnFloor(testFloorIndex);
            if (boss == null)
            {
                DebugLog($"No boss found on floor {testFloorIndex}. Placing one for test...");
                boss = BossManager.Instance.PlaceBoss(testFloorIndex, testBossType, testPosition, 3);
            }

            if (boss != null)
            {
                int originalLevel = boss.Level;
                DebugLog($"Boss original level: {originalLevel}");

                // ボスを撃破状態にする（テスト用）
                boss.TakeDamage(boss.Health);

                if (boss.IsRespawning)
                {
                    DebugLog($"✓ Boss started respawn process");
                    DebugLog($"  Respawn time: {boss.RespawnTime} seconds");
                    DebugLog($"  Defeated level: {boss.DefeatedLevel}");

                    // リポップ進行度を監視
                    StartCoroutine(MonitorRespawnProgress(boss, originalLevel));
                }
                else
                {
                    DebugLog($"✗ Boss did not start respawn process");
                }
            }
        }

        /// <summary>
        /// リポップ進行度を監視
        /// </summary>
        private System.Collections.IEnumerator MonitorRespawnProgress(IBoss boss, int originalLevel)
        {
            while (boss.IsRespawning)
            {
                DebugLog($"Respawn progress: {boss.RespawnProgress:P}");
                yield return new WaitForSeconds(1f);
            }

            // リポップ完了後のレベル確認
            if (boss.Level == originalLevel)
            {
                DebugLog($"✓ Boss respawned with correct level: {boss.Level}");
            }
            else
            {
                DebugLog($"✗ Boss level mismatch. Expected: {originalLevel}, Got: {boss.Level}");
            }
        }

        /// <summary>
        /// ボス管理機能のテスト
        /// </summary>
        private void TestBossManagement()
        {
            DebugLog("--- Testing Boss Management ---");

            if (BossManager.Instance == null)
            {
                DebugLog("ERROR: BossManager not found");
                return;
            }

            // アクティブなボスの一覧
            List<IBoss> activeBosses = BossManager.Instance.GetActiveBosses();
            DebugLog($"Active bosses: {activeBosses.Count}");

            foreach (IBoss boss in activeBosses)
            {
                int floor = FindBossFloor(boss);
                string status = boss.IsRespawning ? "Respawning" : "Active";
                DebugLog($"  Floor {floor}: {boss.BossType} (Level {boss.Level}) - {status}");
            }

            // ボスタイプ検索テスト
            BossType? bossType = BossManager.Instance.GetBossTypeOnFloor(testFloorIndex);
            if (bossType.HasValue)
            {
                DebugLog($"Boss on floor {testFloorIndex}: {bossType.Value}");
            }
            else
            {
                DebugLog($"No boss on floor {testFloorIndex}");
            }
        }

        /// <summary>
        /// ボス状態のテスト
        /// </summary>
        private void TestBossStatus()
        {
            DebugLog("--- Testing Boss Status ---");

            if (BossManager.Instance == null)
            {
                DebugLog("ERROR: BossManager not found");
                return;
            }

            List<IBoss> activeBosses = BossManager.Instance.GetActiveBosses();

            foreach (IBoss boss in activeBosses)
            {
                int floor = FindBossFloor(boss);
                DebugLog($"=== Boss Status: Floor {floor} ===");
                DebugLog($"Type: {boss.BossType}");
                DebugLog($"Level: {boss.Level}");
                DebugLog($"Health: {boss.Health:F0}/{boss.MaxHealth:F0} ({boss.Health/boss.MaxHealth:P})");
                DebugLog($"Mana: {boss.Mana:F0}/{boss.MaxMana:F0} ({boss.Mana/boss.MaxMana:P})");
                DebugLog($"State: {boss.State}");
                DebugLog($"Is Respawning: {boss.IsRespawning}");
                
                if (boss.IsRespawning)
                {
                    DebugLog($"Respawn Progress: {boss.RespawnProgress:P}");
                    DebugLog($"Defeated Level: {boss.DefeatedLevel}");
                }
            }
        }

        /// <summary>
        /// ボスがいる階層を検索
        /// </summary>
        private int FindBossFloor(IBoss boss)
        {
            if (BossManager.Instance == null) return -1;

            for (int i = 1; i <= (FloorSystem.Instance?.CurrentFloorCount ?? 0); i++)
            {
                IBoss floorBoss = BossManager.Instance.GetBossOnFloor(i);
                if (floorBoss == boss)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// セーブ/ロード機能のテスト
        /// </summary>
        public void TestBossSaveLoad()
        {
            DebugLog("--- Testing Boss Save/Load ---");

            if (BossManager.Instance == null)
            {
                DebugLog("ERROR: BossManager not found");
                return;
            }

            // セーブデータを取得
            var saveData = BossManager.Instance.GetBossSaveData();
            DebugLog($"Boss save data contains {saveData.Count} entries");

            foreach (var kvp in saveData)
            {
                int floor = kvp.Key;
                var (type, level, isRespawning, progress) = kvp.Value;
                DebugLog($"Floor {floor}: {type} (Level {level}, Respawning: {isRespawning}, Progress: {progress:P})");
            }

            // ロードテスト（実際のセーブシステムと連携する場合）
            // BossManager.Instance.LoadBossSaveData(saveData);
        }

        /// <summary>
        /// デバッグログ出力
        /// </summary>
        private void DebugLog(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[BossSystemTester] {message}");
            }
        }

        /// <summary>
        /// 外部からテストを実行
        /// </summary>
        public void ExecuteTests()
        {
            RunAllTests();
        }

        /// <summary>
        /// 特定のテストを実行
        /// </summary>
        public void ExecuteTest(string testName)
        {
            switch (testName.ToLower())
            {
                case "placement":
                    TestBossPlacement();
                    break;
                case "respawn":
                    TestBossRespawn();
                    break;
                case "status":
                    TestBossStatus();
                    break;
                case "detection":
                    TestBossFloorDetection();
                    break;
                case "availability":
                    TestBossAvailability();
                    break;
                case "management":
                    TestBossManagement();
                    break;
                case "saveload":
                    TestBossSaveLoad();
                    break;
                default:
                    RunAllTests();
                    break;
            }
        }

        /// <summary>
        /// テスト用のボス強制配置
        /// </summary>
        public void ForceCreateTestBoss(int floorIndex, BossType bossType, int level = 1)
        {
            if (BossManager.Instance == null || FloorSystem.Instance == null)
            {
                DebugLog("ERROR: Required managers not found");
                return;
            }

            // 階層を確保
            while (FloorSystem.Instance.CurrentFloorCount < floorIndex)
            {
                FloorSystem.Instance.ExpandFloor();
            }

            // ボスを配置
            IBoss boss = BossManager.Instance.PlaceBoss(floorIndex, bossType, Vector2.zero, level);
            
            if (boss != null)
            {
                DebugLog($"Force created test boss: {bossType} on floor {floorIndex} at level {level}");
            }
            else
            {
                DebugLog($"Failed to create test boss: {bossType} on floor {floorIndex}");
            }
        }
    }
}