using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEditor.PackageManager.UI;
using System.ServiceModel.Security;

public class TileArea_Window : EditorWindow
{
    public TileArea editing;
    private Vector2Int size;
    private Vector2Int start;
    private Vector2Int end;
    private float maxCharHeight;
    private float maxCharWidth;
    private float minJump;
    private string copyName = "copy";
    IList<(GridTileDrawer Drawer, Layer layer)> values;
    Enum openTo;
    private int currDrawer = 0;
    private string[] DrawerNames
        => values.Select(l => l.layer.Name).ToArray();
    private static readonly int TILE_SIZE = 50;

    private void Initialize() {
        values = editing.Select(l => (new GridTileDrawer(TILE_SIZE), l)).ToList();
        openTo = editing.openTo;
        start = editing.start;
        end = editing.end;
        maxCharHeight = editing.maxCharacterHeight;
        minJump = editing.minJumpSize;
        maxCharWidth = editing.maxCharacterWidth;

        Resize(editing.Size);

        void AddTiles(GridTileDrawer drawer, Layer layer) {
            for(int x = 0; x < size.x; x++) {
                for(int y = 0; y < size.y; y++) {
                    foreach(var (type, conds) in layer[x, y]) {
                        drawer.Values[x, y].Add((type, 
                            conds.Select(c => new CondWrapper(c.Where, c.What, c.Positive, c.Mandatory)).ToList()));
                    }
                }
            }
        }
        foreach(var (d, l) in values) {
            AddTiles(d, l);
        }
    }

    public static void Open(TileArea obj)
    {
        TileArea_Window window = GetWindow<TileArea_Window>("Tile Area Editor");
        window.editing = obj;
        window.Initialize();
    }
    void Resize(Vector2Int s) {
        size = s;
        foreach(var layer in values) {
            layer.Drawer.Resize(s);
        }
    }

    private void CreateCopy() {
        Save();
        var ta = ScriptableObject.CreateInstance<TileArea>();
        editing.CopyInto(ta);
        EditorUtility.SetDirty(ta);
        string path = AssetDatabase.GetAssetPath(editing);
        int dotInd = path.LastIndexOf('.');
        int sepInd = path.LastIndexOf('\\');
        if(sepInd < 0) {
            sepInd = path.LastIndexOf('/');
        }
        path = path.Substring(0, ++sepInd) + copyName + path.Substring(dotInd);
        AssetDatabase.CreateAsset(ta, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
    }

    private void Save() {
        void SaveProperties(Layer layer, GridTileDrawer drawer) {
            TilePreset[,] preset = new TilePreset[size.x, size.y];
            for(int x = 0; x < size.x; x++) {
                for(int y = 0; y < size.y; y++) {
                    preset[x, y] = new TilePreset();
                    foreach(var (type, conds) in drawer.Values[x, y]) {
                        preset[x, y].Add((type, conds
                            .Select(c => new Condition(c.Where, c.Positive, c.Mandatory, c.What))
                            .ToList()));
                    }
                }
            }
            layer.Tiles = preset;
        }
        foreach(var (d, l) in values) {
            SaveProperties(l, d);
        }

        editing.start = start;
        editing.end = end;
        editing.maxCharacterHeight = maxCharHeight;
        editing.maxCharacterWidth = maxCharWidth;
        editing.minJumpSize = minJump;
        editing.openTo = (Side)openTo;

        EditorUtility.SetDirty(editing);
    }

    private void OnDestroy() {
        Save();
    }

    public void OnGUI() {
        GUILayout.BeginHorizontal();
            GUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));
            
            var size2 = EditorGUILayout.Vector2IntField("size", size);
            Resize(size2);
            start = EditorGUILayout.Vector2IntField("start", start);
            end = EditorGUILayout.Vector2IntField("end", end);
            currDrawer = GUILayout.SelectionGrid(currDrawer, DrawerNames, 3);
            openTo = EditorGUILayout.EnumFlagsField("open sides", openTo);
            minJump = EditorGUILayout.FloatField("min jump size", minJump);
            maxCharHeight = EditorGUILayout.FloatField("max character height", maxCharHeight);
            maxCharWidth = EditorGUILayout.FloatField("max character width", maxCharWidth);
            if(GUILayout.Button("create copy")) {
                CreateCopy();
            }
            copyName = EditorGUILayout.TextField("copy name", copyName);

            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
                GUILayout.BeginHorizontal("body", GUILayout.MaxHeight((size.y + 2) * TILE_SIZE));
                values[currDrawer].Drawer.DrawBody();
                GUILayout.EndHorizontal();

