using System;
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerStatDatas
    {
        [ShowInInspector, ReadOnly] public StringPlayerStatDictionary DataDictionary { get; private set; } = null;

        public void Initialize()
        {
            var textAsset = Resources.Load<TextAsset>(Defines.PLAYER_STAT_JSON_PATH);
            var decText = Cryptor.Decrypt2(textAsset.text);
            var datas = JsonConvert.DeserializeObject<PlayerStat[]>(decText);

            DataDictionary = new StringPlayerStatDictionary();
            for (int i = 0; i < datas.Length; i++)
            {
                DataDictionary.Add(datas[i].Name, datas[i]);
            }
        }
    }

    [Serializable]
    public class PlayerStat
    {
        public int Index = default;
        public string Name = string.Empty;
        public float MaxHp = 0f;
        public float CurrentHp = 0f;
        public float Defense = default;
        public float MoveSpeed = default;
        public float InvincibleTime = default;
        public float AttackPower = default;
        public float CriticalChance = default;
        public float CriticalResistance = default;
        public float CriticalMultiplier = default;
        public float DamageDeviation = default;
        public float Penetration = default;
        public float ItemAcquisitionRange = default;
        public float CoolTimeReduce = default;
        public float SearchRange = default;
        public int AddProjectile = default;
        
        public float LifeDrainChance = 0f;
        public float LifeDrainRatio = 0f;
        public float KnockBackChance = 0f;
        public float KnockBackForce = 0f;
        public float InstKillChance = 0f;
        public float PoisonChance = 0f;
        public int PoisonDefaultTier = 1;
        public float BleedChance = 0f;
        public int BleedDefaultTier = 1;
        public float StunChance = 0f;
        public int StunDefaultTier = 1;
        
        public void Clear()
        {
            MaxHp = 0;
            Defense = 0;
            MoveSpeed = 0;
            InvincibleTime = 0;
            AttackPower = 0;
            CriticalChance = 0;
            CriticalResistance = 0;
            CriticalMultiplier = 0;
            DamageDeviation = 0;
            Penetration = 0;
            ItemAcquisitionRange = 0;
            CoolTimeReduce = 0;
            SearchRange = 0;
            AddProjectile = 0;
            LifeDrainChance = 0;
            LifeDrainRatio = 0;
            KnockBackChance = 0;
            KnockBackForce = 0;
            InstKillChance = 0;
            PoisonChance = 0;
            PoisonDefaultTier = 1;
            BleedChance = 0;
            BleedDefaultTier = 1;
            StunChance = 0;
            StunDefaultTier = 1;
        }

        public PlayerStat Clone() {
            return new PlayerStat {
                Index = Index,
                Name = Name,
                MaxHp = MaxHp,
                Defense = Defense,
                MoveSpeed = MoveSpeed,
                InvincibleTime = InvincibleTime,
                AttackPower = AttackPower,
                CriticalChance = CriticalChance,
                CriticalResistance = CriticalResistance,
                CriticalMultiplier = CriticalMultiplier,
                DamageDeviation = DamageDeviation,
                Penetration = Penetration,
                ItemAcquisitionRange = ItemAcquisitionRange,
                CoolTimeReduce = CoolTimeReduce,
                SearchRange = SearchRange,
                AddProjectile = AddProjectile,
                LifeDrainChance = LifeDrainChance,
                LifeDrainRatio = LifeDrainRatio,
                KnockBackChance = KnockBackChance,
                KnockBackForce = KnockBackForce,
                InstKillChance = InstKillChance,
                PoisonChance = PoisonChance,
                PoisonDefaultTier = PoisonDefaultTier,
                BleedChance = BleedChance,
                BleedDefaultTier = BleedDefaultTier,
                StunChance = StunChance,
                StunDefaultTier = StunDefaultTier
            };
        }

        public PlayerStat AddStat(PlayerStat baseStat, PlayerStat buffedStat, PlayerStat equipStat)
        {
            Index = baseStat.Index;
            Name = baseStat.Name;
            MaxHp = baseStat.MaxHp + buffedStat.MaxHp + equipStat.MaxHp;
            Defense = baseStat.Defense + buffedStat.Defense + equipStat.Defense;
            MoveSpeed = baseStat.MoveSpeed + buffedStat.MoveSpeed + equipStat.MoveSpeed;
            InvincibleTime = baseStat.InvincibleTime + buffedStat.InvincibleTime + equipStat.InvincibleTime;
            AttackPower = baseStat.AttackPower + buffedStat.AttackPower + equipStat.AttackPower;
            CriticalChance = baseStat.CriticalChance + buffedStat.CriticalChance + equipStat.CriticalChance;
            CriticalResistance = baseStat.CriticalResistance + buffedStat.CriticalResistance + equipStat.CriticalResistance;
            CriticalMultiplier = baseStat.CriticalMultiplier + buffedStat.CriticalMultiplier + equipStat.CriticalMultiplier;
            DamageDeviation = baseStat.DamageDeviation + buffedStat.DamageDeviation + equipStat.DamageDeviation;
            Penetration = baseStat.Penetration + buffedStat.Penetration + equipStat.Penetration;
            ItemAcquisitionRange = baseStat.ItemAcquisitionRange + buffedStat.ItemAcquisitionRange + equipStat.ItemAcquisitionRange;
            CoolTimeReduce = baseStat.CoolTimeReduce + buffedStat.CoolTimeReduce + equipStat.CoolTimeReduce;
            SearchRange = baseStat.SearchRange + buffedStat.SearchRange + equipStat.SearchRange;
            AddProjectile = baseStat.AddProjectile + buffedStat.AddProjectile + equipStat.AddProjectile;
            LifeDrainChance = baseStat.LifeDrainChance + buffedStat.LifeDrainChance + equipStat.LifeDrainChance;
            LifeDrainRatio = baseStat.LifeDrainRatio + buffedStat.LifeDrainRatio + equipStat.LifeDrainRatio;
            KnockBackChance = baseStat.KnockBackChance + buffedStat.KnockBackChance + equipStat.KnockBackChance;
            KnockBackForce = baseStat.KnockBackForce + buffedStat.KnockBackForce + equipStat.KnockBackForce;
            InstKillChance = baseStat.InstKillChance + buffedStat.InstKillChance + equipStat.InstKillChance;
            PoisonChance = baseStat.PoisonChance + buffedStat.PoisonChance + equipStat.PoisonChance;
            PoisonDefaultTier = baseStat.PoisonDefaultTier + buffedStat.PoisonDefaultTier + equipStat.PoisonDefaultTier;
            BleedChance = baseStat.BleedChance + buffedStat.BleedChance + equipStat.BleedChance;
            BleedDefaultTier = baseStat.BleedDefaultTier + buffedStat.BleedDefaultTier + equipStat.BleedDefaultTier;
            StunChance = baseStat.StunChance + buffedStat.StunChance + equipStat.StunChance;
            StunDefaultTier = baseStat.StunDefaultTier + buffedStat.StunDefaultTier + equipStat.StunDefaultTier;

            return this;
        }

        public void SetHpToMax()
        {
            CurrentHp = MaxHp;
        }

        public void SetHpToHalf() 
        {
            CurrentHp = MaxHp * 0.5f;
        }

        public float CalculateCoolTime(float coolTime)
        {
            return coolTime - coolTime * CoolTimeReduce;
        }

        public void StatEnhancement(PlayerEnhanceData enhanceData)
        {
            MaxHp += enhanceData.MaxHp;
            Defense += enhanceData.Defence;
            MoveSpeed += enhanceData.MoveSpeed;
            AttackPower += enhanceData.AttackPower;
        }
    }
    
    public enum PlayerStatEnum
    {
        MaxHp,
        Defense,
        MoveSpeed,
        AttackPower,
        InvincibleTime,
        CriticalChance,
        CriticalResistance,
        CriticalMultiplier,
        DamageDeviation,
        Penetration,
        ItemAcquisitionRange,
        CoolTimeReduce,
        SearchRange,
        AddProjectile,
    }
}