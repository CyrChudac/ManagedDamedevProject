using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoEnemyMover : MonoBehaviour
{
	[SerializeField]
	private EnemyController enemy;

	public void CanMove(bool value) {
		enemy.moving = value;
	}
}
