using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class PauseHandler : GameHandler {
		[Header("Components")]
		public Animator animator;
		public ParticleSystem particle;

		private void Update() {
			if (animator != null)
				animator.speed = Globals.onPause ? 0 : 1;

			if (particle != null) {
				if (Globals.onPause && particle.isPlaying) particle.Pause();
				else if (!Globals.onPause && particle.isPaused) particle.Play();
			}
		}

		protected override void OnGameResetObject() => Destroy(gameObject, Time.fixedDeltaTime);
	}
}
