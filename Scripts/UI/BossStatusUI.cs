using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DungeonOwner.Data;
using DungeonOwner.Interfaces;
using DungeonOwner.Managers;
using DungeonOwner.Core;

namespace DungeonOwner.UI
{
    /// <summary>
    /// ボスキャラクターの状態表示UIコンポーネント
    /// リポップ進行度、レベル、ステータスを表示
    /// </summary>
    public class BossStatusUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject statusPanel;
        [SerializeField] private Transform bossStatusContainer;
        [SerializeField] private GameObject bossStatusItemPrefab;

        [Header("Individual Boss Status")]
        [SerializeField] private GameObject individualStatusPanel;
        [SerializeField] private Image bossIcon;
        [SerializeField] private Text bossNameText;
        [SerializeField] private Text bossLevelText;
        [SerializeField] private Text floorText;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Slider manaSlider;
        [SerializeField] private Text manaText;
        [SerializeField] private GameObject respawnPanel;
        [SerializeField] private Slider respawnSlider;
        [SerializeField] private Text respawnText;
        [SerializeField] private Button closeButton;

        // 内部状態
        private Dictionary<int, GameObject> bossStatusItems = new Dictionary<int, GameObject>();
        private IBoss currentDisplayedBoss = null;
        private int currentDisplayedFloor = -1;

        private void Start()
        {
            InitializeUI();
            SetupEventListeners();
        }

        private void InitializeUI()
        {
            // 初期状態では非表示
            if (statusPanel != null)
            {
                statusPanel.SetActive(false);
            }

            if (individualStatusPanel != null)
            {
                individualStatusPanel.SetActive(false);
            }
        }

