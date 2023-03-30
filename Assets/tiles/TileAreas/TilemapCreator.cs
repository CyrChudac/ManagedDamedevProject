using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using System.Drawing;
using Unity.VisualScripting;

public class TilemapCreator : MonoBehaviour
{
    [SerializeField]
    private int marginAround = 20;
    [SerializeField]
    private TileTypesToTiles mapping;
    [Header("Tilemaps")]
    [SerializeField]
    private Tilemap groundTilemap;
    [SerializeField]
    private Tilemap climbTilemap;
    [SerializeField]
    private Tilemap foregroundTilemap;
    [SerializeField]
    private Tilemap backgroundTilemap;
    [SerializeField]
    private Tilemap grabTilemap;
    
    public Vector3 ToWorld(Vector2Int pos) {
        var right = groundTilemap.CellToWorld(new Vector3Int(pos.x + 1, -pos.y));
        var left = groundTilemap.CellToWorld(new Vector3Int(pos.x, -pos.y));
        var result = new Vector2((right.x + left.x) / 2, (right.y + left.y) / 2);
        return result;
    }

    TileType TilePresetToTile(TilePreset tp, TileType[,] current) {
        List<TileType> possibles = new List<TileType>();
        TileType? mandatory = null;
        foreach(var item in tp) {
            bool ok = true;
            foreach(var c in item.Item2) {
                if(c.Mandatory)
                    mandatory = item.tile;
                if(!c.Evaluate(current)) {
                    ok = false;
                    break;
                }
            }
            if(mandatory.HasValue) {
                if(ok)
                    break;
                else
                    mandatory = null;
            }
            if(ok) {
                possibles.Add(item.tile);
            }
        }
        if(mandatory.HasValue)
            return mandatory.Value;
        if(possibles.Count == 0 )
            return TileType.NONE;
        return possibles[Random.Range(0, possibles.Count)];
    }

    TileBase TileBaseFromType(TileType type) {
        if(type == TileType.NONE)
            return null;
        foreach(MapTile t in mapping) {
            if (type == t.type) {
                return t.tile;
            }
        }
        return null;
    }

    TileBase[,] AreaToTiles(Layer ta) {
        TileType[,] types = new TileType[ta.Width, ta.Height];
        for(int y = 0; y < ta.Height; y++) {
            for(int x = 0; x < ta.Width; x++) {
                types[x, y] = TilePresetToTile(ta[x, y], types);
            }
        }
        TileBase[,] result = new TileBase[ta.Width, ta.Height];
        for(int x = 0; x < ta.Width; x++) {
            for(int y = 0; y < ta.Height; y++) {
                result[x, y] = TileBaseFromType(types[x, y]);
            }
        }
        return result;
    }

    void SetTile(Tilemap map, int x, int y, TileBase tile) {
        map.SetTile(new Vector3Int(x, -y), tile);
    } 

    void SetTilemapsForTileArea(int x, int y, TileArea area) {
        foreach(Layer layer in area) {
            var map = GetTileMapFromName(layer.Name);
            var tiles = AreaToTiles(layer);
            for(int x2 = 0; x2 < layer.Width; x2++) {
                for(int y2 = 0; y2 < layer.Height; y2++) {
                    if(tiles[x2,y2] != null)
                        SetTile(map, x * area.Size.x + x2,
                                y * area.Size.y + y2,
                            tiles[x2, y2]);
                }
            }
        }
    }

    Tilemap GetTileMapFromName(string name) {
        switch(name) {
            case "main":
                return groundTilemap;
            case "background":
                return backgroundTilemap;
            case "foreground":
                return foregroundTilemap;
            case "climb":
                return climbTilemap;
            case "grab":
                return grabTilemap;
            default:
                throw new System.ArgumentException("layer of name " + name + "does not exist");
        }
    }

    
    void SetTilesBlock(Tilemap t, int x, int y, int sizeX, int sizeY, TileType tileType) {
        var tile = TileBaseFromType(tileType);
        for(int i = x; i < x + sizeX; i++) {
            for(int j = y; j < y + sizeY; j++) {
                SetTile(t, i, j, tile);
            }
        }
    }

    void CreateAround(Vector2Int gridSize, Vector2Int tileSize) {
        var sX = gridSize.x * tileSize.x;
        var sY = gridSize.y * tileSize.y;
        TileType wall = TileType.WALL;
        SetTilesBlock( groundTilemap,
                - marginAround,
                - marginAround,
                marginAround,
                2*marginAround + sY,
                wall);
        SetTilesBlock( groundTilemap,
                sX,
                - marginAround,
                marginAround,
                2*marginAround + sY,
                wall);
        SetTilesBlock( groundTilemap,
                0,
                - marginAround,
                sX,
                marginAround,
                wall);
        SetTilesBlock( groundTilemap,
                0,
                sY,
                sX,
                marginAround,
                wall);
    }
    
    public void CreateGrids(TileArea[,] maze) {
        var gridSize = new Vector2Int(maze.GetLength(0), maze.GetLength(1));
        CreateAround(gridSize, maze[0,0].Size);
        for(int x = 0; x < gridSize.x; x++) {
            for(int y = 0; y < gridSize.y; y++) {
                SetTilemapsForTileArea(x, y, maze[x, y]);

            }
        }
    }

}
