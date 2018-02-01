using UnityEngine;
using System.Collections;

public class NickiGone : MonoBehaviour {

	private TextTrigger trigger;
	private BoxCollider2D collider2d;
	void Start() {
		trigger = GetComponent<TextTrigger> ();
		collider2d = GetComponent<BoxCollider2D> ();
		trigger.enabled = false;
		collider2d.enabled = false;
	}
	// Update is called once per frame
	void Update () {
		if (GameManager.instance.nickiquest == 2) {
			trigger.enabled = true;
			collider2d.enabled = true;
		}
	}
}
