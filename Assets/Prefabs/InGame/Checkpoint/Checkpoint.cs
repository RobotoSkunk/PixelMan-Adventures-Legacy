using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;

using RobotoSkunk.PixelMan.Events;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Checkpoint : GameHandler {
		[Header("Components")]
		public BoxCollider2D boxCollider;
		public SpriteRenderer counterContainer, counter;

		[Header("Properties")]
		public Sprite[] numbers;

		[Header("Shared")]
		public uint attempts;

		bool onUse, doAnimation, destroyed;
		float ang, newAng;

		protected override void OnGameResetObject() {
			ang = newAng = 0f;
			doAnimation = destroyed = onUse = false;
		}

		private void FixedUpdate() {
			if (Globals.onPause) return;

			if (doAnimation) {
				newAng += 360f;
				doAnimation = false;
			}

			ang = Mathf.Lerp(ang, newAng, 0.2f);
			counterContainer.transform.rotation = Quaternion.Euler(0, 0, ang);

			counter.sprite = !onUse ? null : numbers.ClampIndex((int)Globals.respawnAttempts);
			counter.enabled = counterContainer.enabled = !destroyed;
		}

		private void OnTriggerEnter2D(Collider2D collision) {
			if (collision.CompareTag("Player")) {
				if (!onUse && !destroyed) {
					onUse = doAnimation = true;
					Globals.respawnAttempts = attempts;
					Globals.respawnPoint = transform.position + RSMath.GetDirVector((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad);
					Globals.checkpointId = gameObject.GetInstanceID();

					GameEventsHandler.InvokeNewCheckpoint();
				}
			}
		}

		protected override void OnGameCheckpointEnabled() {
			if (onUse && Globals.checkpointId != gameObject.GetInstanceID()) destroyed = true;
		}

		protected override void OnGameCheckpointRespawn() {
			if (onUse && !destroyed && Globals.respawnAttempts > 0) {
				doAnimation = true;
				Globals.respawnAttempts--;

				if (Globals.respawnAttempts == 0) destroyed = true;
			}
		}
	}
}
