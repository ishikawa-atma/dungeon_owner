using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using DungeonOwner.Core;
using DungeonOwner.Interfaces;

namespace DungeonOwner.UI
{
    /// <summary>
    /// リアルタイム戦闘の視覚的表示UI
    /// 要件15.4: リアルタイム戦闘の視覚的表示
    /// </summary>
    public class CombatVisualUI : MonoBehaviour
    {
        public static CombatVisualUI Instance { get; private set; }

        [Header("Combat Visual Settings")]
        [SerializeField] private Canvas combatCanvas;
        [SerializeField] private GameObject damageTextPrefab;
        [SerializeField] private GameObject combatEffectPrefab;
        [SerializeField] private GameObject healEffectPrefab;

        [Header("Damage Display")]
        [SerializeField] private Color monsterDamageColor = Color.red;
        [SerializeField] private Color invaderDamageColor = Color.blue;
        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color criticalColor = Color.yellow;
        [SerializeField] private float damageTextDuration = 2f;
        [SerializeField] private float damageTextMoveDistance = 100f;

        [Header("Combat Effects")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject blockEffectPrefab;
        [SerializeField] private GameObject criticalEffectPrefab;
        [SerializeField] private GameObject deathEffectPrefab;

        [Header("Health Bars")]
        [SerializeField] private GameObject healthBarPrefab;
        [SerializeField] private Transform healthBarContainer;
        [SerializeField] private float healthBarOffset = 50f;
        [SerializeField] private float healthBarDuration = 5f;

        [Header("Combat Status")]
        [SerializeField] private GameObject combatStatusPanel;
        [SerializeField] private TextMeshProUGUI combatCountText;
        [SerializeField] private Image combatIntensityBar;
        [SerializeField] private Color lowIntensityColor = Color.green;
        [SerializeField] private Color highIntensityColor = Color.red;

        // 内部状態
        private Dictionary<ICharacter, GameObject> activeHealthBars = new Dictionary<ICharacter, GameObject>();
        private List<GameObject> activeDamageTexts = new List<GameObject>();
        private Queue<GameObject> damageTextPool = new Queue<GameObject>();
        private Queue<GameObject> effectPool = new Queue<GameObject>();
        private int activeCombats = 0;
        private float combatIntensity = 0f;

        // イベント
        public System.Action<bool> OnCombatStateChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeCombatVisual();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            SubscribeToCombatEvents();
            InitializeObjectPools();
        }

        private void Update()
        {
            UpdateCombatIntensity();
            CleanupExpiredElements();
        }

