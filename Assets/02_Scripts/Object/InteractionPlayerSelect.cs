using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractionPlayerSelect : Interaction {
        protected override void Start() {
            base.Start();
            InteractionType = InteractionType.PlayerSelect;
        }

        public override void Interact() {
            CatLog.Log("OnInteract: Player Select");
            TownUIPresenter.Inst.ActiveCharacterSelectPanel();
        }
    }
}