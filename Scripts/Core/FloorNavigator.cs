using UnityEngine;
using DungeonOwner.Core;

namespace DungeonOwner.Core
{
    /// <summary>
    /// 階層間の移動を管理するコンポーネント
    /// 侵入者やモンスターの階層移動に使用
    /// </summary>
    public class FloorNavigator : MonoBehaviour
    {
        [Header("Navigation Settings")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private bool isMoving = false;

        public int CurrentFloor { get; private set; } = 1;
        public bool IsMoving => isMoving;

        // イベント
        public System.Action<int, int> OnFloorChanged; // (fromFloor, toFloor)
        public System.Action OnMovementStarted;
        public System.Action OnMovementCompleted;

        private void Start()
        {
            // 初期階層を設定
            if (FloorSystem.Instance != null)
            {
                CurrentFloor = 1;
            }
        }

        /// <summary>
        /// 上の階層に移動
        /// </summary>
        public bool MoveToUpperFloor()
        {
            if (isMoving || CurrentFloor <= 1)
            {
                return false;
            }

            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem not found");
                return false;
            }

            int targetFloor = CurrentFloor - 1;
            Floor currentFloorData = FloorSystem.Instance.GetFloor(CurrentFloor);
            Floor targetFloorData = FloorSystem.Instance.GetFloor(targetFloor);

            if (currentFloorData == null || targetFloorData == null)
            {
                Debug.LogError($"Invalid floor data for movement: {CurrentFloor} -> {targetFloor}");
                return false;
            }

            StartMovement(targetFloor, currentFloorData.upStairPosition, targetFloorData.downStairPosition);
            return true;
        }

        /// <summary>
        /// 下の階層に移動
        /// </summary>
        public bool MoveToLowerFloor()
        {
            if (isMoving)
            {
                return false;
            }

            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem not found");
                return false;
            }

            int targetFloor = CurrentFloor + 1;
            
            // 移動先の階層が存在するかチェック
            if (targetFloor > FloorSystem.Instance.CurrentFloorCount)
            {
                Debug.LogWarning($"Target floor {targetFloor} does not exist");
                return false;
            }

            Floor currentFloorData = FloorSystem.Instance.GetFloor(CurrentFloor);
            Floor targetFloorData = FloorSystem.Instance.GetFloor(targetFloor);

            if (currentFloorData == null || targetFloorData == null)
            {
                Debug.LogError($"Invalid floor data for movement: {CurrentFloor} -> {targetFloor}");
                return false;
            }

            StartMovement(targetFloor, currentFloorData.downStairPosition, targetFloorData.upStairPosition);
            return true;
        }

        /// <summary>
        /// 指定した階層に直接移動（テレポート）
        /// </summary>
        public bool TeleportToFloor(int targetFloor)
        {
            if (isMoving || targetFloor < 1)
            {
                return false;
            }

            if (FloorSystem.Instance == null)
            {
                Debug.LogError("FloorSystem not found");
                return false;
            }

            if (targetFloor > FloorSystem.Instance.CurrentFloorCount)
            {
                Debug.LogWarning($"Target floor {targetFloor} does not exist");
                return false;
            }

            Floor targetFloorData = FloorSystem.Instance.GetFloor(targetFloor);
            if (targetFloorData == null)
            {
                Debug.LogError($"Invalid target floor data: {targetFloor}");
                return false;
            }

            // 現在の階層から除去
            RemoveFromCurrentFloor();

            // 新しい階層に移動
            int previousFloor = CurrentFloor;
            CurrentFloor = targetFloor;

            // 適切な位置に配置（上から来た場合は下り階段、下から来た場合は上り階段）
            Vector2 targetPosition = targetFloor > previousFloor ? 
                targetFloorData.upStairPosition : targetFloorData.downStairPosition;
            
            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);

            // 新しい階層に追加
            AddToCurrentFloor();

