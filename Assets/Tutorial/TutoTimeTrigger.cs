using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoTimeTrigger : ITutoTrigger
{
    [SerializeField]
    private float secondsToWait = 1;

	public void StartTrigger() {
		IEnumerator Coroutine() {
			yield return new WaitForSeconds(secondsToWait);
			_onTrigger.Invoke();
		}
		StartCoroutine(Coroutine());
	}

	public override void StartTrigger(Action whenTriggered) {
		IEnumerator Coroutine() {
			yield return new WaitForSeconds(secondsToWait);
			whenTriggered();
			_onTrigger.Invoke();
		}
		StartCoroutine(Coroutine());
	}

	public override void End() {
		Destroy(gameObject);
	}
}
