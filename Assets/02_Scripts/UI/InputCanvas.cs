using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;

namespace CoffeeCat {
    public class InputCanvas : MonoBehaviour {
        [SerializeField] private Canvas canvas = null;
        [SerializeField] private RectTransform canvasRectTr = null;
        [field: SerializeField] public MobileJoyStick JoyStick { get; private set; } = null;
        [field: SerializeField] public GameObject fireBtnGameObject { get; private set; } = null;
        [SerializeField] private DirectionSelectButton[] wayDirectionButtons = null;
        
        [SerializeField, ReadOnly] private Camera mainCam = null;
        [SerializeField, ReadOnly] private Camera uiCam = null;
        
        public void SetCamera(Camera uiCamera, Camera mainCamera) {
            // Set World Camera
            uiCam = uiCamera;
            mainCam = mainCamera;
            canvas.worldCamera = uiCamera;
            
            // Force Refresh Camera
            uiCamera.gameObject.SetActive(false);
            uiCamera.gameObject.SetActive(true);
        }

        public void ReleaseUICamera() {
            canvas.worldCamera = null;
        }

        private void Start() {
            fireBtnGameObject.gameObject.SetActive(false);
        }
        
        public void InitWayDirectionButton(int index, UnityAction<int> unityAction, Vector2 worldPosition) {
            // exception
            if (index >= wayDirectionButtons.Length || index < 0) {
                CatLog.ELog($"Invalid WauDirectionButtons Index: {index.ToString()}");
                return;
            }

            // get button anchored position from world position
            var isConvertSuccessed = UIHelper.ConvertWorldToCameraSpaceCanvasPosition(mainCam, uiCam, worldPosition, canvasRectTr, out Vector2 anchoredPos);
            if (!isConvertSuccessed) {
                CatLog.ELog("Failed to Convert World to Camera Space Canvas Position");
                return;
            }
                
            // set button event
            var targetButton = wayDirectionButtons[index];
            targetButton.SetButton(anchoredPos, () => {
                DisableAllWayDirectionButtons();
                unityAction(index);
            });
        }
        
        private void DisableAllWayDirectionButtons() {
            for (int i = 0; i < wayDirectionButtons.Length; i++) {
                wayDirectionButtons[i].Disable();
            }
        }
    }
}