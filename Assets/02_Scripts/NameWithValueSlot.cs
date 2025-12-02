using UnityEngine;
using TMPro;

namespace CoffeeCat {
    public class NameWithValueSlot : MonoBehaviour {
        [SerializeField] private RectTransform rectTr = null;
        [SerializeField] private TextMeshProUGUI tmpName = null;
        [SerializeField] private TextMeshProUGUI tmpValue = null;
        
        public void Set(string text1, string text2) {
            tmpName.SetText(text1);
            tmpValue.SetText(text2);
            gameObject.SetActive(true);
        }

        public void Disable() {
            gameObject.SetActive(false);
        }
        
        public void SetSiblingIndex(int index) {
            rectTr.SetSiblingIndex(index);
        }
    }
}