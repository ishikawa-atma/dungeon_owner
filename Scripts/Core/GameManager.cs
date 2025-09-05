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
        }

        private void OnDestroy()
        {
            // イベント購読を解除
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged -= OnFloorViewChanged;
                FloorSystem.Instance.OnFloorExpanded -= OnFloorExpanded;
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
            ChangeState(GameState.Playing);
            Time.timeScale = GameSpeed;
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