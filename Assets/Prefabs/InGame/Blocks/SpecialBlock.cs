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

using System.Collections.Generic;

using UnityEngine;

using RobotoSkunk.PixelMan.Events;


namespace RobotoSkunk.PixelMan.Gameplay {
	public class SpecialBlock : GameHandler {
		[Header("Components")]
		public BoxCollider2D box;
		public SpriteRenderer spriteRenderer;

		[Header("Properties")]
		public ContactFilter2D contactFilter;
		public Sprite[] sprites;
		public List<string> tags;

		readonly List<Collider2D> collider2Ds = new();

		float time = 0f;
		bool destroy;
		int sprIndx;

		protected override void OnGameResetObject() {
			time = 0f;
			destroy = false;
			sprIndx = 0;
			spriteRenderer.sprite = sprites[0];
			box.enabled = true;
			spriteRenderer.enabled = true;
			PhysicsEventsHandler.GenerateCompositeGeometry();
		}

		private void FixedUpdate() {
			if (Globals.onPause) return;

			if (!destroy) {
				int nmb = Physics2D.OverlapBox(transform.position, transform.localScale, 0f, contactFilter, collider2Ds);

				if (nmb > 0) {
					foreach (Collider2D c in collider2Ds) {
						if (tags.Contains(c.tag)) {
							destroy = true;
							time = 0.35f;
						}
					}
				}
			} else {
				if (time > 0) {
					time -= Time.fixedDeltaTime;

					if (RSTime.fixedFrameCount % 3 == 0 && spriteRenderer.isVisible) {
						sprIndx = sprIndx == 1 ? 0 : 1;

						spriteRenderer.sprite = sprites[sprIndx];
					}
				} else if (box.enabled) {
					box.enabled = false;
					spriteRenderer.enabled = false;
					PhysicsEventsHandler.GenerateCompositeGeometry();
				}
			}
		}
	}
}
