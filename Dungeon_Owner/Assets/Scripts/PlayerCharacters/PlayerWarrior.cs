using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.PlayerCharacters
{
    public class PlayerWarrior : BasePlayerCharacter
    {
        [Header("Warrior Specific")]
        [SerializeField] private float powerStrikeDamageMultiplier = 2.5f;
        [SerializeField] private float tauntRange = 4f;
        [SerializeField] private float defenseStance = 0.7f; // ダメージ軽減率
        
        private bool isDefending = false;
        private bool isTaunting = false;

        protected override void UpdateCharacterBehavior()
        {
            if (currentState == MonsterState.Idle && IsAlive())
            {
                // 戦士は前線で戦う
                ProcessWarriorBehavior();
            }
        }

        protected override void ExecuteAbility()
        {
            // 戦士のメインアビリティ：パワーストライク
            if (currentMana >= 20f)
            {
                currentMana -= 20f;
                ExecutePowerStrike();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentMana >= 20f && !isDead;
        }

        protected override float GetAbilityCooldown()
        {
            return 8f; // 8秒クールダウン
        }

        private void ProcessWarriorBehavior()
        {
            // 戦士の基本行動：前線維持と防御
            if (currentHealth < MaxHealth * 0.3f && !isDefending)
            {
                StartDefenseStance();
            }
            else if (currentHealth > MaxHealth * 0.7f && isDefending)
            {
                EndDefenseStance();
            }
        }

        private void ExecutePowerStrike()
        {
            // パワーストライク：大ダメージ攻撃
            float damage = GetAttackPower() * powerStrikeDamageMultiplier;
            ShowPowerStrikeEffect();
            
            // 実際の攻撃処理は戦闘システムで実装
            Debug.Log($"Warrior {gameObject.name} used Power Strike! Damage: {damage}");
        }

        public void UseTaunt()
        {
            if (currentMana >= 15f && !isTaunting)
            {
                currentMana -= 15f;
                StartTaunt();
            }
        }

        private void StartTaunt()
        {
            isTaunting = true;
            ShowTauntEffect();
            
            // 5秒間挑発効果
            Invoke(nameof(EndTaunt), 5f);
        }

        private void EndTaunt()
        {
            isTaunting = false;
            ShowTauntEndEffect();
        }

        private void StartDefenseStance()
        {
            isDefending = true;
            ShowDefenseStanceEffect();
        }

        private void EndDefenseStance()
        {
            isDefending = false;
            ShowDefenseStanceEndEffect();
        }

        public override void TakeDamage(float damage)
        {
            // 防御姿勢中はダメージ軽減
            if (isDefending)
            {
                damage *= defenseStance;
                ShowDefenseEffect();
            }
            
            base.TakeDamage(damage);
        }

        // エフェクト表示メソッド
        private void ShowPowerStrikeEffect()
        {
            // TODO: パワーストライクエフェクト
            Debug.Log($"Warrior {gameObject.name} charges up for a powerful strike!");
        }

        private void ShowTauntEffect()
        {
            // TODO: 挑発エフェクト
            Debug.Log($"Warrior {gameObject.name} taunts nearby enemies!");
        }

        private void ShowTauntEndEffect()
        {
            // TODO: 挑発終了エフェクト
            Debug.Log($"Warrior {gameObject.name} taunt effect ended!");
        }

        private void ShowDefenseStanceEffect()
        {
            // TODO: 防御姿勢エフェクト
            Debug.Log($"Warrior {gameObject.name} enters defensive stance!");
        }

        private void ShowDefenseStanceEndEffect()
        {
            // TODO: 防御姿勢終了エフェクト
            Debug.Log($"Warrior {gameObject.name} exits defensive stance!");
        }

        private void ShowDefenseEffect()
        {
            // TODO: 防御成功エフェクト
            Debug.Log($"Warrior {gameObject.name} blocks the attack!");
        }

        // ゲッター
        public bool IsDefending()
        {
            return isDefending;
        }

        public bool IsTaunting()
        {
            return isTaunting;
        }

        public float GetTauntRange()
        {
            return tauntRange;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // 挑発範囲を表示
            if (isTaunting)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, tauntRange);
            }
        }
    }
}