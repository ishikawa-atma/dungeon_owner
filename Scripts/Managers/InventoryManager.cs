using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Scripts.Data.ScriptableObjects;
using Scripts.Data.Enums;

namespace Scripts.Managers
{
    [System.Serializable]
    public class TrapItemStack
    {
        public TrapItemData itemData;
        public int count;
        
        public TrapItemStack(TrapItemData data, int initialCount = 1)
        {
            itemData = data;
            count = initialCount;
        }
        
        public bool CanAddMore()
        {
            return count < itemData.maxStackSize;
        }
        
        public int AddItems(int amount)
        {
            int canAdd = Mathf.Min(amount, itemData.maxStackSize - count);
            count += canAdd;
            return amount - canAdd; // 余った分を返す
        }
        
        public bool UseItem()
        {
            if (count > 0)
            {
                count--;
                return true;
            }
            return false;
        }
    }
    
    public class InventoryManager : MonoBehaviour
    {
        [Header("インベントリ設定")]
        [SerializeField] private int maxSlots = 20;
        
        private List<TrapItemStack> inventory = new List<TrapItemStack>();
        private Dictionary<TrapItemType, float> lastUseTimes = new Dictionary<TrapItemType, float>();
        
        public static InventoryManager Instance { get; private set; }
        
        // イベント
        public event Action<TrapItemData, int> OnItemAdded;
        public event Action<TrapItemData, int> OnItemUsed;
        public event Action OnInventoryChanged;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// アイテムをインベントリに追加
        /// </summary>
        public bool AddItem(TrapItemData itemData, int amount = 1)
        {
            if (itemData == null || amount <= 0) return false;
            
            // 既存のスタックを探す
            var existingStack = inventory.FirstOrDefault(stack => 
                stack.itemData.type == itemData.type && stack.CanAddMore());
            
            if (existingStack != null)
            {
                int remaining = existingStack.AddItems(amount);
                OnItemAdded?.Invoke(itemData, amount - remaining);
                
                // まだ余りがある場合は新しいスタックを作成
                if (remaining > 0 && inventory.Count < maxSlots)
                {
                    inventory.Add(new TrapItemStack(itemData, remaining));
                }
                
                OnInventoryChanged?.Invoke();
                return remaining == 0;
            }
            
            // 新しいスタックを作成
            if (inventory.Count < maxSlots)
            {
                inventory.Add(new TrapItemStack(itemData, amount));
                OnItemAdded?.Invoke(itemData, amount);
                OnInventoryChanged?.Invoke();
                return true;
            }
            
            return false; // インベントリが満杯
        }
        
        /// <summary>
        /// アイテムを使用
        /// </summary>
        public bool UseItem(TrapItemType itemType, Vector2 targetPosition)
        {
            // クールダウンチェック
            if (IsOnCooldown(itemType))
            {
                Debug.Log($"罠アイテム {itemType} はクールダウン中です");
                return false;
            }
            
            var stack = inventory.FirstOrDefault(s => s.itemData.type == itemType && s.count > 0);
            if (stack == null) return false;
            
            // アイテムを使用
            if (stack.UseItem())
            {
                // クールダウンを設定
                lastUseTimes[itemType] = Time.time;
                
                // 罠効果を発動
                ActivateTrap(stack.itemData, targetPosition);
                
                // スタックが空になったら削除
                if (stack.count <= 0)
                {
                    inventory.Remove(stack);
                }
                
                OnItemUsed?.Invoke(stack.itemData, 1);
                OnInventoryChanged?.Invoke();
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// クールダウン中かチェック
        /// </summary>
        public bool IsOnCooldown(TrapItemType itemType)
        {
            if (!lastUseTimes.ContainsKey(itemType)) return false;
            
            var stack = inventory.FirstOrDefault(s => s.itemData.type == itemType);
            if (stack == null) return false;
            
            return Time.time - lastUseTimes[itemType] < stack.itemData.cooldownTime;
        }
        
        /// <summary>
        /// 残りクールダウン時間を取得
        /// </summary>
        public float GetRemainingCooldown(TrapItemType itemType)
        {
            if (!IsOnCooldown(itemType)) return 0f;
            
            var stack = inventory.FirstOrDefault(s => s.itemData.type == itemType);
            if (stack == null) return 0f;
            
            return stack.itemData.cooldownTime - (Time.time - lastUseTimes[itemType]);
        }
        
        /// <summary>
        /// アイテムの所持数を取得
        /// </summary>
        public int GetItemCount(TrapItemType itemType)
        {
            return inventory.Where(s => s.itemData.type == itemType).Sum(s => s.count);
        }
        
        /// <summary>
        /// インベントリの内容を取得
        /// </summary>
        public List<TrapItemStack> GetInventory()
        {
            return new List<TrapItemStack>(inventory);
        }
        
        /// <summary>
        /// 罠効果を発動
        /// </summary>
        private void ActivateTrap(TrapItemData trapData, Vector2 position)
        {
            // 範囲内の侵入者を検索
            var invaders = FindInvadersInRange(position, trapData.range);
            
            foreach (var invader in invaders)
            {
                // ダメージを与える
                if (invader.TryGetComponent<IInvader>(out var invaderComponent))
                {
                    invaderComponent.TakeDamage(trapData.damage);
                }
            }
            
            // 視覚効果を生成
            if (trapData.effectPrefab != null)
            {
                var effect = Instantiate(trapData.effectPrefab, position, Quaternion.identity);
                
                // エフェクトの色を設定
                if (effect.TryGetComponent<ParticleSystem>(out var particles))
                {
                    var main = particles.main;
                    main.startColor = trapData.effectColor;
                }
                
                // 一定時間後にエフェクトを削除
                Destroy(effect, trapData.effectDuration);
            }
            
            // 音効果を再生
            if (trapData.useSound != null)
            {
                AudioSource.PlayClipAtPoint(trapData.useSound, position);
            }
            
            Debug.Log($"罠アイテム {trapData.itemName} を {position} で使用しました");
        }
        
        /// <summary>
        /// 範囲内の侵入者を検索
        /// </summary>
        private List<GameObject> FindInvadersInRange(Vector2 center, float range)
        {
            var invaders = new List<GameObject>();
            var colliders = Physics2D.OverlapCircleAll(center, range);
            
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Invader"))
                {
                    invaders.Add(collider.gameObject);
                }
            }
            
            return invaders;
        }
        
        /// <summary>
        /// セーブデータ用のインベントリ情報を取得
        /// </summary>
        public List<TrapItemSaveData> GetSaveData()
        {
            var saveData = new List<TrapItemSaveData>();
            
            foreach (var stack in inventory)
            {
                saveData.Add(new TrapItemSaveData
                {
                    itemType = stack.itemData.type,
                    count = stack.count
                });
            }
            
            return saveData;
        }
        
        /// <summary>
        /// セーブデータからインベントリを復元
        /// </summary>
        public void LoadFromSaveData(List<TrapItemSaveData> saveData, TrapItemData[] allTrapItems)
        {
            inventory.Clear();
            
            foreach (var data in saveData)
            {
                var itemData = System.Array.Find(allTrapItems, item => item.type == data.itemType);
                if (itemData != null)
                {
                    inventory.Add(new TrapItemStack(itemData, data.count));
                }
            }
            
            OnInventoryChanged?.Invoke();
        }
    }
    
    [System.Serializable]
    public class TrapItemSaveData
    {
        public TrapItemType itemType;
        public int count;
    }
}