using UnityEngine;
using DungeonOwner.Core;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 回復エフェクトの視覚的表示を管理
    /// </summary>
    public class RecoveryEffect : MonoBehaviour
    {
        [Header("エフェクト設定")]
        [SerializeField] private float duration = 1f;
        [SerializeField] private float moveSpeed = 1f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        private SpriteRenderer spriteRenderer;
        private float startTime;
        private Vector3 startPosition;
        private Color originalColor;
        private RecoveryType recoveryType;
        private bool isInShelter;
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }
        
        private void Start()
        {
            startTime = Time.time;
            startPosition = transform.position;
            
            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
            
            // 自動削除
            Destroy(gameObject, duration);
        }
        
        private void Update()
        {
            float elapsed = Time.time - startTime;
            float progress = elapsed / duration;
            
            if (progress >= 1f)
            {
                Destroy(gameObject);
                return;
            }
            
            UpdateEffect(progress);
        }
        
        /// <summary>
        /// エフェクトを初期化
        /// </summary>
        public void Initialize(RecoveryType type, bool inShelter)
        {
            recoveryType = type;
            isInShelter = inShelter;
            
            // 回復タイプに応じてスプライトを設定
            SetupSprite();
        }
        
        /// <summary>
        /// エフェクトの更新処理
        /// </summary>
        private void UpdateEffect(float progress)
        {
            // 上方向に移動
            Vector3 currentPosition = startPosition + Vector3.up * (moveSpeed * progress);
            transform.position = currentPosition;
            
            // スケールアニメーション
            float scale = scaleCurve.Evaluate(progress);
            transform.localScale = Vector3.one * scale;
            
            // アルファアニメーション
            if (spriteRenderer != null)
            {
                float alpha = alphaCurve.Evaluate(progress);
                Color currentColor = originalColor;
                currentColor.a = alpha;
                spriteRenderer.color = currentColor;
            }
        }
        
        /// <summary>
        /// 回復タイプに応じてスプライトを設定
        /// </summary>
        private void SetupSprite()
        {
            if (spriteRenderer == null) return;
            
            // 簡単な円形スプライトを作成（実際のプロジェクトではアセットを使用）
            Texture2D texture = CreateCircleTexture(32);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);
            spriteRenderer.sprite = sprite;
            
            // 回復タイプに応じて色を調整
            Color effectColor = recoveryType == RecoveryType.Health ? Color.green : Color.blue;
            
            // 退避スポットの場合はより明るく
            if (isInShelter)
            {
                effectColor = Color.Lerp(effectColor, Color.white, 0.3f);
            }
            
            spriteRenderer.color = effectColor;
            originalColor = effectColor;
        }
        
        /// <summary>
        /// 円形テクスチャを作成（プレースホルダー）
        /// </summary>
        private Texture2D CreateCircleTexture(int size)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];
            
            Vector2 center = Vector2.one * (size * 0.5f);
            float radius = size * 0.4f;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);
                    
                    if (distance <= radius)
                    {
                        float alpha = 1f - (distance / radius);
                        pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                    }
                    else
                    {
                        pixels[y * size + x] = Color.clear;
                    }
                }
            }
            
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}