using UnityEngine;
using DungeonOwner.Data;
using System.Collections.Generic;

namespace DungeonOwner.Invaders
{
    public abstract class BaseInvader : MonoBehaviour
    {
        [SerializeField] protected string invaderName;
        [SerializeField] protected float health = 100f;
        [SerializeField] protected float attackPower = 10f;
        [SerializeField] protected float moveSpeed = 2f;
        [SerializeField] protected float lastAttackTime;
        
        protected InvaderData invaderData;
        protected Animator animator;
        protected SpriteRenderer spriteRenderer;
        
        protected virtual void Start()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            InitializeInvader();
        }
        
        protected virtual void InitializeInvader()
        {
            // 基本初期化 - 子クラスでオーバーライド可能
        }
        
        protected virtual void UpdateMovement()
        {
            // 移動処理 - 子クラスでオーバーライド可能
        }
        
        protected virtual void UpdateFighting()
        {
            // 戦闘処理 - 子クラスでオーバーライド可能
        }
        
        protected virtual void AttackEnemy(GameObject enemy)
        {
            // 攻撃処理 - 子クラスでオーバーライド可能
        }
        
        protected virtual void FindNextTarget()
        {
            // ターゲット検索 - 子クラスでオーバーライド可能
        }
        
        protected virtual void OnStateChanged(InvaderState newState)
        {
            // 状態変更処理 - 子クラスでオーバーライド可能
        }
        
        public virtual void TakeDamage(float damage)
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
        
        protected List<GameObject> FindNearbyEnemies()
        {
            // 近くの敵を検索する基本実装
            return new List<GameObject>();
        }
        
        protected void PlayAttackEffect()
        {
            // 攻撃エフェクトの基本実装
        }
    }
}