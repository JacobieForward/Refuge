using UnityEngine;
using System.Collections;

public class BanditLeader : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (GameManager.instance.currentnode == GameManager.instance.dialogdictionary ["banditleaderfight"] || GameManager.instance.currentnode == GameManager.instance.dialogdictionary ["banditleaderexit"]) {
			GameManager.instance.banditleader = 1;
		}
		if (GameManager.instance.banditleader > 0 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}
