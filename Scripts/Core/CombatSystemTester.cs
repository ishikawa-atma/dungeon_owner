using UnityEngine;
using DungeonOwner.Interfaces;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// リアルタイム戦闘システムのテスト用クラス
    /// 戦闘の動作確認とデバッグ用の機能を提供
    /// </summary>
    public class CombatSystemTester : MonoBehaviour
    {
        [Header("テスト設定")]
        [SerializeField] private bool enableTesting = true;
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private KeyCode testCombatKey = KeyCode.T;
        [SerializeField] private KeyCode spawnTestUnitsKey = KeyCode.Y;

        [Header("テスト用プレハブ")]
        [SerializeField] private GameObject testMonsterPrefab;
        [SerializeField] private GameObject testInvaderPrefab;

        [Header("スポーン位置")]
        [SerializeField] private Transform monsterSpawnPoint;
        [SerializeField] private Transform invaderSpawnPoint;

        private void Update()
        {
            if (!enableTesting) return;

            HandleTestInput();
        }

        private void HandleTestInput()
        {
            // テスト戦闘の開始
            if (Input.GetKeyDown(testCombatKey))
            {
                TestCombatScenario();
            }

            // テストユニットの生成
            if (Input.GetKeyDown(spawnTestUnitsKey))
            {
                SpawnTestUnits();
            }
        }

        /// <summary>
        /// 戦闘シナリオのテスト
        /// </summary>
        private void TestCombatScenario()
        {
            Debug.Log("=== 戦闘システムテスト開始 ===");

            // 既存のモンスターと侵入者を検索
            var monsters = FindObjectsOfType<MonoBehaviour>();
            IMonster testMonster = null;
            IInvader testInvader = null;

            foreach (var obj in monsters)
            {
                if (testMonster == null)
                {
                    testMonster = obj.GetComponent<IMonster>();
                }
                if (testInvader == null)
                {
                    testInvader = obj.GetComponent<IInvader>();
                }

                if (testMonster != null && testInvader != null)
                    break;
            }

            if (testMonster != null && testInvader != null)
            {
                // 戦闘テスト実行
                TestDirectCombat(testMonster, testInvader);
            }
            else
            {
                Debug.LogWarning("テスト用のモンスターまたは侵入者が見つかりません");
                SpawnTestUnits();
            }
        }

        /// <summary>
        /// 直接戦闘のテスト
        /// </summary>
        private void TestDirectCombat(IMonster monster, IInvader invader)
        {
            if (CombatManager.Instance == null)
            {
                Debug.LogError("CombatManagerが見つかりません");
                return;
            }

            Debug.Log($"戦闘テスト: {monster.Type} vs {invader.Type}");
            Debug.Log($"モンスター HP: {monster.Health}/{monster.MaxHealth}");
            Debug.Log($"侵入者 HP: {invader.Health}/{invader.MaxHealth}");

            // 戦闘実行
            CombatManager.Instance.ProcessCombat(monster, invader);
        }

        /// <summary>
        /// テスト用ユニットの生成
        /// </summary>
        private void SpawnTestUnits()
        {
            Vector3 monsterPos = monsterSpawnPoint != null ? monsterSpawnPoint.position : Vector3.left * 2f;
            Vector3 invaderPos = invaderSpawnPoint != null ? invaderSpawnPoint.position : Vector3.right * 2f;

            // テストモンスター生成
            if (testMonsterPrefab != null)
            {
                GameObject monster = Instantiate(testMonsterPrefab, monsterPos, Quaternion.identity);
                monster.name = "TestMonster";
                monster.tag = "Monster";
                Debug.Log("テストモンスターを生成しました");
            }

            // テスト侵入者生成
            if (testInvaderPrefab != null)
            {
                GameObject invader = Instantiate(testInvaderPrefab, invaderPos, Quaternion.identity);
                invader.name = "TestInvader";
                invader.tag = "Invader";
                Debug.Log("テスト侵入者を生成しました");
            }
        }

        /// <summary>
        /// 戦闘統計の表示
        /// </summary>
        private void OnGUI()
        {
            if (!showDebugInfo || !enableTesting) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== 戦闘システムデバッグ ===");

            // システム状態
            bool combatManagerExists = CombatManager.Instance != null;
            bool combatDetectorExists = CombatDetector.Instance != null;
            bool combatEffectsExists = CombatEffects.Instance != null;

            GUILayout.Label($"CombatManager: {(combatManagerExists ? "OK" : "Missing")}");
            GUILayout.Label($"CombatDetector: {(combatDetectorExists ? "OK" : "Missing")}");
            GUILayout.Label($"CombatEffects: {(combatEffectsExists ? "OK" : "Missing")}");

            GUILayout.Space(10);

            // 操作説明
            GUILayout.Label("操作:");
            GUILayout.Label($"{testCombatKey}: 戦闘テスト");
            GUILayout.Label($"{spawnTestUnitsKey}: テストユニット生成");

            GUILayout.Space(10);

            // 現在のユニット数
            var monsters = FindObjectsOfType<MonoBehaviour>();
            int monsterCount = 0;
            int invaderCount = 0;

            foreach (var obj in monsters)
            {
                if (obj.GetComponent<IMonster>() != null) monsterCount++;
                if (obj.GetComponent<IInvader>() != null) invaderCount++;
            }

            GUILayout.Label($"モンスター数: {monsterCount}");
            GUILayout.Label($"侵入者数: {invaderCount}");

            GUILayout.EndArea();
        }

        /// <summary>
        /// 戦闘ログの出力
        /// </summary>
        public void LogCombatEvent(string eventType, string attacker, string defender, float damage)
        {
            if (showDebugInfo)
            {
                Debug.Log($"[戦闘] {eventType}: {attacker} -> {defender} ({damage:F1} ダメージ)");
            }
        }

        /// <summary>
        /// 戦闘範囲の可視化
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!enableTesting) return;

            // スポーン位置の表示
            if (monsterSpawnPoint != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(monsterSpawnPoint.position, 0.5f);
                Gizmos.DrawLine(monsterSpawnPoint.position, monsterSpawnPoint.position + Vector3.up);
            }

            if (invaderSpawnPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(invaderSpawnPoint.position, 0.5f);
                Gizmos.DrawLine(invaderSpawnPoint.position, invaderSpawnPoint.position + Vector3.up);
            }
        }
    }
}