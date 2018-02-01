using UnityEngine;
using System.Collections;

public class MessageButton : MonoBehaviour {
	// Note to self: Any variables that need to be changed via button press should be kept
	// in GameManager and accessed via GameManager.instance
	public string destinationnodeID; // The dictionary key for the dictionary that is entered if this button is pressed

	public void SelectOption() {
		// Do what should happen when this is button is pressed

		// Switch to node attatched to this button
		// GameManager.instance.currentnode = GameManager.instance.dialogdictionary[destinationnodeID];
		GameManager.instance.checkTextForBool (destinationnodeID);
	}
}