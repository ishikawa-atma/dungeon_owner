using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Invaders
{
    [RequireComponent(typeof(Animator))]
    public class InvaderAnimationController : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Header("Movement Animation")]
        [SerializeField] private float movementThreshold = 0.1f;
        
        private Vector3 lastPosition;
        private InvaderState lastState = InvaderState.Spawning;
        private bool isMoving = false;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponent<Animator>();
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
                
            lastPosition = transform.position;
        }

        private void Update()
        {
            UpdateMovementAnimation();
        }

        private void UpdateMovementAnimation()
        {
            // 移動判定
            Vector3 currentPosition = transform.position;
            float distance = Vector3.Distance(currentPosition, lastPosition);
            bool currentlyMoving = distance > movementThreshold;

            if (currentlyMoving != isMoving)
            {
                isMoving = currentlyMoving;
                UpdateAnimatorParameters();
            }

            // スプライトの向きを更新
            if (currentlyMoving)
            {
                Vector3 direction = (currentPosition - lastPosition).normalized;
                UpdateSpriteDirection(direction);
            }

            lastPosition = currentPosition;
        }

        private void UpdateAnimatorParameters()
        {
            if (animator != null)
            {
                animator.SetBool("IsMoving", isMoving);
            }
        }

        private void UpdateSpriteDirection(Vector3 direction)
        {
            if (spriteRenderer != null)
            {
                // 左右の向きを更新
                if (direction.x < 0)
                {
                    spriteRenderer.flipX = true;
                }
                else if (direction.x > 0)
                {
                    spriteRenderer.flipX = false;
                }
            }
        }

        public void SetState(InvaderState newState)
        {
            if (lastState != newState)
            {
                lastState = newState;
                
                if (animator != null)
                {
                    animator.SetInteger("State", (int)newState);
                    
                    // 状態に応じたトリガー
                    switch (newState)
                    {
                        case InvaderState.Spawning:
                            animator.SetTrigger("Spawn");
                            break;
                        case InvaderState.Fighting:
                            animator.SetTrigger("EnterCombat");
                            break;
                        case InvaderState.Dead:
                            animator.SetTrigger("Die");
                            break;
                    }
                }
            }
        }

        public void PlayAttackAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }

        public void PlayTakeDamageAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger("TakeDamage");
            }
        }

        public void PlayAbilityAnimation(string abilityName)
        {
            if (animator != null)
            {
                animator.SetTrigger(abilityName);
            }
        }

        public void SetAnimationSpeed(float speed)
        {
            if (animator != null)
            {
                animator.speed = speed;
            }
        }

        // エフェクト用メソッド
        public void SetSpriteAlpha(float alpha)
        {
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = Mathf.Clamp01(alpha);
                spriteRenderer.color = color;
            }
        }

        public void SetSpriteColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        public void FlashSprite(Color flashColor, float duration)
        {
            StartCoroutine(FlashCoroutine(flashColor, duration));
        }

        private System.Collections.IEnumerator FlashCoroutine(Color flashColor, float duration)
        {
            if (spriteRenderer == null) yield break;

            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = flashColor;
            
            yield return new WaitForSeconds(duration);
            
            spriteRenderer.color = originalColor;
        }
    }
}