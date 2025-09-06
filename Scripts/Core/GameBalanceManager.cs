using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ゲームバランスの調整とパラメータ最適化を管理するクラス
/// 各システムのバランス値を動的に調整し、最適なゲーム体験を提供する
/// </summary>
public class GameBalanceManager : MonoBehaviour
{
    [Header("バランス調整設定")]
    [SerializeField] private bool enableAutoBalancing = true;
    [SerializeField] private bool enableDetailedLogging = true;
    [SerializeField] private float balanceCheckInterval = 10f;
    
    [Header("戦闘バランス")]
    [SerializeField] private CombatBalance combatBalance = new CombatBalance();
    
    [Header("経済バランス")]
    [SerializeField] private EconomyBalance economyBalance = new EconomyBalance();
    
    [Header("侵入者バランス")]
    [SerializeField] private InvaderBalance invaderBalance = new InvaderBalance();
    
    [Header("モンスターバランス")]
    [SerializeField] private MonsterBalance monsterBalance = new MonsterBalance();
    
    [Header("階層バランス")]
    [SerializeField] private FloorBalance floorBalance = new FloorBalance();
    
    private float lastBalanceCheck = 0f;
    private GameplayMetrics metrics = new GameplayMetrics();
    
    [System.Serializable]
    public class CombatBalance
    {
        [Header("ダメージ計算")]
        public float baseDamageMultiplier = 1.0f;
        public float levelDifferenceMultiplier = 0.1f;
        public float criticalHitChance = 0.1f;
        public float criticalDamageMultiplier = 2.0f;
        
        [Header("戦闘頻度")]
        public float combatCheckInterval = 0.1f;
        public float knockbackForce = 5f;
        public float combatCooldown = 0.5f;
        
        [Header("バランス調整範囲")]
        public float minDamageMultiplier = 0.5f;
        public float maxDamageMultiplier = 2.0f;
        public float minCritChance = 0.05f;
        public float maxCritChance = 0.3f;
    }
    
    [System.Serializable]
    public class EconomyBalance
    {
        [Header("金貨報酬")]
        public int baseGoldReward = 10;
        public float levelBonusMultiplier = 1.2f;
        public int dailyGoldBonus = 100;
        
        [Header("購入コスト")]
        public int baseMonsterCost = 50;
        public float rarityMultiplier = 2.0f;
        public int baseFloorExpansionCost = 200;
        public float floorCostMultiplier = 1.5f;
        
        [Header("売却価格")]
        public float sellPriceRatio = 0.7f;
        
        [Header("バランス調整範囲")]
        public int minGoldReward = 5;
        public int maxGoldReward = 50;
        public float minSellRatio = 0.5f;
        public float maxSellRatio = 0.9f;
    }
    
    [System.Serializable]
    public class InvaderBalance
    {
        [Header("出現頻度")]
        public float baseSpawnInterval = 15f;
        public float minSpawnInterval = 5f;
        public float maxSpawnInterval = 30f;
        public float floorSpawnMultiplier = 0.9f;
        
        [Header("ステータス")]
        public float baseHealthMultiplier = 1.0f;
        public float baseAttackMultiplier = 1.0f;
        public float levelScalingFactor = 1.1f;
        
        [Header("パーティサイズ")]
        public int minPartySize = 1;
        public int maxPartySize = 4;
        public float partyChancePerFloor = 0.1f;
        
        [Header("バランス調整範囲")]
        public float minHealthMultiplier = 0.7f;
        public float maxHealthMultiplier = 1.5f;
        public float minAttackMultiplier = 0.7f;
        public float maxAttackMultiplier = 1.5f;
    }
    
    [System.Serializable]
    public class MonsterBalance
    {
        [Header("基本ステータス")]
        public float baseHealthMultiplier = 1.0f;
        public float baseAttackMultiplier = 1.0f;
        public float baseManaMultiplier = 1.0f;
        
        [Header("回復システム")]
        public float baseRecoveryRate = 1f;
        public float shelterRecoveryMultiplier = 3f;
        public float abilityRecoveryBonus = 0.5f;
        
        [Header("アビリティ")]
        public float abilityEffectMultiplier = 1.0f;
        public float abilityCooldownMultiplier = 1.0f;
        public float reviveTimeMultiplier = 1.0f;
        
