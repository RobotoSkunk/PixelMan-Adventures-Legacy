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
	public class Saw : GameObjectBehaviour {
		[Header("Components")]
		public SpriteRenderer sprRend;

		float speed = 0f, ang = 0f;

		private void Start() {
			speed = Random.Range(15f, 25f) * RSRandom.Sign();
		}

		protected override void OnGameResetObject() {
			sprRend.transform.rotation = default;
			sprRend.transform.localPosition = default;
			ang = 0f;
		}

		public void Update() {
			if (!Globals.onPause && sprRend.isVisible) {
				ang += speed * RSTime.delta;

				sprRend.transform.localPosition = Random.insideUnitCircle * 0.1f;
				sprRend.transform.rotation = Quaternion.Euler(0, 0, ang);
			}
		}
	}
}
