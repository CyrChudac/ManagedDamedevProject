using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TutoSlideShow : MonoBehaviour
{
    [Header("There must always be 1 more note than collider!")]
    [Tooltip("Game objects that informs the player how to play on PC.")]
    [SerializeField]
    private List<GameObject> notesPc;
    [Tooltip("Game objects that informs the player how to play on mobile.")]
    [SerializeField]
    private List<GameObject> notesMobile;
    [Tooltip("Objects, that trigger message change.")]
    [SerializeField]
    private List<ITutoTrigger> colliders;

    private List<GameObject> notes;
    private int currIndex = 0;
    void Start()
    {
        colliders[0].StartTrigger(Proceed);

        for(int i = 0; i < notesMobile.Count; i++) {
            notesMobile[i].SetActive(false);
        }
        for(int i = 0; i < notesPc.Count; i++) {
            notesPc[i].SetActive(false);
        }
#if UNITY_ANDROID
        notes = notesMobile;
#else
        notes = notesPc;
#endif
        notes[0].SetActive(true);
    }

    void Proceed() {
        notes[currIndex].SetActive(false);
        colliders[currIndex].End();
        currIndex++;
        notes[currIndex].SetActive(true);
        if(currIndex < colliders.Count) {
            colliders[currIndex].StartTrigger(Proceed);
        }
    }

}
