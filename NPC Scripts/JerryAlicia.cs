using UnityEngine;
using System.Collections;

public class JerryAlicia : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (GameManager.instance.isaacquest == 3 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
		if (GameManager.instance.isaacquest == 4 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}
