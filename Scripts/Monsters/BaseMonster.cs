using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Monsters
{
    public abstract class BaseMonster : MonoBehaviour, IMonster, ICharacter, ICharacterBase
    {
        [Header("Monster Configuration")]
        [SerializeField] protected MonsterData monsterData;
        [SerializeField] protected int level = 1;
        
        [Header("Current Stats")]
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float currentMana;
        [SerializeField] protected MonsterState currentState = MonsterState.Idle;
        
        protected IParty currentParty;
        protected float lastAbilityUse;
        protected bool isDead = false;

        // IMonster プロパティ
        public MonsterType Type => monsterData?.type ?? MonsterType.Slime;
        public int Level 
        { 
            get => level; 
            set 
            { 
                level = Mathf.Max(1, value);
                UpdateStatsForLevel();
            } 
        }
        
        public float Health => currentHealth;
        public float MaxHealth => monsterData?.GetHealthAtLevel(level) ?? 100f;
        public float Mana => currentMana;
        public float MaxMana => monsterData?.GetManaAtLevel(level) ?? 50f;
        
        public Vector2 Position 
        { 
            get => transform.position; 
            set => transform.position = new Vector3(value.x, value.y, transform.position.z); 
        }
        
        public MonsterState State => currentState;
        public IParty Party => currentParty;

        // Unity ライフサイクル
        protected virtual void Awake()
        {
            if (monsterData == null)
            {
                Debug.LogError($"MonsterData not assigned for {gameObject.name}");
                return;
            }
            
            InitializeMonster();
        }

        protected virtual void Start()
        {
            UpdateStatsForLevel();
        }

        protected virtual void Update()
        {
            if (isDead) return;
            
            UpdateMonsterBehavior();
            ProcessAbilities();
        }

        // 初期化
        protected virtual void InitializeMonster()
        {
            currentState = MonsterState.Idle;
            isDead = false;
            lastAbilityUse = 0f;
            UpdateStatsForLevel();
        }

        protected virtual void UpdateStatsForLevel()
        {
            if (monsterData == null) return;
            
            float maxHealth = MaxHealth;
            float maxMana = MaxMana;
            
            // 初回設定時は満タンに
            if (currentHealth <= 0)
            {
                currentHealth = maxHealth;
            }
            else
            {
                // レベルアップ時は割合を維持
                float healthRatio = currentHealth / MaxHealth;
                currentHealth = maxHealth * healthRatio;
            }
            
            if (currentMana <= 0)
            {
                currentMana = maxMana;
            }
            else
            {
                float manaRatio = currentMana / MaxMana;
                currentMana = maxMana * manaRatio;
            }
        }

        // IMonster 実装
        public virtual void TakeDamage(float damage)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                OnDamageTaken(damage);
            }
        }

        public virtual void Heal(float amount)
        {
            if (isDead) return;
            
            currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
            OnHealed(amount);
        }

        public virtual void UseAbility()
        {
            if (isDead || monsterData == null) return;
            
            float cooldown = GetAbilityCooldown();
            if (Time.time - lastAbilityUse < cooldown) return;
            
            if (CanUseAbility())
            {
                ExecuteAbility();
                lastAbilityUse = Time.time;
            }
        }

        public virtual void JoinParty(IParty party)
        {
            if (currentParty != null)
            {
                LeaveParty();
            }
            
            currentParty = party;
            party?.AddMember(this);
            OnJoinedParty(party);
        }

        public virtual void LeaveParty()
        {
            if (currentParty != null)
            {
                currentParty.RemoveMember(this);
                OnLeftParty(currentParty);
                currentParty = null;
            }
        }

        public virtual void SetState(MonsterState newState)
        {
            if (currentState != newState)
            {
                MonsterState oldState = currentState;
                currentState = newState;
                OnStateChanged(oldState, newState);
            }
        }

        // 抽象メソッド（サブクラスで実装）
        protected abstract void UpdateMonsterBehavior();
        protected abstract void ExecuteAbility();
        protected abstract bool CanUseAbility();
        protected abstract float GetAbilityCooldown();

        // 仮想メソッド（オーバーライド可能）
        protected virtual void ProcessAbilities()
        {
            // 基本的な回復処理
            if (currentState == MonsterState.Idle || currentState == MonsterState.InShelter)
            {
                ProcessNaturalRecovery();
            }
        }

        protected virtual void ProcessNaturalRecovery()
        {
            float recoveryRate = currentState == MonsterState.InShelter ? 5f : 1f;
            
            if (currentHealth < MaxHealth)
            {
                Heal(recoveryRate * Time.deltaTime);
            }
            
            if (currentMana < MaxMana)
            {
                currentMana = Mathf.Min(MaxMana, currentMana + recoveryRate * Time.deltaTime);
            }
        }

        protected virtual void Die()
        {
            isDead = true;
            SetState(MonsterState.Dead);
            
            // 戦闘状況をクリア
            if (Core.CombatDetector.Instance != null)
            {
                Core.CombatDetector.Instance.ClearCombatEngagements(gameObject);
            }
            
            OnDeath();
        }

        // イベントハンドラー（オーバーライド可能）
        protected virtual void OnDamageTaken(float damage) { }
        protected virtual void OnHealed(float amount) { }
        protected virtual void OnDeath() { }
        protected virtual void OnJoinedParty(IParty party) { }
        protected virtual void OnLeftParty(IParty party) { }
        protected virtual void OnStateChanged(MonsterState oldState, MonsterState newState) { }

        // ユーティリティメソッド
        public MonsterData GetMonsterData()
        {
            return monsterData;
        }

        public void SetMonsterData(MonsterData data)
        {
            monsterData = data;
            if (Application.isPlaying)
            {
                InitializeMonster();
            }
        }

        public float GetAttackPower()
        {
            return monsterData?.GetAttackPowerAtLevel(level) ?? 20f;
        }

        public int GetSellPrice()
        {
            return monsterData?.GetSellPrice() ?? 0;
        }

        public bool IsAlive()
        {
            return !isDead && currentHealth > 0;
        }

        // デバッグ用
        protected virtual void OnDrawGizmosSelected()
        {
            if (monsterData != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, 0.5f);
                
                if (currentParty != null)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireSphere(transform.position, 1f);
                }
            }
        }
    }
}