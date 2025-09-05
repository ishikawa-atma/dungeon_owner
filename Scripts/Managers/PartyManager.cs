using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Managers
{
    /// <summary>
    /// パーティシステムの管理クラス
    /// パーティの作成、解散、移動、戦闘処理を管理
    /// </summary>
    public class PartyManager : MonoBehaviour
    {
        [Header("パーティ設定")]
        [SerializeField] private GameObject partyPrefab;
        [SerializeField] private int maxPartiesPerFloor = 5;
        [SerializeField] private float partyMovementSpeed = 2.0f;
        
        [Header("デバッグ")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private List<IParty> activeParties = new List<IParty>();
        private Dictionary<int, List<IParty>> floorParties = new Dictionary<int, List<IParty>>();
        
        public List<IParty> ActiveParties => activeParties;
        
        private void Awake()
        {
            // パーティプレハブが設定されていない場合、デフォルトを作成
            if (partyPrefab == null)
            {
                CreateDefaultPartyPrefab();
            }
        }
        
        private void Update()
        {
            // 非アクティブなパーティを除去
            CleanupInactiveParties();
        }
        
        /// <summary>
        /// 新しいパーティを作成
        /// </summary>
        /// <param name="members">パーティメンバーのリスト</param>
        /// <param name="floorIndex">配置する階層</param>
        /// <returns>作成されたパーティ</returns>
        public IParty CreateParty(List<ICharacterBase> members, int floorIndex = 0)
        {
            if (members == null || members.Count == 0)
            {
                if (enableDebugLogs) Debug.LogWarning("パーティ作成失敗: メンバーが指定されていません");
                return null;
            }
            
            // 階層のパーティ数制限チェック
            if (!CanCreatePartyOnFloor(floorIndex))
            {
                if (enableDebugLogs) Debug.LogWarning($"パーティ作成失敗: 階層{floorIndex}のパーティ数が上限に達しています");
                return null;
            }
            
            // パーティオブジェクトを作成
            GameObject partyObject = Instantiate(partyPrefab);
            IParty party = partyObject.GetComponent<IParty>();
            
            if (party == null)
            {
                if (enableDebugLogs) Debug.LogError("パーティプレハブにIPartyコンポーネントがありません");
                Destroy(partyObject);
                return null;
            }
            
            // メンバーを追加
            foreach (var member in members)
            {
                party.AddMember(member);
            }
            
            // パーティを登録
            activeParties.Add(party);
            RegisterPartyToFloor(party, floorIndex);
            
            if (enableDebugLogs) 
                Debug.Log($"パーティ作成完了: メンバー数{members.Count}, 階層{floorIndex}");
            
            return party;
        }
        
        /// <summary>
        /// パーティを解散
        /// </summary>
        /// <param name="party">解散するパーティ</param>
        public void DisbandParty(IParty party)
        {
            if (party == null) return;
            
            // パーティを解散
            party.DisbandParty();
            
            // 管理リストから除去
            activeParties.Remove(party);
            RemovePartyFromAllFloors(party);
            
            // GameObjectを破棄
            if (party is MonoBehaviour partyMono)
            {
                Destroy(partyMono.gameObject);
            }
            
            if (enableDebugLogs) Debug.Log("パーティを解散しました");
        }
        
        /// <summary>
        /// パーティの移動処理
        /// 要件19.1: パーティメンバーの同期移動システム
        /// </summary>
        /// <param name="party">移動するパーティ</param>
        /// <param name="destination">目標位置</param>
        public void HandlePartyMovement(IParty party, Vector2 destination)
        {
            if (party == null || !party.IsActive) return;
            
            party.MoveParty(destination);
            
            if (enableDebugLogs) 
                Debug.Log($"パーティ移動: {party.Members.Count}メンバー, 目標位置{destination}");
        }
        
        /// <summary>
        /// パーティ間の戦闘処理
        /// 要件19.2: パーティ内でのダメージ分散機能
        /// </summary>
        /// <param name="attackers">攻撃側パーティ</param>
        /// <param name="defenders">防御側パーティ</param>
        public void HandlePartyCombat(IParty attackers, IParty defenders)
        {
            if (attackers == null || defenders == null) return;
            if (!attackers.IsActive || !defenders.IsActive) return;
            
            // 攻撃側の総攻撃力を計算
            float totalAttackPower = CalculatePartyAttackPower(attackers);
            
            // 防御側にダメージを分散
            defenders.DistributeDamage(totalAttackPower);
            
            if (enableDebugLogs) 
                Debug.Log($"パーティ戦闘: 攻撃力{totalAttackPower}, 防御側メンバー{defenders.Members.Count}");
        }
        
        /// <summary>
        /// 指定階層にパーティを作成可能かチェック
        /// </summary>
        /// <param name="floorIndex">階層インデックス</param>
        /// <returns>作成可能な場合true</returns>
        public bool CanCreatePartyOnFloor(int floorIndex)
        {
            if (!floorParties.ContainsKey(floorIndex)) return true;
            
            return floorParties[floorIndex].Count < maxPartiesPerFloor;
        }
        
        /// <summary>
        /// 指定階層のパーティ数を取得
        /// </summary>
        /// <param name="floorIndex">階層インデックス</param>
        /// <returns>パーティ数</returns>
        public int GetPartyCountOnFloor(int floorIndex)
        {
            if (!floorParties.ContainsKey(floorIndex)) return 0;
            
            return floorParties[floorIndex].Count;
        }
        
        /// <summary>
        /// 指定階層のパーティリストを取得
        /// </summary>
        /// <param name="floorIndex">階層インデックス</param>
        /// <returns>パーティリスト</returns>
        public List<IParty> GetPartiesOnFloor(int floorIndex)
        {
            if (!floorParties.ContainsKey(floorIndex)) 
                return new List<IParty>();
            
            return new List<IParty>(floorParties[floorIndex]);
        }
        
        /// <summary>
        /// パーティの攻撃力を計算
        /// </summary>
        private float CalculatePartyAttackPower(IParty party)
        {
            float totalPower = 0f;
            
            foreach (var member in party.Members.Where(m => m.Health > 0))
            {
                // 基本攻撃力を仮定（実際の実装では各キャラクターの攻撃力を取得）
                totalPower += 10f; // 仮の値
            }
            
            return totalPower;
        }
        
        /// <summary>
        /// パーティを階層に登録
        /// </summary>
        private void RegisterPartyToFloor(IParty party, int floorIndex)
        {
            if (!floorParties.ContainsKey(floorIndex))
            {
                floorParties[floorIndex] = new List<IParty>();
            }
            
            floorParties[floorIndex].Add(party);
        }
        
        /// <summary>
        /// パーティを全階層から除去
        /// </summary>
        private void RemovePartyFromAllFloors(IParty party)
        {
            foreach (var floorPartyList in floorParties.Values)
            {
                floorPartyList.Remove(party);
            }
        }
        
        /// <summary>
        /// 非アクティブなパーティを除去
        /// </summary>
        private void CleanupInactiveParties()
        {
            var inactiveParties = activeParties.Where(p => !p.IsActive).ToList();
            
            foreach (var party in inactiveParties)
            {
                DisbandParty(party);
            }
        }
        
        /// <summary>
        /// デフォルトのパーティプレハブを作成
        /// </summary>
        private void CreateDefaultPartyPrefab()
        {
            GameObject prefab = new GameObject("DefaultParty");
            prefab.AddComponent<Party>();
            partyPrefab = prefab;
            
            if (enableDebugLogs) Debug.Log("デフォルトパーティプレハブを作成しました");
        }
        
        /// <summary>
        /// パーティシステムの状態をデバッグ表示
        /// </summary>
        [ContextMenu("Debug Party Status")]
        public void DebugPartyStatus()
        {
            Debug.Log($"=== パーティマネージャー状態 ===");
            Debug.Log($"アクティブパーティ数: {activeParties.Count}");
            
            foreach (var floorEntry in floorParties)
            {
                Debug.Log($"階層{floorEntry.Key}: {floorEntry.Value.Count}パーティ");
            }
            
            for (int i = 0; i < activeParties.Count; i++)
            {
                var party = activeParties[i];
                Debug.Log($"パーティ{i}: メンバー{party.Members.Count}, アクティブ{party.IsActive}");
            }
        }
    }
}