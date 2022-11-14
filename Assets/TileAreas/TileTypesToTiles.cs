using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileMapping", menuName = "Scriptable Obj/Tile Mapping", order = 1)]
public class TileTypesToTiles : ScriptableObject, IEnumerable<MapTile>
{
    public List<MapTile> mapping = new List<MapTile>();
    public (TileType type, TileBase tile) this[int i] => (mapping[i].type, mapping[i].tile);
    public int Length => mapping.Count;
    public IEnumerator GetEnumerator() => mapping.GetEnumerator();

    IEnumerator<MapTile> IEnumerable<MapTile>.GetEnumerator()
        => mapping.GetEnumerator();
}

[Serializable]
public class MapTile {
    [SerializeField]
    public TileType type;
    [SerializeField]
    public Tile tile;
}
