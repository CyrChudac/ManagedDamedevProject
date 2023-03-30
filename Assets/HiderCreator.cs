using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiderCreator : MonoBehaviour {
    [SerializeField]
    private List<HidingPlace> hiders;
    [SerializeField]
    private float hidingTimePossibleChange = 0.25f;
    [SerializeField]
    private float unhideTimePossibleChange = 0.25f;

    public GameObject GetHider() {
        var h = Instantiate(hiders[Random.Range(0, hiders.Count)]);
        h.timeForHide *= (1 - hidingTimePossibleChange) + hidingTimePossibleChange * Random.value;
        h.timeForUnhide *= (1 - unhideTimePossibleChange) + unhideTimePossibleChange * Random.value;
        return h.gameObject;
    }
}
