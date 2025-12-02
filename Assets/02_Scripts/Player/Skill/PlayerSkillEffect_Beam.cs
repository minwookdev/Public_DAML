using System;
using System.Diagnostics.CodeAnalysis;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Beam : PlayerSkillEffect
    {
        public override void SkillEffect()
        {
            var currentCoolTime = skillData.SkillCoolTime;
            
            updateDisposable =
                Observable.EveryUpdate()
                          .Select(_ => currentCoolTime += Time.deltaTime)
                          .Where(_ => currentCoolTime >= playerStat.CalculateCoolTime(skillData.SkillCoolTime))
                          .Where(_ => completedLoadResource)
                          .Subscribe(_ =>
                          {
                              var skillRange = skillData.SkillRange + playerStat.SearchRange;
                              var attackCount = skillData.AttackCount + playerStat.AddProjectile;

                              var targets = FindAroundMonsters(attackCount, skillRange);
                              if (targets == null) return;

                              foreach (var target in targets)
                              {
                                  if (!target.IsAlive) continue;

                                  var skillObj = ObjectPoolManager.Inst.Spawn(skillData.SkillName, target.GetCenterTr().position);
                                  var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                                  projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage, skillData.SkillCoefficient, true);
                              }
                              currentCoolTime = 0;
                          });
        }
        
        public PlayerSkillEffect_Beam(Transform playerTr, PlayerStat playerStat, PlayerSkill skillData) : base(playerTr, playerStat, skillData) { }
    }
}