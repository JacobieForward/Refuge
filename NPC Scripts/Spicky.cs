using UnityEngine;
using System.Collections;

public class Spicky : MonoBehaviour {

	void Update() {
		if (GameManager.instance.helpedmiser == 1 && !ExplorationManager.instance.messagepanel.activeSelf) {
			gameObject.GetComponentInParent<TextTrigger> ().nodeID = "spickyconfronthelpedmiser";

		}
		if (GameManager.instance.helpedmiser == 2 && !ExplorationManager.instance.messagepanel.activeSelf) {
			gameObject.GetComponentInParent<TextTrigger> ().nodeID = "spickyconfrontdeadmiser";
		}

		if (GameManager.instance.spickygone == 1 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}