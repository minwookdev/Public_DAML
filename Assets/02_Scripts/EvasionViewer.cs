using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CoffeeCat {
    public class EvasionViewer : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI tmpCount = null;
        [SerializeField] private Image imgSlider = null;

        public void SetDisable() => gameObject.SetActive(false);

        public void UpdateViewer(int currentEvasionCount, float currentTime, float maxTime) {
            tmpCount.text = currentEvasionCount.ToString();
            imgSlider.fillAmount = currentTime / maxTime;
        }
    }
}