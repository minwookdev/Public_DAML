using System;
using System.Collections.Generic;
using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Datas;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat.Utils.SerializedDictionaries { 
    public abstract class UnitySerializedDictionary<Tkey, TValue> : Dictionary<Tkey, TValue>, ISerializationCallbackReceiver {
        [SerializeField, HideInInspector] private List<Tkey> keyData = new List<Tkey>();
        [SerializeField, HideInInspector] private List<TValue> valueData = new List<TValue>();
    
        void ISerializationCallbackReceiver.OnAfterDeserialize() {
            this.Clear();
            for (int i = 0; i < this.keyData.Count && i < this.valueData.Count; i++) {
                this[this.keyData[i]] = this.valueData[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() {
            this.keyData.Clear();
            this.valueData.Clear();

            foreach (KeyValuePair<Tkey, TValue> keyValuePair in this) {
                this.keyData.Add(keyValuePair.Key);
                this.valueData.Add(keyValuePair.Value);
            }
        }
    }
// Unity 인스펙터에서 딕셔너리를 직렬화하기 위해 키/값을 리스트로 저장하고 역직렬화 시 다시 딕셔너리로 복원한다.
    

    // MANAGERS INFORMATION DICTIONARIES
    [Serializable] public class StringPoolInformationDictionary : UnitySerializedDictionary<string, PoolInfo> { }
    [Serializable] public class StringTransformDictionary : UnitySerializedDictionary<string, Transform> { }
    [Serializable] public class StringGameObjectStackDictionary : UnitySerializedDictionary<string, Stack<GameObject>> { }
    [Serializable] public class StringIntDictionary : UnitySerializedDictionary<string, int> { }
    [Serializable] public class StringResourceInformationDictionary : UnitySerializedDictionary<string, ResourceManager.ResourceInfo> { }
    [Serializable] public class StringEffectInformationDictionary : UnitySerializedDictionary<string, EffectManager.EffectInfo> { }
    [Serializable] public class StringAudioClipDictionary : UnitySerializedDictionary<string, AudioClip> { }
    [Serializable] public class StringAudioSourceDictionary : UnitySerializedDictionary <string, AudioSource> { }   
    
    // DATA DICTIONARIES
    [Serializable] public class StringMonsterStatDictionary : UnitySerializedDictionary<string, MonsterStat> { }
    [Serializable] public class StringMonsterSkillDictionary : UnitySerializedDictionary<string, MonsterSkillStat> { }
    [Serializable] public class StringPlayerStatDictionary : UnitySerializedDictionary<string, PlayerStat> { }
    [Serializable] public class IntPlayerSkillDictionary : UnitySerializedDictionary<int, PlayerSkill> { }
    [Serializable] public class IntPlayerSkillGroupDictionary : UnitySerializedDictionary<int, List<PlayerSkill>> { }
    [Serializable] public class TypeStatusEffectListDictionary : UnitySerializedDictionary<StatusEffectType, List<StatusEffectRecord>> { }
    [Serializable] public class LootTableDictionary : UnitySerializedDictionary<LootCategory, ItemLootList> { }
    [Serializable] public class StartRoomTableDictionary : UnitySerializedDictionary<StartRoomItemPosition, ItemLootList> { }
    [Serializable] public class RewardRoomTableDictionary : UnitySerializedDictionary<RoomGradeType, GuaranteedLootList> { }
    [Serializable] public class ShopRoomTableDictionary : UnitySerializedDictionary<RoomGradeType, GuaranteedLootList> { }
    [Serializable] public class IntItemDictionary : UnitySerializedDictionary<int, Item> { }
    [Serializable] public class IntEquipmentDictionary : UnitySerializedDictionary<int, Equipment> { }
    [Serializable] public class IntConsumableDictionary : UnitySerializedDictionary<int, Consumable> { }
    [Serializable] public class IntResourceDictionary : UnitySerializedDictionary<int, Resource> { }
    [Serializable] public class StatUpgradeListDictionary : UnitySerializedDictionary<StatGrade, StatUpgradeDataList> { }
    [Serializable] public class IndexLevelDictionary : UnitySerializedDictionary<int, int> { }
}
