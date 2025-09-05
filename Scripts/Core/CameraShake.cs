using UnityEngine;
using System.Collections;

namespace DungeonOwner.Core
{
    /// <summary>
    /// カメラシェイク効果を管理するクラス
    /// 戦闘時の衝撃感を演出
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("シェイク設定")]
        [SerializeField] private float defaultIntensity = 0.1f;
        [SerializeField] private float defaultDuration = 0.1f;
        [SerializeField] private AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        private Camera targetCamera;
        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;

        public static CameraShake Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    targetCamera = FindObjectOfType<Camera>();
                }
                
                if (targetCamera != null)
                {
                    originalPosition = targetCamera.transform.localPosition;
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// シェイク効果を開始
        /// </summary>
        /// <param name="intensity">シェイクの強度</param>
        /// <param name="duration">シェイクの持続時間</param>
        public void Shake(float intensity = -1f, float duration = -1f)
        {
            if (targetCamera == null) return;

            if (intensity < 0) intensity = defaultIntensity;
            if (duration < 0) duration = defaultDuration;

            // 既存のシェイクを停止
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakeCoroutine(intensity, duration));
        }

        /// <summary>
        /// シェイク効果のコルーチン
        /// </summary>
        private IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float currentIntensity = intensity * shakeCurve.Evaluate(progress);

                // ランダムな方向にシェイク
                Vector3 randomOffset = new Vector3(
                    Random.Range(-1f, 1f) * currentIntensity,
                    Random.Range(-1f, 1f) * currentIntensity,
                    0f
                );

                targetCamera.transform.localPosition = originalPosition + randomOffset;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 元の位置に戻す
            targetCamera.transform.localPosition = originalPosition;
            shakeCoroutine = null;
        }

        /// <summary>
        /// シェイクを即座に停止
        /// </summary>
        public void StopShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }

            if (targetCamera != null)
            {
                targetCamera.transform.localPosition = originalPosition;
            }
        }

        /// <summary>
        /// 戦闘の種類に応じたシェイク
        /// </summary>
        public void ShakeForCombatType(CombatType combatType)
        {
            switch (combatType)
            {
                case CombatType.Normal:
                    Shake(0.05f, 0.1f);
                    break;
                case CombatType.Critical:
                    Shake(0.15f, 0.2f);
                    break;
                case CombatType.Special:
                    Shake(0.2f, 0.3f);
                    break;
                case CombatType.Boss:
                    Shake(0.3f, 0.5f);
                    break;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    public enum CombatType
    {
        Normal,
        Critical,
        Special,
        Boss
    }
}