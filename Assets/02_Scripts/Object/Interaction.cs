using System;
using UnityEngine;
using TMPro;
using UniRx;
using UniRx.Triggers;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class Interaction : MonoBehaviour {
        [ShowInInspector, ReadOnly] public bool IsEnteredPlayer { get; private set; } = false;
        [SerializeField] protected ParticleSystem ps = null;
        [SerializeField] protected Transform tr = null;
        [SerializeField] protected TextMeshPro infoText = null;
        public InteractionType InteractionType { get; protected set; } = InteractionType.None;
        [SerializeField] protected bool enableYSorting = false;
        [SerializeField] protected string characterCoveredSortingLayerName = "Top";
        [SerializeField] protected string characterBottomSortingLayerName = "Default";
        [SerializeField] protected SpriteRenderer[] modelSprites = null;
        private Player_Dungeon player = null;

        protected virtual void Start() {
            this.OnTriggerEnter2DAsObservable()
                .TakeUntilDestroy(this)
                .Where(other => other.gameObject.layer == Utility.GetPlayerLayer())
                .Subscribe(playerCollider => {
                    // Ignore Not Triggered Collider
                    if (!playerCollider.isTrigger)
                        return;
                    
                    OnPlayerEnter();
                    IsEnteredPlayer = true;
                    
                    RogueLiteManager.Inst.OnEnterInteract(this);
                })
                .AddTo(this);
            
            this.OnTriggerStay2DAsObservable()
                .TakeUntilDestroy(this)
                .Where(other => other.gameObject.layer == Utility.GetPlayerLayer())
                .Subscribe(playerCollider => {
                    // Ignore Not Triggered Collider
                    if (!playerCollider.isTrigger)
                        return;
                    
                    var currentInteractable = RogueLiteManager.Inst.EnteredInteractable;
                    if (currentInteractable || currentInteractable == this) 
                        return;
                    RogueLiteManager.Inst.OnEnterInteract(this);
                })
                .AddTo(this);

            this.OnTriggerExit2DAsObservable()
                .TakeUntilDestroy(this)
                .Where(other => other.gameObject.layer == Utility.GetPlayerLayer())
                .Subscribe(playerCollider => {
                    // Ignore Not Triggered Collider
                    if (!playerCollider.isTrigger)
                        return;
                    
                    OnPlayerExit();
                    IsEnteredPlayer = false;

                    // check exist manager when destroyed
                    var currentInteractable = RogueLiteManager.Inst?.EnteredInteractable;
                    if (!currentInteractable || currentInteractable != this) {
                        return;
                    }
                    
                    RogueLiteManager.Inst?.OnExitInteract();
                })
                .AddTo(this);

            player = RogueLiteManager.Inst.SpawnedPlayer;
        }

        private void Update() {
            if (enableYSorting) {
                UpdateYSorting();
            }

            if (!IsEnteredPlayer) {
                return;
            }
            OnPlayerStay();
        }

        protected virtual void OnDisable() {
            IsEnteredPlayer = false;
        }

        public virtual void Interact() => throw new NotImplementedException();

        public virtual void PlayParticle() {
            ps.Play();
        }
        
        public virtual void StopParticle() {
            ps.Stop();
        }

        protected virtual void OnPlayerEnter() {
            
        }

        protected virtual void OnPlayerStay() {
            
        }

        protected virtual void OnPlayerExit() {
            
        }

        private void UpdateYSorting() {
            var playerPosition = player.Tr.position;
            var layer = playerPosition.y > tr.position.y ? characterCoveredSortingLayerName : characterBottomSortingLayerName;
            for (int i = 0; i < modelSprites.Length; i++) {
                modelSprites[i].sortingLayerName = layer;
            }
            if (!infoText) {
                return;
            }
            infoText.sortingLayerID = SortingLayer.NameToID(layer);
        }
    }
}