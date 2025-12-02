using System.Collections.Generic;
using UnityEngine;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class InteractionDungeonReward : Interaction {
        [SerializeField] private SpriteRenderer spriteRenderer = null;
        [SerializeField] private Sprite closedSprite = null;
        [SerializeField] private Sprite openedSprite = null;
        [SerializeField] private List<ItemLootRequest> requests = new();
        private GuaranteedLootList lootTable = null;
        private bool isOpened = false;

        protected override void Start() {
            base.Start();
            InteractionType = InteractionType.DungeonReward;
            infoText.gameObject.SetActive(false);
        }

        public void Init(GuaranteedLootList table) {
            // set closed sprite
            spriteRenderer.sprite = closedSprite;
            isOpened = false;
            
            // set loot table
            lootTable = table;
        }
        
        public override void PlayParticle() {
            if (isOpened) {
                return;
            }
            base.PlayParticle();
        }

        protected override void OnPlayerEnter() {
            if (isOpened) {
                return;
            }
            infoText.gameObject.SetActive(true);
        }
        
        protected override void OnPlayerExit() {
            infoText.gameObject.SetActive(false);
        }

        public override void Interact() {
            if (isOpened) {
                return;
            }

            isOpened = true;
            spriteRenderer.sprite = openedSprite;
            StopParticle();
            
            requests.Clear();
            var raffledItems = lootTable.GetRewardRoomList();
            for (int i = 0; i < raffledItems.Count; i++) {
                var raffledItem = raffledItems[i];
                var request = new ItemLootRequest() {
                    Amount = raffledItem.GetAmount(),
                    Code = raffledItem.Code
                };
                requests.Add(request);
            }
            
            // loot request items
            DungeonSceneBase.Inst.SpawnItemObject(requests, tr.position,  0.3f);
        }
    }
}