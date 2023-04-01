using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyVision : MonoBehaviour
{
    /// <summary>
    /// The person this viewcone belongs to 
    /// </summary>
    [SerializeField] private GameObject agent;
    /// <summary>
    ///what can the agent NOT see through 
    /// </summary>
    [SerializeField]
    private LayerMask notSeeThrough;
    /// <summary>
    ///what is the agent trying to spot
    /// </summary>
    [SerializeField] private LayerMask whatToSpot;
    /// <summary>
    /// How far does the person see.
    /// </summary>
    [SerializeField] protected float viewDistance = 10f;
    /// <summary>
    /// How wide the viewcone is (angle in degrees).
    /// </summary>
    [SerializeField] private float viewAngle = 60;
    /// <summary>
    /// determines whether the viewcoen should be displayed.
    /// </summary>
    [SerializeField] private bool displayViewcone = true;
    /// <summary>
    /// modifies how many rays will be casted. Smaller number = more rays and less performence.
    /// </summary>
    [SerializeField] private float viewRaysModifier = 10f;
    /// <summary>
    /// How does the mesh look if the player is not detected.
    /// </summary>
    [SerializeField] private Color OkView = Color.gray;
    /// <summary>
    /// How does the mesh look if the player is detected.
    /// </summary>
    [SerializeField] private Color RIPView = Color.red;
    /// <summary>
    /// The material the viewcone is made of.
    /// </summary>
    [SerializeField] private Material viewMaterial;
    /// <summary>
    /// The time it takes the agent to spot the the desired object. SET BY ENEMY CREATOR
    /// </summary>
    [Tooltip("Set by EnemyCreator!")]
    public float timeUntilCaught = 1f;
    /// <summary>
    /// How much to decay within 1 sec. (1 = deacy all, infinity = decay instantly, 0 = do not decay)
    /// </summary>
    [SerializeField] private float spottingDecay = 0.2f;
    /// <summary>
    /// The animation controller that is used to shoot at the player when caught.
    /// </summary>
    [SerializeField] private AnimationController animationController;
    
    /// <summary>
    /// The mesh generated by this script.
    /// </summary>
    private Mesh mesh;

    private MeshRenderer meshRenderer;
    
    [Header("Events")] //public so that they can be used from code as well
    [Tooltip("Event triggered when the player is seen and was NOT seen before.")]
    public UnityEvent SpottedEvent;
    [Tooltip("Event triggered when the player is NOT seen anymore, but was seen before.")]
    public UnityEvent UnseenEvent;
    [Tooltip("Event triggered when the enemy sees the player for " + nameof(timeUntilCaught) +" time.")]
    public UnityEvent CaughtEvent;
    [Tooltip("Event triggered when the enemy stops suspecting anything.")]
    public UnityEvent UnsuspectingEvent;
    

    /// <summary>
    /// How many rays will be cast to form the round shape ot the viewcone
    /// </summary>
    int RayCount => Mathf.RoundToInt(viewAngle / (viewRaysModifier / 10f));
    /// <summary>
    /// Determines whether the agent is flipped on its x axes.
    /// </summary>
    bool IsFlipped => agent.transform.localScale.x < 0;
    /// <summary>
    /// Which direction does the first of the viewcone go
    /// </summary>
    float StartingAngle => (IsFlipped ? 0 : 180) - viewAngle/2;
    /// <summary>
    /// Angle after which to check if there is an obstacle in the way
    /// </summary>
    private float AngleIncrease => viewAngle / RayCount;

    void Start()
    {
        //generate the meshes and attach it to the object
        mesh = new Mesh();
        meshRenderer = CreateMeshBearer("view good", mesh, OkView);
        OkView = new Color(OkView.r, OkView.g, OkView.b, 0);
    }

    private MeshRenderer CreateMeshBearer(string name, Mesh mesh, Color color) {
        //create child
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localScale = Vector3.one;
        //set mesh filter on it
        var mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;
        //and also mesh meshRenderer
        var mr = go.AddComponent<MeshRenderer>();
        mr.material = viewMaterial;
        mr.material.color = color;
        mr.sortingOrder = -2;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return mr;
    }

    private void Update() {
        UpdateVision();
        if(displayViewcone) {
            UpdateMeshes();
        }
    }

    protected virtual float GetLengthForAngle(float angle) {
        return viewDistance;
    }

    private void UpdateMeshes()
    {
        if(investigationRate < 0.05) {
            return;
        }
        //find where is the viewcone cast from and init vertices, uv and triangle arrays
        Vector3 origin = transform.position;
        Vector3[] vertices = new Vector3[RayCount + 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[RayCount * 3];
        vertices[0] = Vector3.zero;

        int flipModifier = (IsFlipped ? -1 : 1);
        float currAngle = StartingAngle * flipModifier;
        int triangleIndex = 0;

        Vector2 antiscale = new Vector2(1/agent.transform.localScale.x, 1/agent.transform.localScale.y);

        for (int i = 1; i < RayCount; i++)
        {
            RaycastHit2D raycast;
            float maxLen = GetLengthForAngle(currAngle + agent.transform.rotation.eulerAngles.y);
            //find out what does the ray from the agent hit in the given angle
            raycast =  Physics2D.Raycast(origin, 
                Vector2Utils.VectorFromAngle(currAngle + agent.transform.rotation.eulerAngles.y),
                maxLen,
                notSeeThrough);
            //if nothing, simply set the end of the view at this angle to max distance
            if(raycast.collider == null)
            {
                vertices[i] = Vector2Utils.VectorFromAngle(currAngle) * maxLen;
            }
            //and if something, set it to be at the position of the thing hit
            else
            {
                vertices[i] = Vector2Utils.VectorFromAngle(currAngle) * Vector3.Distance(raycast.point, origin);
            }
            vertices[i] = Vector2Utils.Scaled(vertices[i], antiscale);
            if (i > 0)
            {
                //add triangle to the mesh = the previous vertex, current vertex and their connecting line
                triangles[triangleIndex] = 0;
                triangles[triangleIndex + 1] = i - 1;
                triangles[triangleIndex + 2] = i;

                triangleIndex += 3;
            }
            currAngle += AngleIncrease * flipModifier;
        }

        //set the meshes to have the current vertices, triangles and uv
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
    }

	private void UpdateVision() {
        Vector3 origin = transform.position;
        float currAngle = StartingAngle;
        
        for (int i = 1; i < RayCount; i++)
        {
            RaycastHit2D raycast;
            float maxLen = GetLengthForAngle(currAngle + agent.transform.rotation.eulerAngles.y);
            //find out what does the ray from the agent hit in the given angle
            raycast =  Physics2D.Raycast(origin, 
                Vector2Utils.VectorFromAngle(currAngle + agent.transform.rotation.eulerAngles.y),
                maxLen,
                whatToSpot | notSeeThrough);
            //if nothing, simply set the end of the view at this angle to max distance
            if(raycast.collider != null 
                && whatToSpot == (whatToSpot | 1 << raycast.collider.gameObject.layer))
            {
                var pm = raycast.collider.GetComponent<PlayerMovement>();
                if(pm != null && !pm.Hiding) {
                    Spotting(true, raycast.point);
                    return;
                }
            }
            currAngle += AngleIncrease;
        }
        Spotting(false, Vector3.zero);
	}

    
    private bool isSpotting = false;
    private float spottedTime = float.NegativeInfinity;
    private float spottingRatio = 0;

    private void Spotting(bool value, Vector3 at) {
        if(isSpotting != value) {
            if(value) {
                SpottedEvent.Invoke();
                spottedTime = Time.timeSinceLevelLoad;
            } else {
                UnseenEvent.Invoke();
            }
        }
        if(isSpotting) {
            spottingRatio = Mathf.Max(spottingRatio, (Time.timeSinceLevelLoad - spottedTime) / timeUntilCaught);
        } else {
            spottingRatio -= spottingDecay * Time.deltaTime;
        }
        //spottingRatio = Mathf.Clamp01(spottingRatio);
        SetInvestigationRate(Mathf.Clamp01(spottingRatio));
        if(spottingRatio >= 1) {
            CaughtEvent.Invoke();
            StartCoroutine(CaughtRoutine());
            if(transform.position.y + 0.3 < at.y) {
                animationController.SetTrigger("attackUp");
            } else if(transform.position.y - 1.5 < at.y) {
                animationController.SetTrigger("attack");
            } else {
                animationController.SetTrigger("AttackDown");
            }
            spottingRatio = float.PositiveInfinity;
        }
        isSpotting = value;
    }

    IEnumerator CaughtRoutine() {
        yield return new WaitForEndOfFrame();
        animationController.SetBool("victory");
    }

    private float investigationRate = 0;
	/// <summary>
	/// Set the rate of how much is the player being detected by the enemy.
	/// </summary>
	private void SetInvestigationRate(float rate)
    {
        if(investigationRate != 0 && rate == 0) {
            UnsuspectingEvent.Invoke();
        }
        investigationRate = rate;
        meshRenderer.material.color = Color.Lerp(OkView, RIPView, Mathf.Clamp01(rate));
    }
    
    public void SetViewDistance(float newDist)
        => viewDistance = newDist;
}

static class Vector2Utils {
    public static Vector2 Scaled(Vector2 from, Vector2 scale) {
        var res = new Vector2(from.x, from.y);
        res.Scale(scale);
        return res;
    }
    
    /// <summary>
    /// Make a vector pointing at a given angle.
    /// </summary>
    /// <param name="degrees">Angle in degrees</param>
    /// <returns>A normalized 2D vector pointing at that angle</returns>
    public static Vector2 VectorFromAngle(float degrees)
    {
        float angle = degrees * (Mathf.PI / 180f);
        return new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle));
    }
}