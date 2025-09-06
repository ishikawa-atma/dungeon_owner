using UnityEngine;

namespace DungeonOwner.Data
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "DungeonOwner/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("ゲーム基本設定")]
        public int initialFloors = 3;
        public int initialGold = 1000;
        public int maxMonstersPerFloor = 15;

        [Header("時間設定")]
        public float dayDurationInSeconds = 300f; // 5分 = 1日
        public float[] gameSpeedMultipliers = { 1.0f, 1.5f, 2.0f };

        [Header("経済設定")]
        public int dailyGoldReward = 100;
        public float monsterSellRatio = 0.7f; // 購入価格の70%で売却

        [Header("階層拡張設定")]
        public int baseFloorExpansionCost = 500;
        public float floorCostMultiplier = 1.5f; // 階層が増えるごとのコスト倍率

        [Header("戦闘設定")]
        public float baseCombatInterval = 1.0f; // 基本戦闘間隔（秒）
        public float levelDifferenceMultiplier = 0.1f; // レベル差による戦闘力補正

        [Header("回復設定")]
        public float floorRecoveryRate = 1.0f; // 階層配置中の回復速度（HP/秒）
        public float shelterRecoveryRate = 5.0f; // 退避スポットでの回復速度（HP/秒）

        [Header("侵入者出現設定")]
        public float baseInvaderSpawnInterval = 30f; // 基本侵入者出現間隔（秒）
        public float invaderSpawnVariation = 10f; // 出現間隔のランダム幅
        public int maxConsecutiveInvaders = 3; // 連続出現最大数

        [Header("パーティ設定")]
        public int maxPartySize = 4;
        public float partyDamageDistribution = 0.8f; // パーティ内ダメージ分散率

        [Header("ボス設定")]
        public int bossFloorInterval = 5; // ボスが配置可能な階層間隔
        public float bossRespawnTime = 120f; // ボス復活時間（秒）

        [Header("UI設定")]
        public bool enableTutorial = true;
        public float uiAnimationSpeed = 1.0f;

        // 階層拡張コスト計算
        public int GetFloorExpansionCost(int targetFloor)
        {
            return Mathf.RoundToInt(baseFloorExpansionCost * Mathf.Pow(floorCostMultiplier, targetFloor - initialFloors));
        }

        // 侵入者レベル計算
        public int GetInvaderLevelForFloor(int floorIndex)
        {
            return Mathf.Max(1, floorIndex / 2 + 1); // 2階層ごとにレベル+1
        }

        // ボス配置可能判定
        public bool CanPlaceBossOnFloor(int floorIndex)
        {
            return floorIndex > 0 && (floorIndex + 1) % bossFloorInterval == 0;
        }
    }
}