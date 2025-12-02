/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION: 
using UnityEngine;

namespace CoffeeCat.Pathfinding2D {
	public class Path {
		public readonly Vector2[] lookPoints;
		public readonly Line[] turnBoundaries;
		public readonly int finishLineIndex;

		public Path(Vector2[] wayPoints, Vector3 startPoint, float turnDist) {
			lookPoints = wayPoints;
			turnBoundaries = new Line[lookPoints.Length];
			finishLineIndex = turnBoundaries.Length - 1;

			Vector2 previousPoint = new Vector2(startPoint.x, startPoint.y);
			for (int i = 0; i < lookPoints.Length; i++) {
				Vector2 currentPoint = new Vector2(lookPoints[i].x, lookPoints[i].y);
				Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
				Vector2 turnBoundaryPoint =
					(i == finishLineIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDist;

				turnBoundaries[i] = new Line(turnBoundaryPoint, 
				                             previousPoint - dirToCurrentPoint * turnDist);
				previousPoint = turnBoundaryPoint;
			}
		}

		private Vector2 ToVector2(Vector3 vector3) {
			return new Vector2(vector3.x, vector3.z);
		}

		public void DrawWithGizmos() {
			Gizmos.color = Color.black;
			foreach (var point in lookPoints) {
				Vector3 position = new Vector3(point.x, point.y, -0.5f);
				Gizmos.DrawCube(position, Vector3.one * 0.25f);
			}

			Gizmos.color = Color.white;
			foreach (var line in turnBoundaries) {
				line.DrawWithGizmos(3);
			}
		}
	}
}
