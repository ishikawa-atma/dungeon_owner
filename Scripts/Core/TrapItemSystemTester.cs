using UnityEngine;
using Scripts.Data.ScriptableObjects;
using Scripts.Data.Enums;
using Scripts.Managers;

namespace Scripts.Core
{
    /// <summary>
    /// 罠アイテムシステムのテスト用クラス
    /// 要件13.1-13.4の動作確認用
    /// </summary>
    public class TrapItemSystemTester : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField] private bool enableAutoTest = false;
        [SerializeField] private float testInterval = 5f;
        [SerializeField] private TrapItemData testTrapItem;
        
        [Header("デバッグ情報")]
        [SerializeField] private bool showDebugInfo = true;
        
        private float lastTestTime;
        
        private void Start()
        {
            Debug.Log("=== 罠アイテムシステムテスト開始 ===");
            
            // 基本機能テスト
            TestBasicFunctionality();
            
            // インベントリテスト
            TestInventorySystem();
        }
        
        private void Update()
        {
            if (enableAutoTest && Time.time - lastTestTime >= testInterval)
            {
                RunAutoTest();
                lastTestTime = Time.time;
            }
            
            // キーボードショートカット
            HandleKeyboardInput();
        }
        
        /// <summary>
        /// 基本機能テスト
        /// </summary>
        private void TestBasicFunctionality()
        {
            Debug.Log("--- 基本機能テスト ---");
            
            // InventoryManagerの存在確認
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager.Instance が見つかりません");
                return;
            }
            
            // TrapItemDropManagerの存在確認
            if (TrapItemDropManager.Instance == null)
            {
                Debug.LogError("TrapItemDropManager.Instance が見つかりません");
                return;
            }
            
            Debug.Log("✓ 基本コンポーネントが正常に初期化されています");
        }
        
        /// <summary>
        /// インベントリシステムテスト
        /// </summary>
        private void TestInventorySystem()
        {
            Debug.Log("--- インベントリシステムテスト ---");
            
            if (InventoryManager.Instance == null) return;
            
            // テスト用罠アイテムを取得
            TrapItemData testItem = GetTestTrapItem();
            if (testItem == null)
            {
                Debug.LogWarning("テスト用罠アイテムが見つかりません");
                return;
            }
            
            // アイテム追加テスト
            bool added = InventoryManager.Instance.AddItem(testItem, 3);
            Debug.Log($"✓ アイテム追加テスト: {(added ? "成功" : "失敗")}");
            
            // アイテム数確認
            int count = InventoryManager.Instance.GetItemCount(testItem.type);
            Debug.Log($"✓ アイテム数確認: {count}個");
            
            // インベントリ内容表示
            var inventory = InventoryManager.Instance.GetInventory();
            Debug.Log($"✓ インベントリ内容: {inventory.Count}種類のアイテム");
        }
        
        /// <summary>
        /// 自動テスト実行
        /// </summary>
        private void RunAutoTest()
        {
            Debug.Log("--- 自動テスト実行 ---");
            
            // ランダムな罠アイテムを追加
            TrapItemData randomItem = GetRandomTrapItem();
            if (randomItem != null && InventoryManager.Instance != null)
            {
                bool added = InventoryManager.Instance.AddItem(randomItem, 1);
                Debug.Log($"ランダムアイテム追加: {randomItem.itemName} - {(added ? "成功" : "失敗")}");
            }
            
            // ドロップ統計表示
            if (TrapItemDropManager.Instance != null)
            {
                int totalDrops = TrapItemDropManager.Instance.GetTotalDrops();
                Debug.Log($"総ドロップ数: {totalDrops}");
            }
        }
        
        /// <summary>
        /// キーボード入力処理
        /// </summary>
        private void HandleKeyboardInput()
        {
            // T キー: テスト用アイテム追加
            if (Input.GetKeyDown(KeyCode.T))
            {
                AddTestItem();
            }
            
            // U キー: テスト用アイテム使用
            if (Input.GetKeyDown(KeyCode.U))
            {
                UseTestItem();
            }
            
            // I キー: インベントリ情報表示
            if (Input.GetKeyDown(KeyCode.I))
            {
                ShowInventoryInfo();
            }
            
            // D キー: ドロップ統計表示
            if (Input.GetKeyDown(KeyCode.D))
            {
                ShowDropStatistics();
            }
        }
        
        /// <summary>
        /// テスト用アイテム追加
        /// </summary>
        [ContextMenu("テスト用アイテム追加")]
        public void AddTestItem()
        {
            TrapItemData testItem = GetTestTrapItem();
            if (testItem != null && InventoryManager.Instance != null)
            {
                bool added = InventoryManager.Instance.AddItem(testItem, 1);
                Debug.Log($"テストアイテム追加: {testItem.itemName} - {(added ? "成功" : "失敗")}");
            }
        }
        
        /// <summary>
        /// テスト用アイテム使用
        /// </summary>
        [ContextMenu("テスト用アイテム使用")]
        public void UseTestItem()
        {
            TrapItemData testItem = GetTestTrapItem();
            if (testItem != null && InventoryManager.Instance != null)
            {
                Vector2 testPosition = transform.position;
                bool used = InventoryManager.Instance.UseItem(testItem.type, testPosition);
                Debug.Log($"テストアイテム使用: {testItem.itemName} - {(used ? "成功" : "失敗")}");
            }
        }
        
        /// <summary>
        /// インベントリ情報表示
        /// </summary>
        [ContextMenu("インベントリ情報表示")]
        public void ShowInventoryInfo()
        {
            if (InventoryManager.Instance == null)
            {
                Debug.Log("InventoryManager が見つかりません");
                return;
            }
            
            var inventory = InventoryManager.Instance.GetInventory();
            Debug.Log($"=== インベントリ情報 ===");
            Debug.Log($"アイテム種類数: {inventory.Count}");
            
            foreach (var stack in inventory)
            {
                Debug.Log($"- {stack.itemData.itemName}: {stack.count}個");
            }
        }
        
        /// <summary>
        /// ドロップ統計表示
        /// </summary>
        [ContextMenu("ドロップ統計表示")]
        public void ShowDropStatistics()
        {
            if (TrapItemDropManager.Instance == null)
            {
                Debug.Log("TrapItemDropManager が見つかりません");
                return;
            }
            
            var statistics = TrapItemDropManager.Instance.GetDropStatistics();
            int totalDrops = TrapItemDropManager.Instance.GetTotalDrops();
            
            Debug.Log($"=== ドロップ統計 ===");
            Debug.Log($"総ドロップ数: {totalDrops}");
            
            foreach (var kvp in statistics)
            {
                if (kvp.Value > 0)
                {
                    Debug.Log($"- {kvp.Key}: {kvp.Value}回");
                }
            }
        }
        
        /// <summary>
        /// テスト用罠アイテムを取得
        /// </summary>
        private TrapItemData GetTestTrapItem()
        {
            if (testTrapItem != null)
            {
                return testTrapItem;
            }
            
            // DataManagerから取得を試行
            if (DataManager.Instance != null)
            {
                return DataManager.Instance.GetTrapItemData(TrapItemType.SpikeTrap);
            }
            
            // Resourcesから直接読み込み
            var trapItems = Resources.LoadAll<TrapItemData>("Data/TrapItems");
            return trapItems.Length > 0 ? trapItems[0] : null;
        }
        
        /// <summary>
        /// ランダムな罠アイテムを取得
        /// </summary>
        private TrapItemData GetRandomTrapItem()
        {
            if (DataManager.Instance != null)
            {
                return DataManager.Instance.GetRandomTrapItem();
            }
            
            var trapItems = Resources.LoadAll<TrapItemData>("Data/TrapItems");
            return trapItems.Length > 0 ? trapItems[Random.Range(0, trapItems.Length)] : null;
        }
        
        /// <summary>
        /// 模擬侵入者撃破テスト
        /// </summary>
        [ContextMenu("模擬侵入者撃破テスト")]
        public void SimulateInvaderDefeat()
        {
            Debug.Log("--- 模擬侵入者撃破テスト ---");
            
            if (TrapItemDropManager.Instance == null)
            {
                Debug.LogError("TrapItemDropManager が見つかりません");
                return;
            }
            
            // 模擬侵入者を作成（実際のIInvaderインターフェースの代わり）
            var mockInvader = new MockInvader
            {
                Type = InvaderType.Warrior,
                Level = Random.Range(1, 10),
                Position = transform.position
            };
            
            // ドロップ処理をテスト
            TrapItemDropManager.Instance.ProcessDrop(mockInvader);
            
            Debug.Log($"模擬侵入者撃破: {mockInvader.Type} Lv.{mockInvader.Level}");
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== 罠アイテムシステムテスト ===");
            
            if (GUILayout.Button("テストアイテム追加 (T)"))
            {
                AddTestItem();
            }
            
            if (GUILayout.Button("テストアイテム使用 (U)"))
            {
                UseTestItem();
            }
            
            if (GUILayout.Button("インベントリ情報 (I)"))
            {
                ShowInventoryInfo();
            }
            
            if (GUILayout.Button("ドロップ統計 (D)"))
            {
                ShowDropStatistics();
            }
            
            if (GUILayout.Button("模擬侵入者撃破"))
            {
                SimulateInvaderDefeat();
            }
            
            GUILayout.EndArea();
        }
    }
    
    /// <summary>
    /// テスト用の模擬侵入者クラス
    /// </summary>
    public class MockInvader : Scripts.Interfaces.IInvader
    {
        public InvaderType Type { get; set; }
        public int Level { get; set; }
        public float Health { get; set; } = 100f;
        public float MaxHealth { get; set; } = 100f;
        public Vector2 Position { get; set; }
        public InvaderState State { get; set; } = InvaderState.Moving;
        public Scripts.Interfaces.IParty Party { get; set; }
        
        public void TakeDamage(float damage)
        {
            Health = Mathf.Max(0, Health - damage);
        }
        
        public void Move(Vector2 targetPosition)
        {
            Position = targetPosition;
        }
        
        public void JoinParty(Scripts.Interfaces.IParty party)
        {
            Party = party;
        }
        
        public void LeaveParty()
        {
            Party = null;
        }
    }
}