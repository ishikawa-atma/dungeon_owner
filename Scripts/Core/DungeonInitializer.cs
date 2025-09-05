using UnityEngine;
using DungeonOwner.Core;

namespace DungeonOwner.Core
{
    /// <summary>
    /// ダンジョンの初期化を管理するコンポーネント
    /// ゲーム開始時の初期設定を行う
    /// </summary>
    public class DungeonInitializer : MonoBehaviour
    {
        [Header("Initialization Settings")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool debugMode = false;

        [Header("Initial Configuration")]
        [SerializeField] private int initialFloors = 3;
        [SerializeField] private int initialGold = 1000;

        public bool IsInitialized { get; private set; } = false;

        // イベント
        public System.Action OnInitializationComplete;

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeDungeon();
            }
        }

        public void InitializeDungeon()
        {
            if (IsInitialized)
            {
                Debug.LogWarning("Dungeon already initialized");
                return;
            }

            Debug.Log("Starting dungeon initialization...");

            // GameManagerの初期化確認
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found during initialization");
                return;
            }

            // FloorSystemの初期化確認
            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem not found during initialization");
                return;
            }

            // 初期階層の確認
            if (FloorSystem.Instance.CurrentFloorCount < initialFloors)
            {
                Debug.LogWarning($"Expected {initialFloors} floors, but only {FloorSystem.Instance.CurrentFloorCount} found");
            }

            // 初期状態の設定
            SetupInitialState();

            IsInitialized = true;
            OnInitializationComplete?.Invoke();

            Debug.Log("Dungeon initialization complete");

            if (debugMode)
            {
                PrintInitializationInfo();
            }
        }

        private void SetupInitialState()
        {
            // 1階層を表示状態に設定
            FloorSystem.Instance.ChangeViewFloor(1);

            // GameManagerの状態を設定
            if (GameManager.Instance.CurrentState == GameState.MainMenu)
            {
                // メニューからゲーム開始への準備
                Debug.Log("Ready to start game from main menu");
            }
        }

        private void PrintInitializationInfo()
        {
            Debug.Log("=== Dungeon Initialization Info ===");
            Debug.Log($"Total Floors: {FloorSystem.Instance.CurrentFloorCount}");
            Debug.Log($"Current View Floor: {FloorSystem.Instance.CurrentViewFloor}");
            Debug.Log($"Game State: {GameManager.Instance.CurrentState}");
            Debug.Log($"Game Speed: {GameManager.Instance.GameSpeed}");

            // 各階層の情報を表示
            foreach (var floor in FloorSystem.Instance.GetAllFloors())
            {
                Debug.Log($"Floor {floor.floorIndex}: " +
                         $"UpStair={floor.upStairPosition}, " +
                         $"DownStair={floor.downStairPosition}, " +
                         $"HasCore={floor.hasCore}");
            }
        }

        /// <summary>
        /// 手動で初期化を実行
        /// </summary>
        [ContextMenu("Initialize Dungeon")]
        public void ManualInitialize()
        {
            IsInitialized = false;
            InitializeDungeon();
        }

        /// <summary>
        /// 初期化状態をリセット
        /// </summary>
        [ContextMenu("Reset Initialization")]
        public void ResetInitialization()
        {
            IsInitialized = false;
            Debug.Log("Initialization state reset");
        }

        /// <summary>
        /// 階層システムの整合性をチェック
        /// </summary>
        public bool ValidateFloorSystem()
        {
            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem instance not found");
                return false;
            }

            var floors = FloorSystem.Instance.GetAllFloors();
            if (floors.Count == 0)
            {
                Debug.LogError("No floors found in FloorSystem");
                return false;
            }

            // 階層番号の連続性をチェック
            for (int i = 0; i < floors.Count; i++)
            {
                if (floors[i].floorIndex != i + 1)
                {
                    Debug.LogError($"Floor index mismatch: expected {i + 1}, got {floors[i].floorIndex}");
                    return false;
                }
            }

            // 最深部にコアがあることを確認
            Floor deepestFloor = floors[floors.Count - 1];
            if (!deepestFloor.hasCore)
            {
                Debug.LogError("Deepest floor does not have core");
                return false;
            }

            Debug.Log("Floor system validation passed");
            return true;
        }
    }
}