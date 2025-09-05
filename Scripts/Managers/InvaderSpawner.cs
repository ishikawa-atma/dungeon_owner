using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DungeonOwner.Data;
using DungeonOwner.Core;
using DungeonOwner.Invaders;

namespace DungeonOwner.Managers
{
    public class InvaderSpawner : MonoBehaviour
    {
        public static InvaderSpawner Instance { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private float baseSpawnInterval = 15f; // 基本出現間隔（秒）
        [SerializeField] private float spawnIntervalVariation = 5f; // 出現間隔のランダム幅
        [SerializeField] private int maxConcurrentInvaders = 10; // 同時出現最大数
        [SerializeField] private float preventConsecutiveSpawnTime = 3f; // 連続出現防止時間

        [Header("Depth-Based Frequency")]
        [SerializeField] private float frequencyScalingFactor = 0.1f; // 階層ごとの頻度上昇率
        [SerializeField] private float minSpawnInterval = 5f; // 最小出現間隔
        [SerializeField] private int partySpawnStartFloor = 5; // パーティ出現開始階層
        [SerializeField] private float partySpawnChance = 0.3f; // パーティ出現確率

        [Header("Level Scaling")]
        [SerializeField] private float levelScalingFactor = 0.2f; // 階層ごとのレベル上昇率
        [SerializeField] private int baseInvaderLevel = 1;

        [Header("Spawn Effects")]
        [SerializeField] private GameObject spawnEffectPrefab;
        [SerializeField] private Transform invaderContainer;

        [Header("Random Timing Control")]
        [SerializeField] private AnimationCurve spawnProbabilityCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private float randomSpawnCheckInterval = 1f; // ランダム出現チェック間隔

        private float lastSpawnTime = -15f;
        private int currentActiveInvaders = 0;
        private Coroutine spawnCoroutine;
        private Coroutine randomSpawnCoroutine;

        // イベント
        public System.Action<GameObject> OnInvaderSpawned;
        public System.Action<GameObject> OnInvaderDefeated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeSpawner();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // ゲーム開始時にスポーンを開始
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += OnGameStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= OnGameStateChanged;
            }
        }

