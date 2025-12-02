using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat.FrameWork
{
    public class UserDataManager : DynamicSingleton<UserDataManager>
    {
        // fields
        [SerializeField] private UserData userData = new();

        // proeprties
        public int Currency => userData.Currency;
        public int SelectedCharacterIndex
        {
            get => userData.selectedCharacterIndex;
            private set => userData.selectedCharacterIndex = value;
        }
        public List<CharacterData> CharacterDatas => userData.CharacterDatas;

        private void LoadUserData()
        {
            var isExistUserData = File.Exists(Defines.USER_DATA_PATH);
            if (isExistUserData)
            {
                var encryptedData = File.ReadAllText(Defines.USER_DATA_PATH);
                var decryptedData = Cryptor.Decrypt(encryptedData);
                userData = JsonConvert.DeserializeObject<UserData>(decryptedData);
                CatLog.Log("User Data Load Completed.");
            }
            else 
            {
                CatLog.Log("User Data file.enc is not exist. Create new User Data.");
            }
            
            userData.Patch();
            CatLog.Log("Patch User Data Completed.");
        }

        private void SaveUserData()
        {
            var serializedData = JsonConvert.SerializeObject(userData);
            var encryptedData = Cryptor.Encrypt(serializedData);
            File.WriteAllText(Defines.USER_DATA_PATH, encryptedData);
            CatLog.Log("User Data Save Completed.");
        }

        protected override void Initialize() => LoadUserData();

#if !UNITY_EDITOR && !UNITY_STANDALONE && (UNITY_ANDROID || UNITY_IOS)
        private void OnApplicationPause(bool pauseStatus){
            // Return to Application
            if (!pauseStatus) {
                return;
            }

            // save user data
            SaveUserData();
        }
#else
        protected override void InvokeOnApplicationQuit() 
        {
            SaveUserData();
        }
#endif

        public void IncreaseCurrency(int amount)
        {
            userData.Currency += amount;
        }

        public bool DecreaseCurrency(int amount)
        {
            if (userData.Currency < amount)
            {
                CatLog.WLog("Not enough currency.");
                return false;
            }

            userData.Currency -= amount;
            return true;
        }

        public CharacterData GetCharacterData(int index)
        {
            return CharacterDatas.Find(c => c.CharacterIndex == index);
        }

        public void SelectedCharacter(int index)
        {
            SelectedCharacterIndex = index;
            var selected = CharacterDatas.Find(c => c.SelectState == CharacterSelectState.SELECTED);
            selected.SelectState = CharacterSelectState.SELECTABLE;
            var selecting = GetCharacterData(index);
            if (selecting == null)
            {
                CatLog.WLog("Selecting Character Data is null");
                return;
            }
            selecting.SelectState = CharacterSelectState.SELECTED;
        }

        public void PurchasedCharacter(int index)
        {
            userData.AddCharacter(index);
        }
        
        public bool IsCompleteUpgrade(StatGrade grade)
        {
            var characterData = GetCharacterData(SelectedCharacterIndex);
            switch (grade)
            {
                case StatGrade.Beginner:
                    return characterData.IsCompleteBegginer;
                case StatGrade.Middle:
                    return characterData.IsCompleteMiddle;
                case StatGrade.High:
                    CatLog.Log("High Grade is not implemented yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(grade), grade, null);
            }

            return false;
        }

        public void SetCompleteUpgrade(StatGrade grade)
        {
            var characterData = GetCharacterData(SelectedCharacterIndex);
            switch (grade)
            {
                case StatGrade.Beginner:
                    characterData.IsCompleteBegginer = true;
                    break;
                case StatGrade.Middle:
                    characterData.IsCompleteMiddle = true;
                    break;
                case StatGrade.High:
                    CatLog.Log("High Grade is not implemented yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(grade), grade, null);
            }
        }

        private IndexLevelDictionary GetUpgradeProgress(StatGrade currentGrade)
        {
            var characterData = GetCharacterData(SelectedCharacterIndex);
            IndexLevelDictionary progress = null;
            switch (currentGrade)
            {
                case StatGrade.Beginner:
                    progress = characterData.UpgradeProgress_B;
                    break;
                case StatGrade.Middle:
                    progress = characterData.UpgradeProgress_M;
                    break;
                case StatGrade.High:
                    CatLog.Log("High Grade is not implemented yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(currentGrade), currentGrade, null);
            }

            if (progress == null)
            {
                CatLog.WLog("progress is null");
                return null;
            }
            if (progress.Count == 0) CatLog.WLog("Character Upgrade Data is not initialized");
            
            return progress;
        }

        public int GetStatCurrentLevel(StatGrade currentGrade, int statIndex)
        {
            var progress = GetUpgradeProgress(currentGrade);
            return progress[statIndex];
        }

        public int GetTotalCurrentUpgradeCount(StatGrade currentGrade)
        {
            var progress = GetUpgradeProgress(currentGrade);

            if (progress != null) return progress.Sum(pair => pair.Value);
            CatLog.Log("progress is null");
            return 0;
        }

        public void UpgradeStat(StatGrade currentGrade, int statIndex)
        {
            var progress = GetUpgradeProgress(currentGrade);
            progress[statIndex]++;
        }

        public int GetAbilityCurrentLevel(int abilityIndex)
        {
            return userData.AbilityProgress[abilityIndex];
        }
        
        public void UpgradeAbility(int abilityIndex)
        {
            userData.AbilityProgress[abilityIndex]++;
        }

        public IndexLevelDictionary GetAbilityProgress() => userData.AbilityProgress;
    }

    [Serializable]
    public class UserData
    {
        public int Currency = 0;
        public int selectedCharacterIndex = 0;
        public List<CharacterData> CharacterDatas = new();
        public IndexLevelDictionary AbilityProgress = new();
        public PlayerStat upgradeData = new();
        
        public void Patch() 
        {
            PatchCharacterData();
            PatchAbilityProgress();
        }

        private void PatchCharacterData() 
        {
            // add all character data if not exist
            var characterDatas = DataManager.Inst.CharacterSelectDatas;
            var characterStatUpgradeData = DataManager.Inst.PlayerStatUpgradeDatas; 
            var characterDataDict = characterDatas.GetDict();
            foreach (var pair in characterDataDict) 
            {
                var characterData = CharacterDatas.Find(data => data.CharacterIndex == pair.Key);
                if (characterData == null) 
                {
                    characterData = new CharacterData();
                    characterData.Init(pair.Key);
                    CharacterDatas.Add(characterData);
                    CatLog.Log("Added New Character Data: " + pair.Key.ToString());
                }
                
                var statUpgradeData = Array.Find(characterStatUpgradeData, data => data.CharacterIndex == characterData.CharacterIndex);
                if (!statUpgradeData) 
                {
                    throw new Exception("Invalid Character Stat Data Index: " + characterData.CharacterIndex);
                }
                
                // patch stat upgrade data
                characterData.Patch(statUpgradeData);
            }
            
            // remove invalid character
            List<int> removeKeys = new();
            foreach (var characterData in CharacterDatas) 
            {
                var key = characterData.CharacterIndex;
                var isExistOnData = characterDataDict.ContainsKey(key);
                if (!isExistOnData) 
                {
                    removeKeys.Add(key);
                }
            }
            
            for (int i = 0; i < removeKeys.Count; i++) 
            {
                var removeKey = removeKeys[i];
                CharacterDatas.RemoveAll(data => data.CharacterIndex == removeKey);
                CatLog.Log("Removed Invalid Character Data: " + removeKey.ToString());
            }
            
            // fix current selected character index
            if (selectedCharacterIndex < CharacterDatas.Count && CharacterDatas[selectedCharacterIndex].SelectState == CharacterSelectState.SELECTED)
            {
                return;
            }
            
            selectedCharacterIndex = 0;
            CharacterDatas[selectedCharacterIndex].SelectState = CharacterSelectState.SELECTED;
            CatLog.Log("Forced Set Default Character.");
        }
        
        private void PatchAbilityProgress()
        {
            // add all ability data if not exist
            var abilityDatas = DataManager.Inst.PlayerAbilityData;
            foreach (var data in abilityDatas.Datas) 
            {
                var isAddSuccessed = AbilityProgress.TryAdd(data.Index, 0);
                if (isAddSuccessed) 
                {
                    CatLog.Log("Added New Ability Data: " + data.Index.ToString());
                }
                
                // fix ability level
                var value = AbilityProgress[data.Index];
                if (value <= data.MaxLevel) 
                {
                    continue;
                }
                AbilityProgress[data.Index] = data.MaxLevel;
                CatLog.Log("Fixed Ability Level Key: " + data.Index.ToString() + " Value: " + value.ToString() + " To: " + data.MaxLevel.ToString());
            }
            
            // remove invalid ability
            List<int> removeKeys = new();
            foreach (var pair in AbilityProgress) 
            {
                var key = pair.Key;
                var isExistOnData = abilityDatas.Datas.Any(data => data.Index == key);
                if (!isExistOnData) 
                {
                    removeKeys.Add(key);
                }
            }

            for (int i = 0; i < removeKeys.Count; i++) 
            {
                var removeKey = removeKeys[i];
                AbilityProgress.Remove(removeKey);
                CatLog.Log("Removed Invalid Ability Data: " + removeKey.ToString());
            }
        }
        
        public void AddCharacter(int index)
        {
            CharacterDatas[index].SelectState = CharacterSelectState.SELECTABLE;
        }
    }

    [Serializable]
    public class CharacterData
    {
        public int CharacterIndex;
        public CharacterSelectState SelectState;
        public bool IsCompleteBegginer = false;
        public bool IsCompleteMiddle = false;
        public IndexLevelDictionary UpgradeProgress_B = null;
        public IndexLevelDictionary UpgradeProgress_M = null;

        public void Init(int index)
        {
            CharacterIndex = index;
            SelectState = CharacterSelectState.NOT_OWNED;
            IsCompleteBegginer = false;
            IsCompleteMiddle = false;
            UpgradeProgress_B = new IndexLevelDictionary();
            UpgradeProgress_M = new IndexLevelDictionary();
        }

        public void Patch(PlayerStatUpgradeData statUpgradeData) 
        {
            // add new character upgrade data if not exist
            var beginnerDatas = statUpgradeData.BegginerDatas;
            for (var i = 0; i < beginnerDatas.Count; i++) 
            {
                var data = beginnerDatas[i];
                if (UpgradeProgress_B.TryAdd(data.Index, 0)) 
                {
                    CatLog.Log("Added New Beginner Upgrade Data: " + data.Index.ToString() + " Character index: " + CharacterIndex.ToString());
                }
                // fix beginner upgrade level
                var value = UpgradeProgress_B[data.Index];
                if (value <= data.MaxGrade) 
                {
                    continue;
                }
                CatLog.Log("Fixed Beginner Upgrade Level Value: " + value.ToString() + "To: " + data.MaxGrade.ToString() + " Character index: " + CharacterIndex.ToString());
                UpgradeProgress_B[data.Index] = data.MaxGrade;
            }
            
            var middleDatas = statUpgradeData.MiddleDatas;
            for (var i = 0; i < middleDatas.Count; i++) 
            {
                var data = middleDatas[i];
                if (UpgradeProgress_M.TryAdd(data.Index, 0)) 
                {
                    CatLog.Log("Added New Middle Upgrade Data: " + data.Index.ToString() + " Character index: " + CharacterIndex.ToString());
                }
                
                // fix middle upgrade level
                var value = UpgradeProgress_M[data.Index];
                if (value <= data.MaxGrade) 
                {
                    continue;
                }
                CatLog.Log("Fixed Middle Upgrade Level Value: " + value.ToString() + "To: " + data.MaxGrade.ToString() + " Character index: " + CharacterIndex.ToString());
                UpgradeProgress_M[data.Index] = data.MaxGrade;
            }

            // remove invalid character upgrade data
            List<int> removeKeys = new();
            foreach (var pair in UpgradeProgress_B) 
            {
                var key = pair.Key;
                var isExistOnData = beginnerDatas.Any(data => data.Index == key);
                if (!isExistOnData) 
                {
                    removeKeys.Add(key);
                }
            }
            
            for (int i = 0; i < removeKeys.Count; i++) 
            {
                var removeKey = removeKeys[i];
                UpgradeProgress_B.Remove(removeKey);
                CatLog.Log("Removed Invalid Beginner Upgrade Data: " + removeKey.ToString() + " Character index: " + CharacterIndex.ToString());
            }
            
            removeKeys.Clear();
            foreach (var pair in UpgradeProgress_M) 
            {
                var key = pair.Key;
                var isExistOnData = middleDatas.Any(data => data.Index == key);
                if (!isExistOnData) 
                {
                    removeKeys.Add(key);
                }
            }
            
            for (int i = 0; i < removeKeys.Count; i++) 
            {
                var removeKey = removeKeys[i];
                UpgradeProgress_M.Remove(removeKey);
                CatLog.Log("Removed Invalid Middle Upgrade Data: " + removeKey.ToString() + " Character index: " + CharacterIndex.ToString());
            }
        }
    }
}