// ReSharper disable InvalidXmlDocComment
// ReSharper disable IdentifierTypo
/// CODER	      :	MINWOOK KIM
/// MODIFIED DATE : 2023. 08. 10
/// IMPLEMENTATION: Pathfind Grid 생성 컴포넌트
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using RandomDungeonWithBluePrint;
using CoffeeCat.Utils;

namespace CoffeeCat.Pathfinding2D {
    public class Grid : MonoBehaviour {
        [Title("Generate Options")]
        public bool IsCompatibleMapGen = false; // 맵 생성기 호환 (자체적으로 Grid생성하지 않음)
        [PropertyTooltip("Grid가 더 많은 메모리를 사용하지만 PathFind에 걸리는 시간이 단축됨")]
        public bool IsBakeNeibourNode = false;  // Grid 생성 시 Neighbour Node 배열 할당
        public LayerMask UnmoveableMask;        // 이동할 수 없는 레이어를 지정
        [DisableIf(nameof(IsCompatibleMapGen))]
        public Vector2 GridWorldSize;           //전체 그리드 사이즈 지정
        [DisableIf(nameof(IsCompatibleMapGen))]
        public float NodeRadius;                // 노드 반지름
        [SerializeField, ReadOnly]
        private float nodeDiameter;             // 노드 지름       
        
        [Title("Terrain Types")]
        public bool IsExistPenaltyCost = true;
        [EnableIf(nameof(IsExistPenaltyCost))]
        public int obstacleProximityPenalty = 10; // 장애물 밑 Node Penalty
        [SerializeField, EnableIf(nameof(IsExistPenaltyCost)), InfoBox("TerrainMask의 여러 Layer 등록 금지.")]
        private PathfindTerrainType[] movableTerrainTypes;
        private LayerMask mergedPenaltyTerrainMask; // Merged TerrainMasks
        private int penaltyMin = int.MaxValue;
        private int penaltyMax = int.MinValue;
        private RaycastHit2D[] raycastHit2DResults = new RaycastHit2D[5];
        private readonly Dictionary<int, int> movableTerrainTypesDict = new Dictionary<int, int>(); // <LayerNumber, Penalty>

        [Title("Debug")]
        public bool IsDrawGridGizmos = true;
        public bool IsDrawPenaltyBlur = false;
        public Transform playerTr = null;
        private Node playerNode = null;
        
        // Fields
        private Transform tr = null;
        private Dictionary<Rect, Node[,]> gridDict = null;
        private readonly PathFindData pathFindData = new PathFindData();
        private readonly List<Node> tempNodeList = new List<Node>();
        private readonly Dictionary<Node, Node[]> neighboursDict = new Dictionary<Node, Node[]>();
        
        public class PathFindData {
            public Node StartNode;
            public Node EndNode;
            public int MaxGridCount;

            public void Set(Node startNode, Node endNode, int gridCountX, int gridCountY) {
                StartNode = startNode;
                EndNode = endNode;
                MaxGridCount = gridCountX * gridCountY;
            }

            public void Clear() {
                StartNode = null;
                EndNode = null;
                MaxGridCount = 0;
            }
        }

        [Serializable]
        public class PathfindTerrainType {
            public LayerMask TerrainMask;
            public int TerrainPenalty;
        }
        
        private void Start() {
            tr = GetComponent<Transform>();
            if (IsExistPenaltyCost) {
                // Merge Movable Terrain Masks and Set Dictionary
                foreach (var terrainType in movableTerrainTypes) {
                    mergedPenaltyTerrainMask.value |= terrainType.TerrainMask.value;
                    movableTerrainTypesDict.Add(Utility.GetLayerNumberInMask(terrainType.TerrainMask.value), 
                                                terrainType.TerrainPenalty);
                }
            }
            if (IsCompatibleMapGen)
                return;
            
            // 노드 지름 구하고 정의되는 각 X축, Y축의 노드 수를 계산
            nodeDiameter = NodeRadius * 2f;
            int gridCountX = Mathf.RoundToInt(GridWorldSize.x / nodeDiameter);
            int gridCountY = Mathf.RoundToInt(GridWorldSize.y / nodeDiameter);
            // 불필요한 공간이 남는지 Node당 소숫점이 존재하는지 체크
            CheckNodeSize(GridWorldSize, nodeDiameter);
            // PathFind 생성
            CreateGrid(gridCountX, gridCountY);
        }

