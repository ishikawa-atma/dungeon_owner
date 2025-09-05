using UnityEngine;
using DungeonOwner.Data;
using System.Collections.Generic;

namespace DungeonOwner.Invaders
{
    public class Cleric : BaseInvader
    {
        [Header("Cleric Specific")]
        [SerializeField] private float healAmount = 30f;
        [SerializeField] private float healRange = 3f;
        [SerializeField] private float healCooldown = 5f;
        [SerializeField] private float lastHealTime = -5f;
        [SerializeField] private float selfHealThreshold = 0.5f; // 50%以下で自己回復

        protected override void InitializeInvader()
        {
            base.InitializeInvader();
            
            // 僧侶は回復能力を持つ
            if (invaderData != null && invaderData.type == InvaderType.Cleric)
            {
                healAmount = attackPower * 0.8f; // 攻撃力の80%を回復量とする
                Debug.Log($"Cleric {name} initialized with Heal ability");
            }
        }

        protected override void UpdateFighting()
        {
            // 回復の優先判定
            if (Time.time > lastHealTime + healCooldown)
            {
                // 自分の体力が低い場合は自己回復
                if (currentHealth < MaxHealth * selfHealThreshold)
                {
                    CastHeal(gameObject);
                    return;
                }
                
                // パーティメンバーの回復
                if (Party != null)
                {
                    var injuredMember = FindMostInjuredPartyMember();
                    if (injuredMember != null)
                    {
                        CastHeal(injuredMember.gameObject);
                        return;
                    }
                }
                
                // 近くの味方侵入者を回復
                var injuredAlly = FindNearbyInjuredAlly();
                if (injuredAlly != null)
                {
                    CastHeal(injuredAlly);
                    return;
                }
            }

            base.UpdateFighting();
        }

        protected override void UpdateMovement()
        {
            // 移動中も回復を優先
            if (Time.time > lastHealTime + healCooldown)
            {
                if (currentHealth < MaxHealth * selfHealThreshold)
                {
                    CastHeal(gameObject);
                    return;
                }
            }

            base.UpdateMovement();
        }

        private void CastHeal(GameObject target)
        {
            if (target == null) return;

            lastHealTime = Time.time;
            
            // 回復エフェクト
            if (animator != null)
            {
                animator.SetTrigger("Heal");
            }
            
            // 回復処理
            var invader = target.GetComponent<IInvader>();
            if (invader != null)
            {
                float currentHp = invader.Health;
                float maxHp = invader.MaxHealth;
                float newHp = Mathf.Min(maxHp, currentHp + healAmount);
                
                // BaseInvaderの場合は直接回復
                var baseInvader = target.GetComponent<BaseInvader>();
                if (baseInvader != null)
                {
                    baseInvader.currentHealth = newHp;
                    Debug.Log($"{name} healed {target.name} for {healAmount}. HP: {newHp}/{maxHp}");
                }
            }
            
            // 回復エフェクトを対象に表示
            CreateHealEffect(target.transform.position);
        }

        private IInvader FindMostInjuredPartyMember()
        {
            if (Party == null) return null;

            IInvader mostInjured = null;
            float lowestHealthRatio = 1f;

            foreach (var member in Party.Members)
            {
                var invader = member as IInvader;
                if (invader != null && invader != (IInvader)this)
                {
                    float healthRatio = invader.Health / invader.MaxHealth;
                    if (healthRatio < lowestHealthRatio && healthRatio < 0.8f) // 80%以下の場合
                    {
                        lowestHealthRatio = healthRatio;
                        mostInjured = invader;
                    }
                }
            }

            return mostInjured;
        }

        private GameObject FindNearbyInjuredAlly()
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, healRange);
            
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && collider.CompareTag("Invader"))
                {
                    var invader = collider.GetComponent<IInvader>();
                    if (invader != null && invader.Health < invader.MaxHealth * 0.8f)
                    {
                        return collider.gameObject;
                    }
                }
            }
            
            return null;
        }

        private void CreateHealEffect(Vector3 position)
        {
            // 回復エフェクトの生成（パーティクルシステムなど）
            // TODO: 実際のエフェクトプレハブを使用
            Debug.Log($"Heal effect created at {position}");
        }

        protected override void AttackEnemy(GameObject enemy)
        {
            // 僧侶は攻撃力が低いが、回復を優先する
            base.AttackEnemy(enemy);
        }

        public override void TakeDamage(float damage)
        {
            base.TakeDamage(damage);
            
            // ダメージを受けたら回復を試みる
            if (currentHealth < MaxHealth * selfHealThreshold && Time.time > lastHealTime + healCooldown * 0.5f)
            {
                CastHeal(gameObject);
            }
        }

        protected override void OnStateChanged(InvaderState newState)
        {
            base.OnStateChanged(newState);
            
            // 僧侶特有の状態変更処理
            if (newState == InvaderState.Fighting)
            {
                // 戦闘開始時に味方の回復を優先
            }
        }

        // デバッグ用
        private void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // 回復範囲を表示
            Gizmos.color = Color.green;
            Gizmos.DrawWireCircle(transform.position, healRange);
        }
    }
}