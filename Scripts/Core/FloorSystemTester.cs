using UnityEngine;
using DungeonOwner.Core;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 階層システムのテスト用コンポーネント
    /// エディタでの動作確認に使用
    /// </summary>
    public class FloorSystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableDebugKeys = true;
        [SerializeField] private GameObject testMonsterPrefab;

        private void Update()
        {
            if (!enableDebugKeys || FloorSystem.Instance == null) return;

            // デバッグキー操作
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                FloorSystem.Instance.ChangeViewFloor(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                FloorSystem.Instance.ChangeViewFloor(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                FloorSystem.Instance.ChangeViewFloor(3);
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                FloorSystem.Instance.MoveToUpperFloor();
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                FloorSystem.Instance.MoveToLowerFloor();
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                TestExpandFloor();
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                TestPlaceMonster();
            }
            else if (Input.GetKeyDown(KeyCode.I))
            {
                FloorSystem.Instance.DebugPrintFloorInfo();
            }
        }

        private void TestExpandFloor()
        {
            if (FloorSystem.Instance.CanExpandFloor())
            {
                bool success = FloorSystem.Instance.ExpandFloor();
                Debug.Log($"Floor expansion test: {(success ? "Success" : "Failed")}");
            }
            else
            {
                Debug.Log("Cannot expand floor: maximum reached");
            }
        }

        private void TestPlaceMonster()
        {
            if (testMonsterPrefab == null)
            {
                Debug.LogWarning("Test monster prefab not assigned");
                return;
            }

            int currentFloor = FloorSystem.Instance.CurrentViewFloor;
            Vector2 testPosition = new Vector2(Random.Range(-3f, 3f), Random.Range(-3f, 3f));

            if (FloorSystem.Instance.CanPlaceMonster(currentFloor, testPosition))
            {
                GameObject testMonster = Instantiate(testMonsterPrefab);
                bool success = FloorSystem.Instance.PlaceMonster(currentFloor, testMonster, testPosition);
                Debug.Log($"Monster placement test: {(success ? "Success" : "Failed")} at {testPosition}");
            }
            else
            {
                Debug.Log($"Cannot place monster at {testPosition} on floor {currentFloor}");
            }
        }

        private void OnGUI()
        {
            if (!enableDebugKeys || FloorSystem.Instance == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== Floor System Debug ===");
            GUILayout.Label($"Current View Floor: {FloorSystem.Instance.CurrentViewFloor}");
            GUILayout.Label($"Total Floors: {FloorSystem.Instance.CurrentFloorCount}");
            GUILayout.Label($"Max Floors: {FloorSystem.Instance.MaxFloors}");
            
            GUILayout.Space(10);
            GUILayout.Label("Controls:");
            GUILayout.Label("1,2,3 - Switch to floor");
            GUILayout.Label("↑↓ - Move between floors");
            GUILayout.Label("E - Expand floor");
            GUILayout.Label("M - Place test monster");
            GUILayout.Label("I - Print floor info");

            if (FloorSystem.Instance.CurrentFloorCount > 0)
            {
                Floor currentFloor = FloorSystem.Instance.GetCurrentFloor();
                if (currentFloor != null)
                {
                    GUILayout.Space(10);
                    GUILayout.Label($"Current Floor Info:");
                    int monsterCount = 0;
                    foreach (var monster in currentFloor.placedMonsters)
                    {
                        if (monster != null) monsterCount++;
                    }
                    GUILayout.Label($"Monsters: {monsterCount}/15");
                    GUILayout.Label($"Has Core: {currentFloor.hasCore}");
                    GUILayout.Label($"Boss: {currentFloor.bossType}");
                }
            }

            GUILayout.EndArea();
        }
    }
}