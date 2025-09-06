using UnityEngine;

namespace DungeonOwner.Core
{
    public abstract class BaseMonster : MonoBehaviour
    {
        [SerializeField] protected string monsterName;
        [SerializeField] protected int health = 50;
        [SerializeField] protected int attackPower = 8;
        [SerializeField] protected int goldReward = 10;
        
        protected virtual void Start()
        {
            // 基本初期化
        }
        
        public virtual void TakeDamage(int damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
        }
        
        protected virtual void Die()
        {
            // 金貨報酬を与える
            var resourceManager = FindObjectOfType<DungeonOwner.Managers.ResourceManager>();
            if (resourceManager != null)
            {
                resourceManager.AddGold(goldReward);
            }
            
            Destroy(gameObject);
        }
    }
}