        /// <summary>
        /// A* Grid 생성
        /// </summary>
        private void CreateGrid(int gridCountX, int gridCountY) {
            // Add to Grid Dictionary
            gridDict = new Dictionary<Rect, Node[,]>();

            // Grid의 World Bottom-Left Vector 계산
            Vector3 worldBottomLeft = tr.position -
                                      Vector3.right * GridWorldSize.x / 2f -
                                      Vector3.up * GridWorldSize.y / 2f;
            
            Rect keyRect = new Rect(worldBottomLeft.x, worldBottomLeft.y, GridWorldSize.x, GridWorldSize.y);
            gridDict.Add(keyRect, new Node[gridCountX, gridCountY]);
            
            for (int x = 0; x < gridCountX; x++) {
                for (int y = 0; y < gridCountY; y++) {
                    // Node 중점 계산
                    Vector2 worldPoint =
                        worldBottomLeft +
                        Vector3.right * (x * nodeDiameter + NodeRadius) +
                        Vector3.up * (y * nodeDiameter + NodeRadius);

                    // Overlap Circle을 통한 Layer검출로 Movable 판별
                    bool isMovable = !(Physics2D.OverlapCircle(worldPoint, NodeRadius, UnmoveableMask));
                    
                    // Set Movement Penalty
                    int penalty = GetLowestPenalty(isMovable, worldPoint, NodeRadius);
                    
                    gridDict[keyRect][x, y] = new Node(isMovable, worldPoint, x, y, penalty);
                }
            }

            // Bake Neighbour Node Dictionary
            BakeNeighbourNodes(gridDict[keyRect], gridCountX, gridCountY);

            // Bake Penalty Blur
            BlurPenaltyMap(3);    
        }
        
