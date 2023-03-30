using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtinguishController : MonoBehaviour {
	[SerializeField] private float extinguishCooldown = 5.0f;
	[SerializeField] private ExtinguishObject windPrefab;
	[SerializeField] private AnimationController animator;

	private float lastExtinguish = float.MinValue;


	public bool TryExtinguish() {
		if(Time.timeSinceLevelLoad < lastExtinguish + extinguishCooldown) {
			return false;
		}
		lastExtinguish = Time.timeSinceLevelLoad;
		var obj = Instantiate(windPrefab);
		obj.SetParticles(transform);
		obj.transform.position = transform.position;
		if(transform.localScale.x < 0) {
			obj.transform.localScale = new Vector3(
				obj.transform.localScale.x * -1,
				obj.transform.localScale.y,
				obj.transform.localScale.z
			);
		}
		animator.SetTrigger("extinguish");
		return true;
	}
}