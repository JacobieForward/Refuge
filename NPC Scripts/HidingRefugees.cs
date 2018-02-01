using UnityEngine;
using System.Collections;

public class HidingRefugees : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (GameManager.instance.deadrefugees == 1 && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}
