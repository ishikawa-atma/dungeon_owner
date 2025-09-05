using UnityEngine;
using System.Collections.Generic;
using DungeonOwner.Core;

namespace DungeonOwner.Invaders
{
    public class InvaderPathfinding : MonoBehaviour
    {
        [Header("Pathfinding Settings")]
        [SerializeField] private float pathUpdateInterval = 0.5f;
        [SerializeField] private float stuckThreshold = 0.1f;
        [SerializeField] private float stuckTime = 2f;
        [SerializeField] private float avoidanceRadius = 1f;

        private List<Vector2> currentPath = new List<Vector2>();
        private int currentPathIndex = 0;
        private Vector2 finalDestination;
        private float lastPathUpdate = 0f;
        private Vector3 lastPosition;
        private float stuckTimer = 0f;

        private BaseInvader invader;

        private void Awake()
        {
            invader = GetComponent<BaseInvader>();
            lastPosition = transform.position;
        }

        private void Update()
        {
            UpdatePathfinding();
            CheckIfStuck();
        }

        private void UpdatePathfinding()
        {
            // 定期的にパスを更新
            if (Time.time > lastPathUpdate + pathUpdateInterval)
            {
                UpdatePath();
                lastPathUpdate = Time.time;
            }

            // 現在のパスに沿って移動
            FollowPath();
        }

