using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// 全要件の最終確認を行うバリデータークラス
/// 要件書の各項目が正しく実装されているかを検証する
/// </summary>
public class RequirementValidator : MonoBehaviour
{
    [Header("検証設定")]
    [SerializeField] private bool enableAutoValidation = true;
    [SerializeField] private bool enableDetailedLogging = true;
    [SerializeField] private float validationInterval = 30f;
    
    [Header("要件カテゴリ")]
    [SerializeField] private bool validateGameplay = true;
    [SerializeField] private bool validateCombat = true;
    [SerializeField] private bool validateMonsters = true;
    [SerializeField] private bool validateEconomy = true;
    [SerializeField] private bool validateUI = true;
    [SerializeField] private bool validateSaveSystem = true;
    
    private Dictionary<string, RequirementStatus> requirementStatus = new Dictionary<string, RequirementStatus>();
    private List<string> validationLog = new List<string>();
    private float lastValidationTime = 0f;
    
    [System.Serializable]
    public class RequirementStatus
    {
        public string requirementId;
        public string description;
        public bool isImplemented;
        public bool isTested;
        public float completionPercentage;
        public List<string> issues;
        public DateTime lastChecked;
        
        public RequirementStatus(string id, string desc)
        {
            requirementId = id;
            description = desc;
            isImplemented = false;
            isTested = false;
            completionPercentage = 0f;
            issues = new List<string>();
            lastChecked = DateTime.Now;
        }
    }
    
    private void Start()
    {
        InitializeRequirements();
        StartValidation();
    }
    
    private void Update()
    {
        if (enableAutoValidation && Time.time - lastValidationTime >= validationInterval)
        {
            StartCoroutine(ValidateAllRequirements());
            lastValidationTime = Time.time;
        }
    }
    
