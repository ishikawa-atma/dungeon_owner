using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DungeonOwner.Data.Enums;

namespace DungeonOwner
{
    /// <summary>
    /// ゲームデータの保存・読み込みを管理するクラス
    /// 要件16.1, 16.2, 16.3, 16.5に対応
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        [Header("Save Settings")]
        [SerializeField] private bool autoSaveEnabled = true;
        [SerializeField] private float autoSaveInterval = 30f; // 30秒間隔での自動保存
        
        private string saveFilePath;
        private float lastAutoSaveTime;
        
        public static SaveManager Instance { get; private set; }
        
        // イベント
        public event System.Action OnSaveCompleted;
        public event System.Action OnLoadCompleted;
        public event System.Action<string> OnSaveError;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeSaveSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            // 自動保存の処理
            if (autoSaveEnabled && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                AutoSave();
                lastAutoSaveTime = Time.time;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // アプリが一時停止される時に保存（要件16.2）
            if (pauseStatus)
            {
                SaveGame();
            }
        }
        
        private void OnApplicationFocus(bool hasFocus)
        {
            // アプリがフォーカスを失った時に保存（要件16.2）
            if (!hasFocus)
            {
                SaveGame();
            }
        }
        
        private void InitializeSaveSystem()
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "savegame.json");
            Debug.Log($"Save file path: {saveFilePath}");
        }
        
        /// <summary>
        /// ゲームデータを保存する（要件16.1, 16.2）
        /// </summary>
        public void SaveGame()
        {
            try
            {
                SaveData saveData = CreateSaveData();
                string jsonData = JsonUtility.ToJson(saveData, true);
                File.WriteAllText(saveFilePath, jsonData);
                
                Debug.Log("Game saved successfully");
                OnSaveCompleted?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save game: {e.Message}");
                OnSaveError?.Invoke(e.Message);
            }
        }
        
        /// <summary>
        /// ゲームデータを読み込む（要件16.3）
        /// </summary>
        public bool LoadGame()
        {
            try
            {
                if (!File.Exists(saveFilePath))
                {
                    Debug.Log("No save file found, starting new game");
                    return false;
                }
                
                string jsonData = File.ReadAllText(saveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
                
                if (saveData == null)
                {
                    Debug.LogError("Save data is null");
                    return false;
                }
                
                ApplySaveData(saveData);
                Debug.Log("Game loaded successfully");
                OnLoadCompleted?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load game: {e.Message}");
                OnSaveError?.Invoke(e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// セーブファイルが存在するかチェック
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(saveFilePath);
        }
        
        /// <summary>
        /// セーブファイルを削除
        /// </summary>
        public void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                    Debug.Log("Save file deleted");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to delete save file: {e.Message}");
            }
        }
        
        /// <summary>
        /// 自動保存を実行
        /// </summary>
        private void AutoSave()
        {
            SaveGame();
        }
        
        /// <summary>
        /// 侵入者撃破時の保存（要件16.1）
        /// </summary>
        public void SaveOnInvaderDefeated()
        {
            SaveGame();
        }        
 
       /// <summary>
        /// 現在のゲーム状態からセーブデータを作成
        /// </summary>
        private SaveData CreateSaveData()
        {
            SaveData saveData = new SaveData();
            
            // GameManagerから基本情報を取得
            if (GameManager.Instance != null)
            {
                saveData.currentFloor = GameManager.Instance.CurrentFloor;
            }
            
            // ResourceManagerから経済情報を取得
            if (DungeonOwner.Managers.ResourceManager.Instance != null)
            {
                saveData.gold = DungeonOwner.Managers.ResourceManager.Instance.Gold;
                saveData.lastDailyReward = DungeonOwner.Managers.ResourceManager.Instance.LastDailyReward;
            }
            
            // 自キャラクター情報を取得
            if (DungeonOwner.Data.DataManager.Instance != null)
            {
                saveData.selectedPlayerCharacter = DungeonOwner.Data.DataManager.Instance.SelectedPlayerCharacterType;
            }
            
            // 階層レイアウト情報を取得
            if (DungeonOwner.Core.FloorSystem.Instance != null)
            {
                saveData.floorLayouts = CreateFloorDataList();
            }
            
            // 配置されたモンスター情報を取得
            if (DungeonOwner.Managers.MonsterPlacementManager.Instance != null)
            {
                saveData.placedMonsters = CreatePlacedMonstersList();
            }
            
            // 退避スポットのモンスター情報を取得
            if (DungeonOwner.Managers.ShelterManager.Instance != null)
            {
                saveData.shelterMonsters = CreateShelterMonstersList();
            }
            
            // インベントリ情報を取得
            if (Scripts.Managers.InventoryManager.Instance != null)
            {
                saveData.inventory = CreateInventoryList();
            }
            
            saveData.lastSaveTime = DateTime.Now;
            
            return saveData;
        }
        
        /// <summary>
        /// セーブデータをゲーム状態に適用
        /// </summary>
        private void ApplySaveData(SaveData saveData)
        {
            // セーブデータの整合性チェック（要件16.5）
            if (!ValidateSaveData(saveData))
            {
                Debug.LogError("Save data validation failed, starting new game");
                return;
            }
            
            // GameManagerに基本情報を適用
            if (DungeonOwner.Core.GameManager.Instance != null)
            {
                DungeonOwner.Core.GameManager.Instance.SetCurrentFloor(saveData.currentFloor);
            }
            
            // ResourceManagerに経済情報を適用
            if (DungeonOwner.Managers.ResourceManager.Instance != null)
            {
                DungeonOwner.Managers.ResourceManager.Instance.SetGold(saveData.gold);
                DungeonOwner.Managers.ResourceManager.Instance.SetLastDailyReward(saveData.lastDailyReward);
            }
            
            // 自キャラクター情報を適用
            if (DungeonOwner.Data.DataManager.Instance != null)
            {
                DungeonOwner.Data.DataManager.Instance.SetSelectedPlayerCharacter(saveData.selectedPlayerCharacter);
            }
            
            // 階層レイアウトを復元
            if (DungeonOwner.Core.FloorSystem.Instance != null)
            {
                RestoreFloorLayouts(saveData.floorLayouts);
            }
            
            // 配置されたモンスターを復元
            if (DungeonOwner.Managers.MonsterPlacementManager.Instance != null)
            {
                RestorePlacedMonsters(saveData.placedMonsters);
            }
            
            // 退避スポットのモンスターを復元
            if (DungeonOwner.Managers.ShelterManager.Instance != null)
            {
                RestoreShelterMonsters(saveData.shelterMonsters);
            }
            
            // インベントリを復元
            if (Scripts.Managers.InventoryManager.Instance != null)
            {
                RestoreInventory(saveData.inventory);
            }
        }
        
        /// <summary>
        /// セーブデータの整合性をチェック（要件16.5）
        /// </summary>
        private bool ValidateSaveData(SaveData saveData)
        {
            if (saveData == null)
            {
                Debug.LogError("Save data is null");
                return false;
            }
            
            // 基本的な値の範囲チェック
            if (saveData.currentFloor < 1 || saveData.currentFloor > 1000)
            {
                Debug.LogError($"Invalid current floor: {saveData.currentFloor}");
                return false;
            }
            
            if (saveData.gold < 0)
            {
                Debug.LogError($"Invalid gold amount: {saveData.gold}");
                return false;
            }
            
            // セーブ時刻の妥当性チェック
            if (saveData.lastSaveTime > DateTime.Now.AddDays(1))
            {
                Debug.LogError("Save time is in the future");
                return false;
            }
            
            return true;
        } 
       
        /// <summary>
        /// 階層データリストを作成
        /// </summary>
        private List<FloorData> CreateFloorDataList()
        {
            List<FloorData> floorDataList = new List<FloorData>();
            
            if (DungeonOwner.Core.FloorSystem.Instance != null)
            {
                for (int i = 0; i < DungeonOwner.Core.FloorSystem.Instance.Floors.Count; i++)
                {
                    var floor = DungeonOwner.Core.FloorSystem.Instance.Floors[i];
                    FloorData floorData = new FloorData
                    {
                        floorIndex = i,
                        upStairPosition = floor.upStairPosition,
                        downStairPosition = floor.downStairPosition,
                        wallPositions = new List<Vector2>(floor.wallPositions)
                    };
                    
                    // ボス情報があれば追加
                    if (floor.bossType != DungeonOwner.Data.Enums.BossType.None)
                    {
                        floorData.bossType = floor.bossType;
                        floorData.bossLevel = floor.bossLevel;
                        floorData.hasBoss = true;
                    }
                    
                    floorDataList.Add(floorData);
                }
            }
            
            return floorDataList;
        }
        
        /// <summary>
        /// 配置されたモンスターリストを作成
        /// </summary>
        private List<MonsterSaveData> CreatePlacedMonstersList()
        {
            List<MonsterSaveData> monsterList = new List<MonsterSaveData>();
            
            if (DungeonOwner.Managers.MonsterPlacementManager.Instance != null)
            {
                foreach (var monster in DungeonOwner.Managers.MonsterPlacementManager.Instance.GetAllPlacedMonsters())
                {
                    int floorIndex = DungeonOwner.Managers.MonsterPlacementManager.Instance.GetMonsterFloorIndex(monster);
                    
                    MonsterSaveData monsterData = new MonsterSaveData
                    {
                        type = monster.Type,
                        level = monster.Level,
                        currentHealth = monster.Health,
                        currentMana = monster.Mana,
                        position = monster.Position,
                        floorIndex = floorIndex,
                        isInShelter = false
                    };
                    
                    monsterList.Add(monsterData);
                }
            }
            
            return monsterList;
        }
        
        /// <summary>
        /// 退避スポットのモンスターリストを作成
        /// </summary>
        private List<MonsterSaveData> CreateShelterMonstersList()
        {
            List<MonsterSaveData> shelterList = new List<MonsterSaveData>();
            
            if (DungeonOwner.Managers.ShelterManager.Instance != null)
            {
                foreach (var monster in DungeonOwner.Managers.ShelterManager.Instance.ShelterMonsters)
                {
                    MonsterSaveData monsterData = new MonsterSaveData
                    {
                        type = monster.Type,
                        level = monster.Level,
                        currentHealth = monster.Health,
                        currentMana = monster.Mana,
                        position = Vector2.zero, // 退避スポットでは位置は不要
                        floorIndex = -1, // 退避スポットを示す
                        isInShelter = true
                    };
                    
                    shelterList.Add(monsterData);
                }
            }
            
            return shelterList;
        }
        
        /// <summary>
        /// インベントリリストを作成
        /// </summary>
        private List<TrapItemSaveData> CreateInventoryList()
        {
            List<TrapItemSaveData> inventoryList = new List<TrapItemSaveData>();
            
            if (Scripts.Managers.InventoryManager.Instance != null)
            {
                foreach (var item in Scripts.Managers.InventoryManager.Instance.GetAllItems())
                {
                    TrapItemSaveData itemData = new TrapItemSaveData
                    {
                        type = item.itemData.type,
                        quantity = item.count
                    };
                    
                    inventoryList.Add(itemData);
                }
            }
            
            return inventoryList;
        }  
      
        /// <summary>
        /// 階層レイアウトを復元
        /// </summary>
        private void RestoreFloorLayouts(List<FloorData> floorDataList)
        {
            if (floorDataList == null || DungeonOwner.Core.FloorSystem.Instance == null) return;
            
            foreach (var floorData in floorDataList)
            {
                DungeonOwner.Core.FloorSystem.Instance.RestoreFloor(floorData);
            }
        }
        
        /// <summary>
        /// 配置されたモンスターを復元
        /// </summary>
        private void RestorePlacedMonsters(List<MonsterSaveData> monsterDataList)
        {
            if (monsterDataList == null || DungeonOwner.Managers.MonsterPlacementManager.Instance == null) return;
            
            foreach (var monsterData in monsterDataList)
            {
                if (!monsterData.isInShelter)
                {
                    DungeonOwner.Managers.MonsterPlacementManager.Instance.RestoreMonster(monsterData);
                }
            }
        }
        
        /// <summary>
        /// 退避スポットのモンスターを復元
        /// </summary>
        private void RestoreShelterMonsters(List<MonsterSaveData> shelterDataList)
        {
            if (shelterDataList == null || DungeonOwner.Managers.ShelterManager.Instance == null) return;
            
            foreach (var monsterData in shelterDataList)
            {
                if (monsterData.isInShelter)
                {
                    DungeonOwner.Managers.ShelterManager.Instance.RestoreMonster(monsterData);
                }
            }
        }
        
        /// <summary>
        /// インベントリを復元
        /// </summary>
        private void RestoreInventory(List<TrapItemSaveData> inventoryDataList)
        {
            if (inventoryDataList == null || Scripts.Managers.InventoryManager.Instance == null) return;
            
            foreach (var itemData in inventoryDataList)
            {
                Scripts.Managers.InventoryManager.Instance.RestoreItem(itemData);
            }
        }
    }
}