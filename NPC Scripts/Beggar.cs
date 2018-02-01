using UnityEngine;
using System.Collections;

public class Beggar : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		if (GameManager.instance.happybeggar == 1 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
		if (GameManager.instance.happybeggar == 2) {
			gameObject.GetComponentInParent<TextTrigger> ().persistentnodeID = "beggarpissed";
		}
	}
}
