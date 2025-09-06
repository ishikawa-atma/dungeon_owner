using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DungeonOwner.UI
{
    public class ResourceDisplayUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private TextMeshProUGUI foodText;
        
        private DungeonOwner.Managers.ResourceManager resourceManager;
        
        private void Start()
        {
            resourceManager = FindObjectOfType<DungeonOwner.Managers.ResourceManager>();
            UpdateDisplay();
        }
        
        private void Update()
        {
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (resourceManager == null) return;
            
            if (goldText != null)
                goldText.text = $"Gold: {resourceManager.GetGold()}";
                
            if (manaText != null)
                manaText.text = $"Mana: {resourceManager.GetMana()}";
                
            if (foodText != null)
                foodText.text = $"Food: {resourceManager.GetFood()}";
        }
    }
}