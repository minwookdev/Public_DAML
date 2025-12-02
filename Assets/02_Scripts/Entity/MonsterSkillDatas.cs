using System;
using System.Collections.Generic;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.JsonParser;
using CoffeeCat.Utils.SerializedDictionaries;
using Newtonsoft.Json;
using UnityEngine;

namespace CoffeeCat.Datas {
    [Serializable]
    public class MonsterSkillDatas {
        [ShowInInspector, ReadOnly] public StringMonsterSkillDictionary DataDictionary { get; private set; } = null;

        public void Initialize() {
            var textAsset = Resources.Load<TextAsset>(Defines.MONSTER_SKILL_JSON_PATH);
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonConvert.DeserializeObject<MonsterSkillStat[]>(decText);
            
            // MonsterSkillStat[] datas = jsonParser.LoadFromJsonInResources<MonsterSkillStat>("Entity/Json/MonsterSkillStat");
            DataDictionary = new StringMonsterSkillDictionary();
            for (int i = 0; i < datas.Length; i++) {
                DataDictionary.Add(datas[i].Key, datas[i]);
            }
        }
    }

    [Serializable]
    public class MonsterSkillStat {
        public string Key = string.Empty; // Projectile Spawn Key
        public string Name = string.Empty;
        public string Desc = string.Empty;
        public float Damage = default;
        public float Ratio = default;
        public float Cost = default;
    }
}