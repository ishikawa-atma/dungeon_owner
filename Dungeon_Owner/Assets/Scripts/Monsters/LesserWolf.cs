using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Monsters
{
    public class LesserWolf : BaseMonster
    {
        [Header("Wolf Specific")]
        [SerializeField] private float moveSpeedMultiplier = 2f; // 移動速度倍率
        [SerializeField] private float packBonusRange = 2f; // パック効果範囲
        [SerializeField] private float packBonusMultiplier = 1.2f; // パック効果倍率
        
        private bool isHowling = false;
        private float lastHowlTime;

        protected override void UpdateMonsterBehavior()
        {
            // ウルフは素早く、群れで行動する
            if (currentState == MonsterState.Idle && IsAlive())
            {
                ProcessPackBehavior();
            }
        }

        protected override void InitializeAbilities()
        {
            base.InitializeAbilities();
            // 将来的にウルフ専用アビリティを追加
        }

        private void ProcessPackBehavior()
        {
            // 近くの他のウルフとの連携行動
            CheckForPackMembers();
        }

        private void CheckForPackMembers()
        {
            // 範囲内の他のウルフを探す
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, packBonusRange);
            
            int packSize = 1; // 自分を含む
            foreach (var collider in nearbyColliders)
            {
                if (collider != this.GetComponent<Collider2D>())
                {
                    LesserWolf otherWolf = collider.GetComponent<LesserWolf>();
                    if (otherWolf != null && otherWolf.IsAlive())
                    {
                        packSize++;
                    }
                }
            }
            
            // パックサイズに応じてボーナス効果
            if (packSize >= 2)
            {
                ApplyPackBonus(packSize);
            }
        }

        private void ApplyPackBonus(int packSize)
        {
            // パックボーナス：攻撃力と移動速度アップ
            float bonus = 1f + (packSize - 1) * 0.1f; // パックメンバー1体につき10%アップ
            // この効果は戦闘システムで参照される
        }

        private void StartHowl()
        {
            isHowling = true;
            lastHowlTime = Time.time;
            ShowHowlEffect();
            
            // 周囲の味方モンスターを強化
            BuffNearbyAllies();
            
            // 3秒間遠吠え
            Invoke(nameof(EndHowl), 3f);
        }

        private void EndHowl()
        {
            isHowling = false;
            ShowHowlEndEffect();
        }

        private void BuffNearbyAllies()
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, packBonusRange * 1.5f);
            
            foreach (var collider in nearbyColliders)
            {
                BaseMonster monster = collider.GetComponent<BaseMonster>();
                if (monster != null && monster != this && monster.IsAlive())
                {
                    // 一時的な強化効果を付与（後で実装）
                    ApplyTemporaryBuff(monster);
                }
            }
        }

        private void ApplyTemporaryBuff(BaseMonster target)
        {
            // TODO: 一時的なバフ効果システム
            Debug.Log($"Wolf {gameObject.name} buffed {target.gameObject.name}!");
        }

        public float GetCurrentMoveSpeed()
        {
            float baseSpeed = monsterData?.moveSpeed ?? 2f;
            return baseSpeed * moveSpeedMultiplier;
        }

        public float GetPackBonus()
        {
            // 現在のパックボーナスを計算
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, packBonusRange);
            int packSize = 1;
            
            foreach (var collider in nearbyColliders)
            {
                if (collider != this.GetComponent<Collider2D>())
                {
                    LesserWolf otherWolf = collider.GetComponent<LesserWolf>();
                    if (otherWolf != null && otherWolf.IsAlive())
                    {
                        packSize++;
                    }
                }
            }
            
            return packSize >= 2 ? 1f + (packSize - 1) * 0.1f : 1f;
        }

        private void ShowHowlEffect()
        {
            // TODO: 遠吠えエフェクト（音波など）
            Debug.Log($"Wolf {gameObject.name} howled to rally the pack!");
        }

        private void ShowHowlEndEffect()
        {
            // TODO: 遠吠え終了エフェクト
            Debug.Log($"Wolf {gameObject.name} finished howling!");
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();
            
            // パック効果範囲を表示
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, packBonusRange);
            
            // バフ範囲を表示
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, packBonusRange * 1.5f);
        }

        public bool IsHowling()
        {
            return isHowling;
        }

        public int GetPackSize()
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, packBonusRange);
            int packSize = 1;
            
            foreach (var collider in nearbyColliders)
            {
                if (collider != this.GetComponent<Collider2D>())
                {
                    LesserWolf otherWolf = collider.GetComponent<LesserWolf>();
                    if (otherWolf != null && otherWolf.IsAlive())
                    {
                        packSize++;
                    }
                }
            }
            
            return packSize;
        }
    }
}