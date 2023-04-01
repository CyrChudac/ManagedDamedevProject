using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private new Rigidbody2D rigidbody;
    [SerializeField]
    private float jumpingThreshold = 0.005f;
    [SerializeField]
    private float fallingThreshold = 0.005f;
    [SerializeField]
    private float movementThreshold = 0.01f;

    private bool allStopped = false;
    public void StopAll() {
        allStopped = true;
    }

    public bool IsClimbing { get; set; }
#if UNITY_EDITOR
    [SerializeField]
    private bool logMe = false;
#endif

    private AnimState _curr = AnimState.idle;
    private AnimState Current {
        set {
            if (value != _curr) {
                animator.ResetTrigger(AnimState.jumpFall.ToString());
                animator.ResetTrigger(AnimState.idle.ToString());
                animator.ResetTrigger(AnimState.walk.ToString());
                animator.SetTrigger(value.ToString());
#if UNITY_EDITOR
                if(logMe)
                    Debug.Log(value.ToString());
#endif
            }
            _curr = value;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(allStopped)
            return;

        if(rigidbody.velocity.y > jumpingThreshold) {
            animator.SetFloat("climbMod", 1);
            Current = IsClimbing ? AnimState.climb : AnimState.jumpStart;
        } else if(rigidbody.velocity.y < -fallingThreshold) {
            animator.SetFloat("climbMod", -1);
            Current = IsClimbing ? AnimState.climb : AnimState.jumpFall;
        } else if(Mathf.Abs(rigidbody.velocity.x) > movementThreshold)
            Current = AnimState.walk;
        else
            Current = IsClimbing ? AnimState.climbIdle : AnimState.idle;
    }
    
    public void SetTrigger(string triggerName) {
        if(allStopped)
            return;
        animator.SetTrigger(triggerName);
    }
    public void SetBool(string name) {
        if(allStopped)
            return;
        animator.SetBool(name, true);
    }
    
    public void SetBool(string name, bool val) {
        if(allStopped)
            return;
        animator.SetBool(name, val);
    }

    public void SetFloat(string name, float value) {
        if(allStopped)
            return;
        animator.SetFloat(name, value);
    }

    public void Die() {
        StopAll();
        animator.SetTrigger("die");
    }

    private enum AnimState {
        idle,
        walk,
        attack,
        die,
        jumpStart,
        jumpFall,
        climb,
        climbIdle
    }
}
