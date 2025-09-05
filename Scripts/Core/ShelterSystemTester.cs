using UnityEngine;
using System.Collections;
using DungeonOwner.Managers;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 退避スポットシステムのテスト用クラス
    /// </summary>
    public class ShelterSystemTester : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField] private bool runTestOnStart = true;
        [SerializeField] private float testDelay = 2f;
        
        private ShelterManager shelterManager;
        private MonsterPlacementManager placementManager;
        private FloorSystem floorSystem;
        
        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunTests());
            }
        }
        
        private IEnumerator RunTests()
        {
            yield return new WaitForSeconds(1f);
            
            // 必要なコンポーネントを取得
            shelterManager = FindObjectOfType<ShelterManager>();
            placementManager = FindObjectOfType<MonsterPlacementManager>();
            floorSystem = FindObjectOfType<FloorSystem>();
            
            if (shelterManager == null || placementManager == null || floorSystem == null)
            {
                Debug.LogError("必要なコンポーネントが見つかりません");
                yield break;
            }
            
            Debug.Log("=== 退避スポットシステムテスト開始 ===");
            
            // テスト1: モンスター配置と退避
            yield return StartCoroutine(TestMonsterSheltering());
            
            yield return new WaitForSeconds(testDelay);
            
            // テスト2: 退避スポットからの配置
            yield return StartCoroutine(TestMonsterDeployment());
            
            yield return new WaitForSeconds(testDelay);
            
            // テスト3: 配置可能階層の取得
            yield return StartCoroutine(TestAvailableFloors());
            
            yield return new WaitForSeconds(testDelay);
            
            // テスト4: 回復システム
            yield return StartCoroutine(TestRecoverySystem());
            
            Debug.Log("=== 退避スポットシステムテスト完了 ===");
        }
        
        /// <summary>
        /// テスト1: モンスターの退避機能
        /// </summary>
        private IEnumerator TestMonsterSheltering()
        {
            Debug.Log("--- テスト1: モンスター退避機能 ---");
            
            // 1階層にモンスターを配置
            Vector2 testPosition = new Vector2(0, 0);
            IMonster testMonster = placementManager.PlaceMonster(1, MonsterType.Slime, testPosition, 1);
            
            if (testMonster == null)
            {
                Debug.LogError("テスト用モンスターの配置に失敗");
                yield break;
            }
            
            Debug.Log($"テスト用モンスター {testMonster.Type} を配置しました");
            yield return new WaitForSeconds(1f);
            
            // 退避スポットに移動
            bool shelterResult = shelterManager.ShelterMonster(testMonster);
            
            if (shelterResult)
            {
                Debug.Log("✓ モンスターの退避に成功");
                Debug.Log($"退避スポット内モンスター数: {shelterManager.CurrentCount}");
            }
            else
            {
                Debug.LogError("✗ モンスターの退避に失敗");
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        /// <summary>
        /// テスト2: 退避スポットからの配置
        /// </summary>
        private IEnumerator TestMonsterDeployment()
        {
            Debug.Log("--- テスト2: 退避スポットからの配置 ---");
            
            if (shelterManager.ShelterMonsters.Count == 0)
            {
                Debug.LogWarning("退避スポットにモンスターがいません");
                yield break;
            }
            
            IMonster shelterMonster = shelterManager.ShelterMonsters[0];
            Vector2 deployPosition = new Vector2(2, 2);
            
            bool deployResult = shelterManager.DeployMonster(shelterMonster, 1, deployPosition);
            
            if (deployResult)
            {
                Debug.Log("✓ 退避スポットからの配置に成功");
                Debug.Log($"退避スポット内モンスター数: {shelterManager.CurrentCount}");
            }
            else
            {
                Debug.LogError("✗ 退避スポットからの配置に失敗");
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        /// <summary>
        /// テスト3: 配置可能階層の取得
        /// </summary>
        private IEnumerator TestAvailableFloors()
        {
            Debug.Log("--- テスト3: 配置可能階層の取得 ---");
            
            var availableFloors = shelterManager.GetAvailableFloors();
            
            Debug.Log($"配置可能階層数: {availableFloors.Count}");
            foreach (int floor in availableFloors)
            {
                var positions = shelterManager.GetAvailablePositions(floor);
                Debug.Log($"階層 {floor}: {positions.Count} 箇所配置可能");
            }
            
            yield return new WaitForSeconds(1f);
        }
        
        /// <summary>
        /// テスト4: 回復システム
        /// </summary>
        private IEnumerator TestRecoverySystem()
        {
            Debug.Log("--- テスト4: 回復システム ---");
            
            // テスト用モンスターを配置して退避
            Vector2 testPosition = new Vector2(-1, -1);
            IMonster testMonster = placementManager.PlaceMonster(1, MonsterType.Goblin, testPosition, 1);
            
            if (testMonster == null)
            {
                Debug.LogError("テスト用モンスターの配置に失敗");
                yield break;
            }
            
            // HPを減らす
            float originalHealth = testMonster.Health;
            testMonster.TakeDamage(testMonster.MaxHealth * 0.5f);
            float damagedHealth = testMonster.Health;
            
            Debug.Log($"モンスターHP: {originalHealth} → {damagedHealth}");
            
            // 退避スポットに移動
            shelterManager.ShelterMonster(testMonster);
            
            // 回復を待つ
            yield return new WaitForSeconds(3f);
            
            float recoveredHealth = testMonster.Health;
            Debug.Log($"回復後HP: {recoveredHealth}");
            
            if (recoveredHealth > damagedHealth)
            {
                Debug.Log("✓ 退避スポットでの回復機能が動作しています");
            }
            else
            {
                Debug.LogWarning("✗ 回復機能が動作していない可能性があります");
            }
        }
        
        /// <summary>
        /// 手動テスト用メソッド
        /// </summary>
        [ContextMenu("退避スポット状態表示")]
        public void ShowShelterStatus()
        {
            if (shelterManager == null)
            {
                shelterManager = FindObjectOfType<ShelterManager>();
            }
            
            if (shelterManager != null)
            {
                Debug.Log($"=== 退避スポット状態 ===");
                Debug.Log($"収容数: {shelterManager.CurrentCount}/{shelterManager.MaxCapacity}");
                Debug.Log($"満杯: {shelterManager.IsFull}");
                
                foreach (var monster in shelterManager.ShelterMonsters)
                {
                    if (monster != null)
                    {
                        Debug.Log($"- {monster.Type} Lv.{monster.Level} HP:{monster.Health:F1}/{monster.MaxHealth:F1}");
                    }
                }
            }
        }
        
        [ContextMenu("配置可能階層表示")]
        public void ShowAvailableFloors()
        {
            if (shelterManager == null)
            {
                shelterManager = FindObjectOfType<ShelterManager>();
            }
            
            if (shelterManager != null)
            {
                var floors = shelterManager.GetAvailableFloors();
                Debug.Log($"=== 配置可能階層 ===");
                
                foreach (int floor in floors)
                {
                    var positions = shelterManager.GetAvailablePositions(floor);
                    Debug.Log($"階層 {floor}: {positions.Count} 箇所配置可能");
                }
            }
        }
    }
}