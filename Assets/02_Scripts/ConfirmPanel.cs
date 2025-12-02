using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace CoffeeCat {
    public class ConfirmPanel : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI tmpConfirmMessage = null;
        [SerializeField] private Button btnConfirm = null;
        [SerializeField] private Button btnCancel = null;

        public void Set(string messageStr, UnityAction onConfirmAction = null, UnityAction onCanceledAction = null) {
            tmpConfirmMessage.SetText(messageStr);
            
            btnConfirm.onClick.RemoveAllListeners();
            btnCancel.onClick.RemoveAllListeners();
            
            if (onConfirmAction != null) {
                btnConfirm.onClick.AddListener(onConfirmAction);
            }
            
            bool isCanceledActionValid = onCanceledAction != null;
            if (isCanceledActionValid) {
                btnCancel.onClick.AddListener(onCanceledAction);
            }
            
            btnConfirm.onClick.AddListener(OnClose);
            btnCancel.onClick.AddListener(OnClose);
            
            btnConfirm.gameObject.SetActive(true);
            btnCancel.gameObject.SetActive(isCanceledActionValid);
            
            gameObject.SetActive(true);
        }

        private void OnClose() {
            gameObject.SetActive(false);
        }
    }
}