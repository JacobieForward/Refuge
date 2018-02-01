using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

	// These are for player movement
	public float maxspeed = 12f;
	public Rigidbody2D rigidbody2d;

	// These two variables are static so they remain persistent between scenes
	// This can be used since there is only one player to move
	// storedplayerposition is the position of the player at the start of the scene
	// It will default to the beginning of the level for the level load
	// Any time after that it will revert to wherever the player left off
	public static Vector3 storedplayerposition;
	public static bool AtStartPosition = true;

	void Awake() {
		// The rigidbody controlls the player movement in exploration mode

		rigidbody2d = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D> ();
	}

	// FixedUpdate is better for physics than Update
	void FixedUpdate() {
		// TODO: Make it so that this doesn't call the unfreeze command every tick. A function should work
		if (GameManager.instance.PlayerMovementEnabled) {
			float horizontal = Input.GetAxis ("Horizontal");
			float vertical = Input.GetAxis ("Vertical");
			rigidbody2d.velocity = new Vector3 (horizontal * maxspeed, vertical * maxspeed);
			// Save player position for the return to exploration mode
			storedplayerposition = rigidbody2d.transform.position;
			rigidbody2d.constraints = RigidbodyConstraints2D.None;
			rigidbody2d.constraints = RigidbodyConstraints2D.FreezeRotation;
		} else {
			// Freeze all forces moving the player
			rigidbody2d.constraints = RigidbodyConstraints2D.FreezeAll;
		}
	}

	void OnDisable() {
		AtStartPosition = false;
	}

	// It is easier to read this with OnEnable and OnDisable as opposed to using OnDisable and putting the OnEnable code in Awake
	void OnEnable() {

		if (AtStartPosition) {
			storedplayerposition = rigidbody2d.transform.position;
		}

		rigidbody2d.transform.position = storedplayerposition;
	}
}
