using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingController : MonoBehaviour
{
	[SerializeField]
	private AnimationController animator;
	private HidingPlace current;
	private bool started = false;


	private void OnTriggerEnter2D(Collider2D other) {
		if(other.gameObject.layer == LayerMask.NameToLayer("Hider"))
			current = other.GetComponent<HidingPlace>();
	}

	private void OnTriggerExit2D(Collider2D other) {
		if(current != null && other.gameObject.GetInstanceID() == current.gameObject.GetInstanceID())
			current = null;
	}

	public bool TryHide() {
		if(started || current == null) {
			return false;
		}
		started = true;
		current.StartHide(gameObject, animator);
		return true;
	}

	public void StopHide() {
		if(! started) {
			return;
		}
		if(! current.IsOccupied) {
			current.StopHide();
		} else {
			current.GoOut();
		}
		started = false;
	}

	public bool IsHiding => current != null && current.IsOccupied;
}
