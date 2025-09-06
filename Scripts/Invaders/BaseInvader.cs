using UnityEngine;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Core;

namespace DungeonOwner.Invaders
{
    public class BaseInvader : MonoBehaviour, IInvader, ICharacterBase, ITimeScalable, IPoolable
    {
        [Header("Invader Configuration")]
        [SerializeField] protected InvaderData invaderData;
        [SerializeField] protected int level = 1;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected InvaderState currentState = InvaderState.Spawning;

        [Header("Movement")]
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected Vector2 targetPosition;
        [SerializeField] protected bool isMoving = false;

        [Header("Combat")]
        [SerializeField] protected float attackPower;
        [SerializeField] protected float attackRange = 1.5f;
        [SerializeField] protected float attackCooldown = 1f;
        [SerializeField] protected float lastAttackTime;

        [Header("Visual")]
        [SerializeField] protected SpriteRenderer spriteRenderer;
        [SerializeField] protected Animator animator;
        [SerializeField] protected InvaderAnimationController animationController;
        
        [Header("Navigation")]
        [SerializeField] protected InvaderPathfinding pathfinding;

        // IInvader プロパティ
        public InvaderType Type => invaderData?.type ?? InvaderType.Warrior;
        public int Level => level;
        public float Health => currentHealth;
        public float MaxHealth => invaderData?.GetHealthAtLevel(level) ?? 100f;
        public Vector2 Position 
        { 
            get => transform.position; 
            set => transform.position = new Vector3(value.x, value.y, transform.position.z); 
        }
        public InvaderState State => currentState;
        public IParty Party { get; set; }

        protected virtual void Awake()
        {
            // コンポーネント取得
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null)
                animator = GetComponent<Animator>();
            if (animationController == null)
                animationController = GetComponent<InvaderAnimationController>();
            if (pathfinding == null)
                pathfinding = GetComponent<InvaderPathfinding>();

            // 初期化
            InitializeInvader();
        }

        protected virtual void Start()
        {
            // 出現エフェクト
            PlaySpawnEffect();
            
            // レベル表示システムに登録
            RegisterLevelDisplay();
            
            // 時間制御システムに登録
            RegisterTimeScalable();
        }

        protected virtual void Update()
        {
            // 通常のUpdate処理は時間制御システムを使用しない場合のフォールバック
            if (TimeManager.Instance == null || !TimeManager.Instance.IsSpeedControlEnabled)
            {
                UpdateInvader();
            }
        }

        protected virtual void InitializeInvader()
        {
            if (invaderData != null)
            {
                // ステータス初期化
                currentHealth = MaxHealth;
                attackPower = invaderData.GetAttackPowerAtLevel(level);
                moveSpeed = invaderData.moveSpeed;

                // 見た目の初期化
                if (spriteRenderer != null && invaderData.icon != null)
                {
                    spriteRenderer.sprite = invaderData.icon;
                }

                Debug.Log($"Initialized {invaderData.displayName} (Level {level}) - HP: {MaxHealth}, Attack: {attackPower}");
            }
        }

        protected virtual void UpdateInvader()
        {
            switch (currentState)
            {
                case InvaderState.Spawning:
                    UpdateSpawning();
                    break;
                case InvaderState.Moving:
                    UpdateMovement();
                    break;
                case InvaderState.Fighting:
                    UpdateFighting();
                    break;
                case InvaderState.Retreating:
                    UpdateRetreating();
                    break;
                case InvaderState.Dead:
                    // 死亡状態では何もしない
                    break;
            }
        }

        protected virtual void UpdateSpawning()
        {
            // 出現アニメーション完了後、移動状態に移行
            if (Time.time > lastAttackTime + 1f) // 1秒後に移動開始
            {
                SetState(InvaderState.Moving);
                FindNextTarget();
            }
        }

