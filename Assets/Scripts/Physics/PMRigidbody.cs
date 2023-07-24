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


namespace RobotoSkunk.PixelMan.Physics {

	[System.Obsolete("Use Rigidbody2D itself instead.")]
	public class PMRigidbody : MonoBehaviour {
		public Rigidbody2D rb;

		//Vector2 directForce, indirectForce;

		public float horizontalSpeed;

		float horizontalSpeedIndirect = 0f, horizontalSpeedResultant = 0f;

		private void FixedUpdate() {
			if (Globals.onPause) return;

			horizontalSpeedIndirect = Mathf.Lerp(
				horizontalSpeedIndirect,
				0f,
				0.04f + Mathf.Abs(
					RSMath.SafeDivision(horizontalSpeed, rb.velocity.x)
				) * Time.fixedDeltaTime
			);

			horizontalSpeedResultant = horizontalSpeed + horizontalSpeedIndirect;

			rb.velocity = new Vector2(
				horizontalSpeedResultant,
				Mathf.Clamp(
					rb.velocity.y,
					-Constants.maxVelocity,
					Constants.maxVelocity
				)
			);
		}

		public void ResetSpeed() {
			horizontalSpeedIndirect = 0f;
			horizontalSpeedResultant = 0f;
			rb.velocity = Vector2.zero;
		}

		public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
			horizontalSpeedIndirect += force.x * rb.mass * (mode == ForceMode2D.Impulse ? 1f : Time.fixedDeltaTime);

			rb.AddForce(new Vector2(0f, force.y), mode);
		}
	}
}
