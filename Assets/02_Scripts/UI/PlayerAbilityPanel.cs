using System.Collections.Generic;
using CoffeeCat.FrameWork;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat.UI
{
    public class PlayerAbilityPanel : MonoBehaviour
    {
        private readonly Dictionary<int, PlayerAbilitySlot> abilitySlots = new();
        
        [Title("Top")]
        public Button btnExit;
        public Button btnBegginer;
        public Button btnMiddle;
        public Button btnHigh;
        public Transform content;

        [Title("Center")]
        public GameObject panelAbilityInfo;
        public TextMeshProUGUI tmpAbilityName;
        public TextMeshProUGUI tmpAbilityLevel;
        public TextMeshProUGUI tmpAbilityPrice;
        public TextMeshProUGUI tmpAbilityDesc;

        [Title("Bottom")]
        public Button btnBuy;
        public Button btnClose;

        private void OnDestroy()
        {
            TownEvtManager.RemoveAbilityUpgradeListener(RefreshAbilityData);
        }

        public void Init()
        {
            btnExit.onClick.AddListener(Close);
            btnClose.onClick.AddListener(Close);
            panelAbilityInfo.SetActive(false);
            btnMiddle.gameObject.SetActive(false);
            btnHigh.gameObject.SetActive(false);
            SetSlots();
            TownEvtManager.AddAbilityUpgradeListener(RefreshAbilityData);            
        }

        private void SetSlots()
        {
            abilitySlots.Clear();
            var abiltyData = DataManager.Inst.PlayerAbilityData;
            
            foreach (var ability in abiltyData.Datas)
            {
                var slot = ObjectPoolManager.Inst.Spawn<PlayerAbilitySlot>("abilitySlot", content);
                var currentLevel = UserDataManager.Inst.GetAbilityCurrentLevel(ability.Index);
                slot.SetSlot(ability.Index, ability, currentLevel);
                slot.gameObject.SetActive(true);
                abilitySlots.Add(ability.Index, slot);
            }
        }
        
        private void RefreshAbilityData(int slotIndex, PlayerAbility ability)
        {
            var currentLevel = UserDataManager.Inst.GetAbilityCurrentLevel(ability.Index);
            var price = ability.Price + ability.PriceIncrease * currentLevel;
            var desc = SetDescripsion(ability, currentLevel);
            
            panelAbilityInfo.SetActive(true);
            tmpAbilityName.text = ability.AbilityName;
            tmpAbilityLevel.text = $"( {currentLevel} / {ability.MaxLevel} )";
            tmpAbilityPrice.text = $"{price.ToStringN0()} G";
            tmpAbilityDesc.text = desc;
            
            btnBuy.onClick.RemoveAllListeners();
            btnBuy.onClick.AddListener(() => OnBuyBtnClick(slotIndex, ability));
        }

        private string SetDescripsion(PlayerAbility ability, int currentLevel)
        {
            var nameStr = ability.type.ToName();
            var valueStr = (ability.Value * currentLevel).ToString();
            var descStr = ability.Description + $"\n\n현재 [ {nameStr} +{valueStr} ] 적용중";

            return descStr;
        }

        private void OnBuyBtnClick(int slotIndex, PlayerAbility ability)
        {
            var currentLevel = UserDataManager.Inst.GetAbilityCurrentLevel(ability.Index);
            var isMaxLevel = ability.IsMaxLevel(currentLevel);
            
            if (isMaxLevel)
            {
                CatLog.Log("Max Level");
                return;
            }
            
            var currency = UserDataManager.Inst.Currency;
            if (currency < ability.Price)
            {
                SoundManager.Inst.PlayButtonSE(false);
                CatLog.Log("currency is not enough");
                return;
            }

            SoundManager.Inst.PlayButtonSE(true);
            UserDataManager.Inst.UpgradeAbility(ability.Index);
            UserDataManager.Inst.DecreaseCurrency(ability.Price);
            TownEvtManager.InvokeCurrencyChanged(UserDataManager.Inst.Currency);
            RefreshAbilityData(slotIndex, ability);
            abilitySlots[slotIndex].SetSlot(slotIndex, ability, currentLevel);
        }

        public void Active()
        {
            gameObject.SetActive(true);
        }

        private void Close()
        {
            SoundManager.Inst.PlayButtonSE(false);
            panelAbilityInfo.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}