        protected virtual void UpdateMovement()
        {
            if (isMoving && targetPosition != Vector2.zero)
            {
                // 目標位置に向かって移動
                Vector2 currentPos = transform.position;
                Vector2 direction = (targetPosition - currentPos).normalized;
                float distance = Vector2.Distance(currentPos, targetPosition);

                if (distance > 0.1f)
                {
                    Vector2 newPosition = currentPos + direction * moveSpeed * Time.deltaTime;
                    transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
                }
                else
                {
                    // 目標位置に到達
                    transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
                    isMoving = false;
                    OnReachedTarget();
                }
            }
            else
            {
                // 新しい目標を探す
                FindNextTarget();
            }

            // 近くの敵をチェック
            CheckForEnemies();
        }

        protected virtual void UpdateFighting()
        {
            // CombatDetectorとCombatManagerが戦闘を処理するため、
            // ここでは戦闘アニメーションと状態管理のみ行う
            
            if (Core.CombatDetector.Instance != null)
            {
                var enemies = Core.CombatDetector.Instance.GetCombatEnemies(gameObject);
                if (enemies.Count == 0)
                {
                    // 敵がいなくなったら移動状態に戻る
                    SetState(InvaderState.Moving);
                }
            }
        }

        protected virtual void UpdateRetreating()
        {
            // 撤退処理（現在は未実装）
            // 将来的にはHPが低い時の撤退ロジックを実装
        }

        protected virtual void FindNextTarget()
        {
            // 下り階段を目標にする（ダンジョンコアに向かう）
            var floorSystem = Core.FloorSystem.Instance;
            if (floorSystem != null)
            {
                var currentFloor = floorSystem.GetCurrentFloor();
                if (currentFloor != null)
                {
                    targetPosition = currentFloor.downStairPosition;
                    isMoving = true;
                    
                    // パスファインディングを使用
                    if (pathfinding != null)
                    {
                        pathfinding.SetDestination(targetPosition);
                    }
                }
            }
        }

        protected virtual void OnReachedTarget()
        {
            // 階段に到達した場合の処理
            // 次の階層に移動するか、ダンジョンコアに到達した場合の処理
            Debug.Log($"{name} reached target position");
        }

        protected virtual void CheckForEnemies()
        {
            // CombatDetectorが戦闘検出を処理するため、ここでは状態チェックのみ
            if (Core.CombatDetector.Instance != null && Core.CombatDetector.Instance.IsInCombat(gameObject))
            {
                if (currentState != InvaderState.Fighting)
                {
                    SetState(InvaderState.Fighting);
                }
            }
        }

        protected virtual System.Collections.Generic.List<GameObject> FindNearbyEnemies()
        {
            var enemies = new System.Collections.Generic.List<GameObject>();
            
            // 攻撃範囲内のモンスターを検索
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
            
            foreach (var collider in colliders)
            {
                if (collider.gameObject != gameObject && collider.CompareTag("Monster"))
                {
                    enemies.Add(collider.gameObject);
                }
            }
            
            return enemies;
        }

        protected virtual void AttackEnemy(GameObject enemy)
        {
            if (enemy == null) return;

            // 攻撃処理
            var monster = enemy.GetComponent<Interfaces.IMonster>();
            if (monster != null)
            {
                monster.TakeDamage(attackPower);
                lastAttackTime = Time.time;
                
                // 攻撃エフェクト
                PlayAttackEffect();
                
                Debug.Log($"{name} attacked {enemy.name} for {attackPower} damage");
            }
        }

        protected virtual void PlaySpawnEffect()
        {
            // 出現エフェクトの再生
            if (animator != null)
            {
                animator.SetTrigger("Spawn");
            }
            
            lastAttackTime = Time.time; // 出現時間を記録
        }

        protected virtual void PlayAttackEffect()
        {
            // 攻撃エフェクトの再生
            if (animationController != null)
            {
                animationController.PlayAttackAnimation();
            }
            else if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }

        // IInvader インターフェース実装
        public virtual void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            // ダメージエフェクト
            if (animationController != null)
            {
                animationController.PlayTakeDamageAnimation();
                animationController.FlashSprite(Color.red, 0.2f);
            }
            else if (animator != null)
            {
                animator.SetTrigger("TakeDamage");
            }
            
