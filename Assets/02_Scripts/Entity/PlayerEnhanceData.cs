using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.Utils.Defines;
using Newtonsoft.Json;
using UnityEngine;

namespace CoffeeCat
{
    [Serializable]
    public class PlayerEnhanceData
    {
        public float MaxHp = default;
        public float Defence = default;
        public float MoveSpeed = default;
        public float AttackPower = default;

        // 불러오기
        public static PlayerEnhanceData GetEnhanceData()
        {
            var jsonData = PlayerPrefs.GetString(Defines.PLAYER_ENHANCE_DATA_KEY);
            jsonData = Cryptor.Decrypt(jsonData);
            
            var enhanceData = JsonConvert.DeserializeObject<PlayerEnhanceData>(jsonData);
            return enhanceData;
        }

        // 저장
        public void SaveEnhanceData()
        {
            var result = JsonConvert.SerializeObject(this);
            result = Cryptor.Encrypt(result);
            PlayerPrefs.SetString(Defines.PLAYER_ENHANCE_DATA_KEY, result);
        }
    }
}