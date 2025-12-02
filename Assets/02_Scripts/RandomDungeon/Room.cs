// ReSharper disable UnusedAutoPropertyAccessor.Global
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CoffeeCat;
using CoffeeCat.Utils;
using CoffeeCat.RogueLite;

namespace RandomDungeonWithBluePrint {
    public class Joint {
        public int Direction;
        public Vector2Int Position;
        public bool Connected;
    }

    public class Room {
        public RectInt Rect;                                // 방 전체 사이즈 Rect
        public readonly List<Vector2Int> Positions = new(); // Room의 전체 좌표 List
        public readonly Dictionary<int, List<Vector2Int>> Edge = new();
        public readonly Dictionary<int, Vector2Int> EdgeWithCenter = new();
        public readonly List<Joint> Joints = new();
        public List<GateObject> GateObjects = new(); // 방의 게이트 오브젝트
        
        // Floor Tiles Data
        public readonly List<Vector2Int> Floors = new List<Vector2Int>(); // 바닥 좌표 Vector2Int List
        public RectInt FloorRectInt;
        
        // Wall Tiles Data
        public readonly int WallHeight = 2; // 벽 두께 ( 0 = NoWalls, 1 = (default), 2 = (extended) ... )
        public readonly Dictionary<int, List<Vector2Int>> WallTilesDict = new Dictionary<int, List<Vector2Int>>(); // < WallTileHeight, TilesPositionList >

        // RogueLite Room Data
        public RoomData RoomData { get; private set; } = null;
        public RoomType RoomType => RoomData?.RoomType ?? RoomType.EmptyRoom;

        public Room(RectInt rect) {
            Rect = rect;
            foreach (var pos in Rect.allPositionsWithin) {
                Positions.Add(pos);
            }
    
            Edge[Constants.Direction.Left] = Positions.Where(p => p.x == Rect.xMin).ToList();
            Edge[Constants.Direction.Right] = Positions.Where(p => p.x == Rect.xMax - 1).ToList();
            Edge[Constants.Direction.Up] = Positions.Where(p => p.y == Rect.yMin).ToList();
            Edge[Constants.Direction.Down] = Positions.Where(p => p.y == Rect.yMax - 1).ToList();

            EdgeWithCenter[Constants.Direction.Left] = Edge[Constants.Direction.Left]
                .Where(c => c.y == Mathf.Round(Rect.yMin + Rect.height / 2)).FirstOrDefault();
            EdgeWithCenter[Constants.Direction.Right] = Edge[Constants.Direction.Right]
                .Where(c => c.y == Mathf.Round(Rect.yMin + Rect.height / 2)).FirstOrDefault();
            EdgeWithCenter[Constants.Direction.Up] = Edge[Constants.Direction.Up]
                .Where(c => c.x == Mathf.Round(Rect.xMin + Rect.width / 2)).FirstOrDefault();
            EdgeWithCenter[Constants.Direction.Down] = Edge[Constants.Direction.Down]
                .Where(c => c.x == Mathf.Round(Rect.xMin + Rect.width / 2)).FirstOrDefault();
            
            SetWalls();
            SetFloor();

            #region INNER
            // 벽 타일 Dictionary 정의 Dictionary<높이, 타일 Vector2>
            void SetWalls() {
                // Walls Dictionary 초기화
                for (int i = 0; i < WallHeight; i++) {
                    WallTilesDict.Add(i, new List<Vector2Int>());
                }

                // Set Wall Tile Positions Dictionary
                for (int i = 0; i < WallHeight; i++) {
                    int previousIndex = i - 1;
                    bool isExistPreviousIndex = previousIndex >= 0;
                    for (int j = 0; j < Positions.Count(); j++) {
                        var position = Positions[j]; // Get Position
                        // Room의 가장 자리에서 Height만큼의 범위에 포함되지 않는 블럭은 제외
                        if (position.x != Rect.xMin + i && position.x != (Rect.xMax - 1) - i && // LEFT, RIGHT
                            position.y != Rect.yMin + i && position.y != (Rect.yMax - 1) - i) // BOTTOM, TOP 
                            continue;

                        // 이미 해당 Height의 Dictionary에 포함된 position 제외
                        if (WallTilesDict[i].Contains(position))
                            continue;

                        // 이전 층의 Dictionary에 포함되어있다면 제외
                        if (isExistPreviousIndex && WallTilesDict[previousIndex].Contains(position))
                            continue;

                        WallTilesDict[i].Add(position);
                    }
                }
            }

            // 바닥 타일 리스트 생성
            void SetFloor() {
                Floors.Clear();
                Floors.AddRange(Positions);

                // Floor List에서 Walls Position제외
                for (int i = 0; i < WallHeight; i++) {
                    for (int j = Floors.Count - 1; j >= 0; j--) {
                        if (WallTilesDict[i].Contains(Floors[j])) {
                            Floors.Remove(Floors[j]);
                        }
                    }
                }
                
                // Floor RectInt 정의
                int xMin = Floors[0].x,
                    xMax = Floors[0].x,
                    yMin = Floors[0].y,
                    yMax = Floors[0].y;
                int diameter = 1;
                foreach (var floor in Floors) {
                    if (xMin > floor.x) xMin = floor.x;
                    if (xMax < floor.x) xMax = floor.x;
                    if (yMin > floor.y) yMin = floor.y;
                    if (yMax < floor.y) yMax = floor.y;
                }
                FloorRectInt = new RectInt(xMin, yMin, (xMax - xMin) + diameter, (yMax - yMin) + diameter);

                // Floor List에서 Edge Position제외 -> Walls Position을 제외하기 때문에 현재 필요없음
                //foreach (var keyValuePair in Edge)
                //{
                //    var valueList = keyValuePair.Value;
                //    foreach (var floor in Positions)
                //    {
                //        for (int j = 0; j < valueList.Count; j++)
                //        {
                //            if (floor == valueList[j])
                //                Floor.Remove(floor);
                //        }
                //    }
                //}
            }
            #endregion
        }

