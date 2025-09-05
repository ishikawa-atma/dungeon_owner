using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Monsters
{
    public class Slime : BaseMonster
    {
        [Header("Slime Specific")]
        [SerializeField] private float autoHealRate = 2f; // 自動回復速度倍率
        [SerializeField] private float healInterval = 1f; // 回復間隔
        
        private float lastHealTime;

        protected override void UpdateMonsterBehavior()
        {
            // スライムは基本的にアイドル状態で自動回復に専念
            if (currentState == MonsterState.Idle && IsAlive())
            {
                // 特別な行動は無し、自動回復のみ
            }
        }

        protected override void ExecuteAbility()
        {
            // スライムのアビリティ：強化された自動回復
            if (currentHealth < MaxHealth)
            {
                float healAmount = MaxHealth * 0.1f; // 最大HPの10%回復
                Heal(healAmount);
                
                // エフェクト表示（後で実装）
                ShowHealEffect();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentHealth < MaxHealth && currentMana >= 10f;
        }

        protected override float GetAbilityCooldown()
        {
            return 3f; // 3秒クールダウン
        }

        protected override void ProcessNaturalRecovery()
        {
            // スライムは通常より高速で回復
            float baseRecoveryRate = currentState == MonsterState.InShelter ? 5f : 1f;
            float slimeRecoveryRate = baseRecoveryRate * autoHealRate;
            
            if (currentHealth < MaxHealth)
            {
                Heal(slimeRecoveryRate * Time.deltaTime);
            }
            
            if (currentMana < MaxMana)
            {
                currentMana = Mathf.Min(MaxMana, currentMana + slimeRecoveryRate * Time.deltaTime);
            }
        }

        private void ShowHealEffect()
        {
            // TODO: パーティクルエフェクトやアニメーション
            Debug.Log($"Slime {gameObject.name} used auto-heal ability!");
        }

        protected override void OnHealed(float amount)
        {
            base.OnHealed(amount);
            // スライムの回復時の特別な処理があれば追加
        }
    }
}