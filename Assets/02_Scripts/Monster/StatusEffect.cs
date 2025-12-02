namespace CoffeeCat {
    public class StatusEffect {
        public int Level { get; private set; } = 0;
        public float Duration { get; private set; } = 0f;       // Seconds
        public float RemainDuration { get; private set; } = 0f; // Seconds
        public float Tick { get; private set; } = 0f;
        public float DamagePerTick { get; private set; } = 0f;
        public bool IsRunning { get; private set; } = false;

        public void Clear() {
            Level = 0;
            Duration = 0f;
            RemainDuration = 0f;
            Tick = 0f;
            DamagePerTick = 0f;
            IsRunning = false;
        }
        
        public void RefreshRemain() {
            RemainDuration = Duration;
        }

        public void SetRemainDuration(float remainDuration) {
            RemainDuration = remainDuration;
        }

        public void SetPoison(StatusEffectRecord entity) {
            Level = entity.Level;
            Duration = entity.Duration;
            RemainDuration = entity.Duration;
            Tick = entity.Interval;
            DamagePerTick = entity.Damage;
            IsRunning = true;
        }

        public void SetBleeding(StatusEffectRecord entity) {
            Level = entity.Level;
            Duration = entity.Duration;
            RemainDuration = entity.Duration;
            Tick = entity.Interval;
            DamagePerTick = entity.Damage;
            IsRunning = true;
        }

        public void SetStun(StatusEffectRecord entity) {
            Level = entity.Level;
            Duration = entity.Duration;
            RemainDuration = entity.Duration;
            IsRunning = true;
        }

        public void Stacking(StatusEffectRecord entity) {
            Level += entity.Level;
            Duration = entity.Duration;
            RemainDuration = entity.Duration;
            Tick = entity.Interval;
            DamagePerTick += entity.Damage;;
        }
    }
}