        /// <summary>
        /// 戦闘ビジュアルの初期化
        /// </summary>
        private void InitializeCombatVisual()
        {
            if (combatCanvas == null)
            {
                combatCanvas = GetComponent<Canvas>();
                if (combatCanvas == null)
                {
                    combatCanvas = gameObject.AddComponent<Canvas>();
                }
            }

            // 戦闘用キャンバスの設定
            combatCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            combatCanvas.sortingOrder = 200; // UIManagerより上位

            // 戦闘状況パネルの初期化
            if (combatStatusPanel != null)
            {
                combatStatusPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 戦闘イベントの購読
        /// </summary>
        private void SubscribeToCombatEvents()
        {
            // CombatManagerのイベントに登録
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.OnCombatStarted += OnCombatStarted;
                CombatManager.Instance.OnCombatEnded += OnCombatEnded;
                CombatManager.Instance.OnDamageDealt += OnDamageDealt;
                CombatManager.Instance.OnCharacterHealed += OnCharacterHealed;
                CombatManager.Instance.OnCharacterDied += OnCharacterDied;
            }

            // PartyCombatSystemのイベントに登録
            if (PartyCombatSystem.Instance != null)
            {
                PartyCombatSystem.Instance.OnPartyCombatStarted += OnPartyCombatStarted;
                PartyCombatSystem.Instance.OnPartyCombatEnded += OnPartyCombatEnded;
            }
        }

        /// <summary>
        /// オブジェクトプールの初期化
        /// </summary>
        private void InitializeObjectPools()
        {
            // ダメージテキストプール
            for (int i = 0; i < 20; i++)
            {
                if (damageTextPrefab != null)
                {
                    GameObject damageText = Instantiate(damageTextPrefab, transform);
                    damageText.SetActive(false);
                    damageTextPool.Enqueue(damageText);
                }
            }

            // エフェクトプール
            for (int i = 0; i < 15; i++)
            {
                if (combatEffectPrefab != null)
                {
                    GameObject effect = Instantiate(combatEffectPrefab, transform);
                    effect.SetActive(false);
                    effectPool.Enqueue(effect);
                }
            }
        }

        /// <summary>
        /// 戦闘開始時の処理
        /// </summary>
        private void OnCombatStarted(ICharacter attacker, ICharacter defender)
        {
            activeCombats++;
            UpdateCombatStatus();

            // 戦闘参加者の体力バーを表示
            ShowHealthBar(attacker);
            ShowHealthBar(defender);

            // 戦闘エフェクトを表示
            ShowCombatEffect(attacker.Position, defender.Position);

            OnCombatStateChanged?.Invoke(true);
        }

        /// <summary>
        /// 戦闘終了時の処理
        /// </summary>
        private void OnCombatEnded(ICharacter attacker, ICharacter defender)
        {
            activeCombats = Mathf.Max(0, activeCombats - 1);
            UpdateCombatStatus();

            if (activeCombats == 0)
            {
                OnCombatStateChanged?.Invoke(false);
            }
        }

        /// <summary>
        /// パーティ戦闘開始時の処理
        /// </summary>
        private void OnPartyCombatStarted(IParty attackerParty, IParty defenderParty)
        {
            activeCombats += attackerParty.Members.Count * defenderParty.Members.Count;
            UpdateCombatStatus();

            // パーティメンバー全員の体力バーを表示
            foreach (var member in attackerParty.Members)
            {
                ShowHealthBar(member);
            }
            foreach (var member in defenderParty.Members)
            {
                ShowHealthBar(member);
            }

            OnCombatStateChanged?.Invoke(true);
        }

        /// <summary>
        /// パーティ戦闘終了時の処理
        /// </summary>
        private void OnPartyCombatEnded(IParty attackerParty, IParty defenderParty)
        {
            activeCombats = Mathf.Max(0, activeCombats - attackerParty.Members.Count * defenderParty.Members.Count);
            UpdateCombatStatus();

            if (activeCombats == 0)
            {
                OnCombatStateChanged?.Invoke(false);
            }
        }

        /// <summary>
        /// ダメージ表示
        /// </summary>
        private void OnDamageDealt(ICharacter attacker, ICharacter target, float damage, bool isCritical)
        {
            ShowDamageText(target.Position, damage, GetDamageColor(target), isCritical);
            ShowHitEffect(target.Position, isCritical);

            // 体力バーの更新
            UpdateHealthBar(target);
        }

        /// <summary>
        /// 回復表示
        /// </summary>
        private void OnCharacterHealed(ICharacter character, float healAmount)
        {
            ShowDamageText(character.Position, healAmount, healColor, false, true);
            ShowHealEffect(character.Position);

            // 体力バーの更新
            UpdateHealthBar(character);
        }

        /// <summary>
        /// キャラクター死亡時の処理
        /// </summary>
        private void OnCharacterDied(ICharacter character)
        {
            ShowDeathEffect(character.Position);
            HideHealthBar(character);
        }

        /// <summary>
        /// ダメージテキストの表示
        /// </summary>
        private void ShowDamageText(Vector2 worldPosition, float amount, Color color, bool isCritical, bool isHeal = false)
        {
            GameObject damageText = GetPooledDamageText();
            if (damageText == null) return;

            // ワールド座標をスクリーン座標に変換
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
            
            // キャンバス座標に変換
            RectTransform canvasRect = combatCanvas.GetComponent<RectTransform>();
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, screenPosition, combatCanvas.worldCamera, out canvasPosition);

            // ダメージテキストの設定
            RectTransform textRect = damageText.GetComponent<RectTransform>();
            textRect.anchoredPosition = canvasPosition;

            TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = isHeal ? $"+{amount:F0}" : $"-{amount:F0}";
                textComponent.color = color;
                textComponent.fontSize = isCritical ? 48f : 36f;
            }

            damageText.SetActive(true);
            activeDamageTexts.Add(damageText);

            // アニメーション開始
            StartCoroutine(AnimateDamageText(damageText, canvasPosition, isCritical));
        }

