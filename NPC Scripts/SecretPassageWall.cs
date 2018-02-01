using UnityEngine;
using System.Collections;

public class SecretPassageWall : MonoBehaviour {

	bool destroythis = false;
	// Update is called once per frame
	void Update () {
		if (GameManager.instance.currentnode == GameManager.instance.dialogdictionary ["secretpassage"]) {
			destroythis = true;
		}
		if (destroythis && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}
