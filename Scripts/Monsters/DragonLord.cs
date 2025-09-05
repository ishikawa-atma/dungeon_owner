using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Monsters
{
    /// <summary>
    /// ドラゴンロード - 範囲攻撃を持つボスキャラクター
    /// </summary>
    public class DragonLord : BaseBoss
    {
        [Header("Dragon Lord Settings")]
        [SerializeField] private float areaAttackRange = 3f;
        [SerializeField] private float areaAttackCooldown = 5f;
        [SerializeField] private GameObject fireBreathEffect;
        
        private float lastAreaAttackTime = 0f;

        protected override void Start()
        {
            base.Start();
            
            // ドラゴンロード専用の初期化
            if (monsterData == null)
            {
                // デフォルトステータス設定
                maxHealth = 800f;
                currentHealth = maxHealth;
                maxMana = 300f;
                currentMana = maxMana;
                attackPower = 120f;
            }

            // 範囲攻撃アビリティを追加
            AddAbility(MonsterAbilityType.AreaAttack);
            
            Debug.Log("DragonLord initialized");
        }

        protected override void UpdateAbilities()
        {
            base.UpdateAbilities();

            // 範囲攻撃の処理
            if (CanUseAreaAttack())
            {
                UseAreaAttack();
            }
        }

        private bool CanUseAreaAttack()
        {
            return Time.time - lastAreaAttackTime >= areaAttackCooldown &&
                   currentMana >= 50f && // MP消費
                   state == MonsterState.Fighting;
        }

        private void UseAreaAttack()
        {
            lastAreaAttackTime = Time.time;
            ConsumeMana(50f);

            // 範囲内の敵を検索
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, areaAttackRange);
            
            foreach (Collider2D target in targets)
            {
                IInvader invader = target.GetComponent<IInvader>();
                if (invader != null)
                {
                    // 範囲攻撃ダメージを与える
                    float damage = attackPower * 1.5f; // 通常攻撃の1.5倍
                    invader.TakeDamage(damage);
                    
                    Debug.Log($"DragonLord area attack hit {invader.Type} for {damage} damage");
                }
            }

            // エフェクト表示
            if (fireBreathEffect != null)
            {
                GameObject effect = Instantiate(fireBreathEffect, transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }

            Debug.Log("DragonLord used area attack");
        }

        public override void UseAbility()
        {
            if (CanUseAreaAttack())
            {
                UseAreaAttack();
            }
            else
            {
                base.UseAbility();
            }
        }

        public override bool UseAbility(MonsterAbilityType abilityType)
        {
            switch (abilityType)
            {
                case MonsterAbilityType.AreaAttack:
                    if (CanUseAreaAttack())
                    {
                        UseAreaAttack();
                        return true;
                    }
                    return false;
                default:
                    return base.UseAbility(abilityType);
            }
        }

        // デバッグ用の範囲表示
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCircle(transform.position, areaAttackRange);
        }
    }
}