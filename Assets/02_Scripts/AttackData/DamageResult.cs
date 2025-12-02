using System;
using Random = UnityEngine.Random;

namespace CoffeeCat.Datas {
    // TODO: 양수/음수 값 보정
    public class DamageResult {
        public const float MAX_DAMAGE_VALUE = 9999;
        
        public float CalculatedDamage { get; private set; } = 0f;
        public bool IsCritical { get; private set; } = false;
        private bool isCleared = true;
        
        public bool IsInstKillOccurs { get; private set; } = false;
        public bool IsOccursPoison { get; private set; } = false;
        public bool IsOccursBleeding { get; private set; } = false;
        public bool IsOccursStun { get; private set; } = false;
        public float LifeDrainValue { get; private set; } = 0f;
        public float KnockBackForce { get; private set; } = 0f;
        public int PoisonTierValue { get; private set; } = 0;
        public int BleedingTierValue { get; private set; } = 0;
        public int StunTierValue { get; private set; } = 0;
        
        /// <summary>
        /// Attacker: Monster, Defender: Player
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public void SetData(ProjectileDamageData attacker, PlayerStat defender) {
            ClearIfNot();
            
            SetIsCritical(attacker.CriticalChance, defender.CriticalResistance);                    // 치명타 발생 계산
            ApplySkillDamage(attacker.SkillDamage, attacker.BaseDamage, attacker.SkillCoefficient); // *스킬 데미지 배율 공식 적용
            ApplyCriticalDamage(attacker.CriticalMultiplier);                                       // 치명타 발생 시 데미지에 치명타 배율 적용
            ApplyDefenceValue(defender.Defense, attacker.Penetration);                              // 방어자 방어 수치 및 관통력 수치 계산
            CorrectionValue();                                                                      // 마이너스 데미지 보정
            isCleared = false;
        }

        /// <summary>
        /// Attacker: Monster Skill(Projectile), Defender: Player 
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="skillStat"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public void SetData(MonsterStat attacker, PlayerStat defender) {
            ClearIfNot();
            
            SetIsCritical(attacker.CriticalChance, defender.CriticalResistance); // 치명타 발생 계산
            SetDamageMinMax(attacker.Damage, attacker.DamageDeviation);          // 최소 ~ 최대 데미지 범위 내 산출
            ApplyCriticalDamage(attacker.CriticalMultiplier);                    // 치명타 발생 시 데미지에 치명타 배율 적용
            ApplyDefenceValue(defender.Defense, attacker.Penetration);           // 방어자 방어 수치 및 관통력 수치 계산
            CorrectionValue();                                                   // 마이너스 데미지 보정
            isCleared = false;
        }

        /// <summary>
        /// Attacker: Player Projectile, Defender: Monster 
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public void SetData(ProjectileDamageData attacker, MonsterStat defender) {
            ClearIfNot();
            
            // 치명타 발생 계산
            SetIsCritical(attacker.CriticalChance, defender.CriticalResist);    
            // *스킬 데미지 배율 공식 적용
            ApplySkillDamage(attacker.SkillDamage, attacker.BaseDamage, attacker.SkillCoefficient); 
            // 치명타 발생 시 데미지에 치명타 배율 적용
            ApplyCriticalDamage(attacker.CriticalMultiplier);                                       
            // 방어자 방어 수치 및 관통력 수치 계산
            ApplyDefenceValue(defender.Defence, attacker.Penetration);                              
            // 마이너스 데미지 보정
            CorrectionValue();                                                                      

            // 즉사 발생 계산
            IsInstKillOccurs = Extensions.EvaluateChance1F(attacker.InstKillChance);       
            // 넉백 발생 계산
            KnockBackForce = Extensions.EvaluateChance1F(attacker.KnockBackChance) ? attacker.KnockBackForce : 0f; 
            // 스턴 발생 계산
            IsOccursStun = Extensions.EvaluateChance1F(attacker.StunChance);
            if (IsOccursStun) {
                StunTierValue = attacker.StunTier;
            }
            // 중독 발생 계산
            IsOccursPoison = Extensions.EvaluateChance1F(attacker.PoisonChance);
            if (IsOccursPoison) {
                PoisonTierValue = attacker.PoisonTier;
            }
            // 출혈 발생 계산            
            IsOccursBleeding = Extensions.EvaluateChance1F(attacker.BleedChance);    
            if (IsOccursBleeding) {
                BleedingTierValue = attacker.BleedTier;
            }
            // 흡혈 발생 계산
            LifeDrainValue = Extensions.EvaluateChance1F(attacker.LifeDrainChance) ? CalculatedDamage * attacker.LifeDrainRatio : 0f; 
            
            // set dirty flag
            isCleared = false;
        }

