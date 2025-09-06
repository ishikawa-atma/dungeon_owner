using UnityEngine;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 改装システムで使用される壁オブジェクト
    /// 改装モード中の一時的な壁表示と相互作用を管理
    /// </summary>
    public class RenovationWall : MonoBehaviour
    {
        [Header("Wall Configuration")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D wallCollider;
        [SerializeField] private Color normalColor = Color.gray;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color errorColor = Color.red;

        [Header("Visual Effects")]
        [SerializeField] private GameObject placementEffect;
        [SerializeField] private GameObject removalEffect;
        [SerializeField] private float effectDuration = 0.5f;

        // 状態管理
        private Vector2 gridPosition;
        private bool isHighlighted = false;
        private bool isTemporary = true;

        // プロパティ
        public Vector2 GridPosition => gridPosition;
        public bool IsTemporary => isTemporary;

        private void Awake()
        {
            InitializeWall();
        }

        private void Start()
        {
            // 配置エフェクトを再生
            PlayPlacementEffect();
        }

        /// <summary>
        /// 壁を初期化
        /// </summary>
        private void InitializeWall()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (wallCollider == null)
            {
                wallCollider = GetComponent<Collider2D>();
            }

            // グリッド位置を設定
            gridPosition = new Vector2(
                Mathf.Round(transform.position.x),
                Mathf.Round(transform.position.y)
            );

            // 位置をグリッドに合わせる
            transform.position = new Vector3(gridPosition.x, gridPosition.y, transform.position.z);

            // 初期色を設定
            SetColor(normalColor);
        }

        /// <summary>
        /// 壁の色を設定
        /// </summary>
        public void SetColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        /// <summary>
        /// ハイライト状態を設定
        /// </summary>
        public void SetHighlight(bool highlight)
        {
            isHighlighted = highlight;
            SetColor(highlight ? highlightColor : normalColor);
        }

        /// <summary>
        /// エラー状態を表示
        /// </summary>
        public void ShowError()
        {
            SetColor(errorColor);
            Invoke(nameof(ResetColor), 1f);
        }

        /// <summary>
        /// 色をリセット
        /// </summary>
        private void ResetColor()
        {
            SetColor(isHighlighted ? highlightColor : normalColor);
        }

        /// <summary>
        /// 一時的な壁かどうかを設定
        /// </summary>
        public void SetTemporary(bool temporary)
        {
            isTemporary = temporary;
            
            // 一時的でない場合は色を変更
            if (!temporary)
            {
                SetColor(Color.white);
            }
        }

        /// <summary>
        /// 配置エフェクトを再生
        /// </summary>
        private void PlayPlacementEffect()
        {
            if (placementEffect != null)
            {
                GameObject effect = Instantiate(placementEffect, transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
        }

        /// <summary>
        /// 除去エフェクトを再生
        /// </summary>
        public void PlayRemovalEffect()
        {
            if (removalEffect != null)
            {
                GameObject effect = Instantiate(removalEffect, transform.position, Quaternion.identity);
                Destroy(effect, effectDuration);
            }
        }

        /// <summary>
        /// マウスオーバー時の処理
        /// </summary>
        private void OnMouseEnter()
        {
            if (FloorRenovationSystem.Instance != null && FloorRenovationSystem.Instance.IsRenovationMode)
            {
                SetHighlight(true);
            }
        }

        /// <summary>
        /// マウスアウト時の処理
        /// </summary>
        private void OnMouseExit()
        {
            if (FloorRenovationSystem.Instance != null && FloorRenovationSystem.Instance.IsRenovationMode)
            {
                SetHighlight(false);
            }
        }

        /// <summary>
        /// クリック時の処理
        /// </summary>
        private void OnMouseDown()
        {
            if (FloorRenovationSystem.Instance != null && FloorRenovationSystem.Instance.IsRenovationMode)
            {
                // 壁を除去
                bool removed = FloorRenovationSystem.Instance.RemoveWall(gridPosition);
                if (removed)
                {
                    PlayRemovalEffect();
                }
                else
                {
                    ShowError();
                }
            }
        }

        /// <summary>
        /// 壁が他のオブジェクトと衝突しているかチェック
        /// </summary>
        public bool IsCollidingWithOthers()
        {
            if (wallCollider == null)
            {
                return false;
            }

            // 他の壁やオブジェクトとの衝突をチェック
            Collider2D[] overlapping = Physics2D.OverlapCircleAll(transform.position, 0.4f);
            
            foreach (Collider2D col in overlapping)
            {
                if (col != wallCollider && col.gameObject != gameObject)
                {
                    // モンスターや侵入者との衝突をチェック
                    if (col.CompareTag("Monster") || col.CompareTag("Invader"))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 壁の情報をデバッグ出力
        /// </summary>
        public void DebugPrintWallInfo()
        {
            Debug.Log($"Wall at {gridPosition}: Temporary={isTemporary}, Highlighted={isHighlighted}, Colliding={IsCollidingWithOthers()}");
        }

        /// <summary>
        /// 壁を破棄する前の処理
        /// </summary>
        private void OnDestroy()
        {
            PlayRemovalEffect();
        }
    }
}