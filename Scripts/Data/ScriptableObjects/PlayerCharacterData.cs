using UnityEngine;
using System.Collections.Generic;

namespace DungeonOwner.Data
{
    [CreateAssetMenu(fileName = "PlayerCharacterData", menuName = "DungeonOwner/PlayerCharacterData")]
    public class PlayerCharacterData : ScriptableObject
    {
        [Header("基本情報")]
        public PlayerCharacterType type;
        public string displayName;
        [TextArea(3, 5)]
        public string description;

        [Header("基本ステータス")]
        public float baseHealth = 150f;
        public float baseMana = 100f;
        public float baseAttackPower = 25f;
        public float moveSpeed = 2.5f;

        [Header("アビリティ")]
        public List<PlayerAbilityType> abilities = new List<PlayerAbilityType>();

        [Header("プレハブ")]
        public GameObject prefab;

        [Header("UI")]
        public Sprite icon;
        public Sprite portrait;
        public Color characterColor = Color.white;

        // レベルに応じたステータス計算
        public float GetHealthAtLevel(int level)
        {
            return baseHealth * (1f + (level - 1) * 0.12f);
        }

        public float GetManaAtLevel(int level)
        {
            return baseMana * (1f + (level - 1) * 0.12f);
        }

        public float GetAttackPowerAtLevel(int level)
        {
            return baseAttackPower * (1f + (level - 1) * 0.18f);
        }

        // 蘇生時間の計算
        public float GetReviveTime(int level)
        {
            float baseReviveTime = 30f; // 基本蘇生時間30秒
            return baseReviveTime * (1f - (level - 1) * 0.02f); // レベルが上がると蘇生時間短縮
        }
    }

    public enum PlayerAbilityType
    {
        None,
        PowerStrike,     // 戦士：強力な一撃
        MagicMissile,    // 魔法使い：魔法の矢
        QuickStep,       // 盗賊：素早い移動
        Blessing,        // 僧侶：祝福（能力値上昇）
        Taunt,           // 戦士：挑発（敵の注意を引く）
        Barrier,         // 魔法使い：魔法障壁
        Backstab,        // 盗賊：背後からの攻撃
        Sanctuary        // 僧侶：聖域（範囲回復）
    }
}