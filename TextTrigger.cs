using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TextTrigger : MonoBehaviour {
	
	public string nodeID; // IDs are strings (as opposed to ints) to help with organization
	// This ID is set for the node to start this dialogue, and changes as the player presses buttons
	bool active = false;
	// This ID is in case this texttrigger needs to be destroyed after one use
	public int ID;
	// If this texttrigger is persistent (this bool is true) then it will not be destroyed after being triggered once
	public bool persistent;
	public string persistentnodeID; // stores the original node if this trigger is persistent
	string destroyed;
	// If this textTrigger can start a combat encounter, the enemies for that encounter will be stored here
	public bool pressfortrigger;

	public List<GameObject> enemiesforencounter = new List<GameObject>();
	public int moneylootforencounter = 0;
	public string lootstringencounter = "";

	void Awake() {
		persistentnodeID = nodeID;
		if (!persistent) {
			destroyed = SceneManager.GetActiveScene ().name + "/trigger" + ID;
			if (PlayerPrefs.GetInt (destroyed) != 0) {
				Destroy (gameObject);
			}
		}
	}

	void OnCollisionEnter2D(Collision2D other) {
		if (!pressfortrigger && other.gameObject.tag == "Player" && enabled) {
			// load the first node
			GameManager.instance.currentnode = GameManager.instance.dialogdictionary [nodeID];
			// Populate the message panel with the correct text and buttons
			ExplorationManager.instance.messagetext.text = GameManager.instance.currentnode.text;
		
			// Make the player unable to move while the messagepanel is open
			GameManager.instance.PlayerMovementEnabled = false;
			
			CreateButtons ();
			
			active = true;
			// Set the message panel active so players can see and interact with it
			ExplorationManager.instance.messagepanel.SetActive (true);
		}
	}

	void OnCollisionStay2D(Collision2D other) {
		if (pressfortrigger && other.gameObject.tag == "Player" && Input.GetKeyDown("space")) {
			if (persistent) {
				nodeID = persistentnodeID;
			}
			// load the first node
			GameManager.instance.currentnode = GameManager.instance.dialogdictionary [nodeID];
			// Populate the message panel with the correct text and buttons
			ExplorationManager.instance.messagetext.text = GameManager.instance.currentnode.text;

			// Make the player unable to move while the messagepanel is open
			GameManager.instance.PlayerMovementEnabled = false;

			CreateButtons ();

			active = true;
			// Set the message panel active so players can see and interact with it
			ExplorationManager.instance.messagepanel.SetActive (true);
		}
	}

	// Populates the EnemyPanel with buttons for each enemy
	void CreateButtons() {
		//TODO: First check to make sure CreateButtons has not already been called for these buttons, so we do not make the same ones twice
		foreach (string buttoninfo in GameManager.instance.currentnode.buttons) {
			string[] parsedinfo = buttoninfo.Split ('/');
			// Creates a new button from the MessageButton prefab
			GameObject newButton = Instantiate (GameManager.instance.messagebutton) as GameObject;

			// TODO: Figure out if this is really the best way to do this. All this reassignment seems unnecessary
			MessageButton mbutton = newButton.GetComponent<MessageButton> ();
			mbutton.destinationnodeID = parsedinfo [1];
							
			// Finds the text component in the new Button and sets it to enemy.Name from the enemy being accessed in the foreach statement
			Text ButtonText = newButton.transform.FindChild ("Text").gameObject.GetComponent<Text> ();
			ButtonText.text = parsedinfo [0];

			newButton.transform.SetParent (ExplorationManager.instance.messagespacer);
			}
	}

	void DeleteButtons() {
		List<GameObject> toDelete = new List<GameObject> ();
		toDelete.AddRange(GameObject.FindGameObjectsWithTag("MessageButton"));
		foreach (GameObject button in toDelete) {
			Object.Destroy (button);
		}
	}

	void Update() {
		// TODO: Move all of this out of update for efficiency purposes
		if (!GameManager.instance.currentnode.ID.Equals(nodeID) && active) {
			GameManager.instance.LoadDialogueNodes ();
			nodeID = GameManager.instance.currentnode.ID;
			if (GameManager.instance.dialogdictionary.TryGetValue (nodeID, out GameManager.instance.currentnode)) {

				// Update the text
				ExplorationManager.instance.messagetext.text = GameManager.instance.currentnode.text;

				// Clear out all buttons and create new ones from the new node
				DeleteButtons ();
				CreateButtons();
			}
			if (GameManager.instance.currentnode.ID.Equals("exit")){
				// Tree is ended, exit dialogue mode
				GameManager.instance.PlayerMovementEnabled = true;
				ExplorationManager.instance.messagepanel.SetActive (false);
				PlayerPrefs.SetInt(destroyed, 1);
				active = false;
				if (!persistent) {
					Object.Destroy (gameObject);
				}
			}
			if (GameManager.instance.currentnode.ID.Equals ("fight")) {
				// A combat encounter! Load combat
				GameManager.instance.PlayerMovementEnabled = true;
				ExplorationManager.instance.messagepanel.SetActive (false);
				PlayerPrefs.SetInt(destroyed, 1);
				active = false;
				if (!persistent) {
					Object.Destroy (gameObject);
				}
				GameManager.instance.InitiateCombat (enemiesforencounter, moneylootforencounter, lootstringencounter);

			}
			if (GameManager.instance.currentnode.ID.Equals ("victory")) {
				// The player has won! Load the victory screen
				ExplorationManager.instance.messagepanel.SetActive (false);
				active = false;
				GameManager.instance.PlayerWon ();

			}
			GameManager.instance.currentnode.ID = nodeID;
		}
	}
}