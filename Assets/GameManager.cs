using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public void GameOver() {
		DOTween.KillAll();
		SceneManager.LoadScene("GameOver");
	}
}
