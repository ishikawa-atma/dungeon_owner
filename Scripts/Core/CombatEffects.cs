using UnityEngine;
using System.Collections;
using TMPro;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 戦闘エフェクトの管理クラス
    /// ダメージテキスト、パーティクル、アニメーションを制御
    /// </summary>
    public class CombatEffects : MonoBehaviour
    {
        [Header("ダメージテキスト")]
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private float textFloatHeight = 1f;
        [SerializeField] private float textDuration = 1f;
        [SerializeField] private AnimationCurve textMoveCurve = AnimationCurve.EaseOut(0, 0, 1, 1);

        [Header("パーティクルエフェクト")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject criticalHitEffectPrefab;
        [SerializeField] private GameObject blockEffectPrefab;
        [SerializeField] private GameObject healEffectPrefab;

        [Header("色設定")]
        [SerializeField] private Color normalDamageColor = Color.white;
        [SerializeField] private Color criticalDamageColor = Color.red;
        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color missColor = Color.gray;

        public static CombatEffects Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// ダメージテキストを表示
        /// </summary>
        public void ShowDamageText(Vector3 position, float damage, bool isCritical = false, bool isMiss = false)
        {
            if (damageTextPrefab == null) return;

            GameObject textObj = Instantiate(damageTextPrefab, position, Quaternion.identity);
            TextMeshPro textComponent = textObj.GetComponent<TextMeshPro>();

            if (textComponent != null)
            {
                if (isMiss)
                {
                    textComponent.text = "MISS";
                    textComponent.color = missColor;
                }
                else
                {
                    textComponent.text = Mathf.RoundToInt(damage).ToString();
                    textComponent.color = isCritical ? criticalDamageColor : normalDamageColor;
                    
                    if (isCritical)
                    {
                        textComponent.fontSize *= 1.5f;
                    }
                }

                StartCoroutine(AnimateDamageText(textObj, textComponent));
            }
        }

        /// <summary>
        /// 回復テキストを表示
        /// </summary>
        public void ShowHealText(Vector3 position, float healAmount)
        {
            if (damageTextPrefab == null) return;

            GameObject textObj = Instantiate(damageTextPrefab, position, Quaternion.identity);
            TextMeshPro textComponent = textObj.GetComponent<TextMeshPro>();

            if (textComponent != null)
            {
                textComponent.text = "+" + Mathf.RoundToInt(healAmount).ToString();
                textComponent.color = healColor;

                StartCoroutine(AnimateDamageText(textObj, textComponent));
            }
        }

        /// <summary>
        /// ダメージテキストのアニメーション
        /// </summary>
        private IEnumerator AnimateDamageText(GameObject textObj, TextMeshPro textComponent)
        {
            Vector3 startPos = textObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * textFloatHeight;
            float elapsed = 0f;

            while (elapsed < textDuration)
            {
                float progress = elapsed / textDuration;
                
                // 位置のアニメーション
                float curveValue = textMoveCurve.Evaluate(progress);
                textObj.transform.position = Vector3.Lerp(startPos, endPos, curveValue);

                // フェードアウト
                Color color = textComponent.color;
                color.a = 1f - progress;
                textComponent.color = color;

                // スケールアニメーション
                float scale = 1f + (1f - progress) * 0.2f;
                textObj.transform.localScale = Vector3.one * scale;

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(textObj);
        }

        /// <summary>
        /// ヒットエフェクトを再生
        /// </summary>
        public void PlayHitEffect(Vector3 position, bool isCritical = false)
        {
            GameObject effectPrefab = isCritical ? criticalHitEffectPrefab : hitEffectPrefab;
            
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab, position, Quaternion.identity);
                
                // パーティクルシステムがあれば再生
                ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    particles.Play();
                }

                // 一定時間後に削除
                Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// ブロックエフェクトを再生
        /// </summary>
        public void PlayBlockEffect(Vector3 position)
        {
            if (blockEffectPrefab != null)
            {
                GameObject effect = Instantiate(blockEffectPrefab, position, Quaternion.identity);
                Destroy(effect, 1f);
            }
        }

        /// <summary>
        /// 回復エフェクトを再生
        /// </summary>
        public void PlayHealEffect(Vector3 position)
        {
            if (healEffectPrefab != null)
            {
                GameObject effect = Instantiate(healEffectPrefab, position, Quaternion.identity);
                
                ParticleSystem particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    particles.Play();
                }

                Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// スプライトの点滅効果
        /// </summary>
        public void FlashSprite(SpriteRenderer spriteRenderer, Color flashColor, float duration)
        {
            if (spriteRenderer != null)
            {
                StartCoroutine(FlashSpriteCoroutine(spriteRenderer, flashColor, duration));
            }
        }

        /// <summary>
        /// スプライト点滅のコルーチン
        /// </summary>
        private IEnumerator FlashSpriteCoroutine(SpriteRenderer spriteRenderer, Color flashColor, float duration)
        {
            Color originalColor = spriteRenderer.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                
                // 点滅効果
                float flashIntensity = Mathf.Sin(progress * Mathf.PI * 6f) * (1f - progress);
                Color currentColor = Color.Lerp(originalColor, flashColor, Mathf.Abs(flashIntensity));
                
                spriteRenderer.color = currentColor;

                elapsed += Time.deltaTime;
                yield return null;
            }

            spriteRenderer.color = originalColor;
        }

        /// <summary>
        /// 戦闘開始エフェクト
        /// </summary>
        public void PlayCombatStartEffect(Vector3 position)
        {
            // 戦闘開始時の視覚効果
            ShowDamageText(position + Vector3.up * 0.5f, 0f, false, false);
            
            // 軽いカメラシェイク
            CameraShake.Instance?.Shake(0.05f, 0.1f);
        }

        /// <summary>
        /// 戦闘終了エフェクト
        /// </summary>
        public void PlayCombatEndEffect(Vector3 position, bool victory)
        {
            Color effectColor = victory ? Color.green : Color.red;
            
            // 勝利/敗北に応じたエフェクト
            if (victory)
            {
                PlayHealEffect(position);
            }
            else
            {
                PlayHitEffect(position, true);
            }
        }

        /// <summary>
        /// 回復エフェクトを表示（パーティ戦闘システム用）
        /// </summary>
        public void ShowHealingEffect(Vector3 position)
        {
            PlayHealEffect(position);
            ShowHealText(position, 0f); // 回復量は別途表示される
        }

        /// <summary>
        /// パーティ戦闘エフェクトを表示
        /// </summary>
        public void ShowPartyBattleEffect(Vector3 position, float damage)
        {
            // パーティ戦闘専用のエフェクト
            ShowDamageText(position, damage, damage > 50f); // 50以上でクリティカル扱い
            PlayHitEffect(position, damage > 50f);
            
            // パーティ戦闘の視覚的表現として複数のエフェクトを表示
            for (int i = 0; i < 3; i++)
            {
                Vector3 offsetPos = position + new Vector3(
                    Random.Range(-1f, 1f), 
                    Random.Range(-0.5f, 0.5f), 
                    0f
                );
                
                StartCoroutine(DelayedEffect(offsetPos, i * 0.1f));
            }
        }

        /// <summary>
        /// 遅延エフェクト用コルーチン
        /// </summary>
        private IEnumerator DelayedEffect(Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayHitEffect(position, false);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}