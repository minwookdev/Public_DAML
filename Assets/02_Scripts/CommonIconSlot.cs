using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;

namespace CoffeeCat {
    public class CommonIconSlot : MonoBehaviour {
        [SerializeField] protected Image imgIcon = null;
        [SerializeField] protected Button button = null;
        private string currentIconKey = string.Empty;

        public virtual void Set(string iconKey, UnityAction onClickedEvent = null) {
            LoadIconSpriteIfDiff(iconKey);
            button.onClick.RemoveAllListeners();
            if (onClickedEvent != null) {
                button.onClick.AddListener(onClickedEvent);
            }
            
            gameObject.SetActive(true);
        }

        protected void LoadIconSpriteIfDiff(string iconKey) {
            if (currentIconKey == iconKey) {
                return;
            }
            currentIconKey = iconKey;
            ResourceManager.Inst.AddressablesAsyncLoad<Sprite>(iconKey, false, (sprite) => {
                if (!sprite || iconKey != currentIconKey) {
                    return;
                }
                
                OnSpriteLoadCompleted(sprite);
            });
        }

        protected virtual void OnSpriteLoadCompleted(Sprite sprite) {
            imgIcon.sprite = sprite;
        }
                
        public void ClearWithDisable() {
            Clear();
            Disable();
        }

        public void Disable() => gameObject.SetActive(false);

        public virtual void Clear() => currentIconKey = string.Empty;

#if UNITY_EDITOR
        [PropertySpace(10f), Button("Setup", ButtonSizes.Medium)]
        protected virtual void Setup() {
            imgIcon = GetComponent<Image>();
            button = GetComponent<Button>();
        }
#endif
    }   
}
