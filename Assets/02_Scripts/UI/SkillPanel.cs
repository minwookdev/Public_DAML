using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoffeeCat.UI
{
    public class SkillPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI skillName = null;
        [SerializeField] private TextMeshProUGUI skillLevel = null;

        public void InitialSkillPanel(PlayerSkill skillData)
        {
            skillName.SetText(skillData.SkillName);
            skillLevel.SetText($"Lv.{skillData.SkillLevel.ToString()}");
        }
    }
}