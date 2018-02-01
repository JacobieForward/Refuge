using UnityEngine;
using System.Collections;

public class Miser : MonoBehaviour {

	void Update() {
		// Have to check if the messagepanel is also inactive, or the object will be destroyed
		// During dialogue
		if (GameManager.instance.helpedmiser == 2 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}