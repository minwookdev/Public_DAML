using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractionPlayerStat : Interaction {
        protected override void Start() {
            base.Start();
            InteractionType = InteractionType.PlayerUpgrade;
        }

        public override void Interact() {
            CatLog.Log("OnInteract: Player Stat");
            TownUIPresenter.Inst.ActiveCharacterUpgradePanel();
        }
    }
}