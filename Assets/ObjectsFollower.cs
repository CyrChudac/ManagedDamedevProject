using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteBefore(typeof(ObjectFollower))]
public class ObjectsFollower : MonoBehaviour
{
    [SerializeField]
    private List<Rigidbody2D> items;
    [SerializeField]
    private ObjectFollower follower;
    [SerializeField]
    private Order order;
    [SerializeField]
    private float consideredReachedDistance = 1;

    private int curr = -1;
    // Start is called before the first frame update
    void Start()
    {
        NewGoal();
    }

    // Update is called once per frame
    void Update()
    {
        if(Vector3.Distance(follower.Offset + items[curr].transform.position, follower.transform.position) < consideredReachedDistance) {
            NewGoal();
        }
    }

	private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        foreach(Rigidbody2D rb in items) {
            Gizmos.DrawSphere(rb.transform.position, 0.3f);
        }
	}

	private void NewGoal() {
        if(order == Order.Sequential) {
            curr = (curr + 1) % items.Count;
        }else if(order == Order.Random) {
            curr = Random.Range(0, items.Count);
        } else {
            throw new System.NotImplementedException();
        }
        follower.obj = items[curr];
    }

    enum Order {
        Random,
        Sequential
    }
}
