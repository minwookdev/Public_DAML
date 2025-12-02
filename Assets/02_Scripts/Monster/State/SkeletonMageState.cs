using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;
using CoffeeCat.Utils;
using UniRx;

namespace CoffeeCat
{
    public class SkeletonMageState : MonsterState
    {
        // Attack
        [TitleGroup("Attack", order: 3), SerializeField]
        private float attackAbleSqrDistance = 5f;

        [TitleGroup("Attack", order: 3), SerializeField]
        private float attackAbleDistanceMultiplier = 1.5f;

        [TitleGroup("Attack", order: 3), SerializeField]
        private float attackStateTime = 2f;

        [TitleGroup("Attack", order: 3), SerializeField, PropertyRange(0f, "attackStateTime")]
        private float projectileSpawnSecondsInAttackStateTime = 1.5f;

        [TitleGroup("Attack", order: 3), SerializeField]
        private float AttackintervalAfterProjectileSpawn = 0.65f;

        [TitleGroup("Attack", order: 3), SerializeField]
        private AddressablesKey projectileKey;

        [TitleGroup("Attack", order: 3), SerializeField]
        private Transform spawnPointSocketTr = null;

        // Fields
        private bool isAttacked = false;
        private bool isArrivalToPlayer = false;
        private float currentAttackStateTime = 0f;
        private float currentIntervalTime = 0f;
        private Vector2 normalizedMoveDirection = Vector2.zero;
        private readonly int animStateHash = Animator.StringToHash("AnimState");

        protected override void Initialize()
        {
            base.Initialize();

            SafeLoader.Regist(projectileKey.ToKey());
        }

        protected override void OnActivated() => StateChange(EnumMonsterState.Idle);

        #region IDLE

        protected override void OnEnterIdleState()
        {
            anim.SetInteger(animStateHash, 0);
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void OnUpdateIdleState()
        {
            if (RogueLiteManager.Inst.IsPlayerNotExistOrDeath())
                return;
            StateChange(EnumMonsterState.Tracking);
        }

        protected override void OnExitIdleState()
        {
        }

        #endregion

        #region TRACKING

        protected override void OnEnterTrackingState()
        {
            anim.SetInteger(animStateHash, 1);
        }

        protected override void OnUpdateTrackingState()
        {
            // Check Player Not Exist Or Death
            if (!RogueLiteManager.Inst.IsPlayerExistAndAlive())
            {
                StateChange(EnumMonsterState.Idle);
                return;
            }

            if (currentIntervalTime > 0f)
            {
                currentIntervalTime -= Time.deltaTime;
                return;
            }

            // Get Direction to Player Position
            isArrivalToPlayer = TrackPlayerToCertainDistance(attackAbleSqrDistance, out normalizedMoveDirection);

            // Set Flip SpriteRenderer
            bool isDirectionRight = Math2DHelper.GetDirectionIsRight(normalizedMoveDirection);
            SetFlipX(isDirectionRight);

            // Check Can be Attack Player
            if (!isArrivalToPlayer)
                return;
            StateChange(EnumMonsterState.Attack);
        }

        protected override void OnFixedUpdateTrackingState()
        {
            if (isArrivalToPlayer || isKnockBacking)
                return;
            SetVelocity(normalizedMoveDirection, currentMoveSpeed);
        }

        protected override void OnExitTrackingState()
        {
            SetVelocityZero();
        }

        #endregion

        #region DEATH

        protected override void OnEnterDeathState() {
            anim.SetInteger(animStateHash, 2);
            DespawnOnDeathAnimationCompleted();
        }

        protected override void OnUpdateDeathState()
        {
            
        }

        protected override void OnExitDeathState()
        {
            
        }

        #endregion

        #region ATTACK

        protected override void OnEnterAttackState()
        {
            anim.SetInteger(animStateHash, 3);
        }

        protected override void OnUpdateAttackState()
        {
            // Increase Attack Time
            currentAttackStateTime += Time.deltaTime;
            if (currentAttackStateTime < projectileSpawnSecondsInAttackStateTime)
            {
                return;
            }

            // Spawn Projectile
            if (!isAttacked)
            {
                SpawnProjectile();
                isAttacked = true;
            }

            // Arrival Max Attack Time
            if (currentAttackStateTime >= attackStateTime)
            {
                return;
            }

            // State to Tracking
            StateChange(EnumMonsterState.Tracking);
        }

        protected override void OnExitAttackState()
        {
            currentAttackStateTime = 0f;
            isAttacked = false;
        }

        #endregion

        private void SpawnProjectile()
        {
            // Failed to Attack
            if (!IsPlayerInAttackRange(out Vector2 playerPosition))
            {
                return;
            }

            // Success to Attack
            currentIntervalTime = AttackintervalAfterProjectileSpawn;
            // TODO: This Logic To Static Utils
            // Get LookAt Target Rotation
            Vector2 spawnPoint = spawnPointSocketTr.position;
            Vector2 direction = playerPosition - spawnPoint;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion lookAtTargetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // Spawn Projectile
            var spawnedProjectile = ObjectPoolManager.Inst.Spawn<MonsterProjectile>(projectileKey.ToKey(), spawnPoint, lookAtTargetRotation);
            spawnedProjectile.Initialize(stat);
        }

        private bool IsPlayerInAttackRange(out Vector2 playerPosition)
        {
            playerPosition = RogueLiteManager.Inst.SpawnedPlayerPosition;
            var sqrDistance = Math2DHelper.GetDirection(Tr.position, playerPosition).sqrMagnitude;
            return sqrDistance <= (attackAbleSqrDistance * attackAbleDistanceMultiplier);
        }
    }
}