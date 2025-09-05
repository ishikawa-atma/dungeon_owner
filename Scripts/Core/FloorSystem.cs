using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    public class FloorSystem : MonoBehaviour
    {
        public static FloorSystem Instance { get; private set; }

        [Header("Floor Configuration")]
        [SerializeField] private int initialFloorCount = 3;
        [SerializeField] private int maxFloors = 100;
        [SerializeField] private GameObject stairPrefab;
        [SerializeField] private Transform floorContainer;

        [Header("Current State")]
        [SerializeField] private int currentViewFloor = 1;

        public int MaxFloors => maxFloors;
        public int CurrentFloorCount { get; private set; }
        public int CurrentViewFloor => currentViewFloor;
        public List<Floor> Floors { get; private set; }

        // イベント
        public System.Action<int> OnFloorChanged;
        public System.Action<int> OnFloorExpanded;
        public System.Action<Floor> OnFloorCreated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeFloorSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeFloorSystem()
        {
            Floors = new List<Floor>();
            CurrentFloorCount = 0;

            // 初期階層を生成
            for (int i = 1; i <= initialFloorCount; i++)
            {
                CreateFloor(i);
            }

            // 最深部にコアを設置
            if (Floors.Count > 0)
            {
                Floors[Floors.Count - 1].SetAsCore();
            }

            Debug.Log($"FloorSystem initialized with {CurrentFloorCount} floors");
        }

        public Floor CreateFloor(int floorIndex)
        {
            if (floorIndex <= 0 || floorIndex > maxFloors)
            {
                Debug.LogError($"Invalid floor index: {floorIndex}");
                return null;
            }

            // 既存の階層をチェック
            Floor existingFloor = GetFloor(floorIndex);
            if (existingFloor != null)
            {
                Debug.LogWarning($"Floor {floorIndex} already exists");
                return existingFloor;
            }

            // 新しい階層を作成
            Floor newFloor = new Floor(floorIndex);
            
            // リストに追加（ソート順を維持）
            Floors.Add(newFloor);
            Floors = Floors.OrderBy(f => f.floorIndex).ToList();
            
            CurrentFloorCount = Floors.Count;

            // 階段オブジェクトを生成
            CreateStairObjects(newFloor);

            OnFloorCreated?.Invoke(newFloor);
            Debug.Log($"Created floor {floorIndex}");

            return newFloor;
        }

        private void CreateStairObjects(Floor floor)
        {
            if (stairPrefab == null || floorContainer == null)
            {
                Debug.LogWarning("Stair prefab or floor container not assigned");
                return;
            }

            // 上り階段を作成（1階層以外）
            if (floor.floorIndex > 1)
            {
                GameObject upStair = Instantiate(stairPrefab, floorContainer);
                upStair.transform.position = new Vector3(floor.upStairPosition.x, floor.upStairPosition.y, 0);
                upStair.name = $"UpStair_Floor{floor.floorIndex}";
                upStair.SetActive(false); // 初期は非表示
            }

            // 下り階段を作成（最深部以外）
            if (floor.floorIndex < maxFloors && !floor.hasCore)
            {
                GameObject downStair = Instantiate(stairPrefab, floorContainer);
                downStair.transform.position = new Vector3(floor.downStairPosition.x, floor.downStairPosition.y, 0);
                downStair.name = $"DownStair_Floor{floor.floorIndex}";
                downStair.SetActive(false); // 初期は非表示
            }
        }

        public Floor GetFloor(int floorIndex)
        {
            return Floors.FirstOrDefault(f => f.floorIndex == floorIndex);
        }

        public Floor GetCurrentFloor()
        {
            return GetFloor(currentViewFloor);
        }

        public bool CanPlaceMonster(int floorIndex, Vector2 position)
        {
            Floor floor = GetFloor(floorIndex);
            if (floor == null)
            {
                return false;
            }

            return floor.CanPlaceMonster(position);
        }

        public bool PlaceMonster(int floorIndex, GameObject monster, Vector2 position)
        {
            if (monster == null)
            {
                return false;
            }

            Floor floor = GetFloor(floorIndex);
            if (floor == null || !floor.CanPlaceMonster(position))
            {
                return false;
            }

            // モンスターを配置
            monster.transform.position = new Vector3(position.x, position.y, 0);
            floor.AddMonster(monster);

            Debug.Log($"Placed monster on floor {floorIndex} at position {position}");
            return true;
        }

        public void RemoveMonster(int floorIndex, GameObject monster)
        {
            Floor floor = GetFloor(floorIndex);
            if (floor != null)
            {
                floor.RemoveMonster(monster);
                Debug.Log($"Removed monster from floor {floorIndex}");
            }
        }

        public bool CanExpandFloor()
        {
            return CurrentFloorCount < maxFloors;
        }

        public int GetExpansionCost(int targetFloor)
        {
            // 階層数に応じてコストが増加
            // 基本コスト: 100 + (階層数 * 50)
            return 100 + (targetFloor * 50);
        }

        public bool ExpandFloor()
        {
            if (!CanExpandFloor())
            {
                Debug.LogWarning("Cannot expand floor: maximum floors reached");
                return false;
            }

            int newFloorIndex = CurrentFloorCount + 1;
            Floor newFloor = CreateFloor(newFloorIndex);

            if (newFloor != null)
            {
                // 前の最深部からコアを移動
                if (Floors.Count > 1)
                {
                    Floor previousDeepest = Floors[Floors.Count - 2];
                    previousDeepest.hasCore = false;
                }

                // 新しい最深部にコアを設置
                newFloor.SetAsCore();

                OnFloorExpanded?.Invoke(newFloorIndex);
                Debug.Log($"Expanded to floor {newFloorIndex}");
                return true;
            }

            return false;
        }

        public void ChangeViewFloor(int floorIndex)
        {
            if (floorIndex < 1 || floorIndex > CurrentFloorCount)
            {
                Debug.LogWarning($"Invalid floor index for view: {floorIndex}");
                return;
            }

            currentViewFloor = floorIndex;
            UpdateFloorVisibility();
            OnFloorChanged?.Invoke(currentViewFloor);

            Debug.Log($"Changed view to floor {currentViewFloor}");
        }

        private void UpdateFloorVisibility()
        {
            if (floorContainer == null) return;

            // 全ての階層オブジェクトを非表示
            foreach (Transform child in floorContainer)
            {
                child.gameObject.SetActive(false);
            }

            // 現在の階層のオブジェクトのみ表示
            string upStairName = $"UpStair_Floor{currentViewFloor}";
            string downStairName = $"DownStair_Floor{currentViewFloor}";

            foreach (Transform child in floorContainer)
            {
                if (child.name == upStairName || child.name == downStairName)
                {
                    child.gameObject.SetActive(true);
                }
            }

            // 現在の階層のモンスターと侵入者を表示
            Floor currentFloor = GetCurrentFloor();
            if (currentFloor != null)
            {
                foreach (var monster in currentFloor.placedMonsters)
                {
                    if (monster != null)
                    {
                        monster.SetActive(true);
                    }
                }

                foreach (var invader in currentFloor.activeInvaders)
                {
                    if (invader != null)
                    {
                        invader.SetActive(true);
                    }
                }
            }
        }

        public void MoveToUpperFloor()
        {
            if (currentViewFloor > 1)
            {
                ChangeViewFloor(currentViewFloor - 1);
            }
        }

        public void MoveToLowerFloor()
        {
            if (currentViewFloor < CurrentFloorCount)
            {
                ChangeViewFloor(currentViewFloor + 1);
            }
        }

        public Vector2 GetUpStairPosition(int floorIndex)
        {
            Floor floor = GetFloor(floorIndex);
            return floor?.upStairPosition ?? Vector2.zero;
        }

        public Vector2 GetDownStairPosition(int floorIndex)
        {
            Floor floor = GetFloor(floorIndex);
            return floor?.downStairPosition ?? Vector2.zero;
        }

        public bool IsFloorEmpty(int floorIndex)
        {
            Floor floor = GetFloor(floorIndex);
            return floor?.IsEmpty() ?? true;
        }

        public List<Floor> GetAllFloors()
        {
            return new List<Floor>(Floors);
        }

        // デバッグ用メソッド
        public void DebugPrintFloorInfo()
        {
            Debug.Log($"=== Floor System Info ===");
            Debug.Log($"Total Floors: {CurrentFloorCount}");
            Debug.Log($"Current View Floor: {currentViewFloor}");
            
            foreach (var floor in Floors)
            {
                int monsterCount = floor.placedMonsters.Count(m => m != null);
                int invaderCount = floor.activeInvaders.Count(i => i != null);
                Debug.Log($"Floor {floor.floorIndex}: Monsters={monsterCount}, Invaders={invaderCount}, HasCore={floor.hasCore}, Boss={floor.bossType}");
            }
        }
    }
}