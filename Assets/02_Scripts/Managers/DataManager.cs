using System;
using Sirenix.OdinInspector;
using RandomDungeonWithBluePrint;
using CoffeeCat.Datas;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.FrameWork {
    public class DataManager : DynamicSingleton<DataManager> {
        [Title(".CSV / .Json")]
        [ShowInInspector, ReadOnly] public MonsterStatDatas MonsterStats { get; private set; } = null;
        [ShowInInspector, ReadOnly] public MonsterSkillDatas MonsterSkills { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerStatDatas PlayerStats { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerSkillDatas PlayerMainSkills { get; private set; } = null;
        [ShowInInspector, ReadOnly] public StatusEffectData StatusEffects { get; private set; } = null;
        [ShowInInspector, ReadOnly] public ItemDatas Items { get; private set; } = null;
        
        [Title("Scriptable Object")]
        [ShowInInspector, ReadOnly] public DungeonInfoScriptable DungeonInfoScriptable { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerLevelData PlayerLevelData { get; private set; } = null;
        [ShowInInspector, ReadOnly] public CharacterSelectDatas CharacterSelectDatas { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerAbilityData PlayerAbilityData { get; private set; } = null;
        [ShowInInspector, ReadOnly] public PlayerStatUpgradeData[] PlayerStatUpgradeDatas { get; private set; } = null;
        public DungeonInfo[] DungeonInfos => DungeonInfoScriptable?.DungeonInfos;

        protected override void Initialize() {
            MonsterStats = new MonsterStatDatas();
            MonsterSkills = new MonsterSkillDatas(); 
            PlayerStats = new PlayerStatDatas();
            PlayerMainSkills = new PlayerSkillDatas();
            StatusEffects = new StatusEffectData();
            Items = new ItemDatas();
            LoadJsonData();
            LoadScriptableObject();
        }
        
        /// <summary>
        /// Load Json Data to Data Class
        /// </summary>
        private void LoadJsonData() {
            MonsterStats.Initialize();
            MonsterSkills.Initialize();
            PlayerStats.Initialize();
            PlayerMainSkills.Initialize();
            StatusEffects.Init();
            Items.Init();
        }
        
        private void LoadScriptableObject() {
            DungeonInfoScriptable = ResourceManager.Inst.ResourcesLoad<DungeonInfoScriptable>(Defines.DATA_PATH_DUNGEON_INFO, true);
            PlayerLevelData       = ResourceManager.Inst.ResourcesLoad<PlayerLevelData>(Defines.DATA_PATH_CAHRA_LEVEL, true);
            CharacterSelectDatas  = ResourceManager.Inst.ResourcesLoad<CharacterSelectDatas>(Defines.DATA_PATH_CHARA_SELECT, true);
            PlayerAbilityData     = ResourceManager.Inst.ResourcesLoad<PlayerAbilityData>(Defines.DATA_PATH_ABILITY, true);
            var statUpgradeData1  = ResourceManager.Inst.ResourcesLoad<PlayerStatUpgradeData>(Defines.DATA_PATH_CHARA_STAT_0, true);
            var statUpgradeData2  = ResourceManager.Inst.ResourcesLoad<PlayerStatUpgradeData>(Defines.DATA_PATH_CHARA_STAT_1, true);
            var statUpgradeData3  = ResourceManager.Inst.ResourcesLoad<PlayerStatUpgradeData>(Defines.DATA_PATH_CHARA_STAT_2, true);
            PlayerStatUpgradeDatas = new[] { statUpgradeData1, statUpgradeData2, statUpgradeData3 };
            CharacterSelectDatas.Init();
        }

        public DungeonBluePrint GetBluePrintQueue(string dungeonId) {
            var infos = DungeonInfoScriptable.DungeonInfos;
            for (int i = 0; i < infos.Length; i++) {
                var info = infos[i];
                if (info.dungeonId == dungeonId) {
                    return info.bp;
                }
            }
            return null;
        }
    }
}

[Serializable]
public class PlayerSkillSelectData {
    public string Name;
    public string Desc;
    public string IconKey;
    public int Index;
    public PlayerSkillType Type;

    public PlayerSkillSelectData(string name, string desc, int index, PlayerSkillType type, string iconKey)
    {
        Name = name;
        Desc = desc;
        Index = index;
        Type = type;
        IconKey = iconKey;
    }
}