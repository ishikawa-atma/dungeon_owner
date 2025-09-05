using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;

namespace DungeonOwner.Core
{
    /// <summary>
    /// モンスターアビリティの基底クラス
    /// 要件4.1, 4.4に対応
    /// </summary>
    public abstract class MonsterAbility : IMonsterAbility
    {
        [Header("Ability Configuration")]
        [SerializeField] protected MonsterAbilityType abilityType;
        [SerializeField] protected string abilityName;
        [SerializeField] protected string description;
        [SerializeField] protected float cooldownTime;
        [SerializeField] protected float manaCost;
        
        protected IMonster owner;
        protected float lastUsedTime = -999f;
        protected bool isInitialized = false;

        // プロパティ
        public MonsterAbilityType AbilityType => abilityType;
        public string AbilityName => abilityName;
        public string Description => description;
        public float CooldownTime => cooldownTime;
        public float ManaCost => manaCost;
        public float LastUsedTime => lastUsedTime;
        
        public virtual bool CanUse
        {
            get
            {
                if (!isInitialized || owner == null) return false;
                
                // クールダウンチェック
                if (Time.time - lastUsedTime < cooldownTime) return false;
                
                // マナチェック
                if (owner.Mana < manaCost) return false;
                
                // カスタム条件チェック
                return CanUseCustomCondition();
            }
        }

        // 初期化
        public virtual void Initialize(IMonster owner)
        {
            this.owner = owner;
            isInitialized = true;
            OnInitialize();
        }

        // アビリティ実行
        public virtual bool Execute()
        {
            if (!CanUse) return false;
            
            // マナを消費
            if (manaCost > 0)
            {
                // マナ消費処理（BaseMonsterに追加が必要）
                ConsumeMana(manaCost);
            }
            
            // 実行時間を記録
            lastUsedTime = Time.time;
            
            // 実際のアビリティ効果を実行
            bool success = ExecuteAbility();
            
            if (success)
            {
                ShowEffect();
                OnAbilityExecuted();
            }
            
            return success;
        }

        // 更新処理
        public virtual void Update()
        {
            if (!isInitialized) return;
            
            UpdateAbility();
        }

        // リセット
        public virtual void Reset()
        {
            lastUsedTime = -999f;
            OnReset();
        }

        // エフェクト表示
        public virtual void ShowEffect()
        {
            // デフォルトはログ出力
            Debug.Log($"{owner?.GetType().Name} used {abilityName}!");
            
            // サブクラスでオーバーライドして視覚効果を実装
            ShowVisualEffect();
        }

        // 抽象メソッド（サブクラスで実装必須）
        protected abstract bool ExecuteAbility();
        protected abstract bool CanUseCustomCondition();
        
        // 仮想メソッド（オーバーライド可能）
        protected virtual void OnInitialize() { }
        protected virtual void UpdateAbility() { }
        protected virtual void OnAbilityExecuted() { }
        protected virtual void OnReset() { }
        protected virtual void ShowVisualEffect() { }
        
        // ユーティリティメソッド
        protected virtual void ConsumeMana(float amount)
        {
            // BaseMonsterにマナ消費メソッドを追加する必要がある
            if (owner is Monsters.BaseMonster baseMonster)
            {
                // リフレクションを使用してマナを減らす（一時的な解決策）
                var manaField = typeof(Monsters.BaseMonster).GetField("currentMana", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (manaField != null)
                {
                    float currentMana = (float)manaField.GetValue(baseMonster);
                    manaField.SetValue(baseMonster, Mathf.Max(0, currentMana - amount));
                }
            }
        }
        
        protected virtual bool IsOwnerAlive()
        {
            return owner != null && owner.Health > 0;
        }
        
        protected virtual float GetOwnerHealthRatio()
        {
            if (owner == null || owner.MaxHealth <= 0) return 0f;
            return owner.Health / owner.MaxHealth;
        }
        
        protected virtual float GetOwnerManaRatio()
        {
            if (owner == null || owner.MaxMana <= 0) return 0f;
            return owner.Mana / owner.MaxMana;
        }
    }
}