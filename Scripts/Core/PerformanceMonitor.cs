using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DungeonOwner.Core
{
    /// <summary>
    /// パフォーマンス監視システム
    /// FPS、メモリ使用量、GC発生頻度を監視し、60FPS維持のための最適化を支援
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        public static PerformanceMonitor Instance { get; private set; }

        [Header("監視設定")]
        [SerializeField] private bool enableMonitoring = true;
        [SerializeField] private float updateInterval = 1.0f;
        [SerializeField] private int frameHistorySize = 60;
        [SerializeField] private bool showDebugUI = false;

        [Header("パフォーマンス閾値")]
        [SerializeField] private float targetFPS = 60f;
        [SerializeField] private float lowFPSThreshold = 45f;
        [SerializeField] private long memoryWarningThreshold = 100 * 1024 * 1024; // 100MB
        [SerializeField] private long memoryCriticalThreshold = 200 * 1024 * 1024; // 200MB

        [Header("自動最適化")]
        [SerializeField] private bool enableAutoOptimization = true;
        [SerializeField] private float optimizationCooldown = 5f;

        // パフォーマンス統計
        private Queue<float> frameTimeHistory = new Queue<float>();
        private Queue<long> memoryHistory = new Queue<long>();
        private Queue<int> gcHistory = new Queue<int>();

        // 現在の統計
        public float CurrentFPS { get; private set; }
        public float AverageFPS { get; private set; }
        public float MinFPS { get; private set; }
        public float MaxFPS { get; private set; }
        public long CurrentMemoryUsage { get; private set; }
        public long PeakMemoryUsage { get; private set; }
        public int GCCollectionCount { get; private set; }
        public int TotalGCCollections { get; private set; }

        // 最適化状態
        private float lastOptimizationTime = 0f;
        private int consecutiveLowFPSFrames = 0;
        private bool isOptimizationActive = false;

        // イベント
        public System.Action<float> OnFPSChanged;
        public System.Action<long> OnMemoryChanged;
        public System.Action OnLowPerformanceDetected;
        public System.Action OnOptimizationTriggered;

        // UI表示用
        private Rect debugRect = new Rect(10, 10, 250, 200);
        private GUIStyle debugStyle;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeMonitor();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (enableMonitoring)
            {
                StartCoroutine(MonitoringCoroutine());
            }
        }

        /// <summary>
        /// 監視システムの初期化
        /// </summary>
        private void InitializeMonitor()
        {
            // フレーム履歴を初期化
            for (int i = 0; i < frameHistorySize; i++)
            {
                frameTimeHistory.Enqueue(1f / targetFPS);
            }

            // メモリ履歴を初期化
            for (int i = 0; i < 10; i++)
            {
                memoryHistory.Enqueue(0);
            }

            // GC履歴を初期化
            for (int i = 0; i < 10; i++)
            {
                gcHistory.Enqueue(0);
            }

            // 初期GC数を記録
            TotalGCCollections = System.GC.CollectionCount(0);

            Debug.Log("PerformanceMonitor initialized");
        }

        /// <summary>
        /// 監視コルーチン
        /// </summary>
        private IEnumerator MonitoringCoroutine()
        {
            while (enableMonitoring)
            {
                UpdatePerformanceStats();
                CheckPerformanceThresholds();
                
                if (enableAutoOptimization)
                {
                    CheckAutoOptimization();
                }

                yield return new WaitForSeconds(updateInterval);
            }
        }

        /// <summary>
        /// パフォーマンス統計の更新
        /// </summary>
        private void UpdatePerformanceStats()
        {
            // FPS計算
            float deltaTime = Time.unscaledDeltaTime;
            CurrentFPS = 1f / deltaTime;

            // フレーム履歴を更新
            if (frameTimeHistory.Count >= frameHistorySize)
            {
                frameTimeHistory.Dequeue();
            }
            frameTimeHistory.Enqueue(deltaTime);

            // 平均、最小、最大FPSを計算
            var frameTimes = frameTimeHistory.ToArray();
            AverageFPS = frameTimes.Length / frameTimes.Sum();
            MinFPS = 1f / frameTimes.Max();
            MaxFPS = 1f / frameTimes.Min();

            // メモリ使用量
            long currentMemory = System.GC.GetTotalMemory(false);
            CurrentMemoryUsage = currentMemory;
            
            if (currentMemory > PeakMemoryUsage)
            {
                PeakMemoryUsage = currentMemory;
            }

            // メモリ履歴を更新
            if (memoryHistory.Count >= 10)
            {
                memoryHistory.Dequeue();
            }
            memoryHistory.Enqueue(currentMemory);

            // GC統計
            int currentGC = System.GC.CollectionCount(0);
            GCCollectionCount = currentGC - TotalGCCollections;
            TotalGCCollections = currentGC;

            // GC履歴を更新
            if (gcHistory.Count >= 10)
            {
                gcHistory.Dequeue();
            }
            gcHistory.Enqueue(GCCollectionCount);

            // イベント発火
            OnFPSChanged?.Invoke(CurrentFPS);
            OnMemoryChanged?.Invoke(CurrentMemoryUsage);
        }

        /// <summary>
        /// パフォーマンス閾値チェック
        /// </summary>
        private void CheckPerformanceThresholds()
        {
            // 低FPS検出
            if (CurrentFPS < lowFPSThreshold)
            {
                consecutiveLowFPSFrames++;
                
                if (consecutiveLowFPSFrames >= 3) // 3回連続で低FPS
                {
                    OnLowPerformanceDetected?.Invoke();
                    
                    if (enableAutoOptimization)
                    {
                        TriggerOptimization("Low FPS detected");
                    }
                }
            }
            else
            {
                consecutiveLowFPSFrames = 0;
            }

            // メモリ使用量チェック
            if (CurrentMemoryUsage > memoryCriticalThreshold)
            {
                Debug.LogWarning($"Critical memory usage: {CurrentMemoryUsage / (1024 * 1024)}MB");
                
                if (enableAutoOptimization)
                {
                    TriggerOptimization("Critical memory usage");
                }
            }
            else if (CurrentMemoryUsage > memoryWarningThreshold)
            {
                Debug.LogWarning($"High memory usage: {CurrentMemoryUsage / (1024 * 1024)}MB");
            }

            // 頻繁なGC検出
            if (GCCollectionCount > 2) // 1秒間に2回以上のGC
            {
                Debug.LogWarning($"Frequent GC detected: {GCCollectionCount} collections");
                
                if (enableAutoOptimization)
                {
                    TriggerOptimization("Frequent GC");
                }
            }
        }

        /// <summary>
        /// 自動最適化チェック
        /// </summary>
        private void CheckAutoOptimization()
        {
            if (isOptimizationActive) return;
            if (Time.time - lastOptimizationTime < optimizationCooldown) return;

            // 平均FPSが目標を下回る場合
            if (AverageFPS < targetFPS * 0.9f)
            {
                TriggerOptimization("Average FPS below target");
            }
        }

        /// <summary>
        /// 最適化処理をトリガー
        /// </summary>
        private void TriggerOptimization(string reason)
        {
            if (isOptimizationActive) return;
            if (Time.time - lastOptimizationTime < optimizationCooldown) return;

            Debug.Log($"Triggering optimization: {reason}");
            
            isOptimizationActive = true;
            lastOptimizationTime = Time.time;
            
            StartCoroutine(OptimizationCoroutine());
            OnOptimizationTriggered?.Invoke();
        }

        /// <summary>
        /// 最適化処理コルーチン
        /// </summary>
        private IEnumerator OptimizationCoroutine()
        {
            // 1. ガベージコレクションを実行
            System.GC.Collect();
            yield return null;

            // 2. オブジェクトプールの最適化
            if (ObjectPool.Instance != null)
            {
                OptimizeObjectPools();
            }
            yield return null;

            // 3. 不要なオブジェクトの削除
            OptimizeGameObjects();
            yield return null;

            // 4. エフェクトの品質を一時的に下げる
            OptimizeEffects();
            yield return null;

            // 5. 音声の最適化
            OptimizeAudio();
            yield return null;

            Debug.Log("Optimization completed");
            isOptimizationActive = false;
        }

        /// <summary>
        /// オブジェクトプールの最適化
        /// </summary>
        private void OptimizeObjectPools()
        {
            if (ObjectPool.Instance == null) return;

            var stats = ObjectPool.Instance.GetAllPoolStats();
            foreach (var stat in stats)
            {
                // 使用率が低いプールのオブジェクトを削減
                if (stat.activeCount == 0 && stat.availableCount > 10)
                {
                    // プールサイズを半分に削減（実装は ObjectPool 側で行う）
                    Debug.Log($"Optimizing pool '{stat.poolName}': reducing size");
                }
            }
        }

        /// <summary>
        /// ゲームオブジェクトの最適化
        /// </summary>
        private void OptimizeGameObjects()
        {
            // 非アクティブなエフェクトオブジェクトを削除
            var effects = GameObject.FindGameObjectsWithTag("Effect");
            foreach (var effect in effects)
            {
                if (!effect.activeInHierarchy)
                {
                    Destroy(effect);
                }
            }

            // 古いダメージテキストを削除
            var damageTexts = GameObject.FindGameObjectsWithTag("DamageText");
            foreach (var text in damageTexts)
            {
                if (text.transform.position.y > 10f) // 画面外に出たテキスト
                {
                    Destroy(text);
                }
            }
        }

        /// <summary>
        /// エフェクトの最適化
        /// </summary>
        private void OptimizeEffects()
        {
            // パーティクルシステムの品質を一時的に下げる
            var particles = FindObjectsOfType<ParticleSystem>();
            foreach (var particle in particles)
            {
                var main = particle.main;
                if (main.maxParticles > 50)
                {
                    main.maxParticles = Mathf.Max(10, main.maxParticles / 2);
                }
            }

            // CombatEffects の品質を下げる
            if (CombatEffects.Instance != null)
            {
                // 実装は CombatEffects 側で行う
                Debug.Log("Reducing combat effects quality");
            }
        }

        /// <summary>
        /// 音声の最適化
        /// </summary>
        private void OptimizeAudio()
        {
            // 再生中でない AudioSource を停止
            var audioSources = FindObjectsOfType<AudioSource>();
            foreach (var source in audioSources)
            {
                if (!source.isPlaying && source.clip != null)
                {
                    source.clip = null;
                }
            }
        }

        /// <summary>
        /// 手動でガベージコレクションを実行
        /// </summary>
        public void ForceGarbageCollection()
        {
            System.GC.Collect();
            Debug.Log("Manual garbage collection executed");
        }

        /// <summary>
        /// パフォーマンス統計をリセット
        /// </summary>
        public void ResetStats()
        {
            frameTimeHistory.Clear();
            memoryHistory.Clear();
            gcHistory.Clear();
            
            PeakMemoryUsage = 0;
            TotalGCCollections = System.GC.CollectionCount(0);
            consecutiveLowFPSFrames = 0;
            
            InitializeMonitor();
            Debug.Log("Performance stats reset");
        }

        /// <summary>
        /// 詳細な統計情報を取得
        /// </summary>
        public PerformanceStats GetDetailedStats()
        {
            return new PerformanceStats
            {
                currentFPS = CurrentFPS,
                averageFPS = AverageFPS,
                minFPS = MinFPS,
                maxFPS = MaxFPS,
                currentMemoryMB = CurrentMemoryUsage / (1024 * 1024),
                peakMemoryMB = PeakMemoryUsage / (1024 * 1024),
                gcCollections = TotalGCCollections,
                recentGCCount = gcHistory.Sum(),
                frameTimeVariance = CalculateFrameTimeVariance(),
                memoryGrowthRate = CalculateMemoryGrowthRate()
            };
        }

        /// <summary>
        /// フレーム時間の分散を計算
        /// </summary>
        private float CalculateFrameTimeVariance()
        {
            if (frameTimeHistory.Count < 2) return 0f;

            var frameTimes = frameTimeHistory.ToArray();
            float mean = frameTimes.Average();
            float variance = frameTimes.Select(x => (x - mean) * (x - mean)).Average();
            
            return variance;
        }

        /// <summary>
        /// メモリ増加率を計算
        /// </summary>
        private float CalculateMemoryGrowthRate()
        {
            if (memoryHistory.Count < 2) return 0f;

            var memories = memoryHistory.ToArray();
            long oldMemory = memories[0];
            long newMemory = memories[memories.Length - 1];
            
            if (oldMemory == 0) return 0f;
            
            return ((float)(newMemory - oldMemory) / oldMemory) * 100f;
        }

        /// <summary>
        /// 監視の有効/無効を切り替え
        /// </summary>
        public void SetMonitoringEnabled(bool enabled)
        {
            enableMonitoring = enabled;
            
            if (enabled && !IsInvoking(nameof(MonitoringCoroutine)))
            {
                StartCoroutine(MonitoringCoroutine());
            }
        }

        /// <summary>
        /// デバッグUI表示の切り替え
        /// </summary>
        public void SetDebugUIEnabled(bool enabled)
        {
            showDebugUI = enabled;
        }

        private void OnGUI()
        {
            if (!showDebugUI) return;

            if (debugStyle == null)
            {
                debugStyle = new GUIStyle(GUI.skin.box);
                debugStyle.alignment = TextAnchor.UpperLeft;
                debugStyle.fontSize = 12;
            }

            var stats = GetDetailedStats();
            string debugText = $"=== Performance Monitor ===\n" +
                              $"FPS: {stats.currentFPS:F1} (Avg: {stats.averageFPS:F1})\n" +
                              $"Min/Max: {stats.minFPS:F1} / {stats.maxFPS:F1}\n" +
                              $"Memory: {stats.currentMemoryMB:F1}MB (Peak: {stats.peakMemoryMB:F1}MB)\n" +
                              $"GC: {stats.gcCollections} (Recent: {stats.recentGCCount})\n" +
                              $"Frame Variance: {stats.frameTimeVariance:F4}\n" +
                              $"Memory Growth: {stats.memoryGrowthRate:F1}%\n" +
                              $"Optimization: {(isOptimizationActive ? "Active" : "Idle")}";

            GUI.Box(debugRect, debugText, debugStyle);
        }

        /// <summary>
        /// 統合テスト用の平均FPS取得
        /// </summary>
        public float GetAverageFPS()
        {
            return AverageFPS;
        }

        /// <summary>
        /// 統合テスト用の現在のメモリ使用量取得
        /// </summary>
        public long GetCurrentMemoryUsage()
        {
            return CurrentMemoryUsage;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// パフォーマンス統計情報
    /// </summary>
    [System.Serializable]
    public class PerformanceStats
    {
        public float currentFPS;
        public float averageFPS;
        public float minFPS;
        public float maxFPS;
        public long currentMemoryMB;
        public long peakMemoryMB;
        public int gcCollections;
        public int recentGCCount;
        public float frameTimeVariance;
        public float memoryGrowthRate;
    }
}