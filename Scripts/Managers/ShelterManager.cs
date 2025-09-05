using System.Collections.Generic;
using UnityEngine;

namespace DungeonOwner.Managers
{
    /// <summary>
    /// 退避スポットシステムを管理するクラス
    /// モンスターの退避、配置、売却機能を提供
    /// </summary>
    public class ShelterManager : MonoBehaviour
    {
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
        /// </summary>
        public bool SellMonster(IMonster monster)
        {
            if (!shelterMonsters.Contains(monster))
            {
                Debug.LogWarning("指定されたモンスターは退避スポットにいません");
                return false;
            }
            
            // 退避スポットから除去
            shelterMonsters.Remove(monster);
            
            // モンスターオブジェクトを破棄
            if (monster is MonoBehaviour monsterMB)
            {
                Destroy(monsterMB.gameObject);
            }
            
            OnMonsterSold?.Invoke(monster);
            Debug.Log($"モンスター {monster.Type} を売却しました");
            
            return true;
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