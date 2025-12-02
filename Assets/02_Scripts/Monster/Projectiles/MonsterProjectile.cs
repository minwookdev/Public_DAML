using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;

// NOTE: 시간에 따라 변화하는 여러 ActiveType을 가질 수 있도록 기능 확장
namespace CoffeeCat {
    public class MonsterProjectile : Projectile {
        public enum ActiveType {
            NONE,
            FORCE_FORWARD,
            FORCE_DIRECTION,
            ANIMATED,
            TRACKING,
        }

        protected MonsterStat statData = null;
        protected Effector effector = null;

        [TitleGroup("PROJECTILE DEFAULT SETTINGS", order: 0)] [SerializeField]
        protected LayerMask collisionLayer = default;

        [SerializeField] protected bool isAutoActive = true;
        [SerializeField] protected ActiveType activeType = ActiveType.NONE;

        [SerializeField, Range(-1f, 15f, order = 1), PropertyTooltip("-1f == Infinity")]
        protected float maxLifeTime = -1f;

        [SerializeField, ShowIf("@this.activeType == ActiveType.FORCE_FORWARD || " + "this.activeType == ActiveType.FORCE_DIRECTION")]
        protected float forceValue = 0f;

        private readonly DamageResult damageResult = new();

        protected virtual void OnDisable() {
            damageResult.Clear();
        }

        protected virtual void SetBaseComponents() {
            if (!tr) {
                tr = GetComponent<Transform>();
            }

            if (!rigidBody2D) {
                rigidBody2D = GetComponent<Rigidbody2D>();
            }

            if (!effector) {
                effector = GetComponent<Effector>();
            }
        }

        public virtual void Initialize(MonsterStat monsterStatData) {
            SetBaseComponents();
            statData = monsterStatData;

            // Despawn 예약
            if (maxLifeTime != -1f) {
                ObjectPoolManager.Inst.Despawn(this.gameObject, maxLifeTime);
            }

            if (isAutoActive) {
                ActiveAuto();
            }
        }

        protected virtual void ActiveAuto() {
            switch (activeType) {
                case ActiveType.NONE: break;
                case ActiveType.FORCE_FORWARD:
                    ActiveForceToForward();
                    break;
                case ActiveType.FORCE_DIRECTION:
                    ActiveForceToDirection();
                    break;
                case ActiveType.ANIMATED:
                    ThrowMessageNotSupported();
                    break;
                default:
                    ThrowMessageNotSupported();
                    break;
            }

            effector?.Play();

            void ActiveForceToForward() {
                this.ForceToForward(forceValue);
            }

            void ActiveForceToDirection() {
                ThrowMessageNotSupported();
            }

            void ThrowMessageNotSupported() {
                CatLog.ELog("Not Implemented this ActiveType.");
            }
        }

        public virtual void ActiveManual(Action<Projectile> action) {
            action?.Invoke(this);
            effector?.Play();
        }

        #region COLLISION BASE

        private void OnTriggerEnter2D(Collider2D collider) {
            if (Utils.Utility.IsLayerInMask(collider.gameObject.layer, collisionLayer)) {
                if (collider.gameObject.layer.Equals(LayerMask.NameToLayer("Player"))) {
                    OnTriggerEnterWithPlayer(collider);
                    return;
                }

                OnTriggerEnterWithTargetLayer(collider);
            }
        }

        protected void OnCollisionEnter2D(Collision2D collision) {
            if (Utils.Utility.IsLayerInMask(collision.gameObject.layer, collisionLayer)) {
                if (collision.gameObject.layer.Equals(LayerMask.NameToLayer("Player"))) {
                    OnCollisionEnterWithPlayer(collision);
                    return;
                }

                OnCollisionEnterWithTargetLayer(collision);
            }
        }

        protected virtual void OnTriggerEnterWithTargetLayer(Collider2D collider) {
        }

        protected virtual void OnTriggerEnterWithPlayer(Collider2D playerCollider) {
            /*if (playerCollider.TryGetComponent(out Player player) == false) {
                return;
            }*/

            // TODO: Fix it
            if (playerCollider.transform.parent.TryGetComponent(out Player_Dungeon player) == false) {
                return;
            }

            // Get Attack Angle
            Vector2 collisionPoint = playerCollider.ClosestPoint(tr.position);
            Vector2 collisionDirection = Math2DHelper.GetNormalizedDirection(collisionPoint, player.transform.position);
            DamageToPlayer(player, collisionPoint, collisionDirection);
        }

        protected virtual void OnCollisionEnterWithTargetLayer(Collision2D collision) {
        }

        protected virtual void OnCollisionEnterWithPlayer(Collision2D playerCollision) {
        }

        #endregion

        #region DAMAGING

        protected virtual void DamageToPlayer(Player_Dungeon player, Vector2 collisionPoint, Vector2 collisionDirection) {
            if (statData == null) {
                CatLog.ELog("Monster Stat Data is Null.");
                return;
            }
            
            damageResult.SetData(statData, player.EnhancedStat);
            player.OnDamaged(damageResult);
            ObjectPoolManager.Inst.Despawn(gameObject);
        }

        #endregion
    }
}