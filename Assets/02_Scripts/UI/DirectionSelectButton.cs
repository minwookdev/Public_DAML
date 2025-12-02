using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace CoffeeCat {
    public class DirectionSelectButton : MonoBehaviour {
        [SerializeField] private Button button = null;
        [SerializeField] private RectTransform rectTr = null;
        
        public void SetButton(Vector2 position, UnityAction unityAction) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(unityAction);

            rectTr.anchoredPosition = position;
            gameObject.SetActive(true);
        }

        public void Disable() {
            gameObject.SetActive(false);
        }
    }
}
