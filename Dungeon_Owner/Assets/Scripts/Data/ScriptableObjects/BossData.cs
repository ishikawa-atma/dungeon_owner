using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Data
{
    [CreateAssetMenu(fileName = "BossData", menuName = "DungeonOwner/BossData")]
    public class BossData : ScriptableObject
    {
        [Header("基本情報")]
        public BossType type;
        public string displayName;
        [TextArea(3, 5)]
        public string description;

        [Header("基本ステータス")]
        public float baseHealth = 500f;
        public float baseMana = 200f;
        public float baseAttackPower = 100f;
        public float moveSpeed = 1.5f;

        [Header("ボス特性")]
        public List<MonsterAbilityType> abilities = new List<MonsterAbilityType>();
        [TextArea(2, 3)]
        public string abilityDescription;

        [Header("リポップ設定")]
        public float respawnTime = 300f; // 5分（秒単位）
        public bool maintainLevelOnRespawn = true; // レベル引き継ぎ

        [Header("報酬")]
        public int goldReward = 200;
        public float goldRewardMultiplier = 2.0f; // 通常モンスターの2倍

        [Header("配置制限")]
        public int requiredFloorMultiple = 5; // 5階層ごとに配置可能
        public int minFloorLevel = 5; // 最低配置階層

        [Header("プレハブ")]
        public GameObject prefab;

        [Header("UI")]
        public Sprite icon;
        public Color bossColor = Color.red;

        // ステータス計算メソッド
        public float GetHealthAtLevel(int level)
        {
            return baseHealth * (1f + (level - 1) * 0.2f); // 通常モンスターより成長率高
        }

        public float GetManaAtLevel(int level)
        {
            return baseMana * (1f + (level - 1) * 0.15f);
        }

        public float GetAttackPowerAtLevel(int level)
        {
            return baseAttackPower * (1f + (level - 1) * 0.25f);
        }

        public int GetGoldReward(int level)
        {
            float reward = goldReward * goldRewardMultiplier;
            float levelMultiplier = 1f + (level - 1) * 0.3f;
            return Mathf.RoundToInt(reward * levelMultiplier);
        }

        public bool CanPlaceOnFloor(int floorIndex)
        {
            return floorIndex >= minFloorLevel && 
                   floorIndex % requiredFloorMultiple == 0;
        }
    }
}