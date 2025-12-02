using System;
using CoffeeCat.FrameWork;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat.UI
{
    public class PlayerAbilitySlot : MonoBehaviour
    {
        private int index;
        private PlayerAbility abilityData;

        public RectTransform rectTr;
        public Image imgIcon;
        public Button btnAbility;
        public TextMeshProUGUI tmpName;
        public TextMeshProUGUI tmpLevel;
        
        public void SetSlot(int slotIndex, PlayerAbility ability, int currentLevel)
        {
            rectTr.localScale = Vector3.one;
            SetIcon(ability.type);
            index = slotIndex;
            abilityData = ability;
            tmpName.text = ability.AbilityName;
            tmpLevel.text = $"{currentLevel} / {ability.MaxLevel}";
            btnAbility.onClick.AddListener(() => OnAbilityBtnClick(currentLevel, ability.MaxLevel));
        }

        private void SetIcon(PlayerStatEnum type)
        {
            var key = GetAbilityIconKey(type);
            ResourceManager.Inst.AddressablesAsyncLoad<Sprite>(key, false, icon =>
            {
                if (icon == null)
                {
                    CatLog.WLog($"Not Found Icon : {key}");
                    return;
                }
                
                imgIcon.sprite = icon;
            });
        }
        
        private void OnAbilityBtnClick(int current, int max)
        {
            SoundManager.Inst.PlayButtonSE(true);
            tmpLevel.text = $"{current} / {max}";
            TownEvtManager.InvokeAbilityUpgrade(index, abilityData);
        }
        
        private string GetAbilityIconKey(PlayerStatEnum stat)
        {
            return stat switch
            {
                PlayerStatEnum.MaxHp                => "icon_ability_maxHp",
                PlayerStatEnum.Defense              => "icon_ability_defense",
                PlayerStatEnum.MoveSpeed            => "icon_ability_moveSpeed",
                PlayerStatEnum.AttackPower          => null,
                PlayerStatEnum.InvincibleTime       => null,
                PlayerStatEnum.CriticalChance       => null,
                PlayerStatEnum.CriticalResistance   => null,
                PlayerStatEnum.CriticalMultiplier   => null,
                PlayerStatEnum.DamageDeviation      => null,
                PlayerStatEnum.Penetration          => null,
                PlayerStatEnum.ItemAcquisitionRange => "icon_ability_itemAcq",
                PlayerStatEnum.CoolTimeReduce       => null,
                PlayerStatEnum.SearchRange          => null,
                PlayerStatEnum.AddProjectile        => null,
                _                                   => throw new ArgumentOutOfRangeException(nameof(stat), stat, null)
            };
        }
    }
}