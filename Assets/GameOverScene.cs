using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverScene : MonoBehaviour
{
    void Update()
    {
        if(Input.GetButtonDown("Cancel")) {
            Application.Quit();
        }
        else if(Input.anyKeyDown) {
            MySceneManager.LoadScene("PlayerPlayground");
        }
    }
}
