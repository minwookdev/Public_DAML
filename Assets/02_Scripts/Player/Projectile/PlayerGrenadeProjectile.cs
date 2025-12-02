using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CoffeeCat.FrameWork;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace CoffeeCat
{
    public class PlayerGrenadeProjectile : PlayerProjectile
    {
        [SerializeField] private GameObject projectile = null;
        [SerializeField] private GameObject explosion = null;
        private const float height = 8f;
        private const float explosionRange = 1.5f;

        private void OnDisable()
        {
            projectile.SetActive(false);
            explosion.SetActive(false);
        }

        public void Fire(PlayerStat playerStat, PlayerSkill skillData, Vector3 startPos, Vector3 targetPos)
        {
            projectile.SetActive(true);
            SetDamageData(playerStat, skillData.SkillBaseDamage, skillData.SkillCoefficient);
            SetProjectilePath(startPos, targetPos);
        }

        private void SetProjectilePath(Vector2 startPos, Vector2 endPos)
        {
            var controlPos = Vector2.Lerp(startPos, endPos, 0.5f);
            controlPos.y += height;

            var progress = 0f;
            this.UpdateAsObservable()
                .Select(_ => progress += Time.deltaTime / 1f)
                .TakeWhile(_ => progress < 1f)
                .DoOnCompleted(Explosion)
                .Subscribe(_ =>
                {
                    tr.position = Utils.Math2DHelper.CalculateQuadBezierPoint2D(startPos, controlPos, endPos, progress);
                });
        }

        private void Explosion()
        {
            projectile.SetActive(false);
            explosion.SetActive(true);
            PlayHitSound();
            DespawnProjectile();

            var targets = GetTargets();
            if (targets == null) return;
            foreach (var target in targets)
            {
                if (!target.IsAlive) return;

                DamageResult.SetData(projectileDamageData, target.CurrentStat);
                target.OnDamaged(DamageResult);
            }
        }

        private List<MonsterStatus> GetTargets()
        {
            var monsterLayer = 1 << LayerMask.NameToLayer("Monster");
            var monsters = Physics2D.OverlapCircleAll(tr.position, explosionRange, monsterLayer);

            if (monsters.Length <= 0) return null;

            return monsters.Select(mon => mon.GetComponent<MonsterStatus>())
                           .Where(mon => mon != null)
                           .ToList();
        }
        
        private void DespawnProjectile()
        {
            var particleDuration = explosion.GetComponent<ParticleSystem>().main.duration;

            Observable.Timer(TimeSpan.FromSeconds(particleDuration))
                      .TakeUntilDisable(gameObject)
                      .Where(_ => gameObject.activeSelf)
                      .Subscribe(_ => { ObjectPoolManager.Inst.Despawn(gameObject); });
        }
    }
}