            Debug.Log($"{name} took {damage} damage. Health: {currentHealth}/{MaxHealth}");
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public virtual void Move(Vector2 targetPos)
        {
            targetPosition = targetPos;
            isMoving = true;
            
            if (currentState != InvaderState.Fighting)
            {
                SetState(InvaderState.Moving);
            }
            
            // パスファインディングを使用
            if (pathfinding != null)
            {
                pathfinding.SetDestination(targetPos);
            }
        }

        public virtual void JoinParty(IParty party)
        {
            if (Party != null)
            {
                Party.RemoveMember(this);
            }
            
            Party = party;
            party?.AddMember(this);
        }

        public virtual void LeaveParty()
        {
            Party?.RemoveMember(this);
            Party = null;
        }

        public virtual void SetState(InvaderState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                OnStateChanged(newState);
            }
        }

        protected virtual void OnStateChanged(InvaderState newState)
        {
            // アニメーション状態の更新
            if (animationController != null)
            {
                animationController.SetState(newState);
            }
            else if (animator != null)
            {
                animator.SetInteger("State", (int)newState);
            }
            
            Debug.Log($"{name} state changed to: {newState}");
        }

        protected virtual void Die()
        {
            SetState(InvaderState.Dead);
            
            // 死亡エフェクト
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }
            
            // 戦闘状況をクリア
            if (Core.CombatDetector.Instance != null)
            {
                Core.CombatDetector.Instance.ClearCombatEngagements(gameObject);
            }
            
            // レベル表示システムから削除
            UnregisterLevelDisplay();
            
            // 時間制御システムから削除
            UnregisterTimeScalable();
            
            // 報酬処理
            GiveRewards();
            
            // パーティから離脱
            LeaveParty();
            
            // 階層から除去
            RemoveFromFloor();
            
            Debug.Log($"{name} has been defeated!");
            
            // InvaderSpawnerに撃破を通知（オブジェクトプールに返却される）
            if (Managers.InvaderSpawner.Instance != null)
            {
                Managers.InvaderSpawner.Instance.OnInvaderDestroyed(gameObject);
            }
            else
            {
                // フォールバック: 従来の破棄処理
                Destroy(gameObject, 1f);
            }
        }

        protected virtual void GiveRewards()
        {
            if (invaderData != null)
            {
                int goldReward = invaderData.GetGoldRewardAtLevel(level);
                
                // 金貨報酬を付与（ResourceManagerが実装されたら）
                Debug.Log($"Gold reward: {goldReward}");
                
                // 罠アイテムドロップ判定
                if (Random.value < invaderData.trapItemDropRate)
                {
                    Debug.Log("Trap item dropped!");
                    // 罠アイテム生成処理（後で実装）
                }
            }
        }

        protected virtual void RemoveFromFloor()
        {
            var floorSystem = Core.FloorSystem.Instance;
            if (floorSystem != null)
            {
                var currentFloor = floorSystem.GetCurrentFloor();
                currentFloor?.RemoveInvader(gameObject);
            }
        }

        // 設定メソッド
        public void SetInvaderData(InvaderData data)
        {
            invaderData = data;
            InitializeInvader();
        }

        public void SetLevel(int newLevel)
        {
            level = Mathf.Max(1, newLevel);
            InitializeInvader();
        }

        public float GetAttackPower()
        {
            return attackPower;
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

        // ITimeScalable 実装
        public virtual void UpdateWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            // 時間スケールを考慮した更新処理
            UpdateInvaderWithTimeScale(scaledDeltaTime, timeScale);
        }

        public virtual void OnTimeScaleChanged(float newTimeScale)
        {
            // 時間スケール変更時の処理
            // アニメーションスピードの調整
            if (animator != null)
            {
                animator.speed = newTimeScale;
            }
            
            // アニメーションコントローラーにも通知
            if (animationController != null && animationController is ITimeScalable timeScalableController)
            {
                timeScalableController.OnTimeScaleChanged(newTimeScale);
            }
        }

        // 時間スケール対応の更新メソッド
        protected virtual void UpdateInvaderWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            switch (currentState)
            {
                case InvaderState.Spawning:
                    UpdateSpawningWithTimeScale(scaledDeltaTime, timeScale);
                    break;
                case InvaderState.Moving:
                    UpdateMovementWithTimeScale(scaledDeltaTime, timeScale);
                    break;
                case InvaderState.Fighting:
                    UpdateFightingWithTimeScale(scaledDeltaTime, timeScale);
                    break;
                case InvaderState.Retreating:
                    UpdateRetreatingWithTimeScale(scaledDeltaTime, timeScale);
                    break;
                case InvaderState.Dead:
                    // 死亡状態では何もしない
                    break;
            }
        }

        protected virtual void UpdateSpawningWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            // 出現アニメーション完了後、移動状態に移行（時間スケール対応）
            if (Time.time > lastAttackTime + (1f / timeScale)) // 時間スケールを考慮
            {
                SetState(InvaderState.Moving);
                FindNextTarget();
            }
        }

        protected virtual void UpdateMovementWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            if (isMoving && targetPosition != Vector2.zero)
            {
                // 目標位置に向かって移動（時間スケール対応）
                Vector2 currentPos = transform.position;
                Vector2 direction = (targetPosition - currentPos).normalized;
                float distance = Vector2.Distance(currentPos, targetPosition);

                if (distance > 0.1f)
                {
                    Vector2 newPosition = currentPos + direction * moveSpeed * scaledDeltaTime;
                    transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
                }
                else
                {
                    // 目標位置に到達
                    transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
                    isMoving = false;
                    OnReachedTarget();
                }
            }
            else
            {
                // 新しい目標を探す
                FindNextTarget();
            }

            // 近くの敵をチェック
            CheckForEnemies();
        }

        protected virtual void UpdateFightingWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            // CombatDetectorとCombatManagerが戦闘を処理するため、
            // ここでは戦闘アニメーションと状態管理のみ行う
            
            if (Core.CombatDetector.Instance != null)
            {
                var enemies = Core.CombatDetector.Instance.GetCombatEnemies(gameObject);
                if (enemies.Count == 0)
                {
                    // 敵がいなくなったら移動状態に戻る
                    SetState(InvaderState.Moving);
                }
            }
        }

        protected virtual void UpdateRetreatingWithTimeScale(float scaledDeltaTime, float timeScale)
        {
            // 撤退処理（現在は未実装）
            // 将来的にはHPが低い時の撤退ロジックを実装
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

        // IPoolable 実装
        public virtual void OnPoolCreated()
        {
            // プール作成時の初期化
            Debug.Log($"Invader {name} created in pool");
        }

        public virtual void OnSpawn()
        {
            // プールから取得時の初期化
            gameObject.SetActive(true);
            
            // ステータスをリセット
            if (invaderData != null)
            {
                currentHealth = MaxHealth;
                currentState = InvaderState.Spawning;
                isMoving = false;
                targetPosition = Vector2.zero;
                lastAttackTime = Time.time;
            }
            
            // システムに再登録
            RegisterLevelDisplay();
            RegisterTimeScalable();
            
            // 出現エフェクト
            PlaySpawnEffect();
            
            Debug.Log($"Invader {name} spawned from pool");
        }

        public virtual void OnReturn()
        {
            // プールに返却時のクリーンアップ
            
            // 戦闘状況をクリア
            if (Core.CombatDetector.Instance != null)
            {
                Core.CombatDetector.Instance.ClearCombatEngagements(gameObject);
            }
            
            // システムから登録解除
            UnregisterLevelDisplay();
            UnregisterTimeScalable();
            
            // パーティから離脱
            LeaveParty();
            
            // 階層から除去
            RemoveFromFloor();
            
            // 状態をリセット
            currentState = InvaderState.Dead;
            isMoving = false;
            targetPosition = Vector2.zero;
            
            // アニメーションをリセット
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }
            
            Debug.Log($"Invader {name} returned to pool");
        }

        // デバッグ用
        private void OnDrawGizmosSelected()
        {
            // 攻撃範囲を表示
            Gizmos.color = Color.red;
            Gizmos.DrawWireCircle(transform.position, attackRange);
            
            // 移動目標を表示
            if (targetPosition != Vector2.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, targetPosition);
                Gizmos.DrawWireSphere(targetPosition, 0.5f);
            }
        }
    }
}