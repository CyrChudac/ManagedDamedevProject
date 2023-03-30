using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class ITutoTrigger : MonoBehaviour
{
	[SerializeField]
	protected UnityEvent _onTrigger;
    public abstract void StartTrigger(Action whenTriggered);
    public abstract void End();
}
