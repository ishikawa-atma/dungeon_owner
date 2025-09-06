using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Invaders
{
    public class Warrior : BaseInvader
    {
        [Header("Warrior Specific")]
        [SerializeField] private float shieldDuration = 5f;
        [SerializeField] private float shieldCooldown = 15f;
        [SerializeField] private bool isShieldActive = false;
        [SerializeField] private float lastShieldTime = -15f;

        protected override void InitializeInvader()
        {
            base.InitializeInvader();
            
            // 戦士は高い体力と攻撃力を持つ
            if (invaderData != null && invaderData.type == InvaderType.Warrior)
            {
                // 戦士特有の初期化
                Debug.Log($"Warrior {name} initialized with Shield ability");
            }
        }

        protected override void UpdateFighting()
        {
            // シールドアビリティの使用判定
            if (!isShieldActive && Time.time > lastShieldTime + shieldCooldown)
            {
                var enemies = FindNearbyEnemies();
                if (enemies.Count >= 2) // 複数の敵がいる場合にシールドを使用
                {
                    ActivateShield();
                }
            }

            base.UpdateFighting();
        }

        private void ActivateShield()
        {
            isShieldActive = true;
            lastShieldTime = Time.time;
            
            // シールドエフェクト
            if (animator != null)
            {
                animator.SetTrigger("Shield");
            }
            
            // シールド効果を一定時間後に解除
            Invoke(nameof(DeactivateShield), shieldDuration);
            
            Debug.Log($"{name} activated Shield!");
        }

        private void DeactivateShield()
        {
            isShieldActive = false;
            Debug.Log($"{name} shield deactivated");
        }

        public override void TakeDamage(float damage)
        {
            // シールドが有効な場合はダメージを軽減
            if (isShieldActive)
            {
                damage *= 0.5f; // 50%ダメージ軽減
                Debug.Log($"{name} Shield reduced damage to {damage}");
            }

            base.TakeDamage(damage);
        }

        protected override void OnStateChanged(InvaderState newState)
        {
            base.OnStateChanged(newState);
            
            // 戦士特有の状態変更処理
            if (newState == InvaderState.Fighting)
            {
                // 戦闘開始時の処理
            }
        }
    }
}