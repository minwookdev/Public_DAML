using UnityEngine;
using DG.Tweening;
using CoffeeCat.FrameWork;

namespace CoffeeCat {
    public class HealthBar : MonoBehaviour {
        [SerializeField] private Transform tr = null;
        [SerializeField] private Transform barTr = null;
        private Vector3 originTrLocalScale = Vector3.zero;
        private float offsetY = 0f;
        private Transform targetTr = null;

        public void Init(Transform target, float offsetYValue) {
            targetTr = target;
            offsetY = offsetYValue;
            Active();
        }

        private void Active() {
            if (originTrLocalScale == default) {
                originTrLocalScale = tr.localScale;
            }
            
            tr.localScale = originTrLocalScale;
            SetValue(1f);
        }
        
        private void Update() {
            if (!targetTr) {
                return;
            }

            var targetPosition = targetTr.position;
            targetPosition.y += offsetY;
            tr.position = targetPosition;
        }
        
        public void SetValue(float current, float max) {
            SetValue(current / max);
        }

        private void SetValue(float ratio) {
            var value = Mathf.Clamp01(ratio);
            var scaleVec3 = barTr.localScale;
            scaleVec3.x = value;
            barTr.localScale = scaleVec3;
        }

        public void Despawn() {
            SetValue(0f);
            targetTr = null;
            offsetY = 0f;
            
            tr.DOScaleY(0f, 0.35f)
              .OnComplete(() => {
                  ObjectPoolManager.Inst.Despawn(gameObject);
              });
        }
    }
}