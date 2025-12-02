using System;
using System.Collections;
using System.Collections.Generic;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils;
using UniRx;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerSkillEffect_NormalAttack : PlayerSkillEffect
    {
        private readonly string skillName;
        
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
                              if (targets == null) 
                                  return;

                              // play random casting sound
                              PlayRandomSound();
                              
                              foreach (var target in targets)
                              {
                                  var targetDirection = target.GetCenterTr().position - player.ProjectileTr.position;
                                  targetDirection = targetDirection.normalized;

                                  var skillObj = ObjectPoolManager.Inst.Spawn(skillName, player.ProjectileTr.position);
                                  var projectile = skillObj.GetComponent<PlayerNormalProjectile>();
                                  projectile.Fire(playerStat, skillData, player.ProjectileTr.position, targetDirection);
                              }

                              currentCoolTime = 0;
                              player.StartAttack();
                          });
        }

        public PlayerSkillEffect_NormalAttack(Transform playerTr, PlayerStat playerStat, PlayerSkill skillData)
        {
            this.playerTr = playerTr;
            this.playerStat = playerStat;
            this.skillData = skillData;

            var player = RogueLiteManager.Inst.SpawnedPlayer;
            skillName = player.NormalAttackKey.ToKey();
            
            SafeLoader.Regist(skillName, onCompleted: completed =>
            {
                completedLoadResource = completed;
                if (!completed)
                    CatLog.WLog("PlayerSkillEffect : Load Resource Failed");
            });
            
            DungeonEvtManager.AddEventOnPlayerKilled(OnDispose);
            DungeonEvtManager.AddEventClearedRoomEvent(roomData =>
            {
                if (roomData.RoomType == RoomType.MonsterSpawnRoom) OnDispose();
            });
            
            SoundManager.Inst.RegistAudioClip(skillData.CastingSoundKey);
            SoundManager.Inst.RegistAudioClip(skillData.HitSoundKey);
        }
    }
}