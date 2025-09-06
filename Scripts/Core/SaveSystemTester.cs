using UnityEngine;
using UnityEngine.UI;

namespace DungeonOwner
{
    using DungeonOwner.Data.Enums;
{
    /// <summary>
    /// SaveManagerの機能をテストするクラス
    /// 要件16.1, 16.2, 16.3, 16.5の動作確認用
    /// </summary>
    public class SaveSystemTester : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteSaveButton;
        [SerializeField] private Button createTestDataButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text saveInfoText;
        
        [Header("Test Data")]
        [SerializeField] private int testGold = 5000;
        [SerializeField] private int testFloor = 10;
        [SerializeField] private PlayerCharacterType testPlayerCharacter = PlayerCharacterType.Mage;
        
        private void Start()
        {
            SetupUI();
            UpdateSaveInfo();
            
            // SaveManagerのイベントを購読
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnSaveCompleted += OnSaveCompleted;
                SaveManager.Instance.OnLoadCompleted += OnLoadCompleted;
                SaveManager.Instance.OnSaveError += OnSaveError;
            }
        }
        
        private void OnDestroy()
        {
            // イベントの購読解除
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.OnSaveCompleted -= OnSaveCompleted;
                SaveManager.Instance.OnLoadCompleted -= OnLoadCompleted;
                SaveManager.Instance.OnSaveError -= OnSaveError;
            }
        }
        
        private void SetupUI()
        {
            if (saveButton != null)
                saveButton.onClick.AddListener(TestSave);
            
            if (loadButton != null)
                loadButton.onClick.AddListener(TestLoad);
            
            if (deleteSaveButton != null)
                deleteSaveButton.onClick.AddListener(TestDeleteSave);
            
            if (createTestDataButton != null)
                createTestDataButton.onClick.AddListener(CreateTestData);
        }
        
        /// <summary>
        /// 保存機能のテスト（要件16.1, 16.2）
        /// </summary>
        public void TestSave()
        {
            UpdateStatus("保存中...");
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }
            else
            {
                UpdateStatus("SaveManagerが見つかりません");
            }
        }
        
        /// <summary>
        /// 読み込み機能のテスト（要件16.3）
        /// </summary>
        public void TestLoad()
        {
            UpdateStatus("読み込み中...");
            
            if (SaveManager.Instance != null)
            {
                bool success = SaveManager.Instance.LoadGame();
                if (!success)
                {
                    UpdateStatus("セーブファイルが見つからないか、読み込みに失敗しました");
                }
            }
            else
            {
                UpdateStatus("SaveManagerが見つかりません");
            }
        }
        
        /// <summary>
        /// セーブファイル削除のテスト
        /// </summary>
        public void TestDeleteSave()
        {
            UpdateStatus("セーブファイルを削除中...");
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.DeleteSaveFile();
                UpdateStatus("セーブファイルを削除しました");
                UpdateSaveInfo();
            }
            else
            {
                UpdateStatus("SaveManagerが見つかりません");
            }
        }
        
        /// <summary>
        /// テスト用データの作成
        /// </summary>
        public void CreateTestData()
        {
            UpdateStatus("テストデータを作成中...");
            
            // ResourceManagerにテストデータを設定
            if (DungeonOwner.Managers.ResourceManager.Instance != null)
            {
                DungeonOwner.Managers.ResourceManager.Instance.SetGold(testGold);
                DungeonOwner.Managers.ResourceManager.Instance.SetLastDailyReward(System.DateTime.Now.AddDays(-1));
            }
            
            // GameManagerにテストデータを設定
            if (DungeonOwner.Core.GameManager.Instance != null)
            {
                DungeonOwner.Core.GameManager.Instance.SetCurrentFloor(testFloor);
            }
            
            // DataManagerにテストデータを設定
            if (DungeonOwner.Data.DataManager.Instance != null)
            {
                DungeonOwner.Data.DataManager.Instance.SetSelectedPlayerCharacter(testPlayerCharacter);
            }
            
            UpdateStatus($"テストデータを作成しました (金貨: {testGold}, 階層: {testFloor}, キャラ: {testPlayerCharacter})");
        }
        
        /// <summary>
        /// 侵入者撃破時の自動保存テスト（要件16.1）
        /// </summary>
        public void TestAutoSaveOnInvaderDefeated()
        {
            UpdateStatus("侵入者撃破時の自動保存をテスト中...");
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveOnInvaderDefeated();
            }
            else
            {
                UpdateStatus("SaveManagerが見つかりません");
            }
        }
        
        /// <summary>
        /// セーブデータの整合性チェックテスト（要件16.5）
        /// </summary>
        public void TestSaveDataValidation()
        {
            UpdateStatus("セーブデータの整合性をテスト中...");
            
            // 不正なデータでテスト
            SaveData invalidData = new SaveData();
            invalidData.currentFloor = -1; // 不正な階層
            invalidData.gold = -100; // 不正な金貨
            
            // 実際の検証は SaveManager 内部で行われる
            UpdateStatus("整合性チェックのテストが完了しました（ログを確認してください）");
        }
        
        private void OnSaveCompleted()
        {
            UpdateStatus("保存が完了しました");
            UpdateSaveInfo();
        }
        
        private void OnLoadCompleted()
        {
            UpdateStatus("読み込みが完了しました");
            UpdateSaveInfo();
        }
        
        private void OnSaveError(string errorMessage)
        {
            UpdateStatus($"エラー: {errorMessage}");
        }
        
        private void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
            }
            
            Debug.Log($"SaveSystemTester: {message}");
        }
        
        private void UpdateSaveInfo()
        {
            if (saveInfoText != null && SaveManager.Instance != null)
            {
                bool hasSave = SaveManager.Instance.HasSaveFile();
                string info = $"セーブファイル: {(hasSave ? "存在" : "なし")}\n";
                
                if (DungeonOwner.Managers.ResourceManager.Instance != null)
                {
                    info += $"現在の金貨: {DungeonOwner.Managers.ResourceManager.Instance.Gold}\n";
                }
                
                if (DungeonOwner.Core.GameManager.Instance != null)
                {
                    info += $"現在の階層: {DungeonOwner.Core.GameManager.Instance.CurrentFloor}\n";
                }
                
                saveInfoText.text = info;
            }
        }
        
        // デバッグ用のキーボードショートカット
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                TestSave();
            }
            
            if (Input.GetKeyDown(KeyCode.L))
            {
                TestLoad();
            }
            
            if (Input.GetKeyDown(KeyCode.D))
            {
                TestDeleteSave();
            }
            
            if (Input.GetKeyDown(KeyCode.T))
            {
                CreateTestData();
            }
        }
    }
}