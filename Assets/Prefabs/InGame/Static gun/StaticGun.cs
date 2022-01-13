using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class StaticGun : GameHandler {
		[Header("Components")]
		public SpriteRenderer spriteRenderer;
		public AudioSource audioSource;

		[Header("Properties")]
		public Bullet bullet;

		[Header("Shared")]
		public float wakeTime;
		public float reloadTime;

		float time;

		private void Start() => time = wakeTime;

		protected override void OnGameResetObject() => Start();

		private void Update() {
			if (Globals.onPause) return;

			if (time < reloadTime) time += Time.deltaTime;
			else {
				Bullet newObj = Instantiate(
					bullet,
					transform.position + 0.15f * RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad),
					Quaternion.Euler(0f, 0f, transform.eulerAngles.z)
				);
				newObj.transform.localScale = transform.localScale;
				newObj.rb.velocity = 15f * transform.localScale.x * RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad);
				newObj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

				time = 0f;
				audioSource.Play();
			}
		}
	}
}
