using System;
using System.Collections;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;
using Random = UnityEngine.Random;
#if UNITY_EDITOR    
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace CoffeeCat {
    public class MonsterState : MonoBehaviour {
        public enum EnumMonsterState {
            None,
            Idle,
            Tracking,
            Patrol,
            Attack,
            TakeDamage,
            Stunned,
            Death,
        }
        
        // Order 0
        [TitleGroup("State", order: 0), ShowInInspector, ReadOnly] 
        public EnumMonsterState State { get; private set; } = EnumMonsterState.None;
        [field: SerializeField, TitleGroup("State", order: 0)]
        public bool IsKnockBackable { get; private set; }  = true;
        [field: SerializeField, TitleGroup("State", order: 0)]
        public bool IsDespawnOnDeathAnimationCompleted { get; protected set; } = true;

        // Order 1
        [TitleGroup("Models & Animation", order: 1), SerializeField]
        protected SpriteRenderer sprite = null;
        [TitleGroup("Models & Animation", order: 1), SerializeField]
        protected SpriteRenderer[] sprites = null;
        [TitleGroup("Models & Animation", order: 1)]
        public bool isDefaultSpriteFlipX = false; // Default Sprite Renderer Direction is Right 
        [TitleGroup("Models & Animation", order: 1), ReadOnly]
        public float deathAnimDuration = 0f;
        [TitleGroup("Models & Animation", order: 1), SerializeField]
        protected Transform centerPointTr = null;
        public Transform CenterPointTr => centerPointTr;
        [TitleGroup("Models & Animation", order: 1), SerializeField]
        private SoundKey[] hitSoundKeys = { };

        // Order 2
        [TitleGroup("Movement", order: 2), SerializeField] protected float defaultMoveSpeed = 8f;
        [TitleGroup("Movement", order: 2), SerializeField] protected float currentMoveSpeed = 0f;

        // Fields
        [field: SerializeField] public Transform Tr { get; private set; } = null;
        protected Animator anim = null;
        protected MonsterStat stat = null;
        protected Collider2D bodyCollider = null;
        protected Rigidbody2D rigidBody = null;
        private float originAnimationSpeed = 0f;
        protected bool isKnockBacking = false;
        private Coroutine knockBackCoroutine = null;

        protected virtual void Initialize() {
            anim = GetComponent<Animator>();
            bodyCollider = GetComponent<Collider2D>();
            rigidBody = GetComponent<Rigidbody2D>();
            originAnimationSpeed = anim.speed;
            
            SoundManager.Inst.RegistAudioClips(hitSoundKeys);
        }

        protected virtual void OnActivated() {
            // Awake    -> Initialize 사용
            // OnEnable -> Activated 사용
            // awake 사용을 피하고 virtual, overriding을 사용하기 위해 함수를 따로 두었음
        }

        protected void Start() {
            SubscribeOnEnableObservable();
        }

        protected void Update() {
            StateUpdate();
        }

        protected void FixedUpdate() {
            StateFixedUpdate();
        }

        private void SubscribeOnEnableObservable() {
            this.OnEnableAsObservable()
                .Skip(TimeSpan.Zero)
                .TakeUntilDestroy(this)
                .DoOnSubscribe(() => {
                    this.Initialize();
                    this.OnActivated();
                })
                .Subscribe(_ => { this.OnActivated(); })
                .AddTo(this);
        }

        private void StateUpdate() {
            switch (State) {
                case EnumMonsterState.None:       break;
                case EnumMonsterState.Idle:       OnUpdateIdleState(); break;
                case EnumMonsterState.Tracking:   OnUpdateTrackingState(); break;
                case EnumMonsterState.Patrol:     break;
                case EnumMonsterState.Attack:     OnUpdateAttackState(); break;
                case EnumMonsterState.Death:      OnUpdateDeathState(); break;
                case EnumMonsterState.TakeDamage: OnUpdateTakeDamageState(); break;
                default:                          break;
            }
        }

        private void StateFixedUpdate() {
            switch (State) {
                case EnumMonsterState.None:     break;
                case EnumMonsterState.Idle:     break;
                case EnumMonsterState.Tracking: OnFixedUpdateTrackingState(); break;
                case EnumMonsterState.Patrol:   break;
                case EnumMonsterState.Attack:   OnFixedUpdateAttackState(); break;
                case EnumMonsterState.Death:    break;
                case EnumMonsterState.TakeDamage: OnFixedUpdateTakeDamageState(); break;
                default: CatLog.ELog("NotImplementedThisState."); return;
            }
        }

        protected void StateChange(EnumMonsterState targetState, float delaySeconds = 0f) {
            if (delaySeconds <= 0) {
                Execute();
                return;
            }

            Observable.Timer(TimeSpan.FromSeconds(delaySeconds))
                      .Skip(TimeSpan.Zero)
                      .TakeUntilDisable(this)
                      .Subscribe(_ => { Execute(); })
                      .AddTo(this);

            void Execute() {
                switch (State) {
                    case EnumMonsterState.None:       break;
                    case EnumMonsterState.Patrol:     break;
                    case EnumMonsterState.Idle:       OnExitIdleState(); break;
                    case EnumMonsterState.Tracking:   OnExitTrackingState(); break;
                    case EnumMonsterState.Attack:     OnExitAttackState(); break;
                    case EnumMonsterState.Death:      OnExitDeathState(); break;
                    case EnumMonsterState.TakeDamage: OnExitTakeDamageState(); break;
                    default:                          break;
                }

                this.State = targetState;

                switch (State) {
                    case EnumMonsterState.None:       break;
                    case EnumMonsterState.Patrol:     break;
                    case EnumMonsterState.Idle:       OnEnterIdleState(); break;
                    case EnumMonsterState.Tracking:   OnEnterTrackingState(); break;
                    case EnumMonsterState.Attack:     OnEnterAttackState(); break;
                    case EnumMonsterState.Death:      OnEnterDeathState(); break;
                    case EnumMonsterState.TakeDamage: OnEnterTakeDamageState(); break;
                    default:                          break;
                }
            }
        }

        public void PlayRandomHitSound() {
            if (hitSoundKeys.Length == 0) {
                return;
            }
            var randomSoundKey = hitSoundKeys[Random.Range(0, hitSoundKeys.Length - 1)];
            SoundManager.Inst.PlaySE(randomSoundKey.ToKey(), 0.6f);
        }

        #region IDLE

        protected virtual void OnEnterIdleState() { }

        protected virtual void OnUpdateIdleState() { }

        protected virtual void OnExitIdleState() { }

        #endregion

        #region TRACKING

        protected virtual void OnEnterTrackingState() { }

        protected virtual void OnUpdateTrackingState() { }

        protected virtual void OnFixedUpdateTrackingState() { }

        protected virtual void OnExitTrackingState() { }

        #endregion

        #region DEATH

        protected virtual void OnEnterDeathState() { }

        protected virtual void OnUpdateDeathState() { }

        protected virtual void OnExitDeathState() { }

        #endregion

        #region ATTACK

        protected virtual void OnEnterAttackState() { }

        protected virtual void OnUpdateAttackState() { }

        protected virtual void OnFixedUpdateAttackState() { }

        protected virtual void OnExitAttackState() { }

        #endregion

        #region TAKE DAMAGE

        protected virtual void OnEnterTakeDamageState() { }

        protected virtual void OnUpdateTakeDamageState() { }

        protected virtual void OnFixedUpdateTakeDamageState() { }

        protected virtual void OnExitTakeDamageState() { }

        #endregion
        
        #region STUNNED
        
        protected virtual void OnEnterStunnedState() { }
        
        protected virtual void OnUpdateStunnedState() { }
        
        protected virtual void OnExitStunnedState() { }
        
        #endregion

        #region OTHER METHODS
        
        public void SetStat(MonsterStat stat) => this.stat = stat;

        public void SetCurrentMoveSpeed(float randomRange) {
            currentMoveSpeed = defaultMoveSpeed + UnityEngine.Random.Range(-randomRange, randomRange);
        }

        public void SetDisplay(bool isDisplay) {
            foreach (var sprite in sprites) {
                sprite.enabled = isDisplay;
            }

            if (bodyCollider) {
                bodyCollider.enabled = isDisplay;
            }
        }

        protected void SetFlipX(bool isRight) => sprite.flipX = isDefaultSpriteFlipX ? isRight : !isRight;

        protected void IncreaseAnimationSpeed(float ratio) => anim.speed = originAnimationSpeed * ratio;

        protected void RestoreAnimationSpeed() => anim.speed = originAnimationSpeed;

        public void SetModelColorByHex(string hexColor) {
            if (!ColorUtility.TryParseHtmlString(hexColor, out Color color)) {
                CatLog.ELog($"Invalid Hex Color Code: {hexColor}");
                return;
            }
            
            SetModelColor(color);
        }

        public void SetModelColor(Color color) {
            sprite.color = color;
        }

        public void RestoreModelColor() {
            sprite.color = Color.white;
        }

        public virtual void OnTakeDamage() { }

        public virtual void OnDeath() {
            StateChange(EnumMonsterState.Death);
        }
        
        /// <summary>
        /// Returns the direction and arrival status of the monster from the current monster's location to the player.
        /// </summary>
        /// <param name="sqrDistance">SqrMagnitude Distance</param>
        /// <param name="normalizedDirection">Direction to Player</param>
        /// <returns>is Arrival to Player</returns>
        protected bool TrackPlayerToCertainDistance(float sqrDistance, out Vector2 normalizedDirection) {
            // Get Direction to Player Position
            normalizedDirection = Math2DHelper.GetDirection(Tr.position, RogueLiteManager.Inst.SpawnedPlayerPosition);
            
            // Get Distance
            var sqrDistanceToPlayer = normalizedDirection.sqrMagnitude;
            
            // Trans to Normalized
            normalizedDirection.Normalize();
            
            // Return Result
            return sqrDistanceToPlayer <= sqrDistance;
        }
        
        protected void SetVelocity(Vector2 direction, float speed) => rigidBody.velocity = direction * speed;
        
        protected void SetVelocityZero() => rigidBody.velocity = Vector2.zero;
        
        public void AddForceToDirection(Vector2 direction, float forceValue, ForceMode2D mode) {
            rigidBody.AddForce(direction * forceValue, mode);
        }

        public void StartKnockBackProcessCoroutine(Vector2 direction, float forceValue) {
            if (!IsKnockBackable || State == EnumMonsterState.Death/* || isKnockBacking*/)
                return;
            
            StopKnockBackCoroutine();
            knockBackCoroutine = StartCoroutine(KnockBackCoroutine(direction, forceValue));
        }
        
        private IEnumerator KnockBackCoroutine(Vector2 direction, float forceValue) {
            SetVelocityZero();
            AddForceToDirection(direction, forceValue, ForceMode2D.Impulse);
            isKnockBacking = true;
            
            yield return new WaitUntil(() => rigidBody.velocity.magnitude <= 0.3f);

            isKnockBacking = false;
        }
        
        private void StopKnockBackCoroutine() {
            if (knockBackCoroutine == null) {
                return;
            }
            
            StopCoroutine(knockBackCoroutine);
            knockBackCoroutine = null; 
        }
        
        protected void DespawnOnDeathAnimationCompleted() {
            // TODO: Optimization
            if (IsDespawnOnDeathAnimationCompleted) {
                Observable.Timer(TimeSpan.FromSeconds(deathAnimDuration))
                          .Skip(0)
                          .TakeUntilDisable(this)
                          .Subscribe(_ => Despawn())
                          .AddTo(this);
            }
        }
        
        public void Despawn() {
            if (ObjectPoolManager.IsExist) {
                if (ObjectPoolManager.Inst.IsExistInPool(gameObject.name)) {
                    ObjectPoolManager.Inst.Despawn(gameObject);
                    return;
                }
            }
            Destroy(gameObject);
        }
        
        #endregion
        
        #region Status
        
        public virtual void StartStunState() {
            if (State == EnumMonsterState.Death) {
                return;
            }
            StateChange(EnumMonsterState.Stunned);
        }

        public virtual void StopStunState() {
            if (State != EnumMonsterState.Stunned) {
                return;
            }
            StateChange(EnumMonsterState.Idle);
        }
        
        #endregion
        
        #region Editor

        [Button("Bake Animation Data", ButtonSizes.Medium), PropertySpace(10), PropertyOrder(99)]
        public void BakeAnimationData() {
#if UNITY_EDITOR
            // Try Get Animation Controller 
            if (TryGetComponent(out Animator animator) == false) {
                CatLog.ELog("Failed To Get Animator Component !");
                return;
            }
            var animatorController = animator.runtimeAnimatorController as AnimatorController;
            if (animatorController == null) {
                CatLog.ELog("Animator does not have an AnimatorController.");
                return;
            }

            // Loop through each layer in the Animator Controller For Find Death Animation
            bool isFindDeathAnimation = false;
            foreach (AnimatorControllerLayer layer in animatorController.layers) {
                // Loop through each state in the layer
                foreach (ChildAnimatorState state in layer.stateMachine.states) {
                    // Get the Motion associated with the state
                    Motion motion = state.state.motion;

                    // If the Motion is an AnimationClip, we can access its details
                    if (motion is AnimationClip clip) {
                        // Logging Animation Info's
                        /*CatLog.Log("Animation Clip Name: " + clip.name);
                        CatLog.Log("Animation Clip Length: " + clip.length);*/

                        // You can add more details you want to retrieve here
                        if (clip.name != "clip_death") 
                            continue;
                        deathAnimDuration = clip.length;
                        isFindDeathAnimation = true;
                    }
                }
            }

            if (!isFindDeathAnimation) {
                CatLog.ELog($"Failed To Find DeathAnimation In AnimatorController. name: {gameObject.name}");
            }
#endif
        }
        
        #endregion
    }
}
