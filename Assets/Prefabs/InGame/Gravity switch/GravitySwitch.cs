using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class GravitySwitch : GameHandler {
		[Header("Components")]
		public CircleCollider2D col;
		public SpriteRenderer sprParticles, loadSprite;
		public AudioSource aud;
		public InGameObjectBehaviour gravityBehaviour;

		[Header("Configuration")]
		public List<string> tags;
		public float maxSize = 1.25f;

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
				loadSprite.size = maxSize * new Vector2(1f, Mathf.Clamp01((gravityBehaviour.properties.safeReloadTime - time) / gravityBehaviour.properties.safeReloadTime));
				loadSprite.transform.localPosition = maxSize / 2f * (time / gravityBehaviour.properties.safeReloadTime) * Vector2.down;
			}

			if (time > 0f) time -= Time.deltaTime;

			col.enabled = sprParticles.enabled = time <= 0f;
		}

		private void OnTriggerEnter2D(Collider2D collision) {
			if (tags.Contains(collision.tag) && time <= 0f) {
				time = gravityBehaviour.properties.safeReloadTime;
				aud.Play();
			}
		}
	}
}
