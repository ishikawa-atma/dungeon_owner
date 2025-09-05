using UnityEngine;
using DungeonOwner.Managers;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core
{
    /// <summary>
    /// モンスター売却システムのテスト用クラス
    /// 要件5.1-5.4の動作確認を行う
    /// </summary>
    public class MonsterSellSystemTester : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private MonsterType testMonsterType = MonsterType.Slime;
        [SerializeField] private int testMonsterLevel = 1;
        
        private ShelterManager shelterManager;
        private ResourceManager resourceManager;
        private DataManager dataManager;
        
        private void Start()
        {
            InitializeManagers();
            
            if (runTestsOnStart)
            {
                StartCoroutine(RunTestsCoroutine());
            }
        }
        
        private void InitializeManagers()
        {
            shelterManager = FindObjectOfType<ShelterManager>();
            resourceManager = FindObjectOfType<ResourceManager>();
            dataManager = DataManager.Instance;
            
            if (shelterManager == null)
                Debug.LogError("ShelterManager not found!");
            
            if (resourceManager == null)
                Debug.LogError("ResourceManager not found!");
            
            if (dataManager == null)
                Debug.LogError("DataManager not found!");
        }
        
        private System.Collections.IEnumerator RunTestsCoroutine()
        {
            yield return new WaitForSeconds(1f);
            
            Debug.Log("=== モンスター売却システムテスト開始 ===");
            
            // テスト1: 売却価格計算テスト
            TestSellPriceCalculation();
            yield return new WaitForSeconds(0.5f);
            
            // テスト2: 売却可能性チェックテスト
            TestCanSellMonster();
            yield return new WaitForSeconds(0.5f);
            
            // テスト3: 実際の売却処理テスト
            yield return StartCoroutine(TestMonsterSelling());
            
            Debug.Log("=== モンスター売却システムテスト完了 ===");
        }
        
        /// <summary>
        /// 売却価格計算のテスト
        /// 要件5.2: 購入価格の一定割合での金貨返還
        /// </summary>
        private void TestSellPriceCalculation()
        {
            Debug.Log("--- 売却価格計算テスト ---");
            
            if (dataManager == null || resourceManager == null)
            {
                Debug.LogError("必要なマネージャーが見つかりません");
                return;
            }
            
            // 各モンスタータイプの売却価格をテスト
            MonsterType[] testTypes = { 
                MonsterType.Slime, 
                MonsterType.LesserSkeleton, 
                MonsterType.LesserGhost,
                MonsterType.LesserGolem,
                MonsterType.Goblin,
                MonsterType.LesserWolf
            };
            
            foreach (var type in testTypes)
            {
                var monsterData = dataManager.GetMonsterData(type);
                if (monsterData != null)
                {
                    int purchasePrice = monsterData.goldCost;
                    int sellPrice = monsterData.GetSellPrice();
                    float ratio = (float)sellPrice / purchasePrice;
                    
                    Debug.Log($"{type}: 購入価格 {purchasePrice}G → 売却価格 {sellPrice}G (比率: {ratio:P0})");
                    
                    // レベル補正のテスト
                    for (int level = 1; level <= 3; level++)
                    {
                        var mockMonster = CreateMockMonster(type, level);
                        int levelAdjustedPrice = resourceManager.CalculateMonsterSellPrice(mockMonster);
                        Debug.Log($"  Lv.{level}: {levelAdjustedPrice}G");
                    }
                }
            }
        }
        
        /// <summary>
        /// 売却可能性チェックのテスト
        /// 要件5.4: 階層配置中のモンスターは売却を無効化
        /// 要件5.5: 自キャラクターは売却を無効化
        /// </summary>
        private void TestCanSellMonster()
        {
            Debug.Log("--- 売却可能性チェックテスト ---");
            
            if (shelterManager == null)
            {
                Debug.LogError("ShelterManager not found!");
                return;
            }
            
            // テスト用モンスターを作成
            var testMonster = CreateMockMonster(testMonsterType, testMonsterLevel);
            
            // 退避スポットにいないモンスターは売却不可
            bool canSellNotInShelter = shelterManager.CanSellMonster(testMonster);
            Debug.Log($"退避スポットにいないモンスター売却可能: {canSellNotInShelter} (期待値: false)");
            
            // nullモンスターは売却不可
            bool canSellNull = shelterManager.CanSellMonster(null);
            Debug.Log($"nullモンスター売却可能: {canSellNull} (期待値: false)");
        }
        
        /// <summary>
        /// 実際の売却処理のテスト
        /// 要件5.1: 退避スポット内のモンスターを選択して売却
        /// 要件5.3: モンスターを完全に除去
        /// </summary>
        private System.Collections.IEnumerator TestMonsterSelling()
        {
            Debug.Log("--- 実際の売却処理テスト ---");
            
            if (shelterManager == null || resourceManager == null)
            {
                Debug.LogError("必要なマネージャーが見つかりません");
                yield break;
            }
            
            // 初期金貨を記録
            int initialGold = resourceManager.Gold;
            Debug.Log($"初期金貨: {initialGold}G");
            
            // テスト用モンスターを作成して退避スポットに配置
            var testMonster = CreateTestMonsterGameObject(testMonsterType, testMonsterLevel);
            if (testMonster != null)
            {
                bool sheltered = shelterManager.ShelterMonster(testMonster);
                Debug.Log($"モンスター退避: {sheltered}");
                
                if (sheltered)
                {
                    yield return new WaitForSeconds(0.1f);
                    
                    // 売却価格を計算
                    int expectedSellPrice = shelterManager.CalculateMonsterSellPrice(testMonster);
                    Debug.Log($"期待売却価格: {expectedSellPrice}G");
                    
                    // 売却実行
                    bool sold = shelterManager.SellMonster(testMonster);
                    Debug.Log($"売却成功: {sold}");
                    
                    if (sold)
                    {
                        yield return new WaitForSeconds(0.1f);
                        
                        // 金貨増加を確認
                        int finalGold = resourceManager.Gold;
                        int goldGained = finalGold - initialGold;
                        Debug.Log($"最終金貨: {finalGold}G (増加: {goldGained}G)");
                        Debug.Log($"期待増加量と一致: {goldGained == expectedSellPrice}");
                        
                        // 退避スポットから除去されたことを確認
                        bool stillInShelter = shelterManager.ShelterMonsters.Contains(testMonster);
                        Debug.Log($"退避スポットから除去: {!stillInShelter}");
                    }
                }
            }
        }
        
        /// <summary>
        /// モックモンスターを作成（インターフェースのみ）
        /// </summary>
        private IMonster CreateMockMonster(MonsterType type, int level)
        {
            return new MockMonster(type, level);
        }
        
        /// <summary>
        /// テスト用のモンスターGameObjectを作成
        /// </summary>
        private IMonster CreateTestMonsterGameObject(MonsterType type, int level)
        {
            if (dataManager == null) return null;
            
            var monsterData = dataManager.GetMonsterData(type);
            if (monsterData == null || monsterData.prefab == null)
            {
                Debug.LogWarning($"MonsterData or prefab not found for {type}");
                return null;
            }
            
            GameObject monsterObj = Instantiate(monsterData.prefab);
            var monster = monsterObj.GetComponent<IMonster>();
            
            if (monster != null)
            {
                monster.Level = level;
                return monster;
            }
            
            Destroy(monsterObj);
            return null;
        }
        
        /// <summary>
        /// デバッグ用メソッド
        /// </summary>
        [ContextMenu("Run Sell Price Test")]
        public void RunSellPriceTest()
        {
            InitializeManagers();
            TestSellPriceCalculation();
        }
        
        [ContextMenu("Run Can Sell Test")]
        public void RunCanSellTest()
        {
            InitializeManagers();
            TestCanSellMonster();
        }
        
        [ContextMenu("Print Shelter Status")]
        public void PrintShelterStatus()
        {
            if (shelterManager != null)
            {
                Debug.Log($"退避スポット状況: {shelterManager.CurrentCount}/{shelterManager.MaxCapacity}");
                foreach (var monster in shelterManager.ShelterMonsters)
                {
                    if (monster != null)
                    {
                        int sellPrice = shelterManager.CalculateMonsterSellPrice(monster);
                        Debug.Log($"- {monster.Type} Lv.{monster.Level} (売却価格: {sellPrice}G)");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// テスト用のモックモンスタークラス
    /// </summary>
    public class MockMonster : IMonster
    {
        public MonsterType Type { get; private set; }
        public int Level { get; set; }
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public float Mana { get; private set; }
        public float MaxMana { get; private set; }
        public Vector2 Position { get; set; }
        public MonsterState State { get; private set; }
        public IParty Party { get; set; }
        
        public MockMonster(MonsterType type, int level)
        {
            Type = type;
            Level = level;
            MaxHealth = 100f;
            Health = MaxHealth;
            MaxMana = 50f;
            Mana = MaxMana;
            State = MonsterState.Idle;
        }
        
        public void TakeDamage(float damage) { }
        public void Heal(float amount) { }
        public void UseAbility() { }
        public bool UseAbility(MonsterAbilityType abilityType) { return false; }
        public void JoinParty(IParty party) { }
        public void LeaveParty() { }
        public void SetState(MonsterState newState) { }
        public void ConsumeMana(float amount) { }
        public IMonsterAbility GetAbility(MonsterAbilityType abilityType) { return null; }
        public bool HasAbility(MonsterAbilityType abilityType) { return false; }
    }
}