using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Core;

namespace DungeonOwner.Monsters
{
    public abstract class BaseMonster : MonoBehaviour, IMonster, ICharacter, ICharacterBase, ITimeScalable
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
        
        // アビリティシステム
        protected List<IMonsterAbility> abilities = new List<IMonsterAbility>();
        protected Dictionary<MonsterAbilityType, IMonsterAbility> abilityMap = new Dictionary<MonsterAbilityType, IMonsterAbility>();

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
            
            // レベル表示システムに登録
            RegisterLevelDisplay();
            
            // 回復システムに登録
            RegisterRecoverySystem();
            
            // 時間制御システムに登録
            RegisterTimeScalable();
        }

        protected virtual void Update()
        {
            if (isDead) return;
            
            // 通常のUpdate処理は時間制御システムを使用しない場合のフォールバック
            if (TimeManager.Instance == null || !TimeManager.Instance.IsSpeedControlEnabled)
            {
                UpdateMonsterBehavior();
                ProcessAbilities();
            }
        }

        // 初期化
        protected virtual void InitializeMonster()
        {
            currentState = MonsterState.Idle;
            isDead = false;
            lastAbilityUse = 0f;
            UpdateStatsForLevel();
            InitializeAbilities();
        }
        
        protected virtual void InitializeAbilities()
        {
            // サブクラスでオーバーライドしてアビリティを追加
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

        /// <summary>
        /// マナを回復する
        /// 退避スポットシステム用
        /// </summary>
        public virtual void RestoreMana(float amount)
        {
            if (isDead) return;
            
            currentMana = Mathf.Min(MaxMana, currentMana + amount);
            OnManaRestored(amount);
        }

        public virtual void UseAbility()
        {
            if (isDead || monsterData == null) return;
            
            // 新しいアビリティシステムを使用
            UseAbility(MonsterAbilityType.AutoHeal); // デフォルトアビリティ
        }
        
        public virtual bool UseAbility(MonsterAbilityType abilityType)
        {
            if (isDead) return false;
            
            if (abilityMap.TryGetValue(abilityType, out IMonsterAbility ability))
            {
                return ability.Execute();
            }
            
            return false;
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
        
        // 仮想メソッド（レガシー対応、オーバーライド可能）
        protected virtual void ExecuteAbility()
        {
            // デフォルト実装：最初のアビリティを使用
            if (abilities.Count > 0)
            {
                abilities[0].Execute();
            }
        }
        
        protected virtual bool CanUseAbility()
        {
            // デフォルト実装：最初のアビリティが使用可能かチェック
            return abilities.Count > 0 && abilities[0].CanUse;
        }
        
        protected virtual float GetAbilityCooldown()
        {
            // デフォルト実装：最初のアビリティのクールダウンを返す
            return abilities.Count > 0 ? abilities[0].CooldownTime : 3f;
        }

        // 仮想メソッド（オーバーライド可能）
        protected virtual void ProcessAbilities()
        {
            // 全アビリティの更新処理
            foreach (var ability in abilities)
            {
                ability.Update();
            }
            
            // 基本的な回復処理
            if (currentState == MonsterState.Idle || currentState == MonsterState.InShelter)
            {
                ProcessNaturalRecovery();
            }
        }

        protected virtual void ProcessNaturalRecovery()
        {
            // 新しい回復システムを使用する場合はスキップ
            if (Managers.RecoveryManager.Instance != null)
            {
                return;
            }
            
            // フォールバック処理（旧システム）
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
            
            // レベル表示システムから削除
            UnregisterLevelDisplay();
            
            // 回復システムから削除
            UnregisterRecoverySystem();
            
            // 時間制御システムから削除
            UnregisterTimeScalable();
            
            OnDeath();
        }

        // イベントハンドラー（オーバーライド可能）
        protected virtual void OnDamageTaken(float damage) { }
        protected virtual void OnHealed(float amount) { }
        protected virtual void OnManaRestored(float amount) { }
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
        
        // アビリティ管理メソッド
        protected void AddAbility(IMonsterAbility ability)
        {
            if (ability == null) return;
            
            ability.Initialize(this);
            abilities.Add(ability);
            abilityMap[ability.AbilityType] = ability;
        }
        
        protected void AddAbility(MonsterAbilityType abilityType)
        {
            // アビリティタイプに基づいてアビリティインスタンスを作成
            IMonsterAbility ability = CreateAbilityInstance(abilityType);
            if (ability != null)
            {
                AddAbility(ability);
            }
        }
        
        private IMonsterAbility CreateAbilityInstance(MonsterAbilityType abilityType)
        {
            // 各アビリティタイプに対応するインスタンスを作成
            // 実際の実装では、アビリティファクトリーパターンを使用することを推奨
            switch (abilityType)
            {
                case MonsterAbilityType.AutoHeal:
                    return gameObject.AddComponent<AutoHealAbility>();
                case MonsterAbilityType.AutoRevive:
                    return gameObject.AddComponent<AutoReviveAbility>();
                // 他のアビリティタイプも必要に応じて追加
                default:
                    Debug.LogWarning($"Ability type {abilityType} not implemented");
                    return null;
            }
        }
        
        protected void RemoveAbility(MonsterAbilityType abilityType)
        {
            if (abilityMap.TryGetValue(abilityType, out IMonsterAbility ability))
            {
                abilities.Remove(ability);
                abilityMap.Remove(abilityType);
            }
        }
        
        public IMonsterAbility GetAbility(MonsterAbilityType abilityType)
        {
            abilityMap.TryGetValue(abilityType, out IMonsterAbility ability);
            return ability;
        }
        
        public List<IMonsterAbility> GetAllAbilities()
        {
            return new List<IMonsterAbility>(abilities);
        }
        
        public bool HasAbility(MonsterAbilityType abilityType)
        {
            return abilityMap.ContainsKey(abilityType);
        }
        
        // マナ消費メソッド（アビリティシステム用）
        public virtual void ConsumeMana(float amount)
        {
            currentMana = Mathf.Max(0, currentMana - amount);
        }

        // レベル表示システム連携
        protected virtual void RegisterLevelDisplay()
        {
            if (Managers.LevelDisplayManager.Instance != null)
            {
                Managers.LevelDisplayManager.Instance.AddLevelDisplay(gameObject, this);
            }
        }

        protected virtual void UnregisterLevelDisplay()
        {
            if (Managers.LevelDisplayManager.Instance != null)
            {
                Managers.LevelDisplayManager.Instance.RemoveLevelDisplay(gameObject);
            }
        }
        
        // 回復システム連携
        protected virtual void RegisterRecoverySystem()
        {
            if (Managers.RecoveryManager.Instance != null)
            {
                Managers.RecoveryManager.Instance.RegisterMonster(this);
            }
        }

        protected virtual void UnregisterRecoverySystem()
        {
            if (Managers.RecoveryManager.Instance != null)
            {
                Managers.RecoveryManager.Instance.UnregisterMonster(this);
            }
        }

        // ITimeScalable 実装
        public virtual void UpdateWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            if (isDead) return;
            
            // 時間スケールを考慮した更新処理
            UpdateMonsterBehaviorWithTimeScale(scaledDeltaTime, timeScale);
            ProcessAbilitiesWithTimeScale(scaledDeltaTime, timeScale);
        }

        public virtual void OnTimeScaleChanged(float newTimeScale)
        {
            // 時間スケール変更時の処理
            // アニメーションスピードの調整など
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = newTimeScale;
            }
        }

        // 時間スケール対応の更新メソッド
        protected virtual void UpdateMonsterBehaviorWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            // デフォルト実装：通常の更新処理を呼び出し
            UpdateMonsterBehavior();
        }

        protected virtual void ProcessAbilitiesWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            // 全アビリティの時間スケール対応更新処理
            foreach (var ability in abilities)
            {
                if (ability is ITimeScalable timeScalableAbility)
                {
                    timeScalableAbility.UpdateWithTimeScale(scaledDeltaTime, timeScale);
                }
                else
                {
                    ability.Update();
                }
            }
            
            // 基本的な回復処理（時間スケール対応）
            if (currentState == MonsterState.Idle || currentState == MonsterState.InShelter)
            {
                ProcessNaturalRecoveryWithTimeScale(scaledDeltaTime, timeScale);
            }
        }

        protected virtual void ProcessNaturalRecoveryWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            // 新しい回復システムを使用する場合はスキップ
            if (Managers.RecoveryManager.Instance != null)
            {
                return;
            }
            
            // フォールバック処理（旧システム、時間スケール対応）
            float recoveryRate = currentState == MonsterState.InShelter ? 5f : 1f;
            
            if (currentHealth < MaxHealth)
            {
                Heal(recoveryRate * scaledDeltaTime);
            }
            
            if (currentMana < MaxMana)
            {
                currentMana = Mathf.Min(MaxMana, currentMana + recoveryRate * scaledDeltaTime);
            }
        }

        // 時間制御システム連携
        protected virtual void RegisterTimeScalable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.RegisterTimeScalable(this);
            }
        }

        protected virtual void UnregisterTimeScalable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.UnregisterTimeScalable(this);
            }
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