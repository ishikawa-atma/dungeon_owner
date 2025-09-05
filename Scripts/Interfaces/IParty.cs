using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Interfaces
{
    public interface ICharacter
    {
        Vector2 Position { get; set; }
        float Health { get; }
        float MaxHealth { get; }
        void TakeDamage(float damage);
        void Heal(float amount);
    }

    public interface IParty
    {
        List<ICharacter> Members { get; }
        Vector2 Position { get; set; }
        bool IsActive { get; }
        
        void AddMember(ICharacter character);
        void RemoveMember(ICharacter character);
        void DistributeDamage(float totalDamage);
        void ApplyPartyHealing(float healAmount);
        void MoveParty(Vector2 targetPosition);
        void DisbandParty();
    }
}