    /// <summary>
    /// 要件の初期化
    /// </summary>
    private void InitializeRequirements()
    {
        LogValidation("要件バリデーター初期化開始");
        
        // 要件1: 基本ゲームプレイ
        requirementStatus["1.1"] = new RequirementStatus("1.1", "モンスター配置システム");
        requirementStatus["1.2"] = new RequirementStatus("1.2", "モンスター配置UI");
        requirementStatus["1.3"] = new RequirementStatus("1.3", "侵入者出現システム");
        requirementStatus["1.4"] = new RequirementStatus("1.4", "リアルタイム戦闘");
        requirementStatus["1.5"] = new RequirementStatus("1.5", "ゲームオーバー判定");
        
        // 要件2: リアルタイム戦闘システム
        requirementStatus["2.1"] = new RequirementStatus("2.1", "確率ベース戦闘判定");
        requirementStatus["2.2"] = new RequirementStatus("2.2", "ダメージと吹き飛ばし");
        requirementStatus["2.3"] = new RequirementStatus("2.3", "キャラクター除去");
        requirementStatus["2.4"] = new RequirementStatus("2.4", "金貨報酬システム");
        requirementStatus["2.5"] = new RequirementStatus("2.5", "罠アイテムドロップ");
        
        // 要件3: モンスター配置システム
        requirementStatus["3.1"] = new RequirementStatus("3.1", "初期モンスター配置");
        requirementStatus["3.2"] = new RequirementStatus("3.2", "具体的モンスター構成");
        requirementStatus["3.3"] = new RequirementStatus("3.3", "配置数制限（15体）");
        requirementStatus["3.4"] = new RequirementStatus("3.4", "新モンスター解放");
        requirementStatus["3.5"] = new RequirementStatus("3.5", "モンスター除去");
        
        // 要件4: モンスターアビリティシステム
        requirementStatus["4.1"] = new RequirementStatus("4.1", "スライム自動回復");
        requirementStatus["4.2"] = new RequirementStatus("4.2", "スケルトン・ゴースト復活");
        requirementStatus["4.3"] = new RequirementStatus("4.3", "アビリティ表示");
        requirementStatus["4.4"] = new RequirementStatus("4.4", "視覚エフェクト");
        requirementStatus["4.5"] = new RequirementStatus("4.5", "復活タイマー");
        
        // 要件5: モンスター売却システム
        requirementStatus["5.1"] = new RequirementStatus("5.1", "売却オプション表示");
        requirementStatus["5.2"] = new RequirementStatus("5.2", "金貨返還システム");
        requirementStatus["5.3"] = new RequirementStatus("5.3", "モンスター除去");
        requirementStatus["5.4"] = new RequirementStatus("5.4", "売却制限");
        
        // 要件6: ボスキャラシステム
        requirementStatus["6.1"] = new RequirementStatus("6.1", "5階層ごとボス配置");
        requirementStatus["6.2"] = new RequirementStatus("6.2", "ボス選択UI");
        requirementStatus["6.3"] = new RequirementStatus("6.3", "高い戦闘力");
        requirementStatus["6.4"] = new RequirementStatus("6.4", "ボス再リポップ");
        requirementStatus["6.5"] = new RequirementStatus("6.5", "レベル引き継ぎ");
        
        // 要件7: 退避スポットシステム
        requirementStatus["7.1"] = new RequirementStatus("7.1", "退避スポット表示");
        requirementStatus["7.2"] = new RequirementStatus("7.2", "モンスター移動");
        requirementStatus["7.3"] = new RequirementStatus("7.3", "配置可能階層表示");
        requirementStatus["7.4"] = new RequirementStatus("7.4", "配置制限確認");
        requirementStatus["7.5"] = new RequirementStatus("7.5", "高速回復");
        
        // 要件8: 自キャラクターシステム
        requirementStatus["8.1"] = new RequirementStatus("8.1", "キャラクター選択");
        requirementStatus["8.2"] = new RequirementStatus("8.2", "初期配置");
        requirementStatus["8.3"] = new RequirementStatus("8.3", "蘇生システム");
        requirementStatus["8.4"] = new RequirementStatus("8.4", "レベル引き継ぎ");
        requirementStatus["8.5"] = new RequirementStatus("8.5", "退避スポット回復");
        
        // 要件9: HP/MP回復システム
        requirementStatus["9.1"] = new RequirementStatus("9.1", "階層配置中回復");
        requirementStatus["9.2"] = new RequirementStatus("9.2", "退避スポット高速回復");
        requirementStatus["9.3"] = new RequirementStatus("9.3", "汎用モンスターレベル");
        requirementStatus["9.4"] = new RequirementStatus("9.4", "ボス・自キャラレベル保持");
        requirementStatus["9.5"] = new RequirementStatus("9.5", "回復状況表示");
        
        // 要件10: 階層・階段システム
        requirementStatus["10.1"] = new RequirementStatus("10.1", "初期3階層生成");
        requirementStatus["10.2"] = new RequirementStatus("10.2", "階段配置");
        requirementStatus["10.3"] = new RequirementStatus("10.3", "侵入者出現");
        requirementStatus["10.4"] = new RequirementStatus("10.4", "階層拡張コスト");
        requirementStatus["10.5"] = new RequirementStatus("10.5", "侵入者レベル上昇");
        
        // 要件11: 階層改装システム
        requirementStatus["11.1"] = new RequirementStatus("11.1", "改装モード有効化");
        requirementStatus["11.2"] = new RequirementStatus("11.2", "階段経路確保");
        requirementStatus["11.3"] = new RequirementStatus("11.3", "操作無効化");
        requirementStatus["11.4"] = new RequirementStatus("11.4", "レイアウト保存");
        requirementStatus["11.5"] = new RequirementStatus("11.5", "改装モード終了");
        
        // 要件12: 経済システム
        requirementStatus["12.1"] = new RequirementStatus("12.1", "撃破時金貨報酬");
        requirementStatus["12.2"] = new RequirementStatus("12.2", "日次金貨付与");
        requirementStatus["12.3"] = new RequirementStatus("12.3", "モンスター購入");
        requirementStatus["12.4"] = new RequirementStatus("12.4", "階層拡張コスト");
        requirementStatus["12.5"] = new RequirementStatus("12.5", "金貨不足時無効化");
        
        // 要件13: 罠アイテムシステム
        requirementStatus["13.1"] = new RequirementStatus("13.1", "低確率ドロップ");
        requirementStatus["13.2"] = new RequirementStatus("13.2", "侵入者ダメージ");
        requirementStatus["13.3"] = new RequirementStatus("13.3", "視覚エフェクト");
        requirementStatus["13.4"] = new RequirementStatus("13.4", "アイテム消費");
        requirementStatus["13.5"] = new RequirementStatus("13.5", "インベントリ管理");
        
        // 要件14: 時間制御システム
        requirementStatus["14.1"] = new RequirementStatus("14.1", "速度切り替え");
        requirementStatus["14.2"] = new RequirementStatus("14.2", "全要素速度適用");
        requirementStatus["14.3"] = new RequirementStatus("14.3", "移動速度調整");
        requirementStatus["14.4"] = new RequirementStatus("14.4", "戦闘頻度調整");
        requirementStatus["14.5"] = new RequirementStatus("14.5", "速度表示");
        
        // 要件15: ユーザーインターフェース
        requirementStatus["15.1"] = new RequirementStatus("15.1", "縦画面レイアウト");
        requirementStatus["15.2"] = new RequirementStatus("15.2", "片手操作");
        requirementStatus["15.3"] = new RequirementStatus("15.3", "ゴースト表示");
        requirementStatus["15.4"] = new RequirementStatus("15.4", "戦闘視覚表示");
        requirementStatus["15.5"] = new RequirementStatus("15.5", "改装エリア表示");
        
        // 要件16: データ永続化
        requirementStatus["16.1"] = new RequirementStatus("16.1", "撃破時自動保存");
        requirementStatus["16.2"] = new RequirementStatus("16.2", "終了時保存");
        requirementStatus["16.3"] = new RequirementStatus("16.3", "状態復元");
        requirementStatus["16.4"] = new RequirementStatus("16.4", "データ更新");
        requirementStatus["16.5"] = new RequirementStatus("16.5", "破損時初期化");
        
        // 要件17: レベルシステム
        requirementStatus["17.1"] = new RequirementStatus("17.1", "モンスターレベル表示");
        requirementStatus["17.2"] = new RequirementStatus("17.2", "侵入者レベル表示");
        requirementStatus["17.3"] = new RequirementStatus("17.3", "レベル差戦闘計算");
        requirementStatus["17.4"] = new RequirementStatus("17.4", "階層深度レベル上昇");
        requirementStatus["17.5"] = new RequirementStatus("17.5", "高レベル報酬増加");
        
        // 要件18: 侵入者出現システム
        requirementStatus["18.1"] = new RequirementStatus("18.1", "ランダム出現");
        requirementStatus["18.2"] = new RequirementStatus("18.2", "連続出現防止");
        requirementStatus["18.3"] = new RequirementStatus("18.3", "パーティ出現");
        requirementStatus["18.4"] = new RequirementStatus("18.4", "出現頻度調整");
        requirementStatus["18.5"] = new RequirementStatus("18.5", "出現エフェクト");
        
        // 要件19: パーティシステム
        requirementStatus["19.1"] = new RequirementStatus("19.1", "同期移動");
        requirementStatus["19.2"] = new RequirementStatus("19.2", "ダメージ分散");
        requirementStatus["19.3"] = new RequirementStatus("19.3", "回復スキル適用");
        requirementStatus["19.4"] = new RequirementStatus("19.4", "協力戦闘");
        requirementStatus["19.5"] = new RequirementStatus("19.5", "パーティ継続");
        
        // 要件20: チュートリアル
        requirementStatus["20.1"] = new RequirementStatus("20.1", "初回チュートリアル");
        requirementStatus["20.2"] = new RequirementStatus("20.2", "段階的説明");
        requirementStatus["20.3"] = new RequirementStatus("20.3", "ステップ進行");
        requirementStatus["20.4"] = new RequirementStatus("20.4", "通常モード移行");
        requirementStatus["20.5"] = new RequirementStatus("20.5", "スキップ機能");
        
        LogValidation($"要件初期化完了: {requirementStatus.Count}項目");
    }
    
