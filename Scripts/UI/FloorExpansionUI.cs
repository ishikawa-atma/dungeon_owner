using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Core;
using DungeonOwner.Managers;
using DungeonOwner.Data;
using System.Collections.Generic;
using System.Linq;

namespace DungeonOwner.UI
{
    /// <summary>
    /// 階層拡張UI
    /// 階層拡張ボタン、コスト表示、新モンスター解放通知を管理
    /// </summary>
    public class FloorExpansionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button expandButton;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI currentFloorText;
        [SerializeField] private TextMeshProUGUI maxFloorText;
        [SerializeField] private GameObject expansionPanel;

        [Header("New Monster Unlock UI")]
        [SerializeField] private GameObject unlockNotificationPanel;
        [SerializeField] private TextMeshProUGUI unlockTitleText;
        [SerializeField] private TextMeshProUGUI unlockDescriptionText;
        [SerializeField] private Transform unlockMonsterContainer;
        [SerializeField] private GameObject monsterIconPrefab;
        [SerializeField] private Button unlockConfirmButton;

        [Header("Preview UI")]
        [SerializeField] private GameObject previewPanel;
        [SerializeField] private TextMeshProUGUI previewFloorText;
        [SerializeField] private TextMeshProUGUI previewCostText;
        [SerializeField] private TextMeshProUGUI previewUnlockText;
        [SerializeField] private Button previewConfirmButton;
        [SerializeField] private Button previewCancelButton;

        private bool isInitialized = false;

        private void Start()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            if (isInitialized) return;

            // ボタンイベント設定
            if (expandButton != null)
            {
                expandButton.onClick.AddListener(OnExpandButtonClicked);
            }

            if (previewConfirmButton != null)
            {
                previewConfirmButton.onClick.AddListener(OnPreviewConfirmClicked);
            }

            if (previewCancelButton != null)
            {
                previewCancelButton.onClick.AddListener(OnPreviewCancelClicked);
            }

            if (unlockConfirmButton != null)
            {
                unlockConfirmButton.onClick.AddListener(OnUnlockConfirmClicked);
            }