        /// <summary>
        /// RandomMapGenerator를 기반으로 A*Grid를 생성
        /// </summary>
        /// <param name="generator">RandomMapGenerator GameObject</param>
        /// <param name="nodeRadius">각각의 Node의 반지름</param>
        /// <returns></returns>
        public void CreateGridDictionary(RandomMapGenerator generator, float nodeRadius = 0.5f) {
            if (!IsCompatibleMapGen) {
                return;
            }
            
            // Rooms 초기화 체크
            var rooms = generator.field.Rooms;
            if (rooms.Count <= 0) {
                return;
            }
            
            // 각각의 Room에서 Floor Tiles 데이터 Dictionary 저장
            // Dictionary<방 전체 크기 RectInt, 타일 WorldPosition Vector2Int List>
            var floorTilesDict = new Dictionary<RectInt, List<Vector2Int>>();
            for (int i = 0; i < rooms.Count; i++) {
                floorTilesDict.Add(rooms[i].Rect, rooms[i].Floors);
            }
            
            // Result Dictionary
            gridDict = new Dictionary<Rect, Node[,]>();
            float radius = nodeRadius;    // 노드의 반지름
            float diameter = radius * 2f; // 노드의 지름
            NodeRadius = radius; nodeDiameter = diameter;
            foreach (var pair in floorTilesDict) {   // Room 마다 Floor타일 리스트
                // Grid 최소, 최대 사이즈를 구하기 위해 선언 후 첫번째 요소로 초기화
                var vec2IntList = pair.Value; // Floor Vec2Int(Tiles) List
                float xMin = vec2IntList[0].x, 
                      xMax = vec2IntList[0].x,
                      yMin = vec2IntList[0].y,
                      yMax = vec2IntList[0].y;
                
                // Floor Tiles List 순회하며 최소, 최대 정점 찾음 (타일 정점: BOTTOM-LEFT)
                for (int i = 0; i < vec2IntList.Count; i++) {
                    if (vec2IntList[i].x < xMin) xMin = vec2IntList[i].x;
                    if (vec2IntList[i].x > xMax) xMax = vec2IntList[i].x;
                    if (vec2IntList[i].y < yMin) yMin = vec2IntList[i].y;
                    if (vec2IntList[i].y > yMax) yMax = vec2IntList[i].y;
                }
                
                // 최소, 최대 정점을 통해 Width, Height구하고 X, Y축으로 최대 노드 갯수 계산
                // TODO: GridWorldSizeX, Y 식 수정 (defaultCellSizeXY를 Generator로 부터 얻기)
                float defaultCellSizeX = 1f, defaultCellSizeY = 1f;
                float gridWorldSizeX  = (xMax + (diameter * (defaultCellSizeX / diameter))) - xMin;
                float gridWorldSizeY  = (yMax + (diameter * (defaultCellSizeY / diameter))) - yMin;

                //if (gridWorldSizeX % diameter != 0f || gridWorldSizeY % diameter != 0f ||           
                //    (gridWorldSizeX / diameter) % 1 != 0 || (gridWorldSizeY / diameter) % 1 != 0) { 
                //    CatLog.WLog("Grid Creating Warning !");
                //}
                // 불필요한 공간이 남는지 Node당 소숫점이 존재하는지 체크
                CheckNodeSize(new Vector2(gridWorldSizeX, gridWorldSizeY), diameter);
                
                int nodeXCount = Mathf.FloorToInt(gridWorldSizeX / diameter);
                int nodeYCount = Mathf.FloorToInt(gridWorldSizeY / diameter);
                //int nodeXCount = Mathf.RoundToInt(gridXSize / diameter);
                //int nodeYCount = Mathf.RoundToInt(gridYSize / diameter);

                // Room 전체 크기 Rect를 Key, 각 Room의 X, Y축 최대 노드 갯수를 통해 초기화
                //Vector2 gridRect = new Vector2(gridSizeX, gridSizeY); //
                // <MapGenerator의 생성로직 변형에 대응하기 위해 Key인 RectInt를 사용하지 않음>
                Rect roomRect = new Rect(xMin, yMin, gridWorldSizeX, gridWorldSizeY);
                gridDict.Add(roomRect, new Node[nodeXCount, nodeYCount]);

                // 각각의 Grid X, Y순회하며 Node할당
                for (int x = 0; x < nodeXCount; x++) {
                    for (int y = 0; y < nodeYCount; y++) {
                        // 현재 Node의 좌표와 Floor Tile좌표 일치 여부를 통해 이동 가능 타일 여부 체크
                        //Vector2Int compareVec2Int = new Vector2Int((int)(xMin + (x * diameter)), (int)(yMin + (y * diameter)));
                        //bool isMovableNode = vec2IntList.Contains(compareVec2Int);
                        
                        // Node의 WorldPosition
                        Vector2 worldPos = new Vector2((xMin + radius) + x * diameter, (yMin + radius) + y * diameter);
                        
                        // Node의 Movable Check (Tile Collider 검출)
                        bool isMovable = !(Physics2D.OverlapCircle(worldPos, radius, UnmoveableMask));
                        //Physics2D.OverlapBox()

                        // Set Penalty By Terrain Types
                        int penalty = GetLowestPenalty(isMovable, worldPos, radius);
                        
                        // X, Y 위치의 Node 할당
                        gridDict[roomRect][x, y] = new Node(isMovable, worldPos, x, y, penalty);
                    }
                }
                // Set Neighbours in Nodes
                BakeNeighbourNodes(gridDict[roomRect], nodeXCount, nodeYCount);
            }
            
            BlurPenaltyMap(3);
        }

