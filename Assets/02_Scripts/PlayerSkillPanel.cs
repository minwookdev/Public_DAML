using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;

namespace CoffeeCat {
    public class PlayerSkillPanel : MonoBehaviour {
        [FoldoutGroup("ActiveSkill"), SerializeField] private TextMeshProUGUI tmpActiveSkillCount = null;
        [FoldoutGroup("ActiveSkill"), SerializeField] private CommonIconSlot[] activeSkillSlots = null;

        [FoldoutGroup("PassiveSkill"), SerializeField] private TextMeshProUGUI tmpPassiveSkillCount = null;
        [FoldoutGroup("PassiveSkill"), SerializeField] private CommonIconSlot[] passiveSkillSlots = null;
        
        [FoldoutGroup("SkillDescription"), SerializeField] private Image imgSkillIcon = null;
        [FoldoutGroup("SkillDescription"), SerializeField] private TextMeshProUGUI tmpSkillName = null;
        [FoldoutGroup("SkillDescription"), SerializeField] private TextMeshProUGUI tmpSkillDesc = null;
        [FoldoutGroup("SkillDescription"), SerializeField] private TextMeshProUGUI tmpSkillCoolTime = null;
        [FoldoutGroup("SkillDescription"), SerializeField] private string defaultSkillNameTextKey = string.Empty;
        [FoldoutGroup("SkillDescription"), SerializeField] private string defaultSkillDescTextKey = string.Empty;
        [FoldoutGroup("SkillDescription"), SerializeField] private string defaultSkillCoolTimeTextKey = string.Empty;
        
        private Player_Dungeon player = null;
        private bool Flag_Require_Upate_Skills = true;

        private void SetFlaggedUpdateSkill() => Flag_Require_Upate_Skills = true;
        
        private void OnEnable() {
            SetPlayerIfNull();
            ClearSkillDescription();
            
            if (!Flag_Require_Upate_Skills) {
                return;
            }
            UpdateActiveSkill();
            UpdatePassiveSkill();
        }

        private void Start() => DungeonEvtManager.AddEventSkillSelectCompleted(SetFlaggedUpdateSkill);

        private void OnDestroy() => DungeonEvtManager.RemoveEventSkillSelectCompleted(SetFlaggedUpdateSkill);

        private void SetPlayerIfNull() {
            player ??= RogueLiteManager.Inst.SpawnedPlayer;
            if (!player) {
                CatLog.WLog("Player is Null !");    
            }
        }
        
        #region Active / Passive

        private void UpdateActiveSkill() {
            // set active skill count text
            var activeSkills = player.ActiveSkills;
            string activeSkillCount = $"{activeSkills.Count.ToString()} / {player.MaxActiveSkillCount.ToString()}";
            tmpActiveSkillCount.SetText(activeSkillCount);
            
            // set active skill slots
            for (int i = 0; i < activeSkillSlots.Length; i++) {
                var slot = activeSkillSlots[i];
                slot.Disable();
            }

            VerifySlotCount(activeSkillSlots, activeSkills.Count);
            for (int i = 0; i < activeSkills.Count; i++) {
                var skill = activeSkills[i];
                activeSkillSlots[i].Set(skill.IconKey, () => {
                    UpdateSkillDescription(skill);
                });
            }

            Flag_Require_Upate_Skills = false;
        }
        
        private void UpdatePassiveSkill() {
            // set passive skill count text
            var passiveSkills = player.PassiveSkills;
            string passiveSkillCount = $"{passiveSkills.Count.ToString()} / {player.MaxPassiveSkillCount.ToString()}";
            tmpPassiveSkillCount.SetText(passiveSkillCount);
            
            // set passive skill slots
            for (int i = 0; i < passiveSkillSlots.Length; i++) {
                var slot = passiveSkillSlots[i];
                slot.Disable();
            }
            
            VerifySlotCount(passiveSkillSlots, passiveSkills.Count);
            for (int i = 0; i < passiveSkills.Count; i++) {
                var skill = passiveSkills[i];
                passiveSkillSlots[i].Set(skill.IconKey, () => {
                    UpdateSkillDescription(skill);
                });
            }
        }

        private void VerifySlotCount(CommonIconSlot[] targetSlots, int count) {
#if UNITY_EDITOR || ENABLE_LOG
            if (targetSlots.Length < count) {
                CatLog.WLog("Skill Slot Count is not enough !");
            }
#endif
        }
        
        #endregion
        
        #region Description

        private void UpdateSkillDescription(PlayerSkill skill) {
            // set texts
            tmpSkillName.SetText(skill.SkillName);
            tmpSkillDesc.SetText(skill.Description);
            tmpSkillCoolTime.SetText($"{skill.SkillCoolTime.ToStringN2()} sec");

            // load skill icon sprite
            ResourceManager.Inst.AddressablesAsyncLoad<Sprite>(skill.IconKey, false, (sprite) => {
                if (!sprite) {
                    return;
                }

                imgSkillIcon.sprite = sprite;
            });
            
            SoundManager.Inst.PlayButtonSE(true);
        }

        private void ClearSkillDescription() {
            tmpSkillName.SetText(defaultSkillNameTextKey);
            tmpSkillDesc.SetText(defaultSkillDescTextKey);
            tmpSkillCoolTime.SetText(defaultSkillCoolTimeTextKey);
            imgSkillIcon.sprite = null;
        }
        
        #endregion
    }
}