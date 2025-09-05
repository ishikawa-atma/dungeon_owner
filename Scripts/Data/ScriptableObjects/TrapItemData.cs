using UnityEngine;
using Scripts.Data.Enums;

namespace Scripts.Data.ScriptableObjects
{
    [CreateAssetMenu(fileName = "TrapItemData", menuName = "DungeonOwner/TrapItemData")]
    public class TrapItemData : ScriptableObject
    {
        [Header("基本情報")]
        public TrapItemType type;
        public string itemName;
        public string description;
        public Sprite icon;
        
        [Header("効果パラメータ")]
        public float damage;
        public float effectDuration;
        public float range;
        public float dropRate = 0.1f; // 10%のドロップ率
        
        [Header("視覚効果")]
        public GameObject effectPrefab;
        public AudioClip useSound;
        public Color effectColor = Color.red;
        
        [Header("使用制限")]
        public int maxStackSize = 5;
        public float cooldownTime = 2f;
    }
}