using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat.Datas {
    [Serializable]
    public class MonsterStatDatas {
        [ShowInInspector, ReadOnly] public StringMonsterStatDictionary DataDictionary { get; private set; } = null;

        public void Initialize() {
            // Get Data From Resources in Json
            var textAsset = Resources.Load<TextAsset>(Defines.MONSTER_STAT_JSON_PATH);
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonConvert.DeserializeObject<List<MonsterStat>>(decText);

            // Add to Data Dictionary
            DataDictionary = new StringMonsterStatDictionary();
            for (int i = 0; i < datas.Count; i++) {
                DataDictionary.Add(datas[i].Name, datas[i]);
            }
        }
    }

    [Serializable]
    public class MonsterStat {
        // =============================================================================================================
        // 1. Add required stat variables and excel data (match names)
        // 2. Applies to DeepCopy and Copy methods
        // =============================================================================================================
        
        // private fields (Show in Inspector)
        // [Required: SerializeField Attributes]
        // [Optional: public Property]
        //[SerializeField] private string name = string.Empty;
        //[SerializeField] private string desc = string.Empty;
        //[SerializeField] private float hp = default;
        //[SerializeField] private float mp = default;
        //
        //public string Name { get => name; }
        //public string Desc { get => desc; }
        //public float HP { get => hp; }
        //public float MP { get => mp; }

        // or

        // public fields
        // [Required: public access restrictor]
        public string Name = string.Empty;
        public string Desc = string.Empty;
        public float HP = 0f;
        public float MP = 0f;
        public float Damage = 0f;
        public float DamageDeviation = 0.05f;
        public float CriticalChance = 0f;
        public float CriticalMultiplier = 0f;
        public float Defence = 0f;
        public float CriticalResist = 0f;
        public float Penetration = 0f;
        public float ExpAmount = 0f;
        public float SpeedRandomRange = 1f;
        public int LootCategory = 0;

        public MonsterStat DeepCopyMonsterStat() {
            var newStat = new MonsterStat {
                Name = Name,
                Desc = Desc,
                HP = HP,
                MP = MP,
                Damage = Damage,
                DamageDeviation = DamageDeviation,
                CriticalChance = CriticalChance,
                CriticalMultiplier = CriticalMultiplier,
                Defence = Defence,
                CriticalResist = CriticalResist,
                Penetration = Penetration,
                ExpAmount = ExpAmount,
                SpeedRandomRange = SpeedRandomRange,
                LootCategory = LootCategory
            };
            return newStat;
        }

        public void CopyValue(MonsterStat originStat) {
            Name = originStat.Name;
            Desc = originStat.Desc;
            HP = originStat.HP;
            MP = originStat.MP;
            Damage = originStat.Damage;
            DamageDeviation = originStat.DamageDeviation;
            CriticalChance = originStat.CriticalChance;
            CriticalMultiplier = originStat.CriticalMultiplier;
            Defence = originStat.Defence;
            CriticalResist = originStat.CriticalResist;
            Penetration = originStat.Penetration;
            ExpAmount = originStat.ExpAmount;
            SpeedRandomRange = originStat.SpeedRandomRange;
            LootCategory = originStat.LootCategory;
        }
    }
}
