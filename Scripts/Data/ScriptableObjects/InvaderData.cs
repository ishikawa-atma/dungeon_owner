using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Data
{
    [CreateAssetMenu(fileName = "InvaderData", menuName = "DungeonOwner/InvaderData")]
    public class InvaderData : ScriptableObject
    {
        [Header("基本情報")]
        public InvaderType type;
        public InvaderRank rank = InvaderRank.Novice;
        public string displayName;
        [TextArea(3, 5)]
        public string description;

        [Header("基本ステータス")]
        public float baseHealth = 80f;
        public float baseAttackPower = 15f;
        public float moveSpeed = 3f;

        [Header("報酬")]
        public int goldReward = 50;
        public float trapItemDropRate = 0.1f; // 10%の確率でアイテムドロップ

        [Header("出現条件")]
        public int minAppearanceFloor = 1; // 出現開始階層

        [Header("アビリティ")]
        public List<InvaderAbilityType> abilities = new List<InvaderAbilityType>();

        [Header("プレハブ")]
        public GameObject prefab;

        [Header("UI")]
        public Sprite icon;
        public Color rankColor = Color.white;

        // レベルに応じたステータス計算
        public float GetHealthAtLevel(int level)
        {
            return baseHealth * (1f + (level - 1) * 0.2f);
        }

        public float GetAttackPowerAtLevel(int level)
        {
            return baseAttackPower * (1f + (level - 1) * 0.2f);
        }

        public int GetGoldRewardAtLevel(int level)
        {
            return Mathf.RoundToInt(goldReward * (1f + (level - 1) * 0.1f));
        }

        // ランクに応じたステータス補正
        public float GetRankMultiplier()
        {
            switch (rank)
            {
                case InvaderRank.Novice: return 1.0f;
                case InvaderRank.Veteran: return 1.3f;
                case InvaderRank.Elite: return 1.6f;
                case InvaderRank.Champion: return 2.0f;
                default: return 1.0f;
            }
        }
    }

    public enum InvaderAbilityType
    {
        None,
        Shield,          // 戦士：防御力上昇
        Fireball,        // 魔法使い：火球攻撃
        Stealth,         // 盗賊：透明化
        Heal,            // 僧侶：回復魔法
        Charge,          // 騎士：突撃攻撃
        Teleport,        // 大魔法使い：瞬間移動
        CriticalStrike,  // 暗殺者：クリティカル攻撃
        GroupHeal,       // 高位僧侶：範囲回復
        HolyStrike,      // パラディン：聖なる攻撃
        SummonUndead     // ネクロマンサー：アンデッド召喚
    }
}