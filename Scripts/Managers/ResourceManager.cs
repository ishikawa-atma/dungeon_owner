using UnityEngine;
using System;
using DungeonOwner.Interfaces;
using DungeonOwner.Data;

namespace DungeonOwner.Managers
{
    /// <summary>
    /// ゲーム内リソース（金貨等）の管理クラス
    /// 侵入者撃破報酬、日次報酬、購入処理を管理
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        [Header("Gold Settings")]
        [SerializeField] private int currentGold = 1000; // 初期金貨
        [SerializeField] private int dailyGoldReward = 100; // 日次報酬
        [SerializeField] private float goldMultiplierPerLevel = 0.2f; // レベルあたりの金貨倍率

        [Header("Reward Settings")]
        [SerializeField] private int baseInvaderReward = 10; // 基本侵入者撃破報酬
        [SerializeField] private int baseMonsterSellPrice = 50; // 基本モンスター売却価格
        [SerializeField] private float sellPriceRatio = 0.7f; // 売却価格比率

        private DateTime lastDailyReward;
        private int lastRewardDay = 0; // ゲーム内日数での管理
        private bool isInitialized = false;

        // イベント
        public System.Action<int> OnGoldChanged;
        public System.Action<int> OnGoldEarned;
        public System.Action<int> OnGoldSpent;