        public void SetJoint(int direction, Vector2Int position) {
            Joints.Add(new Joint {
                Direction = direction,
                Position = position
            });
        }

        public void AddGateObject(GateObject gateObject)
        {
            GateObjects.Add(gateObject);
        }

        /// <summary>
        /// Wall Tiles에서 Joint Position 제외 처리
        /// </summary>
        public void ExceptJointTilesInWallDictionary(int startWallDictIdx = 0) {
            var firstWallList = WallTilesDict[startWallDictIdx];
            for (int i = 0; i < Joints.Count; i++) {
                // 연결되지 않은 Joint는 제외 (벽이 깔려야 함)
                if (!Joints[i].Connected)
                    continue;

                firstWallList.RemoveAll(position => position == Joints[i].Position);
                for (int j = startWallDictIdx + 1; j < WallHeight; j++) {
                    var nextJointPosition = 
                        GetNextJointPosition(Joints[i].Position, Joints[i].Direction, j);
                    WallTilesDict[j].RemoveAll(position => position == nextJointPosition);
                }
            }

            // Joint의 Direction으로 다음 Joint Position 계산
            Vector2Int GetNextJointPosition(Vector2Int jointPosition, int direction, int index) {
                Vector2Int result = new Vector2Int(jointPosition.x, jointPosition.y);
                int defaultGridSizeX = 1, defaultGridSizeY = 1;
                switch (direction) {
                    case Constants.Direction.Down:  result.y -= defaultGridSizeY * index; break;
                    case Constants.Direction.Left:  result.x += defaultGridSizeX * index; break;
                    case Constants.Direction.Up:    result.y += defaultGridSizeY * index; break;
                    case Constants.Direction.Right: result.x -= defaultGridSizeX * index; break;
                    default: 
                        CatLog.ELog("This Joint Direction Not Implemented."); 
                        break;
                } return result;
            }
        }

        public IEnumerable<Joint> GetUnconnectedJoints(int direction) {
            return Joints.Where(j => j.Direction == direction && !j.Connected);
        }

        public IEnumerable<Joint> GetConnectedJoints(int direction) {
            return Joints.Where(j => j.Direction == direction && j.Connected);
        }

        public void SetRoomData(RoomType roomType, int index, RoomDataEntity entity) {
            switch (roomType) {
                case RoomType.MonsterSpawnRoom:
                    if (entity is not BattleRoomDataEntity battleRoomEntity) {
                        CatLog.ELog("RoomDataEntity Converting Error !");
                        break;
                    }
                    RoomData = new BattleRoom(this, index, battleRoomEntity);
                    break;
                case RoomType.PlayerSpawnRoom:
                    RoomData = new PlayerSpawnRoom(this, index);
                    break;
                case RoomType.ShopRoom:
                    RoomData = new ShopRoom(this, index, 0);
                    break;
                case RoomType.RewardRoom:
                    RoomData = new RewardRoom(this, index, 0);
                    break;
                case RoomType.ExitRoom:
                    RoomData = new ExitRoomInteractable(this, index);
                    break;
                case RoomType.EmptyRoom:
                    RoomData = new EmptyRoom(this, index);
                    break;
                case RoomType.BossRoom:
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(roomType), roomType, null);
            }
            // Set Gate Objects
            RoomData?.Initialize();
        }
        
        public bool IsInsideRoom(Vector2 position) {
            return FloorRectInt.IsInSide(position);
        }

        public void Dispose() {
            RoomData?.Dispose();
            RoomData = null;
        }
    }
}