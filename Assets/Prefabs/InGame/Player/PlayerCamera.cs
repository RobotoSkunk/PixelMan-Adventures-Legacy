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
	public class PlayerCamera : GameObjectBehaviour {
		[Header("Components")]
		public Camera cam;

		[Header("Properties")]
		public float orthoDefault;
		public float  maxSpeed;

		[Header("Shared")]
		public Vector2 look;


		Vector2 vTo;
		Vector2 camPos;
		Vector2  lookAt;

		Vector3 startPos;
		Rigidbody2D player;

		float velocity;
		float zoom;


		public void SetPlayer(Rigidbody2D playerBody)
		{
			player = playerBody;
		}

		private void Start()
		{
			camPos = startPos = transform.position;
		}

		protected override void OnGameResetObject()
		{
			velocity = zoom = 0f;
			look = vTo = Vector2.zero;
			camPos = transform.position = startPos;
			cam.orthographicSize = orthoDefault;
		}

		private void FixedUpdate()
		{
			if (player != null) {
				vTo = player.position + (Vector2)(
					RSMath.GetDirVector(RSMath.Direction(camPos, player.position)) *
					Mathf.Min(
						maxSpeed,
						Vector2.Distance(camPos, player.position)
					)
				);

				if (!Globals.onPause) {
					velocity = player.velocity.magnitude;
				}
			} else {
				velocity = 0f;
				look = Vector2.zero;
			}

			lookAt = Vector2.Lerp(lookAt, 2f * look, 0.25f);

			zoom = Mathf.Lerp(zoom, velocity / Constants.maxVelocity, 0.01f);
			cam.orthographicSize = orthoDefault + zoom * orthoDefault;

			camPos += (vTo - camPos) / 30f;

			transform.position = (Vector3)(
					camPos +
					(1f + zoom) * ((Globals.shakeForce * 0.5f * Random.insideUnitCircle) + lookAt)
				) +
				new Vector3(0f, 0f, -10f);
		}
	}
}
