using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public struct MapTile {
    [SerializeField]
    public TileType type;
    [SerializeField]
    public Tile tile;
}