        private void SetupEventListeners()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseIndividualStatus);
            }

            // BossManagerのイベントに登録
            if (BossManager.Instance != null)
            {
                BossManager.Instance.OnBossPlaced += OnBossPlaced;
                BossManager.Instance.OnBossDefeated += OnBossDefeated;
                BossManager.Instance.OnBossRespawned += OnBossRespawned;
            }

            // FloorSystemのイベントに登録
            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged += OnFloorChanged;
            }
        }

        /// <summary>
        /// ボス状態パネルを表示/非表示
        /// </summary>
        public void ToggleStatusPanel()
        {
            if (statusPanel != null)
            {
                bool isActive = statusPanel.activeSelf;
                statusPanel.SetActive(!isActive);

                if (!isActive)
                {
                    RefreshBossStatusList();
                }
            }
        }

        /// <summary>
        /// ボス状態リストを更新
        /// </summary>
        private void RefreshBossStatusList()
        {
            if (BossManager.Instance == null || bossStatusContainer == null)
            {
                return;
            }

            // 既存のアイテムをクリア
            ClearBossStatusItems();

            // アクティブなボスの状態を表示
            List<IBoss> activeBosses = BossManager.Instance.GetActiveBosses();

            foreach (IBoss boss in activeBosses)
            {
                // ボスがどの階層にいるかを検索
                int floorIndex = FindBossFloor(boss);
                if (floorIndex > 0)
                {
                    CreateBossStatusItem(boss, floorIndex);
                }
            }
        }

        /// <summary>
        /// ボスがいる階層を検索
        /// </summary>
        private int FindBossFloor(IBoss boss)
        {
            if (BossManager.Instance == null) return -1;

            // 全階層をチェック
            for (int i = 1; i <= (FloorSystem.Instance?.CurrentFloorCount ?? 0); i++)
            {
                IBoss floorBoss = BossManager.Instance.GetBossOnFloor(i);
                if (floorBoss == boss)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// ボス状態アイテムを作成
        /// </summary>
        private void CreateBossStatusItem(IBoss boss, int floorIndex)
        {
            if (bossStatusItemPrefab == null) return;

            GameObject itemObj = Instantiate(bossStatusItemPrefab, bossStatusContainer);
            bossStatusItems[floorIndex] = itemObj;

            // ボス情報を設定
            UpdateBossStatusItem(itemObj, boss, floorIndex);

            // クリックイベントを設定
            Button itemButton = itemObj.GetComponent<Button>();
            if (itemButton != null)
            {
                itemButton.onClick.AddListener(() => ShowIndividualBossStatus(boss, floorIndex));
            }
        }

        /// <summary>
        /// ボス状態アイテムを更新
        /// </summary>
        private void UpdateBossStatusItem(GameObject itemObj, IBoss boss, int floorIndex)
        {
            if (itemObj == null || boss == null) return;

            // アイコン
            Image iconImage = itemObj.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null && BossManager.Instance != null)
            {
                BossData bossData = BossManager.Instance.GetBossData(boss.BossType);
                if (bossData != null && bossData.icon != null)
                {
                    iconImage.sprite = bossData.icon;
                    iconImage.color = bossData.bossColor;
                }
            }

            // 階層表示
            Text floorText = itemObj.transform.Find("Floor")?.GetComponent<Text>();
            if (floorText != null)
            {
                floorText.text = $"{floorIndex}F";
            }

            // 名前
            Text nameText = itemObj.transform.Find("Name")?.GetComponent<Text>();
            if (nameText != null)
            {
                nameText.text = boss.BossType.ToString();
            }

            // レベル
            Text levelText = itemObj.transform.Find("Level")?.GetComponent<Text>();
            if (levelText != null)
            {
                levelText.text = $"Lv.{boss.Level}";
            }

            // 状態表示
            Text statusText = itemObj.transform.Find("Status")?.GetComponent<Text>();
            GameObject respawnIndicator = itemObj.transform.Find("RespawnIndicator")?.gameObject;

            if (boss.IsRespawning)
            {
                if (statusText != null)
                {
                    statusText.text = "リポップ中";
                    statusText.color = Color.yellow;
                }

                if (respawnIndicator != null)
                {
                    respawnIndicator.SetActive(true);
                    Slider progressSlider = respawnIndicator.GetComponent<Slider>();
                    if (progressSlider != null)
                    {
                        progressSlider.value = boss.RespawnProgress;
                    }
                }
            }
            else
            {
                if (statusText != null)
                {
                    statusText.text = "アクティブ";
                    statusText.color = Color.green;
                }

                if (respawnIndicator != null)
                {
                    respawnIndicator.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 個別ボス状態を表示
        /// </summary>
        private void ShowIndividualBossStatus(IBoss boss, int floorIndex)
        {
            if (individualStatusPanel == null || boss == null) return;

            currentDisplayedBoss = boss;
            currentDisplayedFloor = floorIndex;

            individualStatusPanel.SetActive(true);

            // ボス情報を更新
            UpdateIndividualBossStatus();
        }

        /// <summary>
        /// 個別ボス状態を更新
        /// </summary>
        private void UpdateIndividualBossStatus()
        {
            if (currentDisplayedBoss == null) return;

            BossData bossData = BossManager.Instance?.GetBossData(currentDisplayedBoss.BossType);

            // アイコン
            if (bossIcon != null && bossData != null && bossData.icon != null)
            {
                bossIcon.sprite = bossData.icon;
                bossIcon.color = bossData.bossColor;
            }

            // 名前
            if (bossNameText != null)
            {
                string displayName = bossData?.displayName ?? currentDisplayedBoss.BossType.ToString();
                bossNameText.text = displayName;
            }

            // レベル
            if (bossLevelText != null)
            {
                bossLevelText.text = $"レベル {currentDisplayedBoss.Level}";
            }

            // 階層
            if (floorText != null)
            {
                floorText.text = $"{currentDisplayedFloor}階層";
            }

            // HP
            if (healthSlider != null)
            {
                healthSlider.value = currentDisplayedBoss.Health / currentDisplayedBoss.MaxHealth;
            }

            if (healthText != null)
            {
                healthText.text = $"{currentDisplayedBoss.Health:F0} / {currentDisplayedBoss.MaxHealth:F0}";
            }

            // MP
            if (manaSlider != null)
            {
                manaSlider.value = currentDisplayedBoss.Mana / currentDisplayedBoss.MaxMana;
            }

            if (manaText != null)
            {
                manaText.text = $"{currentDisplayedBoss.Mana:F0} / {currentDisplayedBoss.MaxMana:F0}";
            }

            // リポップ状態
            if (respawnPanel != null)
            {
                respawnPanel.SetActive(currentDisplayedBoss.IsRespawning);

                if (currentDisplayedBoss.IsRespawning)
                {
                    if (respawnSlider != null)
                    {
                        respawnSlider.value = currentDisplayedBoss.RespawnProgress;
                    }

                    if (respawnText != null)
                    {
                        float remainingTime = currentDisplayedBoss.RespawnTime * (1f - currentDisplayedBoss.RespawnProgress);
                        int minutes = Mathf.FloorToInt(remainingTime / 60f);
                        int seconds = Mathf.FloorToInt(remainingTime % 60f);
                        respawnText.text = $"リポップまで {minutes:D2}:{seconds:D2}";
                    }
                }
            }
        }

        /// <summary>
        /// 個別ボス状態を閉じる
        /// </summary>
        private void CloseIndividualStatus()
        {
            if (individualStatusPanel != null)
            {
                individualStatusPanel.SetActive(false);
            }

            currentDisplayedBoss = null;
            currentDisplayedFloor = -1;
        }

        /// <summary>
        /// ボス状態アイテムをクリア
        /// </summary>
        private void ClearBossStatusItems()
        {
            foreach (GameObject item in bossStatusItems.Values)
            {
                if (item != null)
                {
                    Destroy(item);
                }
            }
            bossStatusItems.Clear();
        }

        /// <summary>
        /// ボス配置時の処理
        /// </summary>
        private void OnBossPlaced(IBoss boss, int floorIndex)
        {
            if (statusPanel != null && statusPanel.activeSelf)
            {
                RefreshBossStatusList();
            }
        }

        /// <summary>
        /// ボス撃破時の処理
        /// </summary>
        private void OnBossDefeated(IBoss boss, int floorIndex)
        {
            // 状態アイテムを更新
            if (bossStatusItems.ContainsKey(floorIndex))
            {
                UpdateBossStatusItem(bossStatusItems[floorIndex], boss, floorIndex);
            }

            // 個別表示中の場合は更新
            if (currentDisplayedBoss == boss)
            {
                UpdateIndividualBossStatus();
            }
        }

        /// <summary>
        /// ボスリポップ時の処理
        /// </summary>
        private void OnBossRespawned(IBoss boss, int floorIndex)
        {
            // 状態アイテムを更新
            if (bossStatusItems.ContainsKey(floorIndex))
            {
                UpdateBossStatusItem(bossStatusItems[floorIndex], boss, floorIndex);
            }

            // 個別表示中の場合は更新
            if (currentDisplayedBoss == boss)
            {
                UpdateIndividualBossStatus();
            }
        }

        /// <summary>
        /// 階層変更時の処理
        /// </summary>
        private void OnFloorChanged(int floorIndex)
        {
            // 現在の階層のボス状態を強調表示するなどの処理
            // 必要に応じて実装
        }

        private void Update()
        {
            // 個別ボス状態表示中の場合、リアルタイム更新
            if (currentDisplayedBoss != null && individualStatusPanel != null && individualStatusPanel.activeSelf)
            {
                UpdateIndividualBossStatus();
            }

            // リスト表示中の場合、リポップ進行度を更新
            if (statusPanel != null && statusPanel.activeSelf)
            {
                UpdateRespawnProgress();
            }
        }

        /// <summary>
        /// リポップ進行度を更新
        /// </summary>
        private void UpdateRespawnProgress()
        {
            foreach (var kvp in bossStatusItems)
            {
                int floorIndex = kvp.Key;
                GameObject itemObj = kvp.Value;

                if (itemObj != null && BossManager.Instance != null)
                {
                    IBoss boss = BossManager.Instance.GetBossOnFloor(floorIndex);
                    if (boss != null && boss.IsRespawning)
                    {
                        GameObject respawnIndicator = itemObj.transform.Find("RespawnIndicator")?.gameObject;
                        if (respawnIndicator != null)
                        {
                            Slider progressSlider = respawnIndicator.GetComponent<Slider>();
                            if (progressSlider != null)
                            {
                                progressSlider.value = boss.RespawnProgress;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 外部からボス状態パネルを開く
        /// </summary>
        public void OpenBossStatusPanel()
        {
            if (statusPanel != null)
            {
                statusPanel.SetActive(true);
                RefreshBossStatusList();
            }
        }

        /// <summary>
        /// 外部からボス状態パネルを閉じる
        /// </summary>
        public void CloseBossStatusPanel()
        {
            if (statusPanel != null)
            {
                statusPanel.SetActive(false);
            }

            CloseIndividualStatus();
        }

        private void OnDestroy()
        {
            // イベント登録解除
            if (BossManager.Instance != null)
            {
                BossManager.Instance.OnBossPlaced -= OnBossPlaced;
                BossManager.Instance.OnBossDefeated -= OnBossDefeated;
                BossManager.Instance.OnBossRespawned -= OnBossRespawned;
            }

            if (FloorSystem.Instance != null)
            {
                FloorSystem.Instance.OnFloorChanged -= OnFloorChanged;
            }

            // アイテムをクリア
            ClearBossStatusItems();
        }
    }
}