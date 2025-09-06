using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Managers
{
    public class BossManager : MonoBehaviour
    {
        public static BossManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        // 基本的なダミー実装
        public void SpawnBoss(object bossData)
        {
            // ボス生成処理
        }
        
        public void DefeatBoss()
        {
            // ボス撃破処理
        }
        
        public bool IsBossActive()
        {
            return false;
        }
        
        public object GetCurrentBoss()
        {
            return null;
        }
        
        public System.Action<int> OnBossFloorAvailable;
        public System.Action<object> OnBossPlaced { get; set; }
        public System.Action<object> OnBossDefeated { get; set; }
        public System.Action<object> OnBossRespawned { get; set; }
        
        public bool IsBossFloor(int floor)
        {
            return false;
        }
        
        public bool HasBossOnFloor(int floor)
        {
            return false;
        }
        
        public List<object> GetAvailableBossesForFloor(int floor)
        {
            return new List<object>();
        }
        
        public List<object> GetActiveBosses()
        {
            return new List<object>();
        }
        
        public object GetBossOnFloor(int floor)
        {
            return null;
        }
        
        public bool CanPlaceBoss(object boss, int floor)
        {
            return true;
        }
        
        public bool CanPlaceBoss(object boss)
        {
            return CanPlaceBoss(boss, 1);
        }
        
        public bool PlaceBoss(object boss, int floor)
        {
            return true;
        }
        
        public bool PlaceBoss(object boss, int floor, Vector3 position, object data)
        {
            return PlaceBoss(boss, floor);
        }
        
        public bool PlaceBoss(object boss, object bossType, Vector3 position, object data)
        {
            return PlaceBoss(boss, 1, position, data);
        }
        
        public object GetBossData(object boss)
        {
            return null;
        }
    }
}