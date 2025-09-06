using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Managers
{
    public class MonsterPlacementManager : MonoBehaviour
    {
        public static MonsterPlacementManager Instance { get; private set; }
        
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
        public bool PlaceMonster(object monster, Vector3 position)
        {
            return true;
        }
        
        public bool CanPlaceAt(Vector3 position)
        {
            return true;
        }
        
        public void StartPlacement(object monster)
        {
            // 配置開始処理
        }
        
        public void CancelPlacement()
        {
            // 配置キャンセル処理
        }
        
        public List<object> GetAllPlacedMonsters()
        {
            return new List<object>();
        }
        
        public int GetMonsterFloorIndex(object monster)
        {
            return 1;
        }
        
        public bool RestoreMonster(object monsterData, int floor)
        {
            return true;
        }
        
        public bool RestoreMonster(object monsterData)
        {
            return RestoreMonster(monsterData, 1);
        }
        
        public List<object> GetUnlockedMonsters()
        {
            return new List<object>();
        }
        
        public bool CanPlaceMonster(object monster, Vector3 position)
        {
            return true;
        }
        
        public bool CanPlaceMonster(object monster, Vector3 position, int floor)
        {
            return true;
        }
        
        public bool CanPlaceMonster(object monster, Vector3 position, object monsterType)
        {
            return true;
        }
        
        public int GetMonsterCountOnFloor(int floor)
        {
            return 0;
        }
        
        public System.Action<int> OnFloorMonsterCountChanged { get; set; }
    }
}