using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DungeonOwner.Interfaces;
using DungeonOwner.UI;

namespace DungeonOwner.Managers
{
    /// <summary>
    /// レベル表示システムの管理クラス
    /// 全てのキャラクターのレベル表示を統括管理する
    /// </summary>
    public class LevelDisplayManager : MonoBehaviour
    {
        public static LevelDisplayManager Instance { get; private set; }

        [Header("Prefab Settings")]
        [SerializeField] private GameObject levelDisplayPrefab;

        [Header("Display Settings")]
        [SerializeField] private bool showMonsterLevels = true;
        [SerializeField] private bool showInvaderLevels = true;
        [SerializeField] private bool showPlayerCharacterLevels = true;
        [SerializeField] private float updateInterval = 0.1f; // 更新間隔

        [Header("Performance Settings")]
        [SerializeField] private int maxDisplays = 50; // 最大表示数
        [SerializeField] private float cullingDistance = 15f; // カリング距離

        private Dictionary<GameObject, LevelDisplayUI> activeLevelDisplays = new Dictionary<GameObject, LevelDisplayUI>();
        private Queue<LevelDisplayUI> pooledDisplays = new Queue<LevelDisplayUI>();
        private Transform displayContainer;
        private Camera mainCamera;
        private float lastUpdateTime;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        private void Update()
        {
            // 定期的に表示を更新
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateLevelDisplays();
                lastUpdateTime = Time.time;
            }
        }

        /// <summary>
        /// マネージャーの初期化
        /// </summary>
        private void InitializeManager()
        {
            // 表示コンテナ作成
            GameObject container = new GameObject("LevelDisplayContainer");
            container.transform.SetParent(transform);
            displayContainer = container.transform;

            // プレハブが設定されていない場合のフォールバック
            if (levelDisplayPrefab == null)
            {
                CreateDefaultLevelDisplayPrefab();
            }

            // 初期プール作成
            CreateInitialPool();

            Debug.Log("LevelDisplayManager initialized");
        }

        /// <summary>
        /// デフォルトのレベル表示プレハブ作成
        /// </summary>
        private void CreateDefaultLevelDisplayPrefab()
        {
            GameObject prefab = new GameObject("LevelDisplayPrefab");
            prefab.AddComponent<LevelDisplayUI>();
            levelDisplayPrefab = prefab;
            
            // プレハブとして保存（エディタでのみ）
            #if UNITY_EDITOR
            prefab.SetActive(false);
            #endif
        }

        /// <summary>
        /// 初期プール作成
        /// </summary>
        private void CreateInitialPool()
        {
            for (int i = 0; i < 10; i++)
            {
                CreatePooledDisplay();
            }
        }

        /// <summary>
        /// プールされた表示オブジェクト作成
        /// </summary>
        private LevelDisplayUI CreatePooledDisplay()
        {
            GameObject displayObj = Instantiate(levelDisplayPrefab, displayContainer);
            displayObj.SetActive(false);
            
            LevelDisplayUI display = displayObj.GetComponent<LevelDisplayUI>();
            if (display == null)
            {
                display = displayObj.AddComponent<LevelDisplayUI>();
            }
            
            pooledDisplays.Enqueue(display);
            return display;
        }

        /// <summary>
        /// キャラクターにレベル表示を追加
        /// </summary>
        public void AddLevelDisplay(GameObject character, ICharacterBase characterInterface)
        {
            if (character == null || characterInterface == null)
            {
                return;
            }

            // 既に表示が存在する場合はスキップ
            if (activeLevelDisplays.ContainsKey(character))
            {
                return;
            }

            // 表示タイプチェック
            if (!ShouldShowLevelFor(characterInterface))
            {
                return;
            }

            // 最大表示数チェック
            if (activeLevelDisplays.Count >= maxDisplays)
            {
                return;
            }

            // プールから取得または新規作成
            LevelDisplayUI display = GetPooledDisplay();
            if (display == null)
            {
                display = CreatePooledDisplay();
            }

            // 表示設定
            display.gameObject.SetActive(true);
            display.SetTarget(character.transform, characterInterface);
            
            // 辞書に追加
            activeLevelDisplays[character] = display;

            Debug.Log($"Added level display for {character.name}");
        }

        /// <summary>
        /// キャラクターのレベル表示を削除
        /// </summary>
        public void RemoveLevelDisplay(GameObject character)
        {
            if (character == null || !activeLevelDisplays.ContainsKey(character))
            {
                return;
            }

            LevelDisplayUI display = activeLevelDisplays[character];
            activeLevelDisplays.Remove(character);

            // プールに戻す
            ReturnToPool(display);

            Debug.Log($"Removed level display for {character.name}");
        }

