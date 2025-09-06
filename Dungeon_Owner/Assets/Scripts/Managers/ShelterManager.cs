using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Managers
{
    public class ShelterManager : MonoBehaviour
    {
        [SerializeField] private int shelterLevel = 1;
        [SerializeField] private int maxCapacity = 10;
        [SerializeField] private int currentOccupants = 0;
        
        private void Start()
        {
            // 基本初期化
        }
        
        public bool CanAcceptNewOccupant()
        {
            return currentOccupants < maxCapacity;
        }
        
        public void AddOccupant()
        {
            if (CanAcceptNewOccupant())
            {
                currentOccupants++;
            }
        }
        
        public void RemoveOccupant()
        {
            if (currentOccupants > 0)
            {
                currentOccupants--;
            }
        }
        
        public int GetCurrentOccupants()
        {
            return currentOccupants;
        }
        
        public int GetMaxCapacity()
        {
            return maxCapacity;
        }
        
        // Singletonパターンのサポート
        public static ShelterManager Instance { get; private set; }
        
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
        
        // イベント処理用のダミーメソッド
        public System.Action<object> OnMonsterSheltered { get; set; }
        public System.Action<object> OnMonsterDeployed { get; set; }
        public System.Action<object> OnMonsterSold { get; set; }
        
        public List<object> ShelterMonsters = new List<object>();
        
        public List<int> GetAvailableFloors()
        {
            return new List<int> { 1, 2, 3 }; // 基本実装
        }
        
        public List<Vector3> GetAvailablePositions(int floor)
        {
            return new List<Vector3>(); // 基本実装
        }
        
        public bool DeployMonster(object monster, int floor, Vector3 position)
        {
            return true; // 基本実装
        }
        
        public bool CanSellMonster(object monster)
        {
            return true; // 基本実装
        }
        
        public int CalculateMonsterSellPrice(object monster)
        {
            return 50; // 基本実装
        }
        
        public bool SellMonster(object monster)
        {
            return true; // 基本実装
        }
        
        public int CurrentCount => currentOccupants;
        public int MaxCapacity => maxCapacity;
        
        public bool RestoreMonster(object monsterData)
        {
            return true;
        }
    }
}