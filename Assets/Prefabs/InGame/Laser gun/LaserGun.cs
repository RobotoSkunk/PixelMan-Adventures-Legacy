using System.Collections.Generic;

using UnityEngine;

using RobotoSkunk.PixelMan.Events;


namespace RobotoSkunk.PixelMan.Gameplay {
	public class LaserGun : GameHandler {
		[Header("Components")]
		public AudioSource audioSource;
		public Animator animator;
		public SpriteRenderer dotedLine, normalLine, laser, outline;
		public BoxCollider2D laserCollider;

		[Header("Properties")]
		public ContactFilter2D contactFilter;
		public LayerMask solidLayer;
		public AudioClip detectedClip, shootClip;
		public Sprite[] laserSprites;

		[Header("Shared")]
		public float reloadTime;

		readonly List<RaycastHit2D> hits = new();

		bool wasDetected = false, onReset = false;
		float lineSize;
		AnimatorStateInfo animatorState;

		protected override void OnGameResetObject() {
			animator.SetBool("Detected", false);
			animator.Play("Ready", 0, 0f);
			wasDetected = false;
			audioSource.Stop();
			onReset = true;
		}

		private void FixedUpdate() {
			if (Globals.onPause) {
				animator.speed = 0;
				return;
			} else animator.speed = 1;

			if (!animator.IsInTransition(0))
				animatorState = animator.GetCurrentAnimatorStateInfo(0);

			#region Evaluate line distance and detect player
			if (!onReset) {
				lineSize = Constants.worldHypotenuse;

				int count = Physics2D.Raycast(transform.position, RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad), contactFilter, hits, Constants.worldHypotenuse);

				if (count > 0) {
					float playerDistance = lineSize + 1f;

					foreach (RaycastHit2D hit in hits) {
						if (hit.collider.CompareTag("Player") && hit.distance < playerDistance) playerDistance = hit.distance;
						else if (hit.collider.gameObject.CompareLayers(solidLayer) && hit.distance < lineSize) lineSize = hit.distance;
					}

					if (animatorState.IsName("Ready") && playerDistance <= lineSize) animator.SetBool("Detected", true);
				}
			} else onReset = false;

			dotedLine.size = normalLine.size = new(lineSize, dotedLine.size.y);
			laser.size = new(lineSize, laser.size.y);
			#endregion

			#region Check animator state
			if (animatorState.IsName("Detected") && !wasDetected) {
				wasDetected = true;
				animator.SetBool("Detected", false);

				animator.SetFloat("Speed", 1f / reloadTime);

				audioSource.clip = detectedClip;
				audioSource.Play();
			}

			if (animatorState.IsName("Shooting") && wasDetected) {
				wasDetected = false;

				GeneralEventsHandler.SetShake(0.5f, 0.25f);

				audioSource.clip = shootClip;
				audioSource.Play();
			}
			#endregion
		}

		public void SetUpTesting(bool onTest) {
			if (!onTest) {
				dotedLine.size = normalLine.size = new(0f, dotedLine.size.y);
				laser.size = new(0f, laser.size.y);
			}
		}
	}
}
