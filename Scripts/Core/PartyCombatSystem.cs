using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DungeonOwner.Interfaces;
using DungeonOwner.Managers;

namespace DungeonOwner.Core
{
    /// <summary>
    /// パーティ戦闘システム
    /// 要件19.3: パーティ内に回復スキル持ちが存在する場合、パーティメンバーに回復スキルを適用
    /// 要件19.4: 侵入者パーティが出現する場合、協力戦闘を実行
    /// </summary>
    public class PartyCombatSystem : MonoBehaviour
    {
        [Header("パーティ戦闘設定")]
        [SerializeField] private float partyCooperationRange = 3.0f;
        [SerializeField] private float healingInterval = 2.0f;
        [SerializeField] private float healingAmount = 15.0f;
        [SerializeField] private float partyAttackBonus = 1.2f; // パーティ攻撃時のボーナス
        
        [Header("デバッグ")]
        [SerializeField] private bool enableDebugLogs = true;
        
        private Dictionary<IParty, float> lastHealingTime = new Dictionary<IParty, float>();
        private Dictionary<IParty, List<ICharacterBase>> partyHealers = new Dictionary<IParty, List<ICharacterBase>>();
        
        public static PartyCombatSystem Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Update()
        {
            ProcessPartyHealing();
            ProcessPartyCooperation();
        }
        
        /// <summary>
        /// パーティ内の回復スキル処理
        /// 要件19.3: パーティ内に回復スキル持ちが存在する場合、パーティメンバーに回復スキルを適用
        /// </summary>
        private void ProcessPartyHealing()
        {
            if (PartyManager.Instance == null) return;
            
            var activeParties = PartyManager.Instance.ActiveParties;
            
            foreach (var party in activeParties)
            {
                if (!party.IsActive || party.Members.Count == 0) continue;
                
                // 回復スキル持ちメンバーを特定
                var healers = GetHealersInParty(party);
                if (healers.Count == 0) continue;
                
                // 回復間隔チェック
                if (!CanHealParty(party)) continue;
                
                // パーティメンバーに回復を適用
                ApplyPartyHealing(party, healers);
                
                // 回復時間を記録
                lastHealingTime[party] = Time.time;
            }
        }
        
        /// <summary>
        /// パーティ間の協力戦闘処理
        /// 要件19.4: 侵入者パーティが出現する場合、協力戦闘を実行
        /// </summary>
        private void ProcessPartyCooperation()
        {
            if (PartyManager.Instance == null) return;
            
            var activeParties = PartyManager.Instance.ActiveParties;
            
            // 侵入者パーティとモンスターパーティを分離
            var invaderParties = activeParties.Where(p => IsInvaderParty(p)).ToList();
            var monsterParties = activeParties.Where(p => IsMonsterParty(p)).ToList();
            
            // パーティ間戦闘を処理
            foreach (var invaderParty in invaderParties)
            {
                foreach (var monsterParty in monsterParties)
                {
                    if (ArePartiesInCombatRange(invaderParty, monsterParty))
                    {
                        ProcessPartyVsPartyCombat(invaderParty, monsterParty);
                    }
                }
            }
        }
        
        /// <summary>
        /// パーティ内の回復スキル持ちメンバーを取得
        /// </summary>
        private List<ICharacterBase> GetHealersInParty(IParty party)
        {
            var healers = new List<ICharacterBase>();
            
            foreach (var member in party.Members)
            {
                if (member.Health <= 0) continue; // 死亡メンバーは除外
                
                // 回復スキルを持つかチェック
                if (HasHealingAbility(member))
                {
                    healers.Add(member);
                }
            }
            
            return healers;
        }
        
