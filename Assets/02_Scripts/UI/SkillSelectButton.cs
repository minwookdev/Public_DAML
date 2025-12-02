using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using Sirenix.OdinInspector;

namespace CoffeeCat.UI {
    public class SkillSelectButton : MonoBehaviour {
        [SerializeField] private Button button = null;
        [SerializeField] private TextMeshProUGUI tmpName = null;
        [SerializeField] private TextMeshProUGUI tmpDesc = null;
        [SerializeField] private TextMeshProUGUI tmpType = null;
        [SerializeField] private Image imgIcon = null;
        [SerializeField, ReadOnly] private PlayerSkillSelectData data = null;

        private void Start() {
            // button.onClick.RemoveAllListeners();
            AddButtonEvent(() => 
            {
                if (!RogueLiteManager.IsExist) {
                    return;
                }
                var player = RogueLiteManager.Inst.SpawnedPlayer;
                if (!player) {
                    return;
                }
                RogueLiteManager.Inst.UpdatePlayerSkill(data.Index);
                DungeonUIPresenter.Inst.CloseSkillSelectPanel();
                DungeonEvtManager.InvokeEventSkillSelectCompleted();
            });
        }

        /*public void AddDisableButtonEvent(GameObject parentPanel) {
            AddButtonEvent(() => {
                parentPanel.SetActive(false);
            });
        }*/

        public void Set(PlayerSkillSelectData recievedData) {
            data = recievedData;
            tmpName.SetText(recievedData.Name);
            tmpDesc.SetText(recievedData.Desc);
            tmpType.SetText(TypeText(recievedData.Type));
            var iconkey = recievedData.IconKey;
            if (recievedData.IconKey != string.Empty) {
                ResourceManager.Inst.AddressablesAsyncLoad<Sprite>(iconkey, false, (sprite) => {
                    if (!sprite || data == null || data.IconKey != iconkey) {
                        return;
                    }
                    imgIcon.sprite = sprite;   
                });
            }
            
            return;
            string TypeText(PlayerSkillType type)
            {
                return type switch
                {
                    PlayerSkillType.Active  => "<< Active >>",
                    PlayerSkillType.Passive => "<< Passive >>",
                    _                       => "type is not defined"
                };
            }
        }

        public void Clear() {
            tmpName.SetText("");
            tmpDesc.SetText("");
            tmpType.SetText("");
            data = null;
        }

        private void AddButtonEvent(UnityAction unityAction) {
            button.onClick.AddListener(unityAction);
        }
    }
}
