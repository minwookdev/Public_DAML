using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractionNextFloor : Interaction {
        protected override void Start() {
            base.Start();
            InteractionType = InteractionType.DungeonNextFloor;
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
            DungeonSceneBase.Inst.RequestGenerateNextFloor();
            CatLog.Log("OnInteract: Generate Next Floor");
        }
    }
}