using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 戦闘対象の検出と戦闘開始を管理するクラス
    /// モンスターと侵入者の衝突を監視し、CombatManagerに戦闘処理を委譲
    /// </summary>
    public class CombatDetector : MonoBehaviour
    {
        [Header("検出設定")]
        [SerializeField] private float detectionRadius = 2f;
        [SerializeField] private float detectionInterval = 0.1f; // 検出間隔

        [Header("レイヤー設定")]
        [SerializeField] private LayerMask monsterLayer = 1 << 6;
        [SerializeField] private LayerMask invaderLayer = 1 << 7;

        private Dictionary<GameObject, List<GameObject>> activeEngagements = new Dictionary<GameObject, List<GameObject>>();
        private float lastDetectionTime;

        public static CombatDetector Instance { get; private set; }

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
            if (Time.time - lastDetectionTime >= detectionInterval)
            {
                DetectCombatSituations();
                lastDetectionTime = Time.time;
            }
        }

        /// <summary>
        /// 戦闘状況の検出
        /// </summary>
        private void DetectCombatSituations()
        {
            // 全てのモンスターを取得
            var monsters = FindObjectsOfType<MonoBehaviour>();
            
            foreach (var obj in monsters)
            {
                var monster = obj.GetComponent<IMonster>();
                if (monster != null && monster.Health > 0)
                {
                    DetectEnemiesForCharacter(obj.gameObject, monster, true);
                }

                var invader = obj.GetComponent<IInvader>();
                if (invader != null && invader.Health > 0)
                {
                    DetectEnemiesForCharacter(obj.gameObject, invader, false);
                }
            }
        }

        /// <summary>
        /// 特定キャラクターの敵検出
        /// </summary>
        private void DetectEnemiesForCharacter(GameObject character, ICharacterBase characterInterface, bool isMonster)
        {
            Vector2 position = character.transform.position;
            LayerMask targetLayer = isMonster ? invaderLayer : monsterLayer;
            string targetTag = isMonster ? "Invader" : "Monster";

            // 範囲内の敵を検索
            Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(position, detectionRadius, targetLayer);

            // 現在の交戦リストを取得または作成
            if (!activeEngagements.ContainsKey(character))
            {
                activeEngagements[character] = new List<GameObject>();
            }

            var currentEngagements = activeEngagements[character];
            var newEngagements = new List<GameObject>();

            foreach (var enemyCollider in nearbyEnemies)
            {
                if (enemyCollider.gameObject == character) continue;
                if (!enemyCollider.CompareTag(targetTag)) continue;

                GameObject enemy = enemyCollider.gameObject;
                newEngagements.Add(enemy);

                // 新しい交戦の場合
                if (!currentEngagements.Contains(enemy))
                {
                    StartCombat(character, enemy, isMonster);
                }
                else
                {
                    // 継続中の戦闘
                    ContinueCombat(character, enemy, isMonster);
                }
            }

            // 範囲外になった敵との戦闘終了
            var toRemove = new List<GameObject>();
            foreach (var enemy in currentEngagements)
            {
                if (enemy == null || !newEngagements.Contains(enemy))
                {
                    EndCombat(character, enemy);
                    toRemove.Add(enemy);
                }
            }

            foreach (var enemy in toRemove)
            {
                currentEngagements.Remove(enemy);
            }

            // 新しい交戦リストを更新
            activeEngagements[character] = newEngagements;
        }

        /// <summary>
        /// 戦闘開始
        /// </summary>
        private void StartCombat(GameObject character, GameObject enemy, bool characterIsMonster)
        {
            // 状態を戦闘中に変更
            if (characterIsMonster)
            {
                var monster = character.GetComponent<IMonster>();
                monster?.SetState(MonsterState.Fighting);
            }
            else
            {
                var invader = character.GetComponent<IInvader>();
                invader?.SetState(InvaderState.Fighting);
            }

            Debug.Log($"戦闘開始: {character.name} vs {enemy.name}");
        }

        /// <summary>
        /// 戦闘継続
        /// </summary>
        private void ContinueCombat(GameObject character, GameObject enemy, bool characterIsMonster)
        {
            if (CombatManager.Instance == null) return;

            // CombatManagerに戦闘処理を委譲
            if (characterIsMonster)
            {
                var monster = character.GetComponent<IMonster>();
                var invader = enemy.GetComponent<IInvader>();
                
                if (monster != null && invader != null)
                {
                    CombatManager.Instance.ProcessCombat(monster, invader);
                }
            }
        }

        /// <summary>
        /// 戦闘終了
        /// </summary>
        private void EndCombat(GameObject character, GameObject enemy)
        {
            if (character == null) return;

            // 他に交戦中の敵がいない場合、状態をIdleに戻す
            if (activeEngagements.ContainsKey(character) && activeEngagements[character].Count <= 1)
            {
                var monster = character.GetComponent<IMonster>();
                if (monster != null)
                {
                    monster.SetState(MonsterState.Idle);
                }

                var invader = character.GetComponent<IInvader>();
                if (invader != null)
                {
                    invader.SetState(InvaderState.Moving);
                }
            }

            if (enemy != null)
            {
                Debug.Log($"戦闘終了: {character.name} vs {enemy.name}");
            }
        }

        /// <summary>
        /// キャラクターの戦闘状況をクリア（死亡時など）
        /// </summary>
        public void ClearCombatEngagements(GameObject character)
        {
            if (activeEngagements.ContainsKey(character))
            {
                activeEngagements.Remove(character);
            }

            // 他のキャラクターの交戦リストからも削除
            foreach (var kvp in activeEngagements)
            {
                kvp.Value.Remove(character);
            }
        }

        /// <summary>
        /// 特定キャラクターが戦闘中かチェック
        /// </summary>
        public bool IsInCombat(GameObject character)
        {
            return activeEngagements.ContainsKey(character) && activeEngagements[character].Count > 0;
        }

        /// <summary>
        /// 戦闘中の敵リストを取得
        /// </summary>
        public List<GameObject> GetCombatEnemies(GameObject character)
        {
            if (activeEngagements.ContainsKey(character))
            {
                return new List<GameObject>(activeEngagements[character]);
            }
            return new List<GameObject>();
        }

        /// <summary>
        /// 設定の動的変更
        /// </summary>
        public void SetDetectionSettings(float radius, float interval)
        {
            detectionRadius = radius;
            detectionInterval = interval;
        }

        private void OnDrawGizmosSelected()
        {
            // 検出範囲の可視化
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}