using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float Speed = 2f;
    [SerializeField] private CharacterController2D controller;
	[SerializeField] private Transform frontGroundCheck;
	[SerializeField] private float edgeWaitingTime = 0.5f;
	public float stopBeforeWall = 3f;
	[SerializeField] private SpriteRenderer sizeProvider;
	public bool stopsOnEdge = true;
	public bool moving = true;
	[SerializeField] private AnimationController animationController;

	private float m_EdgeTimeStart = float.MinValue;
	private bool m_waiting = false;
	private float m_goingTo = 1;
	private GameObject frontWallCheck;
	private bool lightingUp = false;

	[SerializeField] private FireController myFire;
	[SerializeField] private float lightUpDelay = 0.2f;
	private float lightUpStart = float.MinValue;
	[SerializeField] private LayerMask whatIsWall;

	public void SetFireRadiusModifier(float radiusModifier)
		=> myFire.Flicker.ModifiyBounds(radiusModifier);

	[SerializeField] 
	private EnemyVision myView;
	public EnemyVision MyVision => myView;

	bool m_spotting = false;

	public void StartSpotting() {
		m_spotting = true;
	}

	private float lastSpotting = float.MinValue;

	public void StopSpotting() {
		m_spotting = false;
		lastSpotting = Time.timeSinceLevelLoad;
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if(collision.gameObject.layer == LayerMask.NameToLayer("Player")) {
			if (lastSpotting + 0.5f > Time.timeSinceLevelLoad) {
				controller.Flip();
				m_goingTo *= -1;
			}
		}
	}


	private Vector3 MidSpot
		=> transform.position + (sizeProvider == null ? Vector3.zero : new Vector3(sizeProvider.bounds.size.x / 2, -sizeProvider.bounds.extents.y * 0.8f));

	[SerializeField]
	private bool shouldFlip;
	public void FlipAfterStart() {
		shouldFlip = true;
	}

	public void StopMovingForGood() => moving = false;

	private void Start() {
		IEnumerator Routine() {
			GameObject go = new GameObject("frontWallCeck");
			go.transform.SetParent(transform, false);
			frontWallCheck = go;
			yield return new WaitForEndOfFrame();
			go.transform.position = MidSpot + Vector3.right * stopBeforeWall;
			if(shouldFlip) {
				m_goingTo = -1;
				yield return new WaitForEndOfFrame();
				controller.Flip();
				//transform.localScale = new Vector3(
				//	transform.localScale.x * -1,
				//	transform.localScale.y,
				//	transform.localScale.z);
			}
		}
		StartCoroutine(Routine());
	}


	private void FixedUpdate() {
		float speed = 
			(! moving) || startStoped || m_spotting || m_waiting || lightingUp ? 0 : Speed * m_goingTo;
		controller.Move(speed, speed.Sign(), false, flipping: false);
	}

	private void Update() {
		if(startStoped || m_spotting || LightUpFire() || !moving)
			return;
		if(m_waiting) {
			if(m_EdgeTimeStart + edgeWaitingTime < Time.timeSinceLevelLoad) {
				m_waiting = false;
				controller.Flip();
				m_goingTo *= -1;
			}
		} else if(controller.Grounded && stopsOnEdge && !controller.IsGrounded(frontGroundCheck.position)) {
			m_waiting = true;
			m_EdgeTimeStart = Time.timeSinceLevelLoad;
		} else {
			var dir = frontWallCheck.transform.position - MidSpot;
			var hit = Physics2D.Raycast(MidSpot, dir.normalized, dir.magnitude, whatIsWall);
			if(hit.collider != null) {
				m_waiting = true;
				m_EdgeTimeStart = Time.timeSinceLevelLoad;
			}
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
				animationController.SetTrigger("lightUp");
				animationController.SetFloat("LightUpSpeed", 1 / lightUpDelay);
				lightingUp = true;
				yield return new WaitForSeconds(Mathf.Max(t, lightUpDelay));
				if(!myFire.IsOn) {
					animationController.SetTrigger("lightUpStop");
					t = myFire.LightUp();
				}
				lightingUp = false;
			}
		}
		if(! lightingUp)
			StartCoroutine(Routine());
		return lightingUp;
	}
	
	public void CanGoFirst() => startStoped = false;
	[Header("for tutorial only - never use in actual level!")]
	[SerializeField]
	private bool startStoped = true;
}
