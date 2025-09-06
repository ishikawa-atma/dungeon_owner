using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Interfaces
{
    /// <summary>
    /// モンスターとプレイヤーキャラクター共通のインターフェース
    /// </summary>
    public interface ICharacter
    {
        int Level { get; set; }
        float Health { get; }
        float MaxHealth { get; }
        float Mana { get; }
        float MaxMana { get; }
        Vector2 Position { get; set; }
        MonsterState State { get; }
        IParty Party { get; set; }
        
        void TakeDamage(float damage);
        void Heal(float amount);
        void UseAbility();
        void JoinParty(IParty party);
        void LeaveParty();
        void SetState(MonsterState newState);
    }
}