        [Header("バランス調整範囲")]
        public float minStatMultiplier = 0.7f;
        public float maxStatMultiplier = 1.5f;
        public float minRecoveryRate = 0.5f;
        public float maxRecoveryRate = 3.0f;
    }
    
    [System.Serializable]
    public class FloorBalance
    {
        [Header("階層設定")]
        public int maxMonstersPerFloor = 15;
        public float floorDifficultyMultiplier = 1.1f;
        public int bossFloorInterval = 5;
        
        [Header("拡張コスト")]
        public int baseExpansionCost = 200;
        public float expansionCostMultiplier = 1.5f;
        public int maxFloors = 50;
        
        [Header("バランス調整範囲")]
        public float minDifficultyMultiplier = 1.05f;
        public float maxDifficultyMultiplier = 1.3f;
        public int minExpansionCost = 100;
        public int maxExpansionCost = 1000;
    }
    
    [System.Serializable]
    public class GameplayMetrics
    {
        public float averageCombatDuration = 0f;
        public float playerWinRate = 0f;
        public float averageGoldPerMinute = 0f;
        public float averageFloorProgression = 0f;
        public int totalCombats = 0;
        public int playerVictories = 0;
        public float totalGoldEarned = 0f;
        public float totalPlayTime = 0f;
        
        public void Reset()
        {
            averageCombatDuration = 0f;
            playerWinRate = 0f;
            averageGoldPerMinute = 0f;
            averageFloorProgression = 0f;
            totalCombats = 0;
            playerVictories = 0;
            totalGoldEarned = 0f;
            totalPlayTime = 0f;
        }
        
        public void UpdateMetrics()
        {
            if (totalCombats > 0)
            {
                playerWinRate = (float)playerVictories / totalCombats;
            }
            
            if (totalPlayTime > 0)
            {
                averageGoldPerMinute = totalGoldEarned / (totalPlayTime / 60f);
            }
        }
    }
    
    private void Start()
    {
        LoadBalanceSettings();
        ApplyBalanceSettings();
    }
    
    private void Update()
    {
        if (enableAutoBalancing && Time.time - lastBalanceCheck >= balanceCheckInterval)
        {
            CheckAndAdjustBalance();
            lastBalanceCheck = Time.time;
        }
        
        UpdateMetrics();
    }
    
    /// <summary>
    /// ゲームプレイメトリクスを更新
    /// </summary>
    private void UpdateMetrics()
    {
        metrics.totalPlayTime += Time.deltaTime;
        
        // ResourceManagerから金貨情報を取得
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            metrics.totalGoldEarned = resourceManager.TotalGoldEarned;
        }
        
        // FloorSystemから階層情報を取得
        FloorSystem floorSystem = FindObjectOfType<FloorSystem>();
        if (floorSystem != null)
        {
            metrics.averageFloorProgression = floorSystem.CurrentFloor;
        }
        
