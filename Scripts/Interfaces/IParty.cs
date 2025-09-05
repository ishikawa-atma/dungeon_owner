using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Interfaces
{
    public interface IParty
    {
        List<ICharacterBase> Members { get; }
        Vector2 Position { get; set; }
        bool IsActive { get; }
        
        void AddMember(ICharacterBase character);
        void RemoveMember(ICharacterBase character);
        void DistributeDamage(float totalDamage);
        void ApplyPartyHealing(float healAmount);
        void MoveParty(Vector2 targetPosition);
        void DisbandParty();
    }
}