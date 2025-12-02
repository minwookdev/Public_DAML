using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "CoffeeCat/Custom Rule Tiles")]
public class CoffeeCatCustomRuleTile : RuleTile<CoffeeCatCustomRuleTile.Neighbor> {
    [Header("COFFEE CAT CUSTOM")]
    public bool IsContainsSpecifiedTilesInCheckThis = false;
    public bool IsCheckSelfInAnyCase;
    public TileBase[] SpecifiedTiles = null;
    public Tilemap[] SpecifiedTileMaps = null;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Any = 3;
        public const int Specified = 4;
        public const int Nothing = 5;

        // 추가적인 번호 지정 가능
        //public const int Custom = 5;
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.This:      return Check_This(tile);
            case Neighbor.NotThis:   return Check_NotThis(tile);
            case Neighbor.Any:       return Check_Any(tile);
            case Neighbor.Specified: return Check_Specified(tile);
            case Neighbor.Nothing:   return Check_Nothing(tile);
        }
        // Default
        return base.RuleMatch(neighbor, tile);

        bool Check_This(TileBase tile) {
            if(!IsContainsSpecifiedTilesInCheckThis) {
                return tile == this;
            }
            else {
                return SpecifiedTiles.Contains(tile) || tile == this;
            }
        }

        bool Check_NotThis(TileBase tile) {
            return tile != this;
        }

        bool Check_Any(TileBase tile) {
            if (IsCheckSelfInAnyCase) {
                return tile != null;
            }
            else {
                return tile != null && tile != this;
            }
        }

        bool Check_Specified(TileBase tile) {
            return SpecifiedTiles.Contains(tile);
        }

        bool Check_Nothing(TileBase tile) {
            return tile == null;
        }
    }
}