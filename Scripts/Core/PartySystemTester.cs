using System.Collections.Generic;
using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Interfaces;
using DungeonOwner.Managers;
using DungeonOwner.Monsters;
using DungeonOwner.Invaders;

namespace DungeonOwner.Core
{
    /// <summary>
    /// パーティシステムのテスト用クラス
    /// 要件19.1, 19.2, 19.5の動作確認
    /// </summary>
    public class PartySystemTester : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enableDetailedLogs = true;
        
        [Header("テスト用プレハブ")]
        [SerializeField] private GameObject slimePrefab;
        [SerializeField] private GameObject skeletonPrefab;
        [SerializeField] private GameObject warriorPrefab;
        
        private PartyManager partyManager;
        private List<IParty> testParties = new List<IParty>();
        
        private void Start()
        {
            InitializeComponents();
            
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }
        
        private void InitializeComponents()
        {
            partyManager = FindObjectOfType<PartyManager>();
            if (partyManager == null)
            {
                GameObject managerObj = new GameObject("PartyManager");
                partyManager = managerObj.AddComponent<PartyManager>();
            }
            
            CreateTestPrefabs();
        }
        
        private void CreateTestPrefabs()
        {
            if (slimePrefab == null)
            {
                slimePrefab = CreateTestMonsterPrefab("TestSlime", typeof(Slime));
            }
            
            if (skeletonPrefab == null)
            {
                skeletonPrefab = CreateTestMonsterPrefab("TestSkeleton", typeof(LesserSkeleton));
            }
            
            if (warriorPrefab == null)
            {
                warriorPrefab = CreateTestInvaderPrefab("TestWarrior", typeof(Warrior));
            }
        }
        
        private GameObject CreateTestMonsterPrefab(string name, System.Type componentType)
        {
            GameObject prefab = new GameObject(name);
            prefab.AddComponent(componentType);
            return prefab;
        }
        
        private GameObject CreateTestInvaderPrefab(string name, System.Type componentType)
        {
            GameObject prefab = new GameObject(name);
            prefab.AddComponent(componentType);
            return prefab;
        }
        
        private System.Collections.IEnumerator RunAllTests()
        {
            LogTest("=== パーティシステムテスト開始 ===");
            
            yield return StartCoroutine(TestPartyCreation());
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(TestPartyMovement());
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(TestDamageDistribution());
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(TestMemberRemoval());
            yield return new WaitForSeconds(1f);
            
            yield return StartCoroutine(TestPartyDisbanding());
            
            LogTest("=== パーティシステムテスト完了 ===");
        }
        
        /// <summary>
        /// パーティ作成テスト
        /// 要件19.1: パーティメンバーの同期移動システム
        /// </summary>
        private System.Collections.IEnumerator TestPartyCreation()
        {
            LogTest("--- パーティ作成テスト ---");
            
            // テスト用モンスターを作成
            List<ICharacterBase> members = new List<ICharacterBase>();
            
            for (int i = 0; i < 3; i++)
            {
                GameObject monsterObj = Instantiate(slimePrefab);
                monsterObj.transform.position = new Vector3(i, 0, 0);
                
                ICharacterBase monster = monsterObj.GetComponent<ICharacterBase>();
                if (monster != null)
                {
                    members.Add(monster);
                    LogTest($"テストモンスター{i}作成: {monster.GetType().Name}");
                }
            }
            
            // パーティを作成
            IParty party = partyManager.CreateParty(members, 0);
            
            if (party != null)
            {
                testParties.Add(party);
                LogTest($"✓ パーティ作成成功: メンバー数{party.Members.Count}");
                
                // パーティの状態を確認
                if (party is Party partyImpl)
                {
                    partyImpl.DebugPartyStatus();
                }
            }
            else
            {
                LogTest("✗ パーティ作成失敗");
            }
            
            yield return null;
        }
        
        /// <summary>
        /// パーティ移動テスト
        /// 要件19.1: パーティメンバーの同期移動システム
        /// </summary>
        private System.Collections.IEnumerator TestPartyMovement()
        {
            LogTest("--- パーティ移動テスト ---");
            
            if (testParties.Count == 0)
            {
                LogTest("✗ テスト用パーティが存在しません");
                yield break;
            }
            
            IParty party = testParties[0];
            Vector2 targetPosition = new Vector2(5, 3);
            
            LogTest($"パーティ移動開始: 目標位置{targetPosition}");
            
            // 移動前の位置を記録
            List<Vector2> beforePositions = new List<Vector2>();
            foreach (var member in party.Members)
            {
                beforePositions.Add(member.Position);
            }
            
            // パーティを移動
            partyManager.HandlePartyMovement(party, targetPosition);
            
            yield return new WaitForSeconds(0.1f);
            
            // 移動後の位置を確認
            bool allMembersMoved = true;
            for (int i = 0; i < party.Members.Count; i++)
            {
                Vector2 newPos = party.Members[i].Position;
                Vector2 oldPos = beforePositions[i];
                
                if (Vector2.Distance(newPos, oldPos) < 0.1f)
                {
                    allMembersMoved = false;
                    LogTest($"✗ メンバー{i}が移動していません: {oldPos} -> {newPos}");
                }
                else
                {
                    LogTest($"✓ メンバー{i}移動確認: {oldPos} -> {newPos}");
                }
            }
            
            if (allMembersMoved)
            {
                LogTest("✓ パーティ同期移動成功");
            }
            else
            {
                LogTest("✗ パーティ同期移動に問題があります");
            }
        }
        