                if(currDrawer == 0) {
                    GUILayout.BeginHorizontal();
                    values[currDrawer].Drawer.DrawWallify();
                    GUILayout.EndHorizontal();
                }

                GUILayout.BeginVertical("box");
                values[currDrawer].Drawer.DrawConditions();
                GUILayout.EndVertical();
            GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}

class GridTileDrawer {
    
    private Vector2Int? selected = null;
    private Vector2Int size;
    private Vector2Int realSize;
    private int _tileSize;
    public List<(TileType type, List<CondWrapper> cond)>[,] Values { get; private set; } = 
        new List<(TileType type, List<CondWrapper> cond)>[0,0];

    public GridTileDrawer(int tileSize) {
        this._tileSize = tileSize;
    }

    private void SetBorderToWall() {
        TileType wall = TileType.WALL;
        void TryAddWall(int x, int y) {
            if(x < 0 || y < 0 || x >= size.x || y >= size.y)
                return;
            if(Values[x, y] == null)
                Values[x, y] = new();
            if(!Values[x, y].Any(t => t.type == wall))
                Values[x, y].Add((wall, new List<CondWrapper>()));
        }
        for(int x = 0; x < size.x; x++) {
            TryAddWall(x, 0);
            TryAddWall(x, size.y - 1);
        }
        for(int y = 1; y < size.y - 1; y++) {
            TryAddWall(0, y);
            TryAddWall(size.x - 1, y);
        }
    }

    public void Select(Vector2Int? target)
        => selected = target;

