using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonOwner.Core;

namespace DungeonOwner.UI
{
    /// <summary>
    /// 階層改装システムのUI管理
    /// 改装モードの開始/終了、壁の配置/除去、レイアウト保存機能を提供
    /// </summary>
    public class FloorRenovationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject renovationPanel;
        [SerializeField] private Button startRenovationButton;
        [SerializeField] private Button endRenovationButton;
        [SerializeField] private Button saveChangesButton;
        [SerializeField] private Button cancelChangesButton;
        [SerializeField] private Button placeWallButton;
        [SerializeField] private Button removeWallButton;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI errorText;

        [Header("Renovation Mode Settings")]
        [SerializeField] private Color renovationModeColor = Color.yellow;
        [SerializeField] private Color normalModeColor = Color.white;
        [SerializeField] private float errorDisplayDuration = 3f;

        // 状態管理
        private bool isPlaceWallMode = false;
        private bool isRemoveWallMode = false;
        private Camera mainCamera;
        private Coroutine errorDisplayCoroutine;

        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUI()
        {
            mainCamera = Camera.main;
            
            // 初期状態では改装パネルを非表示
            if (renovationPanel != null)
            {
                renovationPanel.SetActive(false);
            }

            // ボタンイベントを設定
            if (startRenovationButton != null)
            {
                startRenovationButton.onClick.AddListener(OnStartRenovationClicked);
            }

            if (endRenovationButton != null)
            {
                endRenovationButton.onClick.AddListener(OnEndRenovationClicked);
            }

            if (saveChangesButton != null)
            {
                saveChangesButton.onClick.AddListener(OnSaveChangesClicked);
            }

            if (cancelChangesButton != null)
            {
                cancelChangesButton.onClick.AddListener(OnCancelChangesClicked);
            }

            if (placeWallButton != null)
            {
                placeWallButton.onClick.AddListener(OnPlaceWallModeToggled);
            }

            if (removeWallButton != null)
            {
                removeWallButton.onClick.AddListener(OnRemoveWallModeToggled);
            }

            UpdateUI();
        }

        private void SubscribeToEvents()
        {
            if (FloorRenovationSystem.Instance != null)
            {
                FloorRenovationSystem.Instance.OnRenovationModeStarted += OnRenovationModeStarted;
                FloorRenovationSystem.Instance.OnRenovationModeEnded += OnRenovationModeEnded;
                FloorRenovationSystem.Instance.OnLayoutSaved += OnLayoutSaved;
                FloorRenovationSystem.Instance.OnWallPlaced += OnWallPlaced;
                FloorRenovationSystem.Instance.OnWallRemoved += OnWallRemoved;
                FloorRenovationSystem.Instance.OnRenovationError += OnRenovationError;
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged += OnFloorChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (FloorRenovationSystem.Instance != null)
            {
                FloorRenovationSystem.Instance.OnRenovationModeStarted -= OnRenovationModeStarted;
                FloorRenovationSystem.Instance.OnRenovationModeEnded -= OnRenovationModeEnded;
                FloorRenovationSystem.Instance.OnLayoutSaved -= OnLayoutSaved;
                FloorRenovationSystem.Instance.OnWallPlaced -= OnWallPlaced;
                FloorRenovationSystem.Instance.OnWallRemoved -= OnWallRemoved;
                FloorRenovationSystem.Instance.OnRenovationError -= OnRenovationError;
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged -= OnFloorChanged;
            }
        }

        private void Update()
        {
            HandleInput();
        }

        /// <summary>
        /// 入力処理
        /// </summary>
        private void HandleInput()
        {
            if (!FloorRenovationSystem.Instance.IsRenovationMode)
            {
                return;
            }

            // タッチ/クリック入力処理
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                Vector2 gridPosition = new Vector2(Mathf.Round(worldPosition.x), Mathf.Round(worldPosition.y));

                if (isPlaceWallMode)
                {
                    FloorRenovationSystem.Instance.PlaceWall(gridPosition);
                }
                else if (isRemoveWallMode)
                {
                    FloorRenovationSystem.Instance.RemoveWall(gridPosition);
                }
            }
        }

