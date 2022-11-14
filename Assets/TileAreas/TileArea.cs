using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Tilemaps;
using System.Dynamic;
using System.Runtime.CompilerServices;

[System.Flags]
public enum Side { UP=1, DOWN=2, LEFT=4, RIGHT=8 } 
public enum TileType { NONE, WALL, BREAKING, GRASS, LIAN, ROPE, STICKY}

[CreateAssetMenu(
    fileName = "TileArea", 
    menuName = "Scriptable Obj/Tile Area", 
    order = 0)]
public class TileArea : ScriptableObject, IEnumerable<Layer>
{
    public Side openTo;
    public Vector2Int start;
    public Vector2Int end;

    public Layer tiles = new Layer("main");
    public Layer backgroundTiles = new Layer("background");
    public Layer foregroundTiles = new Layer("foreground");
    public Layer climbTiles = new Layer("climb");
    public Layer grabTiles = new Layer("grab");

    public IEnumerator<Layer> GetEnumerator() {
        yield return tiles;
        yield return backgroundTiles;
        yield return foregroundTiles;
        yield return climbTiles;
        yield return grabTiles;
    }

    public float maxCharacterHeight = 2;
    public float minJumpSize = 1;
    public float maxCharacterWidth = 2;

    public int Width => tiles.Width;
    public int Height => tiles.Height;
    public Vector2Int Size => new Vector2Int(Width, Height);
    public TilePreset this[int x, int y] => tiles[x,y];
    public TilePreset this[Vector2Int v] => tiles[v.x, v.y];

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void CopyInto(TileArea ta) {
        ta.openTo = openTo;
        ta.start = new Vector2Int(start.x, start.y);
        ta.end = new Vector2Int(end.x, end.y);
        ta.maxCharacterHeight = maxCharacterHeight;
        ta.minJumpSize = minJumpSize;
        ta.maxCharacterWidth = maxCharacterWidth;
        ta.tiles = tiles.Copy();
        ta.backgroundTiles = backgroundTiles.Copy();
        ta.foregroundTiles = foregroundTiles.Copy();
        ta.climbTiles = climbTiles.Copy();
        ta.grabTiles = grabTiles.Copy();
    }
}

[Serializable]
public class Layer {
    [SerializeField]
    private TilePreset[] innerTiles = new TilePreset[0];

    [SerializeField] public string Name { get; }
    [SerializeField] public int Width { get; private set; }
    [SerializeField] public int Height { get; private set; }
    public Layer(string name) {
        this.Name = name;
    }
    
    private int GetIndex(int x, int y)
        => x * Height + y;

    public TilePreset[,] Tiles {
        set {
            Width = value.GetLength(0);
            Height = value.GetLength(1);
            innerTiles = new TilePreset[Width * Height];
            for(int x = 0; x < Width; x++) {
                for(int y = 0; y < Height; y++) {
                    innerTiles[GetIndex(x,y)] = value[x, y];
                }
            }
        }
    }

    public TilePreset this[Vector2Int vec] {
        get => this[vec.x, vec.y]; 
        set => this[vec.x, vec.y] = value;
    }

    public TilePreset this[int x, int y] {
        get => innerTiles[GetIndex(x, y)];
        set => innerTiles[GetIndex(x, y)] = value;
    }

    public Layer Copy() {
        Layer res = new Layer(Name);
        res.Tiles = new TilePreset[Width, Height];
        for(int x = 0; x < Width; x++) {
            for(int y = 0; y < Height; y++) {
                res[x, y] = this[x, y].Copy();
            }
        }
        res.Width = Width;
        res.Height = Height;
        return res;
    }
}

[Serializable]
class ConditionList : IList<Condition> {
    private ConditionList(List<Condition> list)
        => conditions = list;

    [SerializeField] List<Condition> conditions = new List<Condition>();

    public static implicit operator ConditionList(List<Condition> list)
        => new ConditionList(list);

    public static implicit operator List<Condition>(ConditionList list)
        => list.conditions;
    
    public static bool operator ==(ConditionList left, List<Condition> right) {
        return (List<Condition>)left == right;
    }
    public static bool operator !=(ConditionList left, List<Condition> right) {
        return ! (left == right);
    }
    public static bool operator ==(List<Condition> left, ConditionList right) {
        return left == (List<Condition>)right;
    }
    public static bool operator !=(List<Condition> left, ConditionList right) {
        return ! (left == right);
    }

	#region IList implementation
	public int IndexOf(Condition item) {
		return ((IList<Condition>)conditions).IndexOf(item);
	}

