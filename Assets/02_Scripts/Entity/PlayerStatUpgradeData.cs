using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CoffeeCat
{
    [Serializable, CreateAssetMenu(menuName = "CoffeeCat/Scriptable Object/PlayerStatUpgradeData")]
    public class PlayerStatUpgradeData : ScriptableObject
    {
        [SerializeField] private StatUpgradeDataList beginnerDatas = new();
        [SerializeField] private StatUpgradeDataList middleDatas = new();
        [SerializeField] private int characterIndex = 0;

        public List<StatUpgradeData> BegginerDatas => beginnerDatas.List;
        public List<StatUpgradeData> MiddleDatas => middleDatas.List;
        public int CharacterIndex => characterIndex;

        [PropertySpace(10f), Button("Data All Clear", ButtonSizes.Medium)]
        public void DataClear()
        {
            beginnerDatas.Clear();
            middleDatas.Clear();
        }

        public int GetTotalMaxUpgradeCount(StatGrade grade)
        {
            var datas = new List<StatUpgradeData>();
            switch (grade)
            {
                case StatGrade.Beginner:
                    datas = BegginerDatas;
                    break;
                case StatGrade.Middle:
                    datas = MiddleDatas;
                    break;
                case StatGrade.High:
                    CatLog.Log("High Grade is not implemented yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(grade), grade, null);
            }

            return datas.Sum(data => data.MaxGrade);
        }
    }

    [Serializable]
    public class StatUpgradeDataList
    {
        public List<StatUpgradeData> List = new();
        
        public void Clear()
        {
            List.Clear();
        }
    }

    [Serializable]
    public class StatUpgradeData
    {
        public PlayerStatEnum Name;
        public int Index;
        public int MaxGrade = default;
        public int BasePrice = default;
        public int PriceIncrease = default;
        public float ValueIncrease = default;

        public StatUpgradeData(PlayerStatEnum name)
        {
            Name = name;
        }
    }

    public enum StatGrade
    {
        Beginner,
        Middle,
        High
    }
}