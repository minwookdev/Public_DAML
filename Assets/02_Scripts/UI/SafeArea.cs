using UnityEngine;

namespace CoffeeCat {
    [RequireComponent(typeof(RectTransform)), ExecuteInEditMode]
    public class SafeArea : MonoBehaviour {
        [SerializeField, HideInInspector] private RectTransform rectTr = null;
        private Rect safeAreaRect = default;
        private Vector2 minAnchor = default;
        private Vector2 maxAnchor = default;
        private Vector2 lastUpdatedResolution = Vector2.zero;
        
        private void Start() {
            lastUpdatedResolution = new Vector2(Screen.width, Screen.height);
            ApplySafeArea();
        }

        private void Update() {
            // Only Update Diffrent Resolution
            var currentResolution = new Vector2(Screen.width, Screen.height);
            if (currentResolution == lastUpdatedResolution) {
                return;
            }
            lastUpdatedResolution = currentResolution;
            ApplySafeArea();
        }
        
        private void ApplySafeArea() {
            if (!rectTr) {
                rectTr = GetComponent<RectTransform>();
            }
            
            safeAreaRect = Screen.safeArea;
            minAnchor = safeAreaRect.position;
            maxAnchor = minAnchor + safeAreaRect.size;

            minAnchor.x /= lastUpdatedResolution.x;
            minAnchor.y /= lastUpdatedResolution.y;
            maxAnchor.x /= lastUpdatedResolution.x;
            maxAnchor.y /= lastUpdatedResolution.y;

            rectTr.anchorMin = minAnchor;
            rectTr.anchorMax = maxAnchor;
            
            // CatLog.Log($"Resolution Update: {lastUpdatedResolution.ToString()}");
        }
    }
}