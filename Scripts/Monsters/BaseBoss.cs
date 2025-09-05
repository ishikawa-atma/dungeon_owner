using UnityEngine;
using System.Collections;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Core;

namespace DungeonOwner.Monsters
{
    public class BaseBoss : BaseMonster, IBoss
    {
        [Header("Boss Settings")]
        [SerializeField] private BossData bossData;
        [SerializeField] private float respawnTime = 300f;
        [SerializeField] private bool isRespawning = false;
        [SerializeField] private float respawnProgress = 0f;
        [SerializeField] private int defeatedLevel = 1;

        // IBoss実装
        public BossType BossType => bossData?.type ?? BossType.DragonLord;
        public float RespawnTime => respawnTime;
        public bool IsRespawning => isRespawning;
        public float RespawnProgress => respawnProgress;
        public int DefeatedLevel => defeatedLevel;

        // イベント
        public System.Action<IBoss> OnBossDefeated;
        public System.Action<IBoss> OnBossRespawned;
        public System.Action<IBoss, float> OnRespawnProgress;

        protected override void Start()
        {
            base.Start();
            
            if (bossData != null)
            {
                SetBossData(bossData);
            }
        }

        public void SetBossData(BossData data)
        {
            if (data == null) return;

            bossData = data;
            respawnTime = data.respawnTime;

            // 基本ステータスを設定
            maxHealth = data.GetHealthAtLevel(level);
            currentHealth = maxHealth;
            maxMana = data.GetManaAtLevel(level);
            currentMana = maxMana;
            attackPower = data.GetAttackPowerAtLevel(level);

            // アビリティを設定
            foreach (var abilityType in data.abilities)
            {
                AddAbility(abilityType);
            }

            Debug.Log($"Boss {data.type} initialized with level {level}");
        }

        public override void TakeDamage(float damage)
        {
            if (isRespawning) return;

            base.TakeDamage(damage);

            // HPが0になった場合の処理
            if (currentHealth <= 0 && state != MonsterState.Dead)
            {
                OnBossDefeated?.Invoke(this);
                defeatedLevel = level; // 撃破時のレベルを記録
                StartRespawn();
            }
        }

        public void StartRespawn()
        {
            if (isRespawning) return;

            isRespawning = true;
            respawnProgress = 0f;
            SetState(MonsterState.Dead);

            // ボスオブジェクトを非表示
            gameObject.SetActive(false);

            // リポップコルーチンを開始
            StartCoroutine(RespawnCoroutine());

            Debug.Log($"Boss {BossType} started respawn process (Level {defeatedLevel})");
        }

        private IEnumerator RespawnCoroutine()
        {
            float elapsed = 0f;

            while (elapsed < respawnTime)
            {
                elapsed += Time.deltaTime;
                respawnProgress = elapsed / respawnTime;
                
                OnRespawnProgress?.Invoke(this, respawnProgress);
                
                yield return null;
            }

            CompleteRespawn();
        }

        public void CompleteRespawn()
        {
            if (!isRespawning) return;

            isRespawning = false;
            respawnProgress = 1f;

            // レベル引き継ぎ（要件6.5）
            if (bossData != null && bossData.maintainLevelOnRespawn)
            {
                level = defeatedLevel;
            }

            // ステータスを復元
            if (bossData != null)
            {
                maxHealth = bossData.GetHealthAtLevel(level);
                currentHealth = maxHealth;
                maxMana = bossData.GetManaAtLevel(level);
                currentMana = maxMana;
                attackPower = bossData.GetAttackPowerAtLevel(level);
            }

            SetState(MonsterState.Idle);

            // ボスオブジェクトを再表示
            gameObject.SetActive(true);

            OnBossRespawned?.Invoke(this);

            Debug.Log($"Boss {BossType} respawned at level {level}");
        }

        public void SetRespawnTime(float time)
        {
            respawnTime = Mathf.Max(0f, time);
        }

        public bool CanRespawn()
        {
            return !isRespawning && state == MonsterState.Dead;
        }

        // ボス特有のアビリティ処理
        protected override void UpdateAbilities()
        {
            base.UpdateAbilities();

            // ボス専用のアビリティ更新処理
            if (bossData != null && !isRespawning)
            {
                // 複数アビリティの同時使用など
                foreach (var abilityType in bossData.abilities)
                {
                    if (HasAbility(abilityType) && CanUseAbility(abilityType))
                    {
                        UseAbility(abilityType);
                    }
                }
            }
        }

        private bool CanUseAbility(MonsterAbilityType abilityType)
        {
            // ボス専用のアビリティ使用条件
            switch (abilityType)
            {
                case MonsterAbilityType.AutoHeal:
                    return currentHealth < maxHealth * 0.5f; // HP50%以下で発動
                case MonsterAbilityType.AreaAttack:
                    return true; // 常時使用可能
                default:
                    return true;
            }
        }

        // デバッグ用メソッド
        public void DebugForceRespawn()
        {
            if (isRespawning)
            {
                StopAllCoroutines();
                CompleteRespawn();
            }
            else
            {
                StartRespawn();
            }
        }

        public void DebugPrintBossInfo()
        {
            Debug.Log($"=== Boss Info: {BossType} ===");
            Debug.Log($"Level: {level}");
            Debug.Log($"Health: {currentHealth}/{maxHealth}");
            Debug.Log($"Mana: {currentMana}/{maxMana}");
            Debug.Log($"Attack Power: {attackPower}");
            Debug.Log($"Is Respawning: {isRespawning}");
            Debug.Log($"Respawn Progress: {respawnProgress:P}");
            Debug.Log($"Defeated Level: {defeatedLevel}");
        }

        private void OnDestroy()
        {
            // コルーチンを停止
            StopAllCoroutines();
        }
    }
}