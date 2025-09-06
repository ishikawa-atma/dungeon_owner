using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.PlayerCharacters
{
    public abstract class BasePlayerCharacter : MonoBehaviour
    {
        [SerializeField] protected string characterName;
        [SerializeField] protected float currentHealth = 100f;
        [SerializeField] protected float MaxHealth = 100f;
        [SerializeField] protected float currentMana = 50f;
        [SerializeField] protected bool isDead = false;
        
        protected MonsterState currentState = MonsterState.Idle;
        
        protected virtual void Start()
        {
            // 基本初期化
        }
        
        protected virtual void UpdateCharacterBehavior()
        {
            // キャラクター行動処理 - 子クラスでオーバーライド可能
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
        
        protected virtual void OnDrawGizmosSelected()
        {
            // ギズモ描画 - 子クラスでオーバーライド可能
        }
        
        public virtual void TakeDamage(float damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        protected virtual void Die()
        {
            isDead = true;
            gameObject.SetActive(false);
        }
        
        protected bool IsAlive()
        {
            return !isDead && currentHealth > 0;
        }
        
        protected float GetAttackPower()
        {
            return 15f; // 基本攻撃力
        }
        
        public void SetCharacterData(object data)
        {
            // キャラクターデータ設定（基本実装）
            if (data != null)
            {
                // データ設定処理
            }
        }
    }
}