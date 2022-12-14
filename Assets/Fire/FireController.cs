using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    [SerializeField] private float extinguishTime = 0.15f;
    [SerializeField] private float lightUpTime = 0.5f;
    private Vector3 scale;
    [SerializeField] private GameObject fireObject;
    [SerializeField] private FireFlicker flicker;
    Tween tween;

    public bool IsOn => flicker.IsOn;

    public void Extinguish() {
        if(tween != null) {
            tween.Complete();
        }
        tween = fireObject.transform.DOScale(0, extinguishTime)
            .OnComplete(() => tween = null);
        flicker.Extinguish(extinguishTime);
    }

    public float LightUp() {
        if(tween != null) {
            tween.Complete();
        }
        tween = fireObject.transform.DOScale(scale, lightUpTime)
            .OnComplete(() => tween = null);
        flicker.LightUp(lightUpTime);
        return lightUpTime;
    }

    void Awake()
    {
        scale = fireObject.transform.localScale;
    }

}
