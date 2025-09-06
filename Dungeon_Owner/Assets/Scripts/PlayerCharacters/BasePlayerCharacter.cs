using UnityEngine;

namespace DungeonOwner.PlayerCharacters
{
    public abstract class BasePlayerCharacter : MonoBehaviour
    {
        [SerializeField] protected string characterName;
        [SerializeField] protected int health = 100;
        [SerializeField] protected int attackPower = 15;
        
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
            // プレイヤーキャラクターの死亡処理
            gameObject.SetActive(false);
        }
    }
}