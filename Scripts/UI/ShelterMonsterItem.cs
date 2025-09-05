using UnityEngine;
using UnityEngine.UI;

namespace DungeonOwner.UI
{
    /// <summary>
    /// 退避スポット内のモンスターアイテムUI
    /// </summary>
    public class ShelterMonsterItem : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private Text monsterNameText;
        [SerializeField] private Text levelText;
        [SerializeField] private Text healthText;
        [SerializeField] private Text sellPriceText;
        [SerializeField] private Image monsterIcon;
        [SerializeField] private Button selectButton;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider manaSlider;
        
        private IMonster monster;
        
        public System.Action<IMonster> OnMonsterSelected;
        
        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(SelectMonster);
            }
        }
        
        /// <summary>
        /// モンスター情報を設定
        /// </summary>
        public void SetMonster(IMonster monster)
        {
            this.monster = monster;
            UpdateDisplay();
        }
        
        /// <summary>
        /// 表示を更新
        /// </summary>
        private void UpdateDisplay()
        {
            if (monster == null) return;
            
            if (monsterNameText != null)
            {
                monsterNameText.text = monster.Type.ToString();
            }
            
            if (levelText != null)
            {
                levelText.text = $"Lv.{monster.Level}";
            }
            
            if (healthText != null)
            {
                healthText.text = $"HP: {monster.Health:F0}/{monster.MaxHealth:F0}";
            }
            
            if (healthSlider != null)
            {
                healthSlider.value = monster.Health / monster.MaxHealth;
            }
            
            if (manaSlider != null)
            {
                manaSlider.value = monster.Mana / monster.MaxMana;
            }
            
            // 売却価格を表示
            if (sellPriceText != null)
            {
                int sellPrice = CalculateSellPrice();
                sellPriceText.text = $"売却: {sellPrice}G";
            }
            
            // アイコンの設定（実装されている場合）
            if (monsterIcon != null)
            {
                // TODO: モンスタータイプに応じたアイコンを設定
            }
        }
        
        /// <summary>
        /// 売却価格を計算
        /// </summary>
        private int CalculateSellPrice()
        {
            if (monster == null) return 0;
            
            var shelterManager = FindObjectOfType<DungeonOwner.Managers.ShelterManager>();
            if (shelterManager != null)
            {
                return shelterManager.CalculateMonsterSellPrice(monster);
            }
            
            return 0;
        }
        
        /// <summary>
        /// モンスターを選択
        /// </summary>
        private void SelectMonster()
        {
            OnMonsterSelected?.Invoke(monster);
        }
        
        /// <summary>
        /// 定期的に表示を更新（回復状況の反映）
        /// </summary>
        private void Update()
        {
            if (monster != null)
            {
                UpdateDisplay();
            }
        }
    }
}