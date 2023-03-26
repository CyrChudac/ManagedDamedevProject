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
    private Vector2Int wholeSize = new Vector2Int(100,100);
    [SerializeField]
    private int marginAround = 20;
    [SerializeField]
    private List<TileArea> areas;
    private Dictionary<Side, IList<TileArea>> tileAreas = new ();
    [SerializeField]
    private TileTypesToTiles mapping;
    [SerializeField]
    private Tile endTile;
    [Tooltip("when specified, only this tile area will be displayed, nothing else")]
    [SerializeField]
    private TileArea OnlyShowThis = null;
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
    [SerializeField]
    private Tilemap endTilemap;
    /// <summary>
    /// size of one tile area that the grid consists of
    /// </summary>
    private Vector2Int tileSize;
    /// <summary>
    /// amount (and layout) of tile areas present in the grid
    /// </summary>
    private Vector2Int gridSize;
    
    public Vector3 StartCoord { get; private set; }

    IEnumerable<T> GetAllValues<T>() where T : System.Enum {
        return System.Enum.GetValues(typeof(Side))
            .Cast<T>();
    }

    int GetFlagsMax<T>() where T : System.Enum {
        var values = System.Enum.GetValues(typeof(Side));
        return (int)(Mathf.Pow(2, values.Length) - 1);
    }

    IEnumerable<T> GetAllFlags<T>() where T : System.Enum {
        return Enumerable.Range(0, GetFlagsMax<T>() + 1).Cast<T>();
    }

    IEnumerable<Side> GetFlags(Side side) {
        return GetAllFlags<Side>().Where(s => side.HasFlag(s));
    }

    void InitializeDictionary()
    {
        Dictionary<Vector2Int, List<TileArea>> ensureAllSameSize = new();
        foreach (var area in areas) {
            ensureAllSameSize.TryAdd(area.Size, new List<TileArea>());
            ensureAllSameSize[area.Size].Add(area);
        }
        (var size, var list) = ensureAllSameSize.
            Aggregate((a, b) => (a.Value.Count > b.Value.Count) ? a : b);
        areas = list;
        tileSize= size;

        foreach(Side s in GetAllFlags<Side>()) {
            tileAreas.Add(s, new List<TileArea>());
        }
        var max = GetFlagsMax<Side>();
        foreach(TileArea a in areas) {
            int s = (int)a.openTo;
            if(s > max) {
                tileAreas[(Side)(s % (max+1))].Add(a);
                tileAreas.TryAdd(a.openTo, new List<TileArea>());
            }
            else if(s < 0) {
                tileAreas[(Side)(s + max + 1)].Add(a);
                tileAreas.TryAdd(a.openTo, new List<TileArea>());
            }
            tileAreas[a.openTo].Add(a);
        }
    }

    void Awake() {
        if(OnlyShowThis == null) {
            InitializeDictionary();
            var se = CreateGrids();
            var right = endTilemap.CellToWorld(new Vector3Int(se.start.x + 1, -se.start.y));
            var left = endTilemap.CellToWorld(new Vector3Int(se.start.x, -se.start.y));
            StartCoord = new Vector3((right.x + left.x)/2, (right.y + left.y)/2);
            SetTile(endTilemap, se.end.x, se.end.y, endTile);
        } else {
            SetTilemapsForTileArea(0, 0, OnlyShowThis);
        }
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
                        SetTile(map, x * tileSize.x + x2 - gridSize.x * tileSize.x / 2,
                                y * tileSize.y + y2 - gridSize.y * tileSize.y / 2,
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

    (Vector2Int start, Vector2Int end) FarPoints(Side[,] maze) {
        Vector2Int st;
        Vector2Int tmp;
        int i = 0;
        do {
            tmp = RandPlace(new Vector2Int(maze.GetLength(0), maze.GetLength(1)));
            st = FurthestFrom(maze, tmp);
            i++;
        } while(st == tmp || i == 1000);
        var end = FurthestFrom(maze, st);
        return (st, end);
    }

    Vector2Int FurthestFrom(Side[,] maze, Vector2Int position) {
        IEnumerable<(int x, int y)> Neighbours(int x, int y) {
            return GetAllValues<Side>().Where(s => maze[x, y].HasFlag(s))
                .Select(s => Direction(s))
                .Select(v => (v.x, v.y));
        }
        Queue<(int x, int y, int len)> que = new();
        que.Enqueue((position.x, position.y, 0));
        HashSet<(int x, int y)> seen = new();
        int max = 0;
        (int x, int y) maxPos = (position.x, position.y);
        while(que.Count > 0) {
            (int x, int y, int len) = que.Dequeue();
            if(seen.Contains((x, y)))
                continue;
            if(len > max) {
                max = len;
                maxPos = (x, y);
            }
            seen.Add((x, y));
            foreach(var n in Neighbours(x,y)) {
                que.Enqueue((x + n.x, y + n.y, len + 1));
            }
        }
        return new Vector2Int(maxPos.x, maxPos.y);
    }
    
    void SetTilesBlock(Tilemap t, int x, int y, int sizeX, int sizeY, TileType tileType) {
        var tile = TileBaseFromType(tileType);
        for(int i = x; i < x + sizeX; i++) {
            for(int j = y; j < y + sizeY; j++) {
                SetTile(t, i, j, tile);
            }
        }
    }

    void CreateAround() {
        var sX = gridSize.x * tileSize.x / 2;
        var sY = gridSize.y * tileSize.y / 2;
        TileType wall = TileType.WALL;
        SetTilesBlock( groundTilemap,
                -sX - marginAround,
                -sY - marginAround,
                marginAround,
                (marginAround + sY)*2,
                wall);
        SetTilesBlock( groundTilemap,
                sX,
                -sY - marginAround,
                marginAround,
                (marginAround + sY)*2,
                wall);
        SetTilesBlock( groundTilemap,
                -sX,
                -sY - marginAround,
                2*sX,
                marginAround,
                wall);
        SetTilesBlock( groundTilemap,
                -sX,
                sY,
                2*sX,
                marginAround,
                wall);
    }

    (Vector2Int start, Vector2Int end) CreateGrids() {
        ComputeGridSize();
        var maze = WilsonAlgo(gridSize);
        Vector2Int start = Vector2Int.zero;
        Vector2Int end = Vector2Int.zero;
        (var s, var e) = FarPoints(maze);
        CreateAround();
        for(int x = 0; x < gridSize.x; x++) {
            for(int y = 0; y < gridSize.y; y++) {
                if(!tileAreas.TryGetValue(maze[x, y], out var tas))
                    throw new System.IndexOutOfRangeException($"side was {maze[x, y].ToString()} ({((int)maze[x, y]).ToString()})");
                if(tas.Count == 0)
                    throw new System.IndexOutOfRangeException($"no elems for {maze[x, y].ToString()} ({((int)maze[x, y]).ToString()})");
                var ta = tas[Random.Range(0, tas.Count)];
                if(s.x == x && s.y == y) {
                    start = new Vector2Int(tileSize.x * s.x + ta.start.x - wholeSize.x / 2,
                        tileSize.y * s.y + ta.start.y - wholeSize.y / 2);
                }
                if(e.x == x && e.y == y) {
                    end = new Vector2Int(tileSize.x * e.x + ta.end.x - wholeSize.x / 2,
                        tileSize.y * e.y + ta.end.y - wholeSize.y / 2);
                }
                SetTilemapsForTileArea(x, y, ta);

            }
        }
        return (start, end);
    }

    void ComputeGridSize() {
        var x = (wholeSize.x + tileSize.x - 1) / tileSize.x;
        var y = (wholeSize.y + tileSize.y - 1) / tileSize.y;
        gridSize = new Vector2Int(x, y);
    }

    static Side Opposite(Side s) {
        if(s == Side.UP) return Side.DOWN;
        if(s == Side.RIGHT) return Side.LEFT;
        if(s == Side.LEFT) return Side.RIGHT;
        if(s == Side.DOWN) return Side.UP;
        throw new System.NotImplementedException();
    }

    static Vector2Int Direction(Side s) {
        if(s == Side.UP) return new Vector2Int(0,-1);
        if(s == Side.RIGHT) return new Vector2Int(1,0);
        if(s == Side.LEFT) return new Vector2Int(-1,0);
        if(s == Side.DOWN) return new Vector2Int(0,1);
        Debug.Log(s);
        throw new System.NotImplementedException();
    }
    
    static Vector2Int RandPlace(Vector2Int size) {
        int x, y;
        x = Random.Range(0, size.x);
        y = Random.Range(0, size.y);
        return new Vector2Int(x, y);
    }

    static Side[,] WilsonAlgo(Vector2Int size) {
        
        Side RandSide(Vector2Int where) {
            Side result;
            Vector2Int curr;
            do {
                var values = System.Enum.GetValues(typeof(Side));
                result = (Side)(int)Mathf.Pow(2, Random.Range(0, values.Length));
                curr = where + Direction(result);
            } while(curr.x < 0 || curr.y < 0 || curr.x >= size.x || curr.y >= size.y);
            return result;
        }

        Side RandOpposite(Side s, Vector2Int where) {
            Side to;
            do {
                to = RandSide(where);
            } while(to == Opposite(s));
            return to;
        }
        
        int cleared = 1;
        Side[,] result = new Side[size.x, size.y];
        bool[,] wasThere = new bool[size.x, size.y];
        void Set(Vector2Int vec, Side s) {
            int x = vec.x;
            int y = vec.y;
            result[x, y] = result[x, y] | s;
            wasThere[x,y] = true;
        }

        Vector2Int RandPlace() {
            Vector2Int result;
            do {
                result = TilemapCreator.RandPlace(size);
            } while(wasThere[result.x, result.y]);
            return result;
        }

        Set(RandPlace(), 0);
        Stack<ThroughNode> stack = new();
        while(cleared < size.x * size.y) {
            var start = RandPlace();
            stack.Push(new ThroughNode(RandSide(start), start, null));
            while(true) {
                var pre = stack.Peek();
                var curr = pre.Where + Direction(pre.To);
                if(wasThere[curr.x, curr.y]) {
                    Set(curr, Opposite(pre.To));
                    var act = stack.Pop();
                    while(stack.Count > 0) {
                        pre = stack.Pop();
                        Set(act.Where, act.To | Opposite(pre.To));
                        cleared++;
                        act = pre;
                    }
                    Set(act.Where, act.To);
                    cleared++;
                    break;
                }
                if(stack.FirstOrDefault(n => n.Where == curr) != null) {
                    while((pre = stack.Pop()).Where != curr) {}
                    var to = (pre.Parent != null) ? RandOpposite(pre.Parent.To, pre.Where) : RandSide(pre.Where);
                    pre = new ThroughNode(to, pre.Where, pre.Parent);
                    stack.Push(pre);
                    curr = pre.Where + Direction(pre.To);
                }
                stack.Push(new ThroughNode(RandOpposite(pre.To, curr), curr, pre));
            }
        }
        return result;
    }

    class ThroughNode {
        public Vector2Int Where { get; }
        public Side To { get; }
        public ThroughNode Parent { get; }
        public ThroughNode(Side to, Vector2Int where, ThroughNode parent) {
            this.To = to;
            this.Where = where;
            this.Parent = parent;
        }
    }
}
