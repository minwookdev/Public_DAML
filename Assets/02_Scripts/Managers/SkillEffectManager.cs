using System;
using UnityEngine;

namespace CoffeeCat.FrameWork
{
    public class SkillEffectManager : DynamicSingleton<SkillEffectManager>
    {
        #region ActiveSkillEffect

        public PlayerSkillEffect InstantiateActiveSkillEffect(Transform playerTr, PlayerStat enhancedStat, PlayerSkill skillData)
        {
            PlayerSkillEffect skillEffect = skillData.GroupIndex switch 
            {
                1000 => NormalAttack(playerTr, enhancedStat, skillData),
                1001 => Explosion(playerTr, enhancedStat, skillData),
                1002 => Beam(playerTr, enhancedStat, skillData),
                1003 => Bubble(playerTr, enhancedStat, skillData),
                1004 => Grenade(playerTr, enhancedStat, skillData),
                1005 => BounceKnife(playerTr, enhancedStat, skillData),
                _    => throw new NotImplementedException("Invalid GroupIndex : " + skillData.GroupIndex)
            };

            return skillEffect;
        }

        private PlayerSkillEffect NormalAttack(Transform playerTr, PlayerStat enhancedStat, PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_NormalAttack(playerTr, enhancedStat, skillData);
            return skillEffect;
        }

        private PlayerSkillEffect Explosion(Transform playerTr, PlayerStat enhancedStat, PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Explosion(playerTr, enhancedStat, skillData);
            return skillEffect;
        }

        private PlayerSkillEffect Beam(Transform playerTr, PlayerStat enhancedStat, PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Beam(playerTr, enhancedStat, skillData);
            return skillEffect;
        }

        private PlayerSkillEffect Bubble(Transform playerTr, PlayerStat enhancedStat, PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Bubble(playerTr, enhancedStat, skillData);
            return skillEffect;
        }

        private PlayerSkillEffect Grenade(Transform playerTr, PlayerStat enhancedStat, PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_Grenade(playerTr, enhancedStat, skillData);
            return skillEffect;
        }

        private PlayerSkillEffect BounceKnife(Transform playerTr, PlayerStat enhancedStat, PlayerSkill skillData)
        {
            var skillEffect = new PlayerSkillEffect_BounceKnife(playerTr, enhancedStat, skillData);
            return skillEffect;
        }

        #endregion

        #region PassiveSkillEffect

        public void PassiveSkillEffect(PlayerSkill skillData)
        {
            switch (skillData.GroupIndex)
            {
                case 2000: Damage(skillData); break;
                case 2001: MoveSpeed(skillData); break;
                case 2002: Casting(skillData); break;
                case 2003: Magnetic(skillData); break;
                case 2004: Block(skillData); break;
                case 2005: SearchRange(skillData); break;
                case 2006: ProjectileCount(skillData); break;
                case 2007: KnockBack(skillData); break;
                case 2008: InstKill(skillData); break;
                case 2009: Poison(skillData); break;
                case 2010: Bleed(skillData); break;
                case 2011: LifeDrain(skillData); break;
                case 2012: Stun(skillData); break;
                default:   throw new NotImplementedException("Invalid GroupIndex : " + skillData.GroupIndex);
            }
        }

        private void Damage(PlayerSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.AttackPower = skillData.SkillBaseDamage;
            player.CalculateEnhancedStat();
        }

        private void MoveSpeed(PlayerSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.MoveSpeed = skillData.SkillBaseDamage;
            player.CalculateEnhancedStat();
        }

        private void Casting(PlayerSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.CoolTimeReduce = skillData.SkillBaseDamage;
            player.CalculateEnhancedStat();
        }

        private void Magnetic(PlayerSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.ItemAcquisitionRange = skillData.SkillBaseDamage;
            player.CalculateEnhancedStat();
        }

        private void Block(PlayerSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.Defense = skillData.SkillBaseDamage;
            player.CalculateEnhancedStat();
        }

        private void SearchRange(PlayerSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.SearchRange = skillData.SkillBaseDamage;
            player.CalculateEnhancedStat();
        }

        private void ProjectileCount(PlayerSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.AddProjectile = (int)skillData.SkillBaseDamage;
            player.CalculateEnhancedStat();
        }

        private void KnockBack(PlayerSkill skillData)
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.KnockBackChance = skillData.SkillBaseDamage;
            player.BuffedStat.KnockBackForce = skillData.AttackCount;
            player.CalculateEnhancedStat();
        }

        private void InstKill(PlayerSkill skillData) {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.InstKillChance = skillData.SkillBaseDamage;
            player.CalculateEnhancedStat();
        }

        private void Poison(PlayerSkill skillData) {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.PoisonChance = skillData.SkillBaseDamage;
            player.BuffedStat.PoisonDefaultTier = skillData.AttackCount;
            player.CalculateEnhancedStat();
        }

        private void Bleed(PlayerSkill skillData) {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.BleedChance = skillData.SkillBaseDamage;
            player.BuffedStat.BleedDefaultTier = skillData.AttackCount;
            player.CalculateEnhancedStat();
        }

        private void LifeDrain(PlayerSkill skillData) {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.LifeDrainChance = skillData.SkillBaseDamage;
            player.BuffedStat.LifeDrainRatio = skillData.SkillCoefficient;
            player.CalculateEnhancedStat();
        }

        private void Stun(PlayerSkill skillData) {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
            player.BuffedStat.StunChance = skillData.SkillBaseDamage;
            player.BuffedStat.StunDefaultTier = skillData.AttackCount;
            player.CalculateEnhancedStat();
        }

        #endregion

        #region Equipment Skill Effect

        public void EquipmentSkillEffect(PlayerStat equipStat, PlayerSkill skillData)
        {
            var skillName = skillData.SkillName;
            switch (skillName)
            {
                case "Iron Skin":
                    IronSkin(equipStat, skillData);
                    break;
                case "Concentration":
                    Concentration(equipStat, skillData);
                    break;
                default:
                    throw new NotImplementedException(skillName);
            }
        }
        
        private void IronSkin(PlayerStat equipStat, PlayerSkill skillData)
        {
            equipStat.Defense += skillData.SkillBaseDamage;
        }

        private void Concentration(PlayerStat equipStat, PlayerSkill skillData)
        {
            equipStat.AttackPower += skillData.SkillBaseDamage;
        }
        
        #endregion
    }
}