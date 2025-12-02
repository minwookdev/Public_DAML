using System;
using UnityEngine;
using UniRx;
using Sirenix.OdinInspector;
using CoffeeCat.Utils;
using CoffeeCat.FrameWork;

namespace CoffeeCat {
    public class Effector : MonoBehaviour {
        [Title("OPTIONS", TitleAlignment = TitleAlignments.Centered)]
        [SerializeField] private float defaultLifeTime = 1f;
        [SerializeField] private bool isDespawnAfterLifeTime = true;
        
        [Title("Flip")]
        [SerializeField] private Vector2 rightFlipValue = Vector2.zero;
        [SerializeField] private Vector2 leftFlipValue = Vector2.zero;
        
        [Title("Pivot")]
        [SerializeField] private Vector2 rightPivotValue = Vector2.zero;
        [SerializeField] private Vector2 leftPivotValue = Vector2.zero;

        [TitleGroup("COMPONENTS", Alignment = TitleAlignments.Centered)]
        [InfoBox("Automatic configuration is recommended. After adding the script.")]
        [SerializeField] private Transform tr = null;
        [SerializeField, PropertySpace(SpaceAfter = 5f)] private ParticleSystem[] particleSystems = null;

        public enum ParticleFixedDirection : byte
        {
            None  = 0,
            Right = 1,
            Left  = 2,
            Up    = 3,
            Down  = 4
        }

        private void Awake() {
            if (tr == null) {
                tr = GetComponent<Transform>();
            }

            // Get ParticleSystem Components
            if (particleSystems == null || particleSystems.Length <= 0) {
                particleSystems = GetComponentsInChildren<ParticleSystem>();
                CatLog.WLog($"this effect Recommanded Configure. name: {gameObject.name}");
            }
        }

        public void Play(EffectPlayOptions options = default(EffectPlayOptions)) {
            if (options.IsPlayRandomRotate) {
                var randomEulerAngles = tr.eulerAngles;
                randomEulerAngles.z = UnityEngine.Random.Range(-360f, 360f);
                tr.eulerAngles = randomEulerAngles;
            }
            
            // Set Particle Systems Flip From Flip Direction Value
            if (options.IsApplyFlipFromDirection) {
                SetAllParticlesFlip(options.FixedDirection);
            }
            
            // Set Particle System Pivot From Pivot Direction Value
            if (options.IsApplyPivotFromDirection) {
                SetAllParticlesPivot(options.FixedDirection);
            }
            
            if (options.PlayDelaySeconds <= 0f) {
                Execute();
                return;
            }

            Observable.Timer(TimeSpan.FromSeconds(options.PlayDelaySeconds))
                      .TakeUntilDisable(this)
                      .Subscribe(_ => {
                          Execute();
                      })
                      .AddTo(this);

            void Execute() {
                AllParticles((particleSystem) => {
                    particleSystem.Play();
                });

                if (isDespawnAfterLifeTime) {
                    ObjectPoolManager.Inst.Despawn(gameObject, defaultLifeTime + options.DespawnDelaySeconds);
                }
            }
        }

        public void Stop(bool isStopEmittingAndClear = true) {
            AllParticles((system) => {
                var particleSystemStopBehavior = (isStopEmittingAndClear)
                    ? ParticleSystemStopBehavior.StopEmittingAndClear
                    : ParticleSystemStopBehavior.StopEmitting;        // this is Particle Systems Default
                system.Stop(false, particleSystemStopBehavior);
            });
        }

        private void AllParticles(Action<ParticleSystem> action) {
            foreach (var particleSystem in particleSystems) {
                action(particleSystem);
            }
        }

        private void AllParticlesRenderer(Action<ParticleSystemRenderer> onRendererAction) {
            foreach (var system in particleSystems) {
                if (!system.TryGetComponent(out ParticleSystemRenderer particleSystemRenderer)) {
                    return;
                }
                onRendererAction.Invoke(particleSystemRenderer);
            }
        }