            // システムイベント購読
            if (FloorExpansionSystem.Instance != null)
            {
                FloorExpansionSystem.Instance.OnFloorExpanded += OnFloorExpanded;
                FloorExpansionSystem.Instance.OnNewMonstersUnlocked += OnNewMonstersUnlocked;
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorExpanded += OnFloorSystemExpanded;
            }

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnGoldChanged += OnGoldChanged;
            }

            // 初期パネル状態設定
            if (expansionPanel != null)
            {
                expansionPanel.SetActive(true);
            }

            if (previewPanel != null)
            {
                previewPanel.SetActive(false);
            }

            if (unlockNotificationPanel != null)
            {
                unlockNotificationPanel.SetActive(false);
            }

            UpdateUI();
            isInitialized = true;

            Debug.Log("FloorExpansionUI initialized");
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (FloorExpansionSystem.Instance != null)
            {
                FloorExpansionSystem.Instance.OnFloorExpanded -= OnFloorExpanded;
                FloorExpansionSystem.Instance.OnNewMonstersUnlocked -= OnNewMonstersUnlocked;
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorExpanded -= OnFloorSystemExpanded;
            }

            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.OnGoldChanged -= OnGoldChanged;
            }
        }

        /// <summary>
        /// UI更新
        /// </summary>
        private void UpdateUI()
        {
            if (!isInitialized) return;

            UpdateFloorInfo();
            UpdateExpansionButton();
        }

        /// <summary>
        /// 階層情報更新
        /// </summary>
        private void UpdateFloorInfo()
        {
            if (FloorSystem.Instance == null) return;

            int currentFloors = FloorSystem.Instance.CurrentFloorCount;
            int maxFloors = FloorSystem.Instance.MaxFloors;

            if (currentFloorText != null)
            {
                currentFloorText.text = $"現在の階層: {currentFloors}";
            }

            if (maxFloorText != null)
            {
                maxFloorText.text = $"最大階層: {maxFloors}";
            }
        }

        /// <summary>
        /// 拡張ボタン更新
        /// </summary>
        private void UpdateExpansionButton()
        {
            if (expandButton == null || FloorExpansionSystem.Instance == null) return;

            bool canExpand = FloorExpansionSystem.Instance.CanExpandFloor();
            expandButton.interactable = canExpand;

            if (canExpand && FloorSystem.Instance != null)
            {
                int nextFloor = FloorSystem.Instance.CurrentFloorCount + 1;
                int cost = FloorExpansionSystem.Instance.CalculateExpansionCost(nextFloor);

                if (costText != null)
                {
                    costText.text = $"拡張コスト: {cost} G";
                }

                // 金貨不足の場合はボタンを無効化
                if (ResourceManager.Instance != null && !ResourceManager.Instance.CanAfford(cost))
                {
                    expandButton.interactable = false;
                    if (costText != null)
                    {
                        costText.text = $"拡張コスト: {cost} G (不足)";
                        costText.color = Color.red;
                    }
                }
                else if (costText != null)
                {
                    costText.color = Color.white;
                }
            }
            else
            {
                if (costText != null)
                {
                    costText.text = "拡張不可";
                    costText.color = Color.gray;
                }
            }
        }

        /// <summary>
        /// 拡張ボタンクリック処理
        /// </summary>
        private void OnExpandButtonClicked()
        {
            if (FloorSystem.Instance == null || FloorExpansionSystem.Instance == null) return;

            int nextFloor = FloorSystem.Instance.CurrentFloorCount + 1;
            ShowExpansionPreview(nextFloor);
        }

        /// <summary>
        /// 拡張プレビュー表示
        /// </summary>
        private void ShowExpansionPreview(int targetFloor)
        {
            if (previewPanel == null || FloorExpansionSystem.Instance == null) return;

            int cost = FloorExpansionSystem.Instance.CalculateExpansionCost(targetFloor);
            var unlockMonsters = FloorExpansionSystem.Instance.GetUnlockedMonstersAtFloor(targetFloor);

            if (previewFloorText != null)
            {
                previewFloorText.text = $"階層 {targetFloor} に拡張";
            }

            if (previewCostText != null)
            {
                previewCostText.text = $"コスト: {cost} G";
            }

            if (previewUnlockText != null)
            {
                if (unlockMonsters.Count > 0)
                {
                    string monsterNames = string.Join(", ", unlockMonsters.Select(m => GetMonsterDisplayName(m)));
                    previewUnlockText.text = $"解放モンスター: {monsterNames}";
                }
                else
                {
                    previewUnlockText.text = "新規解放モンスターなし";
                }
            }

            previewPanel.SetActive(true);
            expansionPanel.SetActive(false);
        }

        /// <summary>
        /// プレビュー確認ボタンクリック処理
        /// </summary>
        private void OnPreviewConfirmClicked()
        {
            if (FloorExpansionSystem.Instance != null)
            {
                bool success = FloorExpansionSystem.Instance.TryExpandFloor();
                if (success)
                {
                    Debug.Log("Floor expansion successful");
                }
                else
                {
                    Debug.Log("Floor expansion failed");
                }
            }

            HidePreview();
        }

        /// <summary>
        /// プレビューキャンセルボタンクリック処理
        /// </summary>
        private void OnPreviewCancelClicked()
        {
            HidePreview();
        }

        /// <summary>
        /// プレビューパネルを非表示
        /// </summary>
        private void HidePreview()
        {
            if (previewPanel != null)
            {
                previewPanel.SetActive(false);
            }

            if (expansionPanel != null)
            {
                expansionPanel.SetActive(true);
            }
        }

        /// <summary>
        /// 階層拡張完了イベント処理
        /// </summary>
        private void OnFloorExpanded(int newFloor)
        {
            UpdateUI();
            Debug.Log($"UI updated for new floor: {newFloor}");
        }

        /// <summary>
        /// FloorSystemの階層拡張イベント処理
        /// </summary>
        private void OnFloorSystemExpanded(int newFloor)
        {
            UpdateUI();
        }

        /// <summary>
        /// 金貨変更イベント処理
        /// </summary>
        private void OnGoldChanged(int newGold)
        {
            UpdateExpansionButton();
        }

        /// <summary>
        /// 新モンスター解放イベント処理
        /// </summary>
        private void OnNewMonstersUnlocked(List<MonsterType> unlockedMonsters)
        {
            ShowUnlockNotification(unlockedMonsters);
        }

        /// <summary>
        /// モンスター解放通知表示
        /// </summary>
        private void ShowUnlockNotification(List<MonsterType> unlockedMonsters)
        {
            if (unlockNotificationPanel == null || unlockedMonsters.Count == 0) return;

            if (unlockTitleText != null)
            {
                unlockTitleText.text = "新モンスター解放！";
            }

            if (unlockDescriptionText != null)
            {
                string monsterNames = string.Join(", ", unlockedMonsters.Select(m => GetMonsterDisplayName(m)));
                unlockDescriptionText.text = $"以下のモンスターが購入可能になりました:\n{monsterNames}";
            }

            // モンスターアイコン表示
            DisplayUnlockedMonsterIcons(unlockedMonsters);

            unlockNotificationPanel.SetActive(true);
        }

        /// <summary>
        /// 解放モンスターアイコン表示
        /// </summary>
        private void DisplayUnlockedMonsterIcons(List<MonsterType> monsters)
        {
            if (unlockMonsterContainer == null || monsterIconPrefab == null) return;

            // 既存のアイコンをクリア
            foreach (Transform child in unlockMonsterContainer)
            {
                Destroy(child.gameObject);
            }

            // 新しいアイコンを生成
            foreach (var monsterType in monsters)
            {
                GameObject iconObj = Instantiate(monsterIconPrefab, unlockMonsterContainer);
                
                // MonsterDataからアイコンを取得して設定
                if (DataManager.Instance != null)
                {
                    var monsterData = DataManager.Instance.GetMonsterData(monsterType);
                    if (monsterData != null && monsterData.icon != null)
                    {
                        Image iconImage = iconObj.GetComponent<Image>();
                        if (iconImage != null)
                        {
                            iconImage.sprite = monsterData.icon;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 解放通知確認ボタンクリック処理
        /// </summary>
        private void OnUnlockConfirmClicked()
        {
            if (unlockNotificationPanel != null)
            {
                unlockNotificationPanel.SetActive(false);
            }
        }

        /// <summary>
        /// モンスター表示名取得
        /// </summary>
        private string GetMonsterDisplayName(MonsterType type)
        {
            if (DataManager.Instance != null)
            {
                var monsterData = DataManager.Instance.GetMonsterData(type);
                if (monsterData != null && !string.IsNullOrEmpty(monsterData.displayName))
                {
                    return monsterData.displayName;
                }
            }

            // フォールバック
            return type.ToString();
        }

        /// <summary>
        /// 外部からUI更新を強制
        /// </summary>
        public void ForceUpdateUI()
        {
            UpdateUI();
        }

        /// <summary>
        /// パネル表示状態を設定
        /// </summary>
        public void SetPanelActive(bool active)
        {
            if (expansionPanel != null)
            {
                expansionPanel.SetActive(active);
            }
        }
    }
}