using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    [SerializeField]
    private AnimationController playerAnimations;
    [SerializeField]
    private MyInput input;
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private MySceneManager sceneManager;
    [SerializeField]
    private GameObject gameOverCourtain;
    [SerializeField]
    private float waitForDeath;
    [SerializeField]
    private float waitAfterDeath;
    [SerializeField]
    private float waitAfterCourtain;

    private bool alreadyEnding = false;

    public void GameOver() {
        if(alreadyEnding) return;
        alreadyEnding = true;
        input.StopAll();
        playerAnimations.StopAll();
        IEnumerator EndRoutine() {
            yield return new WaitForSeconds(waitForDeath);
            playerAnimations.Die();
            yield return new WaitForSeconds(waitAfterDeath);
            var ec = Instantiate(gameOverCourtain);
            ec.transform.position = playerAnimations.transform.position;
            yield return new WaitForSeconds(waitAfterCourtain);
            sceneManager.GameOver();
        }
        StartCoroutine(EndRoutine());
    }

    
    [SerializeField]
    private GameObject levelDoneCourtain;
    [SerializeField]
    private float waitForLevelConclude;
    [SerializeField]
    private float waitAfterLevelConclude;

    private bool levelSuccess = false;
    public void LevelDone() => LevelDone(sm => sm.LevelDone());
    public void LevelDone(Action<MySceneManager> toDo) {
        if(alreadyEnding) { return; }
        alreadyEnding= true;
        input.StopAll();
        playerAnimations.SetTrigger("idle");
        playerAnimations.StopAll();
        IEnumerator EndRoutine() {
            yield return new WaitForSeconds(waitForLevelConclude);
            levelSuccess = true;
            var ec = Instantiate(levelDoneCourtain);
            ec.transform.position = playerAnimations.transform.position;
            curtain = ec;
            Stats.Difficulity += Stats.DifficulityChange * (0.9f + 0.2f * UnityEngine.Random.value) + 0.05f;
            yield return new WaitForSeconds(waitAfterLevelConclude);
            toDo(sceneManager);
        }
        StartCoroutine(EndRoutine());
    }

    public void ToMenuEnd() {
        LevelDone(sm => sm.ToMenu());
    }

    private GameObject curtain;
	private void Update() {
        if(levelSuccess) {
            curtain.transform.position = player.transform.position;
        }else if(!alreadyEnding && Input.GetButtonDown("Cancel")) {
            sceneManager.ToMenu();
        }
	}
}
