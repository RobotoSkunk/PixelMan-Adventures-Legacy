/*
	PixelMan Adventures, an open source platformer game.
	Copyright (C) 2022  RobotoSkunk <contact@robotoskunk.com>

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Affero General Public License as published
	by the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Affero General Public License for more details.

	You should have received a copy of the GNU Affero General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;

using RobotoSkunk.PixelMan.Events;



namespace RobotoSkunk.PixelMan.Gameplay {
	public class Checkpoint : GameObjectBehaviour {
		[Header("Components")]
		public BoxCollider2D boxCollider;
		public SpriteRenderer counterContainer, counter;

		[Header("Properties")]
		public Sprite[] numbers;

		[Header("Shared")]
		public uint attempts;

		bool onUse, doAnimation, destroyed, onEditor;
		float ang, newAng;

		protected override void OnGameResetObject() {
			ang = newAng = 0f;
			doAnimation = destroyed = onUse = false;
			counterContainer.transform.rotation = default;

			SetCounterSprite();
		}

		void SetCounterSprite() {
			if (onEditor) {
				counter.sprite = numbers.ClampIndex((int)attempts);
				counter.enabled = counterContainer.enabled = true;
				return;
			}

			counter.sprite = !onUse ? null : numbers.ClampIndex((int)Globals.respawnAttempts);
			counter.enabled = counterContainer.enabled = !destroyed;
		}

		private void FixedUpdate() {
			if (Globals.onPause) return;

			if (doAnimation) {
				newAng += 360f;
				doAnimation = false;
			}

			ang = Mathf.Lerp(ang, newAng, 0.2f);
			counterContainer.transform.rotation = Quaternion.Euler(0, 0, ang);

			SetCounterSprite();
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

			Debug.Log("Called!");
		}

		public void OnPropertiesChange() => SetCounterSprite();
		public void IsOnEditor(bool trigger) {
			onEditor = trigger;
			SetCounterSprite();
		}
	}
}
