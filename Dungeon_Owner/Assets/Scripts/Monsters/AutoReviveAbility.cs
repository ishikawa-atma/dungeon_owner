using UnityEngine;

namespace DungeonOwner.Monsters
{
    [System.Serializable]
    public class AutoReviveAbility
    {
        [SerializeField] private float reviveHealthPercent = 0.5f;
        [SerializeField] private float reviveCooldown = 30f;
        [SerializeField] private bool hasRevived = false;
        
        public float ReviveHealthPercent => reviveHealthPercent;
        public float ReviveCooldown => reviveCooldown;
        public bool HasRevived => hasRevived;
        
        public void Initialize(float healthPercent, float cooldown)
        {
            reviveHealthPercent = healthPercent;
            reviveCooldown = cooldown;
        }
        
        public void SetRevived()
        {
            hasRevived = true;
        }
        
        public void ResetRevive()
        {
            hasRevived = false;
        }
        
        public void SetReviveTime(float time)
        {
            reviveCooldown = time;
        }
        
        public void SetMaxRevives(int max)
        {
            // 最大復活回数設定（基本実装）
        }
        
        public bool TryStartRevive()
        {
            if (!hasRevived)
            {
                hasRevived = true;
                return true;
            }
            return false;
        }
        
        public int CurrentRevives => hasRevived ? 1 : 0;
        
        public void CancelRevive()
        {
            hasRevived = false;
        }
        
        public bool IsReviving => hasRevived;
        
        public int RemainingRevives => hasRevived ? 0 : 1;
    }
}