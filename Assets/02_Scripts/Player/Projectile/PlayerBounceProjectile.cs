using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CoffeeCat;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace CoffeeCat
{
    public class PlayerBounceProjectile : PlayerProjectile
    {
        private Tweener projectileTween = null;
        private int bounceCount = 3;
        private bool isAttackable = true;
        private float skillRange = 0f;

        private void OnEnable()
        {
            isAttackable = true;
            bounceCount = 3;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out MonsterStatus monsterStat) || isAttackable == false)
                return;

            isAttackable = false;
            bounceCount--;
            DamageResult.SetData(projectileDamageData, monsterStat.CurrentStat);
            PlayHitSound();
            monsterStat.OnDamaged(DamageResult, tr.position);

            if (bounceCount == 0)
            {
                Despawn();
                return;
            }

            if (TryGetNextTarget(monsterStat, out var nextTarget))
            {
                isAttackable = true;
                ChangePathEndValue(nextTarget);
            }
            else
            {
                Despawn();
            }
        }

        public void Fire(PlayerStat playerStat, PlayerSkill skillData, Vector3 startPos, MonsterStatus target)
        {
            skillRange = skillData.SkillRange;
            SetDamageData(playerStat, skillData.SkillBaseDamage, skillData.SkillCoefficient);
            SetPath(target.GetCenterTr().position, startPos, skillData.ProjectileSpeed);
        }

        private void SetPath(Vector3 targetPos, Vector3 startPos, float projectileSpeed)
        {
            var targetDir = targetPos - startPos;
            targetDir = targetDir.normalized;

            tr.DORewind();
            projectileTween = tr.DOMove(targetDir * 20f, projectileSpeed)
                                .SetRelative().SetSpeedBased().SetEase(Ease.OutSine).From(startPos).SetDelay(0.05f)
                                .SetAutoKill(false)
                                .OnComplete(Despawn);
        }

        private void ChangePathEndValue(MonsterStatus nextTarget)
        {
            var targetDir = nextTarget.GetCenterTr().position - tr.position;
            targetDir = targetDir.normalized;
            var endValue = tr.position + targetDir * 20f;
            projectileTween.ChangeEndValue(endValue, true).Restart();
        }

        private bool TryGetNextTarget(MonsterStatus prevTarget, out MonsterStatus nextTarget)
        {
            var monsters = new Collider2D[5];
            var monsterCount = Physics2D.OverlapCircleNonAlloc(tr.position, skillRange, monsters,
                                                               1 << LayerMask.NameToLayer("Monster"));

            if (monsterCount <= 0)
            {
                nextTarget = null;
                return false;
            }

            nextTarget = monsters
                         .Where(col => col != null)
                         .Select(col => col.GetComponent<MonsterStatus>())
                         .FirstOrDefault(mon => mon != prevTarget);

            return nextTarget != null;
        }

        private void Despawn()
        {
            if (!gameObject.activeSelf) return;

            ObjectPoolManager.Inst.Despawn(gameObject);
            DamageResult.Clear();
        }
    }
}