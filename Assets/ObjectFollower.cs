using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    public Rigidbody2D obj;
    [SerializeField] private Vector3 offset;
    [Range(-1,2)]
    [Tooltip("When the followed object is mooving, does this object move in the direction too? (or opposite)")]
    [SerializeField] private float forceJoinX;
    [Range(-1,2)]
    [SerializeField] private float forceJoinDown;
    [Range(-1,2)]
    [SerializeField] private float forceJoinUp;
    [SerializeField] private float speed = 1;
    [SerializeField] private float consideredDone = 0.5f;
    [Min(0.5f)]
    [SerializeField] private float dampMaxDistance = 5.0f;
    [SerializeField] private DampingType damping;
    [Range(0f,0.95f)]
    [SerializeField] private float dampingForce = 1;

    public Vector3 Offset => offset;

    private static Dictionary<DampingType, Func<float, float>> dampingDict = new Dictionary<DampingType, Func<float, float>>() {
        { DampingType.Linear, Linear },
        { DampingType.SmoothIn, SmoothIn },
        { DampingType.SmoothOut, SmoothOut },
        { DampingType.SmoothInOut, SmoothInOut }
    };
    // Start is called before the first frame update
    void Start()
    {
        transform.position = PosCompute();
    }

    // Update is called once per frame
    void Update()
    {
        direction = PosCompute() - transform.position;
        if(direction.magnitude < consideredDone) {
            direction = Vector3.zero;
        }
    }

    Vector3 direction = Vector3.zero;
    
	private void FixedUpdate() {
        var dampVal = Damp(damping, direction, dampMaxDistance);
        transform.position += direction.normalized 
            * speed 
            * Time.fixedDeltaTime 
            *(dampVal + (1 - dampVal)*(1 - dampingForce));
	}


	Vector3 PosCompute() {
        var pos = obj.transform.position + offset;
        var f = new Vector3(obj.velocity.x * forceJoinX, 
            obj.velocity.y * (obj.velocity.y < 0 ? forceJoinDown : forceJoinUp));
        return pos + f;
    }

    static float Damp(DampingType type, Vector3 difference, float dampMaxDistance) {
        var dist = Mathf.Min(dampMaxDistance, difference.magnitude);
        var d = dampingDict[type]((dist / dampMaxDistance));
        return d;
    }

    static float Linear(float input) {
        return input;
    }
    
    static float SmoothIn(float input) {
        return input * input;
    }
    static float Reverse(float input) {
        return 1 - input;
    }
    static float SmoothOut(float input) {
        return Reverse(SmoothIn(Reverse(input)));
    }
    static float SmoothInOut(float input) {
        return SmoothIn(input)*input + SmoothOut(input)*(1-input);
    }
}

public enum DampingType {
    Linear,
    SmoothIn,
    SmoothOut,
    SmoothInOut
}