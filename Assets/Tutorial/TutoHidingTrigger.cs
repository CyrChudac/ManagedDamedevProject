using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class TutoHidingTrigger : ITutoTrigger
{
    [SerializeField]
    private HidingPlace hider;

	public override void StartTrigger(Action whenTriggered) {
		_onTrigger.AddListener(() => whenTriggered());
		hider.onHidden.AddListener(() => CanGo(this, _onTrigger));
	}

	private static void CanGo(TutoHidingTrigger me, UnityEvent what){
		if(!me.IsDestroyed()) {
			what.Invoke();
		}
	}

	public override void End() {
		Destroy(gameObject);
	}
}
