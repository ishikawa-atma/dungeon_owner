using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Managers
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }
        
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
        public bool AddItem(object item)
        {
            return true;
        }
        
        public bool RemoveItem(object item)
        {
            return true;
        }
        
        public bool HasItem(object item)
        {
            return false;
        }
        
        public bool AddItem(object item, int quantity)
        {
            return true;
        }
        
        public List<object> GetAllItems()
        {
            return new List<object>();
        }
        
        public bool RestoreItem(object itemData)
        {
            return true;
        }
    }
}