using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.UI;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat
{
    [Serializable, CreateAssetMenu(menuName = "CoffeeCat/Scriptable Object/CharacterSelectData")]
    public class CharacterSelectDatas : ScriptableObject
    {
        [SerializeField] private List<CharacterSelectData> characterInfos = new();
        private Dictionary<int, CharacterSelectData> characterInfoDict = new();

        public void Init()
        {
            characterInfoDict.Clear();
            foreach (var info in characterInfos)
            {
                characterInfoDict.Add(info.CharacterIndex, info);
            }
        }

        public int GetCharacterCount()
        {
            return characterInfos.Count;
        }

        public string GetSkeletonDataAssetKeys(int characterIndex)
        {
            var data = characterInfos.Find(c => c.CharacterIndex == characterIndex);
            if (data != null) return data.SkeletonAddressableKey;
            CatLog.WLog($"Not Found CharacterIndex : {characterIndex}");
            return string.Empty;
        }
        
        public CharacterSelectData GetCharacterInfo(int index)
        {
            return characterInfoDict[index];
        }

        public Dictionary<int, CharacterSelectData> GetDict() 
        {
            return characterInfoDict;
        }
        
        public int GetLastCharacterIndex()
        {
            var characterInfo = characterInfos[^1];
            return characterInfo.CharacterIndex;
        }
    }

    [Serializable]
    public class CharacterSelectData
    {
        public PlayerAddressablesKey TownSpawnKey;
        public PlayerAddressablesKey DungeonSpawnKey;
        public string SkeletonAddressableKey;
        public int CharacterIndex;
        public string CharacterName;
        public string CharacterJob;
        public int CharacterPrice;
        [Range(0, 8)] public int Power;
        [Range(0, 8)] public int Vitality;
        [Range(0, 8)] public int Utility;
        [TextArea] public string Description;
    }
    
    public enum CharacterSelectState
    {
        NOT_OWNED,
        SELECTED,
        SELECTABLE,
    }
}