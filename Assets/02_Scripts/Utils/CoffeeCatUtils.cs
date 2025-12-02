using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace CoffeeCat.Utils {
    public static class Utility {
        public static LayerMask GetPlayerLayer() => LayerMask.NameToLayer(Defines.Defines.LAYER_PLAYER);

        public static bool IsPlayerLayer(Collider2D collider2D) => collider2D.gameObject.layer == GetPlayerLayer();

        public static bool IsItemAquisitionLayer(Collider2D collider) => collider.gameObject.layer == LayerMask.NameToLayer(Defines.Defines.LAYER_ITEM_AQUISITION_COLLIDER);

        public static bool IsLayerInMask(int layer, LayerMask layerMask) {
            return ((layerMask & (1 << layer)) != 0);
            // return (((1 << layer) & layerMask) != 0)
        }

        /// <summary>
        /// LayerMask에 단일 레이어만 등록되어있을 경우만 작동
        /// </summary>
        /// <param name="layerMask"></param>
        /// <returns></returns>
        public static int GetLayerNumberInMask(LayerMask layerMask) {
            return (int)Mathf.Log(layerMask.value, 2);
        }

        public static bool IsValidRangeUShort(int value) {
            return value >= ushort.MinValue && value <= ushort.MaxValue;
        }

#if UNITY_EDITOR
        public static Vector2 GetGameViewScreenSizeInEditor() {
            var gameViewScreenSize = UnityEditor.Handles.GetMainGameViewSize();
            return new Vector2(gameViewScreenSize.x, gameViewScreenSize.y);

            // 1. another way to get resolution
            // Screen.currentResolution

            // 2. another way to get resolution
            // string[] res = UnityStats.screenRes.Split('x');
            // Debug.Log(int.Parse(res[0]) + " " + int.Parse(res[1]));
        }
#endif
    }

    /// <summary>
    /// 2D 좌표계 관련 기능
    /// </summary>
    public static class Math2DHelper {
        public static bool IsFront(Transform tr1, Transform tr2) {
            return false;
            // 벡터의 내적 사용해서 특정 오브젝트가 앞에있는지 뒤에있는지 판별하는 함수
        }

        public static bool IsLeft(Transform tr1, Transform tr2) {
            return false;
            // 벡터의 외적을 활용해서 특정 오브젝트가 왼쪽에있는지 오른쪽에있는지 판별하는 함수
        }

        public static Vector2 GetNormalizedDirection(Vector2 from, Vector2 to) {
            return (to - from).normalized; // direction = destination - source
        }

        public static Vector2 GetDirection(Vector2 from, Vector2 to) => to - from;

        /// <summary>
        /// Quaternion으로 반환
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="offsetDegreeValue"></param>
        /// <returns></returns>
        public static Quaternion GetRotationByDirection(Vector2 direction, float offsetDegreeValue = 0f) {
            // Quaternion.Euler(new Vector3(0f, 0f, GetAngleToDirection(direction, correctionDegreeValue))); // Way1
            return Quaternion.AngleAxis(GetAngleByDirection(direction, offsetDegreeValue), Vector3.forward); // Way2
        }

        /// <summary>
        /// Degree로 반환
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="offsetDegreeValue"></param>
        /// <returns></returns>
        public static float GetAngleByDirection(Vector2 direction, float offsetDegreeValue = 0f) {
            return (float)(Math.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + offsetDegreeValue;
        }

        /// <summary>
        /// 사용금지 (GetPointByDirection을 사용합니다)
        /// </summary>
        /// <param name="originVec2"></param>
        /// <param name="directionVec2"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector2 GetOffsetByDirection(Vector2 originVec2, Vector2 directionVec2, float distance = 1f) {
            //return (originVec2 + directionVec2) * distance;
            return Vector2.zero;
        }

        /// <summary>
        /// 사용금지 (GetPointByDirection을 사용합니다)
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="direction"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static Vector2 GetInverseOffsetByDirection(Vector2 startPoint, Vector2 direction, float distance = 1f) {
            //return (startPoint + GetInverseVector(direction)) * distance;
            return Vector2.zero;
        }

        public static Vector2 GetPointByDirection(Vector2 origin, Vector2 direction, float distance = 1f) {
            return origin + (direction * distance);
        }

        public static Vector2 GetInversePointByDirection(Vector2 originVec2, Vector2 directionVec2, float distance = 1f) {
            return originVec2 + (GetInverseVector(directionVec2) * distance);
        }

        public static Vector2 GetInverseVector(Vector2 originVec2) {
            return originVec2 * -1;
        }

        /// <summary>
        /// Rect 범위내 존재하는지 반환
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public static bool IsInside(this Rect rect, Vector2 position) {
            return (rect.xMin <= position.x && rect.xMax >= position.x &&
                    rect.yMin <= position.y && rect.yMax >= position.y);
        }

        public static bool IsInSide(this RectInt rectInt, Vector2 position) {
            return (rectInt.xMin <= position.x && rectInt.xMax >= position.x &&
                    rectInt.yMin <= position.y && rectInt.yMax >= position.y);
        }

        public static bool GetDirectionIsRight(Vector2 normalizedDirection) => normalizedDirection.x >= 0f;

        /// <summary>
        /// 2차 베지어 곡선 계산
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="controlPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector2 CalculateQuadBezierPoint2D(Vector2 startPoint, Vector2 controlPoint, Vector2 endPoint, float t) {
            float u = 1 - t;
            return (u * u * startPoint) + (2 * u * t * controlPoint) + (t * t * endPoint);
        }

        /// <summary>
        /// 3차 베지어 곡선 계산
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="controlPoint1"></param>
        /// <param name="controlPoint2"></param>
        /// <param name="endPoint"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static Vector3 CalculateCubicBezierPoint2D(Vector2 startPoint, Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint, float t) {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            return (uuu * startPoint) + (3 * uu * t * controlPoint1) + (3 * u * tt * controlPoint2) + (ttt * endPoint);
        }
    }

    public static class MathHelper {
        public static bool IsTargetInFront(Transform originTr, Transform targetTr, Vector2 direction) {
            Vector3 distance = targetTr.position - originTr.position;
            float theta = Vector3.Angle(direction, distance);
            CatLog.Log($"Theta Value: {theta}");
            return theta < 90f;
        }

        public static bool TargetInSight(Transform originTr, Transform targetTr, float sightDegree) {
            return false;
        }

        public static bool IsDivisible(float denominator, float numerator) => denominator % numerator == 0f;

        public static bool IsDecimalPoint(float value) => value % 1f != 0f;
    }

    /// <summary>
    /// UI좌표 관련 기능 (class명 수정)
    /// </summary>
    public static class UIHelper {
        /// <summary>
        /// World좌표에서 Canvas공간 RectTransform좌표로 변환
        /// </summary>
        /// <param name="worldPosition">Target World Position</param>
        /// <param name="rectTransform"></param>
        /// <param name="targetCamera">World Position을 비추고 있는 Camera</param>
        /// <param name="uiCanvasCamera">RectTransform의 Canvas와 연결된 카메라 (없다면 비워둠)</param>
        /// <returns></returns>
        public static Vector2 ConvertWorldToCanvasPosition(Vector3 worldPosition, RectTransform rectTransform, Camera targetCamera, Camera uiCanvasCamera = null) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, targetCamera.WorldToScreenPoint(worldPosition), uiCanvasCamera, out Vector2 canvasPosition);
            return canvasPosition;
        }

        public static Vector2 WorldPositionToCanvasAnchoredPosition(Camera mainCamera, Vector3 WorldPosition, RectTransform renderTargetRectTr) {
            Vector2 viewPortPosition = mainCamera.WorldToViewportPoint(WorldPosition);
            Vector2 canvasAnchoredPosition = new Vector2(
                                                         ((viewPortPosition.x * renderTargetRectTr.sizeDelta.x) -
                                                          (renderTargetRectTr.sizeDelta.x * 0.5f)),
                                                         ((viewPortPosition.y * renderTargetRectTr.sizeDelta.y) -
                                                          (renderTargetRectTr.sizeDelta.y * 0.5f)));
            return canvasAnchoredPosition;
        }

        /// <summary>
        /// it's works only [Screen Space - Camera] Mode Canvases. return specified RectTransform's anchoredPosition
        /// </summary>
        /// <param name="mainCam"></param>
        /// <param name="uiCamera"></param>
        /// <param name="worldPos"></param>
        /// <param name="uiCanvasRectTr"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool ConvertWorldToCameraSpaceCanvasPosition(Camera mainCam, Camera uiCamera, Vector2 worldPos, RectTransform uiCanvasRectTr, out Vector2 result) {
            Vector2 screenPosition = mainCam.WorldToScreenPoint(worldPos);
            Vector3 uiCamViewPortPoint = uiCamera.ScreenToViewportPoint(screenPosition);
            Vector2 uiCamScreenPoint = new Vector2(uiCamViewPortPoint.x * Screen.width, uiCamViewPortPoint.y * Screen.height);
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(uiCanvasRectTr, uiCamScreenPoint, uiCamera, out result);
        }
        
        public static void SetAnchorAndPivot(this RectTransform rectTr, Vector2 anchorMinVec2, Vector2 anchorMaxVec2,
                                             Vector2 pivotVec2) {
            rectTr.anchorMin = anchorMinVec2;
            rectTr.anchorMax = anchorMaxVec2;
            rectTr.pivot = pivotVec2;
        }

        public static void ResetPosition(this RectTransform rectTr) {
            rectTr.localScale = Vector3.one;
            rectTr.localPosition = Vector2.zero;
            rectTr.anchoredPosition = Vector2.zero;
            rectTr.offsetMin = Vector2.zero;
            rectTr.offsetMax = Vector2.zero;
        }

        public static void SetColorZero(this MaskableGraphic graphics) {
            graphics.color = new Color(0f, 0f, 0f, 0f);
        }

        public static void AddIfRequired<T>(List<T> list, T prefab, int requireCount, RectTransform parent) where T : Object {
            var requireCountDiff = requireCount - list.Count;
            if (requireCountDiff <= 0) {
                return;
            }

            for (int i = 0; i < requireCountDiff; i++) {
                var clone = Object.Instantiate(prefab, parent);
                list.Add(clone);
            }
        }
    }

    public static class EnumUtil {
        public static T Parse<T>(string str) {
            return (T)Enum.Parse(typeof(T), str);
        }

        public static string ToStringExtended(this RoomType roomType) {
            return roomType switch {
                RoomType.PlayerSpawnRoom  => "Entry Room",
                RoomType.MonsterSpawnRoom => "Monster Spawn Room",
                RoomType.ShopRoom         => "Shop Room",
                RoomType.BossRoom         => "Boss Room",
                RoomType.RewardRoom       => "Reward Room",
                RoomType.EmptyRoom        => "Empty Room",
                RoomType.ExitRoom         => "Exit Room",
                _                         => "Not Implemented This Type !"
            };
        }
    }
}

