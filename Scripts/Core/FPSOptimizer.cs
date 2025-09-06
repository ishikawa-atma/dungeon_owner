using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace DungeonOwner.Core
{
    /// <summary>
    /// FPS最適化システム
    /// 60FPS維持のための動的品質調整とパフォーマンス最適化
    /// </summary>
    public class FPSOptimizer : MonoBehaviour
    {
        public static FPSOptimizer Instance { get; private set; }

        [Header("FPS設定")]
        [SerializeField] private int targetFPS = 60;
        [SerializeField] private int minAcceptableFPS = 45;
        [SerializeField] private int criticalFPS = 30;
        [SerializeField] private float fpsCheckInterval = 1f;

        [Header("品質レベル")]
        [SerializeField] private QualityLevel currentQualityLevel = QualityLevel.High;
        [SerializeField] private bool enableDynamicQuality = true;
        [SerializeField] private float qualityAdjustmentCooldown = 5f;

        [Header("最適化設定")]
        [SerializeField] private bool enableParticleOptimization = true;
        [SerializeField] private bool enableEffectOptimization = true;
        [SerializeField] private bool enableAudioOptimization = true;
        [SerializeField] private bool enableRenderOptimization = true;

        // FPS追跡
        private Queue<float> fpsHistory = new Queue<float>();
        private float averageFPS = 60f;
        private float lastQualityAdjustment = 0f;
        private int consecutiveLowFPSFrames = 0;

        // 品質設定
        private Dictionary<QualityLevel, QualitySettings> qualitySettings = new Dictionary<QualityLevel, QualitySettings>();
        
        // 最適化状態
        private bool isOptimizing = false;
        private List<IOptimizable> optimizableComponents = new List<IOptimizable>();

        // イベント
        public System.Action<QualityLevel> OnQualityLevelChanged;
        public System.Action<float> OnFPSOptimized;

        public enum QualityLevel
        {
            Low = 0,
            Medium = 1,
            High = 2,
            Ultra = 3
        }

        [System.Serializable]
        public class QualitySettings
        {
            public int maxParticles = 100;
            public float effectScale = 1f;
            public int audioChannels = 32;
            public float renderScale = 1f;
            public bool enableShadows = true;
            public bool enablePostProcessing = true;
            public int textureQuality = 0; // 0 = Full Res
            public int antiAliasing = 4;
        }

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
            // Unity設定を適用
            Application.targetFrameRate = targetFPS;
            QualitySettings.vSyncCount = 0;
            
            StartCoroutine(FPSMonitoringCoroutine());
            
            // 最適化可能なコンポーネントを検索
            FindOptimizableComponents();
        }

        /// <summary>
        /// 最適化システムの初期化
        /// </summary>
        private void InitializeOptimizer()
        {
            // 品質設定を定義
            qualitySettings[QualityLevel.Low] = new QualitySettings
            {
                maxParticles = 25,
                effectScale = 0.5f,
                audioChannels = 16,
                renderScale = 0.75f,
                enableShadows = false,
                enablePostProcessing = false,
                textureQuality = 2,
                antiAliasing = 0
            };

            qualitySettings[QualityLevel.Medium] = new QualitySettings
            {
                maxParticles = 50,
                effectScale = 0.75f,
                audioChannels = 24,
                renderScale = 0.9f,
                enableShadows = true,
                enablePostProcessing = false,
                textureQuality = 1,
                antiAliasing = 2
            };

            qualitySettings[QualityLevel.High] = new QualitySettings
            {
                maxParticles = 100,
                effectScale = 1f,
                audioChannels = 32,
                renderScale = 1f,
                enableShadows = true,
                enablePostProcessing = true,
                textureQuality = 0,
                antiAliasing = 4
            };

            qualitySettings[QualityLevel.Ultra] = new QualitySettings
            {
                maxParticles = 200,
                effectScale = 1.25f,
                audioChannels = 64,
                renderScale = 1f,
                enableShadows = true,
                enablePostProcessing = true,
                textureQuality = 0,
                antiAliasing = 8
            };

            // 初期品質レベルを適用
            ApplyQualityLevel(currentQualityLevel);

            Debug.Log("FPSOptimizer initialized");
        }

        /// <summary>
        /// FPS監視コルーチン
        /// </summary>
        private IEnumerator FPSMonitoringCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(fpsCheckInterval);
                
                UpdateFPSStats();
                
                if (enableDynamicQuality)
                {
                    CheckAndAdjustQuality();
                }
            }
        }

        /// <summary>
        /// FPS統計の更新
        /// </summary>
        private void UpdateFPSStats()
        {
            float currentFPS = 1f / Time.unscaledDeltaTime;
            
            // FPS履歴を更新
            fpsHistory.Enqueue(currentFPS);
            if (fpsHistory.Count > 10) // 10秒間の履歴を保持
            {
                fpsHistory.Dequeue();
            }
            
            // 平均FPSを計算
            float total = 0f;
            foreach (float fps in fpsHistory)
            {
                total += fps;
            }
            averageFPS = total / fpsHistory.Count;
            
            // 低FPSの連続フレーム数をカウント
            if (currentFPS < minAcceptableFPS)
            {
                consecutiveLowFPSFrames++;
            }
            else
            {
                consecutiveLowFPSFrames = 0;
            }
        }

        /// <summary>
        /// 品質レベルの動的調整
        /// </summary>
        private void CheckAndAdjustQuality()
        {
            if (Time.time - lastQualityAdjustment < qualityAdjustmentCooldown)
            {
                return;
            }

            QualityLevel newQualityLevel = currentQualityLevel;

            // 低FPSが続く場合は品質を下げる
            if (consecutiveLowFPSFrames >= 3 || averageFPS < minAcceptableFPS)
            {
                if (currentQualityLevel > QualityLevel.Low)
                {
                    newQualityLevel = currentQualityLevel - 1;
                    Debug.Log($"Lowering quality due to low FPS: {averageFPS:F1}");
                }
                else
                {
                    // 最低品質でもFPSが低い場合は緊急最適化
                    if (averageFPS < criticalFPS)
                    {
                        StartCoroutine(EmergencyOptimization());
                    }
                }
            }
            // FPSが安定している場合は品質を上げる
            else if (averageFPS > targetFPS * 0.95f && consecutiveLowFPSFrames == 0)
            {
                if (currentQualityLevel < QualityLevel.Ultra)
                {
                    newQualityLevel = currentQualityLevel + 1;
                    Debug.Log($"Raising quality due to stable FPS: {averageFPS:F1}");
                }
            }

            if (newQualityLevel != currentQualityLevel)
            {
                SetQualityLevel(newQualityLevel);
            }
        }

        /// <summary>
        /// 品質レベルを設定
        /// </summary>
        public void SetQualityLevel(QualityLevel level)
        {
            if (currentQualityLevel == level) return;

            currentQualityLevel = level;
            lastQualityAdjustment = Time.time;
            
            ApplyQualityLevel(level);
            OnQualityLevelChanged?.Invoke(level);
            
            Debug.Log($"Quality level changed to: {level}");
        }

        /// <summary>
        /// 品質設定を適用
        /// </summary>
        private void ApplyQualityLevel(QualityLevel level)
        {
            if (!qualitySettings.ContainsKey(level)) return;

            var settings = qualitySettings[level];

            // Unity品質設定
            QualitySettings.SetQualityLevel((int)level, true);
            QualitySettings.masterTextureLimit = settings.textureQuality;
            QualitySettings.antiAliasing = settings.antiAliasing;
            QualitySettings.shadows = settings.enableShadows ? ShadowQuality.All : ShadowQuality.Disable;

            // パーティクル最適化
            if (enableParticleOptimization)
            {
                OptimizeParticles(settings);
            }

            // エフェクト最適化
            if (enableEffectOptimization)
            {
                OptimizeEffects(settings);
            }

            // オーディオ最適化
            if (enableAudioOptimization)
            {
                OptimizeAudio(settings);
            }

            // レンダリング最適化
            if (enableRenderOptimization)
            {
                OptimizeRendering(settings);
            }

            // 最適化可能なコンポーネントに通知
            foreach (var component in optimizableComponents)
            {
                component?.OnQualityChanged(level, settings);
            }
        }

        /// <summary>
        /// パーティクルシステムの最適化
        /// </summary>
        private void OptimizeParticles(QualitySettings settings)
        {
            var particleSystems = FindObjectsOfType<ParticleSystem>();
            
            foreach (var ps in particleSystems)
            {
                var main = ps.main;
                
                // 最大パーティクル数を制限
                if (main.maxParticles > settings.maxParticles)
                {
                    main.maxParticles = settings.maxParticles;
                }
                
                // エミッション率を調整
                var emission = ps.emission;
                if (emission.enabled)
                {
                    var rateOverTime = emission.rateOverTime;
                    rateOverTime.constant *= settings.effectScale;
                    emission.rateOverTime = rateOverTime;
                }
            }
        }

        /// <summary>
        /// エフェクトの最適化
        /// </summary>
        private void OptimizeEffects(QualitySettings settings)
        {
            // CombatEffectsの品質を調整
            if (CombatEffects.Instance != null)
            {
                // 実装はCombatEffects側で行う
                Debug.Log($"Optimizing effects with scale: {settings.effectScale}");
            }

            // その他のエフェクトシステムの最適化
            var effectObjects = GameObject.FindGameObjectsWithTag("Effect");
            foreach (var effect in effectObjects)
            {
                var scale = effect.transform.localScale;
                effect.transform.localScale = scale * settings.effectScale;
            }
        }

        /// <summary>
        /// オーディオの最適化
        /// </summary>
        private void OptimizeAudio(QualitySettings settings)
        {
            // オーディオチャンネル数を制限
            AudioSettings.GetConfiguration(out var config);
            config.numVirtualVoices = settings.audioChannels;
            config.numRealVoices = Mathf.Min(settings.audioChannels, config.numRealVoices);
            AudioSettings.Reset(config);

            // 距離の遠いAudioSourceを無効化
            var audioSources = FindObjectsOfType<AudioSource>();
            var cameraPos = Camera.main?.transform.position ?? Vector3.zero;
            
            foreach (var source in audioSources)
            {
                float distance = Vector3.Distance(source.transform.position, cameraPos);
                
                // 品質レベルに応じて聞こえる距離を調整
                float maxDistance = settings.audioChannels * 0.5f; // チャンネル数に比例
                source.enabled = distance <= maxDistance;
            }
        }

        /// <summary>
        /// レンダリングの最適化
        /// </summary>
        private void OptimizeRendering(QualitySettings settings)
        {
            // レンダースケールの調整（カメラがある場合）
            var cameras = FindObjectsOfType<Camera>();
            foreach (var camera in cameras)
            {
                // レンダーテクスチャのスケールを調整
                if (settings.renderScale < 1f)
                {
                    camera.pixelRect = new Rect(0, 0, 
                        Screen.width * settings.renderScale, 
                        Screen.height * settings.renderScale);
                }
                else
                {
                    camera.pixelRect = new Rect(0, 0, Screen.width, Screen.height);
                }
            }

            // LOD（Level of Detail）の調整
            QualitySettings.lodBias = settings.renderScale;
        }

        /// <summary>
        /// 緊急最適化
        /// </summary>
        private IEnumerator EmergencyOptimization()
        {
            if (isOptimizing) yield break;
            
            isOptimizing = true;
            Debug.LogWarning("Emergency optimization triggered!");

            // 1. 全てのパーティクルを一時停止
            var particleSystems = FindObjectsOfType<ParticleSystem>();
            foreach (var ps in particleSystems)
            {
                ps.Pause();
            }
            yield return null;

            // 2. 非必須エフェクトを無効化
            var effects = GameObject.FindGameObjectsWithTag("Effect");
            foreach (var effect in effects)
            {
                effect.SetActive(false);
            }
            yield return null;

            // 3. オーディオを最小限に
            var audioSources = FindObjectsOfType<AudioSource>();
            foreach (var source in audioSources)
            {
                if (!source.isPlaying)
                {
                    source.enabled = false;
                }
            }
            yield return null;

            // 4. ガベージコレクション
            System.GC.Collect();
            yield return new WaitForSeconds(0.5f);

            // 5. 最低品質に設定
            SetQualityLevel(QualityLevel.Low);
            yield return null;

            Debug.Log("Emergency optimization completed");
            isOptimizing = false;
        }

        /// <summary>
        /// 最適化可能なコンポーネントを検索
        /// </summary>
        private void FindOptimizableComponents()
        {
            optimizableComponents.Clear();
            
            var components = FindObjectsOfType<MonoBehaviour>();
            foreach (var component in components)
            {
                if (component is IOptimizable optimizable)
                {
                    optimizableComponents.Add(optimizable);
                }
            }
            
            Debug.Log($"Found {optimizableComponents.Count} optimizable components");
        }

        /// <summary>
        /// 手動最適化の実行
        /// </summary>
        public void ForceOptimization()
        {
            StartCoroutine(EmergencyOptimization());
        }

        /// <summary>
        /// FPS統計の取得
        /// </summary>
        public FPSStats GetFPSStats()
        {
            return new FPSStats
            {
                currentFPS = 1f / Time.unscaledDeltaTime,
                averageFPS = averageFPS,
                targetFPS = targetFPS,
                currentQualityLevel = currentQualityLevel,
                consecutiveLowFPSFrames = consecutiveLowFPSFrames,
                isOptimizing = isOptimizing
            };
        }

        /// <summary>
        /// 設定の動的変更
        /// </summary>
        public void SetTargetFPS(int fps)
        {
            targetFPS = fps;
            Application.targetFrameRate = fps;
        }

        public void SetMinAcceptableFPS(int fps)
        {
            minAcceptableFPS = fps;
        }

        public void SetDynamicQualityEnabled(bool enabled)
        {
            enableDynamicQuality = enabled;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }

    /// <summary>
    /// FPS統計情報
    /// </summary>
    [System.Serializable]
    public class FPSStats
    {
        public float currentFPS;
        public float averageFPS;
        public int targetFPS;
        public FPSOptimizer.QualityLevel currentQualityLevel;
        public int consecutiveLowFPSFrames;
        public bool isOptimizing;
    }

    /// <summary>
    /// 最適化可能なコンポーネント用インターフェース
    /// </summary>
    public interface IOptimizable
    {
        void OnQualityChanged(FPSOptimizer.QualityLevel level, FPSOptimizer.QualitySettings settings);
    }
}