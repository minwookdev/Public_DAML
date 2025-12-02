using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CoffeeCat.FrameWork;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CoffeeCat.UI
{
    public class StatUpgradeSlot : MonoBehaviour
    {
        public RectTransform rectTr;
        public Button btnUpgrade;
        public TextMeshProUGUI tmpNameAndProgress;
        public TextMeshProUGUI tmpPrice;
        public TextMeshProUGUI tmpGrade;
        public Image sliderProgress;
        public Image imgIcon;
        
        private const string iconKey = "icon_stat_";
        private int price;

        public void SetSlot(StatUpgradeData data, StatGrade currentGrade)
        {
            rectTr.localScale = Vector3.one;
            var currentLevel = UserDataManager.Inst.GetStatCurrentLevel(currentGrade, data.Index);
            var value = data.ValueIncrease * currentLevel;
            tmpNameAndProgress.text = $"{data.Name.ToName()} (+{value.ToString()})";
            SetIcon(currentGrade);
            SetPrice(data.BasePrice, data.PriceIncrease, currentLevel, data.MaxGrade);
            SetGrade(currentLevel, data.MaxGrade);
            SetProgressSlide(currentLevel, data.MaxGrade);
            btnUpgrade.onClick.RemoveAllListeners();
            btnUpgrade.onClick.AddListener(() => OnUpgradeBtnClick(data, currentGrade));
        }

        private void SetPrice(int basePrice, int priceIncrease, int currentGrade, int maxGrade)
        {
            if (currentGrade >= maxGrade)
            {
                tmpPrice.text = "MAX";
                return;
            }

            var result = basePrice + (priceIncrease * currentGrade);
            var priceStr = result.ToStringN0();

            price = result;
            tmpPrice.text = $"{priceStr} G";
        }

        private void SetGrade(int currentGrade, int maxGrade)
        {
            tmpGrade.text = $"( {currentGrade.ToString()} / {maxGrade.ToString()} )";
        }

        private void SetProgressSlide(int currentGrade, int maxGrade)
        {
            sliderProgress.fillAmount = (float)currentGrade / maxGrade;
        }

        private void SetIcon(StatGrade grade)
        {
            var key = string.Empty;

            switch (grade)
            {
                case StatGrade.Beginner:
                    key = iconKey + "01";
                    break;
                case StatGrade.Middle:
                    key = iconKey + "02";
                    break;
                case StatGrade.High:
                    // key = iconKey + "03";
                    CatLog.Log("high grade is not implemented yet");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(grade), grade, null);
            }

            ResourceManager.Inst.AddressablesAsyncLoad<Sprite>(key, false, icon =>
            {
                if (!icon)
                {
                    CatLog.WLog($"Not Found Icon : {key}");
                    return;
                }
                
                imgIcon.sprite = icon;
            });
        }

        private void OnUpgradeBtnClick(StatUpgradeData data, StatGrade currentGrade)
        {
            var currentLevel = UserDataManager.Inst.GetStatCurrentLevel(currentGrade, data.Index);
            
            if (currentLevel >= data.MaxGrade)
            {
                CatLog.Log("already max grade");
                return;
            }

            var currency = UserDataManager.Inst.Currency;
            if (currency < price)
            {
                SoundManager.Inst.PlayButtonSE(false);
                CatLog.Log("currency is not enough");
                return;
            }

            SoundManager.Inst.PlayButtonSE(true);
            UserDataManager.Inst.DecreaseCurrency(price);
            TownEvtManager.InvokeCurrencyChanged(UserDataManager.Inst.Currency);
            UserDataManager.Inst.UpgradeStat(currentGrade, data.Index);
            TownEvtManager.InvokePlayerUpgradeProgress();
            SetSlot(data, currentGrade);
        }
    }
}