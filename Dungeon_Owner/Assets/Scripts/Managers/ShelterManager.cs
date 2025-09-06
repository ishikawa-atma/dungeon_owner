using UnityEngine;

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
    }
}