using UnityEngine;
using DungeonOwner.Core;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    /// <summary>
    /// タスク5の要件検証用クラス
    /// 要件3.1, 3.2, 8.2の実装確認
    /// </summary>
    public class RequirementValidator : MonoBehaviour
    {
        [Header("Validation Results")]
        [SerializeField] private bool requirement3_1_Passed = false; // 1階層への自キャラクター+14体モンスターの初期配置
        [SerializeField] private bool requirement3_2_Passed = false; // 具体的なモンスター構成の実装
        [SerializeField] private bool requirement8_2_Passed = false; // ゲーム開始時の状態初期化
        
        [ContextMenu("Validate All Requirements")]
        public void ValidateAllRequirements()
        {
            Debug.Log("=== Validating Task 5 Requirements ===");
            
            ValidateRequirement3_1();
            ValidateRequirement3_2();
            ValidateRequirement8_2();
            
            PrintValidationSummary();
        }

        /// <summary>
        /// 要件3.1: 1階層への自キャラクター+14体モンスターの初期配置
        /// </summary>
        private void ValidateRequirement3_1()
        {
            Debug.Log("Validating Requirement 3.1: Initial placement of player character + 14 monsters on floor 1");
            
            requirement3_1_Passed = false;
            
            // FloorSystemの確認
            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem not found");
                return;
            }
            
            // 1階層の確認
            Floor firstFloor = FloorSystem.Instance.GetFloor(1);
            if (firstFloor == null)
            {
                Debug.LogError("First floor not found");
                return;
            }
            
            // 配置されたオブジェクト数の確認
            int placedCount = firstFloor.placedMonsters?.Count ?? 0;
            if (placedCount == 15) // プレイヤーキャラクター1 + モンスター14
            {
                Debug.Log($"✅ Requirement 3.1 PASSED: {placedCount} entities placed on floor 1");
                requirement3_1_Passed = true;
            }
            else
            {
                Debug.LogError($"❌ Requirement 3.1 FAILED: {placedCount} entities placed (expected: 15)");
            }
        }

        /// <summary>
        /// 要件3.2: 具体的なモンスター構成（スライム5、スケルトン3等）の実装
        /// </summary>
        private void ValidateRequirement3_2()
        {
            Debug.Log("Validating Requirement 3.2: Specific monster composition implementation");
            
            requirement3_2_Passed = false;
            
            InitialMonsterPlacer placer = FindObjectOfType<InitialMonsterPlacer>();
            if (placer == null)
            {
                Debug.LogError("InitialMonsterPlacer not found");
                return;
            }
            
            if (!placer.IsPlacementComplete)
            {
                Debug.LogError("Monster placement not completed");
                return;
            }
            
            // 配置されたモンスターの確認
            var placedMonsters = placer.GetPlacedMonsters();
            var playerCharacter = placer.GetPlacedPlayerCharacter();
            
            if (playerCharacter == null)
            {
                Debug.LogError("Player character not placed");
                return;
            }
            
            if (placedMonsters.Count != 14)
            {
                Debug.LogError($"Incorrect monster count: {placedMonsters.Count} (expected: 14)");
                return;
            }
            
            // モンスタータイプの分布確認
            var monsterTypes = CountMonsterTypes(placedMonsters);
            
            bool compositionCorrect = 
                monsterTypes.GetValueOrDefault(MonsterType.Slime, 0) == 5 &&
                monsterTypes.GetValueOrDefault(MonsterType.LesserSkeleton, 0) == 3 &&
                monsterTypes.GetValueOrDefault(MonsterType.LesserGhost, 0) == 3 &&
                monsterTypes.GetValueOrDefault(MonsterType.LesserGolem, 0) == 1 &&
                monsterTypes.GetValueOrDefault(MonsterType.Goblin, 0) == 1 &&
                monsterTypes.GetValueOrDefault(MonsterType.LesserWolf, 0) == 1;
            
            if (compositionCorrect)
            {
                Debug.Log("✅ Requirement 3.2 PASSED: Correct monster composition");
                Debug.Log($"   - Slimes: {monsterTypes.GetValueOrDefault(MonsterType.Slime, 0)}");
                Debug.Log($"   - Lesser Skeletons: {monsterTypes.GetValueOrDefault(MonsterType.LesserSkeleton, 0)}");
                Debug.Log($"   - Lesser Ghosts: {monsterTypes.GetValueOrDefault(MonsterType.LesserGhost, 0)}");
                Debug.Log($"   - Lesser Golems: {monsterTypes.GetValueOrDefault(MonsterType.LesserGolem, 0)}");
                Debug.Log($"   - Goblins: {monsterTypes.GetValueOrDefault(MonsterType.Goblin, 0)}");
                Debug.Log($"   - Lesser Wolves: {monsterTypes.GetValueOrDefault(MonsterType.LesserWolf, 0)}");
                requirement3_2_Passed = true;
            }
            else
            {
                Debug.LogError("❌ Requirement 3.2 FAILED: Incorrect monster composition");
                foreach (var kvp in monsterTypes)
                {
                    Debug.LogError($"   - {kvp.Key}: {kvp.Value}");
                }
            }
        }

        /// <summary>
        /// 要件8.2: ゲーム開始時の状態初期化
        /// </summary>
        private void ValidateRequirement8_2()
        {
            Debug.Log("Validating Requirement 8.2: Game start state initialization");
            
            requirement8_2_Passed = false;
            
            // DungeonInitializerの確認
            DungeonInitializer initializer = FindObjectOfType<DungeonInitializer>();
            if (initializer == null)
            {
                Debug.LogError("DungeonInitializer not found");
                return;
            }
            
            // 初期化完了の確認
            if (!initializer.IsInitialized)
            {
                Debug.LogError("Dungeon initialization not completed");
                return;
            }
            
            // ゲーム開始準備の確認
            if (!initializer.IsReadyToStart())
            {
                Debug.LogError("Game not ready to start");
                return;
            }
            
            // GameManagerの状態確認
            if (GameManager.Instance == null)
            {
                Debug.LogError("GameManager not found");
                return;
            }
            
            // FloorSystemの状態確認
            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem not found");
                return;
            }
            
            // 1階層が表示状態になっているか確認
            if (FloorSystem.Instance.CurrentViewFloor != 1)
            {
                Debug.LogError($"Current view floor is {FloorSystem.Instance.CurrentViewFloor} (expected: 1)");
                return;
            }
            
            Debug.Log("✅ Requirement 8.2 PASSED: Game start state properly initialized");
            Debug.Log($"   - Dungeon initialized: {initializer.IsInitialized}");
            Debug.Log($"   - Monster placement complete: {initializer.IsMonsterPlacementComplete}");
            Debug.Log($"   - Ready to start: {initializer.IsReadyToStart()}");
            Debug.Log($"   - Current view floor: {FloorSystem.Instance.CurrentViewFloor}");
            
            requirement8_2_Passed = true;
        }

        private System.Collections.Generic.Dictionary<MonsterType, int> CountMonsterTypes(System.Collections.Generic.List<GameObject> monsters)
        {
            var counts = new System.Collections.Generic.Dictionary<MonsterType, int>();
            
            foreach (var monster in monsters)
            {
                if (monster == null) continue;
                
                // BaseMonsterコンポーネントからタイプを取得
                var baseMonster = monster.GetComponent<DungeonOwner.Monsters.BaseMonster>();
                if (baseMonster != null)
                {
                    MonsterType type = baseMonster.Type;
                    counts[type] = counts.GetValueOrDefault(type, 0) + 1;
                }
            }
            
            return counts;
        }

        private void PrintValidationSummary()
        {
            Debug.Log("\n=== Task 5 Validation Summary ===");
            Debug.Log($"Requirement 3.1 (Initial Placement): {(requirement3_1_Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Requirement 3.2 (Monster Composition): {(requirement3_2_Passed ? "PASSED" : "FAILED")}");
            Debug.Log($"Requirement 8.2 (State Initialization): {(requirement8_2_Passed ? "PASSED" : "FAILED")}");
            
            bool allPassed = requirement3_1_Passed && requirement3_2_Passed && requirement8_2_Passed;
            Debug.Log($"\nOverall Task 5 Status: {(allPassed ? "✅ COMPLETED" : "❌ INCOMPLETE")}");
            
            if (!allPassed)
            {
                Debug.LogWarning("Some requirements are not met. Please check the implementation.");
            }
        }

        // ゲッター
        public bool AreAllRequirementsPassed()
        {
            return requirement3_1_Passed && requirement3_2_Passed && requirement8_2_Passed;
        }

        public bool IsRequirement3_1Passed() => requirement3_1_Passed;
        public bool IsRequirement3_2Passed() => requirement3_2_Passed;
        public bool IsRequirement8_2Passed() => requirement8_2_Passed;
    }
}