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
	[SerializeField] private bool moving = true;

	private float m_EdgeTimeStart = float.MinValue;
	private bool m_waiting = false;
	private float m_goingTo = 1;
	private GameObject frontWallCheck;
	private bool lightingUp = false;

	[SerializeField] private FireController myFire;
	[SerializeField] private float lightUpDelay = 0.2f;
	private float lightUpStart = float.MinValue;

	private void Start() {
		IEnumerator Routine() {
			GameObject go = new GameObject("frontWallCeck");
			go.transform.SetParent(transform, false);
			frontWallCheck = go;
			yield return new WaitForEndOfFrame();
			Vector3 offset = sizeProvider == null ? Vector3.zero : new Vector3(sizeProvider.bounds.size.x / 2, -sizeProvider.bounds.extents.y * 0.8f);
			go.transform.position = transform.position + offset + Vector3.right * stopBeforeWall;
		}
		StartCoroutine(Routine());
	}

	private void FixedUpdate() {
		float speed = (! moving) || m_waiting || lightingUp ? 0 : Speed * m_goingTo;
		controller.Move(speed, speed.Sign(), false, flipping: false);
	}

	private void Update() {
		if(LightUpFire() || !moving)
			return;
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

	bool LightUpFire() {
		if(myFire == null) {
			return false;
		}
		if(myFire.IsOn) {
			lightUpStart = float.MinValue;
			return false;
		}
		IEnumerator Routine() {
			float t = 0;
			while(!myFire.IsOn) {
				lightingUp = true;
				yield return new WaitForSeconds(Mathf.Max(t, lightUpDelay));
				if(!myFire.IsOn)
					t = myFire.LightUp();
				lightingUp = false;
			}
		}
		if(! lightingUp)
			StartCoroutine(Routine());
		return lightingUp;
		if(lightUpStart < 0) {
			lightUpStart = Time.timeSinceLevelLoad;
			lightingUp = true;
		}
		if(lightingUp && lightUpStart + lightUpDelay < Time.timeSinceLevelLoad) {
			myFire.LightUp();
			lightingUp = false;
		}
		return lightingUp;
	}
}
