using System;

namespace DungeonOwner.Data
{
    /// <summary>
    /// モンスターアビリティの種類（統一版）
    /// 以前の重複定義を統合し、使用箇所の互換性を担保します。
    /// </summary>
    public enum MonsterAbilityType
    {
        None = 0,

        // 基本・パッシブ
        AutoHeal,       // 自動体力回復（スライム等）
        AutoRevive,     // 自動復活（スケルトン/ゴースト等）

        // 汎用特性
        HighDefense,    // 高防御（ゴーレム等）
        FastAttack,     // 高速攻撃（ゴブリン等）
        HighSpeed,      // 高速移動（ウルフ等）

        // アクティブ/攻撃系
        AreaAttack,     // 範囲攻撃（ドラゴン等）
        MagicAttack,    // 魔法攻撃（リッチ等）
        LifeDrain,      // 生命吸収（デーモン等）

        // スキル系（将来/一部データ用）
        BoneStrength,       // 骨の強化（スケルトン）
        PhaseShift,         // フェーズシフト（ゴースト）
        SlimeRegeneration,  // スライム再生
        PartyHeal,          // パーティ回復
        Taunt,              // 挑発
        Stealth             // ステルス
    }
}

