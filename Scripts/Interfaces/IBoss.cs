using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Interfaces
{
    public interface IBoss : IMonster
    {
        BossType BossType { get; }
        float RespawnTime { get; }
        bool IsRespawning { get; }
        float RespawnProgress { get; }
        int DefeatedLevel { get; }
        
        void StartRespawn();
        void CompleteRespawn();
        void SetRespawnTime(float time);
        bool CanRespawn();
    }
}