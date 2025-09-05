using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Managers;
using DungeonOwner.Core;

namespace DungeonOwner.UI
{
    /// <summary>
    /// ボスキャラクター選択UIコンポーネント
    /// 5階層ごとのボス配置、選択式UI、配置確認を管理
    /// </summary>
    public class BossSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private Transform bossButtonContainer;
        [SerializeField] private GameObject bossButtonPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text titleText;
        [SerializeField] private Text floorInfoText;

        [Header("Boss Info Panel")]
        [SerializeField] private GameObject bossInfoPanel;
        [SerializeField] private Image bossIcon;
        [SerializeField] private Text bossNameText;
        [SerializeField] private Text bossDescriptionText;
        [SerializeField] private Text bossStatsText;
        [SerializeField] private Button confirmPlaceButton;
        [SerializeField] private Button cancelButton;

        [Header("Placement Settings")]
        [SerializeField] private LayerMask placementLayerMask = -1;
        [SerializeField] private GameObject placementGhost;

        // 内部状態
        private int currentFloorIndex = -1;
        private BossData selectedBossData = null;
        private Vector2 placementPosition = Vector2.zero;
        private bool isPlacementMode = false;
        private List<GameObject> bossButtons = new List<GameObject>();

        // イベント
        public System.Action<int, BossType> OnBossPlaced;
        public System.Action OnSelectionCancelled;

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }

        private void InitializeUI()
        {
            // 初期状態では非表示
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }

            if (bossInfoPanel != null)
            {
                bossInfoPanel.SetActive(false);
            }

            if (placementGhost != null)
            {
                placementGhost.SetActive(false);
            }
        }

        private void SetupEventListeners()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseSelection);
            }

            if (confirmPlaceButton != null)
            {
                confirmPlaceButton.onClick.AddListener(ConfirmPlacement);
            }

            if (cancelButton != null)
            {
                cancelButton.onClick.AddListener(CancelPlacement);
            }

            // BossManagerのイベントに登録
            if (BossManager.Instance != null)
            {
                BossManager.Instance.OnBossFloorAvailable += OnBossFloorAvailable;
            }
        }

        /// <summary>
        /// ボス選択UIを表示
        /// 要件6.1: 5階層ごとにボスキャラ配置オプションを表示
        /// </summary>
        public void ShowBossSelection(int floorIndex)
        {
            if (BossManager.Instance == null || !BossManager.Instance.IsBossFloor(floorIndex))
            {
                Debug.LogWarning($"Floor {floorIndex} is not a boss floor");
                return;
            }

            if (BossManager.Instance.HasBossOnFloor(floorIndex))
            {
                Debug.LogWarning($"Boss already exists on floor {floorIndex}");
                return;
            }

            currentFloorIndex = floorIndex;
            selectedBossData = null;
            isPlacementMode = false;

            // UIを表示
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(true);
            }

            // タイトルとフロア情報を更新
            UpdateFloorInfo();

            // 利用可能なボスリストを表示
            DisplayAvailableBosses();

            Debug.Log($"Showing boss selection for floor {floorIndex}");
        }

        /// <summary>
        /// フロア情報を更新
        /// </summary>
        private void UpdateFloorInfo()
        {
            if (titleText != null)
            {
                titleText.text = "ボスキャラクター選択";
            }

            if (floorInfoText != null)
            {
                floorInfoText.text = $"{currentFloorIndex}階層 - ボス配置";
            }
        }

        /// <summary>
        /// 利用可能なボスリストを表示
        /// 要件6.2: 選択式のボスキャラリストを表示
        /// </summary>
        private void DisplayAvailableBosses()
        {
            // 既存のボタンをクリア
            ClearBossButtons();

            if (BossManager.Instance == null || bossButtonContainer == null || bossButtonPrefab == null)
            {
                return;
            }

            // 利用可能なボスデータを取得
            List<BossData> availableBosses = BossManager.Instance.GetAvailableBossesForFloor(currentFloorIndex);

            foreach (BossData bossData in availableBosses)
            {
                CreateBossButton(bossData);
            }

            Debug.Log($"Displayed {availableBosses.Count} available bosses for floor {currentFloorIndex}");
        }

        /// <summary>
        /// ボス選択ボタンを作成
        /// </summary>
        private void CreateBossButton(BossData bossData)
        {
            GameObject buttonObj = Instantiate(bossButtonPrefab, bossButtonContainer);
            bossButtons.Add(buttonObj);

            // ボタンコンポーネントを設定
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SelectBoss(bossData));
            }

            // アイコンを設定
            Image iconImage = buttonObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null && bossData.icon != null)
            {
                iconImage.sprite = bossData.icon;
                iconImage.color = bossData.bossColor;
            }

            // 名前を設定
            Text nameText = buttonObj.transform.Find("Name")?.GetComponent<Text>();
            if (nameText != null)
            {
                nameText.text = bossData.displayName;
            }

            // レベル要件を設定
            Text requirementText = buttonObj.transform.Find("Requirement")?.GetComponent<Text>();
            if (requirementText != null)
            {
                requirementText.text = $"{bossData.minFloorLevel}階層以上";
            }
        }

        /// <summary>
        /// ボスを選択
        /// </summary>
        private void SelectBoss(BossData bossData)
        {
            selectedBossData = bossData;

            // ボス情報パネルを表示
            ShowBossInfo(bossData);

            Debug.Log($"Selected boss: {bossData.type}");
        }

        /// <summary>
        /// ボス情報を表示
        /// </summary>
        private void ShowBossInfo(BossData bossData)
        {
            if (bossInfoPanel == null) return;

            bossInfoPanel.SetActive(true);

            // アイコン
            if (bossIcon != null && bossData.icon != null)
            {
                bossIcon.sprite = bossData.icon;
                bossIcon.color = bossData.bossColor;
            }

            // 名前
            if (bossNameText != null)
            {
                bossNameText.text = bossData.displayName;
            }

            // 説明
            if (bossDescriptionText != null)
            {
                bossDescriptionText.text = bossData.description;
            }

            // ステータス
            if (bossStatsText != null)
            {
                int level = 1; // 初期レベル
                string stats = $"HP: {bossData.GetHealthAtLevel(level):F0}\n";
                stats += $"MP: {bossData.GetManaAtLevel(level):F0}\n";
                stats += $"攻撃力: {bossData.GetAttackPowerAtLevel(level):F0}\n";
                stats += $"リポップ時間: {bossData.respawnTime / 60f:F1}分\n";
                stats += $"報酬: {bossData.GetGoldReward(level)}金貨";

                bossStatsText.text = stats;
            }

            // 配置ボタンを有効化
            if (confirmPlaceButton != null)
            {
                confirmPlaceButton.interactable = true;
            }
        }

        /// <summary>
        /// 配置確認
        /// </summary>
        private void ConfirmPlacement()
        {
            if (selectedBossData == null)
            {
                Debug.LogWarning("No boss selected");
                return;
            }

            // 配置モードに移行
            StartPlacementMode();
        }

        /// <summary>
        /// 配置モードを開始
        /// </summary>
        private void StartPlacementMode()
        {
            isPlacementMode = true;

            // UIパネルを非表示
            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }

            if (bossInfoPanel != null)
            {
                bossInfoPanel.SetActive(false);
            }

            // 配置ゴーストを表示
            if (placementGhost != null)
            {
                placementGhost.SetActive(true);
            }

            // 指定階層に移動
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.ChangeViewFloor(currentFloorIndex);
            }

            Debug.Log($"Started placement mode for boss {selectedBossData.type} on floor {currentFloorIndex}");
        }

        /// <summary>
        /// 配置をキャンセル
        /// </summary>
        private void CancelPlacement()
        {
            if (isPlacementMode)
            {
                EndPlacementMode();
            }
            else
            {
                CloseSelection();
            }
        }

        /// <summary>
        /// 配置モードを終了
        /// </summary>
        private void EndPlacementMode()
        {
            isPlacementMode = false;

            // 配置ゴーストを非表示
            if (placementGhost != null)
            {
                placementGhost.SetActive(false);
            }

            // 選択UIに戻る
            ShowBossSelection(currentFloorIndex);
        }

        /// <summary>
        /// 選択を閉じる
        /// </summary>
        private void CloseSelection()
        {
            if (isPlacementMode)
            {
                EndPlacementMode();
            }

            if (selectionPanel != null)
            {
                selectionPanel.SetActive(false);
            }

            if (bossInfoPanel != null)
            {
                bossInfoPanel.SetActive(false);
            }

            currentFloorIndex = -1;
            selectedBossData = null;

            OnSelectionCancelled?.Invoke();

            Debug.Log("Boss selection closed");
        }

        /// <summary>
        /// ボスボタンをクリア
        /// </summary>
        private void ClearBossButtons()
        {
            foreach (GameObject button in bossButtons)
            {
                if (button != null)
                {
                    Destroy(button);
                }
            }
            bossButtons.Clear();
        }

        /// <summary>
        /// ボス階層が利用可能になった時の処理
        /// </summary>
        private void OnBossFloorAvailable(int floorIndex)
        {
            // 通知UIなどを表示する場合はここで実装
            Debug.Log($"Boss floor {floorIndex} is now available!");
        }

        private void Update()
        {
            if (isPlacementMode)
            {
                HandlePlacementInput();
            }
        }

        /// <summary>
        /// 配置モード中の入力処理
        /// </summary>
        private void HandlePlacementInput()
        {
            // マウス/タッチ位置を取得
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0;

            // 配置ゴーストの位置を更新
            if (placementGhost != null)
            {
                placementGhost.transform.position = worldPosition;
            }

            // クリック/タップで配置
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceBoss(worldPosition);
            }

            // 右クリック/ESCでキャンセル
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }

        /// <summary>
        /// ボス配置を試行
        /// </summary>
        private void TryPlaceBoss(Vector3 worldPosition)
        {
            if (selectedBossData == null || BossManager.Instance == null)
            {
                return;
            }

            Vector2 position = new Vector2(worldPosition.x, worldPosition.y);

            // 配置可能かチェック
            if (!BossManager.Instance.CanPlaceBoss(currentFloorIndex))
            {
                Debug.LogWarning("Cannot place boss on this floor");
                return;
            }

            // 位置の有効性をチェック（FloorSystemを使用）
            if (FloorSystem.Instance != null && !FloorSystem.Instance.CanPlaceMonster(currentFloorIndex, position))
            {
                Debug.LogWarning("Cannot place boss at this position");
                return;
            }

            // ボスを配置
            IBoss placedBoss = BossManager.Instance.PlaceBoss(currentFloorIndex, selectedBossData.type, position, 1);

            if (placedBoss != null)
            {
                // 配置成功
                OnBossPlaced?.Invoke(currentFloorIndex, selectedBossData.type);
                
                // 配置モードを終了
                isPlacementMode = false;
                
                // UIを閉じる
                CloseSelection();

                Debug.Log($"Successfully placed boss {selectedBossData.type} on floor {currentFloorIndex}");
            }
            else
            {
                Debug.LogError("Failed to place boss");
            }
        }

        /// <summary>
        /// 外部からボス選択UIを開く
        /// </summary>
        public void OpenBossSelectionForFloor(int floorIndex)
        {
            ShowBossSelection(floorIndex);
        }

        /// <summary>
        /// 現在選択中のボスタイプを取得
        /// </summary>
        public BossType? GetSelectedBossType()
        {
            return selectedBossData?.type;
        }

        /// <summary>
        /// 配置モード中かどうか
        /// </summary>
        public bool IsInPlacementMode()
        {
            return isPlacementMode;
        }

        private void OnDestroy()
        {
            // イベント登録解除
            if (BossManager.Instance != null)
            {
                BossManager.Instance.OnBossFloorAvailable -= OnBossFloorAvailable;
            }

            // ボタンをクリア
            ClearBossButtons();
        }
    }
}