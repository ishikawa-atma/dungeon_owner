using UnityEngine;

namespace DungeonOwner.Interfaces
{
    /// <summary>
    /// モンスターと侵入者の共通基底インターフェース
    /// 戦闘システムで使用する共通プロパティとメソッドを定義
    /// </summary>
    public interface ICharacterBase
    {
        Vector2 Position { get; set; }
        float Health { get; }
        float MaxHealth { get; }
        
        void TakeDamage(float damage);
    }
}