        private void BakeNeighbourNodes(Node[,] grid, int gridCountX, int gridCountY) {
            if (!IsBakeNeibourNode) {
                return;
            }
            
            foreach (var node in grid) {
                tempNodeList.Clear();
                for (int x = -1; x <= 1; x++) {
                    for (int y = -1; y <= 1; y++) {
                        // 자신 Node는 제외
                        if (x == 0 && y == 0) {
                            continue;
                        }

                        int checkX = node.GridX + x;
                        int checkY = node.GridY + y;
                        if (checkX >= 0 && checkX < gridCountX && checkY >= 0 && checkY < gridCountY) {
                            tempNodeList.Add(grid[checkX, checkY]);
                        }
                    }
                }
                neighboursDict.Add(node, tempNodeList.ToArray());
            }
        }

        private void CheckNodeSize(Vector2 gridWorldSizeVec2, float diameter) { 
            if (!MathHelper.IsDivisible(gridWorldSizeVec2.x, diameter) || 
                !MathHelper.IsDivisible(gridWorldSizeVec2.y, diameter) ||
                 MathHelper.IsDecimalPoint(gridWorldSizeVec2.x / diameter) || 
                 MathHelper.IsDecimalPoint(gridWorldSizeVec2.y / diameter)) {
                CatLog.WLog("Grid Creating Warning !");
            }
        }
        
        /// <summary>
        /// 해당 WorldPosition에서 Raycast를 통해 Terrain 객체를 체크하고 가장 낮은 Penalty값을 확인
        /// </summary>
        /// <param name="isMovableNode"></param>
        /// <param name="worldPosition"></param>
        /// <param name="nodeRadius"></param>
        /// <returns></returns>
        private int GetLowestPenalty(bool isMovableNode, Vector2 worldPosition, float nodeRadius) {
            int lowestPenalty = 0;
            if (!IsExistPenaltyCost) {
                return lowestPenalty;
            }

            if (!isMovableNode) {
                return obstacleProximityPenalty;
            }
                
            int rayHitCount = Physics2D.CircleCastNonAlloc(worldPosition, nodeRadius - .01f, 
                                                           Vector3.forward, raycastHit2DResults, 
                                                           10f, mergedPenaltyTerrainMask);
            // 가장 낮은 Penalty 검색
            for (int i = 0; i < rayHitCount; i++) {
                int layer = raycastHit2DResults[i].collider.gameObject.layer;
                if (!movableTerrainTypesDict.TryGetValue(layer, out int penalty)) 
                    continue;
                    
                // 첫 순회는 기준 penalty 할당
                if (i == 0) {
                    lowestPenalty = penalty;
                }
                else {
                    // 현재 Penalty와 비교 낮은 Penalty 할당
                    if (penalty < lowestPenalty) {
                        lowestPenalty = penalty;
                    }
                }
            }
            return lowestPenalty;
        }

        /// <summary>
        /// Penalty가 낮은 Node의 가장자리만을 통해 이동하는 현상 완화
        /// </summary>
        /// <param name="blurSize"></param>
        private void BlurPenaltyMap(int blurSize = 3) {
            if (!IsExistPenaltyCost) {
                return;
            }
            
            int kernelSize = blurSize * 2 + 1;
            int kernelExtents = (kernelSize - 1) / 2;

            foreach (var pair in gridDict) {
                int gridCountX = pair.Value.GetLength(0);
                int gridCountY = pair.Value.GetLength(1);
                int[,] penaltiesHorizontalPass = new int[gridCountX, gridCountY];
                int[,] penaltiesVerticalPass = new int[gridCountX, gridCountY];

                for (int y = 0; y < gridCountY; y++) {
                    for (int x = -kernelExtents; x <= kernelExtents; x++) {
                        int sampleX = Mathf.Clamp(x, 0, kernelExtents);
                        penaltiesHorizontalPass[0, y] += pair.Value[sampleX, y].MovementPenalty;
                    }

                    for (int x = 1; x < gridCountX; x++) {
                        int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridCountX);
                        int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridCountX - 1);

                        penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] -
                            pair.Value[removeIndex, y].MovementPenalty + pair.Value[addIndex, y].MovementPenalty;
                        
                    }
                }

