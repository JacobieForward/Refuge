using UnityEngine;
using System.Collections;

public class EnemyButton : MonoBehaviour {

	public GameObject EnemyPrefab;

	public void SelectEnemy() {
		// Needs a reference to the battle state machine, which saves input from the enemy prefab
		GameObject.Find ("BattleManager").GetComponent<BattleStateMachine>().EnemySelection(EnemyPrefab);
	}

	void Update() {
		// This Destroys the enemy button when the enemy attatched to it is destroyed
		// TODO: Do this in EnemyState after the enemy is destroyed
		if (EnemyPrefab == null) {
			Destroy (gameObject);
		}
	}
}