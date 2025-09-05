using UnityEngine;
using DungeonOwner.Interfaces;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// HP/MP回復システム
    /// 階層配置中の緩やかな回復と退避スポットでの高速回復を管理
    /// </summary>
    public class RecoverySystem : MonoBehaviour
    {
        [Header("回復設定")]
        [SerializeField] private float floorRecoveryRate = 1f; // 階層配置中の回復速度（HP/秒）
        [SerializeField] private float shelterRecoveryRate = 5f; // 退避スポット回復速度（HP/秒）
        [SerializeField] private float manaRecoveryMultiplier = 0.8f; // MP回復倍率
        
        [Header("視覚効果")]
        [SerializeField] private GameObject recoveryEffectPrefab;
        [SerializeField] private Color floorRecoveryColor = Color.green;
        [SerializeField] private Color shelterRecoveryColor = Color.cyan;
        
        private static RecoverySystem instance;
        public static RecoverySystem Instance => instance;
        
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
            }
        }
        
        /// <summary>
        /// モンスターの回復処理を実行
        /// </summary>
        /// <param name="monster">回復対象のモンスター</param>
        /// <param name="deltaTime">経過時間</param>
        public void ProcessRecovery(IMonster monster, float deltaTime)
        {
            if (monster == null || monster.Health <= 0) return;
            
            bool isInShelter = monster.State == MonsterState.InShelter;
            float recoveryRate = isInShelter ? shelterRecoveryRate : floorRecoveryRate;
            
            // HP回復
            if (monster.Health < monster.MaxHealth)
            {
                float hpRecovery = recoveryRate * deltaTime;
                monster.Heal(hpRecovery);
                
                // 回復エフェクト表示
                if (hpRecovery > 0)
                {
                    ShowRecoveryEffect(monster, isInShelter, RecoveryType.Health);
                }
            }
            
            // MP回復
            if (monster.Mana < monster.MaxMana)
            {
                float mpRecovery = recoveryRate * manaRecoveryMultiplier * deltaTime;
                RecoverMana(monster, mpRecovery);
                
                // 回復エフェクト表示
                if (mpRecovery > 0)
                {
                    ShowRecoveryEffect(monster, isInShelter, RecoveryType.Mana);
                }
            }
        }
        
        /// <summary>
        /// MP回復処理（IMonsterにMP回復メソッドがない場合の対応）
        /// </summary>
        private void RecoverMana(IMonster monster, float amount)
        {
            // BaseMonsterクラスを通じてMP回復を実行
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
        /// 回復エフェクトを表示
        /// </summary>
        private void ShowRecoveryEffect(IMonster monster, bool isInShelter, RecoveryType recoveryType)
        {
            if (recoveryEffectPrefab == null) return;
            
            Vector3 effectPosition = monster.Position + Vector2.up * 0.5f;
            GameObject effect = Instantiate(recoveryEffectPrefab, effectPosition, Quaternion.identity);
            
            // エフェクトの色を設定
            var renderer = effect.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                Color effectColor = isInShelter ? shelterRecoveryColor : floorRecoveryColor;
                
                // MP回復の場合は青系に調整
                if (recoveryType == RecoveryType.Mana)
                {
                    effectColor = Color.Lerp(effectColor, Color.blue, 0.5f);
                }
                
                renderer.color = effectColor;
            }
            
            // エフェクトの自動削除
            var recoveryEffect = effect.GetComponent<RecoveryEffect>();
            if (recoveryEffect != null)
            {
                recoveryEffect.Initialize(recoveryType, isInShelter);
            }
            else
            {
                Destroy(effect, 1f);
            }
        }
        
        /// <summary>
        /// 回復率を取得
        /// </summary>
        public float GetRecoveryRate(bool isInShelter)
        {
            return isInShelter ? shelterRecoveryRate : floorRecoveryRate;
        }
        
        /// <summary>
        /// 回復率を設定（バランス調整用）
        /// </summary>
        public void SetRecoveryRates(float floorRate, float shelterRate)
        {
            floorRecoveryRate = floorRate;
            shelterRecoveryRate = shelterRate;
        }
    }
    
    /// <summary>
    /// 回復タイプ
    /// </summary>
    public enum RecoveryType
    {
        Health,
        Mana
    }
}