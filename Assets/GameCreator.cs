using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameCreator : MonoBehaviour
{
    [Tooltip("Size of the map in tiles. will be inflated to be divisible by tile area size. Must be at least [2,2].")]
    [SerializeField]
    private Vector2Int wholeSize = new Vector2Int(30,30);
    [SerializeField]
    private List<TileArea> areas;
    private Dictionary<Side, IList<TileArea>> tileAreas = new ();
    [Tooltip("when specified, only this tile area will be displayed, nothing else")]
    [SerializeField]
    private TileArea OnlyShowThis = null;
    [SerializeField]
    private EnemyCreator enemyCreator;
    [SerializeField]
    private HiderCreator hiderCreator;
    [SerializeField]
    private TilemapCreator tilemapCreator;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private List<EnderController> endingPrefabs;
    [SerializeField]
    private GameMaster gameMaster;

    /// <summary>
    /// size of one tile area that the grid consists of
    /// </summary>
    private Vector2Int tileSize;
    /// <summary>
    /// amount (and layout) of tile areas present in the grid
    /// </summary>
    private Vector2Int gridSize;

    // Start is called before the first frame update
    void Start()
    {
        if(OnlyShowThis == null) {
            DifficultyAdjust();
            InitializeDictionary();
            ComputeGridSize();
            var mazeSides = WilsonAlgo(gridSize);
            var se = FarPoints(mazeSides);
            var maze = CreateMaze(mazeSides);
            tilemapCreator.CreateGrids(maze);
            SetStartAndEnd(se, maze);
            AddEnemies(se, maze);
        } else {
            tilemapCreator.CreateGrids(new TileArea[,] { { OnlyShowThis } });
        }
    }

    public void StartEnemies() {
        enemyCreator.StartMovingAll();
    }

    private void DifficultyAdjust() {
        var diffRatio = (-1 / (Stats.Difficulity * 0.3f + 1) + 1);
        wholeSize = new Vector2Int(wholeSize.x + (int)((Stats.MapMaxSize.x - wholeSize.x) * diffRatio),
            wholeSize.y + (int)((Stats.MapMaxSize.y - wholeSize.y) * diffRatio));
    }

    void SetStartAndEnd((Vector2Int start, Vector2Int end) se, TileArea[,] maze) {
        var start = InTileArea(maze[se.start.x, se.start.y].start, se.start);
        player.transform.position = tilemapCreator.ToWorld(start);
        var end = InTileArea(maze[se.end.x, se.end.y].end, se.end);
        var e = Instantiate(endingPrefabs[Random.Range(0, endingPrefabs.Count - 1)]);
        e.SetGameMaster(gameMaster);
        e.transform.position = tilemapCreator.ToWorld(end);
    }

    void AddEnemies((Vector2Int start, Vector2Int end) se, TileArea[,] maze) {
        for(int x = 0; x < maze.GetLength(0); x++) {
            for(int y = 0; y < maze.GetLength(1); y++) {
                if((x == se.start.x && y == se.start.y) || (x == se.end.x && y == se.end.y))
                    continue;
                if(maze[x, y].enemySpawns.Count == 0)
                    continue;
                var e = enemyCreator.GetEnemy();
                var pos = maze[x, y].enemySpawns[Random.Range(0, maze[x, y].enemySpawns.Count)];
                e.transform.position = tilemapCreator.ToWorld(InTileArea(pos, new Vector2Int(x, y)));
                pos = maze[x, y].hiderLocation;
                var h = hiderCreator.GetHider();
                h.transform.position = tilemapCreator.ToWorld(InTileArea(pos, new Vector2Int(x, y)));
            }
        }
    }

    Vector2Int InTileArea(Vector2Int withinArea, Vector2Int tileAreaPos) {
        return withinArea + new Vector2Int(tileSize.x * tileAreaPos.x, tileSize.y * tileAreaPos.y);
    }


    public TileArea[,] CreateMaze(Side[,] maze) {
        TileArea[,] result = new TileArea[maze.GetLength(0), maze.GetLength(1)];
        for(int x = 0; x < gridSize.x; x++) {
            for(int y = 0; y < gridSize.y; y++) {
                if(!tileAreas.TryGetValue(maze[x, y], out var tas))
                    throw new System.IndexOutOfRangeException($"side was {maze[x, y].ToString()} ({((int)maze[x, y]).ToString()})");
                if(tas.Count == 0)
                    throw new System.IndexOutOfRangeException($"no elems for {maze[x, y].ToString()} ({((int)maze[x, y]).ToString()})");
                var ta = tas[Random.Range(0, tas.Count)];
                result[x, y] = ta;
            }
        }
        return result;
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
        tileSize = size;

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

    (Vector2Int start, Vector2Int end) FarPoints(Side[,] maze) {
        Vector2Int st;
        Vector2Int tmp;
        int i = 0;
        do {
            tmp = RandPlace(new Vector2Int(maze.GetLength(0), maze.GetLength(1)));
            st = FurthestFrom(maze, tmp);
            i++;
        } while(st == tmp && i < 100);
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
    void ComputeGridSize() {
        var x = (wholeSize.x + tileSize.x - 1) / tileSize.x;
        var y = (wholeSize.y + tileSize.y - 1) / tileSize.y;
        gridSize = new Vector2Int(x, y);
    }
    
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
        //if vertical noodle
        if(size.x == 1) {
            if(size.y == 1) {
                return new Side[1, 1] { { 0 } };
            }
            Side[,] res = new Side[size.x, size.y];
            res[0, 0] = Side.DOWN;
            for(int y = 1; y < size.y - 1; y++) {
                res[0, y] = Side.DOWN | Side.UP;
            }
            res[0, size.y - 1] = Side.UP;
            return res;
        }
        //or horizontal noodle
        if(size.y == 1) {
            Side[,] res = new Side[size.x, size.y];
            res[0, 0] = Side.RIGHT;
            for(int x = 1; x < size.x - 1; x++) {
                res[x, 0] = Side.LEFT | Side.RIGHT;
            }
            res[size.x - 1, 0] = Side.LEFT;
            return res;
        }

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
                result = GameCreator.RandPlace(size);
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