        /// <summary>
        /// 改装開始ボタンクリック
        /// </summary>
        private void OnStartRenovationClicked()
        {
            if (FloorSystem.Instance == null)
            {
                ShowError("FloorSystemが見つかりません。");
                return;
            }

            int currentFloor = FloorSystem.Instance.CurrentViewFloor;
            
            if (!FloorRenovationSystem.Instance.CanStartRenovation(currentFloor))
            {
                ShowError($"階層{currentFloor}は改装できません。モンスターまたは侵入者が存在します。");
                return;
            }

            FloorRenovationSystem.Instance.StartRenovation(currentFloor);
        }

        /// <summary>
        /// 改装終了ボタンクリック
        /// </summary>
        private void OnEndRenovationClicked()
        {
            FloorRenovationSystem.Instance.EndRenovation(true);
        }

        /// <summary>
        /// 変更保存ボタンクリック
        /// </summary>
        private void OnSaveChangesClicked()
        {
            FloorRenovationSystem.Instance.EndRenovation(true);
        }

        /// <summary>
        /// 変更キャンセルボタンクリック
        /// </summary>
        private void OnCancelChangesClicked()
        {
            FloorRenovationSystem.Instance.EndRenovation(false);
        }

        /// <summary>
        /// 壁配置モード切り替え
        /// </summary>
        private void OnPlaceWallModeToggled()
        {
            isPlaceWallMode = !isPlaceWallMode;
            if (isPlaceWallMode)
            {
                isRemoveWallMode = false;
            }
            UpdateModeButtons();
        }

        /// <summary>
        /// 壁除去モード切り替え
        /// </summary>
        private void OnRemoveWallModeToggled()
        {
            isRemoveWallMode = !isRemoveWallMode;
            if (isRemoveWallMode)
            {
                isPlaceWallMode = false;
            }
            UpdateModeButtons();
        }

        /// <summary>
        /// 改装モード開始時の処理
        /// </summary>
        private void OnRenovationModeStarted(int floorIndex)
        {
            UpdateUI();
            UpdateStatusText($"階層{floorIndex}の改装モードを開始しました");
            UpdateInstructionText("壁配置モードまたは壁除去モードを選択してください");
        }

        /// <summary>
        /// 改装モード終了時の処理
        /// </summary>
        private void OnRenovationModeEnded(int floorIndex)
        {
            isPlaceWallMode = false;
            isRemoveWallMode = false;
            UpdateUI();
            UpdateStatusText($"階層{floorIndex}の改装モードを終了しました");
            UpdateInstructionText("");
        }

        /// <summary>
        /// レイアウト保存時の処理
        /// </summary>
        private void OnLayoutSaved(int floorIndex, System.Collections.Generic.List<Vector2> wallPositions)
        {
            UpdateStatusText($"階層{floorIndex}のレイアウトを保存しました（壁数: {wallPositions.Count}）");
        }

        /// <summary>
        /// 壁配置時の処理
        /// </summary>
        private void OnWallPlaced(Vector2 position)
        {
            UpdateStatusText($"壁を配置しました: ({position.x}, {position.y})");
        }

        /// <summary>
        /// 壁除去時の処理
        /// </summary>
        private void OnWallRemoved(Vector2 position)
        {
            UpdateStatusText($"壁を除去しました: ({position.x}, {position.y})");
        }

        /// <summary>
        /// エラー発生時の処理
        /// </summary>
        private void OnRenovationError(string errorMessage)
        {
            ShowError(errorMessage);
        }

        /// <summary>
        /// 階層変更時の処理
        /// </summary>
        private void OnFloorChanged(int newFloor)
        {
            UpdateUI();
        }

