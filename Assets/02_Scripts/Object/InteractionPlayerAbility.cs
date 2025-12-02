using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractionPlayerAbility : Interaction {
        protected override void Start() {
            base.Start();
            InteractionType = InteractionType.PlayerAbility;
        }

        public override void Interact() {
            CatLog.Log("OnInteract: Player Ability");
            TownUIPresenter.Inst.ActiveAbilityPanel();
        }
    }
}