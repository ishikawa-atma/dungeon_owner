using UnityEngine;
using System.Collections;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core.Abilities
{
    /// <summary>
    /// 自動復活アビリティ（スケルトン・ゴースト用）
    /// 要件4.2, 4.4, 4.5に対応
    /// </summary>
    public class AutoReviveAbility : MonsterAbility
    {
        [Header("Auto Revive Settings")]
        [SerializeField] private float reviveTime = 30f; // 復活時間（秒）
        [SerializeField] private int maxRevives = 3; // 最大復活回数
        [SerializeField] private bool resetLevelOnRevive = true; // 復活時にレベルリセット
        
        private int currentRevives = 0;
        private bool isReviving = false;
        private Coroutine reviveCoroutine;
        private MonoBehaviour ownerMono;

        public AutoReviveAbility()
        {
            abilityType = MonsterAbilityType.AutoRevive;
            abilityName = "自動復活";
            description = "撃破時に一定時間後自動復活（レベルリセット）";
            cooldownTime = 0f; // パッシブアビリティなのでクールダウンなし
            manaCost = 0f; // マナコストなし
        }

        protected override void OnInitialize()
        {
            ownerMono = owner as MonoBehaviour;
            currentRevives = 0;
            isReviving = false;
        }

        protected override bool CanUseCustomCondition()
        {
            // 自動復活は手動実行不可（パッシブアビリティ）
            return false;
        }

        protected override bool ExecuteAbility()
        {
            // 手動実行は不可
            return false;
        }

        protected override void UpdateAbility()
        {
            // 復活処理の監視（実際の復活処理は外部から呼び出される）
        }

        /// <summary>
        /// 復活処理を開始（外部から呼び出される）
        /// </summary>
        /// <returns>復活処理が開始されたかどうか</returns>
        public bool TryStartRevive()
        {
            if (currentRevives >= maxRevives || isReviving || ownerMono == null)
            {
                return false;
            }

            StartReviveProcess();
            return true;
        }

        private void StartReviveProcess()
        {
            isReviving = true;
            owner.SetState(MonsterState.Dead);
            
            ShowRevivingState();
            
            if (ownerMono != null)
            {
                reviveCoroutine = ownerMono.StartCoroutine(ReviveCoroutine());
            }
        }

        private IEnumerator ReviveCoroutine()
        {
            yield return new WaitForSeconds(reviveTime);
            
            if (isReviving && ownerMono != null)
            {
                ExecuteRevive();
            }
        }

        private void ExecuteRevive()
        {
            currentRevives++;
            isReviving = false;
            
            // レベルリセット
            if (resetLevelOnRevive)
            {
                owner.Level = 1;
            }
            
            // HP/MPを満タンで復活
            owner.Heal(owner.MaxHealth); // 満タンまで回復
            
            if (owner is Monsters.BaseMonster baseMonster)
            {
                // リフレクションを使用してマナと死亡状態を回復（一時的な解決策）
                var manaField = typeof(Monsters.BaseMonster).GetField("currentMana", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var isDeadField = typeof(Monsters.BaseMonster).GetField("isDead", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (manaField != null && isDeadField != null)
                {
                    manaField.SetValue(baseMonster, owner.MaxMana);
                    isDeadField.SetValue(baseMonster, false);
                }
            }
            
            owner.SetState(MonsterState.Idle);
            ShowReviveEffect();
            
            Debug.Log($"{owner.GetType().Name} revived! ({currentRevives}/{maxRevives} revives used)");
        }

        /// <summary>
        /// 復活をキャンセル（売却時など）
        /// </summary>
        public void CancelRevive()
        {
            if (isReviving && reviveCoroutine != null && ownerMono != null)
            {
                ownerMono.StopCoroutine(reviveCoroutine);
                isReviving = false;
            }
        }

        protected override void ShowVisualEffect()
        {
            ShowReviveEffect();
        }

        private void ShowRevivingState()
        {
            // TODO: 復活準備中の視覚エフェクト（骨の山、霊体など）
            Debug.Log($"{owner.GetType().Name} is preparing to revive... ({reviveTime}s remaining)");
        }

        private void ShowReviveEffect()
        {
            // TODO: 復活時の視覚エフェクト
            Debug.Log($"{owner.GetType().Name} has revived with full health!");
        }

        protected override void OnReset()
        {
            CancelRevive();
            currentRevives = 0;
        }

        // 公開プロパティ
        public bool IsReviving => isReviving;
        public int RemainingRevives => maxRevives - currentRevives;
        public int CurrentRevives => currentRevives;
        public float ReviveTime => reviveTime;
        public int MaxRevives => maxRevives;

        // 設定変更メソッド（デザイナー用）
        public void SetReviveTime(float time)
        {
            reviveTime = Mathf.Max(1f, time);
        }

        public void SetMaxRevives(int count)
        {
            maxRevives = Mathf.Max(0, count);
        }
    }
}