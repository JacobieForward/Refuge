using UnityEngine;
using System.Collections;

public class Isaac : MonoBehaviour {

	TextTrigger texttrigger;

	void Start() {
		texttrigger = gameObject.GetComponentInParent<TextTrigger> ();
	}
	// Update is called once per frame
	void Update () {
		if (GameManager.instance.isaacquest == 2 && !ExplorationManager.instance.messagepanel.activeSelf) {
			if (texttrigger.nodeID != "isaacpersistent") {
				texttrigger.nodeID = "isaacpersistent";
				texttrigger.persistentnodeID = "isaacpersistent";
			}
		}
		if (GameManager.instance.isaacquest == 3 && !ExplorationManager.instance.messagepanel.activeSelf) {
			if (texttrigger.nodeID != "isaacpersistent") {
				texttrigger.nodeID = "isaacpersistent";
				texttrigger.persistentnodeID = "isaacpersistent";
			}
		}

		if (GameManager.instance.isaacquest == 4 && !ExplorationManager.instance.messagepanel.activeSelf) {
			if (texttrigger.nodeID != "isaacpersistent2") {
				texttrigger.nodeID = "isaacpersistent2";
				texttrigger.persistentnodeID = "isaacpersistent2";
			}
		}
	}
}