        /// <summary>
        /// ダメージ分散テスト
        /// 要件19.2: パーティ内でのダメージ分散機能
        /// </summary>
        private System.Collections.IEnumerator TestDamageDistribution()
        {
            LogTest("--- ダメージ分散テスト ---");
            
            if (testParties.Count == 0)
            {
                LogTest("✗ テスト用パーティが存在しません");
                yield break;
            }
            
            IParty party = testParties[0];
            float totalDamage = 30f;
            
            // ダメージ適用前のHP記録
            List<float> beforeHealth = new List<float>();
            foreach (var member in party.Members)
            {
                beforeHealth.Add(member.Health);
                LogTest($"ダメージ前HP: {member.GetType().Name} = {member.Health}");
            }
            
            // ダメージを分散適用
            party.DistributeDamage(totalDamage);
            
            yield return new WaitForSeconds(0.1f);
            
            // ダメージ適用後のHP確認
            float totalDamageDealt = 0f;
            for (int i = 0; i < party.Members.Count; i++)
            {
                float healthLoss = beforeHealth[i] - party.Members[i].Health;
                totalDamageDealt += healthLoss;
                LogTest($"ダメージ後HP: {party.Members[i].GetType().Name} = {party.Members[i].Health} (ダメージ: {healthLoss})");
            }
            
            float expectedDamagePerMember = totalDamage / party.Members.Count;
            LogTest($"期待ダメージ/メンバー: {expectedDamagePerMember}, 実際の総ダメージ: {totalDamageDealt}");
            
            if (Mathf.Approximately(totalDamageDealt, totalDamage))
            {
                LogTest("✓ ダメージ分散成功");
            }
            else
            {
                LogTest("✗ ダメージ分散に問題があります");
            }
        }
        
        /// <summary>
        /// メンバー除去テスト
        /// 要件19.5: パーティメンバーが撃破された場合の継続処理
        /// </summary>
        private System.Collections.IEnumerator TestMemberRemoval()
        {
            LogTest("--- メンバー除去テスト ---");
            
            if (testParties.Count == 0)
            {
                LogTest("✗ テスト用パーティが存在しません");
                yield break;
            }
            
            IParty party = testParties[0];
            int initialMemberCount = party.Members.Count;
            
            if (initialMemberCount == 0)
            {
                LogTest("✗ パーティにメンバーがいません");
                yield break;
            }
            
            // 最初のメンバーを撃破（HPを0にする）
            ICharacterBase firstMember = party.Members[0];
            LogTest($"メンバー撃破テスト: {firstMember.GetType().Name}のHPを0にします");
            
            // 大ダメージを与えてHPを0にする
            firstMember.TakeDamage(1000f);
            
            yield return new WaitForSeconds(0.5f); // PartyクラスのUpdateで死亡メンバーが除去されるのを待つ
            
            // メンバー数の変化を確認
            int currentMemberCount = party.Members.Count;
            LogTest($"メンバー数変化: {initialMemberCount} -> {currentMemberCount}");
            
            if (currentMemberCount == initialMemberCount - 1)
            {
                LogTest("✓ 撃破されたメンバーの除去成功");
            }
            else
            {
                LogTest("✗ 撃破されたメンバーの除去に問題があります");
            }
            
            // パーティが継続しているか確認
            if (party.IsActive && currentMemberCount > 0)
            {
                LogTest("✓ 残りメンバーでパーティ継続中");
            }
            else if (currentMemberCount == 0)
            {
                LogTest("✓ 全メンバー撃破によりパーティ解散");
            }
            else
            {
                LogTest("✗ パーティ状態に問題があります");
            }
        }
        
        /// <summary>
        /// パーティ解散テスト
        /// </summary>
        private System.Collections.IEnumerator TestPartyDisbanding()
        {
            LogTest("--- パーティ解散テスト ---");
            
            foreach (var party in testParties.ToArray())
            {
                if (party != null && party.IsActive)
                {
                    LogTest($"パーティ解散: メンバー数{party.Members.Count}");
                    partyManager.DisbandParty(party);
                }
            }
            
            yield return new WaitForSeconds(0.1f);
            
            testParties.Clear();
            LogTest("✓ 全テストパーティ解散完了");
        }
        
        private void LogTest(string message)
        {
            if (enableDetailedLogs)
            {
                Debug.Log($"[PartySystemTest] {message}");
            }
        }
        
        /// <summary>
        /// 手動テスト実行用メソッド
        /// </summary>
        [ContextMenu("Run Party Tests")]
        public void RunTests()
        {
            StartCoroutine(RunAllTests());
        }
        
        /// <summary>
        /// テストクリーンアップ
        /// </summary>
        [ContextMenu("Cleanup Tests")]
        public void CleanupTests()
        {
            foreach (var party in testParties)
            {
                if (party != null)
                {
                    partyManager.DisbandParty(party);
                }
            }
            testParties.Clear();
            
            LogTest("テストクリーンアップ完了");
        }
    }
}