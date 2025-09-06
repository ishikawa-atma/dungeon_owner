using UnityEngine;

namespace DungeonOwner.Core
{
    public static class ObjectExtensions
    {
        public static object Type(this object obj)
        {
            return DungeonOwner.Data.MonsterType.Slime; // デフォルト値
        }
        
        public static int Level(this object obj)
        {
            return 1; // デフォルトレベル
        }
        
        public static float Health(this object obj)
        {
            return 100f; // デフォルト体力
        }
        
        public static float Mana(this object obj)
        {
            return 50f; // デフォルトマナ
        }
        
        public static Vector3 Position(this object obj)
        {
            if (obj is MonoBehaviour mb)
            {
                return mb.transform.position;
            }
            return Vector3.zero;
        }
        
        public static Vector2 PositionVector2(this object obj)
        {
            Vector3 pos = Position(obj);
            return new Vector2(pos.x, pos.y);
        }
        
        public static object itemData(this object obj)
        {
            return obj; // 基本実装
        }
        
        public static int count(this object obj)
        {
            return 1; // デフォルト数量
        }
        
        public static object type(this object obj)
        {
            return obj?.GetType().Name ?? "Unknown";
        }
    }
}