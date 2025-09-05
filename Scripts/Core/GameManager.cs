using UnityEngine;
using System;

namespace DungeonOwner.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Tutorial
    }

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

            // TimeManagerとResourceManagerの連携確認
            InitializeEconomySystem();

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
    }
}