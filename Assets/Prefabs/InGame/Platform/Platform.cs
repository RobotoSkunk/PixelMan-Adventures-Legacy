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

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Platform : GameHandler {
		[Header("Properties")]
		public Rigidbody2D rb;
		public BoxCollider2D col;
		public LayerMask layer;
		public InGameObjectBehaviour platformBehaviour;

		[HideInInspector] public Vector2 lastPosition;

		float time = 0f;
		bool goPosStart, goPositive;
		Vector2 lastPos, startPos, velocity;
		Direction dir;

		public enum Direction { HORIZONTAL, VERTICAL }

		override protected void OnGameReady() {
			InGameObjectProperties.Direction direction = platformBehaviour.properties.direction;
			dir = direction == InGameObjectProperties.Direction.Left || direction == InGameObjectProperties.Direction.Right ? Direction.HORIZONTAL : Direction.VERTICAL;

			rb.constraints = (dir == Direction.VERTICAL ? RigidbodyConstraints2D.FreezePositionX : RigidbodyConstraints2D.FreezePositionY) | RigidbodyConstraints2D.FreezeRotation;
			goPositive = direction == InGameObjectProperties.Direction.Right || direction == InGameObjectProperties.Direction.Up;

			startPos = transform.position;
			goPosStart = goPositive;
		}

		private void FixedUpdate() {
			if (Globals.onPause) {
				rb.velocity = Vector2.zero;
				return;
			}

			if (time > 0f) {
				time -= Time.fixedDeltaTime;
				velocity = Vector2.zero;

				lastPos += Vector2.one;
			} else {
				if (dir == Direction.HORIZONTAL)
					velocity = new Vector2(platformBehaviour.properties.speed * (goPositive ? 1f : -1f), 0f);
				else
					velocity = new Vector2(0f, platformBehaviour.properties.speed * (goPositive ? 1f : -1f));

				if (Vector2.Distance(transform.position, lastPos) < 0.05f) {
					goPositive = !goPositive;
					time = 0.25f;
				}

				lastPos = transform.position;
			}

			rb.velocity = velocity;
			lastPosition = transform.position;
		}

		protected override void OnGameResetObject() {
			transform.position = startPos;
			goPositive = goPosStart;
			rb.velocity = velocity = Vector2.zero;
		}
	}
}
