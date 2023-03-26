using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private new Rigidbody2D rigidbody;

    public bool IsClimbing { get; set; }

    private AnimState _curr = AnimState.idle;
    private AnimState Current {
        set {
            if (value != _curr) {
                animator.ResetTrigger(AnimState.jumpFall.ToString());
                animator.SetTrigger(value.ToString());
                Debug.Log(value.ToString());
            }
            _curr = value;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if(rigidbody.velocity.y > 0.005) {
            animator.SetFloat("climbMod", 1);
            Current = IsClimbing ? AnimState.climb : AnimState.jumpStart;
        } else if(rigidbody.velocity.y < -0.005) {
            animator.SetFloat("climbMod", -1);
            Current = IsClimbing ? AnimState.climb : AnimState.jumpFall;
        } else if(Mathf.Abs(rigidbody.velocity.x) > 0.005)
            Current = AnimState.walk;
        else
            Current = IsClimbing ? AnimState.climbIdle : AnimState.idle;
    }
    
    public void SetTrigger(string triggerName) {
        animator.SetTrigger(triggerName);
    }
    
    public void SetBool(string name, bool val) {
        animator.SetBool(name, val);
    }

    public void SetFloat(string name, float value) {
        animator.SetFloat(name, value);
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