        /// <summary>
        /// UI状態を更新
        /// </summary>
        private void UpdateUI()
        {
            bool isRenovationMode = FloorRenovationSystem.Instance != null && FloorRenovationSystem.Instance.IsRenovationMode;
            bool canStartRenovation = false;

            if (FloorSystem.Instance != null && FloorRenovationSystem.Instance != null)
            {
                int currentFloor = FloorSystem.Instance.CurrentViewFloor;
                canStartRenovation = FloorRenovationSystem.Instance.CanStartRenovation(currentFloor);
            }

            // パネル表示/非表示
            if (renovationPanel != null)
            {
                renovationPanel.SetActive(isRenovationMode || canStartRenovation);
            }

            // ボタン状態更新
            if (startRenovationButton != null)
            {
                startRenovationButton.gameObject.SetActive(!isRenovationMode && canStartRenovation);
            }

            if (endRenovationButton != null)
            {
                endRenovationButton.gameObject.SetActive(isRenovationMode);
            }

            if (saveChangesButton != null)
            {
                saveChangesButton.gameObject.SetActive(isRenovationMode);
            }

            if (cancelChangesButton != null)
            {
                cancelChangesButton.gameObject.SetActive(isRenovationMode);
            }

            if (placeWallButton != null)
            {
                placeWallButton.gameObject.SetActive(isRenovationMode);
            }

            if (removeWallButton != null)
            {
                removeWallButton.gameObject.SetActive(isRenovationMode);
            }

            UpdateModeButtons();
        }

        /// <summary>
        /// モードボタンの状態を更新
        /// </summary>
        private void UpdateModeButtons()
        {
            // 壁配置ボタンの色を更新
            if (placeWallButton != null)
            {
                ColorBlock colors = placeWallButton.colors;
                colors.normalColor = isPlaceWallMode ? renovationModeColor : normalModeColor;
                placeWallButton.colors = colors;
            }

            // 壁除去ボタンの色を更新
            if (removeWallButton != null)
            {
                ColorBlock colors = removeWallButton.colors;
                colors.normalColor = isRemoveWallMode ? renovationModeColor : normalModeColor;
                removeWallButton.colors = colors;
            }

            // 指示テキストを更新
            if (isPlaceWallMode)
            {
                UpdateInstructionText("壁配置モード: タップして壁を配置してください");
            }
            else if (isRemoveWallMode)
            {
                UpdateInstructionText("壁除去モード: タップして壁を除去してください");
            }
            else if (FloorRenovationSystem.Instance.IsRenovationMode)
            {
                UpdateInstructionText("壁配置モードまたは壁除去モードを選択してください");
            }
        }

        /// <summary>
        /// ステータステキストを更新
        /// </summary>
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }

        /// <summary>
        /// 指示テキストを更新
        /// </summary>
        private void UpdateInstructionText(string message)
        {
            if (instructionText != null)
            {
                instructionText.text = message;
            }
        }

        /// <summary>
        /// エラーメッセージを表示
        /// </summary>
        private void ShowError(string errorMessage)
        {
            if (errorText != null)
            {
                errorText.text = errorMessage;
                errorText.gameObject.SetActive(true);

                // 既存のコルーチンを停止
                if (errorDisplayCoroutine != null)
                {
                    StopCoroutine(errorDisplayCoroutine);
                }

                // 一定時間後にエラーメッセージを非表示
                errorDisplayCoroutine = StartCoroutine(HideErrorAfterDelay());
            }

            Debug.LogWarning($"Renovation Error: {errorMessage}");
        }

        /// <summary>
        /// エラーメッセージを一定時間後に非表示
        /// </summary>
        private System.Collections.IEnumerator HideErrorAfterDelay()
        {
            yield return new WaitForSeconds(errorDisplayDuration);
            
            if (errorText != null)
            {
                errorText.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 改装可能かどうかをチェックして表示を更新
        /// </summary>
        public void CheckRenovationAvailability()
        {
            UpdateUI();
        }
    }
}