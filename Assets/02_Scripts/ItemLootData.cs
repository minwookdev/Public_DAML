using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CoffeeCat {
    [Serializable]
    public class ItemLootList {
        public List<ItemLootData> Loots = null;

        public ItemLootData Raffle() {
            return Raffle(Loots);
        }

        public static ItemLootData Raffle(List<ItemLootData> list) {
            if (list.Count == 1) {
                return list[0];
            }
            
            float total = 0f;
            for (var i = 0; i < list.Count; i++) {
                var loot = list[i];
                total += loot.Chance;
            }

            float randomPoint = Random.value * total;
            for (var i = 0; i < list.Count; i++) {
                var loot = list[i];
                if (randomPoint < loot.Chance) {
                    return loot;
                }

                randomPoint -= loot.Chance;
            }
            return list[^1];
        }
    }
    
    [Serializable]
    public class ItemLootData {
        public ItemCode Code = ItemCode.None;
        public bool isApplyAmountRandomRange = false;
        [ShowIf("@isApplyAmountRandomRange == false")] public int Amount = 0;
        [ShowIf(nameof(isApplyAmountRandomRange))] public int RandomAmountMin = 0;
        [ShowIf(nameof(isApplyAmountRandomRange))] public int RandomAmountMax = 0;
        [PropertyRange(0, 100)] public int Chance = 0;
        public int Price = -1;

        public int GetAmount() {
            return isApplyAmountRandomRange ? Random.Range(RandomAmountMin, RandomAmountMax + 1) : Amount;
        }

        public int GetDivideAmount(int divideCount) {
            var divideAmount = (float)GetAmount() / divideCount;
            return Mathf.RoundToInt(divideAmount);
        }

        public ItemLootData Clone() {
            return new ItemLootData()
            {
                Code = Code,
                Amount = Amount,
                RandomAmountMin = RandomAmountMin,
                RandomAmountMax = RandomAmountMax,
                Chance = Chance,
                Price = Price
            };
        }
    }

    [Serializable]
    public struct ItemLootRequest {
        public ItemCode Code; // 4 byte
        public int Amount;    // 4 byte
    }

    [Serializable]
    public class GuaranteedLootList : ItemLootList {
        public int MinRollCount = 0;
        public int MaxRollCount = 0;
        // [PropertyRange(0, 100)] public int ChanceForThisGroup = 0;
        
        private readonly List<ItemLootData> resultList = new();
        private readonly List<ItemLootData> tempList = new();

        private int GetRollCount() {
            return Random.Range(MinRollCount, MaxRollCount + 1);
        }
        
        public List<ItemLootData> GetRewardRoomList() {
            resultList.Clear();
            int rollCount = GetRollCount();
            if (rollCount <= 0) {
                return resultList;
            }
            
            for (var i = 0; i < rollCount; i++) {
                var loot = Raffle();
                resultList.Add(loot);
            }
            return resultList;
        }
        
        public List<ItemLootData> GetShopRoomLootList() {
            resultList.Clear();
            tempList.Clear();

            int rollCount = GetRollCount();
            if (rollCount <= 0) {
                return resultList;
            }
            
            // add all loots to temp list
            for (int i = 0; i < Loots.Count; i++) {
                tempList.Add(Loots[i]);
            }

            for (int i = 0; i < rollCount; i++) {
                var raffled = Raffle(tempList);
                tempList.Remove(raffled);
                resultList.Add(raffled);
            }

            return resultList;
        }
    }
}
