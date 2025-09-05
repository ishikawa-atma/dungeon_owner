using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Monsters;
using DungeonOwner.Core.Abilities;

namespace DungeonOwner.Core
{
    /// <summary>
    /// モンスターアビリティシステムのテスター
    /// 要件4.1, 4.2, 4.4, 4.5の動作確認用
    /// </summary>
    public class MonsterAbilityTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool runTestsOnStart = true;
        [SerializeField] private bool enableDebugLogs = true;
        
        [Header("Test Monsters")]
        [SerializeField] private GameObject slimePrefab;
        [SerializeField] private GameObject skeletonPrefab;
        [SerializeField] private GameObject ghostPrefab;
        
        private List<GameObject> testMonsters = new List<GameObject>();

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }

        private System.Collections.IEnumerator RunAllTests()
        {
            LogTest("=== モンスターアビリティシステムテスト開始 ===");
            
            yield return StartCoroutine(TestSlimeAutoHeal());
            yield return new WaitForSeconds(2f);
            
            yield return StartCoroutine(TestSkeletonAutoRevive());
            yield return new WaitForSeconds(2f);
            
            yield return StartCoroutine(TestGhostAutoRevive());
            yield return new WaitForSeconds(2f);
            
            LogTest("=== 全テスト完了 ===");
            CleanupTestMonsters();
        }

        private System.Collections.IEnumerator TestSlimeAutoHeal()
        {
            LogTest("--- スライム自動体力回復テスト ---");
            
            if (slimePrefab == null)
            {
                LogTest("ERROR: Slime prefab not assigned!");
                yield break;
            }

            // スライムを生成
            GameObject slimeObj = Instantiate(slimePrefab, Vector3.zero, Quaternion.identity);
            testMonsters.Add(slimeObj);
            
            Slime slime = slimeObj.GetComponent<Slime>();
            if (slime == null)
            {
                LogTest("ERROR: Slime component not found!");
                yield break;
            }

            yield return new WaitForSeconds(1f); // 初期化待ち

            // 初期状態確認
            LogTest($"スライム初期HP: {slime.Health:F1}/{slime.MaxHealth:F1}");
            LogTest($"スライム初期MP: {slime.Mana:F1}/{slime.MaxMana:F1}");

            // ダメージを与える
            float initialHealth = slime.Health;
            slime.TakeDamage(slime.MaxHealth * 0.5f); // 50%ダメージ
            LogTest($"ダメージ後HP: {slime.Health:F1}/{slime.MaxHealth:F1}");

            // 自動回復を確認
            yield return new WaitForSeconds(3f);
            LogTest($"3秒後HP: {slime.Health:F1}/{slime.MaxHealth:F1}");

            // アクティブアビリティテスト
            bool abilityUsed = slime.UseAbility(MonsterAbilityType.AutoHeal);
            LogTest($"アクティブ回復使用: {abilityUsed}");
            LogTest($"アビリティ後HP: {slime.Health:F1}/{slime.MaxHealth:F1}");

            // アビリティ情報確認
            var autoHealAbility = slime.GetAbility(MonsterAbilityType.AutoHeal);
            if (autoHealAbility != null)
            {
                LogTest($"アビリティ名: {autoHealAbility.AbilityName}");
                LogTest($"説明: {autoHealAbility.Description}");
                LogTest($"クールダウン: {autoHealAbility.CooldownTime}秒");
                LogTest($"マナコスト: {autoHealAbility.ManaCost}");
            }

            LogTest("スライムテスト完了");
        }

        private System.Collections.IEnumerator TestSkeletonAutoRevive()
        {
            LogTest("--- スケルトン自動復活テスト ---");
            
            if (skeletonPrefab == null)
            {
                LogTest("ERROR: Skeleton prefab not assigned!");
                yield break;
            }

            // スケルトンを生成
            GameObject skeletonObj = Instantiate(skeletonPrefab, Vector3.right * 2f, Quaternion.identity);
            testMonsters.Add(skeletonObj);
            
            LesserSkeleton skeleton = skeletonObj.GetComponent<LesserSkeleton>();
            if (skeleton == null)
            {
                LogTest("ERROR: LesserSkeleton component not found!");
                yield break;
            }

            yield return new WaitForSeconds(1f); // 初期化待ち

            // 初期状態確認
            LogTest($"スケルトン初期レベル: {skeleton.Level}");
            LogTest($"スケルトン初期HP: {skeleton.Health:F1}/{skeleton.MaxHealth:F1}");
            LogTest($"残り復活回数: {skeleton.GetRemainingRevives()}");

            // 復活アビリティ確認
            var reviveAbility = skeleton.GetAbility(MonsterAbilityType.AutoRevive);
            if (reviveAbility is AutoReviveAbility autoRevive)
            {
                LogTest($"復活時間: {autoRevive.ReviveTime}秒");
                LogTest($"最大復活回数: {autoRevive.MaxRevives}");
            }

            // スケルトンを撃破
            LogTest("スケルトンを撃破...");
            skeleton.TakeDamage(skeleton.MaxHealth);
            
            LogTest($"撃破後状態: {skeleton.State}");
            LogTest($"復活中: {skeleton.IsReviving()}");

            // 復活を待つ（短縮版）
            LogTest("復活を待機中...");
            yield return new WaitForSeconds(5f); // 実際は30秒だが、テスト用に短縮

            LogTest("スケルトンテスト完了");
        }

        private System.Collections.IEnumerator TestGhostAutoRevive()
        {
            LogTest("--- ゴースト自動復活テスト ---");
            
            if (ghostPrefab == null)
            {
                LogTest("ERROR: Ghost prefab not assigned!");
                yield break;
            }

            // ゴーストを生成
            GameObject ghostObj = Instantiate(ghostPrefab, Vector3.left * 2f, Quaternion.identity);
            testMonsters.Add(ghostObj);
            
            LesserGhost ghost = ghostObj.GetComponent<LesserGhost>();
            if (ghost == null)
            {
                LogTest("ERROR: LesserGhost component not found!");
                yield break;
            }

            yield return new WaitForSeconds(1f); // 初期化待ち

            // 初期状態確認
            LogTest($"ゴースト初期レベル: {ghost.Level}");
            LogTest($"ゴースト初期HP: {ghost.Health:F1}/{ghost.MaxHealth:F1}");
            LogTest($"残り復活回数: {ghost.GetRemainingRevives()}");

            // フェーズアビリティテスト
            LogTest("フェーズアビリティテスト...");
            ghost.UseAbility(); // フェーズアビリティを使用
            LogTest($"フェーズ中: {ghost.IsPhased()}");

            // ダメージテスト（フェーズ中）
            float damageAmount = 50f;
            float healthBefore = ghost.Health;
            ghost.TakeDamage(damageAmount);
            float actualDamage = healthBefore - ghost.Health;
            LogTest($"フェーズ中ダメージ軽減: {damageAmount} -> {actualDamage:F1}");

            yield return new WaitForSeconds(2f);

            // ゴーストを撃破
            LogTest("ゴーストを撃破...");
            ghost.TakeDamage(ghost.MaxHealth);
            
            LogTest($"撃破後状態: {ghost.State}");
            LogTest($"復活中: {ghost.IsReviving()}");

            LogTest("ゴーストテスト完了");
        }

        private void CleanupTestMonsters()
        {
            foreach (var monster in testMonsters)
            {
                if (monster != null)
                {
                    DestroyImmediate(monster);
                }
            }
            testMonsters.Clear();
        }

        private void LogTest(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[MonsterAbilityTest] {message}");
            }
        }

        // 手動テスト用メソッド
        [ContextMenu("Run Slime Test")]
        public void RunSlimeTest()
        {
            StartCoroutine(TestSlimeAutoHeal());
        }

        [ContextMenu("Run Skeleton Test")]
        public void RunSkeletonTest()
        {
            StartCoroutine(TestSkeletonAutoRevive());
        }

        [ContextMenu("Run Ghost Test")]
        public void RunGhostTest()
        {
            StartCoroutine(TestGhostAutoRevive());
        }

        [ContextMenu("Cleanup Test Monsters")]
        public void CleanupTest()
        {
            CleanupTestMonsters();
        }
    }
}