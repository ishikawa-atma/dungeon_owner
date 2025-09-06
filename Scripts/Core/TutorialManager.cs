using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DungeonOwner.UI;
using DungeonOwner.Managers;

namespace DungeonOwner.Core
{
    /// <summary>
    /// チュートリアルシステムの管理
    /// 要件20.1, 20.2, 20.3, 20.4に対応
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }

        [Header("Tutorial Settings")]
        [SerializeField] private bool enableTutorial = true;
        [SerializeField] private float stepDelay = 1.0f;
        [SerializeField] private float highlightDuration = 2.0f;

        [Header("Tutorial UI")]
        [SerializeField] private GameObject tutorialOverlay;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private TMPro.TextMeshProUGUI instructionText;
        [SerializeField] private UnityEngine.UI.Button nextButton;
        [SerializeField] private UnityEngine.UI.Button skipButton;
        [SerializeField] private GameObject highlightEffect;

        // チュートリアルステップ
        private List<TutorialStep> tutorialSteps;
        private int currentStepIndex = 0;
        private bool isTutorialActive = false;
        private bool isStepInProgress = false;

        // イベント
        public event Action OnTutorialStarted;
        public event Action OnTutorialCompleted;
        public event Action OnTutorialSkipped;
        public event Action<int> OnStepCompleted;

        // 内部状態
        private Coroutine currentStepCoroutine;
        private GameObject currentHighlight;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeTutorial();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetupTutorialSteps();
            SetupUI();
            
            // 初回起動時のチュートリアル開始判定
            CheckFirstLaunch();
        }

        /// <summary>
        /// チュートリアル初期化
        /// </summary>
        private void InitializeTutorial()
        {
            tutorialSteps = new List<TutorialStep>();
            currentStepIndex = 0;
            isTutorialActive = false;
            isStepInProgress = false;
        }

        /// <summary>
        /// チュートリアルステップの設定
        /// 要件20.2: 段階的な操作説明
        /// </summary>
        private void SetupTutorialSteps()
        {
            tutorialSteps.Clear();

            // ステップ1: ゲーム概要説明
            tutorialSteps.Add(new TutorialStep
            {
                stepType = TutorialStepType.Information,
                title = "ダンジョンオーナーへようこそ！",
                instruction = "あなたはダンジョンの神です。侵入者からダンジョンコアを守りましょう。",
                targetUI = null,
                requiredAction = TutorialAction.Continue,
                isSkippable = true
            });

            // ステップ2: 画面説明
            tutorialSteps.Add(new TutorialStep
            {
                stepType = TutorialStepType.UIIntroduction,
                title = "画面の見方",
                instruction = "上部に金貨とリソースが表示されます。下部にはモンスター配置ボタンがあります。",
                targetUI = "ResourceDisplay",
                requiredAction = TutorialAction.Continue,
                isSkippable = true
            });

            // ステップ3: モンスター配置説明
            tutorialSteps.Add(new TutorialStep
            {
                stepType = TutorialStepType.ActionGuide,
                title = "モンスターを配置しよう",
                instruction = "下部のモンスターボタンをタップして、階層にモンスターを配置してみましょう。",
                targetUI = "MonsterPlacementButton",
                requiredAction = TutorialAction.PlaceMonster,
                isSkippable = false
            });

            // ステップ4: 侵入者出現説明
            tutorialSteps.Add(new TutorialStep
            {
                stepType = TutorialStepType.Information,
                title = "侵入者の出現",
                instruction = "侵入者は1階層の上り階段から出現し、ダンジョンコアを目指します。",
                targetUI = "Floor1",
                requiredAction = TutorialAction.Continue,
                isSkippable = true
            });

            // ステップ5: 戦闘システム説明
            tutorialSteps.Add(new TutorialStep
            {
                stepType = TutorialStepType.Information,
                title = "戦闘システム",
                instruction = "モンスターと侵入者が接触すると自動で戦闘が始まります。勝利すると金貨を獲得できます。",
                targetUI = null,
                requiredAction = TutorialAction.Continue,
                isSkippable = true
            });

            // ステップ6: 速度制御説明
            tutorialSteps.Add(new TutorialStep
            {
                stepType = TutorialStepType.ActionGuide,
                title = "ゲーム速度の調整",
                instruction = "右下のボタンでゲーム速度を変更できます。タップして試してみましょう。",
                targetUI = "SpeedControlButton",
                requiredAction = TutorialAction.ChangeSpeed,
                isSkippable = false
            });

            // ステップ7: チュートリアル完了
            tutorialSteps.Add(new TutorialStep
            {
                stepType = TutorialStepType.Completion,
                title = "チュートリアル完了！",
                instruction = "基本操作を覚えました。それでは実際にゲームを始めましょう！",
                targetUI = null,
                requiredAction = TutorialAction.Complete,
                isSkippable = false
            });

            Debug.Log($"Tutorial steps initialized: {tutorialSteps.Count} steps");
        }

        /// <summary>
        /// UI設定
        /// </summary>
        private void SetupUI()
        {
            // ボタンイベントの設定
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(OnNextButtonClicked);
            }

            if (skipButton != null)
            {
                skipButton.onClick.AddListener(OnSkipButtonClicked);
            }

            // 初期状態では非表示
            if (tutorialOverlay != null)
            {
                tutorialOverlay.SetActive(false);
            }
        }

        /// <summary>
        /// 初回起動判定
        /// 要件20.1: 初回起動時のチュートリアルモード
        /// </summary>
        private void CheckFirstLaunch()
        {
            bool isFirstLaunch = !PlayerPrefs.HasKey("TutorialCompleted");
            
            if (isFirstLaunch && enableTutorial)
            {
                // 少し遅延してチュートリアル開始
                StartCoroutine(StartTutorialDelayed(1.0f));
            }
        }

        /// <summary>
        /// 遅延チュートリアル開始
        /// </summary>
        private IEnumerator StartTutorialDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartTutorial();
        }

        /// <summary>
        /// チュートリアル開始
        /// </summary>
        public void StartTutorial()
        {
            if (!enableTutorial || isTutorialActive) return;

            Debug.Log("Starting tutorial");
            
            isTutorialActive = true;
            currentStepIndex = 0;
            
            // ゲーム状態をチュートリアルに変更
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Tutorial);
            }

            // チュートリアルUI表示
            ShowTutorialUI();
            
            // 最初のステップ開始
            StartCurrentStep();
            
            OnTutorialStarted?.Invoke();
        }

        /// <summary>
        /// チュートリアルUI表示
        /// </summary>
        private void ShowTutorialUI()
        {
            if (tutorialOverlay != null)
            {
                tutorialOverlay.SetActive(true);
            }

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(true);
            }
        }

        /// <summary>
        /// チュートリアルUI非表示
        /// </summary>
        private void HideTutorialUI()
        {
            if (tutorialOverlay != null)
            {
                tutorialOverlay.SetActive(false);
            }

            if (tutorialPanel != null)
            {
                tutorialPanel.SetActive(false);
            }

            // ハイライト効果も非表示
            HideHighlight();
        }

        /// <summary>
        /// 現在のステップ開始
        /// </summary>
        private void StartCurrentStep()
        {
            if (currentStepIndex >= tutorialSteps.Count)
            {
                CompleteTutorial();
                return;
            }

            TutorialStep currentStep = tutorialSteps[currentStepIndex];
            isStepInProgress = true;

            Debug.Log($"Starting tutorial step {currentStepIndex + 1}: {currentStep.title}");

            // UI更新
            UpdateStepUI(currentStep);

            // ハイライト表示
            ShowHighlight(currentStep.targetUI);

            // ステップタイプに応じた処理
            switch (currentStep.stepType)
            {
                case TutorialStepType.Information:
                    HandleInformationStep(currentStep);
                    break;
                case TutorialStepType.UIIntroduction:
                    HandleUIIntroductionStep(currentStep);
                    break;
                case TutorialStepType.ActionGuide:
                    HandleActionGuideStep(currentStep);
                    break;
                case TutorialStepType.Completion:
                    HandleCompletionStep(currentStep);
                    break;
            }
        }

        /// <summary>
        /// ステップUI更新
        /// </summary>
        private void UpdateStepUI(TutorialStep step)
        {
            if (instructionText != null)
            {
                instructionText.text = $"{step.title}\n\n{step.instruction}";
            }

            // スキップボタンの表示制御
            if (skipButton != null)
            {
                skipButton.gameObject.SetActive(step.isSkippable);
            }

            // 次へボタンの表示制御
            if (nextButton != null)
            {
                bool showNext = step.requiredAction == TutorialAction.Continue;
                nextButton.gameObject.SetActive(showNext);
            }
        }

        /// <summary>
        /// 情報ステップの処理
        /// </summary>
        private void HandleInformationStep(TutorialStep step)
        {
            // 情報表示のみ、ユーザーの「次へ」待ち
        }

        /// <summary>
        /// UI紹介ステップの処理
        /// </summary>
        private void HandleUIIntroductionStep(TutorialStep step)
        {
            // UI要素のハイライト表示
            if (!string.IsNullOrEmpty(step.targetUI))
            {
                HighlightUIElement(step.targetUI);
            }
        }

        /// <summary>
        /// アクションガイドステップの処理
        /// </summary>
        private void HandleActionGuideStep(TutorialStep step)
        {
            // 特定のアクションを待つ
            StartCoroutine(WaitForRequiredAction(step));
        }

        /// <summary>
        /// 完了ステップの処理
        /// </summary>
        private void HandleCompletionStep(TutorialStep step)
        {
            // チュートリアル完了準備
        }

        /// <summary>
        /// 必要なアクションを待つ
        /// </summary>
        private IEnumerator WaitForRequiredAction(TutorialStep step)
        {
            bool actionCompleted = false;

            while (!actionCompleted && isStepInProgress)
            {
                switch (step.requiredAction)
                {
                    case TutorialAction.PlaceMonster:
                        actionCompleted = CheckMonsterPlacement();
                        break;
                    case TutorialAction.ChangeSpeed:
                        actionCompleted = CheckSpeedChange();
                        break;
                }

                yield return new WaitForSeconds(0.1f);
            }

            if (actionCompleted)
            {
                yield return new WaitForSeconds(stepDelay);
                CompleteCurrentStep();
            }
        }

        /// <summary>
        /// モンスター配置チェック
        /// </summary>
        private bool CheckMonsterPlacement()
        {
            // MonsterPlacementManagerでモンスター配置を確認
            if (MonsterPlacementManager.Instance != null)
            {
                return MonsterPlacementManager.Instance.HasMonstersPlaced();
            }
            return false;
        }

        /// <summary>
        /// 速度変更チェック
        /// </summary>
        private bool CheckSpeedChange()
        {
            // GameManagerで速度変更を確認
            if (GameManager.Instance != null)
            {
                return GameManager.Instance.GameSpeed != 1.0f;
            }
            return false;
        }

        /// <summary>
        /// UI要素のハイライト
        /// </summary>
        private void HighlightUIElement(string elementName)
        {
            // UIManagerからUI要素を取得してハイライト
            if (UIManager.Instance != null)
            {
                RectTransform targetElement = UIManager.Instance.GetUIElement(elementName);
                if (targetElement != null)
                {
                    ShowHighlightAt(targetElement);
                }
            }
        }

        /// <summary>
        /// ハイライト表示
        /// </summary>
        private void ShowHighlight(string targetUI)
        {
            if (string.IsNullOrEmpty(targetUI)) return;

            HighlightUIElement(targetUI);
        }

        /// <summary>
        /// 指定位置にハイライト表示
        /// </summary>
        private void ShowHighlightAt(RectTransform target)
        {
            if (highlightEffect == null || target == null) return;

            // 既存のハイライトを削除
            HideHighlight();

            // 新しいハイライト作成
            currentHighlight = Instantiate(highlightEffect, target.parent);
            RectTransform highlightRect = currentHighlight.GetComponent<RectTransform>();
            
            if (highlightRect != null)
            {
                highlightRect.position = target.position;
                highlightRect.sizeDelta = target.sizeDelta * 1.2f; // 少し大きく
            }

            // ハイライトアニメーション開始
            StartCoroutine(AnimateHighlight());
        }

        /// <summary>
        /// ハイライトアニメーション
        /// </summary>
        private IEnumerator AnimateHighlight()
        {
            if (currentHighlight == null) yield break;

            float elapsed = 0f;
            Vector3 originalScale = currentHighlight.transform.localScale;

            while (elapsed < highlightDuration && currentHighlight != null)
            {
                float pulse = 1f + 0.2f * Mathf.Sin(elapsed * 4f);
                currentHighlight.transform.localScale = originalScale * pulse;
                
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (currentHighlight != null)
            {
                currentHighlight.transform.localScale = originalScale;
            }
        }

        /// <summary>
        /// ハイライト非表示
        /// </summary>
        private void HideHighlight()
        {
            if (currentHighlight != null)
            {
                Destroy(currentHighlight);
                currentHighlight = null;
            }
        }

        /// <summary>
        /// 現在のステップ完了
        /// 要件20.3: 次のステップに進行
        /// </summary>
        private void CompleteCurrentStep()
        {
            if (!isStepInProgress) return;

            Debug.Log($"Tutorial step {currentStepIndex + 1} completed");
            
            isStepInProgress = false;
            OnStepCompleted?.Invoke(currentStepIndex);

            // ハイライト非表示
            HideHighlight();

            // 次のステップへ
            currentStepIndex++;
            
            if (currentStepIndex < tutorialSteps.Count)
            {
                StartCoroutine(StartNextStepDelayed());
            }
            else
            {
                CompleteTutorial();
            }
        }

        /// <summary>
        /// 次のステップを遅延開始
        /// </summary>
        private IEnumerator StartNextStepDelayed()
        {
            yield return new WaitForSeconds(stepDelay);
            StartCurrentStep();
        }

        /// <summary>
        /// チュートリアル完了
        /// 要件20.4: 通常ゲームモードに移行
        /// </summary>
        private void CompleteTutorial()
        {
            Debug.Log("Tutorial completed");
            
            isTutorialActive = false;
            isStepInProgress = false;

            // チュートリアル完了フラグ保存
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            // UI非表示
            HideTutorialUI();

            // ゲーム状態を通常プレイに変更
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Playing);
                GameManager.Instance.StartGame();
            }

            OnTutorialCompleted?.Invoke();
        }

        /// <summary>
        /// チュートリアルスキップ
        /// 要件20.5: スキップ機能
        /// </summary>
        public void SkipTutorial()
        {
            if (!isTutorialActive) return;

            Debug.Log("Tutorial skipped");
            
            // 現在のステップを中断
            if (currentStepCoroutine != null)
            {
                StopCoroutine(currentStepCoroutine);
            }

            isTutorialActive = false;
            isStepInProgress = false;

            // チュートリアル完了フラグ保存
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();

            // UI非表示
            HideTutorialUI();

            // ゲーム状態を通常プレイに変更
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.Playing);
                GameManager.Instance.StartGame();
            }

            OnTutorialSkipped?.Invoke();
        }

        /// <summary>
        /// 次へボタンクリック
        /// </summary>
        private void OnNextButtonClicked()
        {
            if (!isStepInProgress) return;

            TutorialStep currentStep = tutorialSteps[currentStepIndex];
            if (currentStep.requiredAction == TutorialAction.Continue)
            {
                CompleteCurrentStep();
            }
        }

        /// <summary>
        /// スキップボタンクリック
        /// </summary>
        private void OnSkipButtonClicked()
        {
            SkipTutorial();
        }

        /// <summary>
        /// チュートリアル状態確認
        /// </summary>
        public bool IsTutorialActive()
        {
            return isTutorialActive;
        }

        /// <summary>
        /// チュートリアル完了状態確認
        /// </summary>
        public bool IsTutorialCompleted()
        {
            return PlayerPrefs.GetInt("TutorialCompleted", 0) == 1;
        }

        /// <summary>
        /// チュートリアルリセット（デバッグ用）
        /// </summary>
        [ContextMenu("Reset Tutorial")]
        public void ResetTutorial()
        {
            PlayerPrefs.DeleteKey("TutorialCompleted");
            PlayerPrefs.Save();
            Debug.Log("Tutorial reset - will show on next launch");
        }

        private void OnDestroy()
        {
            // コルーチン停止
            if (currentStepCoroutine != null)
            {
                StopCoroutine(currentStepCoroutine);
            }

            // ハイライト削除
            HideHighlight();
        }
    }

    /// <summary>
    /// チュートリアルステップデータ
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        public TutorialStepType stepType;
        public string title;
        [TextArea(3, 5)]
        public string instruction;
        public string targetUI;
        public TutorialAction requiredAction;
        public bool isSkippable;
    }

    /// <summary>
    /// チュートリアルステップタイプ
    /// </summary>
    public enum TutorialStepType
    {
        Information,        // 情報表示
        UIIntroduction,     // UI紹介
        ActionGuide,        // アクション指導
        Completion          // 完了
    }

    /// <summary>
    /// チュートリアルアクション
    /// </summary>
    public enum TutorialAction
    {
        Continue,           // 次へボタン
        PlaceMonster,       // モンスター配置
        ChangeSpeed,        // 速度変更
        Complete            // 完了
    }
}