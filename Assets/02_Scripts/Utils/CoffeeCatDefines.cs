using UnityEngine;

namespace CoffeeCat.Utils.Defines
{
    public static class Defines {
        // Defines
        public const int SPAWN_MONSTER_MAX_COUNT = 30;
        public const int PLAYER_SKILL_SELECT_COUNT = 3;
        public const float PLAYER_AREA_SKILL_VECTOR_X = 10;
        public const float PLAYER_AREA_SKILL_VECTOR_Y = 7;
        public const string PLAYER_ENHANCE_DATA_KEY = "PlayerEnhanceData";
        
        // Tags
        public const string TAG_UICAM = "UICamera";
        
        // Layers
        public const string LAYER_PLAYER = "Player";
        public const string LAYER_ITEM_AQUISITION_COLLIDER = "ItemAquisition";
        
        // Encrypt/Decrypt
        public static readonly string ENC_KEY = "m71a12x28";
        
        // Paths
        public const string ITEM_EQUIPMENT_JSON_PATH  = "Entity/Json/ItemsEquipment";
        public const string ITEM_CONSUMABLE_JSON_PATH = "Entity/Json/ItemsConsumable";
        public const string ITEM_RESOURCE_JSON_PATH   = "Entity/Json/ItemsResource";
        public const string MONSTER_STAT_JSON_PATH    = "Entity/Json/MonsterStat";
        public const string MONSTER_SKILL_JSON_PATH   = "Entity/Json/MonsterSkillStat";
        public const string PLAYER_STAT_JSON_PATH     = "Entity/Json/PlayerStat";
        public const string PLAYER_SKILL_JSON_PATH    = "Entity/Json/PlayerSkill";
        public const string STATUS_EFFECT_JSON_PATH   = "Entity/Json/StatusEffect";
        
        public const string DATA_PATH_DUNGEON_INFO = "Entity/ScriptableObject/Dungeon_Information";
        public const string DATA_PATH_CAHRA_LEVEL = "Entity/ScriptableObject/PlayerLevelData";
        public const string DATA_PATH_CHARA_SELECT = "Entity/ScriptableObject/CharacterSelectData";
        public const string DATA_PATH_ABILITY = "Entity/ScriptableObject/PlayerAbilityData";
        public const string DATA_PATH_CHARA_STAT_0 = "Entity/ScriptableObject/UpgradeData_Flower";
        public const string DATA_PATH_CHARA_STAT_1 = "Entity/ScriptableObject/UpgradeData_Flame";
        public const string DATA_PATH_CHARA_STAT_2 = "Entity/ScriptableObject/UpgradeData_Clown";
        
        // Files
        private const string USER_DATA_FILE_NAME = "/UserData.enc";
        private const string USER_SETTINGS_FILE_NAME = "/UserSettings.enc";
        public static readonly string USER_DATA_PATH = Application.persistentDataPath + USER_DATA_FILE_NAME;
        public static readonly string USER_SETTINGS_PATH = Application.persistentDataPath + USER_SETTINGS_FILE_NAME;
    }
    
    #region GLOBAL ENUMS

    public enum SceneName
    {
        NONE,
        LoadingScene,
        DungeonScene,
        TownScene,
        BossScene_T1_E,
        BossScene_T1_N,
        BossScene_T1_H,
    }

    public enum AddressablesKey
    {
        NONE,
        Effect_hit_1,
        Effect_hit_2,
        Effect_hit_3,
        Monster_Skeleton,
        Monster_Skeleton_Warrior,
        Monster_Skeleton_Mage,
        Skeleton_Mage_Projectile_Default,
        Skeleton_Mage_Projectile_Skill,
        GroupSpawnPositions,
        InteractableSign,
        InputCanvas,
        DungeonInfoSO,
        HealthBar,
        Object_Item,
        Object_Start_Room,
        GateObject,
    }

    public enum SoundKey {
        None,
        // BGM
        BGM_Dungeon,
        BGM_Town,
        // SE: UI
        Button_0,
        Button_1,
        // SE: ITEMS
        Currency_0,
        Currency_1,
        Currency_2,
        EXP_0,
        EXP_1,
        EXP_2,
        Item_Consumable,
        Item_Equipment,
        Currency_Used,
        Dropped_Item,
        // SE: PLAYER
        Plaeyr_Enhanced,
        Player_Evasion_0,
        Player_Evasion_1,
        Player_Hit_0,
        Player_Moving,
        // SE: MONSTER
        Monster_Skel_Hit_0,
        Monster_Skel_Hit_1,
    }

    public enum PlayerAddressablesKey
    {
        NONE,
        Flower_Dungeon,
        Flame_Dungeon,
        Clown_Dungeon,
        Flower_Town,
        Flame_Town,
        Clown_Town,
    }

