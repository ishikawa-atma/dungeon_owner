using UnityEngine;
using DungeonOwner.Monsters;
using DungeonOwner.Managers;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// HP/MP回復システムのデモンストレーション
    /// </summary>
    public class RecoverySystemDemo : MonoBehaviour
    {
        [Header("デモ設定")]
        [SerializeField] private bool autoStartDemo = true;
        [SerializeField] private float demoInterval = 3f;
        [SerializeField] private int testMonsterCount = 5;
        
        [Header("モンスタープレハブ")]
        [SerializeField] private GameObject[] monsterPrefabs;
        
        [Header("デモエリア")]
        [SerializeField] private Transform floorArea;
        [SerializeField] private Transform shelterArea;
        [SerializeField] private float areaSize = 5f;
        
        private BaseMonster[] demoMonsters;
        private float lastDemoAction;
        private int demoStep = 0;
        
        private void Start()
        {
            if (autoStartDemo)
            {
                StartDemo();
            }
        }
        
        private void Update()
        {
            if (demoMonsters == null) return;
            
            // 定期的にデモアクションを実行
            if (Time.time - lastDemoAction >= demoInterval)
            {
                ExecuteDemoStep();
                lastDemoAction = Time.time;
            }
            
            // デモ情報を表示
            DisplayDemoInfo();
        }
        
        /// <summary>
        /// デモを開始
        /// </summary>
        [ContextMenu("デモ開始")]
        public void StartDemo()
        {
            Debug.Log("[RecoverySystemDemo] デモを開始します");
            
            // 既存のデモモンスターをクリア
            ClearDemoMonsters();
            
            // デモモンスターを生成
            CreateDemoMonsters();
            
            // 回復システムの設定
            SetupRecoverySystem();
            
            demoStep = 0;
            lastDemoAction = Time.time;
            
            Debug.Log("[RecoverySystemDemo] デモ準備完了");
        }
        
        /// <summary>
        /// デモモンスターを生成
        /// </summary>
        private void CreateDemoMonsters()
        {
            demoMonsters = new BaseMonster[testMonsterCount];
            
            for (int i = 0; i < testMonsterCount; i++)
            {
                // ランダムなモンスタープレハブを選択
                GameObject prefab = GetRandomMonsterPrefab();
                if (prefab == null) continue;
                
                // 階層エリアに配置
                Vector3 position = GetRandomPositionInArea(floorArea, areaSize);
                GameObject monsterObj = Instantiate(prefab, position, Quaternion.identity);
                
                BaseMonster monster = monsterObj.GetComponent<BaseMonster>();
                if (monster != null)
                {
                    demoMonsters[i] = monster;
                    
                    // 初期ダメージを与える（回復を見やすくするため）
                    monster.TakeDamage(monster.MaxHealth * 0.5f);
                    
                    Debug.Log($"[RecoverySystemDemo] デモモンスター{i+1}を生成: {monster.Type}");
                }
            }
        }
        
        /// <summary>
        /// 回復システムの設定
        /// </summary>
        private void SetupRecoverySystem()
        {
            if (RecoveryManager.Instance != null)
            {
                // デモ用の回復率を設定
                RecoveryManager.Instance.SetRecoveryRates(2f, 8f);
                RecoveryManager.Instance.SetRecoveryUIVisible(true);
                
                Debug.Log("[RecoverySystemDemo] 回復システム設定完了 - 階層:2/秒, 退避:8/秒");
            }
        }
        
        /// <summary>
        /// デモステップを実行
        /// </summary>
        private void ExecuteDemoStep()
        {
            if (demoMonsters == null) return;
            
            switch (demoStep % 6)
            {
                case 0:
                    Debug.Log("[RecoverySystemDemo] ステップ1: 全モンスターにダメージ");
                    DamageAllMonsters(30f);
                    break;
                    
                case 1:
                    Debug.Log("[RecoverySystemDemo] ステップ2: 階層での自然回復を観察");
                    // 何もしない（自然回復を観察）
                    break;
                    
                case 2:
                    Debug.Log("[RecoverySystemDemo] ステップ3: 半数を退避スポットに移動");
                    MoveHalfToShelter();
                    break;
                    
                case 3:
                    Debug.Log("[RecoverySystemDemo] ステップ4: 退避スポットでの高速回復を観察");
                    // 何もしない（高速回復を観察）
                    break;
                    
                case 4:
                    Debug.Log("[RecoverySystemDemo] ステップ5: 全モンスターを階層に戻す");
                    MoveAllToFloor();
                    break;
                    
                case 5:
                    Debug.Log("[RecoverySystemDemo] ステップ6: 回復率を変更してテスト");
                    ChangeRecoveryRates();
                    break;
            }
            
            demoStep++;
        }
        
        /// <summary>
        /// 全モンスターにダメージを与える
        /// </summary>
        private void DamageAllMonsters(float damage)
        {
            foreach (var monster in demoMonsters)
            {
                if (monster != null && monster.IsAlive())
                {
                    monster.TakeDamage(damage);
                }
            }
        }
        
        /// <summary>
        /// 半数のモンスターを退避スポットに移動
        /// </summary>
        private void MoveHalfToShelter()
        {
            for (int i = 0; i < demoMonsters.Length / 2; i++)
            {
                if (demoMonsters[i] != null && demoMonsters[i].IsAlive())
                {
                    // 退避エリアに移動
                    Vector3 shelterPos = GetRandomPositionInArea(shelterArea, areaSize);
                    demoMonsters[i].transform.position = shelterPos;
                    
                    // 退避状態に設定
                    demoMonsters[i].SetState(MonsterState.InShelter);
                }
            }
        }
        
        /// <summary>
        /// 全モンスターを階層に戻す
        /// </summary>
        private void MoveAllToFloor()
        {
            foreach (var monster in demoMonsters)
            {
                if (monster != null && monster.IsAlive())
                {
                    // 階層エリアに移動
                    Vector3 floorPos = GetRandomPositionInArea(floorArea, areaSize);
                    monster.transform.position = floorPos;
                    
                    // アイドル状態に設定
                    monster.SetState(MonsterState.Idle);
                }
            }
        }
        
        /// <summary>
        /// 回復率を変更
        /// </summary>
        private void ChangeRecoveryRates()
        {
            if (RecoveryManager.Instance != null)
            {
                float[] floorRates = { 1f, 2f, 3f, 5f };
                float[] shelterRates = { 5f, 8f, 12f, 20f };
                
                int index = (demoStep / 6) % floorRates.Length;
                RecoveryManager.Instance.SetRecoveryRates(floorRates[index], shelterRates[index]);
                
                Debug.Log($"[RecoverySystemDemo] 回復率変更: 階層={floorRates[index]}/秒, 退避={shelterRates[index]}/秒");
            }
        }
        
        /// <summary>
        /// デモ情報を表示
        /// </summary>
        private void DisplayDemoInfo()
        {
            if (RecoveryManager.Instance == null) return;
            
            // 10秒ごとに統計情報を表示
            if (Time.time % 10f < Time.deltaTime)
            {
                var stats = RecoveryManager.Instance.GetRecoveryStats();
                Debug.Log($"[RecoverySystemDemo] 統計 - 総数:{stats.TotalMonsters}, 回復中:{stats.RecoveringMonsters}, 退避中:{stats.ShelterMonsters}");
                
                // 個別モンスターの状況
                for (int i = 0; i < demoMonsters.Length; i++)
                {
                    var monster = demoMonsters[i];
                    if (monster != null && monster.IsAlive())
                    {
                        float hpRatio = monster.Health / monster.MaxHealth;
                        float mpRatio = monster.Mana / monster.MaxMana;
                        Debug.Log($"  モンスター{i+1}: HP={hpRatio:P0}, MP={mpRatio:P0}, 状態={monster.State}");
                    }
                }
            }
        }
        
        /// <summary>
        /// ランダムなモンスタープレハブを取得
        /// </summary>
        private GameObject GetRandomMonsterPrefab()
        {
            if (monsterPrefabs == null || monsterPrefabs.Length == 0)
            {
                Debug.LogWarning("[RecoverySystemDemo] モンスタープレハブが設定されていません");
                return null;
            }
            
            return monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
        }
        
        /// <summary>
        /// エリア内のランダムな位置を取得
        /// </summary>
        private Vector3 GetRandomPositionInArea(Transform area, float size)
        {
            Vector3 basePos = area != null ? area.position : Vector3.zero;
            Vector3 randomOffset = new Vector3(
                Random.Range(-size/2, size/2),
                Random.Range(-size/2, size/2),
                0
            );
            
            return basePos + randomOffset;
        }
        
        /// <summary>
        /// デモモンスターをクリア
        /// </summary>
        private void ClearDemoMonsters()
        {
            if (demoMonsters != null)
            {
                foreach (var monster in demoMonsters)
                {
                    if (monster != null)
                    {
                        DestroyImmediate(monster.gameObject);
                    }
                }
            }
            
            demoMonsters = null;
        }
        
        /// <summary>
        /// デモを停止
        /// </summary>
        [ContextMenu("デモ停止")]
        public void StopDemo()
        {
            ClearDemoMonsters();
            Debug.Log("[RecoverySystemDemo] デモを停止しました");
        }
        
        private void OnDestroy()
        {
            ClearDemoMonsters();
        }
        
        private void OnDrawGizmosSelected()
        {
            // エリアの可視化
            if (floorArea != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(floorArea.position, Vector3.one * areaSize);
            }
            
            if (shelterArea != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(shelterArea.position, Vector3.one * areaSize);
            }
        }
    }
}