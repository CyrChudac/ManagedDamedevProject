using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapRefactorer : MonoBehaviour {
    [SerializeField]
    private TileBase[] tiles;
    public void Refactor(Tilemap tilemap, Vector2Int start, Vector2Int size) {
        bool IsGround(int xI, int yI) {
            return tilemap.HasTile(new Vector3Int(xI, yI, 0));
        }
        for(int x = start.x; x < start.x + size.x; x++) {
            for(int y = 0; y < start.y + size.y; y++) {
            }
        }
    }
}