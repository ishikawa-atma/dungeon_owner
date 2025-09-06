using UnityEngine;
using UnityEngine.UI;
using DungeonOwner.Core;

namespace DungeonOwner.UI
{
    /// <summary>
    /// UIシステムのテスト用スクリプト
    /// 縦画面レイアウトと片手操作UIの動作確認
    /// </summary>
    public class UISystemTester : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool enableTesting = true;
        [SerializeField] private KeyCode testPortraitLayoutKey = KeyCode.P;
        [SerializeField] private KeyCode testOneHandModeKey = KeyCode.O;
        [SerializeField] private KeyCode testCombatVisualKey = KeyCode.C;
        [SerializeField] private KeyCode testGhostSystemKey = KeyCode.G;

        [Header("Test UI Elements")]
        [SerializeField] private Button testButton;
        [SerializeField] private Text statusText;

        private bool isOneHandMode = true;
        private bool isCombatMode = false;

        private void Start()
        {
            if (!enableTesting) return;

            InitializeTestUI();
            LogUISystemStatus();
        }

        private void Update()
        {
            if (!enableTesting) return;

            HandleTestInput();
            UpdateStatusDisplay();
        }

        /// <summary>
        /// テストUIの初期化
        /// </summary>
        private void InitializeTestUI()
        {
            if (testButton != null)
            {
                testButton.onClick.AddListener(OnTestButtonClicked);
            }

            if (statusText != null)
            {
                statusText.text = "UI System Test Ready";
            }
        }

        /// <summary>
        /// テスト入力の処理
        /// </summary>
        private void HandleTestInput()
        {
            // 縦画面レイアウトテスト
            if (Input.GetKeyDown(testPortraitLayoutKey))
            {
                TestPortraitLayout();
            }

            // 片手操作モードテスト
            if (Input.GetKeyDown(testOneHandModeKey))
            {
                TestOneHandMode();
            }

            // 戦闘ビジュアルテスト
            if (Input.GetKeyDown(testCombatVisualKey))
            {
                TestCombatVisual();
            }

            // ゴーストシステムテスト
            if (Input.GetKeyDown(testGhostSystemKey))
            {
                TestGhostSystem();
            }
        }

        /// <summary>
        /// 縦画面レイアウトのテスト
        /// </summary>
        private void TestPortraitLayout()
        {
            Debug.Log("Testing Portrait Layout...");

            if (UIManager.Instance != null)
            {
                // 画面向きの強制変更をシミュレート
                Debug.Log($"Current screen size: {Screen.width}x{Screen.height}");
                Debug.Log($"Portrait mode: {Screen.height > Screen.width}");
                
                LogUISystemStatus();
            }
            else
            {
                Debug.LogWarning("UIManager not found!");
            }
        }

