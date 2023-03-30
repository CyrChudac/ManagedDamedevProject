using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAfter(typeof(GameCreator))]
[ExecuteAfter(typeof(ObjectFollower))]
public class CameraStarter : MonoBehaviour
{
    [SerializeField] 
    private GameObject player;
    [SerializeField] 
    private float distance = 10;
    [SerializeField] 
    private GameObject camera;
    // Start is called before the first frame update
    void Start()
    {
        var dir = new Vector3((Random.value - 0.5f) * 200, (Random.value-0.5f) * 200);
        dir.Normalize();
        camera.transform.position = player.transform.position + dir * distance;
        Destroy(this);
    }

}
