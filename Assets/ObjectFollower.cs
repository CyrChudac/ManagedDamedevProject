using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    [SerializeField] private Rigidbody2D obj;
    [SerializeField] private Vector3 offset;
    [Tooltip("When the object is mooving, does the object move in the direction? (or opposite)")]
    [SerializeField] private float forceJoin;
    [SerializeField] private bool ignoreForceY;
    [SerializeField] private DampingType damping;
    [Range(0,1)]
    [SerializeField] private float dampingForce = 1;

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
        var shift = PosCompute() - transform.position;
        var damPos = Damp(damping, shift);
        transform.position += damPos + (shift - damPos)*(1 - dampingForce);
    }

    Vector3 PosCompute() {
        var pos = obj.transform.position + offset;
        var f = obj.velocity * forceJoin;
        pos += !ignoreForceY ? f : new Vector3(f.x, 0);
        return pos;
    }

    static Vector3 Damp(DampingType type, Vector3 difference) {
        float max = 20.0f;
        var dist = difference.magnitude;
        var d = dampingDict[type](1 - (dist / max));
        return difference * d;
    }

    static float Linear(float input) {
        return input * 0.1f;
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