        metrics.UpdateMetrics();
    }
    
    /// <summary>
    /// バランス調整の実行
    /// </summary>
    private void CheckAndAdjustBalance()
    {
        LogBalance("バランスチェック開始");
        
        // 戦闘バランスの調整
        AdjustCombatBalance();
        
        // 経済バランスの調整
        AdjustEconomyBalance();
        
        // 侵入者バランスの調整
        AdjustInvaderBalance();
        
        // モンスターバランスの調整
        AdjustMonsterBalance();
        
        // 階層バランスの調整
        AdjustFloorBalance();
        
        // 調整後の設定を適用
        ApplyBalanceSettings();
        
        LogBalance("バランス調整完了");
    }
    
    private void AdjustCombatBalance()
    {
        // プレイヤーの勝率に基づいて戦闘バランスを調整
        if (metrics.playerWinRate < 0.4f) // 勝率が低すぎる場合
        {
            combatBalance.baseDamageMultiplier = Mathf.Min(
                combatBalance.baseDamageMultiplier * 1.1f,
                combatBalance.maxDamageMultiplier
            );
            
            combatBalance.criticalHitChance = Mathf.Min(
                combatBalance.criticalHitChance * 1.1f,
                combatBalance.maxCritChance
            );
            
            LogBalance("戦闘バランス調整: プレイヤー有利に調整");
        }
        else if (metrics.playerWinRate > 0.8f) // 勝率が高すぎる場合
        {
            combatBalance.baseDamageMultiplier = Mathf.Max(
                combatBalance.baseDamageMultiplier * 0.9f,
                combatBalance.minDamageMultiplier
            );
            
            combatBalance.criticalHitChance = Mathf.Max(
                combatBalance.criticalHitChance * 0.9f,
                combatBalance.minCritChance
            );
            
            LogBalance("戦闘バランス調整: 難易度上昇");
        }
    }
    
    private void AdjustEconomyBalance()
    {
        // 金貨獲得率に基づいて経済バランスを調整
        if (metrics.averageGoldPerMinute < 20f) // 金貨獲得が少なすぎる場合
        {
            economyBalance.baseGoldReward = Mathf.Min(
                (int)(economyBalance.baseGoldReward * 1.1f),
                economyBalance.maxGoldReward
            );
            
            economyBalance.sellPriceRatio = Mathf.Min(
                economyBalance.sellPriceRatio * 1.05f,
                economyBalance.maxSellRatio
            );
            
            LogBalance("経済バランス調整: 金貨獲得量増加");
        }
        else if (metrics.averageGoldPerMinute > 60f) // 金貨獲得が多すぎる場合
        {
            economyBalance.baseGoldReward = Mathf.Max(
                (int)(economyBalance.baseGoldReward * 0.9f),
                economyBalance.minGoldReward
            );
            
            economyBalance.sellPriceRatio = Mathf.Max(
                economyBalance.sellPriceRatio * 0.95f,
                economyBalance.minSellRatio
            );
            
            LogBalance("経済バランス調整: 金貨獲得量減少");
        }
    }
    
    private void AdjustInvaderBalance()
    {
        // 階層進行度に基づいて侵入者バランスを調整
        if (metrics.averageFloorProgression < 5f) // 進行が遅い場合
        {
            invaderBalance.baseSpawnInterval = Mathf.Max(
                invaderBalance.baseSpawnInterval * 1.1f,
                invaderBalance.minSpawnInterval
            );
            
            invaderBalance.baseHealthMultiplier = Mathf.Max(
                invaderBalance.baseHealthMultiplier * 0.95f,
                invaderBalance.minHealthMultiplier
            );
            
            LogBalance("侵入者バランス調整: 侵入者弱体化");
        }
        else if (metrics.averageFloorProgression > 15f) // 進行が早い場合
        {
            invaderBalance.baseSpawnInterval = Mathf.Min(
                invaderBalance.baseSpawnInterval * 0.9f,
                invaderBalance.maxSpawnInterval
            );
            
            invaderBalance.baseHealthMultiplier = Mathf.Min(
                invaderBalance.baseHealthMultiplier * 1.05f,
                invaderBalance.maxHealthMultiplier
            );
            
            LogBalance("侵入者バランス調整: 侵入者強化");
        }
    }
    
    private void AdjustMonsterBalance()
    {
        // 戦闘結果に基づいてモンスターバランスを調整
        if (metrics.playerWinRate < 0.5f)
        {
            monsterBalance.baseHealthMultiplier = Mathf.Min(
                monsterBalance.baseHealthMultiplier * 1.05f,
                monsterBalance.maxStatMultiplier
            );
            
            monsterBalance.baseRecoveryRate = Mathf.Min(
                monsterBalance.baseRecoveryRate * 1.1f,
                monsterBalance.maxRecoveryRate
            );
            
            LogBalance("モンスターバランス調整: モンスター強化");
        }
        else if (metrics.playerWinRate > 0.7f)
        {
            monsterBalance.baseHealthMultiplier = Mathf.Max(
                monsterBalance.baseHealthMultiplier * 0.95f,
                monsterBalance.minStatMultiplier
            );
            
            monsterBalance.baseRecoveryRate = Mathf.Max(
                monsterBalance.baseRecoveryRate * 0.9f,
                monsterBalance.minRecoveryRate
            );
            
            LogBalance("モンスターバランス調整: モンスター弱体化");
        }
    }
    
    private void AdjustFloorBalance()
    {
        // 階層拡張の頻度に基づいて調整
        if (metrics.averageFloorProgression > 20f)
        {
            floorBalance.baseExpansionCost = Mathf.Min(
                (int)(floorBalance.baseExpansionCost * 1.1f),
                floorBalance.maxExpansionCost
            );
            
            LogBalance("階層バランス調整: 拡張コスト増加");
        }
        else if (metrics.averageFloorProgression < 5f)
        {
            floorBalance.baseExpansionCost = Mathf.Max(
                (int)(floorBalance.baseExpansionCost * 0.9f),
                floorBalance.minExpansionCost
            );
            
            LogBalance("階層バランス調整: 拡張コスト減少");
        }
    }
    
    /// <summary>
    /// バランス設定を各システムに適用
    /// </summary>
    private void ApplyBalanceSettings()
    {
        // CombatManagerに戦闘バランスを適用
        CombatManager combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            combatManager.ApplyBalanceSettings(combatBalance);
        }
        
        // ResourceManagerに経済バランスを適用
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            resourceManager.ApplyBalanceSettings(economyBalance);
        }
        
        // InvaderSpawnerに侵入者バランスを適用
        InvaderSpawner invaderSpawner = FindObjectOfType<InvaderSpawner>();
        if (invaderSpawner != null)
        {
            invaderSpawner.ApplyBalanceSettings(invaderBalance);
        }
        
        // FloorSystemに階層バランスを適用
        FloorSystem floorSystem = FindObjectOfType<FloorSystem>();
        if (floorSystem != null)
        {
            floorSystem.ApplyBalanceSettings(floorBalance);
        }
    }
    
    /// <summary>
    /// バランス設定をファイルから読み込み
    /// </summary>
    private void LoadBalanceSettings()
    {
        string filePath = Application.persistentDataPath + "/balance_settings.json";
        
        if (System.IO.File.Exists(filePath))
        {
            try
            {
                string json = System.IO.File.ReadAllText(filePath);
                var settings = JsonUtility.FromJson<BalanceSettings>(json);
                
                if (settings != null)
                {
                    combatBalance = settings.combatBalance;
                    economyBalance = settings.economyBalance;
                    invaderBalance = settings.invaderBalance;
                    monsterBalance = settings.monsterBalance;
                    floorBalance = settings.floorBalance;
                    
                    LogBalance("バランス設定を読み込みました");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"バランス設定の読み込みに失敗: {e.Message}");
            }
        }
    }
    
    /// <summary>
    /// バランス設定をファイルに保存
    /// </summary>
    public void SaveBalanceSettings()
    {
        try
        {
            var settings = new BalanceSettings
            {
                combatBalance = combatBalance,
                economyBalance = economyBalance,
                invaderBalance = invaderBalance,
                monsterBalance = monsterBalance,
                floorBalance = floorBalance
            };
            
            string json = JsonUtility.ToJson(settings, true);
            string filePath = Application.persistentDataPath + "/balance_settings.json";
            
            System.IO.File.WriteAllText(filePath, json);
            LogBalance("バランス設定を保存しました");
        }
        catch (Exception e)
        {
            Debug.LogError($"バランス設定の保存に失敗: {e.Message}");
        }
    }
    
    /// <summary>
    /// 戦闘結果を記録
    /// </summary>
    public void RecordCombatResult(bool playerWon, float combatDuration)
    {
        metrics.totalCombats++;
        if (playerWon)
        {
            metrics.playerVictories++;
        }
        
        // 平均戦闘時間の更新
        metrics.averageCombatDuration = 
            (metrics.averageCombatDuration * (metrics.totalCombats - 1) + combatDuration) / metrics.totalCombats;
    }
    
    /// <summary>
    /// メトリクス情報を取得
    /// </summary>
    public GameplayMetrics GetMetrics()
    {
        return metrics;
    }
    
    /// <summary>
    /// バランス設定をリセット
    /// </summary>
    public void ResetBalanceSettings()
    {
        combatBalance = new CombatBalance();
        economyBalance = new EconomyBalance();
        invaderBalance = new InvaderBalance();
        monsterBalance = new MonsterBalance();
        floorBalance = new FloorBalance();
        
        ApplyBalanceSettings();
        LogBalance("バランス設定をリセットしました");
    }
    
    private void LogBalance(string message)
    {
        if (enableDetailedLogging)
        {
            Debug.Log($"[バランス調整] {message}");
        }
    }
    
    [System.Serializable]
    private class BalanceSettings
    {
        public CombatBalance combatBalance;
        public EconomyBalance economyBalance;
        public InvaderBalance invaderBalance;
        public MonsterBalance monsterBalance;
        public FloorBalance floorBalance;
    }
}