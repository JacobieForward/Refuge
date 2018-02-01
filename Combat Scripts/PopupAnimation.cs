using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PopupAnimation : MonoBehaviour {
	public Animator animator;

	void OnEnable() {
		AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo (0); // Reference to the length of the animation
		Destroy (gameObject, clipInfo[0].clip.length); // Destroys the popuptext object when the animation's time is up
	}
}