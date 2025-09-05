using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Core;

namespace DungeonOwner.Interfaces
{
    public interface IMonster
    {
        MonsterType Type { get; }
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
        bool UseAbility(MonsterAbilityType abilityType);
        void JoinParty(IParty party);
        void LeaveParty();
        void SetState(MonsterState newState);
        void ConsumeMana(float amount);
        
        // アビリティ関連
        IMonsterAbility GetAbility(MonsterAbilityType abilityType);
        bool HasAbility(MonsterAbilityType abilityType);
    }
}