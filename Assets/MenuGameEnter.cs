using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class MenuGameEnter : MonoBehaviour
{
    [SerializeField]
    private UnityEvent cancelEvent;
    [SerializeField]
    private UnityEvent submitEvent;

    void Update()
    {
        if(Input.GetButtonDown("Cancel")) {
            cancelEvent.Invoke();
        }
        else if(Input.GetButtonDown("Submit")) {
            submitEvent.Invoke();
        }
    }
}
