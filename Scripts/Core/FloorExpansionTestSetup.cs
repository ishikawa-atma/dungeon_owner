using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Managers;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 階層拡張システムのテスト環境セットアップ
    /// テストシーンで使用
    /// </summary>
    public class FloorExpansionTestSetup : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private int initialGold = 5000;
        [SerializeField] private bool enableTester = true;

        private void Start()
        {
            if (autoSetup)
            {
                SetupTestEnvironment();
            }
        }

        /// <summary>
        /// テスト環境のセットアップ
        /// </summary>
        public void SetupTestEnvironment()
        {
            Debug.Log("Setting up Floor Expansion Test Environment...");

            // 必要なマネージャーを作成
            SetupManagers();

            // 初期金貨を設定
            SetupInitialResources();

            // テスターを有効化
            if (enableTester)
            {
                SetupTester();
            }

            Debug.Log("Floor Expansion Test Environment setup completed");
        }

        /// <summary>
        /// 必要なマネージャーのセットアップ
        /// </summary>
        private void SetupManagers()
        {
            // GameManager
            if (GameManager.Instance == null)
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
            }

            // DataManager
            if (DataManager.Instance == null)
            {
                GameObject dataManagerObj = new GameObject("DataManager");
                dataManagerObj.AddComponent<DataManager>();
            }

            // FloorSystem
            if (FloorSystem.Instance == null)
            {
                GameObject floorSystemObj = new GameObject("FloorSystem");
                floorSystemObj.AddComponent<FloorSystem>();
            }

            // ResourceManager
            if (ResourceManager.Instance == null)
            {
                GameObject resourceManagerObj = new GameObject("ResourceManager");
                resourceManagerObj.AddComponent<ResourceManager>();
            }

            // FloorExpansionSystem
            if (FloorExpansionSystem.Instance == null)
            {
                GameObject floorExpansionSystemObj = new GameObject("FloorExpansionSystem");
                floorExpansionSystemObj.AddComponent<FloorExpansionSystem>();
            }

            // TimeManager
            if (TimeManager.Instance == null)
            {
                GameObject timeManagerObj = new GameObject("TimeManager");
                timeManagerObj.AddComponent<TimeManager>();
            }
        }

        /// <summary>
        /// 初期リソースのセットアップ
        /// </summary>
        private void SetupInitialResources()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.SetGold(initialGold);
                Debug.Log($"Initial gold set to: {initialGold}");
            }
        }

        /// <summary>
        /// テスターのセットアップ
        /// </summary>
        private void SetupTester()
        {
            GameObject testerObj = new GameObject("FloorExpansionSystemTester");
            testerObj.AddComponent<FloorExpansionSystemTester>();
            Debug.Log("FloorExpansionSystemTester added");
        }

        /// <summary>
        /// テスト用のモンスターデータを作成
        /// </summary>
        private void CreateTestMonsterData()
        {
            // 実際のプロジェクトではScriptableObjectとして作成されるが、
            // テスト用に動的に作成する場合のサンプル
            
            // この部分は実際のプロジェクトでは不要
            // MonsterDataはResourcesフォルダに配置されたScriptableObjectから読み込まれる
        }

        /// <summary>
        /// 手動セットアップ用の公開メソッド
        /// </summary>
        [ContextMenu("Setup Test Environment")]
        public void ManualSetup()
        {
            SetupTestEnvironment();
        }

        /// <summary>
        /// テスト環境のリセット
        /// </summary>
        [ContextMenu("Reset Test Environment")]
        public void ResetTestEnvironment()
        {
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.SetGold(initialGold);
            }

            if (FloorSystem.Instance != null)
            {
                // 階層を初期状態にリセット（実装が必要な場合）
                Debug.Log("Floor system reset (implementation needed)");
            }

            Debug.Log("Test environment reset");
        }

        /// <summary>
        /// デバッグ情報の表示
        /// </summary>
        [ContextMenu("Print Debug Info")]
        public void PrintDebugInfo()
        {
            Debug.Log("=== Floor Expansion Test Environment Info ===");

            if (FloorSystem.Instance != null)
            {
                Debug.Log($"Current Floors: {FloorSystem.Instance.CurrentFloorCount}");
                Debug.Log($"Max Floors: {FloorSystem.Instance.MaxFloors}");
            }

            if (ResourceManager.Instance != null)
            {
                Debug.Log($"Current Gold: {ResourceManager.Instance.Gold}");
            }

            if (FloorExpansionSystem.Instance != null)
            {
                bool canExpand = FloorExpansionSystem.Instance.CanExpandFloor();
                Debug.Log($"Can Expand: {canExpand}");

                if (canExpand && FloorSystem.Instance != null)
                {
                    int nextFloor = FloorSystem.Instance.CurrentFloorCount + 1;
                    int cost = FloorExpansionSystem.Instance.CalculateExpansionCost(nextFloor);
                    Debug.Log($"Next Expansion Cost: {cost}");
                }

                var availableMonsters = FloorExpansionSystem.Instance.GetAvailableMonsters();
                Debug.Log($"Available Monsters: {availableMonsters.Count}");
            }
        }
    }
}