        /// <summary>
        /// ダメージテキストのアニメーション
        /// </summary>
        private IEnumerator AnimateDamageText(GameObject damageText, Vector2 startPosition, bool isCritical)
        {
            RectTransform textRect = damageText.GetComponent<RectTransform>();
            TextMeshProUGUI textComponent = damageText.GetComponent<TextMeshProUGUI>();
            
            float elapsed = 0f;
            Vector2 endPosition = startPosition + Vector2.up * damageTextMoveDistance;
            
            // クリティカルの場合は横にも移動
            if (isCritical)
            {
                endPosition += Vector2.right * Random.Range(-50f, 50f);
            }

            while (elapsed < damageTextDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / damageTextDuration;

                // 位置のアニメーション
                textRect.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);

                // フェードアウト
                if (textComponent != null)
                {
                    Color color = textComponent.color;
                    color.a = 1f - t;
                    textComponent.color = color;
                }

                // スケールアニメーション（クリティカル用）
                if (isCritical && t < 0.2f)
                {
                    float scale = 1f + (1f - t / 0.2f) * 0.5f;
                    textRect.localScale = Vector3.one * scale;
                }

                yield return null;
            }

            // プールに戻す
            ReturnDamageTextToPool(damageText);
        }

        /// <summary>
        /// 体力バーの表示
        /// </summary>
        private void ShowHealthBar(ICharacter character)
        {
            if (activeHealthBars.ContainsKey(character)) return;

            GameObject healthBar = Instantiate(healthBarPrefab, healthBarContainer);
            activeHealthBars[character] = healthBar;

            // 体力バーの初期設定
            UpdateHealthBar(character);

            // 自動非表示タイマー開始
            StartCoroutine(AutoHideHealthBar(character, healthBarDuration));
        }

        /// <summary>
        /// 体力バーの更新
        /// </summary>
        private void UpdateHealthBar(ICharacter character)
        {
            if (!activeHealthBars.ContainsKey(character)) return;

            GameObject healthBar = activeHealthBars[character];
            if (healthBar == null) return;

            // ワールド座標をスクリーン座標に変換
            Vector3 worldPosition = new Vector3(character.Position.x, character.Position.y + healthBarOffset, 0);
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

            // 体力バーの位置設定
            RectTransform healthBarRect = healthBar.GetComponent<RectTransform>();
            Vector2 canvasPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                healthBarContainer, screenPosition, combatCanvas.worldCamera, out canvasPosition);
            healthBarRect.anchoredPosition = canvasPosition;

            // 体力バーの値設定
            Slider healthSlider = healthBar.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                healthSlider.value = character.Health / character.MaxHealth;
            }

            // 体力バーの色設定
            Image fillImage = healthSlider?.fillRect?.GetComponent<Image>();
            if (fillImage != null)
            {
                float healthRatio = character.Health / character.MaxHealth;
                fillImage.color = Color.Lerp(Color.red, Color.green, healthRatio);
            }
        }

        /// <summary>
        /// 体力バーの非表示
        /// </summary>
        private void HideHealthBar(ICharacter character)
        {
            if (activeHealthBars.ContainsKey(character))
            {
                GameObject healthBar = activeHealthBars[character];
                if (healthBar != null)
                {
                    Destroy(healthBar);
                }
                activeHealthBars.Remove(character);
            }
        }

        /// <summary>
        /// 体力バーの自動非表示
        /// </summary>
        private IEnumerator AutoHideHealthBar(ICharacter character, float delay)
        {
            yield return new WaitForSeconds(delay);
            HideHealthBar(character);
        }

        /// <summary>
        /// 戦闘エフェクトの表示
        /// </summary>
        private void ShowCombatEffect(Vector2 attackerPosition, Vector2 defenderPosition)
        {
            Vector2 effectPosition = (attackerPosition + defenderPosition) / 2f;
            ShowEffect(combatEffectPrefab, effectPosition);
        }

        /// <summary>
        /// ヒットエフェクトの表示
        /// </summary>
        private void ShowHitEffect(Vector2 position, bool isCritical)
        {
            GameObject effectPrefab = isCritical ? criticalEffectPrefab : hitEffectPrefab;
            ShowEffect(effectPrefab, position);
        }

        /// <summary>
        /// 回復エフェクトの表示
        /// </summary>
        private void ShowHealEffect(Vector2 position)
        {
            ShowEffect(healEffectPrefab, position);
        }

        /// <summary>
        /// 死亡エフェクトの表示
        /// </summary>
        private void ShowDeathEffect(Vector2 position)
        {
            ShowEffect(deathEffectPrefab, position);
        }

        /// <summary>
        /// エフェクトの表示
        /// </summary>
        private void ShowEffect(GameObject effectPrefab, Vector2 worldPosition)
        {
            if (effectPrefab == null) return;

            GameObject effect = GetPooledEffect();
            if (effect == null)
            {
                effect = Instantiate(effectPrefab, transform);
            }

            // ワールド座標に配置
            effect.transform.position = worldPosition;
            effect.SetActive(true);

            // 自動非表示
            StartCoroutine(AutoDestroyEffect(effect, 2f));
        }

        /// <summary>
        /// エフェクトの自動破棄
        /// </summary>
        private IEnumerator AutoDestroyEffect(GameObject effect, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnEffectToPool(effect);
        }

        /// <summary>
        /// 戦闘状況の更新
        /// </summary>
        private void UpdateCombatStatus()
        {
            if (combatStatusPanel != null)
            {
                combatStatusPanel.SetActive(activeCombats > 0);
            }

            if (combatCountText != null)
            {
                combatCountText.text = $"戦闘中: {activeCombats}";
            }
        }

        /// <summary>
        /// 戦闘強度の更新
        /// </summary>
        private void UpdateCombatIntensity()
        {
            float targetIntensity = Mathf.Clamp01(activeCombats / 10f);
            combatIntensity = Mathf.Lerp(combatIntensity, targetIntensity, Time.deltaTime * 2f);

            if (combatIntensityBar != null)
            {
                combatIntensityBar.fillAmount = combatIntensity;
                combatIntensityBar.color = Color.Lerp(lowIntensityColor, highIntensityColor, combatIntensity);
            }
        }

        /// <summary>
        /// 期限切れ要素のクリーンアップ
        /// </summary>
        private void CleanupExpiredElements()
        {
            // 非アクティブなダメージテキストをリストから削除
            activeDamageTexts.RemoveAll(text => text == null || !text.activeInHierarchy);

            // 無効なキャラクターの体力バーを削除
            var keysToRemove = new List<ICharacter>();
            foreach (var kvp in activeHealthBars)
            {
                if (kvp.Key == null || kvp.Value == null)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                activeHealthBars.Remove(key);
            }
        }

        /// <summary>
        /// ダメージ色の取得
        /// </summary>
        private Color GetDamageColor(ICharacter character)
        {
            // キャラクターの種類に応じて色を決定
            if (character is IMonster)
            {
                return monsterDamageColor;
            }
            else if (character is IInvader)
            {
                return invaderDamageColor;
            }
            return Color.white;
        }

        /// <summary>
        /// プールからダメージテキストを取得
        /// </summary>
        private GameObject GetPooledDamageText()
        {
            if (damageTextPool.Count > 0)
            {
                return damageTextPool.Dequeue();
            }
            else if (damageTextPrefab != null)
            {
                return Instantiate(damageTextPrefab, transform);
            }
            return null;
        }

        /// <summary>
        /// ダメージテキストをプールに戻す
        /// </summary>
        private void ReturnDamageTextToPool(GameObject damageText)
        {
            if (damageText != null)
            {
                damageText.SetActive(false);
                activeDamageTexts.Remove(damageText);
                damageTextPool.Enqueue(damageText);
            }
        }

        /// <summary>
        /// プールからエフェクトを取得
        /// </summary>
        private GameObject GetPooledEffect()
        {
            if (effectPool.Count > 0)
            {
                return effectPool.Dequeue();
            }
            return null;
        }

        /// <summary>
        /// エフェクトをプールに戻す
        /// </summary>
        private void ReturnEffectToPool(GameObject effect)
        {
            if (effect != null)
            {
                effect.SetActive(false);
                effectPool.Enqueue(effect);
            }
        }

        /// <summary>
        /// 戦闘中かどうかを返す
        /// </summary>
        public bool IsInCombat()
        {
            return activeCombats > 0;
        }

        /// <summary>
        /// 戦闘強度を返す
        /// </summary>
        public float GetCombatIntensity()
        {
            return combatIntensity;
        }

        private void OnDestroy()
        {
            // イベント購読を解除
            if (CombatManager.Instance != null)
            {
                CombatManager.Instance.OnCombatStarted -= OnCombatStarted;
                CombatManager.Instance.OnCombatEnded -= OnCombatEnded;
                CombatManager.Instance.OnDamageDealt -= OnDamageDealt;
                CombatManager.Instance.OnCharacterHealed -= OnCharacterHealed;
                CombatManager.Instance.OnCharacterDied -= OnCharacterDied;
            }

            if (PartyCombatSystem.Instance != null)
            {
                PartyCombatSystem.Instance.OnPartyCombatStarted -= OnPartyCombatStarted;
                PartyCombatSystem.Instance.OnPartyCombatEnded -= OnPartyCombatEnded;
            }
        }
    }
}