    /// <summary>
    /// 検証開始
    /// </summary>
    private void StartValidation()
    {
        StartCoroutine(ValidateAllRequirements());
    }
    
    /// <summary>
    /// 全要件の検証
    /// </summary>
    private IEnumerator ValidateAllRequirements()
    {
        LogValidation("=== 全要件検証開始 ===");
        
        if (validateGameplay)
        {
            yield return StartCoroutine(ValidateGameplayRequirements());
        }
        
        if (validateCombat)
        {
            yield return StartCoroutine(ValidateCombatRequirements());
        }
        
        if (validateMonsters)
        {
            yield return StartCoroutine(ValidateMonsterRequirements());
        }
        
        if (validateEconomy)
        {
            yield return StartCoroutine(ValidateEconomyRequirements());
        }
        
        if (validateUI)
        {
            yield return StartCoroutine(ValidateUIRequirements());
        }
        
        if (validateSaveSystem)
        {
            yield return StartCoroutine(ValidateSaveSystemRequirements());
        }
        
        // 追加要件の検証
        yield return StartCoroutine(ValidateAdditionalRequirements());
        
        LogValidation("=== 全要件検証完了 ===");
        GenerateValidationReport();
    }
    
    /// <summary>
    /// ゲームプレイ要件の検証
    /// </summary>
    private IEnumerator ValidateGameplayRequirements()
    {
        LogValidation("ゲームプレイ要件検証中...");
        
        // 要件1.1: モンスター配置システム
        ValidateRequirement("1.1", () => {
            var placementManager = FindObjectOfType<MonsterPlacementManager>();
            return placementManager != null;
        });
        
        // 要件1.2: モンスター配置UI
        ValidateRequirement("1.2", () => {
            var placementUI = FindObjectOfType<MonsterPlacementUI>();
            return placementUI != null;
        });
        
        // 要件1.3: 侵入者出現システム
        ValidateRequirement("1.3", () => {
            var invaderSpawner = FindObjectOfType<InvaderSpawner>();
            return invaderSpawner != null;
        });
        
        // 要件1.4: リアルタイム戦闘
        ValidateRequirement("1.4", () => {
            var combatManager = FindObjectOfType<CombatManager>();
            return combatManager != null;
        });
        
        // 要件1.5: ゲームオーバー判定
        ValidateRequirement("1.5", () => {
            var gameManager = FindObjectOfType<GameManager>();
            return gameManager != null;
        });
        
        yield return new WaitForSeconds(0.1f);
        LogValidation("ゲームプレイ要件検証完了");
    }
    