        /// <summary>
        /// Attacker: Player, Defender: Monster
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="defender"></param>
        /// <returns></returns>
        public void SetData(PlayerStat attacker, MonsterStat defender) {
            ClearIfNot();
            
            SetIsCritical(attacker.CriticalChance, defender.CriticalResist); // 치명타 발생 계산
            SetDamageMinMax(attacker.AttackPower, attacker.DamageDeviation);               // 최소 ~ 최대 데미지 범위 내 산출
            ApplyCriticalDamage(attacker.CriticalMultiplier);                              // 치명타 발생 시 데미지에 치명타 배율 적용
            ApplyDefenceValue(defender.Defence, attacker.Penetration);                     // 방어자 방어 수치 및 관통력 수치 계산
            CorrectionValue();                                                             // 마이너스 데미지 보정
            isCleared = false;
        }

        private void SetDamageMinMax(float damageValue, float deviationValue) {
            CalculatedDamage = Random.Range(damageValue - (damageValue * deviationValue), damageValue + (damageValue * deviationValue));
        }
        
        private void SetIsCritical(float criticalChance, float criticalResistance) {
            IsCritical = criticalChance - criticalResistance >= Random.Range(1f, 100f);
        }

        private void ApplySkillDamage(float skillDamage, float attackerBaseDamage, float skillCoefficient) {
            CalculatedDamage = skillDamage + (attackerBaseDamage * skillCoefficient);
        }

        private void ApplyCriticalDamage(float criticalMultiplier) {
            CalculatedDamage *= IsCritical ? criticalMultiplier : 1f;
        }

        private void ApplyDefenceValue(float defenceValue, float penetrationValue) {
            float calculatedDefenceValue = defenceValue - (defenceValue * penetrationValue);
            calculatedDefenceValue = (calculatedDefenceValue < 0f) ? 0f : calculatedDefenceValue;
            CalculatedDamage -= calculatedDefenceValue;
        }

        private void CorrectionValue() {
            CalculatedDamage = (CalculatedDamage < 0f) ? 0f : CalculatedDamage;
        }
        
        private void ClearIfNot() {
            if (!isCleared) {
                Clear();
            }
        }

        public void Clear() {
            KnockBackForce = 0f;
            CalculatedDamage = 0f;
            IsCritical = false;
            isCleared = true;
        }

        public void SetDamageToMax() => CalculatedDamage = MAX_DAMAGE_VALUE;
    }

    [Serializable]
    public class ProjectileDamageData {
        private float baseDamage;
        private float criticalChance;
        private float criticalMultiplier;
        private float penetration;
        private float skillDamage;
        private float skillCoefficient;
        private float instKillChance;
        private float knockBackChance;
        private float knockBackForce;
        private float stunChance;
        private int stunTier;
        private float lifeDrainChance;
        private float lifeDrainRatio;
        private float poisonChance;
        private int poisonTier;
        private float bleedChance;
        private int bleedTier;
        
        public float BaseDamage => baseDamage;
        public float CriticalChance => criticalChance;
        public float CriticalMultiplier => criticalMultiplier;
        public float Penetration => penetration;
        public float SkillDamage => skillDamage;
        public float SkillCoefficient => skillCoefficient;
        public float InstKillChance => instKillChance;
        public float KnockBackChance => knockBackChance;
        public float KnockBackForce => knockBackForce;
        public float StunChance => stunChance;
        public int StunTier => stunTier;
        public float LifeDrainChance => lifeDrainChance;
        public float LifeDrainRatio => lifeDrainRatio;
        public float PoisonChance => poisonChance;
        public int PoisonTier => poisonTier;
        public float BleedChance => bleedChance;
        public int BleedTier => bleedTier;

        public ProjectileDamageData(PlayerStat stat, float skillBaseDamage = 0f, float skillCoefficient = 1f) 
        {
            // Calculate Min/Max Damage
            baseDamage = Random.Range(stat.AttackPower - (stat.AttackPower * stat.DamageDeviation), stat.AttackPower + (stat.AttackPower * stat.DamageDeviation));
            criticalChance = stat.CriticalChance;
            criticalMultiplier = stat.CriticalMultiplier;
            penetration = stat.Penetration;
            skillDamage = skillBaseDamage;
            this.skillCoefficient = skillCoefficient;
            
            instKillChance = stat.InstKillChance;
            knockBackChance = stat.KnockBackChance;
            knockBackForce = stat.KnockBackForce;
            stunChance = stat.StunChance;
            stunTier = stat.StunDefaultTier;
            lifeDrainChance = stat.LifeDrainChance;
            lifeDrainRatio = stat.LifeDrainRatio;
            poisonChance = stat.PoisonChance;
            poisonTier = stat.PoisonDefaultTier;
            bleedChance = stat.BleedChance;
            bleedTier = stat.BleedDefaultTier;
        }
    }
}
