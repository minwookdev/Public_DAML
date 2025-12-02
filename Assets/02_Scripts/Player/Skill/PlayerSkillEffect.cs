using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using CoffeeCat.FrameWork;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat
{
    public class PlayerSkillEffect
    {
        protected Transform playerTr = null;
        protected PlayerStat playerStat = null;
        protected PlayerSkill skillData = null;
        public PlayerSkill SkillData { set => skillData = value; }

        protected IDisposable updateDisposable = null;
        protected bool completedLoadResource = false;
        
        // 새로운 스킬 선택
        protected PlayerSkillEffect() { }
        protected PlayerSkillEffect(Transform playerTr, PlayerStat playerStat, PlayerSkill skillData)
        {
            this.playerTr = playerTr;
            this.playerStat = playerStat;
            this.skillData = skillData;

            SafeLoader.Regist(skillData.SkillName, onCompleted: completed =>
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

        // 스킬 효과 활성화
        public virtual void SkillEffect() { }

        // 스킬 효과 비활성화
        public void OnDispose() => updateDisposable?.Dispose();
        
        protected void PlayRandomSound()
        {
            var randomValue = UnityEngine.Random.Range(0, 2);
            var randomSoundKey = randomValue == 0 ? skillData.CastingSoundKey : skillData.HitSoundKey;
            SoundManager.Inst.PlaySE(randomSoundKey);
        }
        
        protected void PlayCastingSound() => SoundManager.Inst.PlaySE(skillData.CastingSoundKey);
        
        #region FindMonster

        protected List<MonsterStatus> FindAllMonsters()
        {
            var monsters = Physics2D.OverlapBoxAll(playerTr.position,
                                                   new Vector2(Defines.PLAYER_AREA_SKILL_VECTOR_X,
                                                               Defines.PLAYER_AREA_SKILL_VECTOR_Y), 0f,
                                                   1 << LayerMask.NameToLayer("Monster"));

            if (monsters.Length <= 0) return null;

            return monsters.Select(mon => mon.GetComponent<MonsterStatus>())
                           .ToList();
        }

        protected List<MonsterStatus> FindAroundMonsters(int attackCount, float skillRange)
        {
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc
                (playerTr.position, skillRange, monsters, 1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0)
                return null;

            var targets = monsters
                          .Where(collider2D => collider2D)
                          .Select(collider2D => collider2D.GetComponent<MonsterStatus>())
                          .ToList();

            return targets;
        }

        protected MonsterStatus FindAroundMonster(int attackCount, float skillRange)
        {
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc(playerTr.position, skillRange, monsters,
                                                               1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0) return null;

            return monsters
                   .Where(collider2D => collider2D)
                   .Select(collider2D => collider2D.GetComponent<MonsterStatus>())
                   .FirstOrDefault();
        }
        
        protected Transform FindNearestMonster(int attackCount, float skillRange, Transform baseTr)
        {
            Collider2D[] result = new Collider2D[Defines.SPAWN_MONSTER_MAX_COUNT];
            var count = Physics2D.OverlapCircleNonAlloc
                (playerTr.position, skillRange, result, 1 << LayerMask.NameToLayer("Monster"));

            if (count <= 0) return null;

            var target = result.Where(Collider2D => Collider2D != null)
                               .Select(Collider2D => Collider2D.GetComponent<MonsterStatus>())
                               .Where(monster => monster.IsAlive)
                               .OrderBy(monster => Vector2.Distance(baseTr.position,
                                                                    monster.GetCenterTr().position))
                               .FirstOrDefault();

            return target == null ? null : target.transform;
        }

        protected void DisplayDamageRange()
        {
            var damageRangeObj = ObjectPoolManager.Inst.Spawn("DamageRange", playerTr.position);
            damageRangeObj.transform.localScale = new Vector3(Defines.PLAYER_AREA_SKILL_VECTOR_X,
                                                              Defines.PLAYER_AREA_SKILL_VECTOR_Y, 1f);
            
            var sprite = damageRangeObj.GetComponent<SpriteRenderer>();
            sprite.DORewind();
            sprite.DOFade(0.15f, 0.5f).SetEase(Ease.Linear)
                  .OnComplete(() =>
                  {
                      sprite.DOFade(0f, 0.5f).SetEase(Ease.Linear);
                      ObjectPoolManager.Inst.Despawn(damageRangeObj, 0.25f);
                  });
        }

        #endregion
    }
}