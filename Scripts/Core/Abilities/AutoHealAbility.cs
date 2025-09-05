using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core.Abilities
{
    /// <summary>
    /// 自動体力回復アビリティ（スライム用）
    /// 要件4.1, 4.2に対応
    /// </summary>
    public class AutoHealAbility : MonsterAbility
    {
        [Header("Auto Heal Settings")]
        [SerializeField] private float healMultiplier = 2f; // 回復速度倍率
        [SerializeField] private float healInterval = 1f; // 回復間隔
        [SerializeField] private float burstHealAmount = 0.1f; // バースト回復量（最大HPの割合）
        
        private float lastHealTime;
        private float nextBurstHealTime;

        public AutoHealAbility()
        {
            abilityType = MonsterAbilityType.AutoHeal;
            abilityName = "自動体力回復";
            description = "時間経過で体力を自動回復し、アクティブ使用で瞬間回復";
            cooldownTime = 3f;
            manaCost = 10f;
        }

        protected override void OnInitialize()
        {
            lastHealTime = Time.time;
            nextBurstHealTime = Time.time + healInterval;
        }

        protected override bool CanUseCustomCondition()
        {
            // 体力が満タンでない場合のみ使用可能
            return IsOwnerAlive() && GetOwnerHealthRatio() < 1f;
        }

        protected override bool ExecuteAbility()
        {
            if (!IsOwnerAlive()) return false;
            
            // バースト回復を実行
            float healAmount = owner.MaxHealth * burstHealAmount;
            owner.Heal(healAmount);
            
            Debug.Log($"Slime burst heal: {healAmount:F1} HP restored!");
            return true;
        }

        protected override void UpdateAbility()
        {
            if (!IsOwnerAlive()) return;
            
            // パッシブ自動回復処理
            ProcessPassiveHealing();
        }

        private void ProcessPassiveHealing()
        {
            // 通常の自動回復（BaseMonsterの回復を強化）
            if (Time.time >= nextBurstHealTime && GetOwnerHealthRatio() < 1f)
            {
                float passiveHealAmount = owner.MaxHealth * 0.02f; // 最大HPの2%
                owner.Heal(passiveHealAmount);
                
                nextBurstHealTime = Time.time + healInterval;
                
                // パッシブ回復エフェクト
                ShowPassiveHealEffect();
            }
        }

        protected override void ShowVisualEffect()
        {
            // アクティブ回復エフェクト
            ShowActiveHealEffect();
        }

        private void ShowPassiveHealEffect()
        {
            // TODO: パッシブ回復の視覚エフェクト（小さな緑の光など）
            if (owner is MonoBehaviour ownerMono)
            {
                Debug.Log($"Slime passive heal at {ownerMono.transform.position}");
            }
        }

        private void ShowActiveHealEffect()
        {
            // TODO: アクティブ回復の視覚エフェクト（大きな緑の光など）
            if (owner is MonoBehaviour ownerMono)
            {
                Debug.Log($"Slime active heal burst at {ownerMono.transform.position}");
            }
        }

        protected override void OnReset()
        {
            lastHealTime = Time.time;
            nextBurstHealTime = Time.time + healInterval;
        }

        // 回復速度倍率を取得（BaseMonsterで使用）
        public float GetHealMultiplier()
        {
            return healMultiplier;
        }

        // 回復間隔を取得
        public float GetHealInterval()
        {
            return healInterval;
        }
    }
}