        public int Gold => currentGold;
        public DateTime LastDailyReward => lastDailyReward;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeResourceManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 日次報酬チェック
            CheckDailyReward();
        }

        /// <summary>
        /// リソースマネージャーの初期化
        /// </summary>
        private void InitializeResourceManager()
        {
            lastDailyReward = DateTime.Now.Date.AddDays(-1); // 初回は報酬を受け取れるように
            isInitialized = true;
            
            Debug.Log($"ResourceManager initialized with {currentGold} gold");
        }

        /// <summary>
        /// 金貨を追加
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;

            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
            OnGoldEarned?.Invoke(amount);

            Debug.Log($"Earned {amount} gold. Total: {currentGold}");
        }

        /// <summary>
        /// 金貨を消費
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (amount <= 0 || !CanAfford(amount))
            {
                return false;
            }

            currentGold -= amount;
            OnGoldChanged?.Invoke(currentGold);
            OnGoldSpent?.Invoke(amount);

            Debug.Log($"Spent {amount} gold. Remaining: {currentGold}");
            return true;
        }

        /// <summary>
        /// 購入可能かチェック
        /// </summary>
        public bool CanAfford(int cost)
        {
            return currentGold >= cost;
        }

        /// <summary>
        /// 侵入者撃破報酬を計算・付与
        /// </summary>
        public void ProcessInvaderDefeatReward(IInvader invader)
        {
            if (invader == null) return;

            int reward = CalculateInvaderReward(invader);
            AddGold(reward);

            Debug.Log($"Defeated {invader.Type} (Lv.{invader.Level}) - Reward: {reward} gold");
        }

        /// <summary>
        /// 侵入者撃破報酬の計算
        /// </summary>
        private int CalculateInvaderReward(IInvader invader)
        {
            // 基本報酬
            float reward = baseInvaderReward;

            // レベルによる倍率適用
            int level = invader.Level;
            float levelMultiplier = 1f + (level - 1) * goldMultiplierPerLevel;
            reward *= levelMultiplier;

            // 侵入者タイプによる補正（将来の拡張用）
            reward *= GetInvaderTypeMultiplier(invader.Type);

            return Mathf.RoundToInt(reward);
        }

        /// <summary>
        /// 侵入者タイプによる報酬倍率
        /// </summary>
        private float GetInvaderTypeMultiplier(InvaderType type)
        {
            switch (type)
            {
                case InvaderType.Warrior:
                    return 1.0f;
                case InvaderType.Mage:
                    return 1.2f; // 魔法使いは高報酬
                case InvaderType.Rogue:
                    return 0.8f; // 盗賊は低報酬だが高速
                case InvaderType.Cleric:
                    return 1.5f; // 回復役は最高報酬
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// モンスター売却価格を計算
        /// </summary>
        public int CalculateMonsterSellPrice(IMonster monster)
        {
            if (monster == null) return 0;

            // 基本価格（購入価格から計算）
            int purchasePrice = GetMonsterPurchasePrice(monster.Type);
            float sellPrice = purchasePrice * sellPriceRatio;

            // レベルによる補正
            int level = monster.Level;
            float levelMultiplier = 1f + (level - 1) * 0.1f;
            sellPrice *= levelMultiplier;

            return Mathf.RoundToInt(sellPrice);
        }

        /// <summary>
        /// モンスター購入価格を取得
        /// </summary>
        private int GetMonsterPurchasePrice(MonsterType type)
        {
            // DataManagerから取得するのが理想だが、フォールバック値を提供
            switch (type)
            {
                case MonsterType.Slime:
                    return 50;
                case MonsterType.LesserSkeleton:
                    return 80;
                case MonsterType.LesserGhost:
                    return 100;
                case MonsterType.LesserGolem:
                    return 150;
                case MonsterType.Goblin:
                    return 70;
                case MonsterType.LesserWolf:
                    return 90;
                default:
                    return baseMonsterSellPrice;
            }
        }

        /// <summary>
        /// モンスター売却処理
        /// </summary>
        public bool SellMonster(IMonster monster)
        {
            if (monster == null) return false;

            int sellPrice = CalculateMonsterSellPrice(monster);
            AddGold(sellPrice);

            Debug.Log($"Sold {monster.Type} (Lv.{monster.Level}) for {sellPrice} gold");
            return true;
        }

        /// <summary>
        /// 日次報酬チェック・付与（ゲーム内時間ベース）
        /// </summary>
        public void CheckDailyReward()
        {
            // ゲーム内時間ベースでの日次報酬
            if (TimeManager.Instance != null)
            {
                int currentGameDay = TimeManager.Instance.CurrentDay;
                
                if (currentGameDay > lastRewardDay)
                {
                    ProcessDailyReward();
                    lastRewardDay = currentGameDay;
                }
            }
            else
            {
                // フォールバック: リアル時間ベース
                DateTime today = DateTime.Now.Date;
                
                if (lastDailyReward.Date < today)
                {
                    ProcessDailyReward();
                    lastDailyReward = today;
                }
            }
        }

        /// <summary>
        /// 日次報酬処理
        /// </summary>
        private void ProcessDailyReward()
        {
            AddGold(dailyGoldReward);
            Debug.Log($"Daily reward: {dailyGoldReward} gold");
        }

        /// <summary>
        /// 階層拡張コストを計算
        /// </summary>
        public int CalculateFloorExpansionCost(int targetFloor)
        {
            // 階層数に応じて指数的に増加
            int baseCost = 200;
            float multiplier = Mathf.Pow(1.5f, targetFloor - 3); // 3階層目以降から増加
            
            return Mathf.RoundToInt(baseCost * multiplier);
        }

        /// <summary>
        /// 階層拡張処理
        /// </summary>
        public bool ProcessFloorExpansion(int targetFloor)
        {
            int cost = CalculateFloorExpansionCost(targetFloor);
            
            if (SpendGold(cost))
            {
                Debug.Log($"Floor expanded to {targetFloor} for {cost} gold");
                return true;
            }
            
            Debug.Log($"Cannot afford floor expansion. Cost: {cost}, Available: {currentGold}");
            return false;
        }

        /// <summary>
        /// セーブデータ用の金貨設定
        /// </summary>
        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(currentGold);
        }

        /// <summary>
        /// セーブデータ用の日次報酬日時設定
        /// </summary>
        public void SetLastDailyReward(DateTime date)
        {
            lastDailyReward = date;
        }

        /// <summary>
        /// セーブデータ用のゲーム内日次報酬日数設定
        /// </summary>
        public void SetLastRewardDay(int day)
        {
            lastRewardDay = day;
        }

        /// <summary>
        /// 現在の報酬日数を取得
        /// </summary>
        public int GetLastRewardDay()
        {
            return lastRewardDay;
        }

        /// <summary>
        /// リソース設定の変更
        /// </summary>
        public void SetGoldSettings(int dailyReward, float levelMultiplier, int baseReward)
        {
            dailyGoldReward = dailyReward;
            goldMultiplierPerLevel = levelMultiplier;
            baseInvaderReward = baseReward;
        }

        /// <summary>
        /// デバッグ用メソッド
        /// </summary>
        public void DebugAddGold(int amount)
        {
            AddGold(amount);
        }

        public void DebugPrintResourceInfo()
        {
            Debug.Log($"=== Resource Manager Info ===");
            Debug.Log($"Current Gold: {currentGold}");
            Debug.Log($"Daily Reward: {dailyGoldReward}");
            Debug.Log($"Last Daily Reward: {lastDailyReward}");
            Debug.Log($"Gold Multiplier Per Level: {goldMultiplierPerLevel}");
            Debug.Log($"Base Invader Reward: {baseInvaderReward}");
        }

        /// <summary>
        /// 強制日次報酬（デバッグ用）
        /// </summary>
        public void ForceDailyReward()
        {
            ProcessDailyReward();
        }
    }
}