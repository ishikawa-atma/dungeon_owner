using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Interfaces;
using DungeonOwner.Data;
using DungeonOwner.Monsters;
using DungeonOwner.Invaders;
using DungeonOwner.Managers;

namespace DungeonOwner.Core
{
    /// <summary>
    /// リアルタイム戦闘システムの管理クラス
    /// モンスターと侵入者の衝突判定、ダメージ計算、戦闘エフェクトを処理
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        [Header("戦闘設定")]
        [SerializeField] private float combatRange = 1.5f;
        [SerializeField] private float combatInterval = 1.0f; // 戦闘判定間隔
        [SerializeField] private float knockbackForce = 2.0f;
        [SerializeField] private float knockbackDuration = 0.3f;

        [Header("確率設定")]
        [SerializeField] private float baseDamageChance = 0.7f; // 基本ダメージ確率
        [SerializeField] private float levelDifferenceModifier = 0.1f; // レベル差による補正

        [Header("エフェクト")]
        [SerializeField] private GameObject damageEffectPrefab;
        [SerializeField] private GameObject knockbackEffectPrefab;

        private Dictionary<GameObject, float> lastCombatTime = new Dictionary<GameObject, float>();
        private Dictionary<GameObject, Vector3> knockbackTargets = new Dictionary<GameObject, Vector3>();
        private Dictionary<GameObject, float> knockbackEndTime = new Dictionary<GameObject, float>();

