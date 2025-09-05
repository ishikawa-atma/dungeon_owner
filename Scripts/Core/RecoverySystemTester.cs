using UnityEngine;
using DungeonOwner.Interfaces;
using DungeonOwner.Managers;
using DungeonOwner.Data;
using DungeonOwner.Monsters;

namespace DungeonOwner.Core
{
    /// <summary>
    /// HP/MP回復システムのテスト用クラス
    /// </summary>
    public class RecoverySystemTester : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField] private bool enableTesting = true;
        [SerializeField] private KeyCode damageTestKey = KeyCode.D;
        [SerializeField] private KeyCode healTestKey = KeyCode.H;
        [SerializeField] private KeyCode shelterToggleKey = KeyCode.S;
        [SerializeField] private KeyCode recoveryRateTestKey = KeyCode.R;
        
        [Header("テスト対象")]
        [SerializeField] private BaseMonster[] testMonsters;
        [SerializeField] private float testDamageAmount = 30f;
        [SerializeField] private float testHealAmount = 20f;
        
        [Header("回復率テスト")]
        [SerializeField] private float[] testFloorRates = { 0.5f, 1f, 2f, 3f };
        [SerializeField] private float[] testShelterRates = { 2f, 5f, 10f, 15f };
        private int currentRateIndex = 0;
        
        private void Start()
        {
            if (!enableTesting) return;
            
            // テスト対象が設定されていない場合、シーン内のモンスターを自動検出
            if (testMonsters == null || testMonsters.Length == 0)
            {
                testMonsters = FindObjectsOfType<BaseMonster>();
            }
            
            Debug.Log($"[RecoverySystemTester] テスト開始 - 対象モンスター数: {testMonsters?.Length ?? 0}");
            LogTestInstructions();
        }
        
        private void Update()
        {
            if (!enableTesting) return;
            
            HandleTestInput();
            DisplayRecoveryStats();
        }
        
        /// <summary>
        /// テスト用入力処理
        /// </summary>
        private void HandleTestInput()
        {
            // ダメージテスト
            if (Input.GetKeyDown(damageTestKey))
            {
                DamageAllTestMonsters();
            }
            
            // 回復テスト
            if (Input.GetKeyDown(healTestKey))
            {
                HealAllTestMonsters();
            }
            
            // 退避スポット状態切り替え
            if (Input.GetKeyDown(shelterToggleKey))
            {
                ToggleShelterState();
            }
            
            // 回復率変更テスト
            if (Input.GetKeyDown(recoveryRateTestKey))
            {
                CycleRecoveryRates();
            }
        }
        
        /// <summary>
        /// 全テストモンスターにダメージを与える
        /// </summary>
        private void DamageAllTestMonsters()
        {
            if (testMonsters == null) return;
            
            int damagedCount = 0;
            foreach (var monster in testMonsters)
            {
                if (monster != null && monster.IsAlive())
                {
                    monster.TakeDamage(testDamageAmount);
                    damagedCount++;
                }
            }
            
            Debug.Log($"[RecoverySystemTester] {damagedCount}体のモンスターに{testDamageAmount}ダメージを与えました");
        }
        
        /// <summary>
        /// 全テストモンスターを回復
        /// </summary>
        private void HealAllTestMonsters()
        {
            if (testMonsters == null) return;
            
            int healedCount = 0;
            foreach (var monster in testMonsters)
            {
                if (monster != null && monster.IsAlive())
                {
                    monster.Heal(testHealAmount);
                    healedCount++;
                }
            }
            
            Debug.Log($"[RecoverySystemTester] {healedCount}体のモンスターを{testHealAmount}回復しました");
        }
        
        /// <summary>
        /// 退避スポット状態を切り替え
        /// </summary>
        private void ToggleShelterState()
        {
            if (testMonsters == null) return;
            
            int toggledCount = 0;
            foreach (var monster in testMonsters)
            {
                if (monster != null && monster.IsAlive())
                {
                    MonsterState newState = monster.State == MonsterState.InShelter 
                        ? MonsterState.Idle 
                        : MonsterState.InShelter;
                    
                    monster.SetState(newState);
                    toggledCount++;
                }
            }
            
            Debug.Log($"[RecoverySystemTester] {toggledCount}体のモンスターの退避状態を切り替えました");
        }
        
        /// <summary>
        /// 回復率を循環的に変更
        /// </summary>
        private void CycleRecoveryRates()
        {
            if (RecoveryManager.Instance == null) return;
            
            currentRateIndex = (currentRateIndex + 1) % testFloorRates.Length;
            
            float floorRate = testFloorRates[currentRateIndex];
            float shelterRate = testShelterRates[currentRateIndex];
            
            RecoveryManager.Instance.SetRecoveryRates(floorRate, shelterRate);
            
            Debug.Log($"[RecoverySystemTester] 回復率を変更: 階層={floorRate}/秒, 退避={shelterRate}/秒");
        }
        
        /// <summary>
        /// 回復統計情報を表示
        /// </summary>
        private void DisplayRecoveryStats()
        {
            if (RecoveryManager.Instance == null) return;
            
            // 5秒ごとに統計情報を表示
            if (Time.time % 5f < Time.deltaTime)
            {
                var stats = RecoveryManager.Instance.GetRecoveryStats();
                Debug.Log($"[RecoveryStats] 総モンスター: {stats.TotalMonsters}, " +
                         $"回復中: {stats.RecoveringMonsters}, " +
                         $"退避中: {stats.ShelterMonsters}, " +
                         $"回復率: {stats.FloorRecoveryRate}/{stats.ShelterRecoveryRate}");
            }
        }
        
        /// <summary>
        /// テスト手順をログに出力
        /// </summary>
        private void LogTestInstructions()
        {
            Debug.Log("[RecoverySystemTester] テスト操作方法:");
            Debug.Log($"  {damageTestKey} - 全モンスターにダメージ");
            Debug.Log($"  {healTestKey} - 全モンスターを回復");
            Debug.Log($"  {shelterToggleKey} - 退避状態切り替え");
            Debug.Log($"  {recoveryRateTestKey} - 回復率変更");
        }
        
        /// <summary>
        /// 特定のモンスターの回復状況を詳細表示
        /// </summary>
        public void ShowDetailedRecoveryInfo(IMonster monster)
        {
            if (monster == null) return;
            
            float healthRatio = monster.Health / monster.MaxHealth;
            float manaRatio = monster.Mana / monster.MaxMana;
            bool isInShelter = monster.State == MonsterState.InShelter;
            
            Debug.Log($"[RecoveryInfo] {monster.Type} Lv.{monster.Level}:");
            Debug.Log($"  HP: {monster.Health:F1}/{monster.MaxHealth:F1} ({healthRatio:P1})");
            Debug.Log($"  MP: {monster.Mana:F1}/{monster.MaxMana:F1} ({manaRatio:P1})");
            Debug.Log($"  状態: {monster.State} (退避中: {isInShelter})");
            
            if (RecoveryManager.Instance != null)
            {
                float recoveryRate = RecoveryManager.Instance.GetRecoveryRate(isInShelter);
                Debug.Log($"  回復速度: {recoveryRate}/秒");
            }
        }
        
        /// <summary>
        /// 回復システムの動作確認
        /// </summary>
        [ContextMenu("回復システム動作確認")]
        public void VerifyRecoverySystem()
        {
            Debug.Log("[RecoverySystemTester] 回復システム動作確認開始");
            
            // RecoveryManagerの存在確認
            bool hasRecoveryManager = RecoveryManager.Instance != null;
            Debug.Log($"  RecoveryManager: {(hasRecoveryManager ? "OK" : "NG")}");
            
            // テストモンスターの確認
            int validMonsters = 0;
            if (testMonsters != null)
            {
                foreach (var monster in testMonsters)
                {
                    if (monster != null && monster.IsAlive())
                    {
                        validMonsters++;
                    }
                }
            }
            Debug.Log($"  有効なテストモンスター: {validMonsters}体");
            
            // 回復システムの統計確認
            if (hasRecoveryManager)
            {
                var stats = RecoveryManager.Instance.GetRecoveryStats();
                Debug.Log($"  登録済みモンスター: {stats.TotalMonsters}体");
                Debug.Log($"  現在の回復率: 階層={stats.FloorRecoveryRate}/秒, 退避={stats.ShelterRecoveryRate}/秒");
            }
            
            Debug.Log("[RecoverySystemTester] 動作確認完了");
        }
    }
}