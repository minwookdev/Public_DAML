using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using CoffeeCat.FrameWork;

namespace CoffeeCat
{
    [SuppressMessage("ReSharper", "Unity.PerformanceCriticalCodeInvocation")]
    public class PlayerNormalProjectile : PlayerProjectile
    {
        private const float maxDistance = 20f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out MonsterStatus monsterStat))
                return;
            
            DamageResult.SetData(projectileDamageData, monsterStat.CurrentStat);
            monsterStat.OnDamaged(DamageResult, tr.position, true);

            if (gameObject.activeSelf) 
                Despawn();
        }

        private void ProjectilePath(float speed, Vector3 startPos, Vector3 direction)
        {
            tr.DORewind();
            tr.DOMove(direction * maxDistance, speed)
              .SetRelative().SetSpeedBased().SetEase(Ease.Linear).SetDelay(0.05f)
              .From(startPos).SetAutoKill(false)
              .OnComplete(() =>
              {
                  if (gameObject.activeSelf)
                      Despawn();
              });
        }
        private void Despawn()
        {
            ObjectPoolManager.Inst.Despawn(gameObject);
            DamageResult.Clear();
        }

        public void Fire(PlayerStat playerStat, PlayerSkill skillData, Vector3 startPos, Vector3 direction)
        {
            SetDamageData(playerStat, skillData.SkillBaseDamage, skillData.SkillCoefficient);
            ProjectilePath(skillData.ProjectileSpeed, startPos, direction);
        }

        public bool TryGetNextTarget(int attackCount, float skillRange, out MonsterStatus target)
        {
            target = null;
            var monsters = new Collider2D[attackCount];
            var monsterCount = Physics2D.OverlapCircleNonAlloc(transform.position, skillRange, monsters,
                                                               1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0) return false;

            target = monsters
                     .Where(collider2D => collider2D)
                     .Select(collider2D => collider2D.GetComponent<MonsterStatus>())
                     .FirstOrDefault();

            return target != null;
        }
    }
}