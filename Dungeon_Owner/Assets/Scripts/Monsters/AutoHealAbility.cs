using UnityEngine;

namespace DungeonOwner.Monsters
{
    [System.Serializable]
    public class AutoHealAbility
    {
        [SerializeField] private float healAmount = 5f;
        [SerializeField] private float healInterval = 2f;
        
        public float HealAmount => healAmount;
        public float HealInterval => healInterval;
        
        public void Initialize(float amount, float interval)
        {
            healAmount = amount;
            healInterval = interval;
        }
        
        public bool CanUse()
        {
            return true; // 基本実装
        }
        
        public float CooldownTime => healInterval;
    }
}