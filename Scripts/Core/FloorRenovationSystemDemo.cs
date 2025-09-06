using UnityEngine;
using System.Collections;
using DungeonOwner.Core;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 階層改装システムのデモンストレーション
    /// 改装機能の使用例と動作確認を提供
    /// </summary>
    public class FloorRenovationSystemDemo : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private bool autoStartDemo = false;
        [SerializeField] private float demoStepDelay = 2f;
        [SerializeField] private int demoFloorIndex = 2;

        [Header("Demo Scenarios")]
        [SerializeField] private bool showBasicRenovation = true;
        [SerializeField] private bool showPathValidation = true;
        [SerializeField] private bool showErrorCases = true;
        [SerializeField] private bool showLayoutSaving = true;

        [Header("UI References")]
        [SerializeField] private GameObject demoUI;
        [SerializeField] private UnityEngine.UI.Button startDemoButton;
        [SerializeField] private UnityEngine.UI.Button stopDemoButton;
        [SerializeField] private TMPro.TextMeshProUGUI demoStatusText;

        private bool isDemoRunning = false;
        private Coroutine currentDemo;

        private void Start()
        {
            InitializeDemo();
            
            if (autoStartDemo)
            {
                StartDemo();
            }
        }

        /// <summary>
        /// デモを初期化
        /// </summary>
        private void InitializeDemo()
        {
            // UIボタンの設定
            if (startDemoButton != null)
            {
                startDemoButton.onClick.AddListener(StartDemo);
            }

            if (stopDemoButton != null)
            {
                stopDemoButton.onClick.AddListener(StopDemo);
            }

            UpdateDemoUI();
        }

        /// <summary>
        /// デモを開始
        /// </summary>
        public void StartDemo()
        {
            if (isDemoRunning)
            {
                Debug.LogWarning("Demo is already running");
                return;
            }

            if (FloorRenovationSystem.Instance == null)
            {
                Debug.LogError("FloorRenovationSystem not found");
                return;
            }

            currentDemo = StartCoroutine(RunDemoSequence());
        }

        /// <summary>
        /// デモを停止
        /// </summary>
        public void StopDemo()
        {
            if (currentDemo != null)
            {
                StopCoroutine(currentDemo);
                currentDemo = null;
            }

            isDemoRunning = false;
            
            // 改装モードが有効な場合は終了
            if (FloorRenovationSystem.Instance.IsRenovationMode)
            {
                FloorRenovationSystem.Instance.EndRenovation(false);
            }

            UpdateDemoStatus("デモを停止しました");
            UpdateDemoUI();
        }

        /// <summary>
        /// デモシーケンスを実行
        /// </summary>
        private IEnumerator RunDemoSequence()
        {
            isDemoRunning = true;
            UpdateDemoUI();

            UpdateDemoStatus("階層改装システムデモを開始します");
            yield return new WaitForSeconds(demoStepDelay);

            // 基本的な改装デモ
            if (showBasicRenovation)
            {
                yield return StartCoroutine(DemoBasicRenovation());
                yield return new WaitForSeconds(demoStepDelay);
            }

            // 経路検証デモ
            if (showPathValidation)
            {
                yield return StartCoroutine(DemoPathValidation());
                yield return new WaitForSeconds(demoStepDelay);
            }

            // エラーケースデモ
            if (showErrorCases)
            {
                yield return StartCoroutine(DemoErrorCases());
                yield return new WaitForSeconds(demoStepDelay);
            }

            // レイアウト保存デモ
            if (showLayoutSaving)
            {
                yield return StartCoroutine(DemoLayoutSaving());
                yield return new WaitForSeconds(demoStepDelay);
            }

            UpdateDemoStatus("デモが完了しました");
            isDemoRunning = false;
            UpdateDemoUI();
        }

        /// <summary>
        /// 基本的な改装機能のデモ
        /// </summary>
        private IEnumerator DemoBasicRenovation()
        {
            UpdateDemoStatus("基本的な改装機能をデモンストレーションします");
            yield return new WaitForSeconds(1f);

            // 改装可能かチェック
            bool canRenovate = FloorRenovationSystem.Instance.CanStartRenovation(demoFloorIndex);
            UpdateDemoStatus($"階層{demoFloorIndex}の改装可能性: {canRenovate}");
            yield return new WaitForSeconds(1f);

            if (canRenovate)
            {
                // 改装開始
                FloorRenovationSystem.Instance.StartRenovation(demoFloorIndex);
                UpdateDemoStatus("改装モードを開始しました");
                yield return new WaitForSeconds(1f);

                // 壁を配置
                Vector2[] wallPositions = {
                    new Vector2(1, 1),
                    new Vector2(2, 1),
                    new Vector2(3, 1)
                };

                foreach (Vector2 pos in wallPositions)
                {
                    bool placed = FloorRenovationSystem.Instance.PlaceWall(pos);
                    UpdateDemoStatus($"壁を配置: {pos} - {(placed ? "成功" : "失敗")}");
                    yield return new WaitForSeconds(0.5f);
                }

                // 壁を除去
                bool removed = FloorRenovationSystem.Instance.RemoveWall(wallPositions[1]);
                UpdateDemoStatus($"壁を除去: {wallPositions[1]} - {(removed ? "成功" : "失敗")}");
                yield return new WaitForSeconds(1f);

                // 改装終了
                FloorRenovationSystem.Instance.EndRenovation(true);
                UpdateDemoStatus("改装モードを終了しました（変更を保存）");
            }
            else
            {
                UpdateDemoStatus("階層が空でないため改装できません");
            }
        }

        /// <summary>
        /// 経路検証機能のデモ
        /// </summary>
        private IEnumerator DemoPathValidation()
        {
            UpdateDemoStatus("経路検証機能をデモンストレーションします");
            yield return new WaitForSeconds(1f);

            if (FloorRenovationSystem.Instance.CanStartRenovation(demoFloorIndex))
            {
                FloorRenovationSystem.Instance.StartRenovation(demoFloorIndex);
                UpdateDemoStatus("改装モードを開始しました");
                yield return new WaitForSeconds(1f);

                // 階段位置に壁を配置しようとする
                Floor floor = FloorSystem.Instance.GetFloor(demoFloorIndex);
                if (floor != null)
                {
                    bool blockedStair = FloorRenovationSystem.Instance.PlaceWall(floor.upStairPosition);
                    UpdateDemoStatus($"階段位置への壁配置: {(blockedStair ? "成功" : "失敗（期待通り）")}");
                    yield return new WaitForSeconds(1f);

                    // 経路を塞がない壁の配置
                    Vector2 safePos = new Vector2(-2, -2);
                    bool safePlacement = FloorRenovationSystem.Instance.PlaceWall(safePos);
                    UpdateDemoStatus($"安全な位置への壁配置: {(safePlacement ? "成功" : "失敗")}");
                    yield return new WaitForSeconds(1f);
                }

                FloorRenovationSystem.Instance.EndRenovation(false);
                UpdateDemoStatus("改装モードを終了しました（変更を破棄）");
            }
        }

        /// <summary>
        /// エラーケースのデモ
        /// </summary>
        private IEnumerator DemoErrorCases()
        {
            UpdateDemoStatus("エラーケースをデモンストレーションします");
            yield return new WaitForSeconds(1f);

            // 改装モードでない状態で壁配置を試行
            bool wallWithoutMode = FloorRenovationSystem.Instance.PlaceWall(Vector2.zero);
            UpdateDemoStatus($"改装モード外での壁配置: {(wallWithoutMode ? "成功" : "失敗（期待通り）")}");
            yield return new WaitForSeconds(1f);

            // 存在しない階層で改装開始を試行
            bool invalidFloor = FloorRenovationSystem.Instance.StartRenovation(999);
            UpdateDemoStatus($"無効な階層での改装開始: {(invalidFloor ? "成功" : "失敗（期待通り）")}");
            yield return new WaitForSeconds(1f);

            // 重複する改装モード開始を試行
            if (FloorRenovationSystem.Instance.CanStartRenovation(demoFloorIndex))
            {
                FloorRenovationSystem.Instance.StartRenovation(demoFloorIndex);
                bool duplicateStart = FloorRenovationSystem.Instance.StartRenovation(demoFloorIndex);
                UpdateDemoStatus($"重複する改装開始: {(duplicateStart ? "成功" : "失敗（期待通り）")}");
                yield return new WaitForSeconds(1f);

                FloorRenovationSystem.Instance.EndRenovation(false);
            }
        }

        /// <summary>
        /// レイアウト保存機能のデモ
        /// </summary>
        private IEnumerator DemoLayoutSaving()
        {
            UpdateDemoStatus("レイアウト保存機能をデモンストレーションします");
            yield return new WaitForSeconds(1f);

            if (FloorRenovationSystem.Instance.CanStartRenovation(demoFloorIndex))
            {
                FloorRenovationSystem.Instance.StartRenovation(demoFloorIndex);
                UpdateDemoStatus("改装モードを開始しました");
                yield return new WaitForSeconds(1f);

                // 複雑なレイアウトを作成
                Vector2[] complexLayout = {
                    new Vector2(0, 2),
                    new Vector2(1, 2),
                    new Vector2(2, 2),
                    new Vector2(2, 1),
                    new Vector2(2, 0),
                    new Vector2(1, 0),
                    new Vector2(0, 0),
                    new Vector2(-1, 0)
                };

                foreach (Vector2 pos in complexLayout)
                {
                    bool placed = FloorRenovationSystem.Instance.PlaceWall(pos);
                    UpdateDemoStatus($"複雑なレイアウト作成中: {pos}");
                    yield return new WaitForSeconds(0.3f);
                }

                UpdateDemoStatus("レイアウトを保存します");
                yield return new WaitForSeconds(1f);

                // 保存して終了
                FloorRenovationSystem.Instance.EndRenovation(true);
                UpdateDemoStatus("レイアウトを保存して改装モードを終了しました");

                yield return new WaitForSeconds(1f);

                // 保存されたレイアウトを確認
                Floor floor = FloorSystem.Instance.GetFloor(demoFloorIndex);
                if (floor != null)
                {
                    UpdateDemoStatus($"保存された壁の数: {floor.wallPositions.Count}");
                }
            }
        }

        /// <summary>
        /// デモステータスを更新
        /// </summary>
        private void UpdateDemoStatus(string status)
        {
            if (demoStatusText != null)
            {
                demoStatusText.text = status;
            }

            Debug.Log($"[RenovationDemo] {status}");
        }

        /// <summary>
        /// デモUIを更新
        /// </summary>
        private void UpdateDemoUI()
        {
            if (startDemoButton != null)
            {
                startDemoButton.interactable = !isDemoRunning;
            }

            if (stopDemoButton != null)
            {
                stopDemoButton.interactable = isDemoRunning;
            }

            if (demoUI != null)
            {
                demoUI.SetActive(true);
            }
        }

        /// <summary>
        /// デモ階層を空にする
        /// </summary>
        [ContextMenu("Clear Demo Floor")]
        public void ClearDemoFloor()
        {
            Floor floor = FloorSystem.Instance.GetFloor(demoFloorIndex);
            if (floor != null)
            {
                // モンスターを全て除去
                for (int i = floor.placedMonsters.Count - 1; i >= 0; i--)
                {
                    if (floor.placedMonsters[i] != null)
                    {
                        DestroyImmediate(floor.placedMonsters[i]);
                    }
                }
                floor.placedMonsters.Clear();

                // 侵入者を全て除去
                for (int i = floor.activeInvaders.Count - 1; i >= 0; i--)
                {
                    if (floor.activeInvaders[i] != null)
                    {
                        DestroyImmediate(floor.activeInvaders[i]);
                    }
                }
                floor.activeInvaders.Clear();

                UpdateDemoStatus($"デモ階層{demoFloorIndex}をクリアしました");
            }
        }
    }
}