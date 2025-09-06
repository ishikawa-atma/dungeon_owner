namespace DungeonOwner.Interfaces
{
    /// <summary>
    /// 時間スケールに対応するコンポーネントのインターフェース
    /// 要件14.2: 全ゲーム要素への速度適用システム
    /// </summary>
    public interface ITimeScalable
    {
        /// <summary>
        /// 時間スケールを考慮した更新処理
        /// </summary>
        /// <param name="scaledDeltaTime">スケール適用済みのデルタタイム</param>
        /// <param name="timeScale">現在の時間スケール</param>
        void UpdateWithTimeScale(float scaledDeltaTime, float timeScale);
        
        /// <summary>
        /// 時間スケールが変更された時の処理
        /// </summary>
        /// <param name="newTimeScale">新しい時間スケール</param>
        void OnTimeScaleChanged(float newTimeScale);
    }
}