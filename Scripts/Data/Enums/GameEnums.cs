namespace DungeonOwner.Data
{
    public enum MonsterType
    {
        // 初期モンスター
        Slime,
        LesserSkeleton,
        LesserGhost,
        LesserGolem,
        Goblin,
        LesserWolf,
        
        // 上位モンスター（階層拡張で解放）
        GreaterSlime,
        Skeleton,
        Ghost,
        Golem,
        Orc,
        Wolf,
        Dragon,
        Lich,
        Demon
    }

    public enum MonsterRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    public enum InvaderType
    {
        // 基本侵入者
        Warrior,
        Mage,
        Rogue,
        Cleric,
        
        // 上位侵入者
        Knight,
        Archmage,
        Assassin,
        HighPriest,
        Paladin,
        Necromancer
    }

    public enum InvaderRank
    {
        Novice,
        Veteran,
        Elite,
        Champion
    }

    public enum PlayerCharacterType
    {
        Warrior,
        Mage,
        Rogue,
        Cleric
    }

    public enum BossType
    {
        DragonLord,
        LichKing,
        DemonGeneral,
        AncientGolem,
        ShadowMaster
    }

    public enum TrapItemType
    {
        SpikeTrap,
        FireTrap,
        IceTrap,
        PoisonTrap,
        LightningTrap
    }

    public enum InvaderState
    {
        Spawning,
        Moving,
        Fighting,
        Retreating,
        Dead
    }

    public enum MonsterState
    {
        Idle,
        Moving,
        Fighting,
        Recovering,
        Dead,
        InShelter
    }
}