using UnityEngine;
using DungeonOwner.Data;

namespace DungeonOwner.Interfaces
{
    /// <summary>
    /// モンスターアビリティのインターフェース
    /// 要件4.1, 4.4に対応
    /// </summary>
    public interface IMonsterAbility
    {
        /// <summary>アビリティの種類</summary>
        MonsterAbilityType AbilityType { get; }
        
        /// <summary>アビリティの名前</summary>
        string AbilityName { get; }
        
        /// <summary>アビリティの説明</summary>
        string Description { get; }
        
        /// <summary>クールダウン時間（秒）</summary>
        float CooldownTime { get; }
        
        /// <summary>マナコスト</summary>
        float ManaCost { get; }
        
        /// <summary>アビリティが使用可能かどうか</summary>
        bool CanUse { get; }
        
        /// <summary>最後に使用した時間</summary>
        float LastUsedTime { get; }
        
        /// <summary>
        /// アビリティを初期化
        /// </summary>
        /// <param name="owner">アビリティの所有者</param>
        void Initialize(IMonster owner);
        
        /// <summary>
        /// アビリティを実行
        /// </summary>
        /// <returns>実行に成功したかどうか</returns>
        bool Execute();
        
        /// <summary>
        /// アビリティの更新処理（毎フレーム呼ばれる）
        /// </summary>
        void Update();
        
        /// <summary>
        /// アビリティをリセット
        /// </summary>
        void Reset();
        
        /// <summary>
        /// アビリティの視覚的エフェクトを表示
        /// </summary>
        void ShowEffect();
    }
}