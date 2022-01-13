using System.Collections.Generic;
using System.Collections;

using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Trampoline : MonoBehaviour {
		[Header("Components")]
		public Animator animator;
		public AudioSource audioSource;

		[Header("Properties")]
		public LayerMask layerMask;

		private void Update() => animator.speed = Globals.onPause ? 0 : 1;

		private void OnTriggerEnter2D(Collider2D collision) {
			if (collision.gameObject.CompareLayers(layerMask)) Impulse();
		}

		private void OnParticleCollision(GameObject other) {
			if (other.CompareLayers(layerMask)) Impulse(other.GetComponent<Rigidbody>());

			Debug.Log("Collision");
		}


		void Impulse(Rigidbody rigidbody = null) {
			audioSource.Play();
			animator.Play("Default", 0, 0f);

			if (rigidbody)
				rigidbody.AddForce(RSMath.GetDirVector((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad) * Constants.trampolineForce, ForceMode.Impulse);
		}
	}
}
