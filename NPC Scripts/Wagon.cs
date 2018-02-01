using UnityEngine;
using System.Collections;

public class Wagon : MonoBehaviour {
	
	// Not the most efficient or elegant way to do this, but it works
	void Update () {
		if (GameManager.instance.wagonpassed == 0) {
			gameObject.SetActive (false);
		}
		if (GameManager.instance.wagonpassed == 1) {
			gameObject.SetActive (true);
		}
	}
}
