using UnityEngine;
using System.Collections;

namespace DungeonOwner.Core
{
    /// <summary>
    /// パフォーマンス管理システム
    /// ObjectPool、PerformanceMonitor、MemoryOptimizer、FPSOptimizerを統合管理
    /// </summary>
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }

        [Header("システム有効化")]
        [SerializeField] private bool enableObjectPool = true;
        [SerializeField] private bool enablePerformanceMonitor = true;
        [SerializeField] private bool enableMemoryOptimizer = true;
        [SerializeField] private bool enableFPSOptimizer = true;

        [Header("自動最適化")]
        [SerializeField] private bool enableAutoOptimization = true;
        [SerializeField] private float optimizationCheckInterval = 10f;
        [SerializeField] private float criticalPerformanceThreshold = 30f; // FPS

        [Header("デバッグ")]
        [SerializeField] private bool showPerformanceUI = false;
        [SerializeField] private KeyCode toggleUIKey = KeyCode.F1;

        // システム参照
        private ObjectPool objectPool;
        private PerformanceMonitor performanceMonitor;
        private MemoryOptimizer memoryOptimizer;
        private FPSOptimizer fpsOptimizer;

        // 統計
        private bool isInitialized = false;
        private float lastOptimizationCheck = 0f;

        // イベント
        public System.Action OnPerformanceSystemsInitialized;
        public System.Action<string> OnOptimizationTriggered;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePerformanceSystems();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (enableAutoOptimization)
            {
                StartCoroutine(AutoOptimizationCoroutine());
            }
        }

        private void Update()
        {
            // デバッグUI切り替え
            if (Input.GetKeyDown(toggleUIKey))
            {
                showPerformanceUI = !showPerformanceUI;
            }
        }

        /// <summary>
        /// パフォーマンスシステムの初期化
        /// </summary>
        private void InitializePerformanceSystems()
        {
            Debug.Log("Initializing performance systems...");

            // ObjectPool初期化
            if (enableObjectPool)
            {
                InitializeObjectPool();
            }

            // PerformanceMonitor初期化
            if (enablePerformanceMonitor)
            {
                InitializePerformanceMonitor();
            }

            // MemoryOptimizer初期化
            if (enableMemoryOptimizer)
            {
                InitializeMemoryOptimizer();
            }

            // FPSOptimizer初期化
            if (enableFPSOptimizer)
            {
                InitializeFPSOptimizer();
            }

            // イベント接続
            ConnectSystemEvents();

            isInitialized = true;
            OnPerformanceSystemsInitialized?.Invoke();
            
            Debug.Log("Performance systems initialized successfully");
        }

        /// <summary>
        /// ObjectPoolの初期化
        /// </summary>
        private void InitializeObjectPool()
        {
            if (ObjectPool.Instance == null)
            {
                GameObject poolObj = new GameObject("ObjectPool");
                poolObj.transform.SetParent(transform);
                objectPool = poolObj.AddComponent<ObjectPool>();

                // 基本的なプール設定を追加
                SetupDefaultPools();
            }
            else
            {
                objectPool = ObjectPool.Instance;
            }
        }

        /// <summary>
        /// デフォルトプールの設定
        /// </summary>
        private void SetupDefaultPools()
        {
            if (objectPool == null) return;

            // 侵入者プール設定（プレハブが利用可能な場合）
            var invaderTypes = System.Enum.GetValues(typeof(Data.InvaderType));
            foreach (Data.InvaderType type in invaderTypes)
            {
                var config = new ObjectPool.PoolConfig
                {
                    poolName = $"Invader_{type}",
                    initialSize = 5,
                    maxSize = 20,
                    allowGrowth = true,
                    autoReturnTime = 0f
                };
                
                // プレハブが設定されている場合のみプールを作成
                if (DataManager.Instance != null)
                {
                    var invaderData = DataManager.Instance.GetInvaderData(type);
                    if (invaderData != null && invaderData.prefab != null)
                    {
                        config.prefab = invaderData.prefab;
                        objectPool.CreatePool(config);
                    }
                }
            }

            // エフェクトプール設定
            CreateEffectPools();
        }

        /// <summary>
        /// エフェクトプールの作成
        /// </summary>
        private void CreateEffectPools()
        {
            // スポーンエフェクト
            var spawnEffectConfig = new ObjectPool.PoolConfig
            {
                poolName = "SpawnEffect",
                initialSize = 10,
                maxSize = 30,
                allowGrowth = true,
                autoReturnTime = 2f
            };

            // ダメージエフェクト
            var damageEffectConfig = new ObjectPool.PoolConfig
            {
                poolName = "DamageEffect",
                initialSize = 20,
                maxSize = 50,
                allowGrowth = true,
                autoReturnTime = 1f
            };

            // ノックバックエフェクト
            var knockbackEffectConfig = new ObjectPool.PoolConfig
            {
                poolName = "KnockbackEffect",
                initialSize = 15,
                maxSize = 40,
                allowGrowth = true,
                autoReturnTime = 0.5f
            };

            // プレハブが設定されている場合のみプールを作成
            // 実際のプレハブ参照は後で設定する必要がある
        }

        /// <summary>
        /// PerformanceMonitorの初期化
        /// </summary>
        private void InitializePerformanceMonitor()
        {
            if (PerformanceMonitor.Instance == null)
            {
                GameObject monitorObj = new GameObject("PerformanceMonitor");
                monitorObj.transform.SetParent(transform);
                performanceMonitor = monitorObj.AddComponent<PerformanceMonitor>();
            }
            else
            {
                performanceMonitor = PerformanceMonitor.Instance;
            }
        }

        /// <summary>
        /// MemoryOptimizerの初期化
        /// </summary>
        private void InitializeMemoryOptimizer()
        {
            if (MemoryOptimizer.Instance == null)
            {
                GameObject optimizerObj = new GameObject("MemoryOptimizer");
                optimizerObj.transform.SetParent(transform);
                memoryOptimizer = optimizerObj.AddComponent<MemoryOptimizer>();
            }
            else
            {
                memoryOptimizer = MemoryOptimizer.Instance;
            }
        }

        /// <summary>
        /// FPSOptimizerの初期化
        /// </summary>
        private void InitializeFPSOptimizer()
        {
            if (FPSOptimizer.Instance == null)
            {
                GameObject fpsObj = new GameObject("FPSOptimizer");
                fpsObj.transform.SetParent(transform);
                fpsOptimizer = fpsObj.AddComponent<FPSOptimizer>();
            }
            else
            {
                fpsOptimizer = FPSOptimizer.Instance;
            }
        }

        /// <summary>
        /// システム間のイベント接続
        /// </summary>
        private void ConnectSystemEvents()
        {
            // PerformanceMonitorのイベント接続
            if (performanceMonitor != null)
            {
                performanceMonitor.OnLowPerformanceDetected += OnLowPerformanceDetected;
                performanceMonitor.OnOptimizationTriggered += OnOptimizationTriggered;
            }

            // MemoryOptimizerのイベント接続
            if (memoryOptimizer != null)
            {
                memoryOptimizer.OnMemoryOptimized += OnMemoryOptimized;
                memoryOptimizer.OnOptimizationStep += OnOptimizationStep;
            }

            // FPSOptimizerのイベント接続
            if (fpsOptimizer != null)
            {
                fpsOptimizer.OnQualityLevelChanged += OnQualityLevelChanged;
                fpsOptimizer.OnFPSOptimized += OnFPSOptimized;
            }
        }

        /// <summary>
        /// 自動最適化コルーチン
        /// </summary>
        private IEnumerator AutoOptimizationCoroutine()
        {
            while (enableAutoOptimization)
            {
                yield return new WaitForSeconds(optimizationCheckInterval);
                
                if (ShouldTriggerOptimization())
                {
                    TriggerComprehensiveOptimization();
                }
            }
        }

        /// <summary>
        /// 最適化が必要かチェック
        /// </summary>
        private bool ShouldTriggerOptimization()
        {
            if (!isInitialized) return false;

            // FPSチェック
            if (performanceMonitor != null)
            {
                var stats = performanceMonitor.GetDetailedStats();
                if (stats.currentFPS < criticalPerformanceThreshold)
                {
                    return true;
                }
            }

            // メモリチェック
            if (memoryOptimizer != null)
            {
                var stats = memoryOptimizer.GetStats();
                if (stats.currentMemoryMB > 150) // 150MB以上
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 包括的最適化の実行
        /// </summary>
        public void TriggerComprehensiveOptimization()
        {
            if (!isInitialized) return;

            Debug.Log("Triggering comprehensive optimization");
            OnOptimizationTriggered?.Invoke("Comprehensive optimization");

            StartCoroutine(ComprehensiveOptimizationCoroutine());
        }

        /// <summary>
        /// 包括的最適化コルーチン
        /// </summary>
        private IEnumerator ComprehensiveOptimizationCoroutine()
        {
            // 1. FPS最適化
            if (fpsOptimizer != null)
            {
                fpsOptimizer.ForceOptimization();
                yield return new WaitForSeconds(0.5f);
            }

            // 2. メモリ最適化
            if (memoryOptimizer != null)
            {
                yield return StartCoroutine(memoryOptimizer.OptimizeMemoryUsage());
            }

            // 3. オブジェクトプールの最適化
            if (objectPool != null)
            {
                OptimizeObjectPools();
                yield return null;
            }

            // 4. パフォーマンス監視のリセット
            if (performanceMonitor != null)
            {
                performanceMonitor.ResetStats();
            }

            Debug.Log("Comprehensive optimization completed");
        }

        /// <summary>
        /// オブジェクトプールの最適化
        /// </summary>
        private void OptimizeObjectPools()
        {
            if (objectPool == null) return;

            var stats = objectPool.GetAllPoolStats();
            foreach (var stat in stats)
            {
                // 使用率が低いプールのサイズを削減
                float usageRate = stat.activeCount > 0 ? 
                    (float)stat.activeCount / (stat.activeCount + stat.availableCount) : 0f;

                if (usageRate < 0.1f && stat.availableCount > 10)
                {
                    Debug.Log($"Pool '{stat.poolName}' has low usage rate: {usageRate:P1}");
                    // 実際の削減処理はObjectPool側で実装する必要がある
                }
            }
        }

        /// <summary>
        /// 統計情報の取得
        /// </summary>
        public PerformanceManagerStats GetStats()
        {
            var stats = new PerformanceManagerStats
            {
                isInitialized = isInitialized,
                systemsEnabled = new bool[]
                {
                    enableObjectPool,
                    enablePerformanceMonitor,
                    enableMemoryOptimizer,
                    enableFPSOptimizer
                }
            };

            if (performanceMonitor != null)
            {
                stats.performanceStats = performanceMonitor.GetDetailedStats();
            }

            if (memoryOptimizer != null)
            {
                stats.memoryStats = memoryOptimizer.GetStats();
            }

            if (fpsOptimizer != null)
            {
                stats.fpsStats = fpsOptimizer.GetFPSStats();
            }

            if (objectPool != null)
            {
                stats.poolStats = objectPool.GetAllPoolStats();
            }

            return stats;
        }

        // イベントハンドラー
        private void OnLowPerformanceDetected()
        {
            Debug.LogWarning("Low performance detected by PerformanceMonitor");
            
            if (enableAutoOptimization)
            {
                TriggerComprehensiveOptimization();
            }
        }

        private void OnMemoryOptimized(long memoryFreed)
        {
            Debug.Log($"Memory optimized: {memoryFreed / (1024 * 1024)}MB freed");
        }

        private void OnOptimizationStep(string step)
        {
            Debug.Log($"Optimization step: {step}");
        }

        private void OnQualityLevelChanged(FPSOptimizer.QualityLevel level)
        {
            Debug.Log($"Quality level changed to: {level}");
        }

        private void OnFPSOptimized(float fps)
        {
            Debug.Log($"FPS optimized to: {fps:F1}");
        }

        /// <summary>
        /// 設定の動的変更
        /// </summary>
        public void SetAutoOptimizationEnabled(bool enabled)
        {
            enableAutoOptimization = enabled;
        }

        public void SetOptimizationCheckInterval(float interval)
        {
            optimizationCheckInterval = interval;
        }

        public void SetCriticalPerformanceThreshold(float threshold)
        {
            criticalPerformanceThreshold = threshold;
        }

        /// <summary>
        /// 手動最適化メソッド
        /// </summary>
        public void ForceMemoryOptimization()
        {
            memoryOptimizer?.ForceOptimization();
        }

        public void ForceFPSOptimization()
        {
            fpsOptimizer?.ForceOptimization();
        }

        public void ForceGarbageCollection()
        {
            performanceMonitor?.ForceGarbageCollection();
        }

        public void ClearAllPools()
        {
            objectPool?.ClearAllPools();
        }

        private void OnGUI()
        {
#if UNITY_EDITOR
            if (!showPerformanceUI) return;

            var stats = GetStats();
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 600));
            GUILayout.Label("=== Performance Manager ===");
            
            if (stats.performanceStats != null)
            {
                var perfStats = stats.performanceStats;
                GUILayout.Label($"FPS: {perfStats.currentFPS:F1} (Avg: {perfStats.averageFPS:F1})");
                GUILayout.Label($"Memory: {perfStats.currentMemoryMB}MB (Peak: {perfStats.peakMemoryMB}MB)");
                GUILayout.Label($"GC: {perfStats.gcCollections}");
            }

            if (stats.fpsStats != null)
            {
                var fpsStats = stats.fpsStats;
                GUILayout.Label($"Quality: {fpsStats.currentQualityLevel}");
                GUILayout.Label($"Target FPS: {fpsStats.targetFPS}");
            }

            GUILayout.Space(10);
            
            if (GUILayout.Button("Force Comprehensive Optimization"))
            {
                TriggerComprehensiveOptimization();
            }
            
            if (GUILayout.Button("Force Memory Optimization"))
            {
                ForceMemoryOptimization();
            }
            
            if (GUILayout.Button("Force FPS Optimization"))
            {
                ForceFPSOptimization();
            }
            
            if (GUILayout.Button("Force Garbage Collection"))
            {
                ForceGarbageCollection();
            }

            GUILayout.EndArea();
#endif
        }

        private void OnDestroy()
        {
            // イベント購読解除
            if (performanceMonitor != null)
            {
                performanceMonitor.OnLowPerformanceDetected -= OnLowPerformanceDetected;
                performanceMonitor.OnOptimizationTriggered -= OnOptimizationTriggered;
            }

            if (memoryOptimizer != null)
            {
                memoryOptimizer.OnMemoryOptimized -= OnMemoryOptimized;
                memoryOptimizer.OnOptimizationStep -= OnOptimizationStep;
            }

            if (fpsOptimizer != null)
            {
                fpsOptimizer.OnQualityLevelChanged -= OnQualityLevelChanged;
                fpsOptimizer.OnFPSOptimized -= OnFPSOptimized;
            }

            StopAllCoroutines();
        }
    }

    /// <summary>
    /// パフォーマンスマネージャー統計情報
    /// </summary>
    [System.Serializable]
    public class PerformanceManagerStats
    {
        public bool isInitialized;
        public bool[] systemsEnabled;
        public PerformanceStats performanceStats;
        public MemoryOptimizerStats memoryStats;
        public FPSStats fpsStats;
        public System.Collections.Generic.List<PoolStats> poolStats;
    }
}
