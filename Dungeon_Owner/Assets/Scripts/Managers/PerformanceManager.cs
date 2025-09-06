using UnityEngine;

namespace DungeonOwner.Managers
{
    public class PerformanceManager : MonoBehaviour
    {
        public static PerformanceManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void OptimizePerformance()
        {
            // パフォーマンス最適化処理
        }
        
        public void ResetPerformance()
        {
            // パフォーマンスリセット処理
        }
        
        public System.Action OnPerformanceSystemsInitialized;
    }
}