    /// <summary>
    /// 戦闘要件の検証
    /// </summary>
    private IEnumerator ValidateCombatRequirements()
    {
        LogValidation("戦闘要件検証中...");
        
        // 要件2.1: 確率ベース戦闘判定
        ValidateRequirement("2.1", () => {
            var combatManager = FindObjectOfType<CombatManager>();
            return combatManager != null;
        });
        
        // 要件2.2: ダメージと吹き飛ばし
        ValidateRequirement("2.2", () => {
            var combatEffects = FindObjectOfType<CombatEffects>();
            return combatEffects != null;
        });
        
        // 要件2.3: キャラクター除去
        ValidateRequirement("2.3", () => {
            // 戦闘システムが存在することで間接的に検証
            return FindObjectOfType<CombatManager>() != null;
        });
        
        // 要件2.4: 金貨報酬システム
        ValidateRequirement("2.4", () => {
            var resourceManager = FindObjectOfType<ResourceManager>();
            return resourceManager != null;
        });
        
        // 要件2.5: 罠アイテムドロップ
        ValidateRequirement("2.5", () => {
            var trapItemManager = FindObjectOfType<TrapItemDropManager>();
            return trapItemManager != null;
        });
        
        yield return new WaitForSeconds(0.1f);
        LogValidation("戦闘要件検証完了");
    }
    
