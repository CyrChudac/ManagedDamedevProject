using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.U2D;
using System.Collections.Generic;
using System.Collections;

public class CharacterController2D : MonoBehaviour
{
	[Range(0, 1)] [SerializeField] private float m_sneakSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[Range(0, 1.2f)][SerializeField] private float m_AirControlModifier = 0.9f;	// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform[] m_GroundChecks;                         // A position marking where to check if the player is grounded.
	[SerializeField] private Transform[] m_CeilingChecks; 
	[Range(0, 1.2f)][SerializeField] private float m_coyoteTime = 0.15f;
	[Range(0, 1.2f)][SerializeField] private float m_jumpRememberTime = 0.1f;

	[Header("Jump")]
	[SerializeField] private float m_JumpHeight = 3f;
	[SerializeField] private float m_floatHeight = 0.3f;
	[Range(0, 15f)][SerializeField] private float m_jumpFloatTime = 0.1f;
	[Range(0, 15f)][SerializeField] private float m_jumpTime = 0.3f;
	[Range(1f, 3.5f)][SerializeField] private float m_fallTimeModifier = 1.5f;
	[Range(0f, 0.35f)][SerializeField] private float m_smoothJumpTime = 0.1f;
	
	private float FallTime => m_jumpTime / m_fallTimeModifier;
	[SerializeField] private float m_fallMaxSpeed = 20f;

	private float JumpFullTime 
		=> m_jumpTime + m_jumpFloatTime + FallTime;

	const float k_GroundedRadius = 0.08f; // Radius of the overlap circle to determine if grounded
	const float k_CeilingRadius = 0.08f;
	private bool m_Grounded;            // Whether or not the player is grounded.
	public bool Grounded => m_Grounded;
	private Rigidbody2D m_Rigidbody2D;
	private bool m_FacingRight = true;  // For determining which way the player is currently facing.

	private Vector3 Velocity = Vector3.zero;
	private float m_lastGrounded = float.MinValue;
	private float m_lastJumpButton = float.MinValue;
	private float m_lastJumpTime = float.MinValue;
	private float m_jumpAtY = float.MaxValue;
	private float m_startFloating = float.MinValue;

	[Header("Events")]
	[Space]
	
	public UnityEvent OnLandEvent;
	public UnityEvent OnJumpEvent;

	[System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	private float gravityScale;

	private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();
		gravityScale = m_Rigidbody2D.gravityScale;

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();
	}

	private void FixedUpdate() {
		Grounding();
		Ceiling();
		Jumping();
	}
	bool is_floating = false;
	private void Jumping() {
		if(m_Rigidbody2D.transform.position.y - m_jumpAtY > m_JumpHeight) {
			if(! is_floating) {
				is_floating = true;
				m_startFloating = Time.timeSinceLevelLoad;
			}
			if(m_startFloating + m_jumpFloatTime < Time.timeSinceLevelLoad) {
				is_floating = false;
				m_Rigidbody2D.gravityScale = gravityScale;
				m_jumpAtY = float.MaxValue;
				return;
			}
			var y = m_Rigidbody2D.velocity.y;
			Mathf.SmoothDamp(m_jumpAtY + m_JumpHeight,
				m_jumpAtY + m_JumpHeight + m_floatHeight,
				ref y,
				m_jumpFloatTime + m_startFloating - Time.timeSinceLevelLoad);
			m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, y);
		}
	}

	private void Ceiling() {
		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		var colliders = m_CeilingChecks
			.Select(ch => Physics2D.OverlapCircleAll(ch.position, k_CeilingRadius, m_WhatIsGround))
			.SelectMany(t => t);
		foreach(Collider2D c in colliders) {
			if (c.gameObject != gameObject)
			{
				m_lastJumpTime = float.MinValue; 
				m_jumpAtY = float.MaxValue;
				m_Rigidbody2D.gravityScale = gravityScale;
				break;
			}
		}

	}

	private void Grounding()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;

		// The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
		// This can be done using layers instead but Sample Assets will not overwrite your project settings.
		
		foreach(var ch in m_GroundChecks) {
			if(IsGrounded(ch.position)) {
				m_Grounded = true;
				if(!wasGrounded) {
					OnLandEvent.Invoke();
				}
				break;
			}
		}
		if(wasGrounded && ! m_Grounded)
			m_lastGrounded = Time.timeSinceLevelLoad;
	}

	public bool IsGrounded(Vector3 position) {
		foreach(Collider2D c in Physics2D.OverlapCircleAll(position, k_GroundedRadius, m_WhatIsGround)) {
			if (c.gameObject != gameObject)
			{
				return true;
			}
		}
		return false;
	}

	public void Move(float move, float direction, bool jump, bool flipping=true, bool forceJump=false)
	{
		if(!m_Grounded)
			move *= m_AirControlModifier;

		// If the player should jump...
		if (jump || m_lastJumpButton + m_jumpRememberTime > Time.timeSinceLevelLoad)
		{
			if(CanJump() || forceJump) {
				m_Grounded = false;
				m_lastJumpButton = float.MinValue;
				m_Rigidbody2D.gravityScale = 0;
				m_jumpAtY = m_Rigidbody2D.transform.position.y;
				m_Rigidbody2D.AddForce(new Vector2(0, m_JumpHeight/m_jumpTime), ForceMode2D.Impulse);
 				m_lastJumpTime = Time.timeSinceLevelLoad;
			} else if(jump){
				m_lastJumpButton = Time.timeSinceLevelLoad;
			}
		}

		// Move the character by finding the target velocity
		Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
		// And then smoothing it out and applying it to the character
		m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref Velocity, m_MovementSmoothing);

		if(flipping) {
			// If the input is moving the player right and the player is facing left...
			if(direction > 0 && !m_FacingRight) {
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if(direction < 0 && m_FacingRight) {
				Flip();
			}
		}
	}
	
	private bool CanJump()
		=> m_Grounded || m_lastGrounded + m_coyoteTime > Time.timeSinceLevelLoad;

	public void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}
}