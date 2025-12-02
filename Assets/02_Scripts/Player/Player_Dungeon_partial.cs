using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;
using Random = UnityEngine.Random;

namespace CoffeeCat
{
    public partial class Player_Dungeon
    {
        [Title("Skill")]
        private int currentSkillPoint = 0;

        [SerializeField] private List<PlayerSkill> activeSkills = new();
        [SerializeField] private List<PlayerSkill> passiveSkills = new();
        [SerializeField] private List<PlayerSkill> equipmentSkills = new();
        private readonly List<PlayerSkill> reservedSkills = new();
        private readonly List<PlayerSkill> upgradeableSkills = new();
        private readonly Dictionary<int, PlayerSkillEffect> activeSkillEffects = new();
        private readonly List<GameObject> equipmentSkillEffects = new();

        private PlayerSkillDatas skillData = null;
        private const int DEFAULT_PROVIDE_SKILL_COUNT = Defines.PLAYER_SKILL_SELECT_COUNT;
        private const int DEFAULT_MAX_ACTIVE_SKILL_COUNT = 5;
        private const int DEFAULT_MAX_PASSIVE_SKILL_COUNT = 5;
        
        public int MaxActiveSkillCount => DEFAULT_MAX_ACTIVE_SKILL_COUNT;
        public int MaxPassiveSkillCount => DEFAULT_MAX_PASSIVE_SKILL_COUNT;
        public List<PlayerSkill> ActiveSkills => activeSkills;
        public List<PlayerSkill> PassiveSkills => passiveSkills;
        
        private void SetSkillData()
        {
            // set skilldatas
            skillData = DataManager.Inst.PlayerMainSkills;

            // add NormalAttack
            UpdateSkillData(1000);
            DungeonEvtManager.InvokeEventSkillSelectCompleted();
        }
        
        // 현재 보유중인 스킬들 활성화 (PlayerEnteredMonsterRoom에서 호출)
        public void ActiveSkillEffect()
        {
            for (int i = 0; i < activeSkills.Count; i++)
            {
                var skill = activeSkills[i];
                var skillEffect = activeSkillEffects[skill.GroupIndex];
                skillEffect.OnDispose();
                skillEffect.SkillEffect();
            }
        }

        private void ActiveSkillSelectPanel()
        {
            // clear temp list
            reservedSkills.Clear();

            // 강화 가능한 스킬과 새로운 스킬을 배울수있는지에 대한 여부를 미리 리스트로 베이크함
            // 해당 리스트를 통해 뽑을 수 있는 스킬 선택지를 만들어줌

            // provice skill select 
            int provideCounts = DEFAULT_PROVIDE_SKILL_COUNT;
            for (int i = 0; i < provideCounts; i++)
            {
                // 하지만 같은 선택지가 발생해서는 안된다
                bool hasUpgradeableSkill = HasUpgradeableSkill();
                bool canAssignedNewSkill = CanAssignedNewSkill();
                switch (hasUpgradeableSkill, canAssignedNewSkill)
                {
                    // 둘 다 가능한 경우
                    case (true, true):
                        SelectUpgOrNewSkill();
                        break;
                    // 업그레이드만 가능한 경우
                    case (true, false):
                        ReserveUpgSkill();
                        break;
                    // 새로운 스킬 할당만 가능한 경우
                    case (false, true):
                        ReserveNewSkill();
                        break;
                    // 둘 다 불가능한 경우
                    case (false, false):
                        // 제공할 수 있는 스킬이 있긴 있음
                        if (reservedSkills.Count > 0)
                        {
                            // 바로 스킬할당 윈도우 열어줌
                        }
                        // 제공할 수 있는 스킬이 아예 없음
                        else
                        {
                            // 골드나 회복 아이템을 선택할 수 있다
                        }

                        break;
                }
            }

            var skillSelectDatas = InitializeSkillSelectData();
            DungeonUIPresenter.Inst.OpenSkillSelectPanel(skillSelectDatas);
        }

