using UnityEngine;

public class RSLogo : MonoBehaviour {
	[Header("Components")]
	public Animator animator;

	[Header("Shared")]
	public bool loaded;

	private void FixedUpdate() {
		animator.SetBool("Loaded", loaded);
	}
}
