using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractionEntranceDungeon : Interaction {
        protected override void Start() {
            base.Start();
            InteractionType = InteractionType.EntranceDungeon;
        }

        public override void Interact() {
            TownUIPresenter.Inst.ActiveDungeonSelectPanel();
            CatLog.Log("OnInteract: Open Dungeon Select Panel");
        }
    }
}
