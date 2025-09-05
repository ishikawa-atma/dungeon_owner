using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Interfaces;
using DungeonOwner.Managers;
using DungeonOwner.Monsters;
using DungeonOwner.Invaders;

namespace DungeonOwner.Core
{
    /// <summary>
    /// パーティ戦闘システムのテスター
    /// 要件19.3: パーティ内に回復スキル持ちが存在する場合、パーティメンバーに回復スキルを適用
    /// 要件19.4: 侵入者パーティが出現する場合、協力戦闘を実行
    /// 要件18.3: 階層が深くなると侵入者をパーティ単位で出現
    /// </summary>
    public class PartyCombatSystemTester : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField] private bool enableAutoTest = false;
        [SerializeField] private float testInterval = 10f;
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("テスト対象")]
        [SerializeField] private GameObject monsterPrefab;
        [SerializeField] private GameObject invaderPrefab;
        [SerializeField] private GameObject clericPrefab;
        
        [Header("テスト位置")]
        [SerializeField] private Vector2 monsterPartyPosition = new Vector2(-2f, 0f);
        [SerializeField] private Vector2 invaderPartyPosition = new Vector2(2f, 0f);
        
        private float lastTestTime;
        private List<GameObject> testObjects = new List<GameObject>();
        
        private void Start()
        {
            if (enableAutoTest)
            {
                InvokeRepeating(nameof(RunAutomaticTest), 2f, testInterval);
            }
        }
        
        private void Update()
        {
            // 手動テスト用のキー入力
            if (Input.GetKeyDown(KeyCode.P))
            {
                TestPartyHealing();
            }
            
            if (Input.GetKeyDown(KeyCode.O))
            {
                TestPartyCombat();
            }
            
            if (Input.GetKeyDown(KeyCode.I))
            {
                TestInvaderPartySpawn();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                CleanupTestObjects();
            }
        }
        
        /// <summary>
        /// 自動テスト実行
        /// </summary>
        private void RunAutomaticTest()
        {
            if (Time.time - lastTestTime < testInterval) return;
            
            lastTestTime = Time.time;
            
            // ランダムにテストを選択
            int testType = Random.Range(0, 3);
            
            switch (testType)
            {
                case 0:
                    TestPartyHealing();
                    break;
                case 1:
                    TestPartyCombat();
                    break;
                case 2:
                    TestInvaderPartySpawn();
                    break;
            }
        }
        
        /// <summary>
        /// パーティ回復システムのテスト
        /// 要件19.3: パーティ内に回復スキル持ちが存在する場合、パーティメンバーに回復スキルを適用
        /// </summary>
        [ContextMenu("Test Party Healing")]
        public void TestPartyHealing()
        {
            if (enableDebugLogs) Debug.Log("=== パーティ回復システムテスト開始 ===");
            
            CleanupTestObjects();
            
            // 侵入者パーティを作成（Clericを含む）
            var invaderParty = CreateTestInvaderParty(true);
            
            if (invaderParty != null)
            {
                // パーティメンバーにダメージを与える
                foreach (var member in invaderParty.Members)
                {
                    if (member is IInvader invader)
                    {
                        invader.TakeDamage(invader.MaxHealth * 0.5f); // 50%ダメージ
                    }
                }
                
                if (enableDebugLogs) Debug.Log($"パーティメンバー{invaderParty.Members.Count}名にダメージを与えました");
                
                // 回復システムが自動的に動作することを確認
                // PartyCombatSystemが回復を処理する
            }
        }
        
        /// <summary>
        /// パーティ戦闘システムのテスト
        /// 要件19.4: 侵入者パーティが出現する場合、協力戦闘を実行
        /// </summary>
        [ContextMenu("Test Party Combat")]
        public void TestPartyCombat()
        {
            if (enableDebugLogs) Debug.Log("=== パーティ戦闘システムテスト開始 ===");
            
            CleanupTestObjects();
            
            // モンスターパーティを作成
            var monsterParty = CreateTestMonsterParty();
            
            // 侵入者パーティを作成
            var invaderParty = CreateTestInvaderParty(false);
            
            if (monsterParty != null && invaderParty != null)
            {
                // パーティを戦闘範囲内に配置
                monsterParty.Position = monsterPartyPosition;
                invaderParty.Position = invaderPartyPosition;
                
                if (enableDebugLogs) 
                {
                    Debug.Log($"パーティ戦闘開始: モンスター{monsterParty.Members.Count}名 vs 侵入者{invaderParty.Members.Count}名");
                }
                
                // PartyCombatSystemが自動的に戦闘を処理する
            }
        }
        
        /// <summary>
        /// 侵入者パーティ出現システムのテスト
        /// 要件18.3: 階層が深くなると侵入者をパーティ単位で出現
        /// </summary>
        [ContextMenu("Test Invader Party Spawn")]
        public void TestInvaderPartySpawn()
        {
            if (enableDebugLogs) Debug.Log("=== 侵入者パーティ出現テスト開始 ===");
            
            if (InvaderSpawner.Instance != null)
            {
                // パーティ出現をテスト
                InvaderSpawner.Instance.SpawnRandomInvaderParty();
                
                if (enableDebugLogs) Debug.Log("侵入者パーティの出現をテストしました");
            }
            else
            {
                Debug.LogWarning("InvaderSpawner.Instanceが見つかりません");
            }
        }
        
        /// <summary>
        /// テスト用モンスターパーティを作成
        /// </summary>
        private IParty CreateTestMonsterParty()
        {
            if (PartyManager.Instance == null || monsterPrefab == null)
            {
                Debug.LogWarning("PartyManagerまたはmonsterPrefabが設定されていません");
                return null;
            }
            
            List<ICharacterBase> members = new List<ICharacterBase>();
            
            // 3体のモンスターを作成
            for (int i = 0; i < 3; i++)
            {
                GameObject monsterObj = Instantiate(monsterPrefab);
                monsterObj.transform.position = monsterPartyPosition + new Vector2(i * 0.5f, 0f);
                monsterObj.name = $"TestMonster_{i}";
                
                var monster = monsterObj.GetComponent<ICharacterBase>();
                if (monster != null)
                {
                    members.Add(monster);
                    testObjects.Add(monsterObj);
                }
            }
            
            if (members.Count > 0)
            {
                var party = PartyManager.Instance.CreateParty(members, 1);
                if (enableDebugLogs) Debug.Log($"テスト用モンスターパーティを作成: {members.Count}名");
                return party;
            }
            
            return null;
        }
        
        /// <summary>
        /// テスト用侵入者パーティを作成
        /// </summary>
        private IParty CreateTestInvaderParty(bool includeCleric)
        {
            if (PartyManager.Instance == null || invaderPrefab == null)
            {
                Debug.LogWarning("PartyManagerまたはinvaderPrefabが設定されていません");
                return null;
            }
            
            List<ICharacterBase> members = new List<ICharacterBase>();
            
            // 通常の侵入者を2体作成
            for (int i = 0; i < 2; i++)
            {
                GameObject invaderObj = Instantiate(invaderPrefab);
                invaderObj.transform.position = invaderPartyPosition + new Vector2(i * 0.5f, 0f);
                invaderObj.name = $"TestInvader_{i}";
                
                var invader = invaderObj.GetComponent<ICharacterBase>();
                if (invader != null)
                {
                    members.Add(invader);
                    testObjects.Add(invaderObj);
                }
            }
            
            // Clericを追加（回復テスト用）
            if (includeCleric && clericPrefab != null)
            {
                GameObject clericObj = Instantiate(clericPrefab);
                clericObj.transform.position = invaderPartyPosition + new Vector2(1f, 0.5f);
                clericObj.name = "TestCleric";
                
                var cleric = clericObj.GetComponent<ICharacterBase>();
                if (cleric != null)
                {
                    members.Add(cleric);
                    testObjects.Add(clericObj);
                }
            }
            
            if (members.Count > 0)
            {
                var party = PartyManager.Instance.CreateParty(members, 1);
                if (enableDebugLogs) 
                {
                    Debug.Log($"テスト用侵入者パーティを作成: {members.Count}名" + 
                             (includeCleric ? " (Cleric含む)" : ""));
                }
                return party;
            }
            
            return null;
        }
        
        /// <summary>
        /// テストオブジェクトをクリーンアップ
        /// </summary>
        [ContextMenu("Cleanup Test Objects")]
        public void CleanupTestObjects()
        {
            foreach (var obj in testObjects)
            {
                if (obj != null)
                {
                    DestroyImmediate(obj);
                }
            }
            
            testObjects.Clear();
            
            if (enableDebugLogs) Debug.Log("テストオブジェクトをクリーンアップしました");
        }
        
        /// <summary>
        /// パーティ戦闘システムの状態をデバッグ表示
        /// </summary>
        [ContextMenu("Debug Party Combat Status")]
        public void DebugPartyCombatStatus()
        {
            Debug.Log("=== パーティ戦闘システム状態 ===");
            
            if (PartyCombatSystem.Instance != null)
            {
                PartyCombatSystem.Instance.DebugPartyCombatStatus();
            }
            else
            {
                Debug.LogWarning("PartyCombatSystem.Instanceが見つかりません");
            }
            
            if (PartyManager.Instance != null)
            {
                PartyManager.Instance.DebugPartyStatus();
            }
            else
            {
                Debug.LogWarning("PartyManager.Instanceが見つかりません");
            }
        }
        
        /// <summary>
        /// パーティスキルのテスト
        /// </summary>
        [ContextMenu("Test Party Skills")]
        public void TestPartySkills()
        {
            if (enableDebugLogs) Debug.Log("=== パーティスキルテスト開始 ===");
            
            if (PartyCombatSystem.Instance == null || PartyManager.Instance == null)
            {
                Debug.LogWarning("必要なシステムが見つかりません");
                return;
            }
            
            var activeParties = PartyManager.Instance.ActiveParties;
            
            foreach (var party in activeParties)
            {
                if (party.IsActive && party.Members.Count > 0)
                {
                    // 各種パーティスキルをテスト
                    PartyCombatSystem.Instance.ApplyPartySkill(party, PartySkillType.GroupHeal, 20f);
                    
                    // 少し待ってから次のスキル
                    StartCoroutine(DelayedSkillTest(party));
                }
            }
        }
        
        private System.Collections.IEnumerator DelayedSkillTest(IParty party)
        {
            yield return new WaitForSeconds(1f);
            PartyCombatSystem.Instance.ApplyPartySkill(party, PartySkillType.AttackBoost, 1.5f);
            
            yield return new WaitForSeconds(1f);
            PartyCombatSystem.Instance.ApplyPartySkill(party, PartySkillType.DefenseBoost, 1.3f);
        }
        
        private void OnDestroy()
        {
            CleanupTestObjects();
        }
        
        private void OnDrawGizmosSelected()
        {
            // テスト位置を可視化
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(monsterPartyPosition, 1f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(invaderPartyPosition, 1f);
            
            // パーティ間の距離を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(monsterPartyPosition, invaderPartyPosition);
        }
    }
}