                for (int x = 0; x < gridCountX; x++) {
                    for (int y = -kernelExtents; y <= kernelExtents; y++) {
                        int sampleY = Mathf.Clamp(y, 0, kernelExtents);
                        penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
                    }
                    
                    int blurredPenalty =
                        Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
                    pair.Value[x, 0].MovementPenalty = blurredPenalty;

                    for (int y = 1; y < gridCountY; y++) {
                        int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridCountY);
                        int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridCountY - 1);

                        penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] -
                            penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
                        blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
                        pair.Value[x, y].MovementPenalty = blurredPenalty;

                        if (blurredPenalty > penaltyMax) {
                            penaltyMax = blurredPenalty;
                        }

                        if (blurredPenalty < penaltyMin) {
                            penaltyMin = blurredPenalty;
                        }
                    }
                }
            }

        }
        
        /// <summary>
        /// 매개변수의 주변 Node를 체크하고 유효한 Node List 반환
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Node[] GetNeighbours(Node node) {
            // Return Baked Neighbour Nodes
            if (IsBakeNeibourNode) {
                return neighboursDict[node];
            }
            
            // Find Neighbour Nodes
            tempNodeList.Clear(); // Neighbours Node Temp List
            var gridCounts = GetGridCounts(node, out Rect keyRect);
            if (gridCounts == null) {
                return tempNodeList.ToArray();
            }

            int gridCountX = gridCounts[0], gridCountY = gridCounts[1];
            
            // 매개변수 노드 주변 8칸 체크
            for (int x = -1; x <= 1; x++) {
                for (int y = -1; y <= 1; y++) {
                    // 자신 Node는 제외
                    if (x == 0 && y == 0) {
                        continue;
                    }
                    
                    int checkX = node.GridX + x;
                    int checkY = node.GridY + y;

                    if (checkX >= 0 && checkX < gridCountX && checkY >= 0 && checkY < gridCountY) {
                        tempNodeList.Add(gridDict[keyRect][checkX, checkY]);
                    }
                }
            }
            return tempNodeList.ToArray();
        }

        /// <summary>
        /// World Position을 통한 Grid Dictionary내 Node를 반환
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="isGetNearestNode"> worldPosition과 가장 까까운 Node강제 반환 </param>
        /// <returns></returns>
        public Node GetNodeFromPosition(Vector2 worldPosition, bool isGetNearestNode = false) {
            if (!TryGetKeyRect(worldPosition, out Rect keyRect)) {
                return null;
            }

            Node[,] targetGrid = gridDict[keyRect];
            int x, y;
            float percentX = (worldPosition.x - keyRect.center.x + keyRect.width / 2f) / keyRect.width;
            float percentY = (worldPosition.y - keyRect.center.y + keyRect.height / 2f) / keyRect.height;
            int gridCountX = targetGrid.GetLength(0); // Grid X Count
            int gridCountY = targetGrid.GetLength(1); // Grid Y Count
            if (isGetNearestNode) { 
                // 가장 가까운 노드를 반환
                x = Mathf.FloorToInt(Mathf.Clamp((gridCountX) * percentX, 0, gridCountX - 1));
                y = Mathf.FloorToInt(Mathf.Clamp((gridCountY) * percentY, 0, gridCountY - 1));
                return targetGrid[x, y];
            }
            else {
                // 일치하는 노드를 반환 (일치하는 노드가 없다면 Null)
                x = Mathf.FloorToInt((gridCountX) * percentX);
                y = Mathf.FloorToInt((gridCountY) * percentY);
                
                // X, Y 수치의 범위를 확인해 존재하는 노드인지 확인 후 반환 
                var resultNode = (x >= 0 && x <= gridCountX - 1 && y >= 0 && y <= gridCountY - 1) ? targetGrid[x, y] : null;
                return resultNode;
            }
        }

        /// <summary>
        /// PathFinder의 필요한 데이터 클래스를 반환
        /// </summary>
        /// <param name="startPosition"></param>
        /// <param name="targetPosition"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool TryGetPathFindData(Vector2 startPosition, Vector2 targetPosition, out PathFindData result) {
            result = null;
            if (!Compare(startPosition, targetPosition, out Rect equalKeyRect)) {
                return false;
            }
            
            Node[,] targetGrid = gridDict[equalKeyRect];
            float percentX, percentY;
            int x, y;
            int gridCountX = targetGrid.GetLength(0);
            int gridCountY = targetGrid.GetLength(1);
            Node startNode  = GetNode(startPosition);
            Node targetNode = GetNode(targetPosition);
            if (startNode == null || targetNode == null) {
                return false;
            }
            
            pathFindData.Clear();
            pathFindData.Set(startNode, targetNode, gridCountX, gridCountY);
            result = pathFindData;
            return true;
            
            // INNER
            Node GetNode(Vector2 worldPosition) {
                percentX = (worldPosition.x - equalKeyRect.center.x + equalKeyRect.width / 2f) / equalKeyRect.width;
                percentY = (worldPosition.y - equalKeyRect.center.y + equalKeyRect.height / 2f) / equalKeyRect.height;
                // 일치하는 노드를 반환 (일치하는 노드가 없다면 Null)
                x = Mathf.FloorToInt((gridCountX) * percentX);
                y = Mathf.FloorToInt((gridCountY) * percentY);
                return (x >= 0 && x <= gridCountX - 1 && y >= 0 && y <= gridCountY - 1) ? targetGrid[x, y] : null;
            }

            bool Compare(Vector2 pos1, Vector2 pos2, out Rect keyRect) {
                keyRect = default;
                bool result1 = TryGetKeyRect(pos1, out Rect keyRect1);
                bool result2 = TryGetKeyRect(pos2, out Rect keyRect2);
                if (!result1 || !result2) {
                    return false;
                }
                if (keyRect1 == keyRect2) {
                    keyRect = keyRect1;
                } 
                return true;
            }
        }

        /// <summary>
        /// return new int[2] { GridXCount, GridYCount } With Dictionary Key
        /// </summary>
        /// <param name="targetNode"></param>
        /// <returns></returns>
        private int[] GetGridCounts(Node targetNode, out Rect key) {
            // Find Key From World Position
            if (!TryGetKeyRect(targetNode.WorldPosition, out key)) {
                return null;
            }
            
            // Return Grid X, Y Counts
            return new int[2] { gridDict[key].GetLength(0), 
                                gridDict[key].GetLength(1) };
        }
        
        /// <summary>
        /// Position을 통해 범위의 Grid Dictionary Key인 RectInt를 검색
        /// </summary>
        /// <param name="worldPosition">World Position Vector2</param>
        /// <returns></returns>
        private Rect GetKeyRect(Vector2 worldPosition) {
            if (gridDict != null && gridDict.Count > 0) {
                var result = gridDict.Select(pair => pair.Key)
                                                .FirstOrDefault(keyRect => keyRect.IsInside(worldPosition));
            }
            CatLog.ELog("Dictionary is Null or Empty");
            return default(Rect);
        }

        /// <summary>
        /// false = 해당 위치의 Room Rect를 찾을 수 없음
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private bool TryGetKeyRect(Vector2 worldPosition, out Rect key) {
            key = default(Rect);
            if (gridDict == null) {
                CatLog.ELog("Grid Dictionary is Null !");
                return false;
            }

            // Find Grid Rect From WorldPosition
            key = gridDict.Select(pair => pair.Key)
                                .FirstOrDefault(keyRect => keyRect.IsInside(worldPosition));
            
            bool result = gridDict.ContainsKey(key);
            //if (!result) { // 같은 Room Rect 확인
            //    CatLog.ELog($"Not Found Key in Grid Dictionary ! key: {key.ToString()}");
            //}
            return result;
        }

        #region ORIGIN
        
        // Original GetNodeFromWorldPoint
        //public Node GetNodeFromWorldPoint(Vector2 worldPosition, bool isGetNearestNode = true) {
        //    Vector3 thisPosition = (tr != null) ? tr.position : transform.position;
        //    float percentX = (worldPosition.x - thisPosition.x + GridWorldSize.x / 2f) / GridWorldSize.x;
        //    float percentY = (worldPosition.y - thisPosition.y + GridWorldSize.y / 2f) / GridWorldSize.y;
        //
        //    if (isGetNearestNode) {
        //        int x = Mathf.FloorToInt(Mathf.Clamp((gridCountX) * percentX, 0, gridCountX - 1));
        //        int y = Mathf.FloorToInt(Mathf.Clamp((gridCountY) * percentY, 0, gridCountY - 1));
        //        return grid[x, y];
        //    }
        //    else {
        //        int x = Mathf.FloorToInt((gridCountX) * percentX);
        //        int y = Mathf.FloorToInt((gridCountY) * percentY);
        //        return (x >= 0 && x <= gridCountX - 1 && y >= 0 && y <= gridCountY - 1) ? grid[x, y] : null;
        //        //resultNode = grids[x, y]; 잘못된 index 짚어도 bool로 변환하는 연산자 오버로딩
        //    }
        //}

        
        #endregion

        private void OnDrawGizmos() {
            if (!IsDrawGridGizmos) {
                return;
            }

            if (!IsCompatibleMapGen) {
                DrawGridWorldSizeWireCube();
            }

            if (gridDict == null) {
                return;
            }

            // Get Player Floored Node
            playerNode = (playerTr) ? GetNodeFromPosition(playerTr.position) : null;
            // Draw Node Gizmos 
            foreach (var pair in gridDict) {
                var grid = pair.Value;
                DrawWireCube(pair.Key);
                foreach (var node in grid) {
                    Gizmos.color = Color.white;
                    if (IsDrawPenaltyBlur) {
                        Gizmos.color = Color.Lerp(Color.white, 
                                                  Color.black, 
                                                  Mathf.InverseLerp(penaltyMin, penaltyMax, node.MovementPenalty));
                    }
                    SetGizmosColor((node.IsMoveable) ? Gizmos.color : Color.red);
                    if (node == playerNode) {
                        SetGizmosColor(Color.cyan);
                    }
                    SetGizmosAlpha();
                    DrawCube(node);
                    
                    //Debug.DrawRay((Vector3)node.WorldPosition + Vector3.back * 10f, Vector3.forward * 20f);
                }
            }

            void DrawWireCube(Rect rect) {
                var wireCubeCenterVec = new Vector3(rect.center.x, rect.center.y, 0f);
                var wireCubeSizeVec = new Vector3(rect.width, rect.height, .5f);
                Gizmos.DrawWireCube(wireCubeCenterVec, wireCubeSizeVec);
            }
            
            void DrawCube(Node node) {
                Gizmos.DrawCube(node.WorldPosition, Vector3.one * (nodeDiameter - .05f));
            }

            void SetGizmosColor(Color targetColor) {
                Gizmos.color = targetColor;
            }

            void SetGizmosAlpha() {
                Color gizmosColor = Gizmos.color;
                gizmosColor.a = 0.75f;
                Gizmos.color = gizmosColor;
            }

            void DrawGridWorldSizeWireCube() {
                var thisPos = transform.position;
                var centerVector3 = new Vector3(thisPos.x, thisPos.y, 0f);
                var sizeVector3 = new Vector3(GridWorldSize.x, GridWorldSize.y, 0f);
                Gizmos.DrawWireCube(centerVector3, sizeVector3);
            }
        }
    }
}