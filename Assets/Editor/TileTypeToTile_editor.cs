using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using UnityEngine.Tilemaps;
using System;

[CustomEditor(typeof(TileTypesToTiles))]
public class TileTypeToTile_editor : Editor
{
    Tile[] dict;
    private TileType FromInt(int i) => (TileType)i;
    private string TileName(int i) => TileName(FromInt(i));
    private string TileName(TileType tt)
    {
        var str = tt.ToString();

        return str[0] + str.Substring(1).ToLower();
    }
    public void Awake()
    {
        dict = new Tile[Enum.GetValues(typeof(TileType)).Length];

        foreach (var m in ((TileTypesToTiles)target).mapping)
            dict[(int)m.type] = m.tile;
        
    }
    public override void OnInspectorGUI()
    {
        for (int i = 0; i < Enum.GetValues(typeof(TileType)).Length; i++)
        {
            dict[i] = (Tile)EditorGUILayout.ObjectField(TileName(i), dict[i], typeof(TileBase));
        }
        if (EditorGUI.EndChangeCheck())
        {
            var m = ((TileTypesToTiles)target).mapping;
            m.Clear();
            m.AddRange(dict.Select((t,i) => new MapTile() { type =(TileType)i, tile = t }));
            EditorUtility.SetDirty(target);
        }
    }
}
