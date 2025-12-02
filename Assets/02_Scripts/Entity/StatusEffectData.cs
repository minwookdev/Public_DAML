using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat
{
    [Serializable]
    public class StatusEffectData {
        [ShowInInspector, ReadOnly] public TypeStatusEffectListDictionary Dict { get; private set; } = null;
        
        public void Init() {
            var textAsset = Resources.Load<TextAsset>(Defines.STATUS_EFFECT_JSON_PATH);
            var decText = Cryptor.Decrypt2(textAsset.text);
            var dataArray = JsonConvert.DeserializeObject<StatusEffectRecord[]>(decText);

            Dict = new TypeStatusEffectListDictionary();
            for (int i = 0; i < dataArray.Length; i++) {
                var data = dataArray[i];
                if (!Dict.TryGetValue(data.EffectType, out var list)) {
                    var newList = new List<StatusEffectRecord> { data };
                    Dict.Add(data.EffectType, newList);
                    continue;
                }
                
                list.Add(data);
            }
        }

        public bool TryGetRecord(StatusEffectType type, int level, out StatusEffectRecord record) {
            record = null;
            if (level <= 0) {
                CatLog.WLog("Invalid Level Count. Level Must Be Greater Than 0");
                return false;
            }
            
            if (!Dict.TryGetValue(type, out var list)) {
                return false;
            }

            for (int i = 0; i < list.Count; i++) {
                var element = list[i];
                if (element.Level != level) {
                    continue;
                }
                record = element;
                return true;
            }

            // return max level status effect
            record = list[^1];
            return true;
        }
    }

    [Serializable]
    public record StatusEffectRecord(int Index, string Name, string Desc, int Level, float Damage, float Duration, float Interval, StatusEffectType EffectType);
}