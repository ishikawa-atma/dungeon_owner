using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Interfaces;
using DungeonOwner.Managers;

namespace DungeonOwner.Core
{
    /// <summary>
    /// パーティシステムのデモンストレーション用クラス
    /// 要件19.1, 19.2, 19.5の動作確認
    /// </summary>
    public class PartySystemDemo : MonoBehaviour
    {
        [Header("デモ設定")]
        [SerializeField] private bool autoStartDemo = true;
        [SerializeField] private float demoStepDelay = 2f;
        
        [Header("テスト用オブジェクト")]
        [SerializeField] private GameObject testCharacterPrefab;
        
        private PartyManager partyManager;
        private List<TestCharacter> testCharacters = new List<TestCharacter>();
        private IParty demoParty;
        
        private void Start()
        {
            InitializeDemo();
            
            if (autoStartDemo)
            {
                StartCoroutine(RunDemo());
            }
        }
        
        private void InitializeDemo()
        {
            // PartyManagerを取得または作成
            partyManager = FindObjectOfType<PartyManager>();
            if (partyManager == null)
            {
                GameObject managerObj = new GameObject("PartyManager");
                partyManager = managerObj.AddComponent<PartyManager>();
            }
            
            // テスト用キャラクタープレハブを作成
            if (testCharacterPrefab == null)
            {
                CreateTestCharacterPrefab();
            }
            
            Debug.Log("パーティシステムデモ初期化完了");
        }
        
        private void CreateTestCharacterPrefab()
        {
            testCharacterPrefab = new GameObject("TestCharacter");
            testCharacterPrefab.AddComponent<TestCharacter>();
            
            // 見た目用のスプライト
            SpriteRenderer sr = testCharacterPrefab.AddComponent<SpriteRenderer>();
            
            // 簡単な四角形スプライトを作成
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Color.blue;
            }
            texture.SetPixels(colors);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            sr.sprite = sprite;
        }
        
        private System.Collections.IEnumerator RunDemo()
        {
            Debug.Log("=== パーティシステムデモ開始 ===");
            
            // ステップ1: テストキャラクターを作成
            yield return StartCoroutine(CreateTestCharacters());
            yield return new WaitForSeconds(demoStepDelay);
            
            // ステップ2: パーティを作成
            yield return StartCoroutine(CreatePartyDemo());
            yield return new WaitForSeconds(demoStepDelay);
            
            // ステップ3: パーティ移動をテスト
            yield return StartCoroutine(TestPartyMovement());
            yield return new WaitForSeconds(demoStepDelay);
            
            // ステップ4: ダメージ分散をテスト
            yield return StartCoroutine(TestDamageDistribution());
            yield return new WaitForSeconds(demoStepDelay);
            
            // ステップ5: メンバー除去をテスト
            yield return StartCoroutine(TestMemberRemoval());
            yield return new WaitForSeconds(demoStepDelay);
            
            Debug.Log("=== パーティシステムデモ完了 ===");
        }
        
        private System.Collections.IEnumerator CreateTestCharacters()
        {
            Debug.Log("--- テストキャラクター作成 ---");
            
            for (int i = 0; i < 4; i++)
            {
                Vector3 position = new Vector3(i * 2f, 0, 0);
                GameObject charObj = Instantiate(testCharacterPrefab, position, Quaternion.identity);
                charObj.name = $"TestCharacter_{i}";
                
                TestCharacter testChar = charObj.GetComponent<TestCharacter>();
                testChar.Initialize($"キャラクター{i}", 100f);
                testCharacters.Add(testChar);
                
                Debug.Log($"テストキャラクター作成: {testChar.name} at {position}");
                yield return new WaitForSeconds(0.2f);
            }
        }
        
        private System.Collections.IEnumerator CreatePartyDemo()
        {
            Debug.Log("--- パーティ作成デモ ---");
            
            List<ICharacterBase> members = new List<ICharacterBase>();
            foreach (var character in testCharacters)
            {
                members.Add(character);
            }
            
            demoParty = partyManager.CreateParty(members, 0);
            
            if (demoParty != null)
            {
                Debug.Log($"✓ パーティ作成成功: メンバー数{demoParty.Members.Count}");
                
                // パーティの状態を表示
                if (demoParty is Party partyImpl)
                {
                    partyImpl.DebugPartyStatus();
                }
            }
            else
            {
                Debug.Log("✗ パーティ作成失敗");
            }
            
            yield return null;
        }
        
        private System.Collections.IEnumerator TestPartyMovement()
        {
            Debug.Log("--- パーティ移動テスト ---");
            
            if (demoParty == null)
            {
                Debug.Log("✗ パーティが存在しません");
                yield break;
            }
            
            Vector2[] moveTargets = {
                new Vector2(5, 0),
                new Vector2(5, 3),
                new Vector2(0, 3),
                new Vector2(0, 0)
            };
            
            foreach (var target in moveTargets)
            {
                Debug.Log($"パーティ移動: 目標位置{target}");
                partyManager.HandlePartyMovement(demoParty, target);
                
                yield return new WaitForSeconds(1f);
                
                // 移動結果を確認
                Debug.Log($"パーティ位置: {demoParty.Position}");
                for (int i = 0; i < demoParty.Members.Count; i++)
                {
                    Debug.Log($"  メンバー{i}位置: {demoParty.Members[i].Position}");
                }
            }
            
            Debug.Log("✓ パーティ移動テスト完了");
        }
        
