using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HidingPlace : MonoBehaviour
{
    public float timeForHide = 0.2f;
    public float timeForUnhide = 0.1f;
    [SerializeField] private string animationName = "hidingJump";
    public UnityEvent onHidden;

    private Sequence hidingTween;
    private AnimationController characterAnimator;
    private Animator[] myAnimators;

	private void Awake() {
        myAnimators = gameObject.GetComponentsInChildren<Animator>();
	}

	/// <summary>
	/// Starts Hiding animation of a given object by informing the given animator.
	/// If no animator is given (null), just tweening will be used.
	/// </summary>
	public void StartHide(GameObject hidingCharacter, AnimationController characterAnimator) {
        this.characterAnimator = characterAnimator;
        characterAnimator.SetBool("hiding", true);
        characterAnimator.SetFloat("hidingTime", 1 / timeForHide);
        characterAnimator.SetTrigger(animationName);
        {
            hidingTween = DOTween.Sequence()
                .Append(hidingCharacter.transform.DOMove(transform.position, timeForHide))
                .OnComplete(() => {
                    hidingTween = null;
                    IsOccupied = true;
                    onHidden.Invoke();
                });
        }
        if(myAnimators.Length > 0) {
            myAnimators.PlayInFixedTimeExt("Hiding", timeForHide);
        }
    }

    /// <summary>
    /// Stops the Hiding animation.
    /// </summary>
    public void StopHide() {
        if(hidingTween != null) {
            hidingTween.Kill();
            hidingTween = null;
        }
        characterAnimator.SetBool("hiding", false);
        myAnimators.PlayInFixedTimeExt("BreakHiding", timeForHide);
    }

    /// <summary>
    /// Makes the object go out from the Hiding place.
    /// </summary>
    public void GoOut() {
        IsOccupied = false;
        characterAnimator.SetBool("hiding", false);
    }

    public bool IsOccupied { get; private set; } = false;

}

public static class Animator_Extensions {
    public static void PlayInFixedTimeExt(this Animator[] animators, string name, float time) {
        foreach(var item in animators) {
            item.SetFloat("HidingSpeed", 1/(time+0.2f));
            item.Play("Base Layer." + name, -1);
        }
    }
}
