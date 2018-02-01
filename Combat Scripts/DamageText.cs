using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DamageText : MonoBehaviour {
	public Animator animator;
	private Text damagetext;

	void OnEnable() {
		AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo (0); // Reference to the length of the animation
		Destroy (gameObject, clipInfo[0].clip.length); // Destroys the popuptext object when the animation's time is up
		damagetext = animator.GetComponent<Text>();
	}

	// Simple method for setting the text on the animator object
	public void setText(string inputtext) {
		damagetext.text = inputtext;
	}
}