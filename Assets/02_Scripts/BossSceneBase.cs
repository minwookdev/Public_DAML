using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class BossSceneBase : SceneSingleton<BossSceneBase> {
        [Title("Spawn Point Transforms")]
        [field: SerializeField] public Transform PlayerSpawnTr { get; private set; } = null;
        [field: SerializeField] public Transform ReturnPortalSpawnTr { get; private set; } = null;
        [field: SerializeField] public Transform RewardSpawnTr { get; private set; } = null;

        private void Start() {
            // Positioning Portal
            var interactable = ObjectPoolManager.Inst.Spawn<InteractionEntranceTown>(InteractionType.EntranceTown.ToKey(), ReturnPortalSpawnTr.position);
            if (interactable) {
                interactable.PlayParticle();
            }

            var DungeonBluePrint = DungeonSceneBase.Inst.DungeonBluePrint;
            var rewardRoomLootTable = DungeonBluePrint.RewardItemTable[RoomGradeType.T1];
            var rewardInteractable = ObjectPoolManager.Inst.Spawn<InteractionDungeonReward>(InteractionType.DungeonReward.ToKey(), RewardSpawnTr.position);
            rewardInteractable.Init(rewardRoomLootTable);
            
            // Positioning Player
            RogueLiteManager.Inst.SetPlayerOnEnteredDungeon(PlayerSpawnTr.position);
        }
    }
}