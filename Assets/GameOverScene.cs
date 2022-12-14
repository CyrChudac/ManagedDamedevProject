using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScene : MonoBehaviour
{
    void Update()
    {
        if(Input.GetButtonDown("Cancel")) {
            Application.Quit();
        }
        else if(Input.anyKeyDown) {
            SceneManager.LoadScene("PlayerPlayground");
        }
    }
}
