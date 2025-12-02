using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;

namespace CoffeeCat {
    [RequireComponent(typeof(RectTransform)), ExecuteInEditMode]
    public class ScreenModeRectScaler : MonoBehaviour {
        [Title("Screen Mode Rect Scaler", TitleAlignment = TitleAlignments.Centered)]
        [SerializeField, HideInInspector] private RectTransform rectTr = null;
        [SerializeField] private bool isApplyHeight = true;  
        [SerializeField] private bool isApplyWidth = true;
        [SerializeField, ReadOnly] private Vector2 hModeSizeDelta = Vector2.zero;
        [SerializeField, ReadOnly] private Vector2 vModeSizeDelta = Vector2.zero;
        
        private Vector2 lastUpdatedResolution = Vector2.zero;

        private void Start() {
            lastUpdatedResolution = new Vector2(Screen.width, Screen.height);
            ApplyScreenModeRectSize();
        }

        private void Update() {
            // Only Update Diffrent Resolution
            var currentResolution = new Vector2(Screen.width, Screen.height);
            if (currentResolution == lastUpdatedResolution) {
                return;
            }
            lastUpdatedResolution = currentResolution;
            ApplyScreenModeRectSize();
        }
        
        private void ApplyScreenModeRectSize() {
            if (!rectTr) {
                rectTr = GetComponent<RectTransform>();
            }
            
            var currentScreenAspectRatio = lastUpdatedResolution.y / lastUpdatedResolution.x;
            var sizeDelta = rectTr.sizeDelta;
            if (currentScreenAspectRatio > 1.0f) {
                // Not Set Vertical Screen Size Delta
                if (vModeSizeDelta == Vector2.zero) {
                    return;
                }
                
                // Vertical Screen Ratio
                switch (isApplyWidth) {
                    case false when !isApplyHeight:
                        CatLog.WLog("ScreenModeRectScaler was Not Apply Width and Height.");
                        return;
                    case false when isApplyHeight:
                        sizeDelta.y = vModeSizeDelta.y;
                        break;
                    case true when !isApplyHeight:
                        sizeDelta.x = vModeSizeDelta.x;
                        break;
                    case true when isApplyHeight:
                        sizeDelta = vModeSizeDelta;
                        break;
                }
            }
            else {
                // Not Set Horizontal Screen Size Delta
                if (hModeSizeDelta == Vector2.zero) {
                    return;
                }
                
                // Horizontal Screen Ratio
                switch (isApplyWidth) {
                    case false when !isApplyHeight:
                        CatLog.WLog("ScreenModeRectScaler was Not Apply Width and Height.");
                        return;
                    case false when isApplyHeight:
                        sizeDelta.y = hModeSizeDelta.y;
                        break;
                    case true when !isApplyHeight:
                        sizeDelta.x = hModeSizeDelta.x;
                        break;
                    case true when isApplyHeight:
                        sizeDelta = hModeSizeDelta;
                        break;
                }
            }

            rectTr.sizeDelta = sizeDelta;
        }

#if UNITY_EDITOR
        [Button("Set Current Rect Size With Screen Mode")]
        private void SetSizeInEditor() {
            if (!rectTr) {
                rectTr = GetComponent<RectTransform>();
            }

            var editorGameViewScreenSize = Utility.GetGameViewScreenSizeInEditor();
            var currentScreenAspectRatio = editorGameViewScreenSize.y / editorGameViewScreenSize.x;
            if (currentScreenAspectRatio > 1.0f) {
                // Vertical Screen Ratio
                vModeSizeDelta = rectTr.sizeDelta;
            }
            else {
                // Horizontal Screen Ratio
                hModeSizeDelta = rectTr.sizeDelta;
            }
            
            // CatLog.Log($"ScreenModeRectScaler x: {editorGameViewScreenSize.x.ToString()}, y: {editorGameViewScreenSize.y.ToString()}");
        }

        [Button("Clear")]
        private void ClearInEditor() {
            vModeSizeDelta = Vector2.zero;
            hModeSizeDelta = Vector2.zero;
        }
#endif
    }
}
