using UnityEngine;
using System.Collections;

public class Nicki : MonoBehaviour {

	TextTrigger texttrigger;

	void Start() {
		texttrigger = gameObject.GetComponent<TextTrigger> ();
	}
	// Update is called once per frame
	void Update () {
		if ((GameManager.instance.nickiquest == 1 || GameManager.instance.nickiquest == 3) && !ExplorationManager.instance.messagepanel.activeSelf) {
			if (texttrigger.nodeID != "nickipersistent") {
				texttrigger.nodeID = "nickipersistent";
				texttrigger.persistentnodeID = "nickipersistent";
			}
		}
		if (GameManager.instance.nickiquest == 2) {
			Destroy (gameObject);
		}
	}
}
