using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DungeonOwner.Core;
using DungeonOwner.Data;
using DungeonOwner.Managers;

namespace DungeonOwner.UI
{
    /// <summary>
    /// モンスター配置時のゴースト表示システム
    /// 要件15.3: モンスター配置時のゴースト表示
    /// </summary>
    public class PlacementGhostSystem : MonoBehaviour
    {
        public static PlacementGhostSystem Instance { get; private set; }

        [Header("Ghost Settings")]
        [SerializeField] private Material ghostMaterial;
        [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
        [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);
        [SerializeField] private Color warningPlacementColor = new Color(1, 1, 0, 0.5f);
        [SerializeField] private float ghostAlpha = 0.5f;

        [Header("Placement Indicators")]
        [SerializeField] private GameObject placementIndicatorPrefab;
        [SerializeField] private GameObject gridOverlayPrefab;
        [SerializeField] private LayerMask placementLayerMask = -1;
        [SerializeField] private float snapDistance = 0.5f;

        [Header("Visual Feedback")]
        [SerializeField] private GameObject validPlacementEffect;
        [SerializeField] private GameObject invalidPlacementEffect;
        [SerializeField] private AudioClip placementSound;
        [SerializeField] private AudioClip invalidSound;

        [Header("Grid Settings")]
        [SerializeField] private bool showGrid = true;
        [SerializeField] private float gridSize = 1f;
        [SerializeField] private Color gridColor = new Color(1, 1, 1, 0.2f);
        [SerializeField] private int gridWidth = 20;
        [SerializeField] private int gridHeight = 20;

        // 内部状態
        private GameObject currentGhost;
        private MonsterData currentMonsterData;
        private Camera mainCamera;
        private bool isPlacementMode = false;
        private Vector2 lastValidPosition;
        private List<GameObject> gridLines = new List<GameObject>();
        private List<GameObject> placementIndicators = new List<GameObject>();
        private Dictionary<Vector2, bool> placementGrid = new Dictionary<Vector2, bool>();

        // コンポーネント参照
        private AudioSource audioSource;
        private MonsterPlacementUI monsterPlacementUI;

        // イベント
        public System.Action<Vector2, bool> OnPlacementValidityChanged;
        public System.Action<Vector2> OnGhostPositionChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeGhostSystem();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SetupReferences();
            SubscribeToEvents();
        }

        private void Update()
        {
            if (isPlacementMode)
            {
                UpdateGhostPosition();
                UpdatePlacementValidation();
            }
        }

        /// <summary>
        /// ゴーストシステムの初期化
        /// </summary>
        private void InitializeGhostSystem()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // グリッドの初期化
            InitializePlacementGrid();
        }

        /// <summary>
        /// 参照の設定
        /// </summary>
        private void SetupReferences()
        {
            monsterPlacementUI = FindObjectOfType<MonsterPlacementUI>();
        }

        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (monsterPlacementUI != null)
            {
                monsterPlacementUI.OnPlacementModeChanged += OnPlacementModeChanged;
            }

