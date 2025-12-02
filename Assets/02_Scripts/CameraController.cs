using System;
using UnityEngine;
using Sirenix.OdinInspector;
using CoffeeCat.Utils.Defines;

namespace CoffeeCat {
    public class CameraController : MonoBehaviour {
        [Title("Camera")]
        [SerializeField] private Transform thisTr = null;
        [SerializeField] private Transform targetTr = null;
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private CameraMoveType currentMoveType = CameraMoveType.Smooth;
        [SerializeField] private CameraUpdateType currentCameraUpdateType = CameraUpdateType.LateUpdate;
        private readonly Vector3 offset = new(0f, 0f, 0f);

        private void Update() {
            if (currentCameraUpdateType != CameraUpdateType.Update) {
                return;
            }
            UpdateCameraPosition();
        }

        private void FixedUpdate() {
            if (currentCameraUpdateType != CameraUpdateType.FixedUpdate) {
                return;
            }
            UpdateCameraPosition();
        }

        private void LateUpdate() {
            if (currentCameraUpdateType != CameraUpdateType.LateUpdate) {
                return;
            }
            UpdateCameraPosition();
        }

        private void UpdateCameraPosition() {
            if (!targetTr) {
                return;
            }
            switch (currentMoveType) {
                case CameraMoveType.Normal: NormalTypeUpdate(); break;
                case CameraMoveType.Smooth: SmoothTypeUpdate(); break;
                default:                      throw new ArgumentOutOfRangeException();
            }
        }

        public void SetUpdateType(CameraUpdateType updateType, CameraMoveType moveType) {
            currentCameraUpdateType = updateType;
            currentMoveType = moveType;
        }

        public void SetTarget(Transform target) {
            if (!target) {
                CatLog.ELog("Invalid Target Transform.");
                return;
            }

            // set position forced
            targetTr = target;
            var initialPosition = targetTr.position;
            initialPosition.z = thisTr.position.z;
            thisTr.position = initialPosition;
        }

        private void SmoothTypeUpdate() {
            Vector3 desiredPosition = new Vector3(targetTr.position.x, targetTr.position.y, thisTr.position.z) + offset;
            Vector3 smoothedPosition = Vector3.Lerp(thisTr.position, desiredPosition, smoothSpeed);
            thisTr.position = smoothedPosition;
            
            // Vector3 velocity = Vector3.zero;
            // Vector3 desiredPosition = new Vector3(targetTr.position.x, targetTr.position.y, thisTr.position.z) + offset;
            // Vector3 smoothedPosition = Vector3.SmoothDamp(thisTr.position, desiredPosition, ref velocity, smoothSpeed);
            // thisTr.position = smoothedPosition;
        }

        private void NormalTypeUpdate() {
            thisTr.position = new Vector3(targetTr.position.x, targetTr.position.y, thisTr.position.z);
        }
    }
}