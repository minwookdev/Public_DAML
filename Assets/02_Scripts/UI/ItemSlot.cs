using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class ItemSlot : CommonIconSlot {
        [SerializeField] private TextMeshProUGUI tmpDesc = null;
        [SerializeField, ReadOnly] private Item itemRef = null;
        private ItemInfoType infoPanelType = ItemInfoType.ReadOnly;

        private void Start() => SetButtonEvent();

        public void Set(Item item, ItemInfoType itemInfoType) {
            // only update item slot if the current reference is different
            itemRef = item;
            LoadIconSpriteIfDiff(item.IconKey);
            infoPanelType = itemInfoType;
            gameObject.SetActive(true);
        }

        protected override void OnSpriteLoadCompleted(Sprite sprite) {
            imgIcon.sprite = sprite;
            imgIcon.color = Color.white;
            if (tmpDesc) {
                tmpDesc.color = Color.clear;
            }
        }

        public override void Clear() {
            base.Clear();
            itemRef = null;
            imgIcon.sprite = null;
            imgIcon.color = Color.clear;
            if (tmpDesc) {
                tmpDesc.color = Color.white;
            }
        }

        private void SetButtonEvent() {
            button.onClick.RemoveAllListeners();
            if (RogueLiteManager.Inst.IsEnteredDungeon) {
                button.onClick.AddListener(OnClickedEventDungeon);
            }
            else {
                button.onClick.AddListener(OnClickedEventHome);
            }
        }

        private void OnClickedEventDungeon() {
            if (!itemRef) {
                CatLog.Log("Item Reference is null. Canceled Operation.");
                return;
            }
            SoundManager.Inst.PlayButtonSE(true);
            DungeonUIPresenter.Inst.OpenItemInfoPanel(itemRef, infoPanelType);
        }

        private void OnClickedEventHome() => throw new NotImplementedException();

        public override void Set(string iconKey, UnityAction onClickedEvent = null) => throw new NotImplementedException();
        
#if UNITY_EDITOR
        protected override void Setup() {
            base.Setup();
            tmpDesc = GetComponentInChildren<TextMeshProUGUI>();
        }
#endif
    }
}