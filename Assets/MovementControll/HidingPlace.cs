using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HidingPlace : MonoBehaviour
{
    [SerializeField] private float timeForHide = 0.3f;
    [SerializeField] private float timeForunhide = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public float TimeForHide => timeForHide;
    private static int ORDER_HIDEN = -6;
    private int orderNormal;

    private Sequence hidingTween;
    private SpriteRenderer hiderSprite;
    private GameObject hider;
    private Animator[] characterAnimators;
    private Animator[] myAnimators;

	private void Awake() {
		spriteRenderer.sortingOrder = ORDER_HIDEN + 1;
        myAnimators = gameObject.GetComponentsInChildren<Animator>();
	}

	/// <summary>
	/// Starts Hiding animation of a given object by informing the given animator.
	/// If no animator is given (null), just tweening will be used.
	/// </summary>
	public void StartHide(GameObject hider, SpriteRenderer sr, params Animator[] characterAnimators) {
        this.hider = hider;
        this.characterAnimators = characterAnimators;
        orderNormal = sr.sortingOrder;
        if(characterAnimators.Length != 0) {
            characterAnimators.PlayInFixedTimeExt("Hiding", timeForHide);
            sr.sortingOrder = ORDER_HIDEN;
        } else {
            hidingTween = DOTween.Sequence()
                .Append(hider.transform.DOMove(transform.position, timeForHide))
                .OnComplete(() => {
                    hidingTween = null;
                    IsOccupied = true;
                    sr.sortingOrder = ORDER_HIDEN;
                });
        }
        if(myAnimators.Length > 0) {
            myAnimators.PlayInFixedTimeExt("Hiding", timeForHide);
        }
        hiderSprite = sr;
    }

    /// <summary>
    /// Stops the Hiding animation.
    /// </summary>
    public void StopHide() {
        if(hidingTween != null) {
            hidingTween.Kill();
            hidingTween = null;
        } else {
            characterAnimators.PlayInFixedTimeExt("Unhide", 0);
            hiderSprite.sortingOrder = orderNormal;
        }
        if(myAnimators != null) {
            myAnimators.PlayInFixedTimeExt("BreakHiding", timeForHide);
        }
    }

    /// <summary>
    /// Makes the object go out from the Hiding place.
    /// </summary>
    public void GoOut() {
        IsOccupied = false;
        if(characterAnimators.Length != 0) {
            characterAnimators.PlayInFixedTimeExt("Unhide", timeForHide);
        }
        hiderSprite.sortingOrder = orderNormal;
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
