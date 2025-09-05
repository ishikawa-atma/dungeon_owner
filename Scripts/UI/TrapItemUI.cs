using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Scripts.Data.ScriptableObjects;
using Scripts.Data.Enums;
using Scripts.Managers;

namespace Scripts.UI
{
    /// <summary>
    /// 罠アイテムUI管理システム
    /// 要件13.3, 13.4: 罠アイテム使用と視覚エフェクト、インベントリ管理
    /// </summary>
    public class TrapItemUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject trapItemPanel;
        [SerializeField] private Transform itemSlotParent;
        [SerializeField] private GameObject itemSlotPrefab;
        [SerializeField] private Button toggleButton;
        [SerializeField] private TextMeshProUGUI instructionText;
        
        [Header("使用モード")]
        [SerializeField] private GameObject targetingCursor;
        [SerializeField] private LineRenderer rangeIndicator;
        [SerializeField] private Color validTargetColor = Color.green;
        [SerializeField] private Color invalidTargetColor = Color.red;
        
        private List<TrapItemSlot> itemSlots = new List<TrapItemSlot>();
        private TrapItemData selectedTrapItem;
        private bool isTargetingMode = false;
        private Camera mainCamera;
        
        // UI状態
        private bool isPanelOpen = false;
        
        private void Start()
        {
            mainCamera = Camera.main;
            
            // UI初期化
            InitializeUI();
            
            // イベント登録
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged += RefreshInventoryDisplay;
            }
            