            // FloorSystemのイベント
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged += OnFloorChanged;
            }
        }

        /// <summary>
        /// 配置グリッドの初期化
        /// </summary>
        private void InitializePlacementGrid()
        {
            placementGrid.Clear();
            
            // グリッドポイントを生成
            for (int x = -gridWidth / 2; x < gridWidth / 2; x++)
            {
                for (int y = -gridHeight / 2; y < gridHeight / 2; y++)
                {
                    Vector2 gridPoint = new Vector2(x * gridSize, y * gridSize);
                    placementGrid[gridPoint] = true; // 初期状態では配置可能
                }
            }
        }

        /// <summary>
        /// ゴーストの作成
        /// </summary>
        public void CreateGhost(MonsterData monsterData)
        {
            if (monsterData == null || monsterData.prefab == null)
            {
                Debug.LogWarning("Invalid monster data for ghost creation");
                return;
            }

            DestroyCurrentGhost();

            currentMonsterData = monsterData;
            currentGhost = Instantiate(monsterData.prefab);
            
            SetupGhostAppearance();
            SetupGhostComponents();
            
            isPlacementMode = true;
            
            if (showGrid)
            {
                ShowPlacementGrid();
            }

            Debug.Log($"Ghost created for {monsterData.displayName}");
        }

        /// <summary>
        /// ゴーストの外観設定
        /// </summary>
        private void SetupGhostAppearance()
        {
            if (currentGhost == null) return;

            // レンダラーの設定
            Renderer[] renderers = currentGhost.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Material[] materials = new Material[renderer.materials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    if (ghostMaterial != null)
                    {
                        materials[i] = ghostMaterial;
                    }
                    else
                    {
                        materials[i] = new Material(renderer.materials[i]);
                        SetMaterialTransparent(materials[i]);
                    }
                }
                renderer.materials = materials;
            }

            // SpriteRendererの設定
            SpriteRenderer[] spriteRenderers = currentGhost.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                Color color = spriteRenderer.color;
                color.a = ghostAlpha;
                spriteRenderer.color = color;
            }

            // UIコンポーネントの設定
            Image[] images = currentGhost.GetComponentsInChildren<Image>();
            foreach (var image in images)
            {
                Color color = image.color;
                color.a = ghostAlpha;
                image.color = color;
            }
        }

        /// <summary>
        /// マテリアルを透明に設定
        /// </summary>
        private void SetMaterialTransparent(Material material)
        {
            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", 3); // Transparent mode
            }
            
            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }
            
            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            
            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0);
            }
            
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;

            if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                color.a = ghostAlpha;
                material.color = color;
            }
        }

        /// <summary>
        /// ゴーストコンポーネントの設定
        /// </summary>
        private void SetupGhostComponents()
        {
            if (currentGhost == null) return;

            // コライダーを無効化
            Collider2D[] colliders = currentGhost.GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // Rigidbodyを無効化
            Rigidbody2D[] rigidbodies = currentGhost.GetComponentsInChildren<Rigidbody2D>();
            foreach (var rigidbody in rigidbodies)
            {
                rigidbody.simulated = false;
            }

            // MonoBehaviourスクリプトを無効化
            MonoBehaviour[] scripts = currentGhost.GetComponentsInChildren<MonoBehaviour>();
            foreach (var script in scripts)
            {
                if (script != null && script != this)
                {
                    script.enabled = false;
                }
            }

            // ゴースト専用のタグを設定
            currentGhost.tag = "PlacementGhost";
        }

        /// <summary>
        /// ゴーストの位置更新
        /// </summary>
        private void UpdateGhostPosition()
        {
            if (currentGhost == null || mainCamera == null) return;

            Vector2 mouseWorldPosition = GetMouseWorldPosition();
            Vector2 snappedPosition = SnapToGrid(mouseWorldPosition);
            
            currentGhost.transform.position = new Vector3(snappedPosition.x, snappedPosition.y, 0);
            
            OnGhostPositionChanged?.Invoke(snappedPosition);
        }

        /// <summary>
        /// 配置検証の更新
        /// </summary>
        private void UpdatePlacementValidation()
        {
            if (currentGhost == null) return;

            Vector2 ghostPosition = currentGhost.transform.position;
            bool isValid = ValidatePlacement(ghostPosition);
            
            UpdateGhostColor(isValid);
            UpdatePlacementIndicators(ghostPosition, isValid);
            
            OnPlacementValidityChanged?.Invoke(ghostPosition, isValid);
            
            if (isValid)
            {
                lastValidPosition = ghostPosition;
            }
        }

        /// <summary>
        /// 配置検証
        /// </summary>
        private bool ValidatePlacement(Vector2 position)
        {
            if (MonsterPlacementManager.Instance == null || FloorSystem.Instance == null)
            {
                return false;
            }

            int currentFloor = FloorSystem.Instance.CurrentViewFloor;
            
            // 基本的な配置可能性チェック
            if (!MonsterPlacementManager.Instance.CanPlaceMonster(currentFloor, position, currentMonsterData.type))
            {
                return false;
            }

            // グリッド内かどうかチェック
            Vector2 snappedPosition = SnapToGrid(position);
            if (!placementGrid.ContainsKey(snappedPosition))
            {
                return false;
            }

            // 他のオブジェクトとの重複チェック
            Collider2D[] overlapping = Physics2D.OverlapCircleAll(position, 0.4f, placementLayerMask);
            foreach (var collider in overlapping)
            {
                if (collider.gameObject != currentGhost && !collider.CompareTag("PlacementGhost"))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// ゴーストの色更新
        /// </summary>
        private void UpdateGhostColor(bool isValid)
        {
            if (currentGhost == null) return;

            Color targetColor = isValid ? validPlacementColor : invalidPlacementColor;
            
            // 金貨不足の場合は警告色
            if (isValid && !CanAffordMonster())
            {
                targetColor = warningPlacementColor;
            }

            // レンダラーの色更新
            Renderer[] renderers = currentGhost.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                foreach (var material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        material.color = targetColor;
                    }
                }
            }

            // SpriteRendererの色更新
            SpriteRenderer[] spriteRenderers = currentGhost.GetComponentsInChildren<SpriteRenderer>();
            foreach (var spriteRenderer in spriteRenderers)
            {
                spriteRenderer.color = targetColor;
            }
        }

        /// <summary>
        /// 配置インジケーターの更新
        /// </summary>
        private void UpdatePlacementIndicators(Vector2 position, bool isValid)
        {
            // 既存のインジケーターをクリア
            ClearPlacementIndicators();

            if (placementIndicatorPrefab == null) return;

            // 新しいインジケーターを作成
            GameObject indicator = Instantiate(placementIndicatorPrefab, transform);
            indicator.transform.position = new Vector3(position.x, position.y, -0.1f);
            
            // インジケーターの色設定
            Renderer indicatorRenderer = indicator.GetComponent<Renderer>();
            if (indicatorRenderer != null)
            {
                Color indicatorColor = isValid ? validPlacementColor : invalidPlacementColor;
                indicatorRenderer.material.color = indicatorColor;
            }

            placementIndicators.Add(indicator);
        }

        /// <summary>
        /// 配置インジケーターのクリア
        /// </summary>
        private void ClearPlacementIndicators()
        {
            foreach (var indicator in placementIndicators)
            {
                if (indicator != null)
                {
                    Destroy(indicator);
                }
            }
            placementIndicators.Clear();
        }

        /// <summary>
        /// 配置グリッドの表示
        /// </summary>
        private void ShowPlacementGrid()
        {
            HidePlacementGrid();

            if (gridOverlayPrefab == null) return;

            // グリッドラインを生成
            for (int x = -gridWidth / 2; x <= gridWidth / 2; x++)
            {
                GameObject line = CreateGridLine(
                    new Vector3(x * gridSize, -gridHeight / 2 * gridSize, -0.5f),
                    new Vector3(x * gridSize, gridHeight / 2 * gridSize, -0.5f)
                );
                gridLines.Add(line);
            }

            for (int y = -gridHeight / 2; y <= gridHeight / 2; y++)
            {
                GameObject line = CreateGridLine(
                    new Vector3(-gridWidth / 2 * gridSize, y * gridSize, -0.5f),
                    new Vector3(gridWidth / 2 * gridSize, y * gridSize, -0.5f)
                );
                gridLines.Add(line);
            }
        }

        /// <summary>
        /// グリッドラインの作成
        /// </summary>
        private GameObject CreateGridLine(Vector3 start, Vector3 end)
        {
            GameObject line = new GameObject("GridLine");
            line.transform.SetParent(transform);
            
            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.color = gridColor;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.sortingOrder = -1;
            
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            
            return line;
        }

        /// <summary>
        /// 配置グリッドの非表示
        /// </summary>
        private void HidePlacementGrid()
        {
            foreach (var line in gridLines)
            {
                if (line != null)
                {
                    Destroy(line);
                }
            }
            gridLines.Clear();
        }

        /// <summary>
        /// グリッドにスナップ
        /// </summary>
        private Vector2 SnapToGrid(Vector2 position)
        {
            float snappedX = Mathf.Round(position.x / gridSize) * gridSize;
            float snappedY = Mathf.Round(position.y / gridSize) * gridSize;
            return new Vector2(snappedX, snappedY);
        }

        /// <summary>
        /// マウスのワールド座標取得
        /// </summary>
        private Vector2 GetMouseWorldPosition()
        {
            if (mainCamera == null) return Vector2.zero;

            Vector3 mousePos = Input.mousePosition;
            mousePos.z = -mainCamera.transform.position.z;
            return mainCamera.ScreenToWorldPoint(mousePos);
        }

        /// <summary>
        /// モンスター購入可能性チェック
        /// </summary>
        private bool CanAffordMonster()
        {
            if (currentMonsterData == null) return false;
            
            // TODO: ResourceManagerと連携
            return true; // 仮実装
        }

        /// <summary>
        /// 配置実行
        /// </summary>
        public bool TryPlaceMonster()
        {
            if (currentGhost == null || currentMonsterData == null) return false;

            Vector2 ghostPosition = currentGhost.transform.position;
            
            if (!ValidatePlacement(ghostPosition))
            {
                PlaySound(invalidSound);
                return false;
            }

            // 配置成功
            PlaySound(placementSound);
            ShowPlacementEffect(ghostPosition, true);
            
            return true;
        }

        /// <summary>
        /// 現在のゴーストを破棄
        /// </summary>
        public void DestroyCurrentGhost()
        {
            if (currentGhost != null)
            {
                Destroy(currentGhost);
                currentGhost = null;
            }

            currentMonsterData = null;
            isPlacementMode = false;
            
            HidePlacementGrid();
            ClearPlacementIndicators();
        }

        /// <summary>
        /// 配置エフェクトの表示
        /// </summary>
        private void ShowPlacementEffect(Vector2 position, bool isValid)
        {
            GameObject effectPrefab = isValid ? validPlacementEffect : invalidPlacementEffect;
            if (effectPrefab != null)
            {
                GameObject effect = Instantiate(effectPrefab);
                effect.transform.position = new Vector3(position.x, position.y, -0.2f);
                Destroy(effect, 2f);
            }
        }

        /// <summary>
        /// サウンド再生
        /// </summary>
        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        /// <summary>
        /// 配置モード変更時の処理
        /// </summary>
        private void OnPlacementModeChanged()
        {
            if (monsterPlacementUI != null && !monsterPlacementUI.IsInPlacementMode())
            {
                DestroyCurrentGhost();
            }
        }

        /// <summary>
        /// 階層変更時の処理
        /// </summary>
        private void OnFloorChanged(int newFloor)
        {
            // 階層が変わったらゴーストを一時的に非表示
            if (currentGhost != null)
            {
                currentGhost.SetActive(false);
                // 少し遅延してから再表示
                Invoke(nameof(ReactivateGhost), 0.1f);
            }
        }

        /// <summary>
        /// ゴーストの再アクティブ化
        /// </summary>
        private void ReactivateGhost()
        {
            if (currentGhost != null)
            {
                currentGhost.SetActive(true);
            }
        }

        /// <summary>
        /// 公開メソッド：配置モード状態の取得
        /// </summary>
        public bool IsInPlacementMode()
        {
            return isPlacementMode;
        }

        /// <summary>
        /// 公開メソッド：現在のゴースト位置の取得
        /// </summary>
        public Vector2 GetCurrentGhostPosition()
        {
            return currentGhost != null ? (Vector2)currentGhost.transform.position : Vector2.zero;
        }

        /// <summary>
        /// 公開メソッド：最後の有効位置の取得
        /// </summary>
        public Vector2 GetLastValidPosition()
        {
            return lastValidPosition;
        }

        private void OnDestroy()
        {
            DestroyCurrentGhost();
            HidePlacementGrid();
            ClearPlacementIndicators();

            // イベント購読解除
            if (monsterPlacementUI != null)
            {
                monsterPlacementUI.OnPlacementModeChanged -= OnPlacementModeChanged;
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged -= OnFloorChanged;
            }
        }
    }
}