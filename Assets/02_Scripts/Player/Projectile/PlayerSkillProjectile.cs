using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using CoffeeCat.FrameWork;

namespace CoffeeCat
{
    public class PlayerSkillProjectile : PlayerProjectile
    {
        private void OnEnable()
        {
            DespawnProjectile();
        }

        private void OnDisable() 
        {
            DamageResult.Clear();
        }

        private void UpdatePosition(Transform monsterCenterTr)
        {
            this.UpdateAsObservable()
                .TakeUntilDisable(this)
                .Subscribe(_ => { tr.position = monsterCenterTr.position; });
        }

        private void DespawnProjectile()
        {
            var particleDuration = GetComponent<ParticleSystem>().main.duration;

            Observable.Timer(TimeSpan.FromSeconds(particleDuration))
                      .TakeUntilDisable(gameObject)
                      .Where(_ => gameObject.activeSelf)
                      .Subscribe(_ => { ObjectPoolManager.Inst.Despawn(gameObject); });
        }

        #region public methods

        public void AreaAttack(PlayerStat playerStat, List<MonsterStatus> monsters, float skillBaseDamage = 0f, float skillCoefficient = 1f)
        {
            SetDamageData(playerStat, skillBaseDamage, skillCoefficient);

            foreach (var monster in monsters)
            {
                if (!monster.IsAlive) continue;

                DamageResult.SetData(projectileDamageData, monster.CurrentStat);
                monster.OnDamaged(DamageResult);
            }
        }

        public void SingleTargetAttack(PlayerStat playerStat, MonsterStatus monster, float skillBaseDamage = 0f, float skillCoefficient = 1f, bool isPlayMonsterHitSound = false)
        {
            SetDamageData(playerStat, skillBaseDamage, skillCoefficient);
            UpdatePosition(monster.GetCenterTr());

            DamageResult.SetData(projectileDamageData, monster.CurrentStat);
            monster.OnDamaged(DamageResult, playHitSound: isPlayMonsterHitSound);
        }

        #endregion
    }
}