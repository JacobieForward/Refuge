using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Enemy : MonoBehaviour {
	// This variable shows whether or not the enemy has been defeated by the player
	// If it has it will dissapear on every level load
	public int ID;
	string defeated; // 0 is not defeated and 1 is defeated
	public Vector3 position = new Vector3();
	public List<GameObject> enemiestobattle = new List<GameObject>();// Enemies to spawn in battle
	public string lootdropstring;
	public int moneydrop;

	void Awake() {
		defeated = SceneManager.GetActiveScene().name + "/enemy" + ID;
		if (PlayerPrefs.GetInt (defeated) != 0) {
			Destroy (gameObject);
		}
	}

	void OnCollisionEnter2D(Collision2D other){
		if (other.gameObject.tag == "Player") {
			// Note to self - simply access GameManager.instance to access gamemanager functions externally
			// GameManager.instance.enemiesinlevel[GameManager.instance.enemiesinlevel.IndexOf(this)].defeated = true;
			// Level1.instance.enemyproperties.Find(x => x.position.Equals(position)).defeated = true;

			PlayerPrefs.SetInt(defeated, 1);
			//Do the other death stuff if needed

			// Pass enemy array to the gamemanager
			// Take the array from the gamemanager to populate enemiesincombat in the BattleStateMachine
			GameManager.instance.enemiesInCombat = enemiestobattle;

			// Start the fight
			// GameManager.instance.currentstate = GameManager.GameState.BATTLE;
			GameManager.instance.InitiateCombat(enemiestobattle, moneydrop, lootdropstring);
		}
	}

	void Update() {
		// Update the position so that enemies will be spawned at the same point they were at before a level load
		position = transform.position;
	}
}