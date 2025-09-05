using UnityEngine;
using System.Collections;
using DungeonOwner.Data;
using DungeonOwner.Core.Abilities;

namespace DungeonOwner.Monsters
{
    public class LesserGhost : BaseMonster
    {
        [Header("Ghost Specific")]
        [SerializeField] private float reviveTime = 25f; // 復活時間（スケルトンより短い）
        [SerializeField] private int maxRevives = 2; // 最大復活回数（スケルトンより少ない）
        [SerializeField] private float phaseAbilityDuration = 5f; // フェーズ能力持続時間
        
        private int currentRevives = 0;
        private bool isReviving = false;
        private bool isPhased = false; // フェーズ状態（物理攻撃無効）
        private Coroutine reviveCoroutine;
        private Coroutine phaseCoroutine;
        private AutoReviveAbility autoReviveAbility;

        protected override void InitializeAbilities()
        {
            base.InitializeAbilities();
            
            // 自動復活アビリティを追加（ゴースト用設定）
            autoReviveAbility = new AutoReviveAbility();
            autoReviveAbility.SetReviveTime(reviveTime);
            autoReviveAbility.SetMaxRevives(maxRevives);
            AddAbility(autoReviveAbility);
        }

        protected override void UpdateMonsterBehavior()
        {
            // ゴーストは浮遊しながら移動
            if (currentState == MonsterState.Idle && IsAlive())
            {
                // 浮遊アニメーション（後で実装）
                ProcessFloating();
            }
        }

        protected override void ExecuteAbility()
        {
            // ゴーストのアビリティ：フェーズ（物理攻撃無効化）
            if (currentMana >= 20f && !isPhased)
            {
                currentMana -= 20f;
                StartPhaseAbility();
            }
        }

        protected override bool CanUseAbility()
        {
            return currentMana >= 20f && !isPhased && !isReviving;
        }

        protected override float GetAbilityCooldown()
        {
            return 15f; // 15秒クールダウン
        }

        public override void TakeDamage(float damage)
        {
            // フェーズ中は物理ダメージを軽減
            if (isPhased)
            {
                damage *= 0.3f; // 70%ダメージ軽減
                ShowPhaseDefenseEffect();
            }
            
            base.TakeDamage(damage);
        }

        protected override void Die()
        {
            // 新しいアビリティシステムを使用して復活を試行
            if (autoReviveAbility != null && autoReviveAbility.TryStartRevive())
            {
                // 復活処理が開始された
                isReviving = true;
                currentRevives = autoReviveAbility.CurrentRevives;
                
                // フェーズ状態を解除
                if (isPhased)
                {
                    EndPhaseAbility();
                }
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
            
            // フェーズ状態を解除
            if (isPhased)
            {
                EndPhaseAbility();
            }
            
            ShowRevivingState();
            reviveCoroutine = StartCoroutine(ReviveCoroutine());
        }

        private IEnumerator ReviveCoroutine()
        {
            yield return new WaitForSeconds(reviveTime);
            
            if (isReviving)
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
            
            Debug.Log($"Ghost {gameObject.name} revived! ({currentRevives}/{maxRevives})");
        }

        private void StartPhaseAbility()
        {
            isPhased = true;
            ShowPhaseStartEffect();
            
            if (phaseCoroutine != null)
            {
                StopCoroutine(phaseCoroutine);
            }
            phaseCoroutine = StartCoroutine(PhaseCoroutine());
        }

        private IEnumerator PhaseCoroutine()
        {
            yield return new WaitForSeconds(phaseAbilityDuration);
            EndPhaseAbility();
        }

        private void EndPhaseAbility()
        {
            isPhased = false;
            ShowPhaseEndEffect();
        }

        private void ProcessFloating()
        {
            // ゴーストの浮遊効果（上下に微妙に動く）
            float floatY = Mathf.Sin(Time.time * 2f) * 0.1f;
            Vector3 pos = transform.position;
            pos.y += floatY * Time.deltaTime;
            transform.position = pos;
        }

        private void ShowPhaseStartEffect()
        {
            // TODO: フェーズ開始エフェクト（半透明化など）
            Debug.Log($"Ghost {gameObject.name} entered phase mode!");
        }

        private void ShowPhaseEndEffect()
        {
            // TODO: フェーズ終了エフェクト
            Debug.Log($"Ghost {gameObject.name} exited phase mode!");
        }

        private void ShowPhaseDefenseEffect()
        {
            // TODO: フェーズ防御エフェクト
            Debug.Log($"Ghost {gameObject.name} phased through attack!");
        }

        private void ShowRevivingState()
        {
            // TODO: 霊体の見た目に変更
            Debug.Log($"Ghost {gameObject.name} is preparing to revive...");
        }

        private void ShowReviveEffect()
        {
            // TODO: 復活エフェクト
            Debug.Log($"Ghost {gameObject.name} has revived!");
        }

        protected override void OnDestroy()
        {
            if (reviveCoroutine != null)
            {
                StopCoroutine(reviveCoroutine);
            }
            if (phaseCoroutine != null)
            {
                StopCoroutine(phaseCoroutine);
            }
        }

        public void CancelRevive()
        {
            if (autoReviveAbility != null)
            {
                autoReviveAbility.CancelRevive();
                isReviving = false;
            }
        }

        public bool IsReviving()
        {
            return autoReviveAbility?.IsReviving ?? false;
        }

        public bool IsPhased()
        {
            return isPhased;
        }

        public int GetRemainingRevives()
        {
            return autoReviveAbility?.RemainingRevives ?? 0;
        }
    }
}