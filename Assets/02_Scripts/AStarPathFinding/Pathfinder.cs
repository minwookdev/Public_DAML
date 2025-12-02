using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using CoffeeCat.Utils;
using PathFindGrid = CoffeeCat.Pathfinding2D.Grid;
using PathFindData = CoffeeCat.Pathfinding2D.Grid.PathFindData;

namespace CoffeeCat.Pathfinding2D {
    public class Pathfinder : MonoBehaviour {
        // Components
        private PathFindGrid pathFindGrid = null;
        
        // Variables
        private Dictionary<int, Heap<Node>> openSetDict;
        private HashSet<Node> closeSet;
        
        private void Start() {
            pathFindGrid = GetComponent<PathFindGrid>();
            openSetDict = new Dictionary<int, Heap<Node>>();
            closeSet = new HashSet<Node>();
        }
        
        public void StartFindPath(Vector2 startPosition, Vector2 targetPosition) {
            StartCoroutine(FindPathAync(startPosition, targetPosition));
        }

        private IEnumerator FindPathAync(Vector2 startPos, Vector2 targetPos) {
            // 로직 수행 시간 측정
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            
            // PathFind에 필요한 데이터 요청
            if (!pathFindGrid.TryGetPathFindData(startPos, targetPos, out PathFindData pathFindData)) {
                // PathFind 데이터 요청 실패 시 RequestManager에 결과 통보
                PathRequestManager.Inst.FinishedProcessingRequest(Array.Empty<Vector2>(), false);
                stopWatch.Stop();
                yield break;
            }
            
            // PathFind에 필요한 변수 세팅
            Node startNode = pathFindData.StartNode;
            Node targetNode = pathFindData.EndNode;
            int maxGridCount = pathFindData.MaxGridCount; // GridCountX * GridCountY

            
            // 포인트 배열과 결과 변수
            Vector2[] wayPoints = Array.Empty<Vector2>();
            bool isPathFindSuccess = false;
            
            // 노드 상태 체크
            if (startNode.IsMoveable && targetNode.IsMoveable) {
                // StartNode gCost 초기화 Overflow방지
                startNode.gCost = 0;

                //Heap<Node> openSet     = new Heap<Node>(pathFindGrid.GridMaxSize);
                //HashSet<Node> closeSet = new HashSet<Node>();
                Heap<Node> openSet = GetOpenSet(maxGridCount);
                openSet.Clear();
                closeSet.Clear();

                openSet.Add(startNode);
                while (openSet.Count > 0) {
                    Node currentNode = openSet.RemoveFirst();
                    // Heap Class 사용 전 List사용 시 로직
                    //for (int i = 1; i < openSet.Count; i++) {
                    //    if (openSet[i].fCost < currentNode.fCost ||
                    //        openSet[i].fCost == currentNode.fCost ||
                    //        openSet[i].hCost < currentNode.hCost) {
                    //        currentNode = openSet[i];
                    //    }
                    //}
                    //
                    //openSet.Remove(currentNode);
                    closeSet.Add(currentNode);

                    // 목표 Node에 다다른 경우
                    if (currentNode == targetNode) {
                        isPathFindSuccess = true;
                        stopWatch.Stop();
                        CatLog.Log("Path find. Time Elapsed: " + stopWatch.ElapsedMilliseconds.ToString() + " ms");
                        break;
                    }

                    var neighbourNodes = pathFindGrid.GetNeighbours(currentNode);
                    foreach (var neighbourNode in neighbourNodes) {
                        if (neighbourNode.IsMoveable == false || closeSet.Contains(neighbourNode)) {
                            continue;
                        }

                        int newMovementCostToNeighbour = currentNode.gCost + 
                                                         GetDistance(currentNode, neighbourNode) + 
                                                         neighbourNode.MovementPenalty;
                        if (newMovementCostToNeighbour < neighbourNode.gCost || !openSet.Contains(neighbourNode)) {
                            neighbourNode.gCost = newMovementCostToNeighbour;
                            neighbourNode.hCost = GetDistance(neighbourNode, targetNode);
                            neighbourNode.parent = currentNode;

                            if (!openSet.Contains(neighbourNode)) {
                                openSet.Add(neighbourNode);
                            }
                            else {
                                openSet.UpdateItem(neighbourNode);
                            }
                        }
                    }
                }
            }

            yield return null;

            if (isPathFindSuccess) {
                wayPoints = RetracePath(startNode, targetNode);
            }
            PathRequestManager.Inst.FinishedProcessingRequest(wayPoints, isPathFindSuccess);
        }

        private Vector2[] RetracePath(Node startNode, Node endNode) {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            
            while (currentNode != startNode) {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Add(currentNode); // StartNode를 포함 (Trace가 어색할 경우 주석) Unmovable Node를 뚫고가는 문제 방지

            // TODO: EndNode - 1 (*로직 수정 distance만큼 떨어진 위치를 목표로)
            path.RemoveAt(0);

            Vector2[] wayPoints = GetSimplifyPath(path);
            Array.Reverse(wayPoints);
            return wayPoints;
        }

        Vector2[] GetSimplifyPath(List<Node> path) {
            List<Vector2> wayPoints = new List<Vector2>();
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < path.Count; i++) {
                Vector2 directionNew = new Vector2(path[i - 1].GridX - path[i].GridX,
                                                   path[i - 1].GridY - path[i].GridY);
                if (directionNew != directionOld) {
                    //wayPoints.Add(path[i].WorldPosition);
                    wayPoints.Add(path[i - 1].WorldPosition);
                }
                directionOld = directionNew;
            }

            return wayPoints.ToArray();
        }

        private int GetDistance(Node startNode, Node endNode) {
            int distanceX = Mathf.Abs(startNode.GridX - endNode.GridX);
            int distanceY = Mathf.Abs(startNode.GridY - endNode.GridY);
            if (distanceX > distanceY) {
                return 14 * distanceY + 10 * (distanceX - distanceY); // 14? 10?
            } return 14 * distanceX + 10 * (distanceY - distanceX);
        }
        
        private Heap<Node> GetOpenSet(int gridMaxCount) {
            if (openSetDict.TryGetValue(gridMaxCount, out var result)) {
                return result;
            }

            openSetDict.Add(gridMaxCount, new Heap<Node>(gridMaxCount));
            result = openSetDict[gridMaxCount];
            return result;
        }

        /// <summary>
        /// 씬 또는 전체 PathFind Grid 변경 시 호출
        /// </summary>
        private void ClearSets() {
            openSetDict.Clear();
            closeSet.Clear();
        }
    }
}
