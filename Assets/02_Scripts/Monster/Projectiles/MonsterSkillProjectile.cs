using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.FrameWork;
using CoffeeCat.Datas;
using CoffeeCat.Utils;
using CoffeeCat.Utils.Defines;

// NOTE: 각종 이벤트(충돌, 데미지, 시간경과)에 따라 추가적인 투사체를 생성하는 AfterTask 구현
namespace CoffeeCat {
    public class MonsterSkillProjectile : MonsterProjectile {
        [Title("Skill Projectile Datas")]
        [SerializeField] protected ProjectileSkillKey skillKey = ProjectileSkillKey.NONE;
        [SerializeField, ReadOnly] protected MonsterSkillStat skillData = null;

        [Title("Spawn Additional Projectile")]
        [SerializeField] protected AfterTask[] tasks = null;

        private readonly DamageResult damageResult = new();

        protected override void OnDisable() {
            base.OnDisable();
        }

        #region ADDITIONAL_PROJECTILE_SPANW

        [System.Serializable]
        public class AfterTask {
            public enum ActiveCondition {
                NONE,
                INITIALIZED,
                TIMER,
                ACTIVE,
                COLLISION,
                ATTACKPLAYER
            }

            public ActiveCondition Cond { get; private set; } = ActiveCondition.NONE;
            
            // 추가적인 투사체 생성 키
            //[SerializeField] private ProjectileSkillKey Key = ProjectileSkillKey.NONE;

            public void Active() {
                // Active..
            }
        }

        private void ActiveAfterTask(AfterTask.ActiveCondition condition) {
            for (int i = 0; i < tasks.Length; i++) {
                if (tasks[i].Cond.Equals(condition)) {
                    tasks[i].Active();
                }
            }
        }

        #endregion

        protected override void SetBaseComponents() {
            base.SetBaseComponents();

            // Get SkillData in DataManager's Dictionary
            if (skillData != null) 
                return;
            if (DataManager.Inst.MonsterSkills.DataDictionary.TryGetValue(skillKey.ToString(), out MonsterSkillStat result) == false) {
                CatLog.ELog($"Not Found Monster Skill Data. Name: {skillKey.ToString()}");
            }
            skillData = result;
        }

        public override void Initialize(MonsterStat monsterStatData) {
            base.Initialize(monsterStatData);
        }

        #region COLLISION_BASE

        protected override void OnCollisionEnterWithPlayer(Collision2D playerCollision) {
            base.OnCollisionEnterWithPlayer(playerCollision);
        }

        protected override void OnCollisionEnterWithTargetLayer(Collision2D collision) {
            base.OnCollisionEnterWithTargetLayer(collision);
        }

        protected override void OnTriggerEnterWithPlayer(Collider2D playerCollider) {
            base.OnTriggerEnterWithPlayer(playerCollider);
        }

        protected override void OnTriggerEnterWithTargetLayer(Collider2D collider) {
            base.OnTriggerEnterWithTargetLayer(collider);
        }

        #endregion

        #region DAMAGED

        protected override void DamageToPlayer(Player_Dungeon player, Vector2 collisionPoint, Vector2 collisionDirection) {
            if (statData == null || skillData == null) {
                CatLog.ELog("Monster Projectiles Stat or Skill Data is Null.");
                return;
            }
            
            damageResult.SetData(statData, player.EnhancedStat);
            player.OnDamaged(damageResult);
            ObjectPoolManager.Inst.Despawn(gameObject);
        }

        #endregion
    }
}
