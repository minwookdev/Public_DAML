using System.Collections.Generic;
using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;

namespace CoffeeCat {
    public class MiniSkillPanel : MonoBehaviour {
        [SerializeField] private RectTransform activeSlotParentRectTr = null;
        [SerializeField] private RectTransform passiveSlotParentRectTr = null;
        [SerializeField] private List<MiniSkillSlot> activeSlots = null;
        [SerializeField] private List<MiniSkillSlot> passiveSlots = null;

        private Player_Dungeon spawnedPlayer = null;

        private void OnEnable() => DungeonEvtManager.AddEventSkillSelectCompleted(UpdatePanel);

        private void OnDisable() => DungeonEvtManager.RemoveEventSkillSelectCompleted(UpdatePanel);

        private void SetPlayerIfNull() {
            if (spawnedPlayer) {
                return;
            }
            spawnedPlayer = RogueLiteManager.Inst.SpawnedPlayer;
        }
        
        private void UpdatePanel() {
            SetPlayerIfNull();
            
            var activeSkills = spawnedPlayer.ActiveSkills;
            var passiveSkills = spawnedPlayer.PassiveSkills;
            
            UIHelper.AddIfRequired(activeSlots, activeSlots[0], activeSkills.Count, activeSlotParentRectTr);
            UIHelper.AddIfRequired(passiveSlots, passiveSlots[0], passiveSkills.Count, passiveSlotParentRectTr);
            
            ClearWithDisableSlots(activeSlots);
            ClearWithDisableSlots(passiveSlots);

            for (int i = 0; i < activeSkills.Count; i++) {
                activeSlots[i].Init(activeSkills[i].IconKey);
            }

            for (int i = 0; i < passiveSkills.Count; i++) {
                passiveSlots[i].Init(passiveSkills[i].IconKey);
            }
        }
        
        private void ClearWithDisableSlots(List<MiniSkillSlot> targetSlotList) {
            for (int i = 0; i < targetSlotList.Count; i++) {
                targetSlotList[i].ClearWithDisable();
            }
        }
    }   
}