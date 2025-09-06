using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 汎用オブジェクトプールシステム
    /// 頻繁に生成・破棄されるオブジェクトのメモリ使用量とGC負荷を最適化
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        [System.Serializable]
        public class PoolConfig
        {
            public string poolName;
            public GameObject prefab;
            public int initialSize = 10;
            public int maxSize = 100;
            public bool allowGrowth = true;
            public float autoReturnTime = 0f; // 0なら自動回収しない
        }

        [Header("プール設定")]
        [SerializeField] private List<PoolConfig> poolConfigs = new List<PoolConfig>();
        
        [Header("デバッグ")]
        [SerializeField] private bool enableDebugLog = false;
        [SerializeField] private bool showPoolStats = false;

        private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
        private Dictionary<string, List<GameObject>> activeObjects = new Dictionary<string, List<GameObject>>();
        private Dictionary<string, PoolConfig> configs = new Dictionary<string, PoolConfig>();
        private Dictionary<GameObject, string> objectToPool = new Dictionary<GameObject, string>();
        private Dictionary<GameObject, float> autoReturnTimers = new Dictionary<GameObject, float>();

        // パフォーマンス統計
        private Dictionary<string, int> spawnCounts = new Dictionary<string, int>();
        private Dictionary<string, int> returnCounts = new Dictionary<string, int>();
        private Dictionary<string, int> createCounts = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePools();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // 自動回収システムを開始
            StartCoroutine(AutoReturnCoroutine());
        }

        /// <summary>
        /// プールの初期化
        /// </summary>
        private void InitializePools()
        {
            foreach (var config in poolConfigs)
            {
                CreatePool(config);
            }

            Debug.Log($"ObjectPool initialized with {poolConfigs.Count} pools");
        }

        /// <summary>
        /// 新しいプールを作成
        /// </summary>
        public void CreatePool(PoolConfig config)
        {
            if (pools.ContainsKey(config.poolName))
            {
                Debug.LogWarning($"Pool '{config.poolName}' already exists");
                return;
            }

            var pool = new Queue<GameObject>();
            var activeList = new List<GameObject>();

            // 初期オブジェクトを生成
            for (int i = 0; i < config.initialSize; i++)
            {
                GameObject obj = CreatePooledObject(config);
                pool.Enqueue(obj);
            }

            pools[config.poolName] = pool;
            activeObjects[config.poolName] = activeList;
            configs[config.poolName] = config;
            spawnCounts[config.poolName] = 0;
            returnCounts[config.poolName] = 0;
            createCounts[config.poolName] = config.initialSize;

            if (enableDebugLog)
            {
                Debug.Log($"Created pool '{config.poolName}' with {config.initialSize} objects");
            }
        }

        /// <summary>
        /// プールからオブジェクトを取得
        /// </summary>
        public GameObject Spawn(string poolName, Vector3 position = default, Quaternion rotation = default, Transform parent = null)
        {
            if (!pools.ContainsKey(poolName))
            {
                Debug.LogError($"Pool '{poolName}' not found");
                return null;
            }

            GameObject obj = GetPooledObject(poolName);
            if (obj == null)
            {
                return null;
            }

            // オブジェクトを設定
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.SetParent(parent);
            obj.SetActive(true);

            // アクティブリストに追加
            activeObjects[poolName].Add(obj);
            objectToPool[obj] = poolName;
            spawnCounts[poolName]++;

            // 自動回収タイマーを設定
            var config = configs[poolName];
            if (config.autoReturnTime > 0)
            {
                autoReturnTimers[obj] = Time.time + config.autoReturnTime;
            }

            // IPoolable インターフェースがあれば OnSpawn を呼び出し
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnSpawn();

            if (enableDebugLog)
            {
                Debug.Log($"Spawned '{poolName}' object at {position}");
            }

            return obj;
        }

        /// <summary>
        /// オブジェクトをプールに返却
        /// </summary>
        public void Return(GameObject obj)
        {
            if (obj == null) return;

            if (!objectToPool.ContainsKey(obj))
            {
                Debug.LogWarning($"Object {obj.name} is not from any pool");
                return;
            }

            string poolName = objectToPool[obj];
            ReturnToPool(obj, poolName);
        }

        /// <summary>
        /// 指定プールのオブジェクトをすべて返却
        /// </summary>
        public void ReturnAll(string poolName)
        {
            if (!activeObjects.ContainsKey(poolName))
            {
                return;
            }

            var activeList = activeObjects[poolName];
            var objectsToReturn = new List<GameObject>(activeList);

            foreach (var obj in objectsToReturn)
            {
                if (obj != null)
                {
                    ReturnToPool(obj, poolName);
                }
            }

            if (enableDebugLog)
            {
                Debug.Log($"Returned all objects from pool '{poolName}'");
            }
        }

        /// <summary>
        /// すべてのプールのオブジェクトを返却
        /// </summary>
        public void ReturnAllObjects()
        {
            foreach (var poolName in pools.Keys)
            {
                ReturnAll(poolName);
            }
        }

        /// <summary>
        /// プールからオブジェクトを取得（内部処理）
        /// </summary>
        private GameObject GetPooledObject(string poolName)
        {
            var pool = pools[poolName];
            var config = configs[poolName];

            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }

            // プールが空の場合
            if (config.allowGrowth && GetTotalObjectCount(poolName) < config.maxSize)
            {
                // 新しいオブジェクトを作成
                GameObject newObj = CreatePooledObject(config);
                createCounts[poolName]++;
                
                if (enableDebugLog)
                {
                    Debug.Log($"Pool '{poolName}' expanded, created new object");
                }
                
                return newObj;
            }

            Debug.LogWarning($"Pool '{poolName}' is full and cannot grow");
            return null;
        }

        /// <summary>
        /// プール用オブジェクトを作成
        /// </summary>
        private GameObject CreatePooledObject(PoolConfig config)
        {
            GameObject obj = Instantiate(config.prefab);
            obj.name = $"{config.prefab.name}_Pooled";
            obj.SetActive(false);
            obj.transform.SetParent(transform);

            // IPoolable インターフェースがあれば初期化
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnPoolCreated();

            return obj;
        }

        /// <summary>
        /// オブジェクトをプールに返却（内部処理）
        /// </summary>
        private void ReturnToPool(GameObject obj, string poolName)
        {
            if (obj == null) return;

            // IPoolable インターフェースがあれば OnReturn を呼び出し
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnReturn();

            // オブジェクトをリセット
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            obj.transform.position = Vector3.zero;
            obj.transform.rotation = Quaternion.identity;

            // プールに戻す
            pools[poolName].Enqueue(obj);
            activeObjects[poolName].Remove(obj);
            objectToPool.Remove(obj);
            autoReturnTimers.Remove(obj);
            returnCounts[poolName]++;

            if (enableDebugLog)
            {
                Debug.Log($"Returned '{poolName}' object to pool");
            }
        }

        /// <summary>
        /// 自動回収コルーチン
        /// </summary>
        private IEnumerator AutoReturnCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f); // 1秒ごとにチェック

                var objectsToReturn = new List<GameObject>();
                float currentTime = Time.time;

                foreach (var kvp in autoReturnTimers)
                {
                    if (currentTime >= kvp.Value)
                    {
                        objectsToReturn.Add(kvp.Key);
                    }
                }

                foreach (var obj in objectsToReturn)
                {
                    Return(obj);
                }
            }
        }

        /// <summary>
        /// プールの統計情報を取得
        /// </summary>
        public PoolStats GetPoolStats(string poolName)
        {
            if (!pools.ContainsKey(poolName))
            {
                return null;
            }

            return new PoolStats
            {
                poolName = poolName,
                availableCount = pools[poolName].Count,
                activeCount = activeObjects[poolName].Count,
                totalCreated = createCounts[poolName],
                totalSpawned = spawnCounts[poolName],
                totalReturned = returnCounts[poolName],
                maxSize = configs[poolName].maxSize
            };
        }

        /// <summary>
        /// すべてのプールの統計情報を取得
        /// </summary>
        public List<PoolStats> GetAllPoolStats()
        {
            var stats = new List<PoolStats>();
            foreach (var poolName in pools.Keys)
            {
                stats.Add(GetPoolStats(poolName));
            }
            return stats;
        }

        /// <summary>
        /// プールの総オブジェクト数を取得
        /// </summary>
        private int GetTotalObjectCount(string poolName)
        {
            return pools[poolName].Count + activeObjects[poolName].Count;
        }

        /// <summary>
        /// プールをクリア
        /// </summary>
        public void ClearPool(string poolName)
        {
            if (!pools.ContainsKey(poolName))
            {
                return;
            }

            // アクティブオブジェクトを破棄
            foreach (var obj in activeObjects[poolName])
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            // プール内オブジェクトを破棄
            while (pools[poolName].Count > 0)
            {
                var obj = pools[poolName].Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }

            activeObjects[poolName].Clear();
            spawnCounts[poolName] = 0;
            returnCounts[poolName] = 0;
            createCounts[poolName] = 0;

            Debug.Log($"Cleared pool '{poolName}'");
        }

        /// <summary>
        /// すべてのプールをクリア
        /// </summary>
        public void ClearAllPools()
        {
            foreach (var poolName in pools.Keys)
            {
                ClearPool(poolName);
            }
        }

        private void OnGUI()
        {
            if (!showPoolStats) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("=== Object Pool Stats ===");

            foreach (var stats in GetAllPoolStats())
            {
                GUILayout.Label($"{stats.poolName}:");
                GUILayout.Label($"  Available: {stats.availableCount}");
                GUILayout.Label($"  Active: {stats.activeCount}");
                GUILayout.Label($"  Created: {stats.totalCreated}");
                GUILayout.Label($"  Spawned: {stats.totalSpawned}");
                GUILayout.Label($"  Returned: {stats.totalReturned}");
                GUILayout.Space(5);
            }

            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            ClearAllPools();
        }
    }

    /// <summary>
    /// プール統計情報
    /// </summary>
    [System.Serializable]
    public class PoolStats
    {
        public string poolName;
        public int availableCount;
        public int activeCount;
        public int totalCreated;
        public int totalSpawned;
        public int totalReturned;
        public int maxSize;
    }

    /// <summary>
    /// プール対応オブジェクト用インターフェース
    /// </summary>
    public interface IPoolable
    {
        void OnPoolCreated();  // プール作成時
        void OnSpawn();        // プールから取得時
        void OnReturn();       // プールに返却時
    }
}