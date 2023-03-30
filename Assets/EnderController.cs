using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnderController : MonoBehaviour
{
    [SerializeField]
    private GameMaster gameMaster;
    [SerializeField]
    private string whatIsPlayer = "Player";
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Result whatToDo;
    public void SetGameMaster(GameMaster gameMaster) {
        this.gameMaster = gameMaster;
    }

	private void OnTriggerEnter2D(Collider2D collision) {
		if(collision.gameObject.layer == LayerMask.NameToLayer(whatIsPlayer)) {
            if(whatToDo == Result.Success)
                gameMaster.LevelDone();
            else if(whatToDo == Result.Menu) {
                gameMaster.ToMenuEnd();
            } else {
                Debug.LogError("Ender result behaviour not implemented");
            }
            animator.SetTrigger("Open");
        }
	}

    enum Result {
        Success,
        Menu
    }

}
