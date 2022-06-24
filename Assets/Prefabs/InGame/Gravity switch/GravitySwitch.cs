using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class GravitySwitch : GameHandler {
		[Header("Components")]
		public CircleCollider2D col;
		public SpriteRenderer sprParticles, loadSprite;
		public AudioSource aud;

		[Header("Configuration")]
		public List<string> tags;
		public float maxSize = 1.25f;

		[Header("Shared")]
		public float reloadTime = 1f;

		float ang, time, rotFactor;

		private void Start() => rotFactor = RSRandom.UnionRange(-8f, -5f, 5f, 8f);

		protected override void OnGameResetObject() {
			time = ang = 0f;
			sprParticles.gameObject.transform.rotation = default;
			loadSprite.size = maxSize * Vector2.one;
			loadSprite.transform.localPosition = default;
			sprParticles.enabled = true;
		}

		private void Update() {
			if (Globals.onPause) return;

			if (sprParticles.isVisible) {
				ang += rotFactor * RSTime.delta;
				sprParticles.gameObject.transform.rotation = Quaternion.Euler(0, 0, ang);
			}
			if (loadSprite.isVisible) {
				loadSprite.size = maxSize * new Vector2(1f, Mathf.Clamp01((reloadTime - time) / reloadTime));
				loadSprite.transform.localPosition = maxSize / 2f * (time / reloadTime) * Vector2.down;
			}

			if (time > 0f) time -= Time.deltaTime;

			col.enabled = sprParticles.enabled = time <= 0f;
		}

		private void OnTriggerEnter2D(Collider2D collision) {
			if (tags.Contains(collision.tag) && time <= 0f) {
				time = reloadTime;
				aud.Play();
			}
		}
	}
}