        private void CheckIfStuck()
        {
            // スタック判定
            float distance = Vector3.Distance(transform.position, lastPosition);
            if (distance < stuckThreshold)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > stuckTime)
                {
                    // スタックした場合の対処
                    HandleStuckSituation();
                    stuckTimer = 0f;
                }
            }
            else
            {
                stuckTimer = 0f;
                lastPosition = transform.position;
            }
        }

        public void SetDestination(Vector2 destination)
        {
            finalDestination = destination;
            UpdatePath();
        }

        private void UpdatePath()
        {
            if (FloorSystem.Instance == null) return;

            // 現在の階層を取得
            Floor currentFloor = GetCurrentFloor();
            if (currentFloor == null) return;

            // 簡易パスファインディング（直線経路 + 障害物回避）
            currentPath.Clear();
            currentPathIndex = 0;

            Vector2 startPos = transform.position;
            Vector2 endPos = finalDestination;

            // 直線経路をチェック
            if (IsPathClear(startPos, endPos))
            {
                currentPath.Add(endPos);
            }
            else
            {
                // 障害物がある場合は迂回路を計算
                CalculateDetourPath(startPos, endPos, currentFloor);
            }
        }

        private void CalculateDetourPath(Vector2 start, Vector2 end, Floor floor)
        {
            // 簡易的な迂回路計算
            Vector2 direction = (end - start).normalized;
            Vector2 perpendicular = new Vector2(-direction.y, direction.x);

            // 左右に迂回ポイントを作成
            Vector2 detourLeft = start + direction * 2f + perpendicular * 2f;
            Vector2 detourRight = start + direction * 2f - perpendicular * 2f;

            // より良い迂回路を選択
            Vector2 chosenDetour = Vector2.Distance(detourLeft, end) < Vector2.Distance(detourRight, end) 
                ? detourLeft : detourRight;

            // パスに追加
            if (IsPositionValid(chosenDetour, floor))
            {
                currentPath.Add(chosenDetour);
            }
            currentPath.Add(end);
        }

        private void FollowPath()
        {
            if (currentPath.Count == 0 || currentPathIndex >= currentPath.Count)
                return;

            Vector2 targetPoint = currentPath[currentPathIndex];
            Vector2 currentPos = transform.position;

            // 目標点に到達したかチェック
            if (Vector2.Distance(currentPos, targetPoint) < 0.3f)
            {
                currentPathIndex++;
                if (currentPathIndex >= currentPath.Count)
                {
                    // パス完了
                    OnPathCompleted();
                }
            }
            else
            {
                // 目標点に向かって移動
                if (invader != null)
                {
                    invader.Move(targetPoint);
                }
            }
        }

        private bool IsPathClear(Vector2 start, Vector2 end)
        {
            // レイキャストで経路上の障害物をチェック
            Vector2 direction = end - start;
            float distance = direction.magnitude;
            
            RaycastHit2D hit = Physics2D.Raycast(start, direction.normalized, distance, LayerMask.GetMask("Wall", "Monster"));
            return hit.collider == null;
        }

        private bool IsPositionValid(Vector2 position, Floor floor)
        {
            if (floor == null) return false;

            // 階段位置との重複チェック
            if (Vector2.Distance(position, floor.upStairPosition) < 1f ||
                Vector2.Distance(position, floor.downStairPosition) < 1f)
            {
                return false;
            }

            // 壁との重複チェック
            foreach (var wallPos in floor.wallPositions)
            {
                if (Vector2.Distance(position, wallPos) < 1f)
                {
                    return false;
                }
            }

            return true;
        }

        private Floor GetCurrentFloor()
        {
            // 現在位置から階層を判定
            // 簡易実装：FloorSystemから現在表示中の階層を取得
            return FloorSystem.Instance?.GetCurrentFloor();
        }

        private void HandleStuckSituation()
        {
            // スタック時の対処法
            Vector2 randomOffset = Random.insideUnitCircle * 2f;
            Vector2 newDestination = (Vector2)transform.position + randomOffset;
            
            // 新しい目標が有効かチェック
            Floor currentFloor = GetCurrentFloor();
            if (IsPositionValid(newDestination, currentFloor))
            {
                currentPath.Clear();
                currentPath.Add(newDestination);
                currentPath.Add(finalDestination);
                currentPathIndex = 0;
                
                Debug.Log($"{name} was stuck, trying new path");
            }
        }

        private void OnPathCompleted()
        {
            // パス完了時の処理
            Debug.Log($"{name} reached destination");
            
            // 次の目標を設定（階段など）
            SetNextDestination();
        }

        private void SetNextDestination()
        {
            if (FloorSystem.Instance == null) return;

            Floor currentFloor = GetCurrentFloor();
            if (currentFloor != null)
            {
                // 下り階段を次の目標に設定
                SetDestination(currentFloor.downStairPosition);
            }
        }

        // 他の侵入者との衝突回避
        private Vector2 CalculateAvoidance()
        {
            Vector2 avoidanceForce = Vector2.zero;
            
            Collider2D[] nearbyInvaders = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius, LayerMask.GetMask("Invader"));
            
            foreach (var other in nearbyInvaders)
            {
                if (other.gameObject != gameObject)
                {
                    Vector2 direction = ((Vector2)transform.position - (Vector2)other.transform.position).normalized;
                    float distance = Vector2.Distance(transform.position, other.transform.position);
                    float force = 1f / (distance + 0.1f); // 距離に反比例
                    
                    avoidanceForce += direction * force;
                }
            }
            
            return avoidanceForce.normalized;
        }

        // デバッグ用
        private void OnDrawGizmosSelected()
        {
            // 現在のパスを表示
            if (currentPath.Count > 0)
            {
                Gizmos.color = Color.blue;
                Vector3 lastPoint = transform.position;
                
                for (int i = currentPathIndex; i < currentPath.Count; i++)
                {
                    Vector3 point = new Vector3(currentPath[i].x, currentPath[i].y, 0);
                    Gizmos.DrawLine(lastPoint, point);
                    Gizmos.DrawWireSphere(point, 0.2f);
                    lastPoint = point;
                }
            }

            // 最終目標を表示
            if (finalDestination != Vector2.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(new Vector3(finalDestination.x, finalDestination.y, 0), 0.5f);
            }

            // 回避範囲を表示
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCircle(transform.position, avoidanceRadius);
        }
    }
}