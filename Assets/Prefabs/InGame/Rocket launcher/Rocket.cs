using System.Collections.Generic;
using UnityEngine;

using RobotoSkunk.PixelMan.Events;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Rocket : GameHandler {
		[Header("Components")]
		public Rigidbody2D rb;
		public Animator animator;
		public AudioSource audioSource;
		public SpriteRenderer spriteRenderer;
		public ParticleSystem particles, explosionParticles;

		[Header("Properties")]
		[Range(0f, 25f)] public float factor = 5f;
		[Range(0f, 25f)] public float velocity = 5f;
		public LayerMask layerMask, importantLayers;
		public AudioClip audioClip;
		public List<string> tags;

		[Header("Shared")]
		public GameObject target;

		float ang, newAng;
		Vector2 speed;

		private void Start() => ang = transform.eulerAngles.z;

		private void FixedUpdate() {
			if (Globals.onPause) {
				rb.velocity = Vector2.zero;
				animator.speed = 0;
				if (audioSource.isPlaying) audioSource.Pause();
				return;
			}
			animator.speed = 1;
			if (!audioSource.isPlaying) audioSource.UnPause();

			if (target && !Globals.isDead)
				newAng = RSMath.Direction(transform.position, target.transform.position) * Mathf.Rad2Deg;
	
			ang += Mathf.Sin((newAng - ang) * Mathf.Deg2Rad) * factor;

			speed = velocity * RSMath.GetDirVector(ang * Mathf.Deg2Rad);

			transform.rotation = Quaternion.Euler(0, 0, ang);
			rb.velocity = speed;
		}

		private void OnTriggerEnter2D(Collider2D collision) {
			if (explosionParticles && collision.gameObject.CompareLayers(layerMask)) {
				if (collision.gameObject.CompareLayers(importantLayers) || tags.Contains(collision.tag)) {
					explosionParticles.transform.parent = null;
					explosionParticles.Play();

					particles.transform.parent = null;
					particles.Stop();
					Destroy(particles.gameObject, 1f);

					GeneralEventsHandler.PlayOnBackground(audioClip);
					GeneralEventsHandler.SetShake(0.5f, 0.2f);

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
