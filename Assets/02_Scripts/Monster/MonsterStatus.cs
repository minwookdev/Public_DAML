using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class MonsterStatus : MonoBehaviour {
        [Title("MONSTER STATUS")]
        [SerializeField] private MonsterState state = null;
        [SerializeField, DisableInPlayMode] private string customStatLoadKey = string.Empty;
        [field: SerializeField, ReadOnly] public MonsterStat CurrentStat { get; private set; } = null;
        private MonsterStat originStat = null; // do not modify this value
        public bool IsAlive => CurrentStat.HP > 0f;
        private readonly KilledInfo killedInfo = new();
        private CancellationTokenSource modelColorChangedCts = null;
        
        [Title("Health Bar Options")]
        [SerializeField] private bool useHealthBar = false;
        [SerializeField] private float healthBarOffsetY = 1.1f;
        [SerializeField, ReadOnly] private HealthBar healthBar = null;

        // status effect variables
        private StatusEffectData statusEffectData = null;
        private readonly Dictionary<StatusEffectType, StatusEffect> statusEffectContainer = new();
        private CancellationTokenSource poisonEffectCts = null;
        private CancellationTokenSource bleedingEffectCts = null;
        private CancellationTokenSource stunEffectCts = null;
        private bool isPoisonCancelled = true;
        private bool isBleedingCancelled = true;
        private bool isStunCancelled = true;

        // color hex by status effect
        private const string HEX_COLOR_POISON_STATUS   = "#78FF66";
        private const string HEX_COLOR_BLEEDING_STATUS = "#FF6767";
        private static readonly Color DefaultColor = Color.white;
        private static readonly Color DamagedColor = Color.red;

        public class KilledInfo {
            public float ExpAmount;
            public float DeathAnimDuration;
            public int LootCategory;
            public Vector2 Position;

            public void Update(float expAmount, Vector2 position, int lootCategory, float deathAnimDuration) {
                ExpAmount = expAmount;
                Position = position;
                LootCategory = lootCategory;
                DeathAnimDuration = deathAnimDuration;
            }
        }

        private void Start() {
            // Get Origin Stat Data
            customStatLoadKey = (customStatLoadKey.Equals(string.Empty)) ? gameObject.name : customStatLoadKey;
            var monsterStatsDataDictionary = DataManager.Inst.MonsterStats.DataDictionary;
            if (!monsterStatsDataDictionary.TryGetValue(customStatLoadKey, out MonsterStat result)) {
                CatLog.WLog($"Not Found Monster Stat Data. Key: {customStatLoadKey}");
                return;
            }

            // DeepCopy Stat Data From Origin Stat
            originStat = result.DeepCopyMonsterStat();
            CurrentStat = new MonsterStat();
            state.SetStat(CurrentStat);
            RestoreToOriginStat();

            statusEffectData = DataManager.Inst.StatusEffects;
        }

        private void OnEnable() {
            RestoreToOriginStat();
            state.RestoreModelColor();
            state.SetCurrentMoveSpeed(CurrentStat.SpeedRandomRange);
        }

        private void OnDisable() {
            if (healthBar) {
                healthBar = null;
            }
            CancelOnDamagedColorChanged();
        }

        public void Spawn() {
            if (!useHealthBar) {
                return;
            }
            healthBar = ObjectPoolManager.Inst.Spawn<HealthBar>(AddressablesKey.HealthBar.ToKey(), Vector3.zero);
            healthBar.Init(state.Tr, healthBarOffsetY);
        }

        private void RestoreToOriginStat() {
            if (originStat == null)
                return;
            CurrentStat.CopyValue(originStat);
        }

        public void OnDamaged(DamageResult result, Vector2 collisionPoint = default, bool playHitSound = false) {
            // start on-damaged process
            OnDamagedPreProcess(result);
            OnDamaged(result.CalculatedDamage, true, collisionPoint, result.KnockBackForce, playHitSound);
            if (!IsAlive) {
                return;
            }
            OnDamagedPostProcess(result);
        }

        private void OnDamaged(float damageValue, bool useDamageText, Vector2 collisionPoint = default, float force = 0f, bool playHitSound = false) {
            if (!IsAlive) {
                return;
            }

            Vector2 knockBackDirection = Vector2.zero;
            if (collisionPoint != default && force > 0f) {
                knockBackDirection = (Vector2)transform.position - collisionPoint;
                knockBackDirection.Normalize();
            }

            // Damage Process
            float finalCalculatedDamageCount = damageValue;
            float tempHealthPoint = CurrentStat.HP - finalCalculatedDamageCount;
            if (tempHealthPoint <= 0f) {
                // RogueLiteManager.Inst.GoldDrop(state.Tr.position, 100);
                Kill();
            }
            else {
                // Decrease Monster Health Point
                CurrentStat.HP = tempHealthPoint;
                state.OnTakeDamage();
                if (healthBar) {
                    healthBar.SetValue(CurrentStat.HP, originStat.HP);
                }

                // Start KnockBack Process
                if (knockBackDirection != Vector2.zero) {
                    state.StartKnockBackProcessCoroutine(knockBackDirection, force);
                }
            }
            
            // Change Damaged Color
            StartColorChangedOnDamaged();

            // play hit sound
            if (playHitSound) {
                state.PlayRandomHitSound();
            }
            
            // Damage Text Process 
            if (!useDamageText) {
                return;
            }

            int floatingCount = Mathf.RoundToInt(finalCalculatedDamageCount);
            if (knockBackDirection != Vector2.zero) {
                DamageTextManager.Inst.OnReflectingText(floatingCount, collisionPoint, knockBackDirection, false);
            }
            else {
                Vector2 textStartPos = collisionPoint == default ? transform.position : collisionPoint;
                textStartPos.y += 1.25f;
                DamageTextManager.Inst.OnFloatingText(floatingCount, textStartPos, false);
            }
        }

        public Transform GetCenterTr() {
            if (state && state.CenterPointTr) {
                return state.CenterPointTr;
            }
            CatLog.ELog("Monster State or Center Point Transform is Null");
            return null;
        }

        public void Kill(bool isInvokeEvent = true) {
            if (!IsAlive) {
                return;
            }
            
            // Clear All Status Effects
            CancelAllStatusEffects();
            
            // Send to Death State
            CurrentStat.HP = 0f;
            state.OnDeath();
            if (healthBar) {
                healthBar.Despawn();
                healthBar = null;
            }

            if (!isInvokeEvent) {
                return;
            }
            // Invoke killed event
            InvokeKilledEvent();
        }

        private void InvokeKilledEvent() {
            // add killed count for current room
            DungeonSceneBase.Inst.AddCurrentRoomKillCount();
            
            // invoke monster killed event
            killedInfo.Update(CurrentStat.ExpAmount, state.Tr.position, CurrentStat.LootCategory, state.deathAnimDuration);
            DungeonEvtManager.InvokeEventMonsterKilledByPlayer(killedInfo);
        }

        private void StartColorChangedOnDamaged() {
            // ignore if specified status effect is running
            foreach (var pair in statusEffectContainer) {
                var value = pair.Value;
                if (!value.IsRunning) {
                    continue;
                }
                switch (pair.Key) {
                    case StatusEffectType.Poison:
                    case StatusEffectType.Bleed:
                        return;
                    case StatusEffectType.None:
                    case StatusEffectType.Stun:
                        break;
                    default: throw new NotImplementedException();
                }
            }
            
            // start color change
            ChangeColorOnDamagedAsync().Forget();
        }

        private async UniTaskVoid ChangeColorOnDamagedAsync() {
            CancelOnDamagedColorChanged();
            modelColorChangedCts = new CancellationTokenSource();
            
            state.SetModelColor(DamagedColor);
            const float DAMAGED_COLOR_RESTORE_TIME = 0.25f;
            float timeElapsed = 0f;
            while (timeElapsed < DAMAGED_COLOR_RESTORE_TIME) {
                timeElapsed += Time.deltaTime;
                var color = Color.Lerp(DamagedColor, DefaultColor, timeElapsed / DAMAGED_COLOR_RESTORE_TIME);
                state.SetModelColor(color);
                var isCanceled = await UniTask.Yield(PlayerLoopTiming.Update, modelColorChangedCts.Token).SuppressCancellationThrow();
                if (isCanceled) {
                    return;
                }
            }
            
            state.SetModelColor(DefaultColor);
        }
        
        private void CancelOnDamagedColorChanged() {
            if (modelColorChangedCts == null) {
                return;
            }
            modelColorChangedCts.Cancel();
            modelColorChangedCts.Dispose();
            modelColorChangedCts = null;
        }

        #region Sub Attack Effects

        private void OnDamagedPreProcess(DamageResult damageResult) {
            // effect: life drain 
            var lifeDrainValue = damageResult.LifeDrainValue;
            if (lifeDrainValue > 0f) {
                RogueLiteManager.Inst.IncreasePlayerHealth(lifeDrainValue);
            }
            
            // effect: instant kill
            if (damageResult.IsInstKillOccurs) {
                damageResult.SetDamageToMax();
            }
            
            // effect: knock back
            // knockback is processed in OnDamaged method
        }

        private void OnDamagedPostProcess(DamageResult damageResult) {
            if (damageResult.IsOccursPoison) {
                OnTakenPoison(damageResult.PoisonTierValue);
            }

            if (damageResult.IsOccursBleeding) {
                OnTakenBleed(damageResult.BleedingTierValue);
            }

            if (damageResult.IsOccursStun) {
                OnTakenStun(damageResult.StunTierValue);
            }
        }

        #region Poison

        private void OnTakenPoison(int level) {
            if (!statusEffectContainer.TryGetValue(StatusEffectType.Poison, out var poisonStatus)) {
                poisonStatus = new StatusEffect();
                statusEffectContainer.Add(StatusEffectType.Poison, poisonStatus);
            }
            else {
                // if the existed check already poisoned
                if (poisonStatus.IsRunning) {
                    if (poisonStatus.Level > level) {
                        return;
                    }
                }
            }

            if (!statusEffectData.TryGetRecord(StatusEffectType.Poison, level, out StatusEffectRecord entity)) {
                CatLog.WLog("Failed to Get Poison Status Effect Record" + level.ToString());
                return;
            }
            
            // refresh poison
            CancelPoisonEffect();
            CancelOnDamagedColorChanged();
            poisonStatus.SetPoison(entity);
            UpdatePoisonEffectAsync().Forget();
        }

        private async UniTaskVoid UpdatePoisonEffectAsync() {
            // cancel previous poison effect async method
            poisonEffectCts = new CancellationTokenSource();
            isPoisonCancelled = false;

            // set model color
            state.SetModelColorByHex(HEX_COLOR_POISON_STATUS);

            // start poison status effect
            var poisonStatus = statusEffectContainer[StatusEffectType.Poison];
            var duration = poisonStatus.Duration;
            var remainDuration = duration;
            var tick = poisonStatus.Tick;
            var nextTickTime = remainDuration - tick;
            while (remainDuration > 0) {
                // poison damage taken process
                if (nextTickTime >= remainDuration) {
                    state.SetModelColorByHex(HEX_COLOR_POISON_STATUS);
                    OnDamaged(poisonStatus.DamagePerTick, true);
                    nextTickTime -= tick;
                }

                // update remain time
                remainDuration -= Time.deltaTime;
                poisonStatus.SetRemainDuration(remainDuration);

                // wait for next frame. if cancelled, return
                var isCancelled = await WaitPoisonWithCancelled();
                if (isCancelled) {
                    return;
                }
            }

            // end poison effect
            poisonStatus.Clear();
            RestoreModelColorByCurrentStatus();
        }

        private async UniTask<bool> WaitPoisonWithCancelled() {
            if (isPoisonCancelled) {
                return true;
            }
            var isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, poisonEffectCts.Token).SuppressCancellationThrow();
            return isCancelled;
        }

        private void CancelPoisonEffect() {
            if (poisonEffectCts == null || isPoisonCancelled) {
                return;
            }

            poisonEffectCts.Cancel();
            poisonEffectCts.Dispose();
            isPoisonCancelled = true;
        }

        private void CancelWithClearPoisonEffect() {
            CancelPoisonEffect();

            if (!statusEffectContainer.TryGetValue(StatusEffectType.Poison, out var value)) {
                return;
            }
            value.Clear();
        }

        #endregion

        #region Bleeding

        private void OnTakenBleed(int level) {
            if (!statusEffectContainer.TryGetValue(StatusEffectType.Bleed, out StatusEffect bleedStatus)) {
                bleedStatus = new StatusEffect();
                statusEffectContainer.Add(StatusEffectType.Bleed, bleedStatus);
            }
            
            // try to get status effect record
            if (!statusEffectData.TryGetRecord(StatusEffectType.Bleed, level, out StatusEffectRecord entity)) {
                CatLog.WLog("Failed to Get Bleed Status Effect Record" + level.ToString());
                return;
            }

            // clear and set or stacking bleeding
            CancelOnDamagedColorChanged();
            CancelBleedEffect();
            if (!bleedStatus.IsRunning) {
                bleedStatus.SetBleeding(entity);
            }
            else {
                bleedStatus.Stacking(entity);
            }

            // start async bleeding
            UpdateBleedEffectAsync().Forget();
        }

        private async UniTaskVoid UpdateBleedEffectAsync() {
            bleedingEffectCts = new CancellationTokenSource();
            isBleedingCancelled = false;

            // set model color
            state.SetModelColorByHex(HEX_COLOR_BLEEDING_STATUS);

            // start bleeding status effect
            var bleedingStatus = statusEffectContainer[StatusEffectType.Bleed];
            var duration = bleedingStatus.Duration;
            var remainDuration = duration;
            var refTick = bleedingStatus.Tick;
            var tick = remainDuration - refTick;
            while (remainDuration > 0) {
                // poison damage taken process
                if (tick >= remainDuration) {
                    state.SetModelColorByHex(HEX_COLOR_BLEEDING_STATUS);
                    OnDamaged(bleedingStatus.DamagePerTick, true);
                    tick -= refTick;
                }

                // update remain time
                remainDuration -= Time.deltaTime;
                bleedingStatus.SetRemainDuration(remainDuration);

                // wait for next frame. if cancelled, return
                var isCancelled = await WaitBleedWithCancelled();
                if (isCancelled) {
                    // cancelled actions
                    return;
                }
            }

            // end poison effect
            bleedingStatus.Clear();
            RestoreModelColorByCurrentStatus();
        }
        
        private async UniTask<bool> WaitBleedWithCancelled() {
            if (isBleedingCancelled) {
                return true;
            }
            var isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, bleedingEffectCts.Token).SuppressCancellationThrow();
            return isCancelled;
        }

        private void CancelBleedEffect() {
            if (bleedingEffectCts == null || isBleedingCancelled) {
                return;
            }

            bleedingEffectCts.Cancel();
            bleedingEffectCts.Dispose();
            isBleedingCancelled = true;
        }

        private void CancelWithClearBleedEffect() {
            CancelBleedEffect();

            if (!statusEffectContainer.TryGetValue(StatusEffectType.Bleed, out var value)) {
                return;
            }
            value.Clear();
        }

        #endregion

        #region Stun

        private void OnTakenStun(int level) {
            if (!statusEffectContainer.TryGetValue(StatusEffectType.Stun, out StatusEffect stunStatus)) {
                stunStatus = new StatusEffect();
                statusEffectContainer.Add(StatusEffectType.Stun, stunStatus);
            }

            if (!statusEffectData.TryGetRecord(StatusEffectType.Stun, level, out StatusEffectRecord entity)) {
                CatLog.WLog("Failed to Get Stun Status Effect Record" + level.ToString());
                return;
            }
            
            // clear and restart stun effect
            CancelStunEffect();
            stunStatus.SetStun(entity);
            UpdateStunEffectAsync().Forget();
        }

        private async UniTaskVoid UpdateStunEffectAsync() {
            stunEffectCts = new CancellationTokenSource();
            isStunCancelled = false;

            // set monster stun state
            state.StartStunState();

            var stunStatus = statusEffectContainer[StatusEffectType.Stun];
            var duration = stunStatus.Duration;
            var remainDuration = duration;
            while (remainDuration <= 0f) {
                // update remain time
                remainDuration -= Time.deltaTime;
                stunStatus.SetRemainDuration(remainDuration);

                // wait for next frame. if cancelled, return
                var isCancelled = await WaitStunWithCancelled();
                if (isCancelled) {
                    return;
                }
            }

            stunStatus.Clear();
            state.StopStunState();
        }

        private async UniTask<bool> WaitStunWithCancelled() {
            if (isStunCancelled) {
                return true;
            }

            var isCancelled = await UniTask.Yield(PlayerLoopTiming.Update, stunEffectCts.Token).SuppressCancellationThrow();
            return isCancelled;
        }

        private void CancelStunEffect() {
            if (stunEffectCts == null || isStunCancelled) {
                return;
            }

            stunEffectCts.Cancel();
            stunEffectCts.Dispose();
            isStunCancelled = true;
        }

        private void CancelWithClearStunEffect() {
            CancelStunEffect();

            if (!statusEffectContainer.TryGetValue(StatusEffectType.Stun, out var value)) {
                return;
            }
            value.Clear();
            state.StopStunState();
        }

        #endregion
        
        private void CancelAllStatusEffects() {
            // var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(poisonEffectCts.Token, bleedingEffectCts.Token);
            // linkedCts.Cancel();
            // linkedCts.Dispose();
            
            // cancel with clear all status
            CancelWithClearPoisonEffect();
            CancelWithClearBleedEffect();
            CancelWithClearStunEffect();
        }

        private void RestoreModelColorByCurrentStatus() {
            /// *If you want, you can even change the color to track which status effects
            /// have the shortest damage interval remaining.
            
            if (statusEffectContainer.TryGetValue(StatusEffectType.Bleed, out var bleedingStatus) && bleedingStatus.IsRunning) {
                state.SetModelColorByHex(HEX_COLOR_BLEEDING_STATUS);
            }
            else if (statusEffectContainer.TryGetValue(StatusEffectType.Poison, out var poisonStatus) && poisonStatus.IsRunning) {
                state.SetModelColorByHex(HEX_COLOR_POISON_STATUS);
            }
            else {
                state.RestoreModelColor();
            }
        }
        
        #endregion
    }
}