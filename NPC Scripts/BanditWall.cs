using UnityEngine;
using System.Collections;

public class BanditWall : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		if (GameManager.instance.currentnode == GameManager.instance.dialogdictionary ["banditentrancewall2"]) {
			GameManager.instance.secretwall = 1;
		}
		if (GameManager.instance.secretwall > 0 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}
