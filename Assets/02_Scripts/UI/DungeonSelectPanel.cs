using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CoffeeCat.FrameWork;

namespace CoffeeCat
{
    public class DungeonSelectPanel : MonoBehaviour
    {
        [SerializeField] private DungeonEnteringSlot originSlot = null;
        [SerializeField] private List<DungeonEnteringSlot> spawnedSlotList = null;
        [SerializeField] private Button[] btnCloses = null;

        private void Start() {
            // set dungeon information slots
            SetSlots();

            // add button event
            for (int i = 0; i < btnCloses.Length; i++) {
                var btnClose = btnCloses[i];
                btnClose.onClick.AddListener(Deactive);
            }
        }

        public void Active() {
            gameObject.SetActive(true);
        }

        private void Deactive() {
            gameObject.SetActive(false);
        }

        private void SetSlots() {
            var dungeonDatas = DataManager.Inst.DungeonInfos;
            if (dungeonDatas == null) {
                return;
            }

            // spawn new require slots
            originSlot.gameObject.SetActive(false);
            var requireValue = dungeonDatas.Length - spawnedSlotList.Count;
            if (requireValue > 0) // need more slot
            {
                for (int i = 0; i < requireValue; i++) {
                    var newSlot = Instantiate(originSlot, originSlot.transform.parent);
                    spawnedSlotList.Add(newSlot);
                }
            }

            // enable slot
            for (int i = 0; i < dungeonDatas.Length; i++) {
                var data = dungeonDatas[i];
                spawnedSlotList[i].Active(data);
            }
        }
        
        private void ClearSlots()
        {
            // clear slots
            for (int i = 0; i < spawnedSlotList.Count; i++)
            {
                spawnedSlotList[i].Clear();
            }
        }
    }
}