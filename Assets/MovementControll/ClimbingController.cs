using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ClimbingController : MonoBehaviour {
	private Collider2D current;
	private Tilemap currentMap;
	[SerializeField] private Rigidbody2D body;
	[SerializeField] private float speed;
	[SerializeField] private float xCorrectionTime = 0.1f;
	private float gravityScale = 0;

	private void Awake() {
		gravityScale = body.gravityScale;
	}

	private void OnTriggerEnter2D(Collider2D other) {
		if(other.gameObject.layer == LayerMask.NameToLayer("Climber")) {
			current = other;
			currentMap = current.GetComponent<Tilemap>();
		}
	}
	private void OnTriggerExit2D(Collider2D other) {
		if(current != null && other.GetInstanceID() == current.GetInstanceID()) {
			current = null;
			currentMap = null;
		}
	}

	public bool TryClimb(float value) {
		if(current == null)
			return false;
		MiddleClimbMotion();
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

	private bool MiddleClimbMotion() {
		if(currentMap == null)
			return false;
		var cw = CollidingWith(currentMap);
		var xs = cw.Select(c => c.x).Distinct();
		if(xs.Count() != 1)
			return false;
		var x = currentMap.CellToWorld(new Vector3Int(xs.First(), 0)).x;
		DOTween.Sequence()
			.Append(body.transform.DOMoveX(x, xCorrectionTime));
		return true;
	}

	List<Vector2Int> CollidingWith(Tilemap map) {
		return new List<Vector2Int>();
	}
}
