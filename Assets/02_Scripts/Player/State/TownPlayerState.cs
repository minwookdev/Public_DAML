using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoffeeCat
{
    public class TownPlayerState : PlayerState
    {
        private Player_Town townPlayer = null;
        
        protected override void Start()
        {
            base.Start();
            townPlayer = GetComponent<Player_Town>();
        }

        #region IDLE

        protected override void Enter_IdleState()
        {
            currentTrack = anim.AnimationState.SetAnimation(0, animIdle, true);
        }

        protected override void Update_IdleState()
        {
            if (townPlayer.IsWalking())
                ChangeState(EnumPlayerState.Walk);
        }

        protected override void Exit_IdleState()
        {
        }

        #endregion

        #region WALK

        protected override void Enter_WalkState()
        {
            currentTrack = anim.AnimationState.SetAnimation(0, animWalk, true);
        }

        protected override void Update_WalkState()
        {
            if (!townPlayer.IsWalking())
                ChangeState(EnumPlayerState.Idle);
        }

        protected override void Exit_WalkState()
        {
        }

        #endregion

        
    }
}