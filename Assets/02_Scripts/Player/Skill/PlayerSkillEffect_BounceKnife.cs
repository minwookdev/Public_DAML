using UnityEngine;
using UniRx;
using CoffeeCat.FrameWork;

namespace CoffeeCat
{
    public class PlayerSkillEffect_BounceKnife : PlayerSkillEffect
    {
        public override void SkillEffect()
        {
            var player = RogueLiteManager.Inst.SpawnedPlayer;
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
                                  var skillObj =
                                      ObjectPoolManager.Inst.Spawn(skillData.SkillName, player.ProjectileTr.position);
                                  var projectile = skillObj.GetComponent<PlayerBounceProjectile>();
                                  projectile.SetHitSound(skillData.HitSoundKey);
                                  projectile.Fire(playerStat, skillData, player.ProjectileTr.position, target);
                              }

                              currentCoolTime = 0;
                          });
        }

        public PlayerSkillEffect_BounceKnife(Transform playerTr, PlayerStat playerStat, PlayerSkill skillData) :
            base(playerTr, playerStat, skillData)
        {
        }
    }
}