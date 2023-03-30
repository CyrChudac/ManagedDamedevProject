using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAfter(typeof(GameCreator))]
public class PlayerActivation : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D playerBody;
    private bool used = false;
    [Tooltip("should be executed given time after start or on player input?")]
    [SerializeField]
    private ExecutionType howToExecute;
    [Tooltip("how many seconds to wait before executing")]
    [SerializeField]
    private float timePeriod;
    [SerializeField]
    private GameCreator gameCreator;
	private void Start() {
		if(howToExecute == ExecutionType.TIME_BASED) {
            IEnumerator coroutine() {
                yield return new WaitForSeconds(timePeriod);
                Run();
            }
            StartCoroutine(coroutine());
        }
	}

	void Update()
    {
        if(howToExecute == ExecutionType.INPUT_BASED && !used && Input.anyKeyDown) {
            Run();
        }
    }
    private void Run() {
            used = true;
            playerBody.bodyType = RigidbodyType2D.Dynamic;
            gameCreator?.StartEnemies();
            Destroy(this);
    }

    public enum ExecutionType{
        TIME_BASED,
        INPUT_BASED
    }
}
