using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DungeonOwner.Core
{
    /// <summary>
    /// メモリ使用量最適化システム
    /// テクスチャ、オーディオ、オブジェクトの動的な読み込み・解放を管理
    /// </summary>
    public class MemoryOptimizer : MonoBehaviour
    {
        public static MemoryOptimizer Instance { get; private set; }

        [Header("最適化設定")]
        [SerializeField] private bool enableAutoOptimization = true;
        [SerializeField] private float optimizationInterval = 30f; // 30秒ごとにチェック
        [SerializeField] private long memoryThreshold = 80 * 1024 * 1024; // 80MB
        [SerializeField] private long criticalMemoryThreshold = 150 * 1024 * 1024; // 150MB

        [Header("テクスチャ最適化")]
        [SerializeField] private bool optimizeTextures = true;
        [SerializeField] private float textureUnloadDelay = 60f; // 60秒後に未使用テクスチャを解放
        [SerializeField] private int maxTextureSize = 1024;
        [SerializeField] private TextureFormat preferredFormat = TextureFormat.ASTC_6x6;

        [Header("オーディオ最適化")]
        [SerializeField] private bool optimizeAudio = true;
        [SerializeField] private float audioUnloadDelay = 30f; // 30秒後に未使用オーディオを解放
        [SerializeField] private AudioCompressionFormat preferredAudioFormat = AudioCompressionFormat.Vorbis;

        [Header("オブジェクト最適化")]
        [SerializeField] private bool optimizeGameObjects = true;
        [SerializeField] private int maxInactiveObjects = 50;
        [SerializeField] private float objectCleanupInterval = 10f;

        // 追跡データ
        private Dictionary<Texture2D, float> textureLastUsed = new Dictionary<Texture2D, float>();
        private Dictionary<AudioClip, float> audioLastUsed = new Dictionary<AudioClip, float>();
        private Dictionary<GameObject, float> objectLastUsed = new Dictionary<GameObject, float>();
        
        // 最適化済みリソース
        private HashSet<Texture2D> optimizedTextures = new HashSet<Texture2D>();
        private HashSet<AudioClip> optimizedAudio = new HashSet<AudioClip>();
        
        // 統計
        private long initialMemoryUsage;
        private long memoryFreed = 0;
        private int texturesOptimized = 0;
        private int audioClipsOptimized = 0;
        private int objectsDestroyed = 0;

        // イベント
        public System.Action<long> OnMemoryOptimized;
        public System.Action<string> OnOptimizationStep;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeOptimizer();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            initialMemoryUsage = System.GC.GetTotalMemory(false);
            
            if (enableAutoOptimization)
            {
                StartCoroutine(OptimizationLoop());
            }
            
            StartCoroutine(ObjectCleanupLoop());
        }

        /// <summary>
        /// 最適化システムの初期化
        /// </summary>
        private void InitializeOptimizer()
        {
            // 初期設定の適用
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            
            // メモリ警告の監視（モバイル用）
            Application.lowMemory += OnLowMemoryWarning;
            
            Debug.Log("MemoryOptimizer initialized");
        }

        /// <summary>
        /// 最適化ループ
        /// </summary>
        private IEnumerator OptimizationLoop()
        {
            while (enableAutoOptimization)
            {
                yield return new WaitForSeconds(optimizationInterval);
                
                long currentMemory = System.GC.GetTotalMemory(false);
                
                if (currentMemory > memoryThreshold)
                {
                    yield return StartCoroutine(OptimizeMemoryUsage());
                }
            }
        }

        /// <summary>
        /// オブジェクトクリーンアップループ
        /// </summary>
        private IEnumerator ObjectCleanupLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(objectCleanupInterval);
                
                if (optimizeGameObjects)
                {
                    CleanupInactiveObjects();
                }
            }
        }

        /// <summary>
        /// メモリ使用量の最適化
        /// </summary>
        public IEnumerator OptimizeMemoryUsage()
        {
            long memoryBefore = System.GC.GetTotalMemory(false);
            OnOptimizationStep?.Invoke("Starting memory optimization");

            // 1. 未使用テクスチャの解放
            if (optimizeTextures)
            {
                yield return StartCoroutine(OptimizeTextures());
            }

            // 2. 未使用オーディオの解放
            if (optimizeAudio)
            {
                yield return StartCoroutine(OptimizeAudio());
            }

            // 3. 不要なゲームオブジェクトの削除
            if (optimizeGameObjects)
            {
                CleanupInactiveObjects();
                yield return null;
            }

            // 4. オブジェクトプールの最適化
            OptimizeObjectPools();
            yield return null;

            // 5. ガベージコレクション
            System.GC.Collect();
            yield return new WaitForSeconds(0.1f);

            long memoryAfter = System.GC.GetTotalMemory(false);
            long freed = memoryBefore - memoryAfter;
            memoryFreed += freed;

            OnOptimizationStep?.Invoke($"Memory optimization completed: {freed / (1024 * 1024)}MB freed");
            OnMemoryOptimized?.Invoke(freed);

            Debug.Log($"Memory optimization: {memoryBefore / (1024 * 1024)}MB -> {memoryAfter / (1024 * 1024)}MB (freed: {freed / (1024 * 1024)}MB)");
        }

        /// <summary>
        /// テクスチャの最適化
        /// </summary>
        private IEnumerator OptimizeTextures()
        {
            OnOptimizationStep?.Invoke("Optimizing textures");
            
            var textures = Resources.FindObjectsOfTypeAll<Texture2D>();
            float currentTime = Time.time;
            int optimized = 0;

            foreach (var texture in textures)
            {
                if (texture == null) continue;

                // システムテクスチャは除外
                if (texture.name.StartsWith("Unity") || texture.name.StartsWith("Default"))
                    continue;

                // 最後に使用された時間をチェック
                if (textureLastUsed.ContainsKey(texture))
                {
                    float lastUsed = textureLastUsed[texture];
                    if (currentTime - lastUsed > textureUnloadDelay)
                    {
                        // 未使用テクスチャを解放
                        Resources.UnloadAsset(texture);
                        textureLastUsed.Remove(texture);
                        optimized++;
                    }
                }
                else
                {
                    // 初回発見時は現在時刻を記録
                    textureLastUsed[texture] = currentTime;
                }

                // テクスチャサイズの最適化
                if (!optimizedTextures.Contains(texture) && texture.width > maxTextureSize)
                {
                    OptimizeTextureSize(texture);
                    optimizedTextures.Add(texture);
                }

                // フレーム分散処理
                if (optimized % 10 == 0)
                {
                    yield return null;
                }
            }

            texturesOptimized += optimized;
            OnOptimizationStep?.Invoke($"Textures optimized: {optimized}");
        }

        /// <summary>
        /// テクスチャサイズの最適化
        /// </summary>
        private void OptimizeTextureSize(Texture2D texture)
        {
            if (texture == null || !texture.isReadable) return;

            try
            {
                // テクスチャを縮小
                int newWidth = Mathf.Min(texture.width, maxTextureSize);
                int newHeight = Mathf.Min(texture.height, maxTextureSize);

                if (newWidth != texture.width || newHeight != texture.height)
                {
                    TextureScale.Bilinear(texture, newWidth, newHeight);
                    texture.Compress(true);
                    Debug.Log($"Optimized texture {texture.name}: {texture.width}x{texture.height} -> {newWidth}x{newHeight}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to optimize texture {texture.name}: {e.Message}");
            }
        }

        /// <summary>
        /// オーディオの最適化
        /// </summary>
        private IEnumerator OptimizeAudio()
        {
            OnOptimizationStep?.Invoke("Optimizing audio");
            
            var audioClips = Resources.FindObjectsOfTypeAll<AudioClip>();
            float currentTime = Time.time;
            int optimized = 0;

            foreach (var clip in audioClips)
            {
                if (clip == null) continue;

                // 最後に使用された時間をチェック
                if (audioLastUsed.ContainsKey(clip))
                {
                    float lastUsed = audioLastUsed[clip];
                    if (currentTime - lastUsed > audioUnloadDelay)
                    {
                        // 現在再生中でないクリップを解放
                        if (!IsAudioClipPlaying(clip))
                        {
                            Resources.UnloadAsset(clip);
                            audioLastUsed.Remove(clip);
                            optimized++;
                        }
                    }
                }
                else
                {
                    // 初回発見時は現在時刻を記録
                    audioLastUsed[clip] = currentTime;
                }

                // フレーム分散処理
                if (optimized % 5 == 0)
                {
                    yield return null;
                }
            }

            audioClipsOptimized += optimized;
            OnOptimizationStep?.Invoke($"Audio clips optimized: {optimized}");
        }

        /// <summary>
        /// オーディオクリップが再生中かチェック
        /// </summary>
        private bool IsAudioClipPlaying(AudioClip clip)
        {
            var audioSources = FindObjectsOfType<AudioSource>();
            return audioSources.Any(source => source.clip == clip && source.isPlaying);
        }

        /// <summary>
        /// 非アクティブオブジェクトのクリーンアップ
        /// </summary>
        private void CleanupInactiveObjects()
        {
            var allObjects = FindObjectsOfType<GameObject>(true);
            var inactiveObjects = allObjects.Where(obj => !obj.activeInHierarchy && 
                                                         obj.transform.parent == null &&
                                                         !obj.name.Contains("Manager") &&
                                                         !obj.name.Contains("System")).ToList();

            int destroyed = 0;
            float currentTime = Time.time;

            foreach (var obj in inactiveObjects)
            {
                if (obj == null) continue;

                // オブジェクトの最後の使用時間をチェック
                if (objectLastUsed.ContainsKey(obj))
                {
                    float lastUsed = objectLastUsed[obj];
                    if (currentTime - lastUsed > 60f) // 60秒間未使用
                    {
                        Destroy(obj);
                        objectLastUsed.Remove(obj);
                        destroyed++;
                    }
                }
                else
                {
                    objectLastUsed[obj] = currentTime;
                }

                // 制限数を超えた場合は古いものから削除
                if (inactiveObjects.Count - destroyed > maxInactiveObjects)
                {
                    Destroy(obj);
                    objectLastUsed.Remove(obj);
                    destroyed++;
                }
            }

            if (destroyed > 0)
            {
                objectsDestroyed += destroyed;
                OnOptimizationStep?.Invoke($"Inactive objects destroyed: {destroyed}");
            }
        }

        /// <summary>
        /// オブジェクトプールの最適化
        /// </summary>
        private void OptimizeObjectPools()
        {
            if (ObjectPool.Instance == null) return;

            OnOptimizationStep?.Invoke("Optimizing object pools");
            
            var stats = ObjectPool.Instance.GetAllPoolStats();
            foreach (var stat in stats)
            {
                // 使用率が低いプールのサイズを削減
                float usageRate = (float)stat.activeCount / (stat.activeCount + stat.availableCount);
                
                if (usageRate < 0.1f && stat.availableCount > 20)
                {
                    // 利用可能オブジェクトの半分を削除（実装はObjectPool側で行う必要がある）
                    Debug.Log($"Pool '{stat.poolName}' usage rate: {usageRate:P1}, considering size reduction");
                }
            }
        }

        /// <summary>
        /// リソース使用の記録
        /// </summary>
        public void RecordTextureUsage(Texture2D texture)
        {
            if (texture != null)
            {
                textureLastUsed[texture] = Time.time;
            }
        }

        public void RecordAudioUsage(AudioClip clip)
        {
            if (clip != null)
            {
                audioLastUsed[clip] = Time.time;
            }
        }

        public void RecordObjectUsage(GameObject obj)
        {
            if (obj != null)
            {
                objectLastUsed[obj] = Time.time;
            }
        }

        /// <summary>
        /// 低メモリ警告の処理
        /// </summary>
        private void OnLowMemoryWarning()
        {
            Debug.LogWarning("Low memory warning received - triggering emergency optimization");
            StartCoroutine(EmergencyOptimization());
        }

        /// <summary>
        /// 緊急最適化
        /// </summary>
        private IEnumerator EmergencyOptimization()
        {
            OnOptimizationStep?.Invoke("Emergency optimization started");

            // より積極的な最適化
            var originalTextureDelay = textureUnloadDelay;
            var originalAudioDelay = audioUnloadDelay;
            
            textureUnloadDelay = 10f; // 10秒に短縮
            audioUnloadDelay = 5f;    // 5秒に短縮

            yield return StartCoroutine(OptimizeMemoryUsage());

            // 追加の緊急処理
            Resources.UnloadUnusedAssets();
            yield return new WaitForSeconds(1f);

            System.GC.Collect();
            yield return new WaitForSeconds(0.5f);

            // 設定を元に戻す
            textureUnloadDelay = originalTextureDelay;
            audioUnloadDelay = originalAudioDelay;

            OnOptimizationStep?.Invoke("Emergency optimization completed");
        }

        /// <summary>
        /// 手動最適化の実行
        /// </summary>
        public void ForceOptimization()
        {
            StartCoroutine(OptimizeMemoryUsage());
        }

        /// <summary>
        /// 統計情報の取得
        /// </summary>
        public MemoryOptimizerStats GetStats()
        {
            return new MemoryOptimizerStats
            {
                initialMemoryMB = initialMemoryUsage / (1024 * 1024),
                currentMemoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024),
                memoryFreedMB = memoryFreed / (1024 * 1024),
                texturesOptimized = texturesOptimized,
                audioClipsOptimized = audioClipsOptimized,
                objectsDestroyed = objectsDestroyed,
                trackedTextures = textureLastUsed.Count,
                trackedAudioClips = audioLastUsed.Count,
                trackedObjects = objectLastUsed.Count
            };
        }

        /// <summary>
        /// 設定の動的変更
        /// </summary>
        public void SetOptimizationSettings(bool enableAuto, float interval, long threshold)
        {
            enableAutoOptimization = enableAuto;
            optimizationInterval = interval;
            memoryThreshold = threshold;
        }

        public void SetTextureSettings(bool optimize, float delay, int maxSize)
        {
            optimizeTextures = optimize;
            textureUnloadDelay = delay;
            maxTextureSize = maxSize;
        }

        public void SetAudioSettings(bool optimize, float delay)
        {
            optimizeAudio = optimize;
            audioUnloadDelay = delay;
        }

        /// <summary>
        /// 統合テスト用の現在のメモリ使用量取得
        /// </summary>
        public long GetCurrentMemoryUsage()
        {
            return System.GC.GetTotalMemory(false);
        }

        private void OnDestroy()
        {
            Application.lowMemory -= OnLowMemoryWarning;
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// メモリ最適化統計情報
    /// </summary>
    [System.Serializable]
    public class MemoryOptimizerStats
    {
        public long initialMemoryMB;
        public long currentMemoryMB;
        public long memoryFreedMB;
        public int texturesOptimized;
        public int audioClipsOptimized;
        public int objectsDestroyed;
        public int trackedTextures;
        public int trackedAudioClips;
        public int trackedObjects;
    }

    /// <summary>
    /// テクスチャスケーリングユーティリティ
    /// </summary>
    public class TextureScale
    {
        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            if (!tex.isReadable) return;

            Color[] pixels = tex.GetPixels();
            Color[] newPixels = new Color[newWidth * newHeight];

            float ratioX = (float)tex.width / newWidth;
            float ratioY = (float)tex.height / newHeight;

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float srcX = x * ratioX;
                    float srcY = y * ratioY;
                    
                    int x1 = Mathf.FloorToInt(srcX);
                    int y1 = Mathf.FloorToInt(srcY);
                    int x2 = Mathf.Min(x1 + 1, tex.width - 1);
                    int y2 = Mathf.Min(y1 + 1, tex.height - 1);

                    float fx = srcX - x1;
                    float fy = srcY - y1;

                    Color c1 = pixels[y1 * tex.width + x1];
                    Color c2 = pixels[y1 * tex.width + x2];
                    Color c3 = pixels[y2 * tex.width + x1];
                    Color c4 = pixels[y2 * tex.width + x2];

                    Color i1 = Color.Lerp(c1, c2, fx);
                    Color i2 = Color.Lerp(c3, c4, fx);
                    Color final = Color.Lerp(i1, i2, fy);

                    newPixels[y * newWidth + x] = final;
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newPixels);
            tex.Apply();
        }
    }
}