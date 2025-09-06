using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Data
{
    [CreateAssetMenu(fileName = "MonsterData", menuName = "DungeonOwner/MonsterData")]
    public class MonsterData : ScriptableObject
    {
        [Header("基本情報")]
        public MonsterType type;
        public MonsterRarity rarity = MonsterRarity.Common;
        public string displayName;
        [TextArea(3, 5)]
        public string description;

        [Header("コスト")]
        public int goldCost = 100;

        [Header("基本ステータス")]
        public float baseHealth = 100f;
        public float baseMana = 50f;
        public float baseAttackPower = 20f;
        public float moveSpeed = 2f;

        [Header("アビリティ")]
        public MonsterAbilityType abilityType;
        [TextArea(2, 3)]
        public string abilityDescription;

        [Header("解放条件")]
        public int unlockFloor = 1; // 解放される階層
        
        [Header("進化")]
        public List<MonsterType> evolutionTargets = new List<MonsterType>();

        [Header("プレハブ")]
        public GameObject prefab;

        [Header("UI")]
        public Sprite icon;
        public Color rarityColor = Color.white;

        // ステータス計算メソッド
        public float GetHealthAtLevel(int level)
        {
            return baseHealth * (1f + (level - 1) * 0.1f);
        }

        public float GetManaAtLevel(int level)
        {
            return baseMana * (1f + (level - 1) * 0.1f);
        }

        public float GetAttackPowerAtLevel(int level)
        {
            return baseAttackPower * (1f + (level - 1) * 0.15f);
        }

        public int GetSellPrice()
        {
            return Mathf.RoundToInt(goldCost * 0.7f); // 購入価格の70%で売却
        }
    }


}