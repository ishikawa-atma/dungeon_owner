using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DungeonOwner.Managers;

namespace DungeonOwner.UI
{
    /// <summary>
    /// 退避スポット管理UIクラス
    /// モンスターの退避、配置、売却のインターフェースを提供
    /// </summary>
    public class ShelterUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject shelterPanel;
        [SerializeField] private Transform monsterListParent;
        [SerializeField] private GameObject monsterItemPrefab;
        [SerializeField] private Button closeShelterButton;
        [SerializeField] private Text capacityText;
        
        [Header("モンスター操作UI")]
        [SerializeField] private GameObject monsterActionPanel;
        [SerializeField] private Button deployButton;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button cancelButton;
        
        [Header("階層選択UI")]
        [SerializeField] private GameObject floorSelectionPanel;
        [SerializeField] private Transform floorButtonParent;
        [SerializeField] private GameObject floorButtonPrefab;
        
        [Header("売却確認UI")]
        [SerializeField] private GameObject sellConfirmationPanel;
        [SerializeField] private Text sellConfirmationText;
        [SerializeField] private Text sellPriceText;
        [SerializeField] private Button confirmSellButton;
        [SerializeField] private Button cancelSellButton;
        
        private ShelterManager shelterManager;
        private FloorSystem floorSystem;
        private ResourceManager resourceManager;
        private IMonster selectedMonster;
        private List<GameObject> monsterItems = new List<GameObject>();
        private List<GameObject> floorButtons = new List<GameObject>();
        
        private void Awake()
        {
            shelterManager = FindObjectOfType<ShelterManager>();
            floorSystem = FindObjectOfType<FloorSystem>();
            resourceManager = FindObjectOfType<ResourceManager>();
            
            SetupUI();
        }
        
        private void OnEnable()
        {
            if (shelterManager != null)
            {
                shelterManager.OnMonsterSheltered += OnMonsterSheltered;
                shelterManager.OnMonsterDeployed += OnMonsterDeployed;
                shelterManager.OnMonsterSold += OnMonsterSold;
            }
        }
        
        private void OnDisable()
        {
            if (shelterManager != null)
            {
                shelterManager.OnMonsterSheltered -= OnMonsterSheltered;
                shelterManager.OnMonsterDeployed -= OnMonsterDeployed;
                shelterManager.OnMonsterSold -= OnMonsterSold;
            }
        }
        
        private void SetupUI()
        {
            if (closeShelterButton != null)
                closeShelterButton.onClick.AddListener(CloseShelter);
            
            if (deployButton != null)
                deployButton.onClick.AddListener(ShowFloorSelection);
            
            if (sellButton != null)
                sellButton.onClick.AddListener(SellSelectedMonster);
            
            if (cancelButton != null)
                cancelButton.onClick.AddListener(CancelSelection);
            
            if (confirmSellButton != null)
                confirmSellButton.onClick.AddListener(ConfirmSellMonster);
            
            if (cancelSellButton != null)
                cancelSellButton.onClick.AddListener(CancelSellConfirmation);
            
            // 初期状態では非表示
            if (shelterPanel != null)
                shelterPanel.SetActive(false);
            
            if (monsterActionPanel != null)
                monsterActionPanel.SetActive(false);
            
            if (floorSelectionPanel != null)
                floorSelectionPanel.SetActive(false);
            
            if (sellConfirmationPanel != null)
                sellConfirmationPanel.SetActive(false);
        }
        
        /// <summary>
        /// 退避スポットUIを表示
        /// 要件7.1: モンスター移動選択時に退避スポットを表示
        /// </summary>
        public void ShowShelter()
        {
            if (shelterPanel != null)
            {
                shelterPanel.SetActive(true);
                RefreshMonsterList();
                UpdateCapacityDisplay();
            }
        }
        