            // 初期状態設定
            SetPanelVisibility(false);
            SetTargetingMode(false);
        }
        
        private void OnDestroy()
        {
            // イベント解除
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.OnInventoryChanged -= RefreshInventoryDisplay;
            }
        }
        
        private void Update()
        {
            if (isTargetingMode)
            {
                HandleTargetingInput();
            }
        }
        
        /// <summary>
        /// UI初期化
        /// </summary>
        private void InitializeUI()
        {
            // トグルボタン設定
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener(TogglePanel);
            }
            
            // 範囲表示設定
            if (rangeIndicator != null)
            {
                rangeIndicator.enabled = false;
                rangeIndicator.useWorldSpace = true;
                rangeIndicator.startWidth = 0.1f;
                rangeIndicator.endWidth = 0.1f;
            }
            
            // ターゲティングカーソル設定
            if (targetingCursor != null)
            {
                targetingCursor.SetActive(false);
            }
            
            // 説明テキスト設定
            if (instructionText != null)
            {
                instructionText.text = "罠アイテムを選択してタップで使用";
            }
        }
        
        /// <summary>
        /// パネルの表示/非表示切り替え
        /// </summary>
        public void TogglePanel()
        {
            SetPanelVisibility(!isPanelOpen);
        }
        
        /// <summary>
        /// パネルの表示状態を設定
        /// </summary>
        public void SetPanelVisibility(bool visible)
        {
            isPanelOpen = visible;
            
            if (trapItemPanel != null)
            {
                trapItemPanel.SetActive(visible);
            }
            
            if (visible)
            {
                RefreshInventoryDisplay();
            }
            else
            {
                // ターゲティングモードを終了
                SetTargetingMode(false);
            }
        }
        
        /// <summary>
        /// インベントリ表示を更新
        /// </summary>
        private void RefreshInventoryDisplay()
        {
            if (InventoryManager.Instance == null) return;
            
            // 既存のスロットをクリア
            ClearItemSlots();
            
            // インベントリからアイテムを取得
            var inventory = InventoryManager.Instance.GetInventory();
            
            foreach (var stack in inventory)
            {
                CreateItemSlot(stack);
            }
        }
        
        /// <summary>
        /// アイテムスロットをクリア
        /// </summary>
        private void ClearItemSlots()
        {
            foreach (var slot in itemSlots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    Destroy(slot.gameObject);
                }
            }
            itemSlots.Clear();
        }
        
        /// <summary>
        /// アイテムスロットを作成
        /// </summary>
        private void CreateItemSlot(TrapItemStack stack)
        {
            if (itemSlotPrefab == null || itemSlotParent == null) return;
            
            GameObject slotObj = Instantiate(itemSlotPrefab, itemSlotParent);
            TrapItemSlot slot = slotObj.GetComponent<TrapItemSlot>();
            
            if (slot == null)
            {
                slot = slotObj.AddComponent<TrapItemSlot>();
            }
            
            // スロット設定
            slot.Initialize(stack, this);
            itemSlots.Add(slot);
        }
        
        /// <summary>
        /// 罠アイテムを選択
        /// </summary>
        public void SelectTrapItem(TrapItemData trapItem)
        {
            selectedTrapItem = trapItem;
            SetTargetingMode(true);
            
            // 説明テキスト更新
            if (instructionText != null)
            {
                instructionText.text = $"{trapItem.itemName}を使用する場所をタップしてください";
            }
            
            Debug.Log($"罠アイテム {trapItem.itemName} を選択しました");
        }
        
        /// <summary>
        /// ターゲティングモードを設定
        /// </summary>
        private void SetTargetingMode(bool enabled)
        {
            isTargetingMode = enabled;
            
            if (targetingCursor != null)
            {
                targetingCursor.SetActive(enabled);
            }
            
            if (rangeIndicator != null)
            {
                rangeIndicator.enabled = enabled;
            }
            
            if (!enabled)
            {
                selectedTrapItem = null;
                
                if (instructionText != null)
                {
                    instructionText.text = "罠アイテムを選択してタップで使用";
                }
            }
        }
        
        /// <summary>
        /// ターゲティング入力処理
        /// </summary>
        private void HandleTargetingInput()
        {
            if (selectedTrapItem == null || mainCamera == null) return;
            
            // マウス/タッチ位置を取得
            Vector3 inputPosition = Input.mousePosition;
            Vector2 worldPosition = mainCamera.ScreenToWorldPoint(inputPosition);
            
            // カーソル位置更新
            if (targetingCursor != null)
            {
                targetingCursor.transform.position = worldPosition;
            }
            
            // 範囲表示更新
            UpdateRangeIndicator(worldPosition);
            
            // 使用可能かチェック
            bool canUse = CanUseTrapAt(worldPosition);
            UpdateTargetingVisuals(canUse);
            
            // クリック/タップ処理
            if (Input.GetMouseButtonDown(0))
            {
                if (canUse)
                {
                    UseTrapItem(worldPosition);
                }
                else
                {
                    Debug.Log("この場所では罠アイテムを使用できません");
                }
            }
            
            // キャンセル処理
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                SetTargetingMode(false);
            }
        }
        
        /// <summary>
        /// 範囲表示を更新
        /// </summary>
        private void UpdateRangeIndicator(Vector2 center)
        {
            if (rangeIndicator == null || selectedTrapItem == null) return;
            
            // 円形の範囲を描画
            int segments = 32;
            float radius = selectedTrapItem.range;
            
            rangeIndicator.positionCount = segments + 1;
            
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * 2f * Mathf.PI / segments;
                Vector3 pos = new Vector3(
                    center.x + Mathf.Cos(angle) * radius,
                    center.y + Mathf.Sin(angle) * radius,
                    0f
                );
                rangeIndicator.SetPosition(i, pos);
            }
        }
        
        /// <summary>
        /// 指定位置で罠を使用可能かチェック
        /// </summary>
        private bool CanUseTrapAt(Vector2 position)
        {
            if (selectedTrapItem == null) return false;
            
            // クールダウンチェック
            if (InventoryManager.Instance != null && 
                InventoryManager.Instance.IsOnCooldown(selectedTrapItem.type))
            {
                return false;
            }
            
            // 範囲内に侵入者がいるかチェック
            var colliders = Physics2D.OverlapCircleAll(position, selectedTrapItem.range);
            bool hasInvaders = false;
            
            foreach (var collider in colliders)
            {
                if (collider.CompareTag("Invader"))
                {
                    hasInvaders = true;
                    break;
                }
            }
            
            return hasInvaders;
        }
        
        /// <summary>
        /// ターゲティング表示を更新
        /// </summary>
        private void UpdateTargetingVisuals(bool canUse)
        {
            Color targetColor = canUse ? validTargetColor : invalidTargetColor;
            
            if (rangeIndicator != null)
            {
                rangeIndicator.color = targetColor;
            }
            
            if (targetingCursor != null)
            {
                var spriteRenderer = targetingCursor.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = targetColor;
                }
            }
        }
        
        /// <summary>
        /// 罠アイテムを使用
        /// 要件13.2, 13.3: 罠アイテム使用と侵入者へのダメージ
        /// </summary>
        private void UseTrapItem(Vector2 position)
        {
            if (selectedTrapItem == null || InventoryManager.Instance == null) return;
            
            // インベントリから使用
            bool used = InventoryManager.Instance.UseItem(selectedTrapItem.type, position);
            
            if (used)
            {
                Debug.Log($"罠アイテム {selectedTrapItem.itemName} を使用しました");
                
                // ターゲティングモード終了
                SetTargetingMode(false);
                
                // インベントリ表示更新
                RefreshInventoryDisplay();
            }
            else
            {
                Debug.Log("罠アイテムの使用に失敗しました");
            }
        }
        
        /// <summary>
        /// パネルが開いているかチェック
        /// </summary>
        public bool IsPanelOpen()
        {
            return isPanelOpen;
        }
        
        /// <summary>
        /// ターゲティングモード中かチェック
        /// </summary>
        public bool IsTargetingMode()
        {
            return isTargetingMode;
        }
    }
    
    /// <summary>
    /// 罠アイテムスロット
    /// </summary>
    public class TrapItemSlot : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private Button useButton;
        [SerializeField] private Image cooldownOverlay;
        
        private TrapItemStack itemStack;
        private TrapItemUI parentUI;
        
        private void Update()
        {
            UpdateCooldownDisplay();
        }
        
        /// <summary>
        /// スロット初期化
        /// </summary>
        public void Initialize(TrapItemStack stack, TrapItemUI parent)
        {
            itemStack = stack;
            parentUI = parent;
            
            // UI更新
            UpdateDisplay();
            
            // ボタンイベント設定
            if (useButton != null)
            {
                useButton.onClick.RemoveAllListeners();
                useButton.onClick.AddListener(OnUseButtonClicked);
            }
        }
        
        /// <summary>
        /// 表示更新
        /// </summary>
        private void UpdateDisplay()
        {
            if (itemStack?.itemData == null) return;
            
            // アイコン設定
            if (iconImage != null && itemStack.itemData.icon != null)
            {
                iconImage.sprite = itemStack.itemData.icon;
            }
            
            // 個数表示
            if (countText != null)
            {
                countText.text = itemStack.count.ToString();
            }
            
            // ボタンの有効/無効
            if (useButton != null)
            {
                useButton.interactable = itemStack.count > 0;
            }
        }
        
        /// <summary>
        /// クールダウン表示更新
        /// </summary>
        private void UpdateCooldownDisplay()
        {
            if (itemStack?.itemData == null || InventoryManager.Instance == null) return;
            
            bool isOnCooldown = InventoryManager.Instance.IsOnCooldown(itemStack.itemData.type);
            
            if (isOnCooldown)
            {
                float remainingTime = InventoryManager.Instance.GetRemainingCooldown(itemStack.itemData.type);
                
                // クールダウンテキスト表示
                if (cooldownText != null)
                {
                    cooldownText.text = $"{remainingTime:F1}s";
                    cooldownText.gameObject.SetActive(true);
                }
                
                // クールダウンオーバーレイ表示
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.gameObject.SetActive(true);
                    float progress = 1f - (remainingTime / itemStack.itemData.cooldownTime);
                    cooldownOverlay.fillAmount = progress;
                }
                
                // ボタン無効化
                if (useButton != null)
                {
                    useButton.interactable = false;
                }
            }
            else
            {
                // クールダウン終了
                if (cooldownText != null)
                {
                    cooldownText.gameObject.SetActive(false);
                }
                
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.gameObject.SetActive(false);
                }
                
                // ボタン有効化（個数チェック）
                if (useButton != null)
                {
                    useButton.interactable = itemStack.count > 0;
                }
            }
        }
        
        /// <summary>
        /// 使用ボタンクリック処理
        /// </summary>
        private void OnUseButtonClicked()
        {
            if (itemStack?.itemData != null && parentUI != null)
            {
                parentUI.SelectTrapItem(itemStack.itemData);
            }
        }
    }
}