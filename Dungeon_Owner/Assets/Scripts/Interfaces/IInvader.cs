using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Interfaces
{
    public interface IInvader
    {
        InvaderType Type { get; }
        int Level { get; }
        float Health { get; }
        float MaxHealth { get; }
        Vector2 Position { get; set; }
        InvaderState State { get; }
        IParty Party { get; set; }
        
        void TakeDamage(float damage);
        void Move(Vector2 targetPosition);
        void JoinParty(IParty party);
        void LeaveParty();
        void SetState(InvaderState newState);
    }
}