        private bool HasUpgradeableSkill()
        {
            for (int i = 0; i < activeSkills.Count; i++)
            {
                var skill = activeSkills[i];
                if (!skillData.TryGetNextLevelSkill(skill.GroupIndex, skill.SkillLevel, out var nextLevelSkill))
                {
                    continue;
                }

                if (!reservedSkills.Contains(nextLevelSkill))
                {
                    return true;
                }
            }

            for (int i = 0; i < passiveSkills.Count; i++)
            {
                var skill = passiveSkills[i];
                if (!skillData.TryGetNextLevelSkill(skill.GroupIndex, skill.SkillLevel, out var nextLevelSkill))
                {
                    continue;
                }

                if (!reservedSkills.Contains(nextLevelSkill))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CanAssignedNewSkill()
        {
            var canAssignedActiveSkill = activeSkills.Count < DEFAULT_MAX_ACTIVE_SKILL_COUNT;
            var canAssignedPassiveSkill = passiveSkills.Count < DEFAULT_MAX_PASSIVE_SKILL_COUNT;
            if (!canAssignedActiveSkill && !canAssignedPassiveSkill)
            {
                return false;
            }

            var isExistOtherActiveSkills = skillData.IsExistOtherActiveSkill(activeSkills, reservedSkills);
            var isExistOtherPassiveSkills = skillData.IsExistOtherPassiveSkill(passiveSkills, reservedSkills);
            if (!isExistOtherActiveSkills && !isExistOtherPassiveSkills)
            {
                return false;
            }

            return true;
        }

        private void SelectUpgOrNewSkill()
        {
            if (IsProvidableUpgrade())
            {
                ReserveUpgSkill();
            }
            else
            {
                ReserveNewSkill();
            }
        }

        private void ReserveNewSkill()
        {
            List<PlayerSkill> learnableSkills = null;

            var isFullActiveSkills = activeSkills.Count >= DEFAULT_MAX_ACTIVE_SKILL_COUNT;
            var isFullPassiveSkills = passiveSkills.Count >= DEFAULT_MAX_PASSIVE_SKILL_COUNT;

            // 남아 있는 스킬 슬롯 여부에 따라 예약할 스킬 타입 선택
            switch (isFullActiveSkills, isFullPassiveSkills)
            {
                case (false, false):
                    if (SelectSkillType())
                    {
                        // Active Skill
                        learnableSkills = skillData.GetLearnableActiveSkills();
                        learnableSkills = RemoveDuplicateSkills(learnableSkills, activeSkills);
                    }
                    else
                    {
                        // Passive Skill 
                        learnableSkills = skillData.GetLearnablePassiveSkills();
                        learnableSkills = RemoveDuplicateSkills(learnableSkills, passiveSkills);
                    }

                    break;
                case (false, true):
                    learnableSkills = skillData.GetLearnableActiveSkills();
                    learnableSkills = RemoveDuplicateSkills(learnableSkills, activeSkills);
                    break;
                case (true, false):
                    learnableSkills = skillData.GetLearnablePassiveSkills();
                    learnableSkills = RemoveDuplicateSkills(learnableSkills, passiveSkills);
                    break;
                case (true, true):
                    CatLog.WLog("Not Enough Skill Slot");
                    break;
            }

            learnableSkills = RemoveDuplicateSkills(learnableSkills, reservedSkills);
            var newSKill = learnableSkills.OrderBy(_ => Random.value).FirstOrDefault();

            if (newSKill == null)
            {
                CatLog.WLog("Picked New Skill is null");
                return;
            }

            reservedSkills.Add(newSKill);
        }
        
        private void ReserveUpgSkill()
        {
            var skills = RemoveDuplicateSkills(upgradeableSkills, reservedSkills);
            var upgSkill = skills.OrderBy(_ => Random.value).FirstOrDefault();
            if (upgSkill == null)
            {
                CatLog.WLog("Picked Upgrade Skill is null");
                return;
            }

            var nextLevelSkillIndex = skillData.GetNextLevelSkillIndex(upgSkill.GroupIndex, upgSkill.SkillLevel);
            var nextLevelSkill = skillData.Dict[nextLevelSkillIndex];
            reservedSkills.Add(nextLevelSkill);
        }

        private List<PlayerSkill> RemoveDuplicateSkills(List<PlayerSkill> leanableSkills, List<PlayerSkill> ExceptSkills)
        {
            for (int i = 0; i < ExceptSkills.Count; i++)
            {
                var exceptSkill = ExceptSkills[i];
                leanableSkills = leanableSkills.Where(skill => skill.GroupIndex != exceptSkill.GroupIndex).ToList();
            }

            return leanableSkills;
        }

        private bool IsProvidableUpgrade()
        {
            var randomValue = Extensions.Roll1To100I();
            return randomValue <= 40;
        }

        private bool SelectSkillType()
        {
            var randomValue = Extensions.Roll1To100I();
            return randomValue <= 50;
        }

        private PlayerSkillSelectData[] InitializeSkillSelectData()
        {
            if (!reservedSkills.Any())
            {
                CatLog.WLog("Reserved Skills is Empty");
                return null;
            }

            var skillSelectData = new PlayerSkillSelectData[reservedSkills.Count];

            for (int i = 0; i < reservedSkills.Count; i++)
            {
                var skill = reservedSkills[i];
                skillSelectData[i] = new PlayerSkillSelectData(skill.SkillName, skill.Description, skill.Index, skill.Type, skill.IconKey);
            }

            return skillSelectData;
        }
        
        // 스킬 선택 시 현재 가진 스킬목록을 업데이트
        public void UpdateSkillData(int skillIndex)
        {
            var skill = skillData.Dict[skillIndex];
            switch (skill.Type)
            {
                case PlayerSkillType.Active:
                    UpdateActiveSkill(skill);
                    break;
                case PlayerSkillType.Passive:
                    UpdatePassiveSkill(skill);
                    break;
                default:
                    CatLog.WLog("Invalid Skill Type");
                    break;
            }
        }

        // 액티브 스킬일 경우 - 목록 업데이트 및 스킬 효과 업데이트
        private void UpdateActiveSkill(PlayerSkill skill)
        {
            var isOwnedSkill = false;

            for (int i = 0; i < activeSkills.Count; i++)
            {
                var activeSkill = activeSkills[i];
                if (activeSkill.GroupIndex == skill.GroupIndex)
                {
                    isOwnedSkill = true;
                    activeSkills.Remove(activeSkill);
                    upgradeableSkills.Remove(activeSkill);
                    break;
                }
            }

            activeSkills.Add(skill);
            UpdateActiveSkillEffect(skill, isOwnedSkill);

            if (!skillData.IsMaxLevel(skill.GroupIndex, skill.SkillLevel))
            {
                upgradeableSkills.Add(skill);
            }
        }
        
        // 액티브 스킬이 업데이트 되며 스킬 효과를 업데이트
        private void UpdateActiveSkillEffect(PlayerSkill skill, bool isOwned)
        {
            // 새로운 스킬이라면 새로운 Effect 생성 및 리스트에 추가
            if (!isOwned)
            {
                var skillEffect = SkillEffectManager.Inst.InstantiateActiveSkillEffect(tr, enhancedStat, skill);
                activeSkillEffects.Add(skill.GroupIndex, skillEffect);
                
                // 현재 배틀룸 진입 상태라면 직접 활성화
                if (isPlayerInBattle)
                {
                    skillEffect.OnDispose();
                    skillEffect.SkillEffect();
                }
            }
            
            // 기존 스킬이라면 SkillData를 수정
            else
            {
                var skillEffect = activeSkillEffects[skill.GroupIndex];
                skillEffect.SkillData = skill;
            }
        }

        // 패시브 스킬일 경우 - 목록 업데이트
        private void UpdatePassiveSkill(PlayerSkill skill)
        {
            for (int i = 0; i < passiveSkills.Count; i++)
            {
                var passiveSkill = passiveSkills[i];
                if (passiveSkill.GroupIndex == skill.GroupIndex)
                {
                    passiveSkills.Remove(passiveSkill);
                    upgradeableSkills.Remove(passiveSkill);
                    break;
                }
            }

            passiveSkills.Add(skill);
            SkillEffectManager.Inst.PassiveSkillEffect(skill);

            if (!skillData.IsMaxLevel(skill.GroupIndex, skill.SkillLevel))
            {
                upgradeableSkills.Add(skill);
            }
        }

        public void EnableSkillSelectPanelIfPossible()
        {
            if (currentSkillPoint <= 0)
            {
                return;
            }

            ActiveSkillSelectPanel();
            currentSkillPoint--;
            if (currentSkillPoint > 0)
            {
                DungeonUIPresenter.Inst.EnableSkillPointButton(currentSkillPoint);
            }
            else
            {
                DungeonUIPresenter.Inst.DisableSkillPointButton();
            }
        }

        public void EnableSkillSelectPanelForced()
        {
            ActiveSkillSelectPanel();
        }
    }
}