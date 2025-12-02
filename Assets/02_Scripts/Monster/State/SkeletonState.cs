using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using System.Linq;

namespace CoffeeCat { 
    public class SkeletonState : MonsterState {
        [TitleGroup("Movement", order: 2), SerializeField] 
        private float trackingStartSqrDistance = 1f;
        
        [TitleGroup("Attack", order: 3), SerializeField] 
        private float dashTime = 1f;
        [TitleGroup("Attack", order: 3), SerializeField] 
        private float speedMultiplierForDash = 1.1f;
        [TitleGroup("Attack", order: 3), SerializeField] 
        private float attackStateAnimationSpeedRatio = 1.3f;

        private float currentDashTime = 0f;
        private Vector2 normalizedMoveDirection = Vector2.zero;
        private readonly int animStateHash = Animator.StringToHash("AnimState");

        protected override void Initialize() {
            base.Initialize();
        }

        protected override void OnActivated() {
            StateChange(EnumMonsterState.Idle);
        }

        #region STATE IDLE

        protected override void OnEnterIdleState() {
            anim.SetInteger(animStateHash, 0);
        }

        protected override void OnUpdateIdleState() {
            // Check Player Exist and Alive 
            if (!RogueLiteManager.Inst.IsPlayerExistAndAlive())
                return;
            StateChange(EnumMonsterState.Tracking);
        }

        protected override void OnExitIdleState() {
            
        }

        #endregion

        #region STATE TRACKING

        protected override void OnEnterTrackingState() {
            anim.SetInteger(animStateHash, 1);
        }

        protected override void OnUpdateTrackingState() {
            // Check Player Death
            if (RogueLiteManager.Inst.IsPlayerNotExistOrDeath()) {
                StateChange(EnumMonsterState.Idle);
                return;
            }
            
            // Get Direction to Player Position
            var isArrival = TrackPlayerToCertainDistance(trackingStartSqrDistance, out normalizedMoveDirection);
            
            // Set Flip SpriteRenderer
            bool isDirectionRight = Math2DHelper.GetDirectionIsRight(normalizedMoveDirection);
            SetFlipX(isDirectionRight);

            // Check Can be Attack Player
            if (!isArrival) 
                return;
            StateChange(EnumMonsterState.Attack);
        }

        protected override void OnFixedUpdateTrackingState() {
            if(isKnockBacking)
                return;
            
            // Tracking Player
            SetVelocity(normalizedMoveDirection, currentMoveSpeed);
        }

        protected override void OnExitTrackingState() {
            SetVelocityZero();
        }

        #endregion

        #region STATE DEATH

        protected override void OnEnterDeathState() {
            anim.SetInteger(animStateHash, 2);
            DespawnOnDeathAnimationCompleted();
        }

        protected override void OnUpdateDeathState() {

        }

        protected override void OnExitDeathState() {
            
        }

        #endregion
        
        #region STATE ATTACK

        protected override void OnEnterAttackState() {
            anim.SetInteger(animStateHash, 1);
            IncreaseAnimationSpeed(attackStateAnimationSpeedRatio);
        }

        protected override void OnUpdateAttackState() {
            // Increase Attack Time
            currentDashTime += Time.deltaTime;
            if (currentDashTime <= dashTime)
                return;

            // Change State to Tracking
            StateChange(EnumMonsterState.Tracking);
        }

        protected override void OnFixedUpdateAttackState() {
            SetVelocity(normalizedMoveDirection, currentMoveSpeed * speedMultiplierForDash);
        }

        protected override void OnExitAttackState() {
            currentDashTime = 0f;
            normalizedMoveDirection = Vector2.zero;
            SetVelocityZero();
            RestoreAnimationSpeed();
        }

        #endregion
        
        #region STATE TAKE DAMAGE

        protected override void OnEnterTakeDamageState() {
            base.OnEnterTakeDamageState();
        }

        protected override void OnUpdateTakeDamageState() {
            base.OnUpdateTakeDamageState();
        }

        protected override void OnFixedUpdateTakeDamageState() {
            base.OnFixedUpdateTakeDamageState();
        }

        protected override void OnExitTakeDamageState() {
            base.OnExitTakeDamageState();
        }

        #endregion
        
        #region STATE STUNNED

        protected override void OnEnterStunnedState() {
            // play idle animation
            anim.SetInteger(animStateHash, 0);
        }

        protected override void OnUpdateStunnedState() {
            
        }

        protected override void OnExitStunnedState() {
            
        }

        #endregion
    }
}
