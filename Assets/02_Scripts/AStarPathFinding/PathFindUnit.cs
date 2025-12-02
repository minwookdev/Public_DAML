// ReSharper disable InvalidXmlDocComment
/// CODER	      :		
/// MODIFIED DATE : 
/// IMPLEMENTATION:
using System;
using System.Collections;
using CoffeeCat.Utils;
using UnityEngine;

namespace CoffeeCat.Pathfinding2D {
	public class PathFindUnit : MonoBehaviour {
		public bool IsPathfindSmooth = false;
		public Transform target = null;
		public float speed = 2.5f;
		public float turnDist = 5f;
		public float turnSpeed = 3f;
		
		// Normal PathFind
		private Vector2[] paths = null;
		private int targetIndex;
		private Coroutine pathFindCoroutine = null;
		private Transform tr = null;
		
		// Smooth PathFind
		private Path path;

		private void Start() {
			tr = GetComponent<Transform>();
		}

		private void Update() {
			if (Input.GetKeyDown(KeyCode.Space)) {
				if (!target)
					return;
				
				if (IsPathfindSmooth) {
					CatLog.WLog("Smooth Pathfind is Not Supported !");
					return;
				}
				
				PathRequestManager.Inst.RequestPathAsync(tr.position, target.position,
				                                             OnPathFound);
			}
		}

		private void OnPathFound(Vector2[] foundPath, bool isPathFindSuccessful) {
			if (isPathFindSuccessful) {
				paths = foundPath;
				targetIndex = 0;
				if (pathFindCoroutine != null) {
					StopCoroutine(pathFindCoroutine);
				}
				pathFindCoroutine = StartCoroutine(FollowPath());
			}
			else {
				CatLog.Log("Failed Find Path !");
			}
		}

		IEnumerator FollowPath() {
			Vector2 currentWayPoint = paths[0];

			while (true) {
				if ((Vector2)tr.position == currentWayPoint) {
					targetIndex++;
					if (targetIndex >= paths.Length) {
						targetIndex = 0;
						paths = Array.Empty<Vector2>();
						yield break;
					}

					currentWayPoint = paths[targetIndex];
				}

				tr.position = Vector2.MoveTowards(tr.position, currentWayPoint, speed * Time.deltaTime);
				yield return null;
			}
		}

		private void OnPathFoundSmooth(Vector2[] wayPoints, bool isPathFindSuccessful) {
			if (isPathFindSuccessful) {
				path = new Path(wayPoints, tr.position, turnDist);
				if (pathFindCoroutine != null) {
					StopCoroutine(pathFindCoroutine);
				}
				pathFindCoroutine = StartCoroutine(FollowPathSmooth());
			}
			else {
				CatLog.Log("Failed Find Path !");
			}
		}

		IEnumerator FollowPathSmooth() {
			bool isFollowingPath = true;
			int pathIndex = 0;
			//tr.LookAt(path.lookPoints[0]);
			
			while (isFollowingPath) {
				Vector2 pos2D = new Vector2(tr.position.x, tr.position.y);
				if (path.turnBoundaries[pathIndex].HasCrossedLine(pos2D)) {
					if (pathIndex == path.finishLineIndex) {
						isFollowingPath = false;
					}
					else {
						pathIndex++;
					}
				}

				if (isFollowingPath) {
					//Vector2 direction = path.lookPoints[pathIndex] - (Vector2)tr.position;
					//Quaternion targetRotation = Quaternion.LookRotation(direction);
					//tr.rotation = Quaternion.Lerp(tr.rotation, targetRotation, Time.deltaTime * turnSpeed);

					//float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
					//float lerpZ = Mathf.Lerp(tr.eulerAngles.z, angle, turnSpeed * Time.deltaTime);
					//tr.eulerAngles = new Vector3(tr.eulerAngles.x, tr.eulerAngles.y, lerpZ);


					//Vector3 targetDirection = (path.lookPoints[pathIndex] - (Vector2)tr.position).normalized;
					//Vector3 moveDirection = 
					//
					//tr.Translate(Vector3.right * (Time.deltaTime * speed), Space.Self);
				}
				
				yield return null;
			}
			
			yield return null;
		}

		private void OnDrawGizmos() {
			if (!IsPathfindSmooth) {
				if (paths != null) {
					for (int i = targetIndex; i < paths.Length; i++) {
						Gizmos.color = Color.black;
						Gizmos.DrawCube(paths[i], Vector3.one * 0.25f);

						if (i == targetIndex) {
							Gizmos.DrawLine(tr.position, paths[i]);
						}
						else {
							Gizmos.DrawLine(paths[i - 1], paths[i]);
						}
					}
				}
			}
			else {
				if (path != null) {
					path.DrawWithGizmos();
				}
			}

		}
	}
}
