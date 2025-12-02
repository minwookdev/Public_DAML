using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_Bubble : PlayerSkillEffect
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
                              var targets = FindAllMonsters();
                              if (targets == null) return;

                              PlayRandomSound();
                              DisplayDamageRange();
                              var skillObj = ObjectPoolManager.Inst.Spawn(skillData.SkillName, playerTr.position);
                              var projectile = skillObj.GetComponent<PlayerSkillProjectile>();
                              projectile.AreaAttack(playerStat, targets, skillData.SkillBaseDamage, skillData.SkillCoefficient);
                              currentCoolTime = 0;
                          });
        }

        public PlayerSkillEffect_Bubble(Transform playerTr, PlayerStat playerStat, PlayerSkill skillData) : base(playerTr, playerStat, skillData)
        {
        }
    }
}