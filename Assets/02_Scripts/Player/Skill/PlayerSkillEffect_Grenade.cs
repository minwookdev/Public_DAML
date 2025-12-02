using UnityEngine;
using UniRx;
using CoffeeCat.FrameWork;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Grenade : PlayerSkillEffect
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

                              PlayCastingSound();
                              foreach (var target in targets)
                              {
                                  var skillObj = ObjectPoolManager.Inst.Spawn(skillData.SkillName, player.ProjectileTr.position);
                                  var projectile = skillObj.GetComponent<PlayerGrenadeProjectile>();
                                  projectile.Fire(player.EnhancedStat, skillData, player.Tr.position, target.GetCenterTr().position);
                              }

                              currentCoolTime = 0;
                          });
        }

        public PlayerSkillEffect_Grenade(Transform playerTr, PlayerStat playerStat, PlayerSkill skillData) : base(playerTr, playerStat, skillData) { }
    }
}