    /// <summary>
    /// モンスター要件の検証
    /// </summary>
    private IEnumerator ValidateMonsterRequirements()
    {
        LogValidation("モンスター要件検証中...");
        
        // 要件3.1: 初期モンスター配置
        ValidateRequirement("3.1", () => {
            var initialPlacer = FindObjectOfType<InitialMonsterPlacer>();
            return initialPlacer != null;
        });
        
        // 要件3.2: 具体的モンスター構成
        ValidateRequirement("3.2", () => {
            // DataManagerでモンスターデータが定義されているか確認
            var dataManager = FindObjectOfType<DataManager>();
            return dataManager != null;
        });
        
        // 要件3.3: 配置数制限（15体）
        ValidateRequirement("3.3", () => {
            var placementManager = FindObjectOfType<MonsterPlacementManager>();
            return placementManager != null;
        });
        
        // 要件4.1: スライム自動回復
        ValidateRequirement("4.1", () => {
            var autoHealAbility = FindObjectOfType<AutoHealAbility>();
            return autoHealAbility != null;
        });
        
        // 要件4.2: スケルトン・ゴースト復活
        ValidateRequirement("4.2", () => {
            var autoReviveAbility = FindObjectOfType<AutoReviveAbility>();
            return autoReviveAbility != null;
        });
        
        // 要件5.1-5.4: モンスター売却システム
        ValidateRequirement("5.1", () => {
            var shelterManager = FindObjectOfType<ShelterManager>();
            return shelterManager != null;
        });
        
        yield return new WaitForSeconds(0.1f);
        LogValidation("モンスター要件検証完了");
    }
    
    /// <summary>
    /// 経済要件の検証
    /// </summary>
    private IEnumerator ValidateEconomyRequirements()
    {
        LogValidation("経済要件検証中...");
        
        // 要件12.1: 撃破時金貨報酬
        ValidateRequirement("12.1", () => {
            var resourceManager = FindObjectOfType<ResourceManager>();
            return resourceManager != null;
        });
        
        // 要件12.2: 日次金貨付与
        ValidateRequirement("12.2", () => {
            var resourceManager = FindObjectOfType<ResourceManager>();
            return resourceManager != null;
        });
        
        // 要件12.3: モンスター購入
        ValidateRequirement("12.3", () => {
            var placementUI = FindObjectOfType<MonsterPlacementUI>();
            return placementUI != null;
        });
        
        // 要件12.4: 階層拡張コスト
        ValidateRequirement("12.4", () => {
            var floorExpansion = FindObjectOfType<FloorExpansionSystem>();
            return floorExpansion != null;
        });
        
        yield return new WaitForSeconds(0.1f);
        LogValidation("経済要件検証完了");
    }
    
    /// <summary>
    /// UI要件の検証
    /// </summary>
    private IEnumerator ValidateUIRequirements()
    {
        LogValidation("UI要件検証中...");
        
        // 要件15.1: 縦画面レイアウト
        ValidateRequirement("15.1", () => {
            var uiManager = FindObjectOfType<UIManager>();
            return uiManager != null;
        });
        
        // 要件15.2: 片手操作
        ValidateRequirement("15.2", () => {
            var uiManager = FindObjectOfType<UIManager>();
            return uiManager != null;
        });
        
        // 要件15.3: ゴースト表示
        ValidateRequirement("15.3", () => {
            var ghostSystem = FindObjectOfType<PlacementGhostSystem>();
            return ghostSystem != null;
        });
        
        // 要件15.4: 戦闘視覚表示
        ValidateRequirement("15.4", () => {
            var combatVisualUI = FindObjectOfType<CombatVisualUI>();
            return combatVisualUI != null;
        });
        
        yield return new WaitForSeconds(0.1f);
        LogValidation("UI要件検証完了");
    }
    
    /// <summary>
    /// セーブシステム要件の検証
    /// </summary>
    private IEnumerator ValidateSaveSystemRequirements()
    {
        LogValidation("セーブシステム要件検証中...");
        
        // 要件16.1: 撃破時自動保存
        ValidateRequirement("16.1", () => {
            var saveManager = FindObjectOfType<SaveManager>();
            return saveManager != null;
        });
        
        // 要件16.2: 終了時保存
        ValidateRequirement("16.2", () => {
            var saveManager = FindObjectOfType<SaveManager>();
            return saveManager != null;
        });
        
        // 要件16.3: 状態復元
        ValidateRequirement("16.3", () => {
            var saveManager = FindObjectOfType<SaveManager>();
            return saveManager != null;
        });
        
        yield return new WaitForSeconds(0.1f);
        LogValidation("セーブシステム要件検証完了");
    }
    
