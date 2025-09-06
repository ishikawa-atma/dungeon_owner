using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Invaders
{
    public class Rogue : BaseInvader
    {
        [Header("Rogue Specific")]
        [SerializeField] private float stealthDuration = 3f;
        [SerializeField] private float stealthCooldown = 12f;
        [SerializeField] private bool isStealthActive = false;
        [SerializeField] private float lastStealthTime = -12f;
        [SerializeField] private float criticalChance = 0.3f; // 30%のクリティカル率
        [SerializeField] private float criticalMultiplier = 2f;

        protected override void InitializeInvader()
        {
            base.InitializeInvader();
            
            // 盗賊は高い移動速度と回避能力を持つ
            if (invaderData != null && invaderData.type == InvaderType.Rogue)
            {
                moveSpeed *= 1.3f; // 移動速度30%アップ
                Debug.Log($"Rogue {name} initialized with Stealth ability");
            }
        }

        protected override void UpdateMovement()
        {
            // ステルス使用判定
            if (!isStealthActive && Time.time > lastStealthTime + stealthCooldown)
            {
                var enemies = FindNearbyEnemies();
                if (enemies.Count > 0) // 敵が近くにいる場合にステルスを使用
                {
                    ActivateStealth();
                }
            }

            base.UpdateMovement();
        }

        private void ActivateStealth()
        {
            isStealthActive = true;
            lastStealthTime = Time.time;
            
            // ステルスエフェクト（半透明化）
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0.3f;
                spriteRenderer.color = color;
            }
            
            if (animator != null)
            {
                animator.SetTrigger("Stealth");
            }
            
            // ステルス効果を一定時間後に解除
            Invoke(nameof(DeactivateStealth), stealthDuration);
            
            Debug.Log($"{name} activated Stealth!");
        }

        private void DeactivateStealth()
        {
            isStealthActive = false;
            
            // 透明度を元に戻す
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1f;
                spriteRenderer.color = color;
            }
            
            Debug.Log($"{name} stealth deactivated");
        }

        protected override void AttackEnemy(GameObject enemy)
        {
            if (enemy == null) return;

            float damage = attackPower;
            
            // クリティカル判定
            bool isCritical = Random.value < criticalChance;
            if (isCritical)
            {
                damage *= criticalMultiplier;
                Debug.Log($"{name} landed a critical hit!");
            }
            
            // ステルス中の攻撃は必ずクリティカル
            if (isStealthActive)
            {
                damage *= criticalMultiplier;
                DeactivateStealth(); // ステルス解除
                Debug.Log($"{name} backstab attack from stealth!");
            }

            // 攻撃処理
            var monster = enemy.GetComponent<Interfaces.IMonster>();
            if (monster != null)
            {
                monster.TakeDamage(damage);
                lastAttackTime = Time.time;
                
                // 攻撃エフェクト
                PlayAttackEffect();
                
                Debug.Log($"{name} attacked {enemy.name} for {damage} damage {(isCritical ? "(Critical!)" : "")}");
            }
        }

        public override void TakeDamage(float damage)
        {
            // ステルス中は攻撃を受けにくい
            if (isStealthActive)
            {
                if (Random.value < 0.7f) // 70%の確率で回避
                {
                    Debug.Log($"{name} avoided damage while stealthed!");
                    return;
                }
                else
                {
                    DeactivateStealth(); // 攻撃を受けたらステルス解除
                }
            }

            base.TakeDamage(damage);
        }

        protected override void FindNextTarget()
        {
            // 盗賊は最短経路を選ぶ
            base.FindNextTarget();
            
            // 敵を避けるルートを選択する処理を追加可能
        }

        protected override void OnStateChanged(InvaderState newState)
        {
            base.OnStateChanged(newState);
            
            // 盗賊特有の状態変更処理
            if (newState == InvaderState.Fighting)
            {
                // 戦闘開始時にステルスを使用する可能性
            }
        }

        protected override void Die()
        {
            // ステルス効果を解除してから死亡
            if (isStealthActive)
            {
                DeactivateStealth();
            }
            
            base.Die();
        }
    }
}