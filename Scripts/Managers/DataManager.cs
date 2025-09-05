using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Scripts.Data.ScriptableObjects;
using Scripts.Data.Enums;

namespace DungeonOwner.Data
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        [Header("ゲーム設定")]
        [SerializeField] private GameConfig gameConfig;

        [Header("データベース")]
        [SerializeField] private List<MonsterData> monsterDatabase = new List<MonsterData>();
        [SerializeField] private List<InvaderData> invaderDatabase = new List<InvaderData>();
        [SerializeField] private List<PlayerCharacterData> playerCharacterDatabase = new List<PlayerCharacterData>();
        [SerializeField] private List<TrapItemData> trapItemDatabase = new List<TrapItemData>();

        public GameConfig Config => gameConfig;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeData()
        {
            // 開発中はリソースから動的にロード
            LoadDataFromResources();
            ValidateData();
        }

        private void LoadDataFromResources()
        {
            // MonsterDataをロード
            MonsterData[] monsters = Resources.LoadAll<MonsterData>("Data/Monsters");
            monsterDatabase.AddRange(monsters);

            // InvaderDataをロード
            InvaderData[] invaders = Resources.LoadAll<InvaderData>("Data/Invaders");
            invaderDatabase.AddRange(invaders);

            // PlayerCharacterDataをロード
            PlayerCharacterData[] characters = Resources.LoadAll<PlayerCharacterData>("Data/PlayerCharacters");
            playerCharacterDatabase.AddRange(characters);

            // TrapItemDataをロード
            TrapItemData[] trapItems = Resources.LoadAll<TrapItemData>("Data/TrapItems");
            trapItemDatabase.AddRange(trapItems);

            Debug.Log($"Loaded {monsterDatabase.Count} monsters, {invaderDatabase.Count} invaders, {playerCharacterDatabase.Count} player characters, {trapItemDatabase.Count} trap items");
        }

        private void ValidateData()
        {
            if (gameConfig == null)
            {
                Debug.LogError("GameConfig is not assigned!");
            }

            if (monsterDatabase.Count == 0)
            {
                Debug.LogWarning("No monster data loaded!");
            }

            if (invaderDatabase.Count == 0)
            {
                Debug.LogWarning("No invader data loaded!");
            }

            if (playerCharacterDatabase.Count == 0)
            {
                Debug.LogWarning("No player character data loaded!");
            }

            if (trapItemDatabase.Count == 0)
            {
                Debug.LogWarning("No trap item data loaded!");
            }
        }

        // モンスターデータ取得
        public MonsterData GetMonsterData(MonsterType type)
        {
            return monsterDatabase.FirstOrDefault(m => m.type == type);
        }

        public List<MonsterData> GetAvailableMonsters(int currentFloor)
        {
            return monsterDatabase.Where(m => m.unlockFloor <= currentFloor).ToList();
        }

        public List<MonsterData> GetMonstersByRarity(MonsterRarity rarity)
        {
            return monsterDatabase.Where(m => m.rarity == rarity).ToList();
        }

        // 侵入者データ取得
        public InvaderData GetInvaderData(InvaderType type)
        {
            return invaderDatabase.FirstOrDefault(i => i.type == type);
        }

        public List<InvaderData> GetAvailableInvaders(int currentFloor)
        {
            return invaderDatabase.Where(i => i.minAppearanceFloor <= currentFloor).ToList();
        }

        public List<InvaderData> GetInvadersByRank(InvaderRank rank)
        {
            return invaderDatabase.Where(i => i.rank == rank).ToList();
        }

        // プレイヤーキャラクターデータ取得
        public PlayerCharacterData GetPlayerCharacterData(PlayerCharacterType type)
        {
            return playerCharacterDatabase.FirstOrDefault(p => p.type == type);
        }

        public List<PlayerCharacterData> GetAllPlayerCharacters()
        {
            return new List<PlayerCharacterData>(playerCharacterDatabase);
        }

        // ランダム選択メソッド
        public InvaderData GetRandomInvader(int floorLevel)
        {
            var availableInvaders = GetAvailableInvaders(floorLevel);
            if (availableInvaders.Count == 0) return null;

            return availableInvaders[Random.Range(0, availableInvaders.Count)];
        }

        public MonsterData GetRandomMonster(int floorLevel)
        {
            var availableMonsters = GetAvailableMonsters(floorLevel);
            if (availableMonsters.Count == 0) return null;

            return availableMonsters[Random.Range(0, availableMonsters.Count)];
        }

        // 罠アイテムデータ取得
        public TrapItemData GetTrapItemData(TrapItemType type)
        {
            return trapItemDatabase.FirstOrDefault(t => t.type == type);
        }

        public List<TrapItemData> GetAllTrapItems()
        {
            return new List<TrapItemData>(trapItemDatabase);
        }

        public TrapItemData GetRandomTrapItem()
        {
            if (trapItemDatabase.Count == 0) return null;
            return trapItemDatabase[Random.Range(0, trapItemDatabase.Count)];
        }
    }
}