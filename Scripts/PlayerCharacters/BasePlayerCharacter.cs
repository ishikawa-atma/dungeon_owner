using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.PlayerCharacters
{
    public abstract class BasePlayerCharacter : MonoBehaviour, ICharacter
    {
        [Header("Player Character Configuration")]
        [SerializeField] protected PlayerCharacterData characterData;
        [SerializeField] protected int level = 1;
        
        [Header("Current Stats")]
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float currentMana;
        [SerializeField] protected MonsterState currentState = MonsterState.Idle;
        
        protected IParty currentParty;
        protected float lastAbilityUse;
        protected bool isDead = false;
        protected bool isInShelter = false;

        // ICharacter プロパティ
        public PlayerCharacterType Type => characterData?.type ?? PlayerCharacterType.Warrior;
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
        public float MaxHealth => characterData?.GetHealthAtLevel(level) ?? 100f;
        public float Mana => currentMana;
        public float MaxMana => characterData?.GetManaAtLevel(level) ?? 50f;
        
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
            if (characterData == null)
            {
                Debug.LogError($"PlayerCharacterData not assigned for {gameObject.name}");
                return;
            }
            
            InitializeCharacter();
        }

        protected virtual void Start()
        {
            UpdateStatsForLevel();
        }

        protected virtual void Update()
        {
            if (isDead) return;
            
            UpdateCharacterBehavior();
            ProcessAbilities();
        }

        // 初期化
        protected virtual void InitializeCharacter()
        {
            currentState = MonsterState.Idle;
            isDead = false;
            isInShelter = false;
            lastAbilityUse = 0f;
            UpdateStatsForLevel();
        }

        protected virtual void UpdateStatsForLevel()
        {
            if (characterData == null) return;
            
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

        // ICharacter 実装
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
            if (isDead || characterData == null) return;
            
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

        // プレイヤーキャラクター特有のメソッド
        public virtual void Revive()
        {
            if (!isDead) return;
            
            isDead = false;
            isInShelter = true; // 退避スポットで蘇生
            currentHealth = MaxHealth;
            currentMana = MaxMana;
            SetState(MonsterState.InShelter);
            
            OnRevived();
            Debug.Log($"Player character {gameObject.name} revived in shelter!");
        }

        public virtual void MoveToShelter()
        {
            if (isDead) return;
            
            isInShelter = true;
            SetState(MonsterState.InShelter);
            OnMovedToShelter();
        }

        public virtual void DeployFromShelter(Vector2 position)
        {
            if (!isInShelter) return;
            
            isInShelter = false;
            Position = position;
            SetState(MonsterState.Idle);
            OnDeployedFromShelter();
        }

        // 抽象メソッド（サブクラスで実装）
        protected abstract void UpdateCharacterBehavior();
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
            float recoveryRate = isInShelter ? 10f : 2f; // 退避スポットでは高速回復
            
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
            OnDeath();
            
            // プレイヤーキャラクターは一定時間後に自動蘇生
            Invoke(nameof(AutoRevive), 10f);
        }

        private void AutoRevive()
        {
            if (isDead)
            {
                Revive();
            }
        }

        // イベントハンドラー（オーバーライド可能）
        protected virtual void OnDamageTaken(float damage) { }
        protected virtual void OnHealed(float amount) { }
        protected virtual void OnDeath() { }
        protected virtual void OnRevived() { }
        protected virtual void OnMovedToShelter() { }
        protected virtual void OnDeployedFromShelter() { }
        protected virtual void OnJoinedParty(IParty party) { }
        protected virtual void OnLeftParty(IParty party) { }
        protected virtual void OnStateChanged(MonsterState oldState, MonsterState newState) { }

        // ユーティリティメソッド
        public PlayerCharacterData GetCharacterData()
        {
            return characterData;
        }

        public void SetCharacterData(PlayerCharacterData data)
        {
            characterData = data;
            if (Application.isPlaying)
            {
                InitializeCharacter();
            }
        }

        public float GetAttackPower()
        {
            return characterData?.GetAttackPowerAtLevel(level) ?? 25f;
        }

        public bool IsAlive()
        {
            return !isDead && currentHealth > 0;
        }

        public bool IsInShelter()
        {
            return isInShelter;
        }

        // デバッグ用
        protected virtual void OnDrawGizmosSelected()
        {
            if (characterData != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, 0.7f);
                
                if (isInShelter)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(transform.position, 1.2f);
                }
            }
        }
    }
}