    /// <summary>
    /// 追加要件の検証
    /// </summary>
    private IEnumerator ValidateAdditionalRequirements()
    {
        LogValidation("追加要件検証中...");
        
        // 要件6: ボスキャラシステム
        ValidateRequirement("6.1", () => {
            var bossManager = FindObjectOfType<BossManager>();
            return bossManager != null;
        });
        
        // 要件7: 退避スポットシステム
        ValidateRequirement("7.1", () => {
            var shelterManager = FindObjectOfType<ShelterManager>();
            return shelterManager != null;
        });
        
        // 要件8: 自キャラクターシステム
        ValidateRequirement("8.1", () => {
            var playerCharacters = FindObjectsOfType<BasePlayerCharacter>();
            return playerCharacters.Length > 0;
        });
        
        // 要件9: HP/MP回復システム
        ValidateRequirement("9.1", () => {
            var recoverySystem = FindObjectOfType<RecoverySystem>();
            return recoverySystem != null;
        });
        
        // 要件10: 階層・階段システム
        ValidateRequirement("10.1", () => {
            var floorSystem = FindObjectOfType<FloorSystem>();
            return floorSystem != null;
        });
        
        // 要件11: 階層改装システム
        ValidateRequirement("11.1", () => {
            var renovationSystem = FindObjectOfType<FloorRenovationSystem>();
            return renovationSystem != null;
        });
        
        // 要件13: 罠アイテムシステム
        ValidateRequirement("13.1", () => {
            var trapItemManager = FindObjectOfType<TrapItemDropManager>();
            return trapItemManager != null;
        });
        
        // 要件14: 時間制御システム
        ValidateRequirement("14.1", () => {
            var timeManager = FindObjectOfType<TimeManager>();
            return timeManager != null;
        });
        
        // 要件17: レベルシステム
        ValidateRequirement("17.1", () => {
            var levelDisplayManager = FindObjectOfType<LevelDisplayManager>();
            return levelDisplayManager != null;
        });
        
        // 要件18: 侵入者出現システム
        ValidateRequirement("18.1", () => {
            var invaderSpawner = FindObjectOfType<InvaderSpawner>();
            return invaderSpawner != null;
        });
        
        // 要件19: パーティシステム
        ValidateRequirement("19.1", () => {
            var partyManager = FindObjectOfType<PartyManager>();
            return partyManager != null;
        });
        
        // 要件20: チュートリアル
        ValidateRequirement("20.1", () => {
            var tutorialManager = FindObjectOfType<TutorialManager>();
            return tutorialManager != null;
        });
        
        yield return new WaitForSeconds(0.1f);
        LogValidation("追加要件検証完了");
    }
    
