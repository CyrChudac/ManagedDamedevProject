using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScene : MonoBehaviour
{
    [SerializeField]
    private string nextScene = "Menu";
    void Update()
    {
        if(Input.GetButtonDown("Cancel")) {
            Application.Quit();
        }
        else if(Input.anyKeyDown) {
            MySceneManager.LoadScene(nextScene);
        }
    }
}
