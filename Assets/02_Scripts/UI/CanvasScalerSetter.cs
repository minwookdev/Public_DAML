using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

namespace CoffeeCat {
    [RequireComponent(typeof(CanvasScaler)), ExecuteInEditMode]
    public class CanvasScalerSetter : MonoBehaviour {
        [Title("Canvas Scaler Setter", TitleAlignment = TitleAlignments.Centered)]
        [SerializeField, HideInInspector] private CanvasScaler canvasScaler;
        [SerializeField, ReadOnly] private float screenAspectRatio = 0f;
        
        // aspect reference
        private Vector2 lastUpdatedResolution = Vector2.zero;
        private static readonly Vector2 designedVerticalAspect = new(720f, 1560f);
        private static readonly Vector2 designedHorizontalAspect = new(1560f, 720f);
        private static float designedVerticalAspectRatio => designedVerticalAspect.y / designedVerticalAspect.x;       // 2.1666f
        private static float designedHorizontalAspectRatio => designedHorizontalAspect.y / designedHorizontalAspect.x; // 0.4615f 
        
        private void Start() {
            lastUpdatedResolution = new Vector2(Screen.width, Screen.height);
            UpdateCanvasScalerMatchWidthOrHeight();
        }

        private void Update() {
            // Only Update Diffrent Resolution
            var currentResolution = new Vector2(Screen.width, Screen.height);
            if (currentResolution == lastUpdatedResolution) {
                return;
            }
            lastUpdatedResolution = currentResolution;
            UpdateCanvasScalerMatchWidthOrHeight();
        }

        private void UpdateCanvasScalerMatchWidthOrHeight() {
            if (!canvasScaler) {
                canvasScaler = GetComponent<CanvasScaler>();
            }
            
            // calculate current screen aspect ratio
            screenAspectRatio = lastUpdatedResolution.y / lastUpdatedResolution.x;

            // vertical screen ratio
            if (screenAspectRatio > 1.0f) {
                canvasScaler.referenceResolution = designedVerticalAspect;
                canvasScaler.matchWidthOrHeight = screenAspectRatio < designedVerticalAspectRatio ? 1f : 0f;
            }
            // horizontal screen ratio
            else {
                canvasScaler.referenceResolution = designedHorizontalAspect;
                canvasScaler.matchWidthOrHeight = screenAspectRatio < designedHorizontalAspectRatio ? 1f : 0f;
            }
             
            // CatLog.Log($"Resolution Update: {lastUpdatedResolution.ToString()}");
        }
    }
}