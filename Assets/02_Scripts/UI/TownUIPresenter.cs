using System;
using UnityEngine;
using TMPro;
using CoffeeCat.FrameWork;
using CoffeeCat.UI;

namespace CoffeeCat
{
    public class TownUIPresenter : SceneSingleton<TownUIPresenter>
    {
        [SerializeField] private EnhancementStatPanel enhancementStatPanel;
        [SerializeField] private DungeonSelectPanel dungeonSelectPanel;
        [SerializeField] private DungeonEnteringConfirm dungeonEnteringConfirmPanel;
        [SerializeField] private CharacterSelectPanel characterSelectPanel;
        [SerializeField] private CharacterUpgradePanel characterUpgradePanel;
        [SerializeField] private PlayerAbilityPanel abilityPanel;
        [SerializeField] private TextMeshProUGUI tmpCurrency = null;

        public void Init()
        {
            characterSelectPanel.Init();
            characterUpgradePanel.Init();
            abilityPanel.Init();
            UpdateCurrency(UserDataManager.Inst.Currency);
            TownEvtManager.AddCurrencyChangedListener(UpdateCurrency);
        }

        private void OnDisable()
        {
            TownEvtManager.RemoveCurrencyChangedListener(UpdateCurrency);
        }

        public void ActiveEnhancementPanel()
        {
            enhancementStatPanel.Enable();
        }

        public void ActiveDungeonSelectPanel()
        {
            dungeonSelectPanel.Active();
        }

        public void ActiveCharacterSelectPanel()
        {
            characterSelectPanel.Active();
        }

        public void ActiveCharacterUpgradePanel()
        {
            characterUpgradePanel.Active();
        }
        
        public void ActiveAbilityPanel()
        {
            abilityPanel.Active();
        }

        public void ActiveDuneonConfirmPanel(string dungeonName, string dungeonKey)
        {
            dungeonEnteringConfirmPanel.Active(dungeonName, dungeonKey);
        }

        public void RefreshCharacterUpgradePanel()
        {
            characterUpgradePanel.RefreshUpgradePanel();
        }

        public void UpdateCurrency(int amount) => tmpCurrency.SetText(amount.ToStringN0());
    }
}