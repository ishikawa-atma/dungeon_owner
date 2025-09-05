using UnityEngine;
using System.Collections;
using DungeonOwner.Data;

namespace DungeonOwner.Monsters
{
    public class LesserSkeleton : BaseMonster
    {
        [Header("Skeleton Specific")]
        [SerializeField] private float reviveTime = 30f; // 復活時間（秒）
        [SerializeField] private int maxRevives = 3; // 最大復活回数
        
        private int currentRevives = 0;
        private bool isReviving = false;
        private Coroutine reviveCoroutine;

        protected override void UpdateMonsterBehavior()
        {
            // スケルトンは基本的な戦闘行動
            if (currentState == MonsterState.Idle && IsAlive())
            {
                // 敵を探索する行動など（後で実装）
            }
        }

        protected override void ExecuteAbility()
        {
            // スケルトンのアビリティ：骨の強化（防御力アップ）
            if (currentMana >= 15f)
            {
                currentMana -= 15f;
                // 一時的な防御力アップ効果（後で実装）
                ShowBoneStrengthEffect();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentMana >= 15f && !isReviving;
        }

        protected override float GetAbilityCooldown()
        {
            return 10f; // 10秒クールダウン
        }

        protected override void Die()
        {
            if (currentRevives < maxRevives && !isReviving)
            {
                // 復活処理を開始
                StartReviveProcess();
            }
            else
            {
                // 完全に死亡
                base.Die();
            }
        }

        private void StartReviveProcess()
        {
            isReviving = true;
            SetState(MonsterState.Dead);
            
            // 見た目を変更（骨の山など）
            ShowRevivingState();
            
            reviveCoroutine = StartCoroutine(ReviveCoroutine());
        }

        private IEnumerator ReviveCoroutine()
        {
            yield return new WaitForSeconds(reviveTime);
            
            if (isReviving) // まだ復活処理中の場合
            {
                Revive();
            }
        }

        private void Revive()
        {
            currentRevives++;
            isReviving = false;
            isDead = false;
            
            // レベルをリセット
            level = 1;
            UpdateStatsForLevel();
            
            // HPを満タンで復活
            currentHealth = MaxHealth;
            currentMana = MaxMana;
            
            SetState(MonsterState.Idle);
            ShowReviveEffect();
            
            Debug.Log($"Skeleton {gameObject.name} revived! ({currentRevives}/{maxRevives})");
        }

        private void ShowBoneStrengthEffect()
        {
            // TODO: 骨強化エフェクト
            Debug.Log($"Skeleton {gameObject.name} used bone strength!");
        }

        private void ShowRevivingState()
        {
            // TODO: 骨の山の見た目に変更
            Debug.Log($"Skeleton {gameObject.name} is preparing to revive...");
        }

        private void ShowReviveEffect()
        {
            // TODO: 復活エフェクト
            Debug.Log($"Skeleton {gameObject.name} has revived!");
        }

        protected override void OnDestroy()
        {
            if (reviveCoroutine != null)
            {
                StopCoroutine(reviveCoroutine);
            }
        }

        // 復活をキャンセル（売却時など）
        public void CancelRevive()
        {
            if (isReviving && reviveCoroutine != null)
            {
                StopCoroutine(reviveCoroutine);
                isReviving = false;
            }
        }

        public bool IsReviving()
        {
            return isReviving;
        }

        public int GetRemainingRevives()
        {
            return maxRevives - currentRevives;
        }
    }
}