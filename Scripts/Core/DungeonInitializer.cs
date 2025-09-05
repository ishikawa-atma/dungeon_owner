using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// ダンジョンの初期化を管理するコンポーネント
    /// ゲーム開始時の初期設定を行う
    /// タスク5: 初期モンスター配置とゲーム開始状態の実装
    /// </summary>
    public class DungeonInitializer : MonoBehaviour
    {
        [Header("Initialization Settings")]
        [SerializeField] private bool initializeOnStart = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool placeInitialMonsters = true;

        [Header("Initial Configuration")]
        [SerializeField] private int initialFloors = 3;
        [SerializeField] private int initialGold = 1000;
        [SerializeField] private PlayerCharacterType defaultPlayerCharacter = PlayerCharacterType.Warrior;

        [Header("Component References")]
        [SerializeField] private InitialMonsterPlacer monsterPlacer;

        public bool IsInitialized { get; private set; } = false;
        public bool IsMonsterPlacementComplete { get; private set; } = false;

        // イベント
        public System.Action OnInitializationComplete;
        public System.Action OnMonsterPlacementComplete;

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeDungeon();
            }
        }

        private void Awake()
        {
            // InitialMonsterPlacerの参照を取得
            if (monsterPlacer == null)
            {
                monsterPlacer = FindObjectOfType<InitialMonsterPlacer>();
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

            // 必要なシステムの初期化確認
            if (!ValidateRequiredSystems())
            {
                Debug.LogError("Required systems not available for initialization");
                return;
            }

            // 初期階層の確認
            if (FloorSystem.Instance.CurrentFloorCount < initialFloors)
            {
                Debug.LogWarning($"Expected {initialFloors} floors, but only {FloorSystem.Instance.CurrentFloorCount} found");
            }

            // 初期状態の設定
            SetupInitialState();

            // 初期モンスター配置
            if (placeInitialMonsters)
            {
                PlaceInitialMonsters();
            }

            IsInitialized = true;
            OnInitializationComplete?.Invoke();

            Debug.Log("Dungeon initialization complete");

            if (debugMode)
            {
                PrintInitializationInfo();
            }
        }

        private bool ValidateRequiredSystems()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found during initialization");
                return false;
            }

            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem not found during initialization");
                return false;
            }

            if (DataManager.Instance == null)
            {
                Debug.LogError("DataManager not found during initialization");
                return false;
            }

            return true;
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

            // 初期リソースの設定（経済システムが実装されたら使用）
            Debug.Log($"Initial gold set to: {initialGold}");
        }

        private void PlaceInitialMonsters()
        {
            if (monsterPlacer == null)
            {
                Debug.LogWarning("InitialMonsterPlacer not found, creating one...");
                CreateMonsterPlacer();
            }

            if (monsterPlacer != null)
            {
                // プレイヤーキャラクタータイプを設定
                monsterPlacer.SetPlayerCharacterType(defaultPlayerCharacter);

                // イベント購読
                monsterPlacer.OnPlacementComplete += OnInitialMonsterPlacementComplete;

                // 初期配置実行
                monsterPlacer.PlaceInitialMonsters();
            }
            else
            {
                Debug.LogError("Failed to create or find InitialMonsterPlacer");
            }
        }

        private void CreateMonsterPlacer()
        {
            GameObject placerObj = new GameObject("InitialMonsterPlacer");
            monsterPlacer = placerObj.AddComponent<InitialMonsterPlacer>();
            placerObj.transform.SetParent(this.transform);
        }

        private void OnInitialMonsterPlacementComplete()
        {
            IsMonsterPlacementComplete = true;
            OnMonsterPlacementComplete?.Invoke();
            
            Debug.Log("Initial monster placement completed successfully");
            
            // イベント購読解除
            if (monsterPlacer != null)
            {
                monsterPlacer.OnPlacementComplete -= OnInitialMonsterPlacementComplete;
            }
        }

        private void PrintInitializationInfo()
        {
            Debug.Log("=== Dungeon Initialization Info ===");
            Debug.Log($"Total Floors: {FloorSystem.Instance.CurrentFloorCount}");
            Debug.Log($"Current View Floor: {FloorSystem.Instance.CurrentViewFloor}");
            Debug.Log($"Game State: {GameManager.Instance.CurrentState}");
            Debug.Log($"Game Speed: {GameManager.Instance.GameSpeed}");
            Debug.Log($"Initial Gold: {initialGold}");
            Debug.Log($"Default Player Character: {defaultPlayerCharacter}");
            Debug.Log($"Monster Placement Complete: {IsMonsterPlacementComplete}");

            // 各階層の情報を表示
            foreach (var floor in FloorSystem.Instance.GetAllFloors())
            {
                int monsterCount = floor.placedMonsters?.Count ?? 0;
                Debug.Log($"Floor {floor.floorIndex}: " +
                         $"UpStair={floor.upStairPosition}, " +
                         $"DownStair={floor.downStairPosition}, " +
                         $"HasCore={floor.hasCore}, " +
                         $"Monsters={monsterCount}");
            }

            // 配置されたモンスターの詳細
            if (monsterPlacer != null && IsMonsterPlacementComplete)
            {
                var placedMonsters = monsterPlacer.GetPlacedMonsters();
                var playerCharacter = monsterPlacer.GetPlacedPlayerCharacter();
                
                Debug.Log($"Placed Monsters: {placedMonsters.Count}");
                Debug.Log($"Player Character: {(playerCharacter != null ? playerCharacter.name : "None")}");
            }
        }

        /// <summary>
        /// 手動で初期化を実行
        /// </summary>
        [ContextMenu("Initialize Dungeon")]
        public void ManualInitialize()
        {
            IsInitialized = false;
            IsMonsterPlacementComplete = false;
            InitializeDungeon();
        }

        /// <summary>
        /// 初期化状態をリセット
        /// </summary>
        [ContextMenu("Reset Initialization")]
        public void ResetInitialization()
        {
            // 配置されたモンスターをクリア
            if (monsterPlacer != null)
            {
                monsterPlacer.ClearPlacedMonsters();
            }

            IsInitialized = false;
            IsMonsterPlacementComplete = false;
            Debug.Log("Initialization state reset");
        }

        /// <summary>
        /// プレイヤーキャラクタータイプを設定
        /// </summary>
        public void SetPlayerCharacterType(PlayerCharacterType characterType)
        {
            defaultPlayerCharacter = characterType;
            
            if (monsterPlacer != null)
            {
                monsterPlacer.SetPlayerCharacterType(characterType);
            }
            
            Debug.Log($"Player character type set to: {characterType}");
        }

        /// <summary>
        /// ゲーム開始準備が完了しているかチェック
        /// </summary>
        public bool IsReadyToStart()
        {
            return IsInitialized && IsMonsterPlacementComplete;
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