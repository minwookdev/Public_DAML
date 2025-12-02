using System;
using System.Diagnostics.CodeAnalysis;
using CoffeeCat.Utils;
using DG.Tweening;
using Spine;
using Spine.Unity;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerState : MonoBehaviour
    {
        public enum EnumPlayerState
        {
            None,
            Idle,
            Walk,
            Attack,
            Hit,
            Dead
        }

        [SerializeField] public EnumPlayerState State = EnumPlayerState.None;

        protected Transform tr = null;
        protected SkeletonAnimation anim = null;
        protected TrackEntry currentTrack = null;
        protected Player_Dungeon player = null;

        protected const string animIdle = "Idle_2";
        protected const string animWalk = "Walk_NoHand";
        protected const string animAttack = "Attack_2";
        protected const string animHit = "Hit";
        protected const string animDead = "Die_2";

        protected virtual void Start()
        {
            tr = GetComponent<Transform>();
            anim = GetComponent<SkeletonAnimation>();

            ChangeState(EnumPlayerState.Idle);
        }

        protected virtual void Update()
        {
            UpdateState();
        }

        private void UpdateState()
        {
            switch (State)
            {
                case EnumPlayerState.None:
                    break;
                case EnumPlayerState.Idle:
                    Update_IdleState();
                    break;
                case EnumPlayerState.Walk:
                    Update_WalkState();
                    break;
                case EnumPlayerState.Attack:
                    Update_AttackState();
                    break;
                case EnumPlayerState.Hit:
                    Update_HitState();
                    break;
                case EnumPlayerState.Dead:
                    Update_DeadState();
                    break;
                default:
                    break;
            }
        }

        protected void ChangeState(EnumPlayerState targetState, float delayTime = 0f)
        {
            if (delayTime <= 0)
            {
                Execute();
                return;
            }

            Observable.Timer(TimeSpan.FromSeconds(delayTime))
                      .Skip(TimeSpan.Zero)
                      .Subscribe(_ => Execute())
                      .AddTo(this);

            void Execute()
            {
                switch (State)
                {
                    case EnumPlayerState.None:
                        break;
                    case EnumPlayerState.Idle:
                        Exit_IdleState();
                        break;
                    case EnumPlayerState.Walk:
                        Exit_WalkState();
                        break;
                    case EnumPlayerState.Attack:
                        Exit_AttackState();
                        break;
                    case EnumPlayerState.Hit:
                        Exit_HitState();
                        break;
                    case EnumPlayerState.Dead:
                        Exit_DeadState();
                        break;
                }

                State = targetState;

                switch (State)
                {
                    case EnumPlayerState.None:
                        break;
                    case EnumPlayerState.Idle:
                        Enter_IdleState();
                        break;
                    case EnumPlayerState.Walk:
                        Enter_WalkState();
                        break;
                    case EnumPlayerState.Attack:
                        Enter_AttackState();
                        break;
                    case EnumPlayerState.Hit:
                        Enter_HitState();
                        break;
                    case EnumPlayerState.Dead:
                        Enter_DeadState();
                        break;
                }
            }
        }

        protected void OnPlayerInvincivle()
        {
            Tweener tween = null;
            var originColor = Color.white;
            var hitColor = new Color(1f, 0.7f, 0.7f, 0.5f);
            var invincibleTime = player.EnhancedStat.InvincibleTime;

            tween = DOTween.To(() => originColor, color => anim.Skeleton.SetColor(color), hitColor, 0.1f)
                           .SetLoops(-1, LoopType.Yoyo);

            Observable.Timer(TimeSpan.FromSeconds(invincibleTime))
                      .Subscribe(_ =>
                      {
                          tween.Kill();
                          anim.Skeleton.SetColor(originColor);
                      })
                      .AddTo(this);
        }
        
        protected void SetTrackTimeScale(float timeScale) => currentTrack.TimeScale = timeScale;

        #region IDLE

        protected virtual void Enter_IdleState()
        {
        }

        protected virtual void Update_IdleState()
        {
        }

        protected virtual void Exit_IdleState()
        {
        }

        #endregion

        #region WALK

        protected virtual void Enter_WalkState()
        {
        }

        protected virtual void Update_WalkState()
        {
        }

        protected virtual void Exit_WalkState()
        {
        }

        #endregion

        #region ATTACK

        protected virtual void Enter_AttackState()
        {
        }

        protected virtual void Update_AttackState()
        {
        }

        protected virtual void Exit_AttackState()
        {
        }

        #endregion

        #region HIT

        protected virtual void Enter_HitState()
        {
        }

        protected virtual void Update_HitState()
        {
        }

        protected virtual void Exit_HitState()
        {
        }

        #endregion

        #region DEAD

        protected virtual void Enter_DeadState()
        {
        }

        protected virtual void Update_DeadState()
        {
        }

        protected virtual void Exit_DeadState()
        {
        }

        #endregion
    }
}