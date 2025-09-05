using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Monsters
{
    public class LesserGolem : BaseMonster
    {
        [Header("Golem Specific")]
        [SerializeField] private float defenseMultiplier = 0.5f; // ダメージ軽減率
        [SerializeField] private float tauntRange = 3f; // 挑発範囲
        [SerializeField] private float moveSpeed = 0.5f; // 移動速度（遅い）
        
        private bool isTaunting = false;

        protected override void UpdateMonsterBehavior()
        {
            // ゴーレムは重厚で遅いが、高い防御力を持つ
            if (currentState == MonsterState.Idle && IsAlive())
            {
                // 近くの敵を挑発する
                ProcessTaunt();
            }
        }

        protected override void ExecuteAbility()
        {
            // ゴーレムのアビリティ：石の盾（防御力大幅アップ）
            if (currentMana >= 25f)
            {
                currentMana -= 25f;
                StartStoneShield();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentMana >= 25f && !isTaunting;
        }

        protected override float GetAbilityCooldown()
        {
            return 20f; // 20秒クールダウン
        }

        public override void TakeDamage(float damage)
        {
            // ゴーレムは常に物理ダメージを軽減
            float reducedDamage = damage * defenseMultiplier;
            base.TakeDamage(reducedDamage);
            
            ShowDefenseEffect();
        }

        private void ProcessTaunt()
        {
            // 挑発範囲内の敵を引き寄せる効果（後で実装）
            // 現在は基本的な防御姿勢のみ
        }

        private void StartStoneShield()
        {
            // 石の盾効果：一定時間ダメージをさらに軽減
            isTaunting = true;
            ShowStoneShieldEffect();
            
            // 5秒間効果持続
            Invoke(nameof(EndStoneShield), 5f);
        }

        private void EndStoneShield()
        {
            isTaunting = false;
            ShowStoneShieldEndEffect();
        }

        private void ShowDefenseEffect()
        {
            // TODO: 防御エフェクト（石の破片など）
            Debug.Log($"Golem {gameObject.name} defended with stone armor!");
        }

        private void ShowStoneShieldEffect()
        {
            // TODO: 石の盾エフェクト
            Debug.Log($"Golem {gameObject.name} activated stone shield!");
        }

        private void ShowStoneShieldEndEffect()
        {
            // TODO: 石の盾終了エフェクト
            Debug.Log($"Golem {gameObject.name} stone shield ended!");
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // 挑発範囲を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, tauntRange);
        }

        public bool IsTaunting()
        {
            return isTaunting;
        }

        public float GetDefenseMultiplier()
        {
            return isTaunting ? defenseMultiplier * 0.5f : defenseMultiplier;
        }
    }
}