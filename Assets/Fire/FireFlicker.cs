using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FireFlicker : MonoBehaviour
{
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D lightSource;
    [Min(0)] [SerializeField] private float max = 5;
    [Min(0)] [SerializeField] private float min = 4;
    [Min(0)] [SerializeField] private float speed = 1;
    [Min(0)] [SerializeField] private float minimal_same = 0.1f;
    [Min(0)] [SerializeField] private float maximal_same = 0.6f;

    float startTime;
    bool increasing;
    float innerToOuterRatio;

    private void Awake()
    {
        startTime = Time.timeSinceLevelLoad;
        if (lightSource == null)
        {
            if (!TryGetComponent<UnityEngine.Rendering.Universal.Light2D>(out lightSource))
                throw new UnassignedReferenceException("Light source not found");
        }
        increasing = increasing = Random.value > 0.5f;
        innerToOuterRatio = lightSource.pointLightInnerRadius / lightSource.pointLightOuterRadius;
    }

    void Update()
    {
        if (lightSource.pointLightOuterRadius >= max)
        {
            increasing = false; 
            startTime = Time.timeSinceLevelLoad;
        }
        else if (lightSource.pointLightOuterRadius <= min)
        {
            startTime = Time.timeSinceLevelLoad;
            increasing = true;
        }
        else
        {
            var ratio = Time.timeSinceLevelLoad - (startTime + minimal_same) / (maximal_same - minimal_same);
            ratio *= ratio;
            if (ratio > Random.value)
            {
                increasing = !increasing;
                startTime = Time.timeSinceLevelLoad;
            }
        }
        var modifier = increasing ? 1 : -1;
        lightSource.pointLightOuterRadius += modifier * Time.deltaTime * speed;
        lightSource.pointLightInnerRadius =
            lightSource.pointLightOuterRadius * innerToOuterRatio;
    }
}
