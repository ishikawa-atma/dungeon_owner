using UnityEngine;

namespace DungeonOwner.Managers
{
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }
        
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
        
        // 基本的なダミー実装
        public void StartTutorial()
        {
            // チュートリアル開始処理
        }
        
        public void CompleteTutorial()
        {
            // チュートリアル完了処理
        }
        
        public bool IsTutorialActive()
        {
            return false;
        }
    }
}