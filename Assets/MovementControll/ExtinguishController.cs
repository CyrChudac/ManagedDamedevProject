using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtinguishController : MonoBehaviour
{
    [SerializeField] private float extinguishCooldown = 5.0f;
    [SerializeField] private float extinguishSpeed = 1.0f;
	[SerializeField] private Transform rayStart;
	[SerializeField] private Transform[] rayObjs;

	private float lastExtinguish = float.MinValue;
    private Animator myAnimator;
	private LayerMask fireLayer;
	private static int RAY_COUNT = 50;

	private void Awake() {
		myAnimator= GetComponent<Animator>();
		fireLayer = LayerMask.GetMask("Fire","Walls");
	}

	private void OnDrawGizmos() {
		for(int i = 0; i < rayObjs.Length; i++) {
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(rayObjs[i].position, 0.2f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(rayStart.position, rayObjs[i].position);
			if(i == 0)
				continue;
			Gizmos.DrawLine(rayObjs[i-1].position, rayObjs[i].position);
		}
	}

	private HashSet<Collider2D> GetFires() {
		HashSet<Collider2D> result = new();
		for(int i = 1; i < rayObjs.Length; i++) {
			for(int j = 0; j < RAY_COUNT; j++) {
				var goal = Vector3.Lerp(rayObjs[i - 1].position, rayObjs[i].position, j * 1f / RAY_COUNT);
				var dir = goal - rayStart.position;
				var ray = Physics2D.Raycast(rayStart.position, dir.normalized, dir.magnitude, fireLayer);
				if(ray.collider != null && ! result.Contains(ray.collider)) {
					result.Add(ray.collider);
				}
			}
		}
		return result;
	}

	public bool TryExtinguish(GameObject sender)
    {
        if(Time.timeSinceLevelLoad < lastExtinguish + extinguishSpeed) {
			return false;
		}
		lastExtinguish = Time.timeSinceLevelLoad;
		foreach(var f in GetFires()) {
			var fc = f.GetComponent<FireController>();
			if(fc == null || !fc.IsOn)
				continue;
			var dist = Vector3.Distance(sender.transform.position, f.bounds.center);
			DOTween.Sequence().
				AppendInterval(dist/extinguishSpeed)
				.OnComplete(() => fc.Extinguish());
		}
		PlayAnim(sender.GetComponent<Animator>());
		PlayAnim(myAnimator);
		return true;
    }

	void PlayAnim(Animator a) {
		if(a != null)
			a.SetTrigger("Extinguish");
	}
}