        /// <summary>
        /// 片手操作モードのテスト
        /// </summary>
        private void TestOneHandMode()
        {
            isOneHandMode = !isOneHandMode;
            Debug.Log($"Testing One-Hand Mode: {isOneHandMode}");

            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetOneHandMode(isOneHandMode);
                Debug.Log($"One-hand mode set to: {isOneHandMode}");
            }
            else
            {
                Debug.LogWarning("UIManager not found!");
            }
        }

        /// <summary>
        /// 戦闘ビジュアルのテスト
        /// </summary>
        private void TestCombatVisual()
        {
            isCombatMode = !isCombatMode;
            Debug.Log($"Testing Combat Visual: {isCombatMode}");

            if (CombatVisualUI.Instance != null)
            {
                Debug.Log($"Combat Visual UI found. In combat: {CombatVisualUI.Instance.IsInCombat()}");
                Debug.Log($"Combat intensity: {CombatVisualUI.Instance.GetCombatIntensity()}");
            }
            else
            {
                Debug.LogWarning("CombatVisualUI not found!");
            }

            // ResourceDisplayUIのコンパクトモードテスト
            if (ResourceDisplayUI.Instance != null)
            {
                ResourceDisplayUI.Instance.SetCompactModeManual(isCombatMode);
                Debug.Log($"Resource display compact mode: {isCombatMode}");
            }
        }

        /// <summary>
        /// ゴーストシステムのテスト
        /// </summary>
        private void TestGhostSystem()
        {
            Debug.Log("Testing Ghost System...");

            if (PlacementGhostSystem.Instance != null)
            {
                bool isInPlacementMode = PlacementGhostSystem.Instance.IsInPlacementMode();
                Debug.Log($"Ghost system found. In placement mode: {isInPlacementMode}");
                
                if (isInPlacementMode)
                {
                    Vector2 ghostPosition = PlacementGhostSystem.Instance.GetCurrentGhostPosition();
                    Vector2 lastValidPosition = PlacementGhostSystem.Instance.GetLastValidPosition();
                    Debug.Log($"Ghost position: {ghostPosition}");
                    Debug.Log($"Last valid position: {lastValidPosition}");
                }
            }
            else
            {
                Debug.LogWarning("PlacementGhostSystem not found!");
            }
        }

        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText == null) return;

            string status = "UI System Status:\n";
            status += $"One-Hand Mode: {isOneHandMode}\n";
            status += $"Combat Mode: {isCombatMode}\n";
            status += $"Screen: {Screen.width}x{Screen.height}\n";
            status += $"Portrait: {Screen.height > Screen.width}\n";

            // UIManager状態
            if (UIManager.Instance != null)
            {
                status += "UIManager: OK\n";
            }
            else
            {
                status += "UIManager: Missing\n";
            }

            // CombatVisualUI状態
            if (CombatVisualUI.Instance != null)
            {
                status += $"Combat UI: OK (In Combat: {CombatVisualUI.Instance.IsInCombat()})\n";
            }
            else
            {
                status += "Combat UI: Missing\n";
            }

            // PlacementGhostSystem状態
            if (PlacementGhostSystem.Instance != null)
            {
                status += $"Ghost System: OK (Active: {PlacementGhostSystem.Instance.IsInPlacementMode()})\n";
            }
            else
            {
                status += "Ghost System: Missing\n";
            }

            // ResourceDisplayUI状態
            if (ResourceDisplayUI.Instance != null)
            {
                status += "Resource UI: OK\n";
            }
            else
            {
                status += "Resource UI: Missing\n";
            }

            statusText.text = status;
        }

        /// <summary>
        /// UIシステムの状態をログ出力
        /// </summary>
        private void LogUISystemStatus()
        {
            Debug.Log("=== UI System Status ===");
            Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height}");
            Debug.Log($"Screen DPI: {Screen.dpi}");
            Debug.Log($"Portrait Mode: {Screen.height > Screen.width}");
            Debug.Log($"Safe Area: {Screen.safeArea}");

            // 各UIコンポーネントの状態確認
            Debug.Log($"UIManager: {(UIManager.Instance != null ? "OK" : "Missing")}");
            Debug.Log($"CombatVisualUI: {(CombatVisualUI.Instance != null ? "OK" : "Missing")}");
            Debug.Log($"PlacementGhostSystem: {(PlacementGhostSystem.Instance != null ? "OK" : "Missing")}");
            Debug.Log($"ResourceDisplayUI: {(ResourceDisplayUI.Instance != null ? "OK" : "Missing")}");

            Debug.Log("========================");
        }

        /// <summary>
        /// テストボタンクリック時の処理
        /// </summary>
        private void OnTestButtonClicked()
        {
            Debug.Log("Test button clicked - UI system is responsive!");
            
            // 簡単な視覚的フィードバック
            if (testButton != null)
            {
                StartCoroutine(ButtonFeedback());
            }
        }

        /// <summary>
        /// ボタンフィードバックアニメーション
        /// </summary>
        private System.Collections.IEnumerator ButtonFeedback()
        {
            Vector3 originalScale = testButton.transform.localScale;
            
            // 拡大
            float duration = 0.1f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                testButton.transform.localScale = Vector3.Lerp(originalScale, originalScale * 1.1f, t);
                yield return null;
            }
            
            // 縮小
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                testButton.transform.localScale = Vector3.Lerp(originalScale * 1.1f, originalScale, t);
                yield return null;
            }
            
            testButton.transform.localScale = originalScale;
        }

        private void OnGUI()
        {
            if (!enableTesting) return;

            // 画面上部にテスト用の情報を表示
            GUI.Box(new Rect(10, 10, 300, 120), "UI System Test Controls");
            GUI.Label(new Rect(20, 35, 280, 20), $"P: Test Portrait Layout");
            GUI.Label(new Rect(20, 55, 280, 20), $"O: Toggle One-Hand Mode ({isOneHandMode})");
            GUI.Label(new Rect(20, 75, 280, 20), $"C: Toggle Combat Visual ({isCombatMode})");
            GUI.Label(new Rect(20, 95, 280, 20), $"G: Test Ghost System");
        }
    }
}