        /// <summary>
        /// 退避スポットUIを閉じる
        /// </summary>
        public void CloseShelter()
        {
            if (shelterPanel != null)
                shelterPanel.SetActive(false);
            
            CancelSelection();
        }
        
        /// <summary>
        /// モンスターリストを更新
        /// </summary>
        private void RefreshMonsterList()
        {
            // 既存のアイテムを削除
            foreach (var item in monsterItems)
            {
                if (item != null)
                    Destroy(item);
            }
            monsterItems.Clear();
            
            if (shelterManager == null || monsterItemPrefab == null || monsterListParent == null)
                return;
            
            // 退避スポット内のモンスターでアイテムを作成
            foreach (var monster in shelterManager.ShelterMonsters)
            {
                CreateMonsterItem(monster);
            }
        }
        
        /// <summary>
        /// モンスターアイテムUIを作成
        /// </summary>
        private void CreateMonsterItem(IMonster monster)
        {
            GameObject item = Instantiate(monsterItemPrefab, monsterListParent);
            monsterItems.Add(item);
            
            // モンスター情報を設定
            var monsterInfo = item.GetComponent<ShelterMonsterItem>();
            if (monsterInfo != null)
            {
                monsterInfo.SetMonster(monster);
                monsterInfo.OnMonsterSelected += OnMonsterSelected;
            }
            else
            {
                // フォールバック: 基本的なボタン設定
                var button = item.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => OnMonsterSelected(monster));
                }
                
