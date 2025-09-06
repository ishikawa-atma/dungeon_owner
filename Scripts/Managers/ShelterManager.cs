using System.Collections.Generic;
using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Managers
{
    /// <summary>
    /// 退避スポットシステムを管理するクラス
    /// モンスターの退避、配置、売却機能を提供
    /// </summary>
    public class ShelterManager : MonoBehaviour
    {
        public static ShelterManager Instance { get; private set; }
        
        [Header("退避スポット設定")]
        [SerializeField] private int maxCapacity = 50; // 最大収容数
        [SerializeField] private float recoveryRate = 2.0f; // HP/MP回復速度倍率
        
        private List<IMonster> shelterMonsters = new List<IMonster>();
        private MonsterPlacementManager placementManager;
        private FloorSystem floorSystem;
        
        public List<IMonster> ShelterMonsters => shelterMonsters;
        public int MaxCapacity => maxCapacity;
        public int CurrentCount => shelterMonsters.Count;
        public bool IsFull => shelterMonsters.Count >= maxCapacity;
        
        // イベント
        public System.Action<IMonster> OnMonsterSheltered;
        public System.Action<IMonster> OnMonsterDeployed;
        public System.Action<IMonster> OnMonsterSold;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeManager()
        {
            placementManager = FindObjectOfType<MonsterPlacementManager>();
            floorSystem = FindObjectOfType<FloorSystem>();
        }
        
        /// <summary>
        /// モンスターを退避スポットに移動できるかチェック
        /// </summary>
        public bool CanShelter(IMonster monster)
        {
            if (monster == null) return false;
            if (IsFull) return false;
            if (shelterMonsters.Contains(monster)) return false;
            
            return true;
        }
        
        /// <summary>
        /// モンスターを退避スポットに移動
        /// 要件7.2: 退避スポットへの移動で階層からモンスターを除去
        /// </summary>
        public bool ShelterMonster(IMonster monster)
        {
            if (!CanShelter(monster))
            {
                Debug.LogWarning($"モンスター {monster.Type} を退避できません");
                return false;
            }
            
            // 階層からモンスターを除去
            if (placementManager != null)
            {
                placementManager.RemoveMonster(monster);
            }
            
            // 退避スポットに追加
            shelterMonsters.Add(monster);
            
            // モンスターの状態を退避中に設定
            if (monster is MonoBehaviour monsterMB)
            {
                monsterMB.gameObject.SetActive(false);
            }
            
            OnMonsterSheltered?.Invoke(monster);
            Debug.Log($"モンスター {monster.Type} を退避スポットに移動しました");
            
            return true;
        }
        
        /// <summary>
        /// 退避スポットから階層にモンスターを配置
        /// 要件7.4: 退避スポットから階層への配置時にモンスター配置制限を確認
        /// </summary>
        public bool DeployMonster(IMonster monster, int floorIndex, Vector2 position)
        {
            if (!shelterMonsters.Contains(monster))
            {
                Debug.LogWarning("指定されたモンスターは退避スポットにいません");
                return false;
            }
            
            // 配置可能かチェック
            if (floorSystem != null && !floorSystem.CanPlaceMonster(floorIndex, position))
            {
                Debug.LogWarning($"階層 {floorIndex} の位置 {position} にモンスターを配置できません");
                return false;
            }
            
            // 退避スポットから除去
            shelterMonsters.Remove(monster);
            
            // 階層に配置
            if (placementManager != null)
            {
                placementManager.PlaceMonster(monster, floorIndex, position);
            }
            
            // モンスターをアクティブに
            if (monster is MonoBehaviour monsterMB)
            {
                monsterMB.gameObject.SetActive(true);
                monsterMB.transform.position = position;
            }
            
            OnMonsterDeployed?.Invoke(monster);
            Debug.Log($"モンスター {monster.Type} を階層 {floorIndex} に配置しました");
            
            return true;
        }
        
        /// <summary>
        /// 退避スポット内のモンスターを売却
        /// 要件5.1: 退避スポット内のモンスターを選択して売却オプションを表示
        /// 要件5.2: 購入価格の一定割合を金貨で返還
        /// 要件5.3: モンスターを完全に除去
        /// </summary>
        public bool SellMonster(IMonster monster)
        {
            if (!CanSellMonster(monster))
            {
                return false;
            }
            
            // 売却価格を計算して金貨を付与
            int sellPrice = CalculateMonsterSellPrice(monster);
            var resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.AddGold(sellPrice);
            }
            
            // 退避スポットから除去
            shelterMonsters.Remove(monster);
            
            // モンスターオブジェクトを破棄
            if (monster is MonoBehaviour monsterMB)
            {
                Destroy(monsterMB.gameObject);
            }
            
            OnMonsterSold?.Invoke(monster);
            Debug.Log($"モンスター {monster.Type} を {sellPrice} 金貨で売却しました");
            
            return true;
        }
        
        /// <summary>
        /// モンスターを売却できるかチェック
        /// 要件5.4: 階層配置中のモンスターは売却を無効化
        /// 要件5.5: 自キャラクターは売却を無効化
        /// </summary>
        public bool CanSellMonster(IMonster monster)
        {
            if (monster == null)
            {
                Debug.LogWarning("モンスターが null です");
                return false;
            }
            
            if (!shelterMonsters.Contains(monster))
            {
                Debug.LogWarning("指定されたモンスターは退避スポットにいません");
                return false;
            }
            
            // 自キャラクターは売却不可
            if (IsPlayerCharacter(monster))
            {
                Debug.LogWarning("自キャラクターは売却できません");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// モンスターの売却価格を計算
        /// 要件5.2: 購入価格の一定割合を金貨で返還
        /// </summary>
        public int CalculateMonsterSellPrice(IMonster monster)
        {
            if (monster == null) return 0;
            
            // DataManagerからモンスターデータを取得
            var dataManager = DataManager.Instance;
            if (dataManager != null)
            {
                var monsterData = dataManager.GetMonsterData(monster.Type);
                if (monsterData != null)
                {
                    // MonsterDataの売却価格メソッドを使用
                    int basePrice = monsterData.GetSellPrice();
                    
                    // レベルによる補正（レベル1以上で10%ずつ増加）
                    float levelMultiplier = 1f + (monster.Level - 1) * 0.1f;
                    
                    return Mathf.RoundToInt(basePrice * levelMultiplier);
                }
            }
            
            // フォールバック: ResourceManagerの計算を使用
            var resourceManager = FindObjectOfType<ResourceManager>();
            if (resourceManager != null)
            {
                return resourceManager.CalculateMonsterSellPrice(monster);
            }
            
            // 最終フォールバック
            return 50;
        }
        
        /// <summary>
        /// モンスターが自キャラクターかどうかチェック
        /// </summary>
        private bool IsPlayerCharacter(IMonster monster)
        {
            // BasePlayerCharacterクラスかどうかで判定
            return monster is DungeonOwner.PlayerCharacters.BasePlayerCharacter;
        }
        
        /// <summary>
        /// 退避スポット内モンスターのHP/MP回復処理
        /// 要件7.5: 退避スポット内のモンスターは時間経過でHP/MPを高速回復
        /// </summary>
        private void Update()
        {
            UpdateRecovery(Time.deltaTime);
        }
        
        public void UpdateRecovery(float deltaTime)
        {
            foreach (var monster in shelterMonsters)
            {
                if (monster == null) continue;
                
                // HP回復
                if (monster.Health < monster.MaxHealth)
                {
                    float healAmount = monster.MaxHealth * 0.1f * recoveryRate * deltaTime;
                    monster.Heal(healAmount);
                }
                
                // MP回復
                if (monster.Mana < monster.MaxMana)
                {
                    float manaAmount = monster.MaxMana * 0.1f * recoveryRate * deltaTime;
                    
                    // BaseMonsterクラスのRestoreManaメソッドを使用
                    if (monster is Monsters.BaseMonster baseMonster)
                    {
                        baseMonster.RestoreMana(manaAmount);
                    }
                }
            }
        }
        
        /// <summary>
        /// 配置可能な階層のリストを取得
        /// 要件7.3: 退避スポット内モンスター選択時に配置可能階層を表示
        /// </summary>
        public List<int> GetAvailableFloors()
        {
            List<int> availableFloors = new List<int>();
            
            if (floorSystem == null) return availableFloors;
            
            for (int i = 0; i < floorSystem.MaxFloors; i++)
            {
                if (floorSystem.HasAvailableSpace(i))
                {
                    availableFloors.Add(i);
                }
            }
            
            return availableFloors;
        }
        
        /// <summary>
        /// 指定階層の配置可能位置を取得
        /// </summary>
        public List<Vector2> GetAvailablePositions(int floorIndex)
        {
            if (floorSystem == null) return new List<Vector2>();
            
            return floorSystem.GetAvailablePositions(floorIndex);
        }
        
        /// <summary>
        /// セーブデータからモンスターを復元
        /// </summary>
        public void RestoreMonster(MonsterSaveData monsterData)
        {
            if (monsterData == null || !monsterData.isInShelter) return;

            var dataManager = DataManager.Instance;
            if (dataManager == null) return;

            var data = dataManager.GetMonsterData(monsterData.type);
            if (data == null || data.prefab == null)
            {
                Debug.LogError($"Cannot restore shelter monster: data not found for {monsterData.type}");
                return;
            }

            // モンスターオブジェクトを生成（非アクティブ状態）
            GameObject monsterObj = Instantiate(data.prefab);
            monsterObj.name = $"{monsterData.type}_shelter_restored";
            monsterObj.SetActive(false);

            // モンスターコンポーネントを取得・設定
            IMonster monster = monsterObj.GetComponent<IMonster>();
            if (monster == null)
            {
                Debug.LogError($"Restored shelter monster prefab for {monsterData.type} does not have IMonster component");
                Destroy(monsterObj);
                return;
            }

            // BaseMonsterコンポーネントにデータを設定
            var baseMonster = monsterObj.GetComponent<Monsters.BaseMonster>();
            if (baseMonster != null)
            {
                baseMonster.SetMonsterData(data);
                baseMonster.Level = monsterData.level;
                baseMonster.SetHealth(monsterData.currentHealth);
                baseMonster.SetMana(monsterData.currentMana);
            }

            // 退避スポットに追加
            shelterMonsters.Add(monster);

            Debug.Log($"Restored {monsterData.type} in shelter");
        }

        /// <summary>
        /// 退避スポットをクリア（デバッグ用）
        /// </summary>
        public void ClearShelter()
        {
            foreach (var monster in shelterMonsters)
            {
                if (monster is MonoBehaviour monsterMB)
                {
                    Destroy(monsterMB.gameObject);
                }
            }
            
            shelterMonsters.Clear();
            Debug.Log("退避スポットをクリアしました");
        }
    }
}