        /// <summary>
        /// プールから表示オブジェクト取得
        /// </summary>
        private LevelDisplayUI GetPooledDisplay()
        {
            if (pooledDisplays.Count > 0)
            {
                return pooledDisplays.Dequeue();
            }
            return null;
        }

        /// <summary>
        /// 表示オブジェクトをプールに戻す
        /// </summary>
        private void ReturnToPool(LevelDisplayUI display)
        {
            if (display != null)
            {
                display.gameObject.SetActive(false);
                display.SetTarget(null, null);
                pooledDisplays.Enqueue(display);
            }
        }

        /// <summary>
        /// レベル表示の更新
        /// </summary>
        private void UpdateLevelDisplays()
        {
            var toRemove = new List<GameObject>();

            foreach (var kvp in activeLevelDisplays)
            {
                GameObject character = kvp.Key;
                LevelDisplayUI display = kvp.Value;

                // キャラクターが削除された場合
                if (character == null)
                {
                    toRemove.Add(character);
                    ReturnToPool(display);
                    continue;
                }

                // カリング距離チェック
                if (mainCamera != null)
                {
                    float distance = Vector3.Distance(mainCamera.transform.position, character.transform.position);
                    if (distance > cullingDistance)
                    {
                        display.SetVisible(false);
                    }
                    else
                    {
                        display.SetVisible(true);
                    }
                }
            }

            // 削除対象を処理
            foreach (var character in toRemove)
            {
                activeLevelDisplays.Remove(character);
            }
        }

        /// <summary>
        /// レベル表示が必要かチェック
        /// </summary>
        private bool ShouldShowLevelFor(ICharacterBase character)
        {
            if (character is IMonster)
            {
                return showMonsterLevels;
            }
            else if (character is IInvader)
            {
                return showInvaderLevels;
            }
            else
            {
                return showPlayerCharacterLevels;
            }
        }

        /// <summary>
        /// 全ての表示を更新
        /// </summary>
        public void RefreshAllDisplays()
        {
            // 現在のキャラクターを再スキャン
            var monsters = FindObjectsOfType<MonoBehaviour>().Where(mb => mb is IMonster).Cast<IMonster>();
            var invaders = FindObjectsOfType<MonoBehaviour>().Where(mb => mb is IInvader).Cast<IInvader>();

            // モンスターの表示追加
            foreach (var monster in monsters)
            {
                var monsterObj = (monster as MonoBehaviour)?.gameObject;
                if (monsterObj != null)
                {
                    AddLevelDisplay(monsterObj, monster as ICharacterBase);
                }
            }

            // 侵入者の表示追加
            foreach (var invader in invaders)
            {
                var invaderObj = (invader as MonoBehaviour)?.gameObject;
                if (invaderObj != null)
                {
                    AddLevelDisplay(invaderObj, invader as ICharacterBase);
                }
            }
        }

        /// <summary>
        /// 表示設定の変更
        /// </summary>
        public void SetShowMonsterLevels(bool show)
        {
            showMonsterLevels = show;
            RefreshDisplayVisibility();
        }

        public void SetShowInvaderLevels(bool show)
        {
            showInvaderLevels = show;
            RefreshDisplayVisibility();
        }

        public void SetShowPlayerCharacterLevels(bool show)
        {
            showPlayerCharacterLevels = show;
            RefreshDisplayVisibility();
        }

        /// <summary>
        /// 表示可視性の更新
        /// </summary>
        private void RefreshDisplayVisibility()
        {
            foreach (var kvp in activeLevelDisplays)
            {
                GameObject character = kvp.Key;
                LevelDisplayUI display = kvp.Value;

                if (character != null)
                {
                    var characterInterface = character.GetComponent<ICharacterBase>();
                    bool shouldShow = ShouldShowLevelFor(characterInterface);
                    display.SetVisible(shouldShow);
                }
            }
        }

        /// <summary>
        /// 全ての表示をクリア
        /// </summary>
        public void ClearAllDisplays()
        {
            var characters = new List<GameObject>(activeLevelDisplays.Keys);
            foreach (var character in characters)
            {
                RemoveLevelDisplay(character);
            }
        }

        /// <summary>
        /// デバッグ情報表示
        /// </summary>
        public void DebugPrintInfo()
        {
            Debug.Log($"=== Level Display Manager Info ===");
            Debug.Log($"Active Displays: {activeLevelDisplays.Count}/{maxDisplays}");
            Debug.Log($"Pooled Displays: {pooledDisplays.Count}");
            Debug.Log($"Show Settings - Monsters: {showMonsterLevels}, Invaders: {showInvaderLevels}, Players: {showPlayerCharacterLevels}");
        }

        private void OnDestroy()
        {
            ClearAllDisplays();
        }
    }
}