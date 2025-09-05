using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Monsters;
using DungeonOwner.Core.Abilities;

namespace DungeonOwner.Core
{
    /// <summary>
    /// モンスターアビリティシステムのデモンストレーション
    /// 要件4.1, 4.2, 4.4, 4.5の実装確認用
    /// </summary>
    public class MonsterAbilitySystemDemo : MonoBehaviour
    {
        [Header("Demo Settings")]
        [SerializeField] private bool autoStartDemo = true;
        [SerializeField] private float demoInterval = 5f;
        
        [Header("Test Monsters")]
        [SerializeField] private Slime testSlime;
        [SerializeField] private LesserSkeleton testSkeleton;
        [SerializeField] private LesserGhost testGhost;

        private void Start()
        {
            if (autoStartDemo)
            {
                InvokeRepeating(nameof(RunDemo), 1f, demoInterval);
            }
        }

        [ContextMenu("Run Demo")]
        public void RunDemo()
        {
            Debug.Log("=== モンスターアビリティシステムデモ ===");
            
            DemoSlimeAbilities();
            DemoSkeletonAbilities();
            DemoGhostAbilities();
        }

        private void DemoSlimeAbilities()
        {
            if (testSlime == null) return;
            
            Debug.Log("--- スライムアビリティデモ ---");
            
            // 現在の状態を表示
            Debug.Log($"スライム HP: {testSlime.Health:F1}/{testSlime.MaxHealth:F1}");
            Debug.Log($"スライム MP: {testSlime.Mana:F1}/{testSlime.MaxMana:F1}");
            
            // アビリティ情報を表示
            var autoHealAbility = testSlime.GetAbility(MonsterAbilityType.AutoHeal);
            if (autoHealAbility != null)
            {
                Debug.Log($"自動回復アビリティ: {autoHealAbility.AbilityName}");
                Debug.Log($"使用可能: {autoHealAbility.CanUse}");
                Debug.Log($"クールダウン: {autoHealAbility.CooldownTime}秒");
            }
            
            // ダメージを与えてアビリティをテスト
            if (testSlime.Health > testSlime.MaxHealth * 0.5f)
            {
                testSlime.TakeDamage(testSlime.MaxHealth * 0.3f);
                Debug.Log($"ダメージ後 HP: {testSlime.Health:F1}/{testSlime.MaxHealth:F1}");
            }
            
            // アクティブアビリティを使用
            bool abilityUsed = testSlime.UseAbility(MonsterAbilityType.AutoHeal);
            Debug.Log($"アクティブ回復使用: {abilityUsed}");
        }

        private void DemoSkeletonAbilities()
        {
            if (testSkeleton == null) return;
            
            Debug.Log("--- スケルトンアビリティデモ ---");
            
            // 現在の状態を表示
            Debug.Log($"スケルトン レベル: {testSkeleton.Level}");
            Debug.Log($"スケルトン HP: {testSkeleton.Health:F1}/{testSkeleton.MaxHealth:F1}");
            Debug.Log($"残り復活回数: {testSkeleton.GetRemainingRevives()}");
            Debug.Log($"復活中: {testSkeleton.IsReviving()}");
            
            // 復活アビリティ情報を表示
            var reviveAbility = testSkeleton.GetAbility(MonsterAbilityType.AutoRevive);
            if (reviveAbility is AutoReviveAbility autoRevive)
            {
                Debug.Log($"復活アビリティ: {autoRevive.AbilityName}");
                Debug.Log($"復活時間: {autoRevive.ReviveTime}秒");
                Debug.Log($"最大復活回数: {autoRevive.MaxRevives}");
            }
        }

        private void DemoGhostAbilities()
        {
            if (testGhost == null) return;
            
            Debug.Log("--- ゴーストアビリティデモ ---");
            
            // 現在の状態を表示
            Debug.Log($"ゴースト レベル: {testGhost.Level}");
            Debug.Log($"ゴースト HP: {testGhost.Health:F1}/{testGhost.MaxHealth:F1}");
            Debug.Log($"残り復活回数: {testGhost.GetRemainingRevives()}");
            Debug.Log($"復活中: {testGhost.IsReviving()}");
            Debug.Log($"フェーズ中: {testGhost.IsPhased()}");
            
            // フェーズアビリティをテスト
            if (!testGhost.IsPhased())
            {
                testGhost.UseAbility(); // フェーズアビリティを使用
                Debug.Log($"フェーズアビリティ使用後: {testGhost.IsPhased()}");
            }
        }

        [ContextMenu("Damage All Monsters")]
        public void DamageAllMonsters()
        {
            if (testSlime != null)
            {
                testSlime.TakeDamage(testSlime.MaxHealth * 0.5f);
                Debug.Log("スライムにダメージを与えました");
            }
            
            if (testSkeleton != null)
            {
                testSkeleton.TakeDamage(testSkeleton.MaxHealth);
                Debug.Log("スケルトンを撃破しました");
            }
            
            if (testGhost != null)
            {
                testGhost.TakeDamage(testGhost.MaxHealth);
                Debug.Log("ゴーストを撃破しました");
            }
        }

        [ContextMenu("Heal All Monsters")]
        public void HealAllMonsters()
        {
            if (testSlime != null)
            {
                testSlime.Heal(testSlime.MaxHealth);
                Debug.Log("スライムを回復しました");
            }
            
            if (testSkeleton != null)
            {
                testSkeleton.Heal(testSkeleton.MaxHealth);
                Debug.Log("スケルトンを回復しました");
            }
            
            if (testGhost != null)
            {
                testGhost.Heal(testGhost.MaxHealth);
                Debug.Log("ゴーストを回復しました");
            }
        }

        [ContextMenu("Show All Abilities")]
        public void ShowAllAbilities()
        {
            Debug.Log("=== 全モンスターのアビリティ情報 ===");
            
            ShowMonsterAbilities("スライム", testSlime);
            ShowMonsterAbilities("スケルトン", testSkeleton);
            ShowMonsterAbilities("ゴースト", testGhost);
        }

        private void ShowMonsterAbilities(string monsterName, BaseMonster monster)
        {
            if (monster == null) return;
            
            Debug.Log($"--- {monsterName} ---");
            var abilities = monster.GetAllAbilities();
            
            if (abilities.Count == 0)
            {
                Debug.Log($"{monsterName}: アビリティなし");
                return;
            }
            
            foreach (var ability in abilities)
            {
                Debug.Log($"{monsterName}: {ability.AbilityName} - {ability.Description}");
                Debug.Log($"  クールダウン: {ability.CooldownTime}秒, マナコスト: {ability.ManaCost}");
                Debug.Log($"  使用可能: {ability.CanUse}");
            }
        }
    }
}