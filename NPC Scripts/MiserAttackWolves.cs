using UnityEngine;
using System.Collections;

public class MiserAttackWolves : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (GameManager.instance.helpedmiser != 0  && !ExplorationManager.instance.messagepanel.activeSelf) {
			Destroy (gameObject);
		}
	}
}