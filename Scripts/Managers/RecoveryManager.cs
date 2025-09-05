using System.Collections.Generic;
using UnityEngine;
using DungeonOwner.Interfaces;
using DungeonOwner.Core;
using DungeonOwner.UI;
using DungeonOwner.Data;

namespace DungeonOwner.Managers
{
    /// <summary>
    /// 回復システム全体を管理するマネージャー
    /// </summary>
    public class RecoveryManager : MonoBehaviour
    {
        [Header("回復設定")]
        [SerializeField] private float floorRecoveryRate = 1f; // 階層配置中の回復速度（HP/秒）
        [SerializeField] private float shelterRecoveryRate = 5f; // 退避スポット回復速度（HP/秒）
        [SerializeField] private float manaRecoveryMultiplier = 0.8f; // MP回復倍率
        
        [Header("UI設定")]
        [SerializeField] private GameObject recoveryStatusUIPrefab;
        [SerializeField] private bool showRecoveryUI = true;
        [SerializeField] private float uiUpdateInterval = 0.1f; // UI更新間隔
        
        [Header("エフェクト設定")]
        [SerializeField] private GameObject recoveryEffectPrefab;
        [SerializeField] private float effectSpawnInterval = 2f; // エフェクト生成間隔
        
        private static RecoveryManager instance;
        public static RecoveryManager Instance => instance;
        
        private Dictionary<IMonster, RecoveryStatusUI> monsterUIMap = new Dictionary<IMonster, RecoveryStatusUI>();
        private Dictionary<IMonster, float> lastEffectTime = new Dictionary<IMonster, float>();
        private List<IMonster> activeMonsters = new List<IMonster>();
        
        private float lastUIUpdate;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Update()
        {
            ProcessAllRecovery();
            UpdateRecoveryUI();
        }
        
        /// <summary>
        /// 全モンスターの回復処理
        /// </summary>
        private void ProcessAllRecovery()
        {
            // アクティブなモンスターリストをクリーンアップ
            activeMonsters.RemoveAll(monster => monster == null || monster.Health <= 0);
            
            foreach (var monster in activeMonsters)
            {
                ProcessMonsterRecovery(monster);
            }
        }
        
        /// <summary>
        /// 個別モンスターの回復処理
        /// </summary>
        private void ProcessMonsterRecovery(IMonster monster)
        {
            if (monster == null || monster.Health <= 0) return;
            
            bool isInShelter = monster.State == MonsterState.InShelter;
            float recoveryRate = isInShelter ? shelterRecoveryRate : floorRecoveryRate;
            
            bool wasRecovering = false;
            
            // HP回復
            if (monster.Health < monster.MaxHealth)
            {
                float hpRecovery = recoveryRate * Time.deltaTime;
                monster.Heal(hpRecovery);
                wasRecovering = true;
            }
            
            // MP回復
            if (monster.Mana < monster.MaxMana)
            {
                float mpRecovery = recoveryRate * manaRecoveryMultiplier * Time.deltaTime;
                RecoverMana(monster, mpRecovery);
                wasRecovering = true;
            }
            
            // 回復エフェクトの表示
            if (wasRecovering && ShouldShowEffect(monster))
            {
                ShowRecoveryEffect(monster, isInShelter);
                lastEffectTime[monster] = Time.time;
            }
        }
        
