using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DialogueNode {

	public string ID;// The ID for this node in the dictionary
	public string text; // The text to be displayed in MessageText
	public List<string> buttons = new List<string>();

	public DialogueNode(string identification, string txt, List<string> buttns) {
		ID = identification;
		text = txt;
		buttons.AddRange (buttns);
	}
}