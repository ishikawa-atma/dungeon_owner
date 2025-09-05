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

        [Header("Level Scaling")]
        [SerializeField] private float levelScalingFactor = 0.2f; // 階層ごとのレベル上昇率
        [SerializeField] private int baseInvaderLevel = 1;

        [Header("Spawn Effects")]
        [SerializeField] private GameObject spawnEffectPrefab;
        [SerializeField] private Transform invaderContainer;

        private float lastSpawnTime = -15f;
        private int currentActiveInvaders = 0;
        private Coroutine spawnCoroutine;

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
        }

        public void StopSpawning()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
                Debug.Log("Invader spawning stopped");
            }
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                // 次の出現時間を計算
                float nextSpawnTime = baseSpawnInterval + Random.Range(-spawnIntervalVariation, spawnIntervalVariation);
                yield return new WaitForSeconds(nextSpawnTime);

                // 出現条件をチェック
                if (CanSpawnInvader())
                {
                    SpawnRandomInvader();
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

            // 同時出現数制限
            if (currentActiveInvaders >= maxConcurrentInvaders)
            {
                return false;
            }

            // 連続出現防止
            if (Time.time < lastSpawnTime + preventConsecutiveSpawnTime)
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

        private void CreateSpawnEffect(Vector2 position)
        {
            if (spawnEffectPrefab != null)
            {
                GameObject effect = Instantiate(spawnEffectPrefab);
                effect.transform.position = new Vector3(position.x, position.y, 0);
                
                // エフェクトを一定時間後に削除
                Destroy(effect, 2f);
            }
        }

        public void OnInvaderDestroyed(GameObject invader)
        {
            currentActiveInvaders = Mathf.Max(0, currentActiveInvaders - 1);
            OnInvaderDefeated?.Invoke(invader);
            
            Debug.Log($"Invader destroyed. Active count: {currentActiveInvaders}");
        }

        // パーティ出現システム（将来の拡張用）
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
                Vector2 memberPosition = basePosition + new Vector2(i * 1f, 0);
                GameObject member = SpawnInvader(partyMembers[i], baseLevel);
                
                if (member != null)
                {
                    member.transform.position = new Vector3(memberPosition.x, memberPosition.y, 0);
                    partyObjects.Add(member);
                }
            }

            // パーティ編成処理（IPartyが実装されたら）
            // TODO: パーティシステムの実装
            
            Debug.Log($"Spawned invader party with {partyObjects.Count} members");
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
            Debug.Log($"Active Invaders: {currentActiveInvaders}/{maxConcurrentInvaders}");
            Debug.Log($"Spawn Interval: {baseSpawnInterval}s");
            Debug.Log($"Last Spawn: {Time.time - lastSpawnTime}s ago");
            Debug.Log($"Spawning Active: {spawnCoroutine != null}");
        }
    }
}