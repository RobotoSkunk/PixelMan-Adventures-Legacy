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

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Killzone : GameObjectBehaviour {
		[Header("Components")]
		public SpriteRenderer sprRend;

		[Header("Properties")]
		public ContactFilter2D ContactFilter2D;
		public float radius = 5f;

		float delta = 0f;
		readonly List<Collider2D> overlapResult = new();

		bool inEditor = false;

		private void Start() {
			if (!inEditor) sprRend.color = Color.clear;
		}

		private void FixedUpdate() {
			if (inEditor) {
				int buffer = Physics2D.OverlapCircle(transform.position, radius, ContactFilter2D, overlapResult);
				float lastDistance = radius + 1f;

				if (buffer != 0) {
					foreach (Collider2D col in overlapResult) {
						float distance = Vector2.Distance(transform.position, col.transform.position);

						if (distance < lastDistance) lastDistance = distance;
					}
				}

				delta = Mathf.Lerp(delta, Mathf.Clamp01((radius - lastDistance) / radius), 0.3f);
				sprRend.color = new Color(1f, 1f, 1f, 0.2f + 0.8f * delta);
			}
		}

		public void InEditor(bool inEditor) => this.inEditor = inEditor;
		protected override void OnGameResetObject() {
			if (inEditor) sprRend.color = Color.white;
		}
	}
}