        public static CombatManager Instance { get; private set; }

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
            ProcessKnockbacks();
        }

        /// <summary>
        /// モンスターと侵入者の戦闘処理
        /// </summary>
        public void ProcessCombat(IMonster monster, IInvader invader)
        {
            if (monster == null || invader == null) return;

            GameObject monsterObj = (monster as MonoBehaviour)?.gameObject;
            GameObject invaderObj = (invader as MonoBehaviour)?.gameObject;

            if (monsterObj == null || invaderObj == null) return;

            // 戦闘間隔チェック
            if (!CanEngageInCombat(monsterObj) || !CanEngageInCombat(invaderObj))
                return;

            // 距離チェック
            float distance = Vector2.Distance(monster.Position, invader.Position);
            if (distance > combatRange)
                return;

            // パーティ戦闘かチェック
            if (monster.Party != null && invader.Party != null)
            {
                // パーティ対パーティの戦闘
                ProcessPartyCombat(monster.Party, invader.Party);
            }
            else if (monster.Party != null)
            {
                // モンスターパーティ対単体侵入者
                ProcessPartyVsIndividualCombat(monster.Party, invader);
            }
            else if (invader.Party != null)
            {
                // 単体モンスター対侵入者パーティ
                ProcessIndividualVsPartyCombat(monster, invader.Party);
            }
            else
            {
                // 通常の1対1戦闘
                ExecuteCombat(monster, invader, monsterObj, invaderObj);
            }
        }

        /// <summary>
        /// 戦闘実行処理
        /// </summary>
        private void ExecuteCombat(IMonster monster, IInvader invader, GameObject monsterObj, GameObject invaderObj)
        {
            // 戦闘時間を記録
            lastCombatTime[monsterObj] = Time.time;
            lastCombatTime[invaderObj] = Time.time;

            // モンスターの攻撃
            ProcessAttack(monster, invader, monsterObj, invaderObj);

            // 侵入者の攻撃
            ProcessAttack(invader, monster, invaderObj, monsterObj);
        }

        /// <summary>
        /// 攻撃処理（ジェネリック）
        /// </summary>
        private void ProcessAttack(ICharacterBase attacker, ICharacterBase defender, GameObject attackerObj, GameObject defenderObj)
        {
            float attackPower = GetAttackPower(attacker);
            float damageChance = CalculateDamageChance(attacker, defender);

            // ダメージ判定
            if (Random.value <= damageChance)
            {
                // ダメージ計算
                float damage = CalculateDamage(attackPower, attacker, defender);
                
                // ダメージ適用
                ApplyDamage(defender, damage);

                // 吹き飛ばし効果
                ApplyKnockback(defenderObj, attackerObj.transform.position);

                // エフェクト再生
                PlayCombatEffects(attackerObj, defenderObj, damage);

                Debug.Log($"{attackerObj.name} が {defenderObj.name} に {damage:F1} ダメージを与えました");
            }
            else
            {
                Debug.Log($"{attackerObj.name} の攻撃が外れました");
            }
        }

        /// <summary>
        /// ダメージ確率計算（レベル差を詳細に考慮）
        /// </summary>
        private float CalculateDamageChance(ICharacterBase attacker, ICharacterBase defender)
        {
            float chance = baseDamageChance;

            // レベル差による補正（より詳細な計算）
            int levelDifference = GetLevel(attacker) - GetLevel(defender);
            
            if (levelDifference > 0)
            {
                // 攻撃側が高レベルの場合：命中率上昇
                chance += levelDifference * levelDifferenceModifier;
            }
            else if (levelDifference < 0)
            {
                // 防御側が高レベルの場合：回避率上昇
                chance += levelDifference * (levelDifferenceModifier * 0.7f);
            }

            // レベル差が大きい場合の特別処理
            if (Mathf.Abs(levelDifference) >= 5)
            {
                float extremeBonus = (Mathf.Abs(levelDifference) - 4) * 0.05f;
                chance += levelDifference > 0 ? extremeBonus : -extremeBonus;
            }

            return Mathf.Clamp01(chance);
        }

        /// <summary>
        /// ダメージ計算（レベル差を詳細に考慮）
        /// </summary>
        private float CalculateDamage(float baseAttack, ICharacterBase attacker, ICharacterBase defender)
        {
            float damage = baseAttack;

            // レベル差による補正（より詳細な計算）
            int attackerLevel = GetLevel(attacker);
            int defenderLevel = GetLevel(defender);
            int levelDifference = attackerLevel - defenderLevel;

            // 基本レベル補正
            if (levelDifference > 0)
            {
                // 攻撃側が高レベル：ダメージ増加
                float damageMultiplier = 1f + (levelDifference * 0.15f);
                damage *= damageMultiplier;
            }
            else if (levelDifference < 0)
            {
                // 防御側が高レベル：ダメージ軽減
                float damageReduction = 1f + (levelDifference * 0.08f);
                damage *= damageReduction;
            }

            // 絶対レベルによる基本ダメージ補正
            float attackerLevelBonus = 1f + (attackerLevel - 1) * 0.05f;
            damage *= attackerLevelBonus;

            // 極端なレベル差の場合の特別処理
            if (Mathf.Abs(levelDifference) >= 10)
            {
                if (levelDifference > 0)
                {
                    damage *= 1.5f; // 圧倒的優位
                }
                else
                {
                    damage *= 0.3f; // 圧倒的劣勢
                }
            }

            // ランダム要素（±15%）
            damage *= Random.Range(0.85f, 1.15f);

            // 最低ダメージ保証（レベル差を考慮）
            float minDamage = Mathf.Max(1f, attackerLevel * 0.5f);
            return Mathf.Max(minDamage, damage);
        }

        /// <summary>
        /// ダメージ適用
        /// </summary>
        private void ApplyDamage(ICharacterBase target, float damage)
        {
            float previousHealth = target.Health;
            
            if (target is IMonster monster)
            {
                monster.TakeDamage(damage);
            }
            else if (target is IInvader invader)
            {
                invader.TakeDamage(damage);
                
                // 侵入者が撃破された場合の報酬処理
                if (previousHealth > 0 && invader.Health <= 0)
                {
                    ProcessInvaderDefeat(invader);
                }
            }
        }

        /// <summary>
        /// 侵入者撃破処理
        /// </summary>
        private void ProcessInvaderDefeat(IInvader invader)
        {
            // ResourceManagerに報酬処理を委譲
            if (ResourceManager.Instance != null)
            {
                ResourceManager.Instance.ProcessInvaderDefeatReward(invader);
            }

            // InvaderSpawnerに撃破通知
            if (InvaderSpawner.Instance != null)
            {
                var invaderObj = (invader as MonoBehaviour)?.gameObject;
                if (invaderObj != null)
                {
                    InvaderSpawner.Instance.OnInvaderDestroyed(invaderObj);
                }
            }

            Debug.Log($"Invader {invader.Type} (Lv.{invader.Level}) defeated!");
        }

        /// <summary>
        /// 吹き飛ばし効果適用
        /// </summary>
        private void ApplyKnockback(GameObject target, Vector3 attackerPosition)
        {
            Vector3 knockbackDirection = (target.transform.position - attackerPosition).normalized;
            Vector3 knockbackTarget = target.transform.position + knockbackDirection * knockbackForce;

            knockbackTargets[target] = knockbackTarget;
            knockbackEndTime[target] = Time.time + knockbackDuration;
        }

        /// <summary>
        /// 吹き飛ばし処理更新
        /// </summary>
        private void ProcessKnockbacks()
        {
            var toRemove = new List<GameObject>();

            foreach (var kvp in knockbackTargets)
            {
                GameObject obj = kvp.Key;
                Vector3 targetPos = kvp.Value;

                if (obj == null || Time.time >= knockbackEndTime[obj])
                {
                    toRemove.Add(obj);
                    continue;
                }

                // 吹き飛ばし移動
                float progress = 1f - (knockbackEndTime[obj] - Time.time) / knockbackDuration;
                Vector3 currentPos = Vector3.Lerp(obj.transform.position, targetPos, progress * 5f * Time.deltaTime);
                obj.transform.position = currentPos;
            }

            // 完了した吹き飛ばしを削除
            foreach (var obj in toRemove)
            {
                knockbackTargets.Remove(obj);
                knockbackEndTime.Remove(obj);
            }
        }

        /// <summary>
        /// 戦闘エフェクト再生
        /// </summary>
        private void PlayCombatEffects(GameObject attacker, GameObject defender, float damage)
        {
            Vector3 defenderPos = defender.transform.position;

            // CombatEffectsシステムを使用
            if (CombatEffects.Instance != null)
            {
                // ダメージテキスト表示
                bool isCritical = damage > GetAttackPower(attacker as ICharacterBase) * 1.5f;
                CombatEffects.Instance.ShowDamageText(defenderPos, damage, isCritical);
                
                // ヒットエフェクト再生
                CombatEffects.Instance.PlayHitEffect(defenderPos, isCritical);
                
                // スプライト点滅
                var spriteRenderer = defender.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    CombatEffects.Instance.FlashSprite(spriteRenderer, Color.red, 0.2f);
                }
            }
            else
            {
                // フォールバック: 基本エフェクト
                if (damageEffectPrefab != null)
                {
                    Vector3 effectPos = defenderPos + Vector3.up * 0.5f;
                    GameObject effect = Instantiate(damageEffectPrefab, effectPos, Quaternion.identity);
                    
                    var textComponent = effect.GetComponentInChildren<TMPro.TextMeshPro>();
                    if (textComponent != null)
                    {
                        textComponent.text = Mathf.RoundToInt(damage).ToString();
                    }

                    Destroy(effect, 1f);
                }

                if (knockbackEffectPrefab != null)
                {
                    GameObject effect = Instantiate(knockbackEffectPrefab, defenderPos, Quaternion.identity);
                    Destroy(effect, 0.5f);
                }
            }

            // 画面シェイク
            CameraShake.Instance?.Shake(0.1f, 0.1f);
        }

        /// <summary>
        /// 戦闘可能かチェック
        /// </summary>
        private bool CanEngageInCombat(GameObject obj)
        {
            if (!lastCombatTime.ContainsKey(obj))
            {
                lastCombatTime[obj] = 0f;
            }

            return Time.time - lastCombatTime[obj] >= combatInterval;
        }

        /// <summary>
        /// 攻撃力取得
        /// </summary>
        private float GetAttackPower(ICharacterBase character)
        {
            if (character is IMonster monster)
            {
                var monsterComponent = monster as Monsters.BaseMonster;
                return monsterComponent?.GetAttackPower() ?? 20f;
            }
            else if (character is IInvader invader)
            {
                var invaderComponent = invader as Invaders.BaseInvader;
                return invaderComponent?.GetAttackPower() ?? 15f;
            }

            return 10f;
        }

        /// <summary>
        /// レベル取得
        /// </summary>
        private int GetLevel(ICharacterBase character)
        {
            if (character is IMonster monster)
            {
                return monster.Level;
            }
            else if (character is IInvader invader)
            {
                return invader.Level;
            }

            return 1;
        }

        /// <summary>
        /// 戦闘範囲内の敵を検索
        /// </summary>
        public List<ICharacterBase> FindEnemiesInRange(Vector2 position, float range, bool findMonsters)
        {
            var enemies = new List<ICharacterBase>();
            string targetTag = findMonsters ? "Monster" : "Invader";

            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, range);

            foreach (var collider in colliders)
            {
                if (collider.CompareTag(targetTag))
                {
                    if (findMonsters)
                    {
                        var monster = collider.GetComponent<IMonster>();
                        if (monster != null)
                        {
                            enemies.Add(monster as ICharacterBase);
                        }
                    }
                    else
                    {
                        var invader = collider.GetComponent<IInvader>();
                        if (invader != null)
                        {
                            enemies.Add(invader as ICharacterBase);
                        }
                    }
                }
            }

            return enemies;
        }

        /// <summary>
        /// 設定値の動的変更
        /// </summary>
        public void SetCombatSettings(float range, float interval, float knockback)
        {
            combatRange = range;
            combatInterval = interval;
            knockbackForce = knockback;
        }

        /// <summary>
        /// パーティ対パーティの戦闘処理
        /// 要件19.4: 侵入者パーティが出現する場合、協力戦闘を実行
        /// </summary>
        private void ProcessPartyCombat(IParty monsterParty, IParty invaderParty)
        {
            if (PartyCombatSystem.Instance != null)
            {
                PartyCombatSystem.Instance.HandlePartyCombat(monsterParty, invaderParty);
            }
            else
            {
                // フォールバック: 従来の戦闘システム
                if (PartyManager.Instance != null)
                {
                    PartyManager.Instance.HandlePartyCombat(monsterParty, invaderParty);
                }
            }
        }

        /// <summary>
        /// パーティ対単体の戦闘処理
        /// </summary>
        private void ProcessPartyVsIndividualCombat(IParty party, ICharacterBase individual)
        {
            // パーティの総攻撃力を計算
            float totalAttackPower = 0f;
            foreach (var member in party.Members)
            {
                if (member.Health > 0)
                {
                    totalAttackPower += GetAttackPower(member);
                }
            }

            // パーティボーナスを適用
            totalAttackPower *= 1.3f;

            // 単体キャラクターにダメージを適用
            ApplyDamage(individual, totalAttackPower);

            // エフェクト表示
            Vector3 combatPosition = (party.Position + individual.Position) / 2f;
            PlayCombatEffects(null, individual as MonoBehaviour, totalAttackPower);

            Debug.Log($"パーティ({party.Members.Count}名) vs 単体戦闘: ダメージ{totalAttackPower}");
        }

        /// <summary>
        /// 単体対パーティの戦闘処理
        /// </summary>
        private void ProcessIndividualVsPartyCombat(ICharacterBase individual, IParty party)
        {
            float attackPower = GetAttackPower(individual);

            // パーティにダメージを分散
            party.DistributeDamage(attackPower);

            // エフェクト表示
            Vector3 combatPosition = (individual.Position + party.Position) / 2f;
            PlayCombatEffects(individual as MonoBehaviour, null, attackPower);

            Debug.Log($"単体 vs パーティ({party.Members.Count}名)戦闘: ダメージ{attackPower}");
        }

        private void OnDrawGizmosSelected()
        {
            // 戦闘範囲の可視化
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, combatRange);
        }
    }
}