        /// <summary>
        /// キャラクターが回復スキルを持つかチェック
        /// </summary>
        private bool HasHealingAbility(ICharacterBase character)
        {
            // Clericクラスは回復スキルを持つ
            if (character.GetType().Name.Contains("Cleric"))
            {
                return true;
            }
            
            // モンスターの場合、スライムは自動回復アビリティを持つ
            if (character is IMonster monster)
            {
                var monsterComponent = monster as Monsters.BaseMonster;
                if (monsterComponent != null)
                {
                    // スライムの自動回復アビリティをチェック
                    return monsterComponent.GetType().Name.Contains("Slime");
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// パーティに回復を適用可能かチェック
        /// </summary>
        private bool CanHealParty(IParty party)
        {
            if (!lastHealingTime.ContainsKey(party))
            {
                lastHealingTime[party] = 0f;
            }
            
            return Time.time - lastHealingTime[party] >= healingInterval;
        }
        
        /// <summary>
        /// パーティメンバーに回復を適用
        /// </summary>
        private void ApplyPartyHealing(IParty party, List<ICharacterBase> healers)
        {
            // 回復が必要なメンバーを特定
            var injuredMembers = party.Members.Where(m => m.Health > 0 && m.Health < m.MaxHealth).ToList();
            
            if (injuredMembers.Count == 0) return;
            
            // 回復量を計算（回復スキル持ちの数に応じて増加）
            float totalHealAmount = healingAmount * healers.Count;
            
            // 各負傷メンバーに回復を適用
            foreach (var member in injuredMembers)
            {
                float healPerMember = totalHealAmount / injuredMembers.Count;
                
                if (member is ICharacter character)
                {
                    character.Heal(healPerMember);
                }
                
                // 回復エフェクトを表示
                ShowHealingEffect(member);
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"パーティ回復適用: 回復者{healers.Count}名, 対象{injuredMembers.Count}名, 回復量{totalHealAmount}");
            }
        }
        
        /// <summary>
        /// 侵入者パーティかチェック
        /// </summary>
        private bool IsInvaderParty(IParty party)
        {
            if (party.Members.Count == 0) return false;
            
            return party.Members.Any(m => m is IInvader);
        }
        
        /// <summary>
        /// モンスターパーティかチェック
        /// </summary>
        private bool IsMonsterParty(IParty party)
        {
            if (party.Members.Count == 0) return false;
            
            return party.Members.Any(m => m is IMonster);
        }
        
        /// <summary>
        /// パーティ間が戦闘範囲内にあるかチェック
        /// </summary>
        private bool ArePartiesInCombatRange(IParty party1, IParty party2)
        {
            float distance = Vector2.Distance(party1.Position, party2.Position);
            return distance <= partyCooperationRange;
        }
        
        /// <summary>
        /// パーティ対パーティの戦闘処理
        /// </summary>
        private void ProcessPartyVsPartyCombat(IParty attackerParty, IParty defenderParty)
        {
            if (!attackerParty.IsActive || !defenderParty.IsActive) return;
            
            // 攻撃側の総攻撃力を計算
            float totalAttackPower = CalculatePartyAttackPower(attackerParty);
            
            // パーティボーナスを適用
            totalAttackPower *= partyAttackBonus;
            
            // 防御側にダメージを分散
            defenderParty.DistributeDamage(totalAttackPower);
            
            // 戦闘エフェクトを表示
            ShowPartyCombatEffect(attackerParty, defenderParty, totalAttackPower);
            
            if (enableDebugLogs)
            {
                Debug.Log($"パーティ戦闘: 攻撃側{attackerParty.Members.Count}名 vs 防御側{defenderParty.Members.Count}名, ダメージ{totalAttackPower}");
            }
        }
        
        /// <summary>
        /// パーティの総攻撃力を計算
        /// </summary>
        private float CalculatePartyAttackPower(IParty party)
        {
            float totalPower = 0f;
            
            foreach (var member in party.Members.Where(m => m.Health > 0))
            {
                if (member is IMonster monster)
                {
                    var monsterComponent = monster as Monsters.BaseMonster;
                    totalPower += monsterComponent?.GetAttackPower() ?? 20f;
                }
                else if (member is IInvader invader)
                {
                    var invaderComponent = invader as Invaders.BaseInvader;
                    totalPower += invaderComponent?.GetAttackPower() ?? 15f;
                }
            }
            
            return totalPower;
        }
        
        /// <summary>
        /// 回復エフェクトを表示
        /// </summary>
        private void ShowHealingEffect(ICharacterBase character)
        {
            Vector3 position = character.Position;
            
            // CombatEffectsシステムを使用
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.ShowHealingEffect(position);
            }
            
            // スプライトを緑色に点滅
            var characterMono = character as MonoBehaviour;
            if (characterMono != null)
            {
                var spriteRenderer = characterMono.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && CombatEffects.Instance != null)
                {
                    CombatEffects.Instance.FlashSprite(spriteRenderer, Color.green, 0.3f);
                }
            }
        }
        
