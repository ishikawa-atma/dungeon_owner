using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Monsters
{
    /// <summary>
    /// リッチキング - 魔法攻撃と自動回復を持つボスキャラクター
    /// </summary>
    public class LichKing : BaseBoss
    {
        [Header("Lich King Settings")]
        [SerializeField] private float magicAttackRange = 5f;
        [SerializeField] private float magicAttackCooldown = 3f;
        [SerializeField] private float healAmount = 100f;
        [SerializeField] private float healCooldown = 10f;
        [SerializeField] private GameObject magicMissileEffect;
        [SerializeField] private GameObject healEffect;
        
        private float lastMagicAttackTime = 0f;
        private float lastHealTime = 0f;

        protected override void Start()
        {
            base.Start();
            
            // リッチキング専用の初期化
            if (monsterData == null)
            {
                // デフォルトステータス設定
                maxHealth = 600f;
                currentHealth = maxHealth;
                maxMana = 500f;
                currentMana = maxMana;
                attackPower = 100f;
            }

            // 魔法攻撃と自動回復アビリティを追加
            AddAbility(MonsterAbilityType.MagicAttack);
            AddAbility(MonsterAbilityType.AutoHeal);
            
            Debug.Log("LichKing initialized");
        }

        protected override void UpdateAbilities()
        {
            base.UpdateAbilities();

            // 魔法攻撃の処理
            if (CanUseMagicAttack())
            {
                UseMagicAttack();
            }

            // 自動回復の処理
            if (CanUseHeal())
            {
                UseHeal();
            }
        }

        private bool CanUseMagicAttack()
        {
            return Time.time - lastMagicAttackTime >= magicAttackCooldown &&
                   currentMana >= 30f && // MP消費
                   state == MonsterState.Fighting;
        }

        private void UseMagicAttack()
        {
            lastMagicAttackTime = Time.time;
            ConsumeMana(30f);

            // 範囲内の最も近い敵を検索
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, magicAttackRange);
            IInvader closestInvader = null;
            float closestDistance = float.MaxValue;

            foreach (Collider2D target in targets)
            {
                IInvader invader = target.GetComponent<IInvader>();
                if (invader != null)
                {
                    float distance = Vector2.Distance(transform.position, target.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInvader = invader;
                    }
                }
            }

            if (closestInvader != null)
            {
                // 魔法攻撃ダメージを与える
                float damage = attackPower * 2f; // 通常攻撃の2倍
                closestInvader.TakeDamage(damage);

                // エフェクト表示
                if (magicMissileEffect != null)
                {
                    Vector3 targetPos = (closestInvader as MonoBehaviour).transform.position;
                    GameObject effect = Instantiate(magicMissileEffect, transform.position, Quaternion.identity);
                    
                    // エフェクトを敵に向かって移動させる（簡易実装）
                    StartCoroutine(MoveEffectToTarget(effect, targetPos));
                }

                Debug.Log($"LichKing magic attack hit {closestInvader.Type} for {damage} damage");
            }
        }

        private System.Collections.IEnumerator MoveEffectToTarget(GameObject effect, Vector3 targetPos)
        {
            Vector3 startPos = effect.transform.position;
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (effect != null)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;
                    effect.transform.position = Vector3.Lerp(startPos, targetPos, t);
                }
                yield return null;
            }

            if (effect != null)
            {
                Destroy(effect);
            }
        }

        private bool CanUseHeal()
        {
            return Time.time - lastHealTime >= healCooldown &&
                   currentMana >= 40f && // MP消費
                   currentHealth < maxHealth * 0.7f; // HP70%以下で発動
        }

        private void UseHeal()
        {
            lastHealTime = Time.time;
            ConsumeMana(40f);

            // 自己回復
            Heal(healAmount);

            // エフェクト表示
            if (healEffect != null)
            {
                GameObject effect = Instantiate(healEffect, transform.position, Quaternion.identity);
                Destroy(effect, 3f);
            }

            Debug.Log($"LichKing healed for {healAmount} HP");
        }

        public override void UseAbility()
        {
            if (CanUseMagicAttack())
            {
                UseMagicAttack();
            }
            else if (CanUseHeal())
            {
                UseHeal();
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
                case MonsterAbilityType.MagicAttack:
                    if (CanUseMagicAttack())
                    {
                        UseMagicAttack();
                        return true;
                    }
                    return false;
                case MonsterAbilityType.AutoHeal:
                    if (CanUseHeal())
                    {
                        UseHeal();
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
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCircle(transform.position, magicAttackRange);
        }
    }
}