        private void InitializeSpawner()
        {
            if (invaderContainer == null)
            {
                GameObject container = new GameObject("InvaderContainer");
                invaderContainer = container.transform;
                container.transform.SetParent(transform);
            }

            Debug.Log("InvaderSpawner initialized");
        }

        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Playing:
                    StartSpawning();
                    break;
                case GameState.Paused:
                case GameState.GameOver:
                    StopSpawning();
                    break;
            }
        }

        public void StartSpawning()
        {
            if (spawnCoroutine == null)
            {
                spawnCoroutine = StartCoroutine(SpawnLoop());
                Debug.Log("Invader spawning started");
            }

            if (randomSpawnCoroutine == null)
            {
                randomSpawnCoroutine = StartCoroutine(RandomSpawnLoop());
                Debug.Log("Random invader spawning started");
            }
        }

        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
                Debug.Log("Invader spawning stopped");
            }

            if (randomSpawnCoroutine != null)
            {
                StopCoroutine(randomSpawnCoroutine);
                randomSpawnCoroutine = null;
                Debug.Log("Random invader spawning stopped");
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                // 階層深度に応じた出現間隔を計算
                float adjustedInterval = CalculateAdjustedSpawnInterval();
                float nextSpawnTime = adjustedInterval + Random.Range(-spawnIntervalVariation, spawnIntervalVariation);
                
                // 最小間隔を保証
                nextSpawnTime = Mathf.Max(nextSpawnTime, minSpawnInterval);
                
                yield return new WaitForSeconds(nextSpawnTime);

                // 出現条件をチェック
                if (CanSpawnInvader())
                {
                    // パーティ出現判定
                    if (ShouldSpawnParty())
                    {
                        SpawnRandomInvaderParty();
                    }
                    else
                    {
                        SpawnRandomInvader();
                    }
                }
            }
        }

        /// <summary>
        /// ランダムタイミングでの出現チェック
        /// 要件18.1: ランダムなタイミングで侵入者を生成
        /// </summary>
        private IEnumerator RandomSpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(randomSpawnCheckInterval);

                // ランダム出現判定
                if (CanSpawnInvader() && ShouldRandomSpawn())
                {
                    // 連続出現防止をより厳密にチェック
                    if (Time.time >= lastSpawnTime + preventConsecutiveSpawnTime)
                    {
                        SpawnRandomInvader();
                        Debug.Log("Random spawn triggered");
                    }
                }
            }
        }

        private bool CanSpawnInvader()
        {
            // ゲーム状態チェック
            if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.Playing)
            {
                return false;
            }

            // 同時出現数制限（階層深度に応じて調整）
            int adjustedMaxInvaders = CalculateMaxConcurrentInvaders();
            if (currentActiveInvaders >= adjustedMaxInvaders)
            {
                return false;
            }

            // FloorSystemの確認
            if (FloorSystem.Instance == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 階層深度に応じた出現間隔を計算
        /// 要件18.4: 階層が深いほど出現頻度を調整
        /// </summary>
        private float CalculateAdjustedSpawnInterval()
        {
            if (FloorSystem.Instance == null)
            {
                return baseSpawnInterval;
            }

            int currentFloorCount = FloorSystem.Instance.CurrentFloorCount;
            
            // 階層が深いほど出現間隔を短縮（頻度上昇）
            float reduction = currentFloorCount * frequencyScalingFactor;
            float adjustedInterval = baseSpawnInterval - reduction;
            
            return Mathf.Max(adjustedInterval, minSpawnInterval);
        }

        /// <summary>
        /// 階層深度に応じた最大同時出現数を計算
        /// </summary>
        private int CalculateMaxConcurrentInvaders()
        {
            if (FloorSystem.Instance == null)
            {
                return maxConcurrentInvaders;
            }

            int currentFloorCount = FloorSystem.Instance.CurrentFloorCount;
            
            // 階層が深いほど同時出現数を増加
            int bonus = Mathf.FloorToInt(currentFloorCount * 0.5f);
            return maxConcurrentInvaders + bonus;
        }

        /// <summary>
        /// ランダム出現判定
        /// 要件18.1: ランダムなタイミングで侵入者を生成
        /// </summary>
        private bool ShouldRandomSpawn()
        {
            if (FloorSystem.Instance == null)
            {
                return false;
            }

            int currentFloorCount = FloorSystem.Instance.CurrentFloorCount;
            
            // 階層が深いほどランダム出現確率を上昇
            float baseChance = 0.02f; // 2%の基本確率
            float floorBonus = currentFloorCount * 0.005f; // 階層ごとに0.5%上昇
            float totalChance = baseChance + floorBonus;
            
            // 確率曲線を適用
            float normalizedTime = Mathf.Clamp01((Time.time - lastSpawnTime) / baseSpawnInterval);
            float curveMultiplier = spawnProbabilityCurve.Evaluate(normalizedTime);
            
            return Random.value < (totalChance * curveMultiplier);
        }

        /// <summary>
        /// パーティ出現判定
        /// 要件18.3: 階層が深くなると侵入者をパーティ単位で出現
        /// </summary>
        private bool ShouldSpawnParty()
        {
            if (FloorSystem.Instance == null)
            {
                return false;
            }

            int currentFloorCount = FloorSystem.Instance.CurrentFloorCount;
            
            // 指定階層以降でパーティ出現開始
            if (currentFloorCount < partySpawnStartFloor)
            {
                return false;
            }

            // 階層が深いほどパーティ出現確率上昇
            float floorBonus = (currentFloorCount - partySpawnStartFloor) * 0.1f;
            float totalChance = partySpawnChance + floorBonus;
            
            return Random.value < totalChance;
        }

        public void SpawnRandomInvader()
        {
            if (DataManager.Instance == null || FloorSystem.Instance == null)
            {
                Debug.LogWarning("Required managers not available for spawning");
                return;
            }

            // 現在の階層に基づいて侵入者を選択
            int currentFloor = FloorSystem.Instance.CurrentFloorCount;
            var availableInvaders = DataManager.Instance.GetAvailableInvaders(currentFloor);

            if (availableInvaders.Count == 0)
            {
                Debug.LogWarning("No available invaders for current floor");
                return;
            }

            // ランダムに侵入者を選択
            InvaderData selectedData = availableInvaders[Random.Range(0, availableInvaders.Count)];
            
            // レベルを計算
            int invaderLevel = CalculateInvaderLevel(currentFloor);
            
            // 侵入者を生成
            SpawnInvader(selectedData, invaderLevel);
        }

        public GameObject SpawnInvader(InvaderData invaderData, int level = 1)
        {
            if (invaderData == null || invaderData.prefab == null)
            {
                Debug.LogError("Invalid invader data or missing prefab");
                return null;
            }

            // 1階層の上り階段位置を取得
            Vector2 spawnPosition = GetSpawnPosition();
            
            // 侵入者オブジェクトを生成
            GameObject invaderObj = Instantiate(invaderData.prefab, invaderContainer);
            invaderObj.transform.position = new Vector3(spawnPosition.x, spawnPosition.y, 0);
            invaderObj.name = $"{invaderData.displayName}_Lv{level}";

            // 侵入者コンポーネントを設定
            BaseInvader invader = GetInvaderComponent(invaderObj, invaderData.type);
            if (invader != null)
            {
                invader.SetInvaderData(invaderData);
                invader.SetLevel(level);
            }

            // 階層に追加
            var firstFloor = FloorSystem.Instance.GetFloor(1);
            firstFloor?.AddInvader(invaderObj);

            // 出現エフェクト
            CreateSpawnEffect(spawnPosition);

            // カウンター更新
            currentActiveInvaders++;
            lastSpawnTime = Time.time;

            // イベント発火
            OnInvaderSpawned?.Invoke(invaderObj);

            Debug.Log($"Spawned {invaderData.displayName} (Level {level}) at {spawnPosition}");
            return invaderObj;
        }

        private BaseInvader GetInvaderComponent(GameObject invaderObj, InvaderType type)
        {
            // 既存のコンポーネントをチェック
            BaseInvader existing = invaderObj.GetComponent<BaseInvader>();
            if (existing != null)
            {
                return existing;
            }

            // タイプに応じて適切なコンポーネントを追加
            switch (type)
            {
                case InvaderType.Warrior:
                    return invaderObj.AddComponent<Warrior>();
                case InvaderType.Mage:
                    return invaderObj.AddComponent<Mage>();
                case InvaderType.Rogue:
                    return invaderObj.AddComponent<Rogue>();
                case InvaderType.Cleric:
                    return invaderObj.AddComponent<Cleric>();
                default:
                    return invaderObj.AddComponent<BaseInvader>();
            }
        }

        private Vector2 GetSpawnPosition()
        {
            // 1階層の上り階段位置から出現
            if (FloorSystem.Instance != null)
            {
                Vector2 stairPos = FloorSystem.Instance.GetUpStairPosition(1);
                
                // 階段位置から少しずらして出現
                Vector2 offset = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
                return stairPos + offset;
            }

            // フォールバック位置
            return new Vector2(-4f, 4f);
        }

        private int CalculateInvaderLevel(int floorDepth)
        {
            // 階層の深さに応じてレベルを上昇
            int level = baseInvaderLevel + Mathf.FloorToInt(floorDepth * levelScalingFactor);
            return Mathf.Max(1, level);
        }

        /// <summary>
        /// 出現エフェクトを作成
        /// 要件18.5: 侵入者が1階層の上り階段から出現時に視覚的な出現エフェクトを表示
        /// </summary>
        private void CreateSpawnEffect(Vector2 position)
        {
            if (spawnEffectPrefab != null)
            {
                GameObject effect = Instantiate(spawnEffectPrefab);
                effect.transform.position = new Vector3(position.x, position.y, 0);
                effect.name = "InvaderSpawnEffect";
                
                // エフェクトを一定時間後に削除
                Destroy(effect, 2f);
                
                Debug.Log($"Created spawn effect at {position}");
            }
            else
            {
                // エフェクトプレハブがない場合のフォールバック
                Debug.Log($"Invader spawned at {position} (no effect prefab)");
            }
        }

        public void OnInvaderDestroyed(GameObject invader)
        {
            currentActiveInvaders = Mathf.Max(0, currentActiveInvaders - 1);
            OnInvaderDefeated?.Invoke(invader);
            
            // 連続出現防止のタイマーをリセット（撃破時は即座に次の出現を許可しない）
            lastSpawnTime = Time.time;
            
            Debug.Log($"Invader destroyed. Active count: {currentActiveInvaders}");
        }

        /// <summary>
        /// ランダムパーティ出現
        /// 要件18.3: 階層が深くなると侵入者をパーティ単位で出現
        /// </summary>
        public void SpawnRandomInvaderParty()
        {
            if (DataManager.Instance == null || FloorSystem.Instance == null)
            {
                Debug.LogWarning("Required managers not available for party spawning");
                return;
            }

            int currentFloor = FloorSystem.Instance.CurrentFloorCount;
            var availableInvaders = DataManager.Instance.GetAvailableInvaders(currentFloor);

            if (availableInvaders.Count == 0)
            {
                Debug.LogWarning("No available invaders for party spawning");
                return;
            }

            // パーティサイズを決定（2-4人）
            int partySize = Random.Range(2, 5);
            List<InvaderData> partyMembers = new List<InvaderData>();

            // パーティメンバーを選択
            for (int i = 0; i < partySize; i++)
            {
                InvaderData member = availableInvaders[Random.Range(0, availableInvaders.Count)];
                partyMembers.Add(member);
            }

            // パーティレベルを計算
            int partyLevel = CalculateInvaderLevel(currentFloor);

            SpawnInvaderParty(partyMembers, partyLevel);
        }

        /// <summary>
        /// 指定パーティ出現システム
        /// </summary>
        public void SpawnInvaderParty(List<InvaderData> partyMembers, int baseLevel = 1)
        {
            if (partyMembers == null || partyMembers.Count == 0)
            {
                return;
            }

            Vector2 basePosition = GetSpawnPosition();
            List<GameObject> partyObjects = new List<GameObject>();

            // パーティメンバーを順次生成
            for (int i = 0; i < partyMembers.Count; i++)
            {
                // パーティ内での配置位置を計算
                Vector2 memberOffset = CalculatePartyMemberOffset(i, partyMembers.Count);
                Vector2 memberPosition = basePosition + memberOffset;
                
                GameObject member = SpawnInvader(partyMembers[i], baseLevel);
                
                if (member != null)
                {
                    member.transform.position = new Vector3(memberPosition.x, memberPosition.y, 0);
                    partyObjects.Add(member);
                    
                    // パーティメンバーであることを示すタグを追加
                    member.tag = "InvaderParty";
                }
            }

            // パーティ編成処理（IPartyが実装されたら）
            // TODO: パーティシステムの実装
            
            Debug.Log($"Spawned invader party with {partyObjects.Count} members at level {baseLevel}");
        }

        /// <summary>
        /// パーティメンバーの配置オフセットを計算
        /// </summary>
        private Vector2 CalculatePartyMemberOffset(int memberIndex, int totalMembers)
        {
            // 円形配置でパーティメンバーを配置
            float angle = (360f / totalMembers) * memberIndex * Mathf.Deg2Rad;
            float radius = 1.5f;
            
            return new Vector2(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius
            );
        }

        // 設定メソッド
        public void SetSpawnInterval(float interval)
        {
            baseSpawnInterval = Mathf.Max(1f, interval);
        }

        public void SetMaxConcurrentInvaders(int max)
        {
            maxConcurrentInvaders = Mathf.Max(1, max);
        }

        public void SetFrequencyScaling(float factor)
        {
            frequencyScalingFactor = Mathf.Max(0f, factor);
        }

        public void SetPartySpawnSettings(int startFloor, float chance)
        {
            partySpawnStartFloor = Mathf.Max(1, startFloor);
            partySpawnChance = Mathf.Clamp01(chance);
        }

        /// <summary>
        /// 連続出現防止時間を動的に調整
        /// 要件18.2: 連続出現を防止
        /// </summary>
        public void SetPreventConsecutiveSpawnTime(float time)
        {
            preventConsecutiveSpawnTime = Mathf.Max(0.5f, time);
        }

        // デバッグ用メソッド
        public void ForceSpawnInvader(InvaderType type, int level = 1)
        {
            if (DataManager.Instance != null)
            {
                InvaderData data = DataManager.Instance.GetInvaderData(type);
                if (data != null)
                {
                    SpawnInvader(data, level);
                }
            }
        }

        public void DebugPrintSpawnerInfo()
        {
            Debug.Log($"=== Invader Spawner Info ===");
            Debug.Log($"Active Invaders: {currentActiveInvaders}/{CalculateMaxConcurrentInvaders()}");
            Debug.Log($"Base Spawn Interval: {baseSpawnInterval}s");
            Debug.Log($"Adjusted Spawn Interval: {CalculateAdjustedSpawnInterval()}s");
            Debug.Log($"Last Spawn: {Time.time - lastSpawnTime}s ago");
            Debug.Log($"Spawning Active: {spawnCoroutine != null}");
            Debug.Log($"Random Spawning Active: {randomSpawnCoroutine != null}");
            Debug.Log($"Party Spawn Chance: {partySpawnChance * 100}%");
            Debug.Log($"Current Floor Count: {(FloorSystem.Instance != null ? FloorSystem.Instance.CurrentFloorCount : 0)}");
        }
    }
}