using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

	// GUI Style for the menu title
	private GUIStyle guistyle = new GUIStyle();

	void Start() {
		// Set the font size for the menu text
		guistyle.fontSize = 55;
	}

	// This function will continually draw GUI buttons
	// It runs constantly like Update does
	void OnGUI() {
		int menuButtonHeight = 50;
		int menuButtonWidth = 150;
		// If the current gamestate is Menu then load the menu buttons
		// Replaced with UI elements in editor	
		// GUI.Label(new Rect(Screen.width/2 - menuButtonWidth/2 - 10, Screen.height/2 - menuButtonHeight/2 - 100, 1000, 1000), "Refuge", guistyle);
	
		if (GUI.Button (new Rect (Screen.width/2 - menuButtonWidth/2, Screen.height/2 - menuButtonHeight/2, menuButtonWidth, menuButtonHeight), "New Game")) {
			SceneManager.LoadScene("Level1");
		}
	}
}
