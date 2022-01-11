using System.Collections.Generic;
using UnityEngine;

using RobotoSkunk.PixelMan.Events;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Bullet : GameHandler {
		[Header("Components")]
		public Rigidbody2D rb;
		public SpriteRenderer spriteRenderer;
		public ParticleSystem explosionParticles;

		[Header("Properties")]
		public AudioClip audioClip;
		public LayerMask layerMask, importantLayers;
		public Sprite[] sprites;
		public List<string> tags;

		int index;
		Vector2 speed;

		private void Start() => speed = rb.velocity;

		private void FixedUpdate() {
			rb.velocity = !Globals.onPause ? speed : Vector2.zero;

			if (spriteRenderer.isVisible && !Globals.onPause && RSTime.fixedFrameCount % 3 == 0) {
				index = index == 0 ? 1 : 0;
				spriteRenderer.sprite = sprites[index];
			}
		}

		private void OnTriggerEnter2D(Collider2D collision) {
			if (explosionParticles && collision.gameObject.CompareLayers(layerMask)) {
				if (collision.gameObject.CompareLayers(importantLayers) || tags.Contains(collision.tag)) {
					explosionParticles.transform.parent = null;
					explosionParticles.Play();

					GeneralEventsHandler.PlayOnBackground(audioClip);

					DestroyMyself();
				}
			}
		}

		protected override void OnGameResetObject() => DestroyMyself();

		void DestroyMyself() {
			rb.velocity = Vector2.zero;
			Destroy(gameObject, Time.fixedDeltaTime);
		}
	}
}
