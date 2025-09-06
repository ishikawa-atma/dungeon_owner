using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Monsters
{
    public abstract class BaseMonster : MonoBehaviour
    {
        [SerializeField] protected string monsterName;
        [SerializeField] protected float health = 50f;
        [SerializeField] protected float currentHealth = 50f;
        [SerializeField] protected float MaxHealth = 50f;
        [SerializeField] protected float attackPower = 8f;
        [SerializeField] protected float currentMana = 30f;
        [SerializeField] protected float MaxMana = 30f;
        [SerializeField] protected int goldReward = 10;
        [SerializeField] protected int level = 1;
        [SerializeField] protected bool isDead = false;
        
        protected MonsterState currentState = MonsterState.Idle;
        protected MonsterData monsterData;
        
        protected virtual void Start()
        {
            InitializeAbilities();
        }
        
        protected virtual void InitializeAbilities()
        {
            // アビリティ初期化 - 子クラスでオーバーライド可能
        }
        
        protected virtual void UpdateMonsterBehavior()
        {
            // モンスター行動処理 - 子クラスでオーバーライド可能
        }
        
        protected virtual void ExecuteAbility()
        {
            // アビリティ実行 - 子クラスでオーバーライド可能
        }
        
        protected virtual bool CanUseAbility()
        {
            // アビリティ使用可能判定 - 子クラスでオーバーライド可能
            return false;
        }
        
        protected virtual float GetAbilityCooldown()
        {
            // アビリティクールダウン - 子クラスでオーバーライド可能
            return 5f;
        }
        
        protected virtual void ProcessNaturalRecovery()
        {
            // 自然回復処理 - 子クラスでオーバーライド可能
        }
        
        protected virtual void OnDamageTaken(float damage)
        {
            // ダメージ受けた時の処理 - 子クラスでオーバーライド可能
        }
        
        protected virtual void OnHealed(float healAmount)
        {
            // 回復時の処理 - 子クラスでオーバーライド可能
        }
        
        protected virtual void OnDrawGizmosSelected()
        {
            // ギズモ描画 - 子クラスでオーバーライド可能
        }
        
        public virtual void TakeDamage(float damage)
        {
            health -= damage;
            OnDamageTaken(damage);
            if (health <= 0)
            {
                Die();
            }
        }
        
        protected virtual void Die()
        {
            isDead = true;
            // 金貨報酬を与える
            var resourceManager = FindObjectOfType<DungeonOwner.Managers.ResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.AddGold(goldReward);
            }
            
            Destroy(gameObject);
        }
        
        public bool IsAlive()
        {
            return !isDead && currentHealth > 0;
        }
        
        protected float GetAttackPower()
        {
            return attackPower;
        }
        
        protected void SetState(MonsterState newState)
        {
            currentState = newState;
        }
        
        protected void Heal(float amount)
        {
            currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
        }
        
        protected void UseAbility()
        {
            // 基本アビリティ使用処理
        }
        
        protected void UseAbility(object ability)
        {
            // オーバーロード版（基本実装）
        }
        
        protected void AddAbility(object ability)
        {
            // アビリティ追加処理（基本実装）
        }
        
        protected void UpdateStatsForLevel()
        {
            // レベルに応じたステータス更新
        }
        
        public void SetMonsterData(MonsterData data)
        {
            if (data != null)
            {
                monsterData = data;
            }
        }
        
        public void SetMonsterData(object data)
        {
            // オーバーロード版（基本実装）
            if (data is MonsterData monsterDataTyped)
            {
                SetMonsterData(monsterDataTyped);
            }
        }
    }
}