using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public class FireFlicker : MonoBehaviour
{
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D lightSource;
    [Min(0)] [SerializeField] private float max = 5;
    [Min(0)] [SerializeField] private float min = 4;
    [Min(0)] [SerializeField] private float speed = 1;
    [Min(0)] [SerializeField] private float minimal_same = 0.1f;
    [Min(0)] [SerializeField] private float maximal_same = 0.6f;
    [SerializeField] private EnemyVision visionDistance;
    [Range(-1,1)]
    [SerializeField] private float visionModifier;

    public bool IsOn { get; private set; } = true;

    float startTime;
    bool increasing;
    float innerToOuterRatio;
    float currMax;
    float currMin;
    float addingDuration = -1;
    float shrinkingDuration = -1;

    public void Extinguish(float duration) {
        addingDuration = -1;
        shrinkingDuration = duration;
        return;
    }
    public void LightUp(float duration) {
        shrinkingDuration = -1;
        addingDuration = duration;
        return;
    }

    private void Awake()
    {
        startTime = Time.timeSinceLevelLoad;
        if (lightSource == null)
        {
            if (!TryGetComponent<UnityEngine.Rendering.Universal.Light2D>(out lightSource))
                throw new UnassignedReferenceException("Light source not found");
        }
        currMax = max;
        currMin = min;
        increasing = increasing = Random.value > 0.5f;
        innerToOuterRatio = lightSource.pointLightInnerRadius / lightSource.pointLightOuterRadius;
    }

    void Update()
    {
        ShrinkAndAdd();
        float value;
        if (lightSource.pointLightOuterRadius > currMax)
        {
            increasing = false;
            value = currMax;
            startTime = Time.timeSinceLevelLoad;
        }
        else if (lightSource.pointLightOuterRadius < currMin)
        {
            startTime = Time.timeSinceLevelLoad;
            value = currMin;
            increasing = true;
        }
        else
        {
            var ratio = (Time.timeSinceLevelLoad - startTime + minimal_same) / (maximal_same - minimal_same);
            ratio *= ratio;
            if (ratio > Random.value)
            {
                increasing = !increasing;
                startTime = Time.timeSinceLevelLoad;
            }
            var modifier = increasing ? 1 : -1;
            value = lightSource.pointLightOuterRadius + modifier * Time.deltaTime * speed;
        }
        lightSource.pointLightOuterRadius = value;
        lightSource.pointLightInnerRadius =
            lightSource.pointLightOuterRadius * innerToOuterRatio;
        if(visionDistance != null) {
            var inner = lightSource.pointLightInnerRadius;
            var outer = lightSource.pointLightOuterRadius;
            var dist = inner + (outer - inner) * (visionModifier + 1) / 2;
            visionDistance.SetViewDistance(dist);
        }

    }

    void ShrinkAndAdd() {
        if(shrinkingDuration > 0) {
            var minDif = min * Time.deltaTime / shrinkingDuration;
            var maxDif = max * Time.deltaTime / shrinkingDuration;
            currMin = Mathf.Max(0, currMin - minDif);
            currMax = Mathf.Max(0, currMax - maxDif);
            if(currMin == 0 && currMax == 0) {
                IsOn = false;
                shrinkingDuration = -1;
            }
        }else if(addingDuration > 0) {
            var minDif = min * Time.deltaTime / addingDuration;
            var maxDif = max * Time.deltaTime / addingDuration;
            currMin = Mathf.Min(min, currMin + minDif);
            currMax = Mathf.Min(max, currMax + maxDif);
            if(currMin == min && currMax == max) {
                IsOn = true;
                addingDuration = -1;
            }
        }
    }
}
