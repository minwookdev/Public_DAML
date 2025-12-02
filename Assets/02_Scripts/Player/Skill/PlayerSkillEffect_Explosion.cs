using UnityEngine;
using UniRx;
using CoffeeCat.FrameWork;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Explosion : PlayerSkillEffect
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
                              if (targets == null) 
                                  return;

                              PlayRandomSound();
                              foreach (var target in targets)
                              {
                                  if (!target.IsAlive) 
                                      continue;

                                  var skillObj = ObjectPoolManager.Inst.Spawn(skillData.SkillName, target.GetCenterTr().position);
                                  var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                                  projectile.SingleTargetAttack(playerStat, target, skillData.SkillBaseDamage, skillData.SkillCoefficient);
                              }
                              currentCoolTime = 0;
                          });
        }
        
        public PlayerSkillEffect_Explosion(Transform playerTr, PlayerStat playerStat, PlayerSkill skillData) : base(playerTr, playerStat, skillData) { }
    }
}