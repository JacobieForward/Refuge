using UnityEngine;
using System.Collections;

public class BorderGuard : MonoBehaviour {

	TextTrigger texttrigger;
	bool setonce;

	void Start() {
		setonce = true;
		texttrigger = gameObject.GetComponent<TextTrigger> ();
	}

	// Update is called once per frame
	void Update () {
		if (GameManager.instance.borderguard == 1 && gameObject.name == "BorderGuard") {
			// increase x position by 1
			if (setonce) {
				setonce = false;
				Vector3 temp = new Vector3 (gameObject.transform.position.x + 1f, gameObject.transform.position.y, gameObject.transform.position.z);
				gameObject.transform.position = temp;
			}
			if (texttrigger.nodeID != "borderguardstatic") {
				texttrigger.nodeID = "borderguardstatic";
				texttrigger.persistentnodeID = "borderguardstatic";
			}
		}
		if (GameManager.instance.borderguard == 2 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}
