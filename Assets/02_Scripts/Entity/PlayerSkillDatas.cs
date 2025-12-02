using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerSkillDatas
    {
        [ShowInInspector, ReadOnly] public IntPlayerSkillDictionary Dict { get; private set; } = null;
        [ShowInInspector, ReadOnly] private IntPlayerSkillGroupDictionary grouppedDict = null;
        [ShowInInspector, ReadOnly] public IntPlayerSkillDictionary ItemSkillDict { get; private set; } = null;
        private readonly List<int> tempIntList = new();

        public void Initialize()
        {
            var textAsset = Resources.Load<TextAsset>(Defines.PLAYER_SKILL_JSON_PATH);
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonConvert.DeserializeObject<PlayerSkill[]>(decText);

            grouppedDict = new IntPlayerSkillGroupDictionary();
            Dict = new IntPlayerSkillDictionary();
            ItemSkillDict = new IntPlayerSkillDictionary();
            for (int i = 0; i < datas.Length; i++)
            {
                var data = datas[i];
                // exception item skill
                if (data.Index >= 3000) 
                {
                    ItemSkillDict.Add(data.Index, data);
                    continue;
                }
                
                Dict.Add(data.Index, data);
                if (grouppedDict.TryGetValue(data.GroupIndex, out List<PlayerSkill> results))
                {
                    results.Add(data);
                }
                else
                {
                    grouppedDict.Add(data.GroupIndex, new List<PlayerSkill> { data });
                }
            }
        }

        public bool TryGetNextLevelSkill(int groupIndex, int currentLevel, out PlayerSkill nextLevelSkill)
        {
            nextLevelSkill = null; // invalid index
            if (!grouppedDict.TryGetValue(groupIndex, out List<PlayerSkill> list))
            {
                throw new Exception($"Group Number {groupIndex} is not exist in PlayerMainSkillDatas");
            }

            var highestLevel = list.Max(s => s.SkillLevel);
            var isMaxLevel = highestLevel == currentLevel;
            if (isMaxLevel)
            {
                return false;
            }

            int targetLevel = currentLevel + 1;
            bool isFind = false;
            for (int i = 0; i < list.Count; i++)
            {
                var skill = list[i];
                if (skill.SkillLevel != targetLevel)
                {
                    continue;
                }

                if (!isFind)
                {
                    isFind = true;
                    nextLevelSkill = skill;
                }
                else
                {
                    throw new NotImplementedException("Find Duplicate Level Skills !");
                }
            }

            if (!isFind)
            {
                throw new
                    Exception($"Not Found Next Level Skill Index: GroupIndex {groupIndex}, CurrentLevel {currentLevel}");
            }

            return true;
        }

        public bool IsMaxLevel(int groupIndex, int level)
        {
            if (!grouppedDict.TryGetValue(groupIndex, out List<PlayerSkill> list))
            {
                throw new Exception($"Group Number {groupIndex} is not exist in PlayerMainSkillDatas");
            }

            var highestLevel = list.Max(s => s.SkillLevel);
            return highestLevel == level;
        }

        public int GetNextLevelSkillIndex(int groupIndex, int level)
        {
            if (!grouppedDict.TryGetValue(groupIndex, out List<PlayerSkill> list))
            {
                throw new Exception($"Group Number {groupIndex} is not exist in PlayerMainSkillDatas");
            }

            var targetLevel = level + 1;
            bool isFind = false;
            int findIndex = -1;
            for (int i = 0; i < list.Count; i++)
            {
                var skill = list[i];
                if (skill.SkillLevel != targetLevel)
                {
                    continue;
                }

                if (!isFind)
                {
                    isFind = true;
                    findIndex = skill.Index;
                }
                else
                {
                    CatLog.WLog("Find Duplicate Level Skills !");
                }
            }

            return findIndex;
        }

        public int GetAllActiveSkillCount()
        {
            int result = 0;
            foreach (var pair in grouppedDict)
            {
                var first = pair.Value[0];
                if (first.Type == PlayerSkillType.Active)
                {
                    result++;
                }
            }

            return result;
        }

        public int GetAllPassiveSkillCount()
        {
            int result = 0;
            foreach (var pair in grouppedDict)
            {
                var first = pair.Value[0];
                if (first.Type == PlayerSkillType.Passive)
                {
                    result++;
                }
            }

            return result;
        }

        public bool IsExistOtherActiveSkill(List<PlayerSkill> earnedSkills,
                                            List<PlayerSkill> reservedSkills)
        {
            tempIntList.Clear();

            // add to temp list to group index
            for (int i = 0; i < earnedSkills.Count; i++)
            {
                var groupIndex = earnedSkills[i].GroupIndex;
                if (!tempIntList.Contains(groupIndex))
                {
                    tempIntList.Add(groupIndex);
                }
            }

            for (int i = 0; i < reservedSkills.Count; i++)
            {
                var groupIndex = reservedSkills[i].GroupIndex;
                if (!tempIntList.Contains(groupIndex))
                {
                    tempIntList.Add(groupIndex);
                }
            }

            foreach (var pair in grouppedDict)
            {
                var first = pair.Value[0];
                if (first.Type != PlayerSkillType.Active)
                {
                    continue;
                }

                if (!tempIntList.Contains(first.GroupIndex))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsExistOtherPassiveSkill(List<PlayerSkill> earnedSkills,
                                             List<PlayerSkill> reservedSkills)
        {
            tempIntList.Clear();

            // add to temp list to group index
            for (int i = 0; i < earnedSkills.Count; i++)
            {
                var groupIndex = earnedSkills[i].GroupIndex;
                if (!tempIntList.Contains(groupIndex))
                {
                    tempIntList.Add(groupIndex);
                }
            }

            for (int i = 0; i < reservedSkills.Count; i++)
            {
                var groupIndex = reservedSkills[i].GroupIndex;
                if (!tempIntList.Contains(groupIndex))
                {
                    tempIntList.Add(groupIndex);
                }
            }

            foreach (var pair in grouppedDict)
            {
                var first = pair.Value[0];
                if (first.Type != PlayerSkillType.Passive)
                {
                    continue;
                }

                if (!tempIntList.Contains(first.GroupIndex))
                {
                    return true;
                }
            }

            return false;
        }

        public List<PlayerSkill> GetLearnableActiveSkills()
        {
            var result = new List<PlayerSkill>();

            foreach (var pair in Dict)
            {
                var skill = pair.Value;
                if (skill.SkillLevel == 1 && skill.Type == PlayerSkillType.Active)
                {
                    result.Add(skill);
                }
            }

            return result;
        }
        
        public List<PlayerSkill> GetLearnablePassiveSkills()
        {
            var result = new List<PlayerSkill>();

            foreach (var pair in Dict)
            {
                var skill = pair.Value;
                if (skill.SkillLevel == 1 && skill.Type == PlayerSkillType.Passive)
                {
                    result.Add(skill);
                }
                
            }

            return result;
        }
    }

    [Serializable]
    public class PlayerSkill
    {
        public int Index = default;
        public string SkillName = default;
        public int SkillLevel = default;
        public float SkillCoolTime = default;
        public float SkillBaseDamage = default;
        public float SkillCoefficient = default;
        public float SkillRange = default;
        public int AttackCount = default;
        public float ProjectileSpeed = default;
        public string Description = default;
        public int GroupIndex = -1;
        public PlayerSkillType Type;
        public string IconKey = string.Empty;
        public string CastingSoundKey;
        public string HitSoundKey;
    }
}