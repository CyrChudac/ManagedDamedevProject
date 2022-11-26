using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float Speed = 2f;
    [SerializeField] private CharacterController2D controller;
	[SerializeField] private Transform frontGroundCheck;
	[SerializeField] private float edgeWaitingTime = 0.5f;
	[SerializeField] private float stopBeforeWall = 0.5f;
	[SerializeField] private SpriteRenderer sizeProvider;
	[SerializeField] private bool stopsOnEdge = true;

	private float m_EdgeTimeStart = float.MinValue;
	private bool m_waiting = false;
	private float m_goingTo = 1;
	private GameObject frontWallCheck;

	private void Start() {
		IEnumerator Routine() {
			GameObject go = new GameObject("frontWallCeck");
			go.transform.SetParent(transform, false);
			frontWallCheck = go;
			yield return new WaitForEndOfFrame();
			Vector3 offset = sizeProvider == null ? Vector3.zero : new Vector3(sizeProvider.bounds.size.x / 2, 0);
			go.transform.localPosition = offset + Vector3.right * stopBeforeWall;
		}
		StartCoroutine(Routine());
	}

	private void FixedUpdate() {
		float speed = m_waiting ? 0 : Speed * m_goingTo;
		controller.Move(speed, false, false, flipping: false);
	}

	private void Update() {
		if(m_waiting) {
			if(m_EdgeTimeStart + edgeWaitingTime < Time.timeSinceLevelLoad) {
				m_waiting = false;
				controller.Flip();
				m_goingTo *= -1;
			}
		} else if(controller.Grounded && stopsOnEdge && ! controller.IsGrounded(frontGroundCheck.position)) {
			m_waiting = true;
			m_EdgeTimeStart = Time.timeSinceLevelLoad;
		} else if(controller.Grounded && controller.IsGrounded(frontWallCheck.transform.position)){
			m_waiting = true;
			m_EdgeTimeStart = Time.timeSinceLevelLoad;
		}

	}
}
