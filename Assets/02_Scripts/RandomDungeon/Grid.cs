using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace RandomDungeonWithBluePrint
{
    public class Grid
    {
        public Vector2Int Size => new Vector2Int(grid.First().Count, grid.Count);
        private readonly List<List<int>> grid = new List<List<int>>();
        public int this[int x, int y] => grid[y][x]; // Getter

        public void Build(Vector2Int size, List<Room> rooms, List<Vector2Int> branches, List<Gate> gates)
        {
            MakeGrid(size.x, size.y);
            for (var i = 0; i < size.y; i++)
            {
                for (var j = 0; j < size.x; j++)
                {
                    grid[i][j] = (int)Constants.MapChipType.BackGround;
                }
            }

            foreach (var room in rooms)
            {
                foreach (var pos in room.Rect.allPositionsWithin)
                {
                    grid[pos.y][pos.x] = (int)Constants.MapChipType.Floor;
                }
            }
            
            // Make Walls
            BuildWalls(rooms);

            foreach (var branch in branches.Distinct())
            {
                grid[branch.y][branch.x] = (int)Constants.MapChipType.Branch;
            }

            foreach (var gate in gates.Distinct())
            {
                grid[gate.Position.y][gate.Position.x] = (int)Constants.MapChipType.Gate;
            }
        }

        private void MakeGrid(int x, int y)
        {
            for (var i = 0; i < y; i++)
            {
                grid.Add(new List<int>());
                for (var j = 0; j < x; j++)
                {
                    grid.Last().Add(0);
                }
            }
        }
        
        private void BuildWalls(List<Room> rooms) {
            // TODO: !for문 덜 돌릴방법 찾기
            for (int i = 0; i < rooms.Count(); i++) {
                for (int j = 0; j < rooms[i].WallHeight; j++) {
                    var wallVec2List = rooms[i].WallTilesDict[j];
                    for (int k = 0; k < wallVec2List.Count; k++) {
                        grid[wallVec2List[k].y][wallVec2List[k].x] = GetWallType(j);
                    }
                }
            }
            
            int GetWallType(int wallDictionaryIndex) {
                return wallDictionaryIndex switch {
                    0 => (int)Constants.MapChipType.WallTop,
                    1 => (int)Constants.MapChipType.WallBottom,
                    _ => (int)Constants.MapChipType.WallBottom
                };
            }
        }
    }
}