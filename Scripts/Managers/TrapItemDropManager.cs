using UnityEngine;
using System.Collections.Generic;
using Scripts.Data.ScriptableObjects;
using Scripts.Data.Enums;
using Scripts.Interfaces;

namespace Scripts.Managers
{
    /// <summary>
    /// 罠アイテムドロップ管理システム
    /// 要件13.1: 侵入者撃破時の低確率ドロップを管理
    /// </summary>
    public class TrapItemDropManager : MonoBehaviour
    {
        [Header("ドロップ設定")]
        [SerializeField] private TrapItemData[] availableTrapItems;
        [SerializeField] private float baseDropRate = 0.15f; // 基本ドロップ率15%
        [SerializeField] private float levelBonusRate = 0.01f; // レベル1につき1%のボーナス
        [SerializeField] private float eliteDropMultiplier = 2.0f; // エリート侵入者のドロップ率倍率
        
        [Header("視覚効果")]
        [SerializeField] private GameObject dropEffectPrefab;
        [SerializeField] private AudioClip dropSound;
        [SerializeField] private float effectDuration = 2f;
        
        public static TrapItemDropManager Instance { get; private set; }
        
        // ドロップ統計
        private Dictionary<TrapItemType, int> dropCounts = new Dictionary<TrapItemType, int>();
        private int totalDrops = 0;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDropCounts();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // 利用可能な罠アイテムが設定されていない場合、Resourcesから読み込み
            if (availableTrapItems == null || availableTrapItems.Length == 0)
            {
                LoadTrapItemsFromResources();
            }
        }
        
        /// <summary>
        /// ドロップカウントを初期化
        /// </summary>
        private void InitializeDropCounts()
        {
            foreach (TrapItemType itemType in System.Enum.GetValues(typeof(TrapItemType)))
            {
                dropCounts[itemType] = 0;
            }
        }
        
        /// <summary>
        /// Resourcesから罠アイテムデータを読み込み
        /// </summary>
        private void LoadTrapItemsFromResources()
        {
            availableTrapItems = Resources.LoadAll<TrapItemData>("Data/TrapItems");
            
            if (availableTrapItems.Length == 0)
            {
                Debug.LogWarning("罠アイテムデータが見つかりません。Resources/Data/TrapItemsフォルダにTrapItemDataを配置してください。");
            }
        }
        
        /// <summary>
        /// 侵入者撃破時のドロップ処理
        /// 要件13.1: 侵入者を撃破すると低確率で罠アイテムをドロップ
        /// </summary>
        public void ProcessDrop(IInvader invader)
        {
            if (invader == null || availableTrapItems.Length == 0) return;
            
            // ドロップ率計算
            float dropChance = CalculateDropChance(invader);
            
            // ドロップ判定
            if (Random.value <= dropChance)
            {
                // ドロップするアイテムを選択
                TrapItemData droppedItem = SelectDropItem(invader);
                
                if (droppedItem != null)
                {
                    // インベントリに追加
                    bool added = InventoryManager.Instance?.AddItem(droppedItem, 1) ?? false;
                    
                    if (added)
                    {
                        // ドロップ効果を表示
                        ShowDropEffect(invader.Position, droppedItem);
                        
                        // 統計更新
                        UpdateDropStatistics(droppedItem.type);
                        
                        Debug.Log($"罠アイテム {droppedItem.itemName} がドロップしました！ (確率: {dropChance:P1})");
                    }
                    else
                    {
                        Debug.Log("インベントリが満杯のため、罠アイテムを取得できませんでした");
                    }
                }
            }
        }
        
        /// <summary>
        /// ドロップ率を計算
        /// </summary>
        private float CalculateDropChance(IInvader invader)
        {
            float dropChance = baseDropRate;
            
            // レベルボーナス
            dropChance += invader.Level * levelBonusRate;
            
            // 侵入者ランクによるボーナス（InvaderDataから取得）
            var invaderComponent = invader as MonoBehaviour;
            if (invaderComponent != null)
            {
                var invaderData = GetInvaderData(invader.Type);
                if (invaderData != null)
                {
                    // エリート以上の侵入者はドロップ率が高い
                    if (invaderData.rank == InvaderRank.Elite || invaderData.rank == InvaderRank.Champion)
                    {
                        dropChance *= eliteDropMultiplier;
                    }
                }
            }
            
            // パーティメンバーの場合はボーナス
            if (invader.Party != null && invader.Party.Members.Count > 1)
            {
                dropChance *= 1.2f; // 20%ボーナス
            }
            
            return Mathf.Clamp01(dropChance);
        }
        
