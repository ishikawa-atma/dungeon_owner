using System;
using System.Collections.Generic;
using UnityEngine;

namespace DungeonOwner.Data
{
    /// <summary>
    /// ゲームの保存データ構造
    /// 設計書のSaveData構造に基づく実装
    /// </summary>
    [System.Serializable]
    public class SaveData
    {
        [Header("Game Progress")]
        public int currentFloor;
        public int gold;
        public DateTime lastDailyReward;
        public PlayerCharacterType selectedPlayerCharacter;
        
        [Header("Floor and Monster Data")]
        public List<FloorData> floorLayouts;
        public List<MonsterSaveData> placedMonsters;
        public List<MonsterSaveData> shelterMonsters;
        
        [Header("Inventory")]
        public List<TrapItemSaveData> inventory;
        
        [Header("Save Info")]
        public DateTime lastSaveTime;
        
        public SaveData()
        {
            // デフォルト値の設定
            currentFloor = 1;
            gold = 1000; // 初期金貨
            lastDailyReward = DateTime.MinValue;
            selectedPlayerCharacter = PlayerCharacterType.Warrior;
            
            floorLayouts = new List<FloorData>();
            placedMonsters = new List<MonsterSaveData>();
            shelterMonsters = new List<MonsterSaveData>();
            inventory = new List<TrapItemSaveData>();
            
            lastSaveTime = DateTime.Now;
        }
    }
    
    /// <summary>
    /// モンスターの保存データ
    /// </summary>
    [System.Serializable]
    public class MonsterSaveData
    {
        public MonsterType type;
        public int level;
        public float currentHealth;
        public float currentMana;
        public Vector2 position;
        public int floorIndex;
        public bool isInShelter;
        
        // パーティ情報（将来の拡張用）
        public int partyId = -1;
        
        public MonsterSaveData()
        {
            type = MonsterType.Slime;
            level = 1;
            currentHealth = 100f;
            currentMana = 50f;
            position = Vector2.zero;
            floorIndex = 0;
            isInShelter = false;
        }
    }
    
    /// <summary>
    /// 階層の保存データ
    /// </summary>
    [System.Serializable]
    public class FloorData
    {
        public int floorIndex;
        public Vector2 upStairPosition;
        public Vector2 downStairPosition;
        public List<Vector2> wallPositions;
        
        // ボス情報
        public BossType bossType;
        public int bossLevel;
        public bool hasBoss;
        
        public FloorData()
        {
            floorIndex = 0;
            upStairPosition = Vector2.zero;
            downStairPosition = Vector2.zero;
            wallPositions = new List<Vector2>();
            bossType = BossType.None;
            bossLevel = 1;
            hasBoss = false;
        }
    }
    
    /// <summary>
    /// 罠アイテムの保存データ
    /// </summary>
    [System.Serializable]
    public class TrapItemSaveData
    {
        public TrapItemType type;
        public int quantity;
        
        public TrapItemSaveData()
        {
            type = TrapItemType.ExplosiveTrap;
            quantity = 0;
        }
        
        public TrapItemSaveData(TrapItemType itemType, int itemQuantity)
        {
            type = itemType;
            quantity = itemQuantity;
        }
    }
}