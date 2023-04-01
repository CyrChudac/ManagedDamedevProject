using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScene : MonoBehaviour
{
    [SerializeField]
    private string cancelScene = "Menu";
    [SerializeField]
    private string submitScene = "SampleScene";
#if UNITY_EDITOR
    [SerializeField]
    private bool vocal = true;
#endif

    void Update()
    {
        if(Input.GetButtonDown("Cancel")) {
            Load(cancelScene);
        }
        else if(Input.GetButtonDown("Submit")) {
            Load(submitScene);
        }
    }

    private void Load(string name) {
#if UNITY_EDITOR
        if(vocal)
            Debug.Log("Loading scene " + name + ".");
#endif
        MySceneManager.LoadScene(name);
    }
}
