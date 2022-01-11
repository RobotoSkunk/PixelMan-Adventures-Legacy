using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class PauseHandler : MonoBehaviour {
		[Header("Components")]
		public Animator animator;
		public ParticleSystem particle;

		private void FixedUpdate() {
			if (animator != null)
				animator.speed = Globals.onPause ? 0 : 1;

			if (particle != null) {
				if (Globals.onPause && particle.isPlaying) particle.Pause();
				else if (!Globals.onPause && particle.isPaused) particle.Play();
			}
		}
	}
}