        /// <summary>
        /// MP回復処理
        /// </summary>
        private void RecoverMana(IMonster monster, float amount)
        {
            if (monster is Monsters.BaseMonster baseMonster)
            {
                // リフレクションを使用してcurrentManaを更新
                var field = typeof(Monsters.BaseMonster).GetField("currentMana", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (field != null)
                {
                    float currentMana = (float)field.GetValue(baseMonster);
                    float newMana = Mathf.Min(monster.MaxMana, currentMana + amount);
                    field.SetValue(baseMonster, newMana);
                }
            }
        }
        
        /// <summary>
        /// 回復UIの更新
        /// </summary>
        private void UpdateRecoveryUI()
        {
            if (!showRecoveryUI || Time.time - lastUIUpdate < uiUpdateInterval) return;
            
            lastUIUpdate = Time.time;
            
            foreach (var kvp in monsterUIMap)
            {
                var monster = kvp.Key;
                var ui = kvp.Value;
                
                if (monster == null || monster.Health <= 0)
                {
                    if (ui != null)
                    {
                        ui.SetVisible(false);
                    }
                    continue;
                }
                
                // 回復中のモンスターのみUIを表示
                bool isRecovering = monster.Health < monster.MaxHealth || monster.Mana < monster.MaxMana;
                ui.SetVisible(isRecovering);
                
                if (isRecovering)
                {
                    bool isInShelter = monster.State == MonsterState.InShelter;
                    float rate = isInShelter ? shelterRecoveryRate : floorRecoveryRate;
                    ui.ShowRecoveryRate(rate, isInShelter);
                }
            }
        }
        
        /// <summary>
        /// モンスターを回復システムに登録
        /// </summary>
        public void RegisterMonster(IMonster monster)
        {
            if (monster == null || activeMonsters.Contains(monster)) return;
            
            activeMonsters.Add(monster);
            lastEffectTime[monster] = 0f;
            
            // 回復UIを作成
            if (showRecoveryUI && recoveryStatusUIPrefab != null)
            {
                CreateRecoveryUI(monster);
            }
        }
        
        /// <summary>
        /// モンスターを回復システムから除外
        /// </summary>
        public void UnregisterMonster(IMonster monster)
        {
            if (monster == null) return;
            
            activeMonsters.Remove(monster);
            lastEffectTime.Remove(monster);
            
            // 回復UIを削除
            if (monsterUIMap.TryGetValue(monster, out RecoveryStatusUI ui))
            {
                if (ui != null)
                {
                    Destroy(ui.gameObject);
                }
                monsterUIMap.Remove(monster);
            }
        }
        
        /// <summary>
        /// 回復UIを作成
        /// </summary>
        private void CreateRecoveryUI(IMonster monster)
        {
            if (recoveryStatusUIPrefab == null || monsterUIMap.ContainsKey(monster)) return;
            
            GameObject uiObject = Instantiate(recoveryStatusUIPrefab);
            RecoveryStatusUI ui = uiObject.GetComponent<RecoveryStatusUI>();
            
            if (ui != null)
            {
                ui.SetTargetMonster(monster);
                monsterUIMap[monster] = ui;
            }
            else
            {
                Destroy(uiObject);
            }
        }
        
        /// <summary>
        /// 回復エフェクトを表示
        /// </summary>
        private void ShowRecoveryEffect(IMonster monster, bool isInShelter)
        {
            if (recoveryEffectPrefab == null) return;
            
            Vector3 effectPosition = monster.Position + Vector2.up * 0.5f;
            GameObject effect = Instantiate(recoveryEffectPrefab, effectPosition, Quaternion.identity);
            
            var recoveryEffect = effect.GetComponent<RecoveryEffect>();
            if (recoveryEffect != null)
            {
                recoveryEffect.Initialize(RecoveryType.Health, isInShelter);
            }
            else
            {
                // 基本的なエフェクト処理
                Destroy(effect, 1f);
            }
        }
        
        /// <summary>
        /// エフェクトを表示すべきかどうかを判定
        /// </summary>
        private bool ShouldShowEffect(IMonster monster)
        {
            if (!lastEffectTime.ContainsKey(monster)) return true;
            
            return Time.time - lastEffectTime[monster] >= effectSpawnInterval;
        }
        
        /// <summary>
        /// 回復率を設定
        /// </summary>
        public void SetRecoveryRates(float floorRate, float shelterRate)
        {
            floorRecoveryRate = floorRate;
            shelterRecoveryRate = shelterRate;
        }
        
        /// <summary>
        /// 回復率を取得
        /// </summary>
        public float GetRecoveryRate(bool isInShelter)
        {
            return isInShelter ? shelterRecoveryRate : floorRecoveryRate;
        }
        
        /// <summary>
        /// 回復UIの表示/非表示を切り替え
        /// </summary>
        public void SetRecoveryUIVisible(bool visible)
        {
            showRecoveryUI = visible;
            
            foreach (var ui in monsterUIMap.Values)
            {
                if (ui != null)
                {
                    ui.SetVisible(visible);
                }
            }
        }
        
        /// <summary>
        /// 全モンスターの即座回復（デバッグ用）
        /// </summary>
        public void HealAllMonsters()
        {
            foreach (var monster in activeMonsters)
            {
                if (monster != null && monster.Health > 0)
                {
                    monster.Heal(monster.MaxHealth);
                    RecoverMana(monster, monster.MaxMana);
                }
            }
        }
        
        /// <summary>
        /// 統計情報を取得
        /// </summary>
        public RecoveryStats GetRecoveryStats()
        {
            int recoveringMonsters = 0;
            int shelterMonsters = 0;
            
            foreach (var monster in activeMonsters)
            {
                if (monster == null || monster.Health <= 0) continue;
                
                if (monster.Health < monster.MaxHealth || monster.Mana < monster.MaxMana)
                {
                    recoveringMonsters++;
                }
                
                if (monster.State == MonsterState.InShelter)
                {
                    shelterMonsters++;
                }
            }
            
            return new RecoveryStats
            {
                TotalMonsters = activeMonsters.Count,
                RecoveringMonsters = recoveringMonsters,
                ShelterMonsters = shelterMonsters,
                FloorRecoveryRate = floorRecoveryRate,
                ShelterRecoveryRate = shelterRecoveryRate
            };
        }
    }
    
    /// <summary>
    /// 回復システムの統計情報
    /// </summary>
    [System.Serializable]
    public struct RecoveryStats
    {
        public int TotalMonsters;
        public int RecoveringMonsters;
        public int ShelterMonsters;
        public float FloorRecoveryRate;
        public float ShelterRecoveryRate;
    }
}