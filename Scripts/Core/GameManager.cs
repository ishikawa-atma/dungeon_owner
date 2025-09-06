using UnityEngine;
using System;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Game Settings")]
        [SerializeField] private int initialFloors = 3;
        [SerializeField] private int initialGold = 1000;

        public GameState CurrentState { get; private set; }
        public int CurrentFloor { get; private set; }
        public float GameSpeed { get; private set; } = 1.0f;

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnFloorChanged;
        public event Action<float> OnGameSpeedChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGame();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeGame()
        {
            CurrentState = GameState.MainMenu;
            CurrentFloor = 1;
            GameSpeed = 1.0f;
        }

        private void Start()
        {
            // FloorSystemの初期化を待つ
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged += OnFloorViewChanged;
                FloorSystem.Instance.OnFloorExpanded += OnFloorExpanded;
            }

            // ResourceManagerとLevelDisplayManagerの初期化確認
            InitializeManagers();
        }

        /// <summary>
        /// 必要なマネージャーの初期化確認
        /// </summary>
        private void InitializeManagers()
        {
            // パフォーマンス管理システムの初期化（最優先）
            InitializePerformanceSystem();

            // ResourceManagerの確認
            if (Managers.ResourceManager.Instance == null)
            {
                Debug.LogWarning("ResourceManager not found. Creating one...");
                GameObject resourceManagerObj = new GameObject("ResourceManager");
                resourceManagerObj.AddComponent<Managers.ResourceManager>();
            }

            // LevelDisplayManagerの確認
            if (Managers.LevelDisplayManager.Instance == null)
            {
                Debug.LogWarning("LevelDisplayManager not found. Creating one...");
                GameObject levelDisplayManagerObj = new GameObject("LevelDisplayManager");
                levelDisplayManagerObj.AddComponent<Managers.LevelDisplayManager>();
            }

            // RecoveryManagerの確認
            if (Managers.RecoveryManager.Instance == null)
            {
                Debug.LogWarning("RecoveryManager not found. Creating one...");
                GameObject recoveryManagerObj = new GameObject("RecoveryManager");
                recoveryManagerObj.AddComponent<Managers.RecoveryManager>();
            }

            // FloorExpansionSystemの確認
            if (FloorExpansionSystem.Instance == null)
            {
                Debug.LogWarning("FloorExpansionSystem not found. Creating one...");
                GameObject floorExpansionSystemObj = new GameObject("FloorExpansionSystem");
                floorExpansionSystemObj.AddComponent<FloorExpansionSystem>();
            }

            // TutorialManagerの確認
            if (TutorialManager.Instance == null)
            {
                Debug.LogWarning("TutorialManager not found. Creating one...");
                GameObject tutorialManagerObj = new GameObject("TutorialManager");
                tutorialManagerObj.AddComponent<TutorialManager>();
            }

            // TimeManagerとResourceManagerの連携確認
            InitializeEconomySystem();
            
            // パーティ戦闘システムの初期化
            InitializePartyCombatSystem();

            Debug.Log("All required managers initialized");
        }

        /// <summary>
        /// 経済システムの初期化
        /// </summary>
        private void InitializeEconomySystem()
        {
            // TimeManagerの日次報酬イベントに登録
            if (TimeManager.Instance != null && Managers.ResourceManager.Instance != null)
            {
                TimeManager.Instance.OnDayCompleted += OnDayCompleted;
                Debug.Log("Economy system initialized - TimeManager and ResourceManager linked");
            }
            else
            {
                Debug.LogWarning("Failed to initialize economy system - missing managers");
            }
        }

        /// <summary>
        /// 日完了時の処理
        /// </summary>
        private void OnDayCompleted()
        {
            Debug.Log("Day completed - processing daily rewards");
            
            // ResourceManagerで日次報酬処理
            if (Managers.ResourceManager.Instance != null)
            {
                Managers.ResourceManager.Instance.CheckDailyReward();
            }
        }

        /// <summary>
        /// パフォーマンス管理システムの初期化
        /// </summary>
        private void InitializePerformanceSystem()
        {
            if (PerformanceManager.Instance == null)
            {
                Debug.Log("Initializing Performance Management System...");
                GameObject performanceManagerObj = new GameObject("PerformanceManager");
                performanceManagerObj.AddComponent<PerformanceManager>();
                
                // パフォーマンスシステム初期化完了イベントを購読
                PerformanceManager.Instance.OnPerformanceSystemsInitialized += OnPerformanceSystemsReady;
            }
            else
            {
                Debug.Log("Performance Management System already initialized");
            }
        }

        /// <summary>
        /// パフォーマンスシステム準備完了時の処理
        /// </summary>
        private void OnPerformanceSystemsReady()
        {
            Debug.Log("Performance systems are ready - game optimization enabled");
            
            // パフォーマンス最適化の設定
            if (PerformanceManager.Instance != null)
            {
                // 自動最適化を有効化
                PerformanceManager.Instance.SetAutoOptimizationEnabled(true);
                
                // モバイル向けの設定
                #if UNITY_ANDROID || UNITY_IOS
                PerformanceManager.Instance.SetCriticalPerformanceThreshold(45f); // モバイルは45FPS
                #else
                PerformanceManager.Instance.SetCriticalPerformanceThreshold(50f); // PCは50FPS
                #endif
            }
        }

        /// <summary>
        /// パーティ戦闘システムの初期化
        /// </summary>
        private void InitializePartyCombatSystem()
        {
            // PartyCombatSystemが存在することを確認
            if (PartyCombatSystem.Instance != null)
            {
                Debug.Log("Party combat system initialized");
            }
            else
            {
                // PartyCombatSystemが存在しない場合は作成
                GameObject partyCombatObj = new GameObject("PartyCombatSystem");
                partyCombatObj.AddComponent<PartyCombatSystem>();
                Debug.Log("Party combat system created and initialized");
            }
        }

        private void OnDestroy()
        {
            // イベント購読を解除
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged -= OnFloorViewChanged;
                FloorSystem.Instance.OnFloorExpanded -= OnFloorExpanded;
            }

            // TimeManagerのイベント購読を解除
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayCompleted -= OnDayCompleted;
            }

            // PerformanceManagerのイベント購読を解除
            if (PerformanceManager.Instance != null)
            {
                PerformanceManager.Instance.OnPerformanceSystemsInitialized -= OnPerformanceSystemsReady;
            }
        }

        private void OnFloorViewChanged(int newFloor)
        {
            CurrentFloor = newFloor;
            OnFloorChanged?.Invoke(CurrentFloor);
        }

        private void OnFloorExpanded(int newFloorIndex)
        {
            Debug.Log($"New floor {newFloorIndex} has been added to the dungeon");
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState != newState)
            {
                CurrentState = newState;
                OnStateChanged?.Invoke(newState);
                Debug.Log($"Game state changed to: {newState}");
            }
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
                Time.timeScale = GameSpeed;
            }
        }

        public void SetGameSpeed(float multiplier)
        {
            GameSpeed = Mathf.Clamp(multiplier, 0.5f, 2.0f);
            
            if (CurrentState == GameState.Playing)
            {
                Time.timeScale = GameSpeed;
            }
            
            OnGameSpeedChanged?.Invoke(GameSpeed);
            Debug.Log($"Game speed set to: {GameSpeed}x");
        }

        public void ExpandFloor()
        {
            if (FloorSystem.Instance != null && FloorSystem.Instance.CanExpandFloor())
            {
                bool success = FloorSystem.Instance.ExpandFloor();
                if (success)
                {
                    Debug.Log("Floor expanded successfully");
                }
                else
                {
                    Debug.LogWarning("Failed to expand floor");
                }
            }
            else
            {
                Debug.LogWarning("Cannot expand floor: FloorSystem not available or max floors reached");
            }
        }

        public void StartGame()
        {
            // ダンジョン初期化の確認
            DungeonInitializer initializer = FindObjectOfType<DungeonInitializer>();
            if (initializer != null && !initializer.IsReadyToStart())
            {
                Debug.LogWarning("Dungeon not ready to start. Initializing...");
                initializer.InitializeDungeon();
                
                // 初期化完了を待つ
                if (!initializer.IsReadyToStart())
                {
                    Debug.LogError("Failed to initialize dungeon for game start");
                    return;
                }
            }

            ChangeState(GameState.Playing);
            Time.timeScale = GameSpeed;
            Debug.Log("Game started successfully!");
        }

        public void EndGame()
        {
            ChangeState(GameState.GameOver);
            Time.timeScale = 0f;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && CurrentState == GameState.Playing)
            {
                PauseGame();
            }
        }

        /// <summary>
        /// セーブデータ用の階層設定
        /// </summary>
        public void SetCurrentFloor(int floor)
        {
            CurrentFloor = Mathf.Max(1, floor);
            OnFloorChanged?.Invoke(CurrentFloor);
        }

        /// <summary>
        /// 現在の階層を取得
        /// </summary>
        public int GetCurrentFloor()
        {
            return CurrentFloor;
        }

        // 統合テスト用メソッド
        /// <summary>
        /// ゲームが初期化済みかどうかを確認
        /// </summary>
        public bool IsInitialized
        {
            get { return Instance != null; }
        }

        /// <summary>
        /// ゲームが開始可能な状態かどうかを確認
        /// </summary>
        public bool IsGameReady()
        {
            return FloorSystem.Instance != null &&
                   Managers.ResourceManager.Instance != null &&
                   FindObjectOfType<DungeonInitializer>() != null;
        }

        /// <summary>
        /// 統合テスト用のゲーム状態リセット
        /// </summary>
        public void ResetForTesting()
        {
            CurrentState = GameState.MainMenu;
            CurrentFloor = 1;
            GameSpeed = 1.0f;
            Time.timeScale = 1.0f;
        }
    }
}