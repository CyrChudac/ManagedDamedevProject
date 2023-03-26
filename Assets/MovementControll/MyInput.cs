using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MyInput : MonoBehaviour
{
    private bool mobile;
    [SerializeField]
    private Button hideButton;
    [SerializeField]
    private Button extinguishButton;

    // Start is called before the first frame update
    void Start()
    {
        mobile = Application.platform == RuntimePlatform.Android;
        if(hideButton == null) {
            Debug.LogWarning("hide button not found in input manager");
        } else {
            hideButton.gameObject.SetActive(mobile);
            hideButton.onClick.AddListener(HideFunc);
        }
        if(extinguishButton == null) {
            Debug.LogWarning("hide button not found in input manager");
        } else {
            extinguishButton.gameObject.SetActive(mobile);
            extinguishButton.onClick.AddListener(ExtinguishFunc);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!mobile) {
            VerticalAxis = Input.GetAxisRaw("Vertical");
            HorizontalAxis = Input.GetAxisRaw("Horizontal");
            Jump = Input.GetButtonDown("Jump");

            Extinguish = Input.GetButtonDown("Extinguish");
            Hide = Input.GetButtonDown("Hide");
        } else {
            VerticalAxis = 0;
            HorizontalAxis = 0;
            Jump = false;
            for(int i = 0; i < Input.touchCount; i++) {
                var t = Input.GetTouch(i);
                if(t.rawPosition.x > Screen.width * 3 / 5.0f) {
                    Jump = Jump || t.deltaPosition.y > 0;
                }else if (t.rawPosition.x < Screen.width * 2 / 5.0f) {
                    if(t.deltaPosition.y > t.deltaPosition.x) {
                        VerticalAxis = t.deltaPosition.y;
                    } else {
                        HorizontalAxis = t.deltaPosition.x;
                    }
                }
            }
            Extinguish = innerExtinguish;
            innerExtinguish = false;
            Hide = innerHide;
            innerHide = false;
        }
    }
    private bool innerExtinguish = false;
    void ExtinguishFunc() {
        innerExtinguish = true;
    }
    private bool innerHide = false;
    void HideFunc() {
        innerHide = true;
    }

    public float VerticalAxis { get; private set;}
    public float HorizontalAxis { get; private set;}
    public bool Jump { get; private set;}
    public bool Extinguish { get; private set;}
    public bool Hide { get; private set;}
}