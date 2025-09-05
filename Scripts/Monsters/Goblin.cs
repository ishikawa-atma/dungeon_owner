using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Monsters
{
    public class Goblin : BaseMonster
    {
        [Header("Goblin Specific")]
        [SerializeField] private float attackSpeedMultiplier = 1.5f; // 攻撃速度倍率
        [SerializeField] private float criticalChance = 0.2f; // クリティカル確率
        [SerializeField] private float criticalMultiplier = 2f; // クリティカル倍率
        
        private float lastAttackTime;
        private bool isRaging = false;

        protected override void UpdateMonsterBehavior()
        {
            // ゴブリンは攻撃的で素早い
            if (currentState == MonsterState.Idle && IsAlive())
            {
                // 敵を積極的に探索
                ProcessAggressiveBehavior();
            }
        }

        protected override void ExecuteAbility()
        {
            // ゴブリンのアビリティ：狂戦士の怒り（攻撃力・速度アップ）
            if (currentMana >= 15f)
            {
                currentMana -= 15f;
                StartBerserkerRage();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentMana >= 15f && !isRaging;
        }

        protected override float GetAbilityCooldown()
        {
            return 12f; // 12秒クールダウン
        }

        private void ProcessAggressiveBehavior()
        {
            // ゴブリンの攻撃的な行動パターン
            // 近くの敵を探して攻撃する（後で実装）
        }

        private void StartBerserkerRage()
        {
            isRaging = true;
            ShowRageEffect();
            
            // 8秒間効果持続
            Invoke(nameof(EndBerserkerRage), 8f);
        }

        private void EndBerserkerRage()
        {
            isRaging = false;
            ShowRageEndEffect();
        }

        public float GetCurrentAttackPower()
        {
            float baseAttack = GetAttackPower();
            
            // 狂戦士状態では攻撃力1.5倍
            if (isRaging)
            {
                baseAttack *= 1.5f;
            }
            
            return baseAttack;
        }

        public bool RollCritical()
        {
            return Random.Range(0f, 1f) < criticalChance;
        }

        public float GetCriticalDamage(float baseDamage)
        {
            return baseDamage * criticalMultiplier;
        }

        public float GetAttackSpeed()
        {
            float baseSpeed = attackSpeedMultiplier;
            
            // 狂戦士状態では攻撃速度さらにアップ
            if (isRaging)
            {
                baseSpeed *= 1.3f;
            }
            
            return baseSpeed;
        }

        private void ShowRageEffect()
        {
            // TODO: 狂戦士エフェクト（赤いオーラなど）
            Debug.Log($"Goblin {gameObject.name} entered berserker rage!");
        }

        private void ShowRageEndEffect()
        {
            // TODO: 狂戦士終了エフェクト
            Debug.Log($"Goblin {gameObject.name} rage ended!");
        }

        protected override void OnDamageTaken(float damage)
        {
            base.OnDamageTaken(damage);
            
            // ダメージを受けると怒りやすくなる
            if (currentHealth < MaxHealth * 0.5f && !isRaging)
            {
                // 低HP時に自動で狂戦士発動の可能性
                if (Random.Range(0f, 1f) < 0.3f && CanUseAbility())
                {
                    UseAbility();
                }
            }
        }

        public bool IsRaging()
        {
            return isRaging;
        }
    }
}