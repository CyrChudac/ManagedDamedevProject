using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class TutoAreaCollider : ITutoTrigger
{
	[SerializeField]
	private string layerThatActivates = "Player";
	private bool used = false;

	public override void StartTrigger(Action whenTriggered) {
		_onTrigger.AddListener(() => whenTriggered());
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		 if(!used && collision.gameObject.layer == LayerMask.NameToLayer(layerThatActivates)) {
			_onTrigger.Invoke();
		 }
	}

	public override void End() {
		used = true;
		Destroy(gameObject);
	}
}
