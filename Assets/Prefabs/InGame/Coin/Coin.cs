using UnityEngine;



namespace RobotoSkunk.PixelMan.Gameplay {
	public class Coin : GameHandler {
		public Animator animator;
		public SpriteRenderer spriteRenderer;
		public ParticleSystem particles;

		bool collected = false;
		float delta = 0f;


		public void OnGameState(bool isEditor) {
			if (isEditor) particles.Stop();
			else particles.Play();

			if (isEditor) {
				animator.speed = 0f;
				animator.Play("Coin", 0, 0f);
			}
		}

		protected override void OnGameResetObject() {
			collected = false;
			Globals.gotCoin = false;

			UpdateCoin(true);
		}

		private void Update() {
			animator.speed = (!Globals.onPause).ToInt();

			UpdateCoin();
		}

		private void OnTriggerEnter2D(Collider2D other) {
			if (other.gameObject.CompareTag("Player")) {
				collected = true;
				Globals.gotCoin = true;
			}
		}

		void UpdateCoin(bool force = false) {
			if (collected) {
				delta += Time.deltaTime;
				delta = Mathf.Clamp01(delta);

				animator.SetBool("Collected", true);

				if (delta >= 0.5f && !particles.isStopped) particles.Stop();
				return;
			}

			if (!force) return;

			spriteRenderer.color = Color.white;
			animator.speed = 1f;
			delta = 0f;
			particles.Play();
			animator.SetBool("Collected", false);
			animator.Play("Coin", 0, 0f);
		}
	}
}
