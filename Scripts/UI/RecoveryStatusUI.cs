using UnityEngine;
using UnityEngine.UI;
using DungeonOwner.Interfaces;
using DungeonOwner.Data;

namespace DungeonOwner.UI
{
    /// <summary>
    /// モンスターの回復状況を表示するUIコンポーネント
    /// </summary>
    public class RecoveryStatusUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider manaBar;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image manaFill;
        [SerializeField] private Text recoveryStatusText;
        [SerializeField] private Image recoveryIcon;
        
        [Header("色設定")]
        [SerializeField] private Color healthColor = Color.red;
        [SerializeField] private Color manaColor = Color.blue;
        [SerializeField] private Color floorRecoveryColor = Color.green;
        [SerializeField] private Color shelterRecoveryColor = Color.cyan;
        
        [Header("アニメーション")]
        [SerializeField] private float updateSpeed = 2f;
        [SerializeField] private bool smoothTransition = true;
        
        private IMonster targetMonster;
        private Canvas uiCanvas;
        private Camera mainCamera;
        private float targetHealthRatio;
        private float targetManaRatio;
        private float currentHealthRatio;
        private float currentManaRatio;
        
        private void Awake()
        {
            uiCanvas = GetComponent<Canvas>();
            if (uiCanvas == null)
            {
                uiCanvas = gameObject.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.WorldSpace;
                uiCanvas.worldCamera = Camera.main;
            }
            
            mainCamera = Camera.main;
            
            // UI要素の初期設定
            SetupUIElements();
        }
        
        private void Start()
        {
            // カメラが見つからない場合の対処
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }
        
        private void Update()
        {
            if (targetMonster == null) return;
            
            UpdateHealthAndMana();
            UpdateRecoveryStatus();
            UpdateUIPosition();
        }
        
        /// <summary>
        /// 監視対象のモンスターを設定
        /// </summary>
        public void SetTargetMonster(IMonster monster)
        {
            targetMonster = monster;
            
            if (monster != null)
            {
                // 初期値を設定
                currentHealthRatio = targetHealthRatio = monster.Health / monster.MaxHealth;
                currentManaRatio = targetManaRatio = monster.Mana / monster.MaxMana;
                
                UpdateUI();
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// UI要素の初期設定
        /// </summary>
        private void SetupUIElements()
        {
            // ヘルスバーの設定
            if (healthBar != null && healthFill != null)
            {
                healthFill.color = healthColor;
                healthBar.value = 1f;
            }
            
            // マナバーの設定
            if (manaBar != null && manaFill != null)
            {
                manaFill.color = manaColor;
                manaBar.value = 1f;
            }
            
            // 初期状態では非表示
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// HP/MPの更新処理
        /// </summary>
        private void UpdateHealthAndMana()
        {
            if (targetMonster.MaxHealth > 0)
            {
                targetHealthRatio = targetMonster.Health / targetMonster.MaxHealth;
            }
            
            if (targetMonster.MaxMana > 0)
            {
                targetManaRatio = targetMonster.Mana / targetMonster.MaxMana;
            }
            
            // スムーズな遷移
            if (smoothTransition)
            {
                currentHealthRatio = Mathf.Lerp(currentHealthRatio, targetHealthRatio, updateSpeed * Time.deltaTime);
                currentManaRatio = Mathf.Lerp(currentManaRatio, targetManaRatio, updateSpeed * Time.deltaTime);
            }
            else
            {
                currentHealthRatio = targetHealthRatio;
                currentManaRatio = targetManaRatio;
            }
            
            UpdateUI();
        }
        
        /// <summary>
        /// 回復状況の更新
        /// </summary>
        private void UpdateRecoveryStatus()
        {
            bool isRecovering = IsRecovering();
            bool isInShelter = targetMonster.State == MonsterState.InShelter;
            
            // 回復アイコンの表示/非表示
            if (recoveryIcon != null)
            {
                recoveryIcon.gameObject.SetActive(isRecovering);
                
                if (isRecovering)
                {
                    Color iconColor = isInShelter ? shelterRecoveryColor : floorRecoveryColor;
                    recoveryIcon.color = iconColor;
                    
                    // 回復アイコンのパルス効果
                    float pulse = Mathf.Sin(Time.time * 3f) * 0.2f + 0.8f;
                    recoveryIcon.transform.localScale = Vector3.one * pulse;
                }
            }
            
            // 回復状況テキスト
            if (recoveryStatusText != null)
            {
                if (isRecovering)
                {
                    string statusText = isInShelter ? "高速回復中" : "回復中";
                    recoveryStatusText.text = statusText;
                    recoveryStatusText.color = isInShelter ? shelterRecoveryColor : floorRecoveryColor;
                }
                else
                {
                    recoveryStatusText.text = "";
                }
            }
        }
        
        /// <summary>
        /// UIの位置を更新（モンスターの上に表示）
        /// </summary>
        private void UpdateUIPosition()
        {
            if (mainCamera == null || targetMonster == null) return;
            
            Vector3 worldPosition = targetMonster.Position + Vector2.up * 1.5f;
            transform.position = worldPosition;
            
            // カメラの方向を向く
            if (uiCanvas.renderMode == RenderMode.WorldSpace)
            {
                transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                                mainCamera.transform.rotation * Vector3.up);
            }
        }
        
        /// <summary>
        /// UIの表示を更新
        /// </summary>
        private void UpdateUI()
        {
            if (healthBar != null)
            {
                healthBar.value = currentHealthRatio;
            }
            
            if (manaBar != null)
            {
                manaBar.value = currentManaRatio;
            }
        }
        
        /// <summary>
        /// 回復中かどうかを判定
        /// </summary>
        private bool IsRecovering()
        {
            if (targetMonster == null) return false;
            
            bool healthRecovering = targetMonster.Health < targetMonster.MaxHealth;
            bool manaRecovering = targetMonster.Mana < targetMonster.MaxMana;
            
            return healthRecovering || manaRecovering;
        }
        
        /// <summary>
        /// UIの表示/非表示を切り替え
        /// </summary>
        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible && targetMonster != null);
        }
        
        /// <summary>
        /// 回復率の情報を表示
        /// </summary>
        public void ShowRecoveryRate(float rate, bool isInShelter)
        {
            if (recoveryStatusText != null)
            {
                string rateText = $"回復速度: {rate:F1}/秒";
                if (isInShelter)
                {
                    rateText += " (退避中)";
                }
                recoveryStatusText.text = rateText;
            }
        }
    }
}