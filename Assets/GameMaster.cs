using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField]
    private Transform player;

    [SerializeField]
    private TilemapCreator tilemapCreator;
    // Start is called before the first frame update
    void Start()
    {
        player.position = tilemapCreator.StartCoord;
    }
}
