using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MySceneManager : MonoBehaviour
{
	public void GameOver() => LoadScene("GameOver");
	public void LevelDone() => LoadScene("LevelDone");
	public void ToMenu() => LoadScene("Menu");
	public void ToOptions() => LoadScene("Options");
	public void ToGame() => LoadScene("SampleScene");
	public void Quit() {
		DOTween.KillAll();
		Application.Quit();
	}
	public static void LoadScene(string scene) {
		DOTween.KillAll();
		UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
	}
}
