using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class StaticGun : GameHandler {
		[Header("Components")]
		public SpriteRenderer spriteRenderer;
		public AudioSource audioSource;
		public InGameObjectBehaviour gunBehaviour;

		[Header("Properties")]
		public Bullet bullet;

		float time;

		void ResetTime() => time = gunBehaviour.properties.wakeTime;

		protected override void OnGameReady() => ResetTime();
		protected override void OnGameResetObject() => ResetTime();

		private void FixedUpdate() {
			if (Globals.onPause) return;

			if (time > 0f) time -= Time.fixedDeltaTime;
			else {
				Bullet newObj = Instantiate(
					bullet,
					transform.position + 0.15f * RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad),
					Quaternion.Euler(0f, 0f, transform.eulerAngles.z)
				);
				newObj.transform.localScale = transform.localScale;
				newObj.rb.velocity = 15f * transform.localScale.x * RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad);
				newObj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

				time = gunBehaviour.properties.safeReloadTime;
				audioSource.Play();
			}
		}
	}
}