                var text = item.GetComponentInChildren<Text>();
                if (text != null)
                {
                    text.text = $"{monster.Type} Lv.{monster.Level}";
                }
            }
        }
        
        /// <summary>
        /// モンスターが選択された時の処理
        /// 要件7.3: 退避スポット内モンスター選択時に配置可能階層を表示
        /// </summary>
        private void OnMonsterSelected(IMonster monster)
        {
            selectedMonster = monster;
            
            if (monsterActionPanel != null)
            {
                monsterActionPanel.SetActive(true);
            }
            
            Debug.Log($"モンスター {monster.Type} が選択されました");
        }
        
        /// <summary>
        /// 階層選択UIを表示
        /// </summary>
        private void ShowFloorSelection()
        {
            if (selectedMonster == null || floorSelectionPanel == null)
                return;
            
            floorSelectionPanel.SetActive(true);
            RefreshFloorButtons();
        }
        
        /// <summary>
        /// 階層ボタンを更新
        /// </summary>
        private void RefreshFloorButtons()
        {
            // 既存のボタンを削除
            foreach (var button in floorButtons)
            {
                if (button != null)
                    Destroy(button);
            }
            floorButtons.Clear();
            
            if (shelterManager == null || floorButtonPrefab == null || floorButtonParent == null)
                return;
            
            // 配置可能な階層のボタンを作成
            var availableFloors = shelterManager.GetAvailableFloors();
            foreach (int floorIndex in availableFloors)
            {
                CreateFloorButton(floorIndex);
            }
        }
        
        /// <summary>
        /// 階層ボタンを作成
        /// </summary>
        private void CreateFloorButton(int floorIndex)
        {
            GameObject buttonObj = Instantiate(floorButtonPrefab, floorButtonParent);
            floorButtons.Add(buttonObj);
            
            var button = buttonObj.GetComponent<Button>();
            var text = buttonObj.GetComponentInChildren<Text>();
            
            if (text != null)
            {
                text.text = $"階層 {floorIndex + 1}";
            }
            
            if (button != null)
            {
                button.onClick.AddListener(() => SelectFloor(floorIndex));
            }
        }
        
        /// <summary>
        /// 階層を選択してモンスターを配置
        /// </summary>
        private void SelectFloor(int floorIndex)
        {
            if (selectedMonster == null || shelterManager == null)
                return;
            
            // 配置可能な位置を取得
            var availablePositions = shelterManager.GetAvailablePositions(floorIndex);
            if (availablePositions.Count == 0)
            {
                Debug.LogWarning($"階層 {floorIndex} に配置可能な位置がありません");
                return;
            }
            
            // 最初の利用可能な位置に配置
            Vector2 position = availablePositions[0];
            
            if (shelterManager.DeployMonster(selectedMonster, floorIndex, position))
            {
                Debug.Log($"モンスター {selectedMonster.Type} を階層 {floorIndex} に配置しました");
                CancelSelection();
                RefreshMonsterList();
            }
        }
        
        /// <summary>
        /// 選択されたモンスターの売却確認を表示
        /// 要件5.1: 退避スポット内のモンスターを選択して売却オプションを表示
        /// 要件5.3: 売却確認UIと安全機能
        /// </summary>
        private void SellSelectedMonster()
        {
            if (selectedMonster == null || shelterManager == null)
                return;
            
            // 売却可能かチェック
            if (!shelterManager.CanSellMonster(selectedMonster))
            {
                Debug.LogWarning($"モンスター {selectedMonster.Type} は売却できません");
                return;
            }
            
            ShowSellConfirmation();
        }
        
        /// <summary>
        /// 売却確認ダイアログを表示
        /// 要件5.3: 売却確認UIと安全機能
        /// </summary>
        private void ShowSellConfirmation()
        {
            if (selectedMonster == null || sellConfirmationPanel == null)
                return;
            
            // 売却価格を計算
            int sellPrice = shelterManager.CalculateMonsterSellPrice(selectedMonster);
            
            // 確認テキストを設定
            if (sellConfirmationText != null)
            {
                sellConfirmationText.text = $"{selectedMonster.Type} Lv.{selectedMonster.Level}\nを売却しますか？";
            }
            
            if (sellPriceText != null)
            {
                sellPriceText.text = $"売却価格: {sellPrice} 金貨";
            }
            
            // 確認パネルを表示
            sellConfirmationPanel.SetActive(true);
        }
        
        /// <summary>
        /// 売却を確定
        /// 要件5.2: 購入価格の一定割合を金貨で返還
        /// 要件5.3: モンスターを完全に除去
        /// </summary>
        private void ConfirmSellMonster()
        {
            if (selectedMonster == null || shelterManager == null)
                return;
            
            if (shelterManager.SellMonster(selectedMonster))
            {
                Debug.Log($"モンスター {selectedMonster.Type} を売却しました");
                CancelSellConfirmation();
                CancelSelection();
                RefreshMonsterList();
                UpdateCapacityDisplay();
            }
        }
        
        /// <summary>
        /// 売却確認をキャンセル
        /// </summary>
        private void CancelSellConfirmation()
        {
            if (sellConfirmationPanel != null)
                sellConfirmationPanel.SetActive(false);
        }
        
        /// <summary>
        /// 選択をキャンセル
        /// </summary>
        private void CancelSelection()
        {
            selectedMonster = null;
            
            if (monsterActionPanel != null)
                monsterActionPanel.SetActive(false);
            
            if (floorSelectionPanel != null)
                floorSelectionPanel.SetActive(false);
            
            if (sellConfirmationPanel != null)
                sellConfirmationPanel.SetActive(false);
        }
        
        /// <summary>
        /// 収容数表示を更新
        /// </summary>
        private void UpdateCapacityDisplay()
        {
            if (capacityText != null && shelterManager != null)
            {
                capacityText.text = $"{shelterManager.CurrentCount} / {shelterManager.MaxCapacity}";
            }
        }
        
        // イベントハンドラー
        private void OnMonsterSheltered(IMonster monster)
        {
            RefreshMonsterList();
            UpdateCapacityDisplay();
        }
        
        private void OnMonsterDeployed(IMonster monster)
        {
            RefreshMonsterList();
            UpdateCapacityDisplay();
        }
        
        private void OnMonsterSold(IMonster monster)
        {
            RefreshMonsterList();
            UpdateCapacityDisplay();
        }
    }
}