    public void DrawBody() {
        var basicColor = GUI.backgroundColor;
        Dictionary<TileType, Color> buttonColors = new() {
            {TileType.NONE, Color.cyan },
            {TileType.WALL, Color.grey },
            {TileType.BREAKING, Color.white },
            {TileType.LIAN, Color.gray },
            {TileType.STICKY, Color.yellow },
            {TileType.GRASS, Color.green },
            {TileType.ROPE, Color.green },
        };

        bool DrawButton(int x, int y, TileType tile){
            if(buttonColors.TryGetValue(tile, out var color))
                GUI.backgroundColor = color;
            bool result = GUILayout.Button($"({x},{y})", GUILayout.Width(_tileSize * 0.9f),
                GUILayout.Height(_tileSize * 0.9f));
            GUI.backgroundColor = basicColor;
            return result;
        };
        TileType SmartTileSelection(List<(TileType, List<CondWrapper>)> val) {
            int min = int.MaxValue;
            TileType result = TileType.NONE;
            foreach(var v in val) {
                if(v.Item1 != TileType.NONE && v.Item2.Count < min) {
                    min = v.Item2.Count;
                    result = v.Item1;
                }
            }
            return result;
        }
        for(int x = 0; x < size.x; x++) {
            GUILayout.BeginVertical(GUILayout.MaxWidth(_tileSize));
            for(int y = 0; y < size.y; y++) {
                GUILayout.BeginHorizontal(GUILayout.MinHeight(_tileSize));
                TileType type = SmartTileSelection(Values[x, y]);
                if(DrawButton(x, y, type)) {
                    selected = new Vector2Int(x, y);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
    }

    public void DrawWallify() {
        if(GUILayout.Button("add walls to borders")) {
            SetBorderToWall();
        }
    }
    public void Resize(Vector2Int futureSize) {
        if(futureSize == size)
            return;
        if(futureSize.x < realSize.x && futureSize.y < realSize.y) {
            size = futureSize;
            return;
        }
        int x = math.max(realSize.x, futureSize.x);
        int y = math.max(realSize.y, futureSize.y);
        List<(TileType type, List<CondWrapper> cond)>[,] tmp = 
            new List<(TileType type, List<CondWrapper> cond)>[x, y];
        for(int i = 0; i < Values.GetLength(0); i++) {
            for(int j = 0; j < Values.GetLength(1); j++) {
                tmp[i, j] = Values[i, j];
            }
        }
        for(int i = Values.GetLength(0); i < x; i++) {
            for(int j = 0; j < y; j++) {
                tmp[i, j] = new List<(TileType type, List<CondWrapper> cond)>();
            }
        }
        for(int i = 0; i < Values.GetLength(0); i++) {
            for(int j = Values.GetLength(1); j < y; j++) {
                tmp[i, j] = new List<(TileType type, List<CondWrapper> cond)>();
            }
        }
        Values = tmp;
        realSize = new Vector2Int(x, y);
        size = futureSize;
    }

    public void DrawConditions() {
        if(selected == null)
            return;
        GUILayout.BeginHorizontal();
        GUILayout.Label($"({selected.Value.x},{selected.Value.y})");
        GUILayout.EndHorizontal();
        for(int i = 0; i < Values[selected.Value.x, selected.Value.y].Count; i++) {
            var val = Values[selected.Value.x, selected.Value.y][i];
            var heightMult = math.max(1, Values[selected.Value.x, selected.Value.y][i].cond.Count);
            GUILayout.BeginHorizontal(GUILayout.Height(heightMult * 30));
            GUILayout.BeginVertical(GUILayout.Width(45));
            if(GUILayout.Button("-", GUILayout.Width(40), GUILayout.Height(20))) {
                Values[selected.Value.x, selected.Value.y].RemoveAt(i);
                i--;
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                continue;
            }
            GUILayout.EndVertical();
            int curr = 0;
            int sum = 0;
            var tmp = Enum.GetValues(typeof(TileType))
                    .Cast<TileType>().ToList();
            List<TileType> innerValues = new();
            Dictionary<int, int> offset = new();
            Dictionary<int, int> backOffset = new();
            for(int j = 0; j < tmp.Count; j++) {
                TileType t = tmp[j];
                if(t == val.type || !Values[selected.Value.x, selected.Value.y]
                        .Select(tc => tc.type)
                        .Contains(t)) {
                    offset.Add(curr++, sum);
                    innerValues.Add(t);
                } else {
                    sum++;
                }
                backOffset.Add(j, sum);
            }
            GUILayout.BeginVertical(GUILayout.MaxWidth(180));
            int typ = EditorGUILayout.Popup(
                (int)val.type - backOffset[(int)val.type], 
                innerValues
                    .Select(t => t.ToString())
                    .ToArray());
            typ += offset[typ];
            Values[selected.Value.x, selected.Value.y][i] = ((TileType)typ, val.cond);
            GUILayout.EndVertical();
            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.MaxWidth(180));
            for(int j = 0; j < Values[selected.Value.x, selected.Value.y][i].cond.Count; j++) {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(45));
                if(GUILayout.Button("-", GUILayout.Width(40), GUILayout.Height(20))) {
                    Values[selected.Value.x, selected.Value.y].RemoveAt(j);
                    j--;
                    continue;
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(100));
                var vec = Values[selected.Value.x, selected.Value.y][i].cond[j].Where;
                vec = EditorGUILayout.Vector2IntField("pos", vec);
                Values[selected.Value.x, selected.Value.y][i].cond[j].Where = vec;
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(100));
                var type = Values[selected.Value.x, selected.Value.y][i].cond[j].What;
                int typeIndex = EditorGUILayout.Popup((int)type,
                    Enum.GetValues(typeof(TileType)).Cast<TileType>().Select(t => t.ToString()).ToArray());
                Values[selected.Value.x, selected.Value.y][i].cond[j].What = (TileType)typeIndex;
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(100));
                var positive = Values[selected.Value.x, selected.Value.y][i].cond[j].Positive;
                positive = EditorGUILayout.Toggle("positive", positive);
                Values[selected.Value.x, selected.Value.y][i].cond[j].Positive = positive;
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(100));
                var mandatory = Values[selected.Value.x, selected.Value.y][i].cond[j].Mandatory;
                mandatory = EditorGUILayout.Toggle("mandatory", mandatory);
                Values[selected.Value.x, selected.Value.y][i].cond[j].Mandatory = mandatory;
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
            if (GUILayout.Button(new GUIContent("+", "Add Condition."))) {
                Values[selected.Value.x, selected.Value.y][i].cond.Add(new CondWrapper(Vector2Int.zero, TileType.NONE, true, false));
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
        if(Values[selected.Value.x, selected.Value.y].Count < Enum.GetValues(typeof(TileType)).Length) {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("+", "Add item."))) {
                Values[selected.Value.x, selected.Value.y].Add((
                    Enum.GetValues(typeof(TileType))
                        .Cast<TileType>()
                        .Where(t => !Values[selected.Value.x, selected.Value.y]
                            .Select(tc => tc.type)
                            .Contains(t))
                        .First()
                    , new List<CondWrapper>()));
            }
            GUILayout.EndHorizontal();
        }
    }
}

public class CondWrapper {
    public Vector2Int Where { get; set; }
    public TileType What { get; set; }
    public bool Positive { get; set; }
    public bool Mandatory { get; set;  }
    public CondWrapper(Vector2Int where, TileType what, bool positive, bool mandatory) {
        Where = where;
        What = what;
        Positive = positive;
        Mandatory = mandatory;
    }
}