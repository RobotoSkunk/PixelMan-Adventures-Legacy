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

namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class GameTitle : MonoBehaviour {
		public float speed, maxCurve, sizeDelta;
		
		float __t = 0f;
		float minSize { get => 1.8f - sizeDelta; }


		private void Update() {
			__t += Time.deltaTime * speed;
			if (__t > 360f) __t -= 360f;

			float c = Mathf.Sin(__t * Mathf.Deg2Rad), c2 = Mathf.Sin(__t * 2f * Mathf.Deg2Rad);

			transform.localRotation = Quaternion.Euler(0f, 0f, c * maxCurve);
			transform.localScale = (Vector3)(minSize * Vector2.one) + (Vector3)(sizeDelta * c2 * Vector2.one) + Vector3.forward;
		}
	}
}
