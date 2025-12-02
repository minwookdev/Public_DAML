using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace CoffeeCat {
    public class SettingsPanel : MonoBehaviour {
        [Title("Common")]
        [SerializeField] private Button[] closeBtns = null;
        [SerializeField] private Button btnExitDungeon = null;
        
        [Title("Input Type")]
        [SerializeField] private Toggle toggleInputTypeA = null;
        [SerializeField] private Toggle toggleInputTypeB = null;

        private void OnEnable() {
            var currentJoyStickType = InputManager.Inst.GetCurrentJoyStickType();
            toggleInputTypeA.isOn = currentJoyStickType == JoyStickType.Static;
            toggleInputTypeB.isOn = currentJoyStickType == JoyStickType.Dynamic;
        }

        private void Start() {
            foreach (var btn in closeBtns) {
                btn.onClick.AddListener(ClosePanel);
            }

            toggleInputTypeA.onValueChanged.AddListener((isOn) => {
                toggleInputTypeB.isOn = !isOn;
                if (isOn) {
                    InputManager.Inst.SetJoyStickType(JoyStickType.Static);
                }
            });

            toggleInputTypeB.onValueChanged.AddListener((isOn) => {
                toggleInputTypeA.isOn = !isOn;
                if (isOn) {
                    InputManager.Inst.SetJoyStickType(JoyStickType.Dynamic);
                }
            });
            
            btnExitDungeon.onClick.AddListener(OnButtonExitDungeon);
        }

        public void EnablePanel() {
            gameObject.SetActive(true);
        }

        public void ClosePanel() {
            gameObject.SetActive(false);
        }

        private void OnButtonExitDungeon() {
            DungeonSceneBase.Inst.RequestToTownScene();
        }
    }
}