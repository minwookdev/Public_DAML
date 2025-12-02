using UnityEngine;
using CoffeeCat.Datas;
using CoffeeCat.FrameWork;

namespace CoffeeCat
{
    public class PlayerProjectile : MonoBehaviour
    {
        [SerializeField] protected float knockBackForce = 0f; 
        protected Transform tr = null;
        protected ProjectileDamageData projectileDamageData = null;
        protected readonly DamageResult DamageResult = new();
        protected string hitSoundKey = string.Empty;

        private void Awake()
        {
            tr = GetComponent<Transform>();
        }
        
        protected virtual void SetDamageData(PlayerStat playerStat, float skillBaseDamage = 0f, float skillCoefficient = 1f)
        {
            projectileDamageData = new ProjectileDamageData(playerStat, skillBaseDamage, skillCoefficient);
        }

        public void SetHitSound(string soundKeyStr) {
            hitSoundKey = soundKeyStr;
        }

        protected void PlayHitSound() {
            if (hitSoundKey != string.Empty) {
                SoundManager.Inst.PlaySE(hitSoundKey);
            }
        }
    }
}