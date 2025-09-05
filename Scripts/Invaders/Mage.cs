using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Invaders
{
    public class Mage : BaseInvader
    {
        [Header("Mage Specific")]
        [SerializeField] private float fireballRange = 4f;
        [SerializeField] private float fireballDamage = 25f;
        [SerializeField] private float fireballCooldown = 3f;
        [SerializeField] private float lastFireballTime = -3f;
        [SerializeField] private GameObject fireballPrefab;

        protected override void InitializeInvader()
        {
            base.InitializeInvader();
            
            // 魔法使いは遠距離攻撃が得意
            if (invaderData != null && invaderData.type == InvaderType.Mage)
            {
                attackRange = fireballRange; // 攻撃範囲を拡大
                fireballDamage = attackPower * 1.5f; // ファイアボールは通常攻撃より強力
                Debug.Log($"Mage {name} initialized with Fireball ability");
            }
        }

        protected override void UpdateFighting()
        {
            // ファイアボール攻撃の判定
            if (Time.time > lastFireballTime + fireballCooldown)
            {
                var enemies = FindNearbyEnemies();
                if (enemies.Count > 0)
                {
                    CastFireball(enemies[0]);
                    return; // ファイアボールを撃ったら通常攻撃はスキップ
                }
            }

            base.UpdateFighting();
        }

        protected override System.Collections.Generic.List<GameObject> FindNearbyEnemies()
        {
            var enemies = new System.Collections.Generic.List<GameObject>();
            
            // 魔法使いは遠距離攻撃が可能
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, fireballRange);
            
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && collider.CompareTag("Monster"))
                {
                    enemies.Add(collider.gameObject);
                }
            }
            
            return enemies;
        }

        private void CastFireball(GameObject target)
        {
            if (target == null) return;

            lastFireballTime = Time.time;
            
            // ファイアボールエフェクト
            if (animator != null)
            {
                animator.SetTrigger("Fireball");
            }
            
            // ファイアボール生成（プレハブがある場合）
            if (fireballPrefab != null)
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                GameObject fireball = Instantiate(fireballPrefab, transform.position, Quaternion.LookRotation(Vector3.forward, direction));
                
                // ファイアボールの移動処理（簡易版）
                StartCoroutine(MoveFireball(fireball, target.transform.position));
            }
            else
            {
                // プレハブがない場合は直接ダメージ
                var monster = target.GetComponent<Interfaces.IMonster>();
                if (monster != null)
                {
                    monster.TakeDamage(fireballDamage);
                    Debug.Log($"{name} cast Fireball on {target.name} for {fireballDamage} damage");
                }
            }
        }

        private System.Collections.IEnumerator MoveFireball(GameObject fireball, Vector3 targetPos)
        {
            float speed = 8f;
            Vector3 startPos = fireball.transform.position;
            
            while (Vector3.Distance(fireball.transform.position, targetPos) > 0.1f)
            {
                fireball.transform.position = Vector3.MoveTowards(fireball.transform.position, targetPos, speed * Time.deltaTime);
                yield return null;
            }
            
            // 着弾エフェクト
            // TODO: 爆発エフェクトを追加
            
            // 範囲ダメージ
            Collider2D[] hitTargets = Physics2D.OverlapCircleAll(targetPos, 1.5f);
            foreach (var hit in hitTargets)
            {
                if (hit.CompareTag("Monster"))
                {
                    var monster = hit.GetComponent<Interfaces.IMonster>();
                    monster?.TakeDamage(fireballDamage);
                }
            }
            
            Destroy(fireball);
        }

        protected override void OnStateChanged(InvaderState newState)
        {
            base.OnStateChanged(newState);
            
            // 魔法使い特有の状態変更処理
            if (newState == InvaderState.Moving)
            {
                // 移動時は遠距離から攻撃可能な敵を探す
            }
        }

        // デバッグ用
        private void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // ファイアボール範囲を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCircle(transform.position, fireballRange);
        }
    }
}