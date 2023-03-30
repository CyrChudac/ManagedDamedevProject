using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtinguishObject : MonoBehaviour
{
	[SerializeField] private Transform rayStart1;
	[SerializeField] private Transform rayStart2;
	[SerializeField] private Transform rayEnd1;
	[SerializeField] private Transform rayEnd2;
    [SerializeField] private float windExistenceTime = 1.0f;
    [SerializeField] private float colliderWidth = 3.0f;
	[SerializeField] private int rayCount = 50;
    [SerializeField] private List<ParticleSystem> particleSystems;

	private LayerMask fireLayer;
	private LayerMask wallLayer;
    private float startTime;
    // Start is called before the first frame update
    void Start()
    {
		fireLayer = LayerMask.GetMask("Fire");
		wallLayer = LayerMask.GetMask("Walls");
        startTime = Time.time;
    }

    public void SetParticles(Transform obj) {
        foreach(var ps in particleSystems) {
            var main = ps.main;
            main.customSimulationSpace = obj;
        }
    }

	public void OnDrawGizmos() {
        if(rayStart1 == null || rayStart2 == null || rayEnd1 == null || rayEnd2 == null) {
            return;
        }
		Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rayStart1.position, rayStart2.position);
        Gizmos.DrawLine(rayEnd1.position, rayEnd2.position);
		Gizmos.color = Color.white;
        for(int i = 0; i < rayCount + 1; i++) {
            var start = Vector3.Lerp(rayStart1.position, rayStart2.position, i * 1.0f / rayCount);
            var end = Vector3.Lerp(rayEnd1.position, rayEnd2.position, i * 1.0f / rayCount);
            Gizmos.DrawLine(start, end);
        }
	}

	// Update is called once per frame
	void Update()
    {
        if(startTime + windExistenceTime < Time.time) {
            Destroy(gameObject);
            return;
        }
        List<Collider2D> hitFires = new List<Collider2D>(0);
        for(int i = 0; i < rayCount + 1; i++) {
            var start = Vector3.Lerp(rayStart1.position, rayStart2.position, i * 1.0f / rayCount);
            var maxEnd = Vector3.Lerp(rayEnd1.position, rayEnd2.position, i * 1.0f / rayCount);
            var lenRatio = Time.time - startTime / windExistenceTime;
            var end = Vector3.Lerp(start, maxEnd, lenRatio);
            var dir = (maxEnd - start).normalized;
            var mid = end - colliderWidth * dir;
            RaycastHit2D raycast =  Physics2D.Raycast(start, dir,
                (start - mid).magnitude,
                wallLayer);
            if(raycast.collider != null) {
                continue;
            }
            raycast =  Physics2D.Raycast(mid, dir,
                (mid - end).magnitude,
                fireLayer);
            if(raycast.collider != null ) {
                if(hitFires.Contains(raycast.collider)) {
                    continue;
                }
                hitFires.Add(raycast.collider);
			    var fc = raycast.collider.GetComponent<FireController>();
                fc.Extinguish();
            }
        }
        
    }
}
