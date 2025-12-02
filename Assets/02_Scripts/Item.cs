using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat {
    [Serializable]
    public class ItemDatas {
        [ShowInInspector, ReadOnly] public IntItemDictionary Dict { get; private set; } = null;
        [ShowInInspector, ReadOnly] public IntEquipmentDictionary EquipmentDict { get; private set; } = null;
        [ShowInInspector, ReadOnly] public IntConsumableDictionary ConsumableDict { get; private set; } = null;
        [ShowInInspector, ReadOnly] public IntResourceDictionary ResourceDict { get; private set; } = null;
        
        public void Init() {
            Dict = new IntItemDictionary();
            
            // load equipment and consumable items
            var textAsset1 = Resources.Load<TextAsset>(Defines.ITEM_EQUIPMENT_JSON_PATH);
            var decryptedText1 = Cryptor.Decrypt2(textAsset1.text);
            var equipmentItemDatas = JsonConvert.DeserializeObject<Equipment[]>(decryptedText1);
            EquipmentDict = new IntEquipmentDictionary();
            for (int i = 0; i < equipmentItemDatas.Length; i++) {
                EquipmentDict.Add(equipmentItemDatas[i].Id, equipmentItemDatas[i]);
            }
            
            var textAsset2 = Resources.Load<TextAsset>(Defines.ITEM_CONSUMABLE_JSON_PATH);
            var decryptedText2 = Cryptor.Decrypt2(textAsset2.text);
            var consumableItemDatas = JsonConvert.DeserializeObject<Consumable[]>(decryptedText2);
            ConsumableDict = new IntConsumableDictionary();
            for (int i = 0; i < consumableItemDatas.Length; i++) {
                ConsumableDict.Add(consumableItemDatas[i].Id, consumableItemDatas[i]);
            }
            
            // load resource items
            var textAsset3 = Resources.Load<TextAsset>(Defines.ITEM_RESOURCE_JSON_PATH);
            var decryptedText3 = Cryptor.Decrypt2(textAsset3.text);
            var resourceItemDatas = JsonConvert.DeserializeObject<Resource[]>(decryptedText3);
            ResourceDict = new IntResourceDictionary();
            for (int i = 0; i < resourceItemDatas.Length; i++) {
                ResourceDict.Add(resourceItemDatas[i].Id, resourceItemDatas[i]);
            }
            
            // add to all items dictionary  
            foreach (var item in equipmentItemDatas) {
                Dict.Add(item.Id, item);
            }
            foreach (var item in consumableItemDatas) {
                Dict.Add(item.Id, item);
            }
            foreach (var item in resourceItemDatas) {
                Dict.Add(item.Id, item);
            }
        }

        public Item GetItemData(int id) {
            return Dict.GetValueOrDefault(id);
        }
        
        public string GetIconKey(int id) {
            return Dict.TryGetValue(id, out Item result) ? result.IconKey : string.Empty;
        }
        
        public ItemType GetItemType(int id) {
            return Dict.TryGetValue(id, out Item result) ? result.Type : ItemType.None;
        }
    }
    
    [Serializable]
    public class Item {
        public int Id;
        public string NameKey;
        public string DescKey;
        public string IconKey;
        public int Amount;
        public int Price;
        public ItemGrade Grade;
        public ItemType Type;
        public bool IsUpdated;
        
        public static implicit operator bool(Item item) => item != null;
        
        public virtual void WriteAttributes(Dictionary<string, string> dict) {
            dict.Add("NAME: ", NameKey);
            dict.Add("AMOUNT: ", DescKey);
            dict.Add("GRADE: : ", Grade.ToStringEx());
        }

        public virtual Item Clone() => throw new NotImplementedException();
    }
    
    public class Resource : Item {
        public override void WriteAttributes(Dictionary<string, string> dict) {
            base.WriteAttributes(dict);
            dict.Add("TYPE: ", "RESOURCE");
        }

        public override Item Clone() {
            return new Resource {
                Id = Id,
                NameKey = NameKey,
                DescKey = DescKey,
                IconKey = IconKey,
                Amount = Amount,
                Grade = Grade,
                Type = Type
            };
        }
    }
    
    public class Equipment : Item {
        public EquipmentType EquipType;
        
        public int AddMaxHP;
        public int AddDamage;
        public int AddDefence;
        public int AddGrantSkillId;
        public int AddSkillChainCount;
        public int AddProjectileCount;
        public int AddMoveSpeed;
        public int AddPenetration;
        public float AddMaxHPRatio;
        public float AddHPRegenPerSeconds;
        public float AddCoolTimeReductionRatio;
        public float AddCriticalChance;
        public float AddCriticalMultiplier;
        public float AddItemAcquisitionRange;
        public float AddMonsterFindRange;
        public float AddSkillRangeRatio;
        public float AddExpGainRatio;
        public float AddGoldGainRatio;
        public float AddItemDropRateRatio;

        public override void WriteAttributes(Dictionary<string, string> dict) {
            base.WriteAttributes(dict);
            dict.Add("TYPE: ", "EQUIPMENT / " + EquipType.ToStringEx());
            if (AddMaxHP > 0)                   dict.Add("MAX HP: ", AddMaxHP.ToStringN0());
            if (AddDamage > 0)                  dict.Add("DAMAGE: ", AddDamage.ToStringN0());
            if (AddDefence > 0)                 dict.Add("DEFENCE: ", AddDefence.ToStringN0());
            if (AddGrantSkillId > 0)            dict.Add("GRANT SKILL: ", AddGrantSkillId.ToString());
            if (AddSkillChainCount > 0)         dict.Add("SKILL CHAIN COUNT: ", AddSkillChainCount.ToStringN0());
            if (AddProjectileCount > 0)         dict.Add("PROJECTILE COUNT: ", AddProjectileCount.ToStringN0());
            if (AddMoveSpeed > 0)               dict.Add("MOVE SPEED: ", AddMoveSpeed.ToStringN0());
            if (AddPenetration > 0)             dict.Add("PENETRATION: ", AddPenetration.ToStringN0());
            if (AddMaxHPRatio > 0f)             dict.Add("MAX HP RATIO: ", (AddMaxHPRatio * 100).ToStringN0() + " %");
            if (AddHPRegenPerSeconds > 0f)      dict.Add("REGEN HP: ", AddHPRegenPerSeconds.ToStringN2() + " /sec");
            if (AddCoolTimeReductionRatio > 0f) dict.Add("COOL TIME REDUCTION: ", (AddCoolTimeReductionRatio * 100).ToStringN0() + " %");
            if (AddCriticalChance > 0f)         dict.Add("CRITICAL CHANCE: ", AddCriticalChance.ToStringN2() + " %");
            if (AddCriticalMultiplier > 0f)     dict.Add("CRITICAL MULTIPLIER: ", (AddCriticalMultiplier * 100).ToStringN0() + " %");
            if (AddItemAcquisitionRange > 0f)   dict.Add("ITEM ACQUISITION RANGE: ", AddItemAcquisitionRange.ToStringN2());
            if (AddMonsterFindRange > 0f)       dict.Add("MONSTER FIND RANGE: ", AddMonsterFindRange.ToStringN2());
            if (AddSkillRangeRatio > 0f)        dict.Add("SKILL RANGE RATIO: ", AddSkillRangeRatio.ToStringN2());
            if (AddExpGainRatio > 0f)           dict.Add("EXP GAIN RATIO: ", (AddExpGainRatio * 100).ToStringN0() + " %");
            if (AddGoldGainRatio > 0f)          dict.Add("GOLD GAIN RATIO: ", (AddGoldGainRatio * 100).ToStringN0() + " %");
            if (AddItemDropRateRatio > 0f)      dict.Add("ITEM DROP RATE RATIO: ", (AddItemDropRateRatio * 100).ToStringN0() + " %");
        }

        public override Item Clone() {
            return new Equipment {
                Id = Id,
                NameKey = NameKey,
                DescKey = DescKey,
                IconKey = IconKey,
                Amount = Amount,
                Grade = Grade,
                Type = Type,
                EquipType = EquipType,
                AddMaxHP = AddMaxHP,
                AddDamage = AddDamage,
                AddDefence = AddDefence,
                AddGrantSkillId = AddGrantSkillId,
                AddSkillChainCount = AddSkillChainCount,
                AddProjectileCount = AddProjectileCount,
                AddMoveSpeed = AddMoveSpeed,
                AddPenetration = AddPenetration,
                AddMaxHPRatio = AddMaxHPRatio,
                AddHPRegenPerSeconds = AddHPRegenPerSeconds,
                AddCoolTimeReductionRatio = AddCoolTimeReductionRatio,
                AddCriticalChance = AddCriticalChance,
                AddCriticalMultiplier = AddCriticalMultiplier,
                AddItemAcquisitionRange = AddItemAcquisitionRange,
                AddMonsterFindRange = AddMonsterFindRange,
                AddSkillRangeRatio = AddSkillRangeRatio,
                AddExpGainRatio = AddExpGainRatio,
                AddGoldGainRatio = AddGoldGainRatio,
                AddItemDropRateRatio = AddItemDropRateRatio
            };
        }
    }
    
    public class Consumable : Item {
        public float Value;
        
        public override void WriteAttributes(Dictionary<string, string> dict) {
            base.WriteAttributes(dict);
            dict.Add("TYPE: ", "CONSUMABLE");
            dict.Add("VALUE: ", Value.ToStringN0());
        }

        public override Item Clone() {
            return new Consumable {
                Id = Id,
                NameKey = NameKey,
                DescKey = DescKey,
                IconKey = IconKey,
                Amount = Amount,
                Grade = Grade,
                Type = Type,
                Value = Value
            };
        }
    }
        
    [Serializable]
    public class Inventory {
        private static List<Item> resultList = new();
        public List<Item> Items { get; init; } = new();
    }
    
    [Serializable]
    public class EquipmentSet {
        public Equipment Weapon;
        public Equipment Accessory;
        public Equipment Artifact;
        public Equipment Armor;
        public Equipment Gloves;
        public Equipment Shoes;
        
        public void Clear() {
            Weapon = null;
            Accessory = null;
            Artifact = null;
            Armor = null;
            Gloves = null;
            Shoes = null;
        }
    }

    [Serializable]
    public class CharacterInventorySystem {
        public EquipmentSet EquipmentSet { get; init; } = new();
        public Inventory Inventory { get; init; } = new();
        private static List<Item> resultList = new();
        private const byte MAX_STACK_COUNT = 100;
        private ItemDatas itemData = null;
        public int Currency { get; private set; } = 0;

        public void Init(ItemDatas data) => itemData = data;

        public void AddCurrency(int amount) {
            Currency += amount;
            DungeonEvtManager.InvokeEventOnPlayerCurrencyChanged(Currency);
        }

        public void RemoveCurrency(int amount)
        {
            Currency -= amount;
            DungeonEvtManager.InvokeEventOnPlayerCurrencyChanged(Currency);
        }

        public void EquipItem(Item item) {
            if (item is not Equipment equipment) {
                throw new InvalidCastException($"Item is not Equipment: {item.NameKey}");
            }
            
            var isSuccessed = Inventory.Items.Remove(item);
            if (!isSuccessed) {
                CatLog.WLog("Failed to Remove Item from Inventory: " + item.NameKey);
            }

            switch (equipment.EquipType) {
                case EquipmentType.Weapon:
                    if (EquipmentSet.Weapon) {
                        Inventory.Items.Add(EquipmentSet.Weapon);
                    }
                    EquipmentSet.Weapon = equipment; 
                    break;
                case EquipmentType.Ring: 
                    if (EquipmentSet.Accessory) {
                        Inventory.Items.Add(EquipmentSet.Accessory);
                    }
                    EquipmentSet.Accessory = equipment;
                    break;
                case EquipmentType.Artifact: 
                    if (EquipmentSet.Artifact) {
                        Inventory.Items.Add(EquipmentSet.Artifact);
                    }
                    EquipmentSet.Artifact = equipment;
                    break;
                case EquipmentType.Armor: 
                    if (EquipmentSet.Armor) {
                        Inventory.Items.Add(EquipmentSet.Armor);
                    }
                    EquipmentSet.Armor = equipment;
                    break;
                case EquipmentType.Gloves: 
                    if (EquipmentSet.Gloves) {
                        Inventory.Items.Add(EquipmentSet.Gloves);
                    }
                    EquipmentSet.Gloves = equipment; 
                    break;
                case EquipmentType.Shoes: 
                    if (EquipmentSet.Shoes) {
                        Inventory.Items.Add(EquipmentSet.Shoes);
                    }
                    EquipmentSet.Shoes = equipment; 
                    break;
                default: 
                    throw new NotImplementedException($"Invalid EquipType: {equipment.EquipType}");
            }
            
            // UPDATEA PLAYER STATS
            DungeonEvtManager.InvokeEventOnPlayerEquipmentsChanged(EquipmentSet);
            DungeonEvtManager.InvokeEventOnPlayerInventoryChanged();
        }

        public void ReleaseItem(Item item) {
            if (item is not Equipment equipment) {
                throw new InvalidCastException($"Item is not Equipment: {item.NameKey}");
            }

            switch (equipment.EquipType) {
                case EquipmentType.Weapon:
                    if (equipment != EquipmentSet.Weapon) {
                        throw new Exception($"not matched equipped item: {equipment.NameKey}");
                    }
                    Inventory.Items.Add(EquipmentSet.Weapon);
                    EquipmentSet.Weapon = null;
                    break;
                case EquipmentType.Armor:
                    if (equipment != EquipmentSet.Armor) {
                        throw new Exception($"not matched equipped item: {equipment.NameKey}");
                    }
                    Inventory.Items.Add(EquipmentSet.Armor);
                    EquipmentSet.Armor = null;
                    break;
                case EquipmentType.Gloves:
                    if (equipment != EquipmentSet.Gloves) {
                        throw new Exception($"not matched equipped item: {equipment.NameKey}");
                    }
                    Inventory.Items.Add(EquipmentSet.Gloves);
                    EquipmentSet.Gloves = null;
                    break;
                case EquipmentType.Shoes:
                    if (equipment != EquipmentSet.Shoes) {
                        throw new Exception($"not matched equipped item: {equipment.NameKey}");
                    }
                    Inventory.Items.Add(EquipmentSet.Shoes);
                    EquipmentSet.Shoes = null;
                    break;
                case EquipmentType.Ring:
                    if (equipment != EquipmentSet.Accessory) {
                        throw new Exception($"not matched equipped item: {equipment.NameKey}");
                    }
                    Inventory.Items.Add(EquipmentSet.Accessory);
                    EquipmentSet.Accessory = null;
                    break;
                case EquipmentType.Artifact:
                    if (equipment != EquipmentSet.Artifact) {
                        throw new Exception($"not matched equipped item: {equipment.NameKey}");
                    }
                    Inventory.Items.Add(EquipmentSet.Artifact);
                    EquipmentSet.Artifact = null;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            DungeonEvtManager.InvokeEventOnPlayerEquipmentsChanged(EquipmentSet);
            DungeonEvtManager.InvokeEventOnPlayerInventoryChanged();
        }
        
        public bool RemoveItem(Item item) {
            if (!Inventory.Items.Contains(item)) {
                CatLog.WLog($"Item is not in Inventory: {item.NameKey}");
                return false;
            }
            Inventory.Items.Remove(item);
            DungeonEvtManager.InvokeEventOnPlayerInventoryChanged();
            return true;
        }
        
        public List<Item> GetItemsBySortType(ItemSortType sortType) {
            var inventoryItems = Inventory.Items;
            if (sortType == ItemSortType.All) {
                return inventoryItems;
            }
            
            resultList.Clear();
            // Resource, Consumable, AllEquipments
            var isDetailedEquipmentType = sortType.IsDetailedEquipmentType();
            if (!isDetailedEquipmentType) {
                // single type
                var hasMultipleTypes = sortType.IsIncludeMutipleItemType();
                if (!hasMultipleTypes) {
                    var convertedItemType = sortType.ConvertToItemType();
                    for (int i = 0; i < inventoryItems.Count; i++) {
                        var item = inventoryItems[i];
                        if (item.Type == convertedItemType) {
                            resultList.Add(item);
                        }
                    }
                    return resultList;
                }
                
                // multiple type
                var itemTypes = sortType.ConvertToMultipleTypes();
                for (int i = 0; i < inventoryItems.Count; i++) {
                    var item = inventoryItems[i];
                    if (itemTypes.Contains(item.Type)) {
                        resultList.Add(item);
                    }
                }
                return resultList;
            }
            
            // Detailed Equipment
            var convertedEquipType = sortType.ConvertToEquipType();
            for (int i = 0; i < inventoryItems.Count; i++) {
                var item = inventoryItems[i];
                if (item.Type != ItemType.Equipment) {
                    continue;
                }
                        
                if (item is not Equipment equipment) {
                    throw new InvalidCastException($"Item is not Equipment: {item}");
                }
                        
                if (equipment.EquipType == convertedEquipType) {
                    resultList.Add(item);
                }
            }
            return resultList;
        }

        /// <summary>
        /// For equipment items, amount is ignored (fixed to 1)
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="amount"></param>
        public void AddItem(int itemId, int amount = 1) {
            var entity = itemData.GetItemData(itemId);
            if (!entity) {
                CatLog.WLog("Invalid Item Id : " + itemId);
                return;
            }
            AddItem(entity, amount);
        }

        /// <summary>
        /// For equipment items, amount is ignored (fixed to 1)
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="amount"></param>
        public void AddItem(Item entity, int amount = 1) {
            if (TryUseConsumableItem(entity, amount)) {
                return;
            }
            
            // invoke event
            DungeonEvtManager.InvokeEventOnPlayerInventoryChanged();
            
            // add equipment item
            var entityType = entity.Type;
            if (entityType == ItemType.Equipment) {
                var equipmentClone = entity.Clone();
                equipmentClone.Amount = 1;
                Inventory.Items.Add(equipmentClone);
                return;
            }

            // add resource and consumable
            if (amount <= 0) {
                CatLog.WLog("Invalid Item Amount : " + amount);
                return;
            }
            
            // stacking item by max stack count
            for (int i = 0; i < Inventory.Items.Count; i++) {
                // find matched item and check more stackable
                var item = Inventory.Items[i];
                if (item.Id != entity.Id || item.Amount >= MAX_STACK_COUNT) {
                    continue;
                }
                
                // stackable item
                var remainAmount = MAX_STACK_COUNT - item.Amount;   
                if (remainAmount >= amount) {
                    item.Amount += amount;
                    return;
                }
                
                // stack and decrease remain amount 
                item.Amount = MAX_STACK_COUNT;
                amount -= remainAmount;
            }
            
            var cloneCommon = entity.Clone();
            cloneCommon.Amount = amount;
            Inventory.Items.Add(cloneCommon);
        }

        public bool BuyItem(Item entity) {
            if (Currency < entity.Price) {
                return false;
            }
            
            AddCurrency(-entity.Price);
            AddItem(entity);
            DungeonEvtManager.InvokeEventOnPlayerBuyedItem(entity.Id, entity.Price);
            return true;
        }

        public bool SellItem(Item item) {
            if (!RemoveItem(item)) {
                return false;
            }

            AddCurrency(item.Price);
            DungeonEvtManager.InvokeEventOnPlayerSelledItem();
            return true;
        }

        private bool TryUseConsumableItem(Item entity, int amount) {
            var itemCode = (ItemCode)entity.Id;
            if (itemCode == ItemCode.Currency) {
                AddCurrency(amount);
                return true;
            }

            if (!itemData.ConsumableDict.TryGetValue(entity.Id, out Consumable consumableItem)) {
                return false;
            }
            
            switch (itemCode) {
                // INCREASE PLAYER HP ITEMS
                case ItemCode.SmallHeal: 
                case ItemCode.MediumHeal:
                case ItemCode.LargeHeal:
                    RogueLiteManager.Inst.IncreasePlayerHealth(consumableItem.Value);
                    return true;
                // INCREASE PLAYER EXP ITEMS
                case ItemCode.ExpForced:
                    RogueLiteManager.Inst.ForcedIncreasePlayerLevel();
                    return true;
                case ItemCode.ExpSmall:
                case ItemCode.ExpMedium:
                case ItemCode.ExpLarge: 
                    RogueLiteManager.Inst.AddExp(consumableItem.Value);
                    return true;
                case ItemCode.ExpFull:
                    throw new NotImplementedException();
                // OTHER ITEMS
                default: return false;
            }
        }

        public void Clear() {
            EquipmentSet.Clear();
            Inventory.Items.Clear();
            Currency = 0;
        }
    }
}