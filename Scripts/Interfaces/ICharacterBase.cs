using UnityEngine;

namespace DungeonOwner.Interfaces
{
    /// <summary>
    /// モンスターと侵入者の共通基底インターフェース
    /// </summary>
    public interface ICharacterBase
    {
        int Level { get; }
        float Health { get; }
        float MaxHealth { get; }
        Vector2 Position { get; set; }
        IParty Party { get; set; }
        
        void TakeDamage(float damage);
        void JoinParty(IParty party);
        void LeaveParty();
    }
}