        /// <summary>
        /// パーティ戦闘エフェクトを表示
        /// </summary>
        private void ShowPartyCombatEffect(IParty attackerParty, IParty defenderParty, float damage)
        {
            Vector3 midPoint = (attackerParty.Position + defenderParty.Position) / 2f;
            
            // CombatEffectsシステムを使用
            if (CombatEffects.Instance != null)
            {
                CombatEffects.Instance.ShowPartyBattleEffect(midPoint, damage);
            }
            
            // カメラシェイク
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.2f, 0.15f);
            }
        }
        
        /// <summary>
        /// パーティに特殊スキルを適用
        /// </summary>
        public void ApplyPartySkill(IParty party, PartySkillType skillType, float effectValue)
        {
            if (!party.IsActive) return;
            
            switch (skillType)
            {
                case PartySkillType.GroupHeal:
                    party.ApplyPartyHealing(effectValue);
                    break;
                    
                case PartySkillType.AttackBoost:
                    ApplyAttackBoost(party, effectValue);
                    break;
                    
                case PartySkillType.DefenseBoost:
                    ApplyDefenseBoost(party, effectValue);
                    break;
            }
            
            if (enableDebugLogs)
            {
                Debug.Log($"パーティスキル適用: {skillType}, 効果値{effectValue}, 対象{party.Members.Count}名");
            }
        }
        
        /// <summary>
        /// パーティに攻撃力ブーストを適用
        /// </summary>
        private void ApplyAttackBoost(IParty party, float boostValue)
        {
            // 実装は各キャラクタークラスでバフシステムが必要
            // 現在は概念的な実装
            foreach (var member in party.Members.Where(m => m.Health > 0))
            {
                // バフエフェクトを表示
                ShowBuffEffect(member, Color.red);
            }
        }
        
        /// <summary>
        /// パーティに防御力ブーストを適用
        /// </summary>
        private void ApplyDefenseBoost(IParty party, float boostValue)
        {
            // 実装は各キャラクタークラスでバフシステムが必要
            // 現在は概念的な実装
            foreach (var member in party.Members.Where(m => m.Health > 0))
            {
                // バフエフェクトを表示
                ShowBuffEffect(member, Color.blue);
            }
        }
        
        /// <summary>
        /// バフエフェクトを表示
        /// </summary>
        private void ShowBuffEffect(ICharacterBase character, Color effectColor)
        {
            var characterMono = character as MonoBehaviour;
            if (characterMono != null)
            {
                var spriteRenderer = characterMono.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && CombatEffects.Instance != null)
                {
                    CombatEffects.Instance.FlashSprite(spriteRenderer, effectColor, 0.5f);
                }
            }
        }
        
        /// <summary>
        /// デバッグ用：パーティ戦闘状況を表示
        /// </summary>
        [ContextMenu("Debug Party Combat Status")]
        public void DebugPartyCombatStatus()
        {
            Debug.Log("=== パーティ戦闘システム状況 ===");
            
            if (PartyManager.Instance != null)
            {
                var activeParties = PartyManager.Instance.ActiveParties;
                Debug.Log($"アクティブパーティ数: {activeParties.Count}");
                
                foreach (var party in activeParties)
                {
                    var healers = GetHealersInParty(party);
                    bool isInvader = IsInvaderParty(party);
                    bool isMonster = IsMonsterParty(party);
                    
                    Debug.Log($"パーティ - メンバー{party.Members.Count}名, 回復者{healers.Count}名, " +
                             $"種類: {(isInvader ? "侵入者" : isMonster ? "モンスター" : "混合")}");
                }
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // パーティ協力範囲を可視化
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, partyCooperationRange);
        }
    }
    
    /// <summary>
    /// パーティスキルの種類
    /// </summary>
    public enum PartySkillType
    {
        GroupHeal,      // グループ回復
        AttackBoost,    // 攻撃力上昇
        DefenseBoost    // 防御力上昇
    }
}