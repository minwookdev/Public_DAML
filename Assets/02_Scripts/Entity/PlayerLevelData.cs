using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat
{
    [Serializable] public class IntIntDictionary : UnitySerializedDictionary<int, float> { }
    [Serializable, CreateAssetMenu(menuName = "CoffeeCat/Scriptable Object/PlayerLevelData")]
    public class PlayerLevelData : ScriptableObject
    {
        [SerializeField] private IntIntDictionary levelToExp = default;

        private int currentLevel = 1;
        private float currentExp = 0;

        public void Initialize()
        {
            currentLevel = 1;
            currentExp = 0;
        }
        
        public float GetExpToNextLevel()
        {
            return levelToExp[currentLevel];
        }
        
        public void AddExp(float exp)
        {
            currentExp += exp;
        }
        
        public bool isReadyLevelUp()
        {
            if (currentExp >= GetExpToNextLevel())
                return true;

            return false;
        }

        public void LevelUp()
        {
            currentExp -= GetExpToNextLevel();
            currentLevel++;
        }

        public void ForcedLevelUp() {
            currentExp = 0;
            currentLevel++;
        }

        public int GetCurrentLevel() => currentLevel;
        
        public float GetCurrentExp() => currentExp;

#if UNITY_EDITOR
        [PropertySpace(10f), Button("Add Level Data From ~ To")]
        private void AddLevelData(int from, int to) {
            if (from <= 0 || from >= to) {
                CatLog.WLog("Invalid Level Range");
                return;
            }
            
            for (int i = from; i <= to; i++) {
                if (levelToExp.TryAdd(i, 0)) {
                    continue;
                }
                CatLog.WLog($"Already Exist Level Data {i.ToString()}");
            }
        }
#endif
    }
}