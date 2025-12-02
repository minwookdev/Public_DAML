using CoffeeCat.FrameWork;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeCat {
    public class MiniSkillSlot : MonoBehaviour {
        [field: SerializeField] public RectTransform Tr { get; private set; } = null;
        [SerializeField] private Image imageIcon = null;

        public void Init(string iconKey) {
            ResourceManager.Inst.AddressablesAsyncLoad<Sprite>(iconKey, false, (sprite) => {
                imageIcon.sprite = sprite;
            });
            gameObject.SetActive(true);
        }

        private void Clear() {
            
        }

        public void ClearWithDisable() {
            Clear();
            gameObject.SetActive(false);
        }
    }   
}
