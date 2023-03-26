using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.Tilemaps.Tile;
using UnityEngine.Tilemaps;
using System.Linq.Expressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GroundTile : Tile 
{
    /*
    0:
        // _ 
        //_O_
        // _ 
    1:
        // O 
        //_O_
        // _ 
    2:
        // _ 
        //_OO
        // _ 
    3:
        // O 
        //_OO
        // _ 
    4:
        // _ 
        //_O_
        // O 
    5:
        // O 
        //_O_
        // O 
    6:
        // _ 
        //_OO
        // O 
    7:
        // O 
        //_OO
        // O 
    8:
        // _ 
        //OO_
        // _ 
    9:
        // O 
        //OO_
        // _ 
    10:
        // _ 
        //OOO
        // _ 
    11:
        // O 
        //OOO
        // _ 
    12:
        // _ 
        //OO_
        // O 
    13:
        // O 
        //OO_
        // O 
    14:
        // _ 
        //OOO
        // O 
    15:
        // O 
        //OOO
        // O 
    */
    [SerializeField] 
    private SpriteList[] m_Sprites;
    [SerializeField] 
    private Sprite defaultSprite;
    
    public GroundTile() {
        colliderType = ColliderType.Grid;
    }
    // This refreshes itself and other RoadTiles that are orthogonally and diagonally adjacent
    public override void RefreshTile(Vector3Int location, ITilemap tilemap)
    {
        for (int yd = -1; yd <= 1; yd++)
            for (int xd = -1; xd <= 1; xd++)
            {
                Vector3Int position = new Vector3Int(location.x + xd, location.y + yd, location.z);
                if (HasTile(tilemap, position))
                    tilemap.RefreshTile(position);
            }
    }
    // This determines which sprite is used based on the RoadTiles that are adjacent to it and rotates it to fit the other tiles.
    // As the rotation is determined by the RoadTile, the TileFlags.OverrideTransform is set for the tile.
    public override void GetTileData(Vector3Int location, ITilemap tilemap, ref TileData tileData)
    {
        int mask = HasTile(tilemap, location + new Vector3Int(0, 1, 0)) ? 1 : 0;
        mask += HasTile(tilemap, location + new Vector3Int(1, 0, 0)) ? 2 : 0;
        mask += HasTile(tilemap, location + new Vector3Int(0, -1, 0)) ? 4 : 0;
        mask += HasTile(tilemap, location + new Vector3Int(-1, 0, 0)) ? 8 : 0;
        tileData.sprite = GetSprite((byte)mask);
        tileData.color = color;
        var m = tileData.transform;
        m.SetTRS(Vector3.zero, GetRotation((byte) mask), Vector3.one);
        tileData.transform = m;
        tileData.flags = flags;
        tileData.colliderType = colliderType;
    }
    // This determines if the Tile at the position is the same RoadTile.
    private bool HasTile(ITilemap tilemap, Vector3Int position)
    {
        return tilemap.GetTile(position) == this;
    }
    // The following determines which sprite to use based on the number of adjacent RoadTiles
    private Sprite GetSprite(byte mask)
    {
        if(mask >= 0 && mask < m_Sprites.Length && m_Sprites[mask].Count > 0) {
            return m_Sprites[mask][Random.Range(0, m_Sprites[mask].Count)];
        }
        return defaultSprite;
    }
// The following determines which rotation to use based on the positions of adjacent RoadTiles
    private Quaternion GetRotation(byte mask)
    {
        switch (mask)
        {
            case 130:
                return Quaternion.Euler(0f, 0f, -90f);
            case 131:
                return Quaternion.Euler(0f, 0f, -180f);
            case 132:
                return Quaternion.Euler(0f, 0f, -270f);
        }
        return Quaternion.Euler(0f, 0f, 0f);
    }
#if UNITY_EDITOR
// The following is a helper that adds a menu item to create a GroundTile Asset
    [MenuItem("Assets/Create/GroundTile")]
    public static void CreateGroundTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save Ground Tile", "GroundTile", "Asset", "Save Ground Tile", "Assets");
        if (path == "")
            return;
    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<GroundTile>(), path);
    }
#endif
}

[System.Serializable]
class SpriteList {
    [SerializeField]
    private List<Sprite> Sprites = new List<Sprite>();
    public Sprite this[int i] => Sprites[i];
    public int Count => Sprites.Count;
}