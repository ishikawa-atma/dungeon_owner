using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DungeonOwner.Data;
using DungeonOwner.Managers;
using DungeonOwner.Core;

namespace DungeonOwner.UI
{
    public class MonsterPlacementUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject monsterSelectionPanel;
        [SerializeField] private Transform monsterButtonContainer;
        [SerializeField] private Button monsterButtonPrefab;
        [SerializeField] private Button placementModeButton;
        [SerializeField] private Button cancelPlacementButton;
        [SerializeField] private Text floorCapacityText;
        [SerializeField] private Text goldText;

        [Header("Placement")]
        [SerializeField] private GameObject placementGhostPrefab;
        [SerializeField] private LayerMask placementLayerMask = -1;

        // 状態管理
        private bool isPlacementMode = false;
        private MonsterType selectedMonsterType;
        private MonsterData selectedMonsterData;
        private GameObject placementGhost;
        private Camera mainCamera;
        private List<Button> monsterButtons = new List<Button>();

        // イベント
        public System.Action<MonsterType, Vector2> OnMonsterPlaceRequested;
        public System.Action OnPlacementModeChanged;

        private void Awake()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
            UpdateUI();
        }

        private void Update()
        {
            if (isPlacementMode)
            {
                HandlePlacementInput();
                UpdatePlacementGhost();
            }
        }

        private void InitializeUI()
        {
            // 初期状態では配置モードは無効
            SetPlacementMode(false);
            
            // モンスター選択パネルを非表示
            if (monsterSelectionPanel != null)
            {
                monsterSelectionPanel.SetActive(false);
            }

            CreateMonsterButtons();
        }

        private void SetupEventListeners()
        {
            // 配置モードボタン
            if (placementModeButton != null)
            {
                placementModeButton.onClick.AddListener(ToggleMonsterSelection);
            }

            // キャンセルボタン
            if (cancelPlacementButton != null)
            {
                cancelPlacementButton.onClick.AddListener(CancelPlacement);
            }

            // MonsterPlacementManagerのイベント
            if (MonsterPlacementManager.Instance != null)
            {
                MonsterPlacementManager.Instance.OnFloorMonsterCountChanged += OnFloorMonsterCountChanged;
            }

            // FloorSystemのイベント
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged += OnFloorChanged;
            }

            // FloorExpansionSystemのイベント
            if (FloorExpansionSystem.Instance != null)
            {
                FloorExpansionSystem.Instance.OnNewMonstersUnlocked += OnNewMonstersUnlocked;
            }
        }

        private void CreateMonsterButtons()
        {
            if (monsterButtonContainer == null || monsterButtonPrefab == null)
            {
                Debug.LogWarning("Monster button container or prefab not assigned");
                return;
            }

            // 既存のボタンをクリア
            foreach (var button in monsterButtons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }
            monsterButtons.Clear();

            // 階層拡張システムから利用可能なモンスターを取得
            List<MonsterType> availableMonsterTypes;
            if (FloorExpansionSystem.Instance != null)
            {
                availableMonsterTypes = FloorExpansionSystem.Instance.GetAvailableMonsters();
            }
            else
            {
                // フォールバック: MonsterPlacementManagerから取得
                var unlockedMonsters = MonsterPlacementManager.Instance?.GetUnlockedMonsters();
                availableMonsterTypes = new List<MonsterType>();
                if (unlockedMonsters != null)
                {
                    foreach (var monsterData in unlockedMonsters)
                    {
                        availableMonsterTypes.Add(monsterData.type);
                    }
                }
            }

            // DataManagerからMonsterDataを取得してボタンを作成
            if (DataManager.Instance != null)
            {
                foreach (var monsterType in availableMonsterTypes)
                {
                    var monsterData = DataManager.Instance.GetMonsterData(monsterType);
                    if (monsterData != null)
                    {
                        CreateMonsterButton(monsterData);
                    }
                }
            }
        }

        private void CreateMonsterButton(MonsterData monsterData)
        {
            Button button = Instantiate(monsterButtonPrefab, monsterButtonContainer);
            monsterButtons.Add(button);

            // ボタンの見た目を設定
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null && monsterData.icon != null)
            {
                buttonImage.sprite = monsterData.icon;
                buttonImage.color = monsterData.rarityColor;
            }

            // テキストを設定
            Text buttonText = button.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = $"{monsterData.displayName}\n{monsterData.goldCost}G";
            }

            // クリックイベントを設定
            button.onClick.AddListener(() => SelectMonster(monsterData));

            // 購入可能かどうかでボタンの状態を更新
            UpdateMonsterButtonState(button, monsterData);
        }

        private void UpdateMonsterButtonState(Button button, MonsterData monsterData)
        {
            // TODO: 金貨システムと連携して購入可能性をチェック
            bool canAfford = true; // 仮実装
            
            button.interactable = canAfford;
            
            // 色で視覚的に表現
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = canAfford ? monsterData.rarityColor : Color.gray;
            }
        }

        private void SelectMonster(MonsterData monsterData)
        {
            selectedMonsterData = monsterData;
            selectedMonsterType = monsterData.type;
            
            // 配置モードに移行
            SetPlacementMode(true);
            
            // モンスター選択パネルを非表示
            if (monsterSelectionPanel != null)
            {
                monsterSelectionPanel.SetActive(false);
            }

            Debug.Log($"Selected monster: {selectedMonsterType}");
        }

        private void ToggleMonsterSelection()
        {
            if (monsterSelectionPanel != null)
            {
                bool isActive = monsterSelectionPanel.activeSelf;
                monsterSelectionPanel.SetActive(!isActive);
                
                if (!isActive)
                {
                    // パネルを開く時にボタンを更新
                    UpdateMonsterButtons();
                }
            }
        }

        private void UpdateMonsterButtons()
        {
            // 解放状況が変わった場合にボタンを再作成
            CreateMonsterButtons();
        }

        private void SetPlacementMode(bool enabled)
        {
            isPlacementMode = enabled;
            
            // UIの表示状態を更新
            if (cancelPlacementButton != null)
            {
                cancelPlacementButton.gameObject.SetActive(enabled);
            }

            // ゴーストオブジェクトの管理
            if (enabled)
            {
                CreatePlacementGhost();
            }
            else
            {
                DestroyPlacementGhost();
            }

            OnPlacementModeChanged?.Invoke();
        }

        private void CreatePlacementGhost()
        {
            if (selectedMonsterData == null || selectedMonsterData.prefab == null)
            {
                return;
            }

            DestroyPlacementGhost();

            // PlacementGhostSystemを使用してゴーストを作成
            if (PlacementGhostSystem.Instance != null)
            {
                PlacementGhostSystem.Instance.CreateGhost(selectedMonsterData);
            }
            else
            {
                // フォールバック：従来の方法
                if (placementGhostPrefab != null)
                {
                    placementGhost = Instantiate(placementGhostPrefab);
                }
                else
                {
                    placementGhost = Instantiate(selectedMonsterData.prefab);
                }

                // ゴーストの設定
                SetupPlacementGhost();
            }
        }

        private void SetupPlacementGhost()
        {
            if (placementGhost == null) return;

            // 半透明にする
            Renderer[] renderers = placementGhost.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = 0.5f;
                        material.color = color;
                    }
                }
            }

            // コライダーを無効化
            Collider2D[] colliders = placementGhost.GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // スクリプトを無効化
            MonoBehaviour[] scripts = placementGhost.GetComponentsInChildren<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != null)
                {
                    script.enabled = false;
                }
            }
        }

        private void DestroyPlacementGhost()
        {
            // PlacementGhostSystemを使用してゴーストを破棄
            if (PlacementGhostSystem.Instance != null)
            {
                PlacementGhostSystem.Instance.DestroyCurrentGhost();
            }
            
            // フォールバック：従来の方法
            if (placementGhost != null)
            {
                Destroy(placementGhost);
                placementGhost = null;
            }
        }

        private void HandlePlacementInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 worldPosition = GetMouseWorldPosition();
                TryPlaceMonster(worldPosition);
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }

        private void UpdatePlacementGhost()
        {
            if (placementGhost == null) return;

            Vector2 mousePosition = GetMouseWorldPosition();
            placementGhost.transform.position = new Vector3(mousePosition.x, mousePosition.y, 0);

            // 配置可能かどうかで色を変更
            bool canPlace = CanPlaceAtPosition(mousePosition);
            UpdateGhostColor(canPlace);
        }

        private bool CanPlaceAtPosition(Vector2 position)
        {
            if (MonsterPlacementManager.Instance == null || FloorSystem.Instance == null)
            {
                return false;
            }

            int currentFloor = FloorSystem.Instance.CurrentViewFloor;
            return MonsterPlacementManager.Instance.CanPlaceMonster(currentFloor, position, selectedMonsterType);
        }

        private void UpdateGhostColor(bool canPlace)
        {
            if (placementGhost == null) return;

            Color targetColor = canPlace ? Color.green : Color.red;
            targetColor.a = 0.5f;

            Renderer[] renderers = placementGhost.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        material.color = targetColor;
                    }
                }
            }
        }

        private void TryPlaceMonster(Vector2 position)
        {
            if (MonsterPlacementManager.Instance == null || FloorSystem.Instance == null)
            {
                return;
            }

            // PlacementGhostSystemを使用して配置を試行
            if (PlacementGhostSystem.Instance != null)
            {
                if (PlacementGhostSystem.Instance.TryPlaceMonster())
                {
                    // 配置要求イベントを発火
                    OnMonsterPlaceRequested?.Invoke(selectedMonsterType, position);
                }
                else
                {
                    Debug.Log("Cannot place monster at this position");
                }
                return;
            }

            // フォールバック：従来の方法
            int currentFloor = FloorSystem.Instance.CurrentViewFloor;
            
            if (MonsterPlacementManager.Instance.CanPlaceMonster(currentFloor, position, selectedMonsterType))
            {
                // 配置要求イベントを発火
                OnMonsterPlaceRequested?.Invoke(selectedMonsterType, position);
                
                // 配置モードを継続するか終了するかは設定による
                // 今回は継続する
            }
            else
            {
                Debug.Log("Cannot place monster at this position");
                // TODO: エラーメッセージ表示
            }
        }

        private Vector2 GetMouseWorldPosition()
        {
            if (mainCamera == null) return Vector2.zero;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(mousePos);
        }

        private void CancelPlacement()
        {
            SetPlacementMode(false);
            selectedMonsterData = null;
        }

        private void UpdateUI()
        {
            UpdateFloorCapacityText();
            UpdateGoldText();
        }

        private void UpdateFloorCapacityText()
        {
            if (floorCapacityText == null || FloorSystem.Instance == null || MonsterPlacementManager.Instance == null)
            {
                return;
            }

            int currentFloor = FloorSystem.Instance.CurrentViewFloor;
            int currentCount = MonsterPlacementManager.Instance.GetMonsterCountOnFloor(currentFloor);
            int maxCount = 15; // maxMonstersPerFloor

            floorCapacityText.text = $"モンスター: {currentCount}/{maxCount}";
        }

        private void UpdateGoldText()
        {
            if (goldText == null)
            {
                return;
            }

            // TODO: ResourceManagerと連携
            goldText.text = "Gold: 1000"; // 仮実装
        }

        // イベントハンドラー
        private void OnFloorMonsterCountChanged(int floorIndex, int newCount)
        {
            if (FloorSystem.Instance != null && floorIndex == FloorSystem.Instance.CurrentViewFloor)
            {
                UpdateFloorCapacityText();
            }
        }

        private void OnFloorChanged(int newFloor)
        {
            UpdateUI();
        }

        private void OnDestroy()
        {
            // イベントの購読解除
            if (MonsterPlacementManager.Instance != null)
            {
                MonsterPlacementManager.Instance.OnFloorMonsterCountChanged -= OnFloorMonsterCountChanged;
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged -= OnFloorChanged;
            }

            if (FloorExpansionSystem.Instance != null)
            {
                FloorExpansionSystem.Instance.OnNewMonstersUnlocked -= OnNewMonstersUnlocked;
            }

            DestroyPlacementGhost();
        }

        // 公開メソッド
        public bool IsInPlacementMode()
        {
            return isPlacementMode;
        }

        public MonsterType GetSelectedMonsterType()
        {
            return selectedMonsterType;
        }

        public void RefreshUnlockedMonsters()
        {
            UpdateMonsterButtons();
        }

        /// <summary>
        /// 新モンスター解放イベントハンドラー
        /// </summary>
        private void OnNewMonstersUnlocked(List<MonsterType> unlockedMonsters)
        {
            Debug.Log($"MonsterPlacementUI: New monsters unlocked, refreshing buttons");
            RefreshUnlockedMonsters();
        }
    }
}