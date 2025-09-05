using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core
{
    /// <summary>
    /// パーティシステムの基本実装
    /// 要件19.1: パーティメンバーの同期移動
    /// 要件19.2: パーティ内でのダメージ分散
    /// 要件19.5: パーティメンバーが撃破された場合の継続処理
    /// </summary>
    public class Party : MonoBehaviour, IParty
    {
        [SerializeField] private List<ICharacterBase> members = new List<ICharacterBase>();
        [SerializeField] private Vector2 position;
        [SerializeField] private bool isActive = true;
        [SerializeField] private float moveSpeed = 2.0f;
        [SerializeField] private float formationSpacing = 1.0f;
        
        public List<ICharacterBase> Members => members;
        public Vector2 Position 
        { 
            get => position; 
            set => position = value; 
        }
        public bool IsActive => isActive && members.Count > 0;
        
        private void Update()
        {
            // パーティが無効または空の場合は処理しない
            if (!IsActive) return;
            
            // 死亡したメンバーを除去
            RemoveDeadMembers();
        }
        
        /// <summary>
        /// パーティにメンバーを追加
        /// </summary>
        public void AddMember(ICharacterBase character)
        {
            if (character == null || members.Contains(character)) return;
            
            members.Add(character);
            
            // ICharacterインターフェースを実装している場合、パーティ参照を設定
            if (character is ICharacter characterWithParty)
            {
                characterWithParty.JoinParty(this);
            }
            
            Debug.Log($"パーティにメンバーを追加: {character.GetType().Name}, 現在のメンバー数: {members.Count}");
        }
        
        /// <summary>
        /// パーティからメンバーを除去
        /// </summary>
        public void RemoveMember(ICharacterBase character)
        {
            if (character == null || !members.Contains(character)) return;
            
            members.Remove(character);
            
            // ICharacterインターフェースを実装している場合、パーティ参照を解除
            if (character is ICharacter characterWithParty)
            {
                characterWithParty.LeaveParty();
            }
            
            Debug.Log($"パーティからメンバーを除去: {character.GetType().Name}, 残りメンバー数: {members.Count}");
            
            // メンバーがいなくなったらパーティを解散
            if (members.Count == 0)
            {
                DisbandParty();
            }
        }
        
        /// <summary>
        /// パーティ内でダメージを分散
        /// 要件19.2: パーティメンバーが戦闘に参加する場合、ダメージをパーティ全員で負担
        /// </summary>
        public void DistributeDamage(float totalDamage)
        {
            if (!IsActive || members.Count == 0) return;
            
            // 生存しているメンバーのみを対象
            var aliveMembers = members.Where(m => m.Health > 0).ToList();
            if (aliveMembers.Count == 0) return;
            
            float damagePerMember = totalDamage / aliveMembers.Count;
            
            foreach (var member in aliveMembers)
            {
                member.TakeDamage(damagePerMember);
            }
            
            Debug.Log($"パーティダメージ分散: 総ダメージ{totalDamage}, メンバー数{aliveMembers.Count}, 個別ダメージ{damagePerMember}");
        }
        
        /// <summary>
        /// パーティメンバーに回復を適用
        /// </summary>
        public void ApplyPartyHealing(float healAmount)
        {
            if (!IsActive) return;
            
            foreach (var member in members.Where(m => m.Health > 0))
            {
                if (member is ICharacter character)
                {
                    character.Heal(healAmount);
                }
            }
            
            Debug.Log($"パーティ回復適用: 回復量{healAmount}, 対象メンバー数{members.Count}");
        }
        
        /// <summary>
        /// パーティ全体を移動
        /// 要件19.1: パーティメンバーの同期移動システム
        /// </summary>
        public void MoveParty(Vector2 targetPosition)
        {
            if (!IsActive) return;
            
            Position = targetPosition;
            
            // フォーメーションを維持しながらメンバーを移動
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] == null) continue;
                
                // 簡単なフォーメーション配置（横一列）
                Vector2 memberOffset = new Vector2(i * formationSpacing - (members.Count - 1) * formationSpacing * 0.5f, 0);
                Vector2 memberTargetPosition = targetPosition + memberOffset;
                
                members[i].Position = memberTargetPosition;
            }
        }
        
        /// <summary>
        /// パーティを解散
        /// </summary>
        public void DisbandParty()
        {
            Debug.Log($"パーティ解散: メンバー数{members.Count}");
            
            // 全メンバーのパーティ参照を解除
            foreach (var member in members.ToList())
            {
                if (member is ICharacter character)
                {
                    character.LeaveParty();
                }
            }
            
            members.Clear();
            isActive = false;
        }
        
        /// <summary>
        /// 死亡したメンバーを除去
        /// 要件19.5: パーティメンバーが撃破された場合、残りメンバーでパーティを継続
        /// </summary>
        private void RemoveDeadMembers()
        {
            var deadMembers = members.Where(m => m.Health <= 0).ToList();
            
            foreach (var deadMember in deadMembers)
            {
                RemoveMember(deadMember);
            }
        }
        
        /// <summary>
        /// パーティの状態をデバッグ表示
        /// </summary>
        public void DebugPartyStatus()
        {
            Debug.Log($"パーティ状態 - アクティブ: {IsActive}, メンバー数: {members.Count}, 位置: {Position}");
            
            for (int i = 0; i < members.Count; i++)
            {
                var member = members[i];
                if (member != null)
                {
                    Debug.Log($"  メンバー{i}: {member.GetType().Name}, HP: {member.Health}/{member.MaxHealth}, 位置: {member.Position}");
                }
            }
        }
    }
}