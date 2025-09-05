using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Core.Abilities;

namespace DungeonOwner.Monsters
{
    public class Slime : BaseMonster
    {
        [Header("Slime Specific")]
        [SerializeField] private float autoHealRate = 2f; // 自動回復速度倍率
        [SerializeField] private float healInterval = 1f; // 回復間隔
        
        private float lastHealTime;
        private AutoHealAbility autoHealAbility;

        protected override void InitializeAbilities()
        {
            base.InitializeAbilities();
            
            // 自動体力回復アビリティを追加
            autoHealAbility = new AutoHealAbility();
            AddAbility(autoHealAbility);
        }

        protected override void UpdateMonsterBehavior()
        {
            // スライムは基本的にアイドル状態で自動回復に専念
            if (currentState == MonsterState.Idle && IsAlive())
            {
                // アビリティシステムが自動回復を処理
            }
        }

        protected override void ExecuteAbility()
        {
            // 新しいアビリティシステムを使用
            UseAbility(MonsterAbilityType.AutoHeal);
        }

        protected override bool CanUseAbility()
        {
            return autoHealAbility?.CanUse ?? false;
        }

        protected override float GetAbilityCooldown()
        {
            return autoHealAbility?.CooldownTime ?? 3f;
        }

        protected override void ProcessNaturalRecovery()
        {
            // スライムは通常より高速で回復（アビリティシステムと併用）
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