using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbingController : MonoBehaviour
{
	private Collider2D current;
	[SerializeField] private Rigidbody2D body;
	[SerializeField] private float speed;
	[SerializeField] private float xCorrectionTime = 0.1f;
	private float gravityScale = 0;

	private void Awake() {
		gravityScale = body.gravityScale;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if(other.gameObject.layer == LayerMask.NameToLayer("Climber"))
			current = other;
	}
	private void OnTriggerExit2D(Collider2D other) {
		if(current != null && other.GetInstanceID() == current.GetInstanceID())
			current = null;
	}

	public bool TryClimb(float value) {
		if(current == null) 
			return false;
		var x = current.bounds.center.x;
		DOTween.Sequence()
			.Append(body.transform.DOMoveX(x, xCorrectionTime));
		body.velocity = new Vector2(0, value * speed * 10);
		body.gravityScale = 0;
		IsClimbing = true;
		return true;
	}

	public void StopClimbing() {
		body.gravityScale = gravityScale;
		IsClimbing = false;
	}

	public bool IsClimbing { get; private set; }
}
