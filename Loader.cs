using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {
	public GameObject gameManager;

	// Use this for initialization
	void Awake () {
		// Creates a gamemanager from the gamemanager prefab if there is not one
		if (GameManager.instance == null) {
			Instantiate (gameManager);
		}
	}
}