        private System.Collections.IEnumerator TestDamageDistribution()
        {
            Debug.Log("--- ダメージ分散テスト ---");
            
            if (demoParty == null)
            {
                Debug.Log("✗ パーティが存在しません");
                yield break;
            }
            
            // ダメージ前のHP記録
            List<float> beforeHealth = new List<float>();
            foreach (var member in demoParty.Members)
            {
                beforeHealth.Add(member.Health);
                Debug.Log($"ダメージ前HP: {member.Health}");
            }
            
            // ダメージを分散適用
            float totalDamage = 40f;
            Debug.Log($"総ダメージ{totalDamage}を分散適用");
            demoParty.DistributeDamage(totalDamage);
            
            yield return new WaitForSeconds(0.5f);
            
            // ダメージ後のHP確認
            float actualTotalDamage = 0f;
            for (int i = 0; i < demoParty.Members.Count; i++)
            {
                float healthLoss = beforeHealth[i] - demoParty.Members[i].Health;
                actualTotalDamage += healthLoss;
                Debug.Log($"ダメージ後HP: {demoParty.Members[i].Health} (ダメージ: {healthLoss})");
            }
            
            Debug.Log($"期待総ダメージ: {totalDamage}, 実際の総ダメージ: {actualTotalDamage}");
            
            if (Mathf.Approximately(actualTotalDamage, totalDamage))
            {
                Debug.Log("✓ ダメージ分散テスト成功");
            }
            else
            {
                Debug.Log("✗ ダメージ分散テストに問題があります");
            }
        }
        
        private System.Collections.IEnumerator TestMemberRemoval()
        {
            Debug.Log("--- メンバー除去テスト ---");
            
            if (demoParty == null || demoParty.Members.Count == 0)
            {
                Debug.Log("✗ パーティまたはメンバーが存在しません");
                yield break;
            }
            
            int initialCount = demoParty.Members.Count;
            ICharacterBase targetMember = demoParty.Members[0];
            
            Debug.Log($"メンバー除去テスト: {targetMember.GetType().Name}を撃破");
            
            // 大ダメージを与えてHPを0にする
            targetMember.TakeDamage(1000f);
            
            yield return new WaitForSeconds(1f); // PartyクラスのUpdateで死亡メンバーが除去されるのを待つ
            
            int currentCount = demoParty.Members.Count;
            Debug.Log($"メンバー数変化: {initialCount} -> {currentCount}");
            
            if (currentCount == initialCount - 1)
            {
                Debug.Log("✓ 撃破されたメンバーの除去成功");
            }
            else
            {
                Debug.Log("✗ 撃破されたメンバーの除去に問題があります");
            }
            
            if (demoParty.IsActive && currentCount > 0)
            {
                Debug.Log("✓ 残りメンバーでパーティ継続中");
            }
            else if (currentCount == 0)
            {
                Debug.Log("✓ 全メンバー撃破によりパーティ解散");
            }
        }
        
        [ContextMenu("Start Demo")]
        public void StartDemo()
        {
            StartCoroutine(RunDemo());
        }
        
        [ContextMenu("Cleanup Demo")]
        public void CleanupDemo()
        {
            // パーティを解散
            if (demoParty != null)
            {
                partyManager.DisbandParty(demoParty);
                demoParty = null;
            }
            
            // テストキャラクターを削除
            foreach (var character in testCharacters)
            {
                if (character != null)
                {
                    DestroyImmediate(character.gameObject);
                }
            }
            testCharacters.Clear();
            
            Debug.Log("デモクリーンアップ完了");
        }
    }
    
    /// <summary>
    /// テスト用の簡単なキャラクタークラス
    /// </summary>
    public class TestCharacter : MonoBehaviour, ICharacterBase
    {
        [SerializeField] private float health = 100f;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private string characterName = "TestCharacter";
        
        public Vector2 Position 
        { 
            get => transform.position; 
            set => transform.position = new Vector3(value.x, value.y, transform.position.z); 
        }
        
        public float Health => health;
        public float MaxHealth => maxHealth;
        
        public void Initialize(string name, float maxHp)
        {
            characterName = name;
            maxHealth = maxHp;
            health = maxHp;
            gameObject.name = name;
        }
        
        public void TakeDamage(float damage)
        {
            health = Mathf.Max(0, health - damage);
            Debug.Log($"{characterName} took {damage} damage. Health: {health}/{maxHealth}");
            
            if (health <= 0)
            {
                Debug.Log($"{characterName} has been defeated!");
            }
        }
    }
}