    public enum PlayerNormalAttackKey
    {
        NONE,
        NormalAttack_Flower,
        NormalAttack_Flame,
        NormalAttack_Clown,
    }

    public enum Layer
    {
        NONE,
        PLAYER,
    }

    public enum Tag
    {
        NONE,
    }

    public enum ProjectileKey
    {
        NONE,
        monster_attack_fireball,
    }

    public enum ProjectileSkillKey
    {
        NONE,
        monster_skill_expsphere,
    }

    public enum InteractionType
    {
        None,
        DungeonNextFloor,
        DungeonShop,
        DungeonReward,
        DungeonBoss,
        EntranceTown,
        EntranceDungeon,
        PlayerUpgrade,
        PlayerAbility,
        PlayerSelect,
    }

    public enum StatusEffectType {
        None   = 0,
        Poison = 1000,
        Bleed  = 1001,
        Stun   = 1002,
    }

    public enum JoyStickType 
    {
        Static,
        Dynamic
    }

    public enum LootCategory {
        None = 0, // *none
        T0 = 1,   // common
        T1 = 2,   // common
        T2 = 3,   // common
        T3 = 4,   // common
        T4 = 5,   // common
        T5 = 6,   // common
        TB = 99,  // *boss
    }

    public enum RoomGradeType {
        T0 = 0,
        T1 = 1,
        T2 = 2,
        T3 = 3,
    }

    public enum StartRoomItemPosition {
        D0,
        D1,
        D2,
        D3,
    }

    public enum ItemCode {
        // none
        None = -1,

        // resources   1000~
        Currency = 1000,

        // consumables 2000~
        SmallHeal  = 2000,
        MediumHeal = 2001,
        LargeHeal  = 2002,
        ExpForced  = 2003,
        ExpFull    = 2004,
        ExpSmall   = 2005,
        ExpMedium  = 2006,
        ExpLarge   = 2007,

        // equipments  3000~
        MysticStaffT1 = 3100,
        MysticStaffT2 = 3101,
        MysticStaffT3 = 3102,
        RubyStaffT1   = 3103,
        RubyStaffT2   = 3104,
        RubyStaffT3   = 3105,
        SapireStaffT1 = 3106,
        SapireStaffT2 = 3107,
        SapireStaffT3 = 3108,
        
        // robe
        MysticRobeT1 = 3200,
        MysticRobeT2 = 3201,
        MysticRobeT3 = 3202,
        DefensiveRobeT1 = 3203,
        DefensiveRobeT2 = 3204,
        DefensiveRobeT3 = 3205,
        DestructionRobeT1 = 3206,
        DestructionRobeT2 = 3207,
        DestructionRobeT3 = 3208,
        
        // gloves 
        MysticGlovesT1 = 3300,
        MysticGlovesT2 = 3301,
        MysticGlovesT3 = 3302,
        JewelryGlovesT1 = 3303,
        JewelryGlovesT2 = 3304,
        JewelryGlovesT3 = 3305,
        FastCasterGlovesT1 = 3306,
        FastCasterGlovesT2 = 3307,
        FastCasterGlovesT3 = 3308,
        
        // shoese
        MysticShoesT1 = 3400,
        MysticShoesT2 = 3401,
        MysticShoesT3 = 3402,
        FeatherShoesT1 = 3403,
        FeatherShoesT2 = 3404,
        FeatherShoesT3 = 3405,
        HoofShoesT1 = 3406,
        HoofShoesT2 = 3407,
        HoofShoesT3 = 3408,
    }

    public enum ItemType {
        None,
        Resource,
        Consumable,
        Equipment,
    }

    public enum EquipmentType {
        Weapon   = 0, // code: 30xx
        Armor    = 1, // code: 31xx
        Gloves   = 2, // code: 33xx
        Shoes    = 3, // code: 34xx
        Ring     = 4, // code: 35xx
        Artifact = 5, // code: 36xx
    }
    
    public enum ItemGrade {
        None,
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
    }

    public enum ItemSortType {
        None,
        All,
        AllEquipments,
        Weapon,
        Armor,
        Gloves,
        Shoes,
        Ring,
        Artifact,
        Resource,
        Consumable,
        ResourceAndConsumable,
    }

    public enum PlayerSkillType {
        Active,
        Passive
    }
    
    public enum ItemInfoType {
        ReadOnly,
        Equipable,
        Releaseable,
        Buyable,
        Sellable,
    }
    
    public enum CameraUpdateType {
        Update,
        FixedUpdate,
        LateUpdate,
    }
        
    public enum CameraMoveType {
        Normal,
        Smooth,
    }
    
    #endregion
}