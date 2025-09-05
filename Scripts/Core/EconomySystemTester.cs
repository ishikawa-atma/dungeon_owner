using UnityEngine;
using DungeonOwner.Managers;
using DungeonOwner.Core;
using DungeonOwner.Interfaces;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 経済システムのテスト用クラス
    /// ResourceManagerの各機能をテストし、動作確認を行う
    /// </summary>
    public class EconomySystemTester : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField] private bool runTestsOnStart = false;
        [SerializeField] private bool enableDebugUI = true;

        [Header("テスト用パラメータ")]
        [SerializeField] private int testGoldAmount = 100;
        [SerializeField] private int testSpendAmount = 50;
        [SerializeField] private InvaderType testInvaderType = InvaderType.Warrior;
        [SerializeField] private int testInvaderLevel = 3;
        [SerializeField] private MonsterType testMonsterType = MonsterType.Slime;
        [SerializeField] private int testMonsterLevel = 2;

        private void Start()
        {
            if (runTestsOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }

        private System.Collections.IEnumerator RunAllTests()
        {
            yield return new WaitForSeconds(1f); // 初期化待ち

            Debug.Log("=== 経済システムテスト開始 ===");

            TestBasicGoldOperations();
            yield return new WaitForSeconds(0.5f);

            TestInvaderRewardSystem();
            yield return new WaitForSeconds(0.5f);

            TestMonsterSellSystem();
            yield return new WaitForSeconds(0.5f);

            TestFloorExpansionCost();
            yield return new WaitForSeconds(0.5f);

            TestDailyRewardSystem();
            yield return new WaitForSeconds(0.5f);

            Debug.Log("=== 経済システムテスト完了 ===");
        }

        /// <summary>
        /// 基本的な金貨操作のテスト
        /// </summary>
        private void TestBasicGoldOperations()
        {
            Debug.Log("--- 基本金貨操作テスト ---");

            if (ResourceManager.Instance == null)
            {
                Debug.LogError("ResourceManager.Instance が null です");
                return;
            }

            var resourceManager = ResourceManager.Instance;
            int initialGold = resourceManager.Gold;

            // 金貨追加テスト
            resourceManager.AddGold(testGoldAmount);
            int afterAdd = resourceManager.Gold;
            Debug.Log($"金貨追加テスト: {initialGold} → {afterAdd} (期待値: {initialGold + testGoldAmount})");

            // 金貨消費テスト（成功）
            bool canAfford = resourceManager.CanAfford(testSpendAmount);
            bool spendSuccess = resourceManager.SpendGold(testSpendAmount);
            int afterSpend = resourceManager.Gold;
            Debug.Log($"金貨消費テスト: 購入可能={canAfford}, 消費成功={spendSuccess}, 残高={afterSpend}");

            // 金貨消費テスト（失敗）
            int impossibleAmount = resourceManager.Gold + 1000;
            bool canAffordImpossible = resourceManager.CanAfford(impossibleAmount);
            bool spendFail = resourceManager.SpendGold(impossibleAmount);
            Debug.Log($"過剰消費テスト: 購入可能={canAffordImpossible}, 消費成功={spendFail} (両方falseが期待値)");
        }

        /// <summary>
        /// 侵入者撃破報酬システムのテスト
        /// </summary>
        private void TestInvaderRewardSystem()
        {
            Debug.Log("--- 侵入者撃破報酬テスト ---");

            if (ResourceManager.Instance == null) return;

            var resourceManager = ResourceManager.Instance;
            int initialGold = resourceManager.Gold;

            // テスト用侵入者データを作成
            var testInvader = CreateTestInvader(testInvaderType, testInvaderLevel);

            // 撃破報酬処理
            resourceManager.ProcessInvaderDefeatReward(testInvader);
            int afterReward = resourceManager.Gold;

            Debug.Log($"侵入者撃破報酬テスト: {testInvaderType} Lv.{testInvaderLevel}");
            Debug.Log($"報酬前: {initialGold}, 報酬後: {afterReward}, 獲得金貨: {afterReward - initialGold}");

            // 異なるレベルでのテスト
            for (int level = 1; level <= 5; level++)
            {
                var invader = CreateTestInvader(testInvaderType, level);
                int goldBefore = resourceManager.Gold;
                resourceManager.ProcessInvaderDefeatReward(invader);
                int goldAfter = resourceManager.Gold;
                Debug.Log($"レベル {level} 報酬: {goldAfter - goldBefore} 金貨");
            }
        }

        /// <summary>
        /// モンスター売却システムのテスト
        /// </summary>
        private void TestMonsterSellSystem()
        {
            Debug.Log("--- モンスター売却システムテスト ---");

            if (ResourceManager.Instance == null) return;

            var resourceManager = ResourceManager.Instance;

            // テスト用モンスターデータを作成
            var testMonster = CreateTestMonster(testMonsterType, testMonsterLevel);

            // 売却価格計算テスト
            int sellPrice = resourceManager.CalculateMonsterSellPrice(testMonster);
            Debug.Log($"モンスター売却価格: {testMonsterType} Lv.{testMonsterLevel} = {sellPrice} 金貨");

            // 売却処理テスト
            int goldBefore = resourceManager.Gold;
            bool sellSuccess = resourceManager.SellMonster(testMonster);
            int goldAfter = resourceManager.Gold;

            Debug.Log($"売却テスト: 成功={sellSuccess}, 売却前={goldBefore}, 売却後={goldAfter}");

            // 異なるモンスタータイプでのテスト
            MonsterType[] monsterTypes = { MonsterType.Slime, MonsterType.LesserSkeleton, MonsterType.LesserGhost, MonsterType.Goblin };
            
            foreach (var monsterType in monsterTypes)
            {
                var monster = CreateTestMonster(monsterType, 1);
                int price = resourceManager.CalculateMonsterSellPrice(monster);
                Debug.Log($"{monsterType} Lv.1 売却価格: {price} 金貨");
            }
        }

        /// <summary>
        /// 階層拡張コストのテスト
        /// </summary>
        private void TestFloorExpansionCost()
        {
            Debug.Log("--- 階層拡張コストテスト ---");

            if (ResourceManager.Instance == null) return;

            var resourceManager = ResourceManager.Instance;

            // 各階層の拡張コストを計算
            for (int floor = 4; floor <= 10; floor++)
            {
                int cost = resourceManager.CalculateFloorExpansionCost(floor);
                Debug.Log($"階層 {floor} 拡張コスト: {cost} 金貨");
            }

            // 拡張処理テスト（実際には拡張しない）
            int testFloor = 4;
            int expansionCost = resourceManager.CalculateFloorExpansionCost(testFloor);
            bool canAfford = resourceManager.CanAfford(expansionCost);
            
            Debug.Log($"階層 {testFloor} 拡張テスト: コスト={expansionCost}, 購入可能={canAfford}");
        }

        /// <summary>
        /// 日次報酬システムのテスト
        /// </summary>
        private void TestDailyRewardSystem()
        {
            Debug.Log("--- 日次報酬システムテスト ---");

            if (ResourceManager.Instance == null) return;

            var resourceManager = ResourceManager.Instance;
            int goldBefore = resourceManager.Gold;

            // 強制日次報酬実行
            resourceManager.ForceDailyReward();
            int goldAfter = resourceManager.Gold;

            Debug.Log($"日次報酬テスト: 報酬前={goldBefore}, 報酬後={goldAfter}, 獲得={goldAfter - goldBefore}");

            // 日次報酬チェック
            resourceManager.CheckDailyReward();
            Debug.Log("日次報酬チェック完了");
        }

        /// <summary>
        /// テスト用侵入者を作成
        /// </summary>
        private IInvader CreateTestInvader(InvaderType type, int level)
        {
            return new TestInvader(type, level);
        }

        /// <summary>
        /// テスト用モンスターを作成
        /// </summary>
        private IMonster CreateTestMonster(MonsterType type, int level)
        {
            return new TestMonster(type, level);
        }

        private void OnGUI()
        {
            if (!enableDebugUI) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("=== 経済システムデバッグ ===");

            if (ResourceManager.Instance != null)
            {
                var resourceManager = ResourceManager.Instance;
                
                GUILayout.Label($"現在の金貨: {resourceManager.Gold}");
                GUILayout.Label($"最終日次報酬: {resourceManager.LastDailyReward:yyyy/MM/dd}");
                GUILayout.Label($"最終報酬日: {resourceManager.GetLastRewardDay()}");

                GUILayout.Space(10);

                if (GUILayout.Button("金貨 +100"))
                {
                    resourceManager.AddGold(100);
                }

                if (GUILayout.Button("金貨 -50"))
                {
                    resourceManager.SpendGold(50);
                }

                if (GUILayout.Button("強制日次報酬"))
                {
                    resourceManager.ForceDailyReward();
                }

                if (GUILayout.Button("侵入者撃破テスト"))
                {
                    var testInvader = CreateTestInvader(InvaderType.Warrior, 3);
                    resourceManager.ProcessInvaderDefeatReward(testInvader);
                }

                if (GUILayout.Button("モンスター売却テスト"))
                {
                    var testMonster = CreateTestMonster(MonsterType.Slime, 2);
                    resourceManager.SellMonster(testMonster);
                }

                if (GUILayout.Button("全テスト実行"))
                {
                    StartCoroutine(RunAllTests());
                }

                if (GUILayout.Button("リソース情報表示"))
                {
                    resourceManager.DebugPrintResourceInfo();
                }
            }
            else
            {
                GUILayout.Label("ResourceManager が見つかりません");
            }

            GUILayout.EndArea();
        }

        /// <summary>
        /// テスト用侵入者クラス
        /// </summary>
        private class TestInvader : IInvader
        {
            public InvaderType Type { get; private set; }
            public int Level { get; private set; }
            public float Health { get; private set; }
            public float MaxHealth { get; private set; }
            public UnityEngine.Vector2 Position { get; set; }
            public InvaderState State { get; private set; }
            public IParty Party { get; set; }

            public TestInvader(InvaderType type, int level)
            {
                Type = type;
                Level = level;
                MaxHealth = 100f;
                Health = MaxHealth;
                State = InvaderState.Moving;
            }

            public void TakeDamage(float damage)
            {
                Health = Mathf.Max(0, Health - damage);
            }

            public void Move(UnityEngine.Vector2 targetPosition)
            {
                Position = targetPosition;
            }

            public void JoinParty(IParty party)
            {
                Party = party;
            }

            public void LeaveParty()
            {
                Party = null;
            }
        }

        /// <summary>
        /// テスト用モンスタークラス
        /// </summary>
        private class TestMonster : IMonster
        {
            public MonsterType Type { get; private set; }
            public int Level { get; set; }
            public float Health { get; private set; }
            public float MaxHealth { get; private set; }
            public float Mana { get; private set; }
            public float MaxMana { get; private set; }
            public UnityEngine.Vector2 Position { get; set; }
            public MonsterAbility Ability { get; private set; }
            public IParty Party { get; set; }

            public TestMonster(MonsterType type, int level)
            {
                Type = type;
                Level = level;
                MaxHealth = 80f;
                Health = MaxHealth;
                MaxMana = 50f;
                Mana = MaxMana;
                Ability = MonsterAbility.None;
            }

            public void TakeDamage(float damage)
            {
                Health = Mathf.Max(0, Health - damage);
            }

            public void Heal(float amount)
            {
                Health = Mathf.Min(MaxHealth, Health + amount);
            }

            public void UseAbility()
            {
                // テスト用の空実装
            }

            public void JoinParty(IParty party)
            {
                Party = party;
            }

            public void LeaveParty()
            {
                Party = null;
            }
        }
    }
}