        /// <summary>
        /// ドロップアイテムを選択
        /// </summary>
        private TrapItemData SelectDropItem(IInvader invader)
        {
            if (availableTrapItems.Length == 0) return null;
            
            // 侵入者のレベルに応じて重み付け
            var weightedItems = new List<(TrapItemData item, float weight)>();
            
            foreach (var item in availableTrapItems)
            {
                float weight = 1f;
                
                // レベルに応じた重み調整
                if (invader.Level >= 5)
                {
                    // 高レベル侵入者からは強力な罠アイテムがドロップしやすい
                    switch (item.type)
                    {
                        case TrapItemType.LightningTrap:
                        case TrapItemType.FireTrap:
                            weight = 2f;
                            break;
                        case TrapItemType.IceTrap:
                        case TrapItemType.PoisonTrap:
                            weight = 1.5f;
                            break;
                        case TrapItemType.SpikeTrap:
                            weight = 0.8f;
                            break;
                    }
                }
                
                // 侵入者タイプに応じた重み調整
                switch (invader.Type)
                {
                    case InvaderType.Mage:
                    case InvaderType.Archmage:
                        if (item.type == TrapItemType.LightningTrap) weight *= 1.5f;
                        break;
                    case InvaderType.Warrior:
                    case InvaderType.Knight:
                        if (item.type == TrapItemType.SpikeTrap) weight *= 1.5f;
                        break;
                    case InvaderType.Rogue:
                    case InvaderType.Assassin:
                        if (item.type == TrapItemType.PoisonTrap) weight *= 1.5f;
                        break;
                    case InvaderType.Cleric:
                    case InvaderType.HighPriest:
                        if (item.type == TrapItemType.FireTrap) weight *= 1.5f;
                        break;
                }
                
                weightedItems.Add((item, weight));
            }
            
            // 重み付きランダム選択
            return SelectWeightedRandom(weightedItems);
        }
        
        /// <summary>
        /// 重み付きランダム選択
        /// </summary>
        private TrapItemData SelectWeightedRandom(List<(TrapItemData item, float weight)> weightedItems)
        {
            float totalWeight = 0f;
            foreach (var (item, weight) in weightedItems)
            {
                totalWeight += weight;
            }
            
            float randomValue = Random.value * totalWeight;
            float currentWeight = 0f;
            
            foreach (var (item, weight) in weightedItems)
            {
                currentWeight += weight;
                if (randomValue <= currentWeight)
                {
                    return item;
                }
            }
            
            // フォールバック
            return weightedItems[0].item;
        }
        
        /// <summary>
        /// ドロップ効果を表示
        /// </summary>
        private void ShowDropEffect(Vector2 position, TrapItemData item)
        {
            // 視覚効果
            if (dropEffectPrefab != null)
            {
                var effect = Instantiate(dropEffectPrefab, position, Quaternion.identity);
                
                // エフェクトの色をアイテムに応じて変更
                var particleSystem = effect.GetComponent<ParticleSystem>();
                if (particleSystem != null)
                {
                    var main = particleSystem.main;
                    main.startColor = GetItemColor(item.type);
                }
                
                Destroy(effect, effectDuration);
            }
            
            // 音効果
            if (dropSound != null)
            {
                AudioSource.PlayClipAtPoint(dropSound, position);
            }
            
            // UI通知（将来的に実装）
            // UIManager.Instance?.ShowItemDropNotification(item);
        }
        
        /// <summary>
        /// アイテムタイプに応じた色を取得
        /// </summary>
        private Color GetItemColor(TrapItemType itemType)
        {
            return itemType switch
            {
                TrapItemType.SpikeTrap => Color.gray,
                TrapItemType.FireTrap => Color.red,
                TrapItemType.IceTrap => Color.cyan,
                TrapItemType.PoisonTrap => Color.green,
                TrapItemType.LightningTrap => Color.yellow,
                _ => Color.white
            };
        }
        
        /// <summary>
        /// 侵入者データを取得
        /// </summary>
        private InvaderData GetInvaderData(InvaderType invaderType)
        {
            // DataManagerから取得（実装されている場合）
            if (DataManager.Instance != null)
            {
                return DataManager.Instance.GetInvaderData(invaderType);
            }
            
            // フォールバック: Resourcesから直接読み込み
            var invaderDatas = Resources.LoadAll<InvaderData>("Data/Invaders");
            return System.Array.Find(invaderDatas, data => data.type == invaderType);
        }
        
        /// <summary>
        /// ドロップ統計を更新
        /// </summary>
        private void UpdateDropStatistics(TrapItemType itemType)
        {
            dropCounts[itemType]++;
            totalDrops++;
        }
        
        /// <summary>
        /// ドロップ統計を取得
        /// </summary>
        public Dictionary<TrapItemType, int> GetDropStatistics()
        {
            return new Dictionary<TrapItemType, int>(dropCounts);
        }
        
        /// <summary>
        /// 総ドロップ数を取得
        /// </summary>
        public int GetTotalDrops()
        {
            return totalDrops;
        }
        
        /// <summary>
        /// ドロップ率設定を変更
        /// </summary>
        public void SetDropRates(float baseRate, float levelBonus, float eliteMultiplier)
        {
            baseDropRate = Mathf.Clamp01(baseRate);
            levelBonusRate = Mathf.Clamp01(levelBonus);
            eliteDropMultiplier = Mathf.Max(1f, eliteMultiplier);
        }
        
        /// <summary>
        /// 利用可能な罠アイテムを設定
        /// </summary>
        public void SetAvailableTrapItems(TrapItemData[] items)
        {
            availableTrapItems = items;
        }
    }
}