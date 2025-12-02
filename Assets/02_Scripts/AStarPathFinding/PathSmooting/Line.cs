// ReSharper disable InvalidXmlDocComment
/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION: 
using UnityEngine;

namespace CoffeeCat.Pathfinding2D {
    public struct Line {
        private const float verticalLineGradient = 1e5f;

        private float gradient;
        private float y_intercept;
        private Vector2 pointOnLine_1;
        private Vector2 pointOnLine_2;

        private float gradientPerpendicular;
        private bool approachSide;

        public Line(Vector2 pointOnLine, Vector2 pointPerpendicularToLine) {
            float dx = pointOnLine.x - pointPerpendicularToLine.x;
            float dy = pointOnLine.y - pointPerpendicularToLine.y;

            if (dx == 0) {
                gradientPerpendicular = verticalLineGradient;
            }
            else {
                gradientPerpendicular = dy / dx;
            }

            if (gradientPerpendicular == 0) {
                gradient = verticalLineGradient;
            }
            else {
                gradient = -1 / gradientPerpendicular;
            }

            y_intercept = pointOnLine.y - gradient * pointOnLine.x;
            pointOnLine_1 = pointOnLine;
            pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

            approachSide = false;
            approachSide = GetSide(pointPerpendicularToLine);
        }

        private bool GetSide(Vector2 p) {
            return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) >
                   (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
        }

        public bool HasCrossedLine(Vector2 p) {
            return GetSide(p) != approachSide;
        }

        public void DrawWithGizmos(float lenght) {
            float depth = -0.5f;
            Vector3 lineDir = new Vector3(1f, gradient, depth).normalized;
            //Vector3 lineCenter = new Vector3(pointOnLine_1.x, pointOnLine_1.y, -depth) + Vector3.up;
            Vector3 lineCenter = new Vector3(pointOnLine_1.x, pointOnLine_1.y, depth);
            Gizmos.DrawLine(lineCenter - lineDir * lenght / 2f, lineCenter + lineDir * lenght / 2f);
            Gizmos.DrawSphere(lineCenter, 0.25f);
        }
    }
}
