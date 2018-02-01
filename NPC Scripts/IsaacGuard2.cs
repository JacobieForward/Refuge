using UnityEngine;
using System.Collections;

public class IsaacGuard2 : MonoBehaviour {

	void Update() {
		if (GameManager.instance.helpedmiser == 2 && !ExplorationManager.instance.messagepanel.activeSelf) {
			gameObject.GetComponentInParent<TextTrigger> ().nodeID = "isaachouseguard2bad";
		}
		if (GameManager.instance.isaachouseguardhelped == 1 && !ExplorationManager.instance.messagepanel.activeSelf) {
			gameObject.GetComponentInParent<TextTrigger> ().nodeID = "isaachouseguard2helped";
		}
	}
}