            OnFloorChanged?.Invoke(previousFloor, CurrentFloor);
            Debug.Log($"Teleported from floor {previousFloor} to floor {CurrentFloor}");

            return true;
        }

        private void StartMovement(int targetFloor, Vector2 startPos, Vector2 endPos)
        {
            if (isMoving)
            {
                return;
            }

            isMoving = true;
            OnMovementStarted?.Invoke();

            // 移動開始位置に移動
            transform.position = new Vector3(startPos.x, startPos.y, transform.position.z);

            // コルーチンで移動実行
            StartCoroutine(MoveToPosition(targetFloor, endPos));
        }

        private System.Collections.IEnumerator MoveToPosition(int targetFloor, Vector2 targetPos)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            // 現在の階層から除去
            RemoveFromCurrentFloor();

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;

            // 階層変更を完了
            int previousFloor = CurrentFloor;
            CurrentFloor = targetFloor;

            // 新しい階層に追加
            AddToCurrentFloor();

            isMoving = false;
            OnMovementCompleted?.Invoke();
            OnFloorChanged?.Invoke(previousFloor, CurrentFloor);

            Debug.Log($"Moved from floor {previousFloor} to floor {CurrentFloor}");
        }

        private void RemoveFromCurrentFloor()
        {
            if (FloorSystem.Instance == null) return;

            Floor currentFloorData = FloorSystem.Instance.GetFloor(CurrentFloor);
            if (currentFloorData != null)
            {
                // モンスターまたは侵入者として除去
                currentFloorData.RemoveMonster(gameObject);
                currentFloorData.RemoveInvader(gameObject);
            }
        }

        private void AddToCurrentFloor()
        {
            if (FloorSystem.Instance == null) return;

            Floor currentFloorData = FloorSystem.Instance.GetFloor(CurrentFloor);
            if (currentFloorData != null)
            {
                // オブジェクトのタイプに応じて追加
                // この判定は後でインターフェースベースに改善する予定
                if (gameObject.name.Contains("Monster") || gameObject.GetComponent<MonoBehaviour>() != null)
                {
                    currentFloorData.AddMonster(gameObject);
                }
                else if (gameObject.name.Contains("Invader"))
                {
                    currentFloorData.AddInvader(gameObject);
                }
            }
        }

        /// <summary>
        /// 現在位置から最も近い階段位置を取得
        /// </summary>
        public Vector2 GetNearestStairPosition()
        {
            if (FloorSystem.Instance == null) return transform.position;

            Floor currentFloorData = FloorSystem.Instance.GetFloor(CurrentFloor);
            if (currentFloorData == null) return transform.position;

            Vector2 currentPos = transform.position;
            float upStairDistance = Vector2.Distance(currentPos, currentFloorData.upStairPosition);
            float downStairDistance = Vector2.Distance(currentPos, currentFloorData.downStairPosition);

            return upStairDistance < downStairDistance ? 
                currentFloorData.upStairPosition : currentFloorData.downStairPosition;
        }

        /// <summary>
        /// 指定した階段に向かって移動
        /// </summary>
        public void MoveToStair(bool isUpStair)
        {
            if (isMoving || FloorSystem.Instance == null) return;

            Floor currentFloorData = FloorSystem.Instance.GetFloor(CurrentFloor);
            if (currentFloorData == null) return;

            Vector2 targetPos = isUpStair ? currentFloorData.upStairPosition : currentFloorData.downStairPosition;
            StartCoroutine(MoveToPositionSimple(targetPos));
        }

        private System.Collections.IEnumerator MoveToPositionSimple(Vector2 targetPos)
        {
            isMoving = true;
            OnMovementStarted?.Invoke();

            Vector3 startPos = transform.position;
            Vector3 endPos = new Vector3(targetPos.x, targetPos.y, transform.position.z);
            float distance = Vector3.Distance(startPos, endPos);
            float duration = distance / moveSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            transform.position = endPos;
            isMoving = false;
            OnMovementCompleted?.Invoke();
        }
    }
}