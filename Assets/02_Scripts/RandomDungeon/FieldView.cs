using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RandomDungeonWithBluePrint
{
    public class FieldView : MonoBehaviour
    {
        [SerializeField] private Tilemap backgroundTilemap = default;
        [SerializeField] private Tilemap floorTilemap = default;
        [SerializeField] private Tilemap wallTilemap = default;
        [SerializeField] private Tilemap branchTilemap = default;

        [SerializeField] private RuleTile floorTile = default;
        [SerializeField] private RuleTile wallBottomRuleTile = default;
        [SerializeField] private RuleTile wallTopRuleTile = default;
        [SerializeField] private RuleTile branchRuleTile = default;
        [SerializeField] private Tile backgroundTile = default;
        [SerializeField] private Tile branchTile = default;

        public void DrawDungeon(Field field)
        {
            AllTilemapClear();

            for (var x = 0; x < field.Grid.Size.x; x++)
            {
                for (var y = 0; y < field.Grid.Size.y; y++)
                {
                    switch (field.Grid[x, y])
                    {
                        case (int)Constants.MapChipType.BackGround:
                            backgroundTilemap.SetTile(new Vector3Int(x, y, 0), backgroundTile);
                            break;
                        case (int)Constants.MapChipType.WallBottom:
                            wallTilemap.SetTile(new Vector3Int(x, y, 0), wallBottomRuleTile);
                            break;
                        case (int)Constants.MapChipType.WallTop:
                            wallTilemap.SetTile(new Vector3Int(x, y, 0), wallTopRuleTile);
                            break;
                        case (int)Constants.MapChipType.Floor:
                            floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                            break;
                        case (int)Constants.MapChipType.Branch:
                            branchTilemap.SetTile(new Vector3Int(x, y, 0), branchRuleTile);
                            break;
                        case (int)Constants.MapChipType.Gate:
                            floorTilemap.SetTile(new Vector3Int(x, y, 0), floorTile);
                            break; 
                    }
                }
            }
        }

        public void AllTilemapClear()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).GetComponent<Tilemap>().ClearAllTiles();
            }
        }
    }
}