        public void SetAllParticlesFlip(ParticleFixedDirection direction) {
            Vector2 flipValue = Vector2.zero;
            switch (direction) {
                case ParticleFixedDirection.None:  break;
                case ParticleFixedDirection.Right: flipValue = rightFlipValue; break;
                case ParticleFixedDirection.Left:  flipValue = leftFlipValue;  break;
                case ParticleFixedDirection.Up:
                case ParticleFixedDirection.Down:
                default:
                    CatLog.ELog("Not Implemented This Direction Type.");
                    return;
            }
            AllParticlesRenderer(particleSystemRenderer => particleSystemRenderer.flip =  flipValue);
        }

        public void SetAllParticlesPivot(ParticleFixedDirection direction) {
            Vector2 pivotValue = Vector2.zero;
            switch (direction) {
                case ParticleFixedDirection.None:  break;
                case ParticleFixedDirection.Right: pivotValue = rightPivotValue; break;
                case ParticleFixedDirection.Left:  pivotValue = leftPivotValue;  break;
                case ParticleFixedDirection.Up:
                case ParticleFixedDirection.Down:
                default:
                    CatLog.ELog("Not Implemented This Direction Type.");
                    return;
            }
            AllParticlesRenderer(particleSystemRenderer => { particleSystemRenderer.pivot = pivotValue; });
        }

        #region BUTTONS

        [BoxGroup("Buttons", ShowLabel = false)]
        [Button("Auto Configuration")]
        private void AutoConfiguration() {
            float longestDuration = 0f;
            ParticleSystem[] allParticleSystems = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < allParticleSystems.Length; i++) {
                var mainModules = allParticleSystems[i].main;
                mainModules.playOnAwake = false;
                longestDuration = (longestDuration < mainModules.duration) ? mainModules.duration : longestDuration;

                //var renderModules = allParticleSystems[i].GetComponent<ParticleSystemRenderer>();
                //renderModules.renderMode = ParticleSystemRenderMode.Billboard;
                //renderModules.alignment = ParticleSystemRenderSpace.Local;
            }

            particleSystems = allParticleSystems;
            defaultLifeTime = longestDuration;
            isDespawnAfterLifeTime = true;
            tr = GetComponent<Transform>();

            CatLog.Log($"Auto Configuration Completed. name: {gameObject.name}");
        }

        [BoxGroup("Buttons", ShowLabel = false), Button("Set to Default")]
        private void SetDefault() {
            defaultLifeTime = 1f;
            isDespawnAfterLifeTime = true;
        }

        [BoxGroup("Buttons", showLabel: false), Button("Force Play")]
        private void ForcePlay() {
            AllParticles(particleSystem => particleSystem.Play());
        }

        #endregion
    }

    public struct EffectPlayOptions {
        public float PlayDelaySeconds;
        public float DespawnDelaySeconds;
        public bool IsPlayRandomRotate;
        public bool IsApplyPivotFromDirection;
        public bool IsApplyFlipFromDirection;
        public Effector.ParticleFixedDirection FixedDirection;

        public static EffectPlayOptions Custom(float playDelaySeconds = 0f, float despawnDelaySeconds = 0f, bool isRandomRotation = false,
                                               bool isSetPivotFromDirection = false, bool isSetFlipFromDirection = false,
                                               Effector.ParticleFixedDirection direction = Effector.ParticleFixedDirection.None) {
            return new EffectPlayOptions(playDelaySeconds, despawnDelaySeconds, isRandomRotation, isSetPivotFromDirection, isSetFlipFromDirection, direction);
        }

        private EffectPlayOptions(float playDelaySeconds = 0f, float despawnDelaySeconds = 0f, 
                                  bool isRandomRotation = false, bool isSetPivotFromDirection = false, bool isSetFlipFromDirection = false,
                                  Effector.ParticleFixedDirection direction = Effector.ParticleFixedDirection.None) {
            PlayDelaySeconds = playDelaySeconds;
            DespawnDelaySeconds = despawnDelaySeconds;
            IsPlayRandomRotate = isRandomRotation;
            IsApplyFlipFromDirection = isSetFlipFromDirection;
            IsApplyPivotFromDirection = isSetPivotFromDirection;
            FixedDirection = direction;
        }
    }
}
