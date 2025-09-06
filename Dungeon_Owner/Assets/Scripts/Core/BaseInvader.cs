using UnityEngine;

namespace DungeonOwner.Core
{
    public abstract class BaseInvader : MonoBehaviour
    {
        [SerializeField] protected string invaderName;
        [SerializeField] protected int health = 100;
        [SerializeField] protected int attackPower = 10;
        
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
            Destroy(gameObject);
        }
    }
}