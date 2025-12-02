using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils.SerializedDictionaries;

namespace CoffeeCat {
    public class StartRoomObject : MonoBehaviour {
        [SerializeField] private Transform[] rewardItemSpawnPoses = null;

        public void Init(StartRoomTableDictionary tableDict) {
            for (int i = 0; i < rewardItemSpawnPoses.Length; i++) {
                var tr = rewardItemSpawnPoses[i];
                tr.parent.gameObject.SetActive(false);
            }

            int index = 0;
            foreach (var pair in tableDict) {
                var lootList = pair.Value;
                var raffle = lootList.Raffle();
                
                var request = new ItemLootRequest {
                    Code = raffle.Code,
                    Amount = raffle.GetAmount()
                };
                
                rewardItemSpawnPoses[index].parent.gameObject.SetActive(true);
                var itemSpawnPos = rewardItemSpawnPoses[index].position;
                var objectItem = ObjectPoolManager.Inst.Spawn<ItemObject>(AddressablesKey.Object_Item.ToKey(), itemSpawnPos, Quaternion.identity);
                objectItem.Init(in request, false);
                
                index++;
            }
        }
    }
}
