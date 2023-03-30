using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class MyInput : MonoBehaviour
{
    [SerializeField]
    private Button hideButton;
    [SerializeField]
    private Button extinguishButton;
    [SerializeField]
    private float mobileSpeedup = 4.0f;
    [SerializeField]
    private List<GameObject> hiddenOnPc;
    private bool stopped = false;
    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
        if(hideButton == null) {
            Debug.LogWarning("hide button not found in input manager");
        } else {
            hideButton.onClick.AddListener(HideFunc);
        }
        if(extinguishButton == null) {
            Debug.LogWarning("hide button not found in input manager");
        } else {
            extinguishButton.onClick.AddListener(ExtinguishFunc);
        }
#else
        foreach(var go in hiddenOnPc) {
            go.SetActive(false);
        }
#endif
    }

	public void StopAll() {
        stopped = true;
	}

	// Update is called once per frame
	void Update()
    {
        if(stopped) {
            VerticalAxis = 0;
            HorizontalAxis = 0;
            Jump = false;
            Extinguish = false;
            Hide = false;
            return;
        }
#if UNITY_ANDROID
        HandleAndroidInput();
#else
        HandlePCInput();
#endif
    }
    private void HandlePCInput() {
        VerticalAxis = Input.GetAxisRaw("Vertical");
        HorizontalAxis = Input.GetAxisRaw("Horizontal");
        Jump = Input.GetButtonDown("Jump");

        Extinguish = Input.GetButtonDown("Extinguish");
        Hide = Input.GetButtonDown("Hide");
    }

    Dictionary<int, Vector2> rawPositions = new Dictionary<int, Vector2>();
    private void HandleAndroidInput() {
        VerticalAxis = 0;
        HorizontalAxis = 0;
        Jump = false;
        for(int i = 0; i < Input.touchCount; i++) {
            var t = Input.GetTouch(i);
            if(t.phase == TouchPhase.Began) {
                rawPositions.Add(t.fingerId, t.position - t.deltaPosition);
                Debug.Log($"start:[{t.position.x}, {t.position.y}] with delta [{t.deltaPosition.x}, {t.deltaPosition.y}]");
            } else {
                Debug.Log($"touch:[{t.position.x}, {t.position.y}] from [{rawPositions[t.fingerId].x}, {rawPositions[t.fingerId].y}]");
            }
            if(rawPositions[t.fingerId].x > Screen.width * 3 / 5.0f) {
                Jump = Jump || t.deltaPosition.y > Mathf.Abs(t.deltaPosition.x);
            }else if (rawPositions[t.fingerId].x < Screen.width * 2 / 5.0f) {
                var delta = t.position - rawPositions[t.fingerId];
                if(Mathf.Abs(delta.y) * 2 > Mathf.Abs(delta.x)) {
                    VerticalAxis = Mathf.Sign(delta.y) * t.deltaTime * mobileSpeedup;
                } else {
                    HorizontalAxis = Mathf.Sign(delta.x) * t.deltaTime * mobileSpeedup;
                }
            }
        }
        if(Input.touchCount == 0) {
            rawPositions.Clear();
        }
        Extinguish = innerExtinguish;
        innerExtinguish = false;
        Hide = innerHide;
        innerHide = false;

    }

    private bool innerExtinguish = false;
    void ExtinguishFunc() {
        innerExtinguish = true;
    }
    private bool innerHide = false;
    void HideFunc() {
        innerHide = true;
    }

    [Header("--RUNTIME-VALUES--")]
    public float VerticalAxis;
    public float HorizontalAxis;
    public bool Jump;
    public bool Extinguish;
    public bool Hide;
}