using UnityEngine;

namespace CoffeeCat {
    public class DummyMonState : MonsterState {
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
        
        #endregion
        
        #region STATE DETAH
        
        protected override void OnEnterDeathState() {
            anim.SetInteger(animStateHash, 2);
            DespawnOnDeathAnimationCompleted();
        }
        
        #endregion
    }
}