	public void Insert(int index, Condition item) {
		((IList<Condition>)conditions).Insert(index, item);
	}

	public void RemoveAt(int index) {
		((IList<Condition>)conditions).RemoveAt(index);
	}

	public Condition this[int index] { get => ((IList<Condition>)conditions)[index]; set => ((IList<Condition>)conditions)[index] = value; }

	public void Add(Condition item) {
		((ICollection<Condition>)conditions).Add(item);
	}

	public void Clear() {
		((ICollection<Condition>)conditions).Clear();
	}

	public bool Contains(Condition item) {
		return ((ICollection<Condition>)conditions).Contains(item);
	}

	public void CopyTo(Condition[] array, int arrayIndex) {
		((ICollection<Condition>)conditions).CopyTo(array, arrayIndex);
	}

	public bool Remove(Condition item) {
		return ((ICollection<Condition>)conditions).Remove(item);
	}

	public int Count => ((ICollection<Condition>)conditions).Count;

	public bool IsReadOnly => ((ICollection<Condition>)conditions).IsReadOnly;

	public IEnumerator<Condition> GetEnumerator() {
		return ((IEnumerable<Condition>)conditions).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return ((IEnumerable)conditions).GetEnumerator();
	}
	#endregion
}


[Serializable]
public class TilePreset : IList<(TileType tile, List<Condition>)>
{
    [SerializeField]
    private List<TileType> tiles = new();
    [SerializeField]
    private List<ConditionList> conditions = new();
    public void Add(TileType tile, List<Condition> conditions)
        => Add((tile, conditions));
    public TilePreset Copy() {
        var res = new TilePreset();
        foreach(var item in this) {
            List<Condition> cl = new List<Condition>();
            foreach(var c in item.Item2) {
                cl.Add(c.Copy());
            }
            res.Add(item.tile, cl);
        }
        return res;
    }

	public int IndexOf((TileType tile, List<Condition>) item) {
		return tiles.IndexOf(item.tile);
	}

	public void Insert(int index, (TileType tile, List<Condition>) item) {
		tiles.Insert(index, item.tile);
		conditions.Insert(index, item.Item2);
	}

	public void RemoveAt(int index) {
		tiles.RemoveAt(index);
		conditions.RemoveAt(index);
	}

	public (TileType tile, List<Condition>) this[int index] { 
        get => (tiles[index], conditions[index]); 
        set {
            tiles[index] = value.tile;
            conditions[index] = value.Item2;
        } 
    }

	public void Add((TileType tile, List<Condition>) item) {
		tiles.Add(item.tile);
		conditions.Add(item.Item2);
	}

	public void Clear() {
		tiles.Clear();
		conditions.Clear();
	}

	public bool Contains((TileType tile, List<Condition>) item) {
        int i = tiles.IndexOf(item.tile);
        return (i >= 0) && conditions[i] == item.Item2;
	}

	public void CopyTo((TileType tile, List<Condition>)[] array, int arrayIndex) {
        List<(TileType tile, List<Condition>)> repr = new(Count);
        for(int i = 0; i < Count; i++) {
            repr.Add((tiles[i], conditions[i]));
        }
        repr.CopyTo(array, arrayIndex);
	}

	public bool Remove((TileType tile, List<Condition>) item) {
        for(int i = 0; i < Count; i++) {
            if(tiles[i] == item.tile && conditions[i] == item.Item2) {
                tiles.RemoveAt(i);
                conditions.RemoveAt(i);
                return true;
            }
        }
        return false;
	}

	public int Count => tiles.Count;

	public bool IsReadOnly => false;

	public IEnumerator<(TileType tile, List<Condition>)> GetEnumerator() {
        for(int i = 0; i < Count; i++) {
            yield return (tiles[i], conditions[i]);
        }
	}

	IEnumerator IEnumerable.GetEnumerator() {
		return GetEnumerator();
	}
}

[Serializable]
public class Condition
{
    public Condition(Vector2Int where, bool positive, TileType what)
    {
        this.Where = where;
        this.Positive = positive;
        this.What = what;
    }
    [SerializeField]
    public Vector2Int Where { get; }
    [SerializeField]
    public bool Positive { get; }
    [SerializeField]
    public TileType What { get; }

    public virtual bool Evaluate(TileType[,] tiles)
    {
        var tile = tiles[Where.x, Where.y];
        return (Positive && tile == What) || (!Positive && tile != What);
    }
    public Condition Copy() {
        return new Condition(new Vector2Int(Where.x, Where.y), Positive, What);
    }
}