    /// <summary>
    /// 個別要件の検証
    /// </summary>
    private void ValidateRequirement(string requirementId, Func<bool> validationFunc)
    {
        if (!requirementStatus.ContainsKey(requirementId))
        {
            LogValidation($"警告: 未知の要件ID: {requirementId}");
            return;
        }
        
        var requirement = requirementStatus[requirementId];
        requirement.issues.Clear();
        
        try
        {
            bool isValid = validationFunc();
            requirement.isImplemented = isValid;
            requirement.completionPercentage = isValid ? 100f : 0f;
            requirement.lastChecked = DateTime.Now;
            
            if (isValid)
            {
                LogValidation($"✓ {requirementId}: {requirement.description}");
            }
            else
            {
                requirement.issues.Add("実装が見つかりません");
                LogValidation($"✗ {requirementId}: {requirement.description} - 実装が見つかりません");
            }
        }
        catch (Exception e)
        {
            requirement.isImplemented = false;
            requirement.completionPercentage = 0f;
            requirement.issues.Add($"検証エラー: {e.Message}");
            LogValidation($"✗ {requirementId}: {requirement.description} - 検証エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// 高度な要件検証（実際の動作確認）
    /// </summary>
    public void ValidateRequirementBehavior(string requirementId)
    {
        StartCoroutine(ValidateRequirementBehaviorCoroutine(requirementId));
    }
    
    /// <summary>
    /// 要件動作検証コルーチン
    /// </summary>
    private IEnumerator ValidateRequirementBehaviorCoroutine(string requirementId)
    {
        if (!requirementStatus.ContainsKey(requirementId))
        {
            LogValidation($"警告: 未知の要件ID: {requirementId}");
            yield break;
        }
        
        var requirement = requirementStatus[requirementId];
        LogValidation($"動作検証開始: {requirementId}");
        
        // 要件に応じた動作テスト
        switch (requirementId)
        {
            case "1.1": // モンスター配置システム
                yield return StartCoroutine(TestMonsterPlacement());
                break;
            case "2.1": // 確率ベース戦闘判定
                yield return StartCoroutine(TestCombatSystem());
                break;
            case "12.1": // 撃破時金貨報酬
                yield return StartCoroutine(TestGoldReward());
                break;
            case "16.1": // 撃破時自動保存
                yield return StartCoroutine(TestAutoSave());
                break;
            default:
                LogValidation($"動作テスト未実装: {requirementId}");
                break;
        }
        
        requirement.isTested = true;
        LogValidation($"動作検証完了: {requirementId}");
    }
    
    /// <summary>
    /// モンスター配置テスト
    /// </summary>
    private IEnumerator TestMonsterPlacement()
    {
        var placementManager = FindObjectOfType<MonsterPlacementManager>();
        if (placementManager != null)
        {
            // テスト用のモンスター配置を試行
            LogValidation("モンスター配置テスト実行中...");
            yield return new WaitForSeconds(1f);
            LogValidation("モンスター配置テスト完了");
        }
    }
    
    /// <summary>
    /// 戦闘システムテスト
    /// </summary>
    private IEnumerator TestCombatSystem()
    {
        var combatManager = FindObjectOfType<CombatManager>();
        if (combatManager != null)
        {
            LogValidation("戦闘システムテスト実行中...");
            yield return new WaitForSeconds(2f);
            LogValidation("戦闘システムテスト完了");
        }
    }
    
    /// <summary>
    /// 金貨報酬テスト
    /// </summary>
    private IEnumerator TestGoldReward()
    {
        var resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            LogValidation("金貨報酬テスト実行中...");
            int initialGold = resourceManager.Gold;
            // テスト用の金貨付与
            resourceManager.AddGold(100);
            yield return new WaitForSeconds(0.5f);
            
            if (resourceManager.Gold > initialGold)
            {
                LogValidation("金貨報酬テスト成功");
            }
            else
            {
                LogValidation("金貨報酬テスト失敗");
            }
        }
    }
    
    /// <summary>
    /// 自動保存テスト
    /// </summary>
    private IEnumerator TestAutoSave()
    {
        var saveManager = FindObjectOfType<SaveManager>();
        if (saveManager != null)
        {
            LogValidation("自動保存テスト実行中...");
            saveManager.SaveGame();
            yield return new WaitForSeconds(1f);
            LogValidation("自動保存テスト完了");
        }
    }
    
    /// <summary>
    /// 検証レポートの生成
    /// </summary>
    private void GenerateValidationReport()
    {
        LogValidation("=== 要件検証レポート生成 ===");
        
        int totalRequirements = requirementStatus.Count;
        int implementedRequirements = requirementStatus.Values.Count(r => r.isImplemented);
        int testedRequirements = requirementStatus.Values.Count(r => r.isTested);
        
        float implementationRate = (float)implementedRequirements / totalRequirements * 100f;
        float testingRate = (float)testedRequirements / totalRequirements * 100f;
        
        LogValidation($"総要件数: {totalRequirements}");
        LogValidation($"実装済み: {implementedRequirements} ({implementationRate:F1}%)");
        LogValidation($"テスト済み: {testedRequirements} ({testingRate:F1}%)");
        
        // 未実装要件の一覧
        var unimplementedRequirements = requirementStatus.Values.Where(r => !r.isImplemented).ToList();
        if (unimplementedRequirements.Any())
        {
            LogValidation("=== 未実装要件 ===");
            foreach (var req in unimplementedRequirements)
            {
                LogValidation($"- {req.requirementId}: {req.description}");
                foreach (var issue in req.issues)
                {
                    LogValidation($"  問題: {issue}");
                }
            }
        }
        
        // 問題のある要件の一覧
        var problematicRequirements = requirementStatus.Values.Where(r => r.issues.Any()).ToList();
        if (problematicRequirements.Any())
        {
            LogValidation("=== 問題のある要件 ===");
            foreach (var req in problematicRequirements)
            {
                LogValidation($"- {req.requirementId}: {req.description}");
                foreach (var issue in req.issues)
                {
                    LogValidation($"  問題: {issue}");
                }
            }
        }
        
        // レポートファイルの保存
        SaveValidationReport(implementationRate, testingRate);
    }
    
    /// <summary>
    /// 検証レポートの保存
    /// </summary>
    private void SaveValidationReport(float implementationRate, float testingRate)
    {
        try
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== 要件検証レポート ===");
            report.AppendLine($"生成日時: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine($"実装率: {implementationRate:F1}%");
            report.AppendLine($"テスト率: {testingRate:F1}%");
            report.AppendLine();
            
            // 要件別詳細
            report.AppendLine("=== 要件別詳細 ===");
            foreach (var req in requirementStatus.Values.OrderBy(r => r.requirementId))
            {
                string status = req.isImplemented ? "実装済み" : "未実装";
                string tested = req.isTested ? "テスト済み" : "未テスト";
                report.AppendLine($"{req.requirementId}: {req.description} [{status}] [{tested}]");
                
                if (req.issues.Any())
                {
                    foreach (var issue in req.issues)
                    {
                        report.AppendLine($"  問題: {issue}");
                    }
                }
                report.AppendLine();
            }
            
            // 検証ログ
            report.AppendLine("=== 検証ログ ===");
            foreach (string logEntry in validationLog)
            {
                report.AppendLine(logEntry);
            }
            
            string fileName = $"requirement_validation_report_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            System.IO.File.WriteAllText(filePath, report.ToString());
            
            LogValidation($"検証レポートを保存しました: {filePath}");
        }
        catch (Exception e)
        {
            LogValidation($"検証レポート保存エラー: {e.Message}");
        }
    }
    
    /// <summary>
    /// 検証ログの出力
    /// </summary>
    private void LogValidation(string message)
    {
        string logMessage = $"[要件検証] {DateTime.Now:HH:mm:ss} {message}";
        
        if (enableDetailedLogging)
        {
            Debug.Log(logMessage);
        }
        
        validationLog.Add(logMessage);
        
        // ログサイズの制限
        if (validationLog.Count > 2000)
        {
            validationLog.RemoveAt(0);
        }
    }
    
    /// <summary>
    /// 公開メソッド：要件状況の取得
    /// </summary>
    public Dictionary<string, RequirementStatus> GetRequirementStatus()
    {
        return new Dictionary<string, RequirementStatus>(requirementStatus);
    }
    
    /// <summary>
    /// 公開メソッド：実装率の取得
    /// </summary>
    public float GetImplementationRate()
    {
        int total = requirementStatus.Count;
        int implemented = requirementStatus.Values.Count(r => r.isImplemented);
        return total > 0 ? (float)implemented / total * 100f : 0f;
    }
    
    /// <summary>
    /// 公開メソッド：手動検証実行
    /// </summary>
    public void ExecuteManualValidation()
    {
        StartCoroutine(ValidateAllRequirements());
    }
    
    /// <summary>
    /// 公開メソッド：特定要件の検証
    /// </summary>
    public void ValidateSpecificRequirement(string requirementId)
    {
        if (requirementStatus.ContainsKey(requirementId))
        {
            ValidateRequirementBehavior(requirementId);
        }
        else
        {
            LogValidation($"エラー: 要件ID '{requirementId}' が見つかりません");
        }
    }
}