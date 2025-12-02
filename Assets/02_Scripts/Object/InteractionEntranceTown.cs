using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractionEntranceTown : Interaction {
        protected override void Start() {
            base.Start();
            InteractionType = InteractionType.EntranceTown;
            infoText.gameObject.SetActive(false);
        }

        protected override void OnPlayerEnter() {
            base.OnPlayerEnter();
            infoText.gameObject.SetActive(true);
        }
        
        protected override void OnPlayerExit() {
            base.OnPlayerExit();
            infoText.gameObject.SetActive(false);
        }

        public override void Interact() {
            RogueLiteManager.Inst.OnExitInteract();
            DungeonSceneBase.Inst.RequestToTownScene();
            SaveCurrency();
            CatLog.Log("OnInteract: Go To Town");
        }
        
        private void SaveCurrency()
        {
            var currency = RogueLiteManager.Inst.PlayerInvenSystem.Currency;
            UserDataManager.Inst.IncreaseCurrency(currency);
        }
    }
}