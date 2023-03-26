using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileMapping", menuName = "Scriptable Obj/TileTypesToTiles", order = 1)]
public class TileTypesToTiles : ScriptableObject
{
    [SerializeField]
    public List<MapTile> mapping = new List<MapTile>();
    public (TileType type, TileBase tile) this[int i] => (mapping[i].type, mapping[i].tile);
    public int Length => mapping.Count;
    public IEnumerator GetEnumerator() => mapping.GetEnumerator();

}
