using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Data;

namespace DungeonOwner.Core
{
    [System.Serializable]
    public class Floor
    {
        [Header("Floor Configuration")]
        public int floorIndex;
        public Vector2 upStairPosition;
        public Vector2 downStairPosition;
        public List<Vector2> wallPositions;
        public BossType? bossType;
        public int bossLevel;

        [Header("Floor State")]
        public List<GameObject> placedMonsters;
        public List<GameObject> activeInvaders;
        public bool hasCore; // 最深部のコアがあるかどうか

        public Floor(int index)
        {
            floorIndex = index;
            placedMonsters = new List<GameObject>();
            activeInvaders = new List<GameObject>();
            wallPositions = new List<Vector2>();
            bossType = null;
            bossLevel = 1;
            hasCore = false;
            
            // デフォルトの階段位置を設定
            SetDefaultStairPositions();
        }

        private void SetDefaultStairPositions()
        {
            // 上り階段は左上、下り階段は右下に配置
            upStairPosition = new Vector2(-4f, 4f);
            downStairPosition = new Vector2(4f, -4f);
        }

        public bool CanPlaceMonster(Vector2 position)
        {
            // 階段位置との重複チェック
            if (Vector2.Distance(position, upStairPosition) < 1f ||
                Vector2.Distance(position, downStairPosition) < 1f)
            {
                return false;
            }

            // 壁との重複チェック
            foreach (var wallPos in wallPositions)
            {
                if (Vector2.Distance(position, wallPos) < 1f)
                {
                    return false;
                }
            }

            // 既存モンスターとの重複チェック
            foreach (var monster in placedMonsters)
            {
                if (monster != null && Vector2.Distance(position, monster.transform.position) < 1f)
                {
                    return false;
                }
            }

            // モンスター数制限チェック（15体まで）
            int activeMonsterCount = 0;
            foreach (var monster in placedMonsters)
            {
                if (monster != null)
                {
                    activeMonsterCount++;
                }
            }

            return activeMonsterCount < 15;
        }

        public void AddMonster(GameObject monster)
        {
            if (monster != null && !placedMonsters.Contains(monster))
            {
                placedMonsters.Add(monster);
            }
        }

        public void RemoveMonster(GameObject monster)
        {
            if (monster != null)
            {
                placedMonsters.Remove(monster);
            }
        }

        public void AddInvader(GameObject invader)
        {
            if (invader != null && !activeInvaders.Contains(invader))
            {
                activeInvaders.Add(invader);
            }
        }

        public void RemoveInvader(GameObject invader)
        {
            if (invader != null)
            {
                activeInvaders.Remove(invader);
            }
        }

        public bool IsEmpty()
        {
            // モンスターと侵入者が存在しない場合は空
            int activeMonsters = 0;
            foreach (var monster in placedMonsters)
            {
                if (monster != null) activeMonsters++;
            }

            int activeInvaderCount = 0;
            foreach (var invader in activeInvaders)
            {
                if (invader != null) activeInvaderCount++;
            }

            return activeMonsters == 0 && activeInvaderCount == 0;
        }

        public void SetBoss(BossType boss, int level)
        {
            bossType = boss;
            bossLevel = level;
        }

        public void RemoveBoss()
        {
            bossType = null;
            bossLevel = 1;
        }

        public bool IsBossFloor()
        {
            return bossType.HasValue;
        }

        public void SetAsCore()
        {
            hasCore = true;
        }

        /// <summary>
        /// 現在のモンスター数を取得
        /// 退避スポットシステム用
        /// </summary>
        public int GetMonsterCount()
        {
            int count = 0;
            foreach (var monster in placedMonsters)
            {
                if (monster != null) count++;
            }
            return count;
        }

        /// <summary>
        /// 最大モンスター数を取得
        /// </summary>
        public int maxMonsters => 15;

        /// <summary>
        /// 配置可能な位置のリストを取得
        /// 退避スポットシステム用
        /// </summary>
        public List<Vector2> GetAvailablePositions()
        {
            List<Vector2> availablePositions = new List<Vector2>();
            
            // グリッド範囲内で配置可能な位置を検索
            for (int x = -5; x <= 5; x++)
            {
                for (int y = -5; y <= 5; y++)
                {
                    Vector2 position = new Vector2(x, y);
                    if (CanPlaceMonster(position))
                    {
                        availablePositions.Add(position);
                    }
                }
            }
            
            return availablePositions;
        }

        /// <summary>
        /// 指定されたモンスターがこの階層に配置されているかチェック
        /// </summary>
        public bool HasMonster(GameObject monster)
        {
            return monster != null && placedMonsters.Contains(monster);
        }
    }
}