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
		public float maxSpeed;

		[Header("Shared")]
		public Vector2 look;


		Vector2 rawDestination;
		Vector2 cameraPosition;
		Vector2 playerLookAtStick;

		Vector3 startPos;
		Rigidbody2D player;

		float playerVelocity;
		float zoom;


		/// <summary>
		/// The actual size of the viewport in world units.
		/// </summary>
		Vector2 cameraSize {
			get {
				float h = orthoDefault * 2f;
				float w = h * cam.aspect;

				return new(w, h);
			}
		}

		Rect levelBounds {
			get {
				if (Globals.levelData == null) {
					return new Rect(
						Constants.levelDefaultSize / -2f,
						Constants.levelDefaultSize
					);
				}

				Rect bounds = Globals.levelData.bounds;

				bounds.min += cameraSize / 2f;
				bounds.max -= cameraSize / 2f;

				return bounds;
			}
		}



		public void SetPlayer(Rigidbody2D playerBody)
		{
			player = playerBody;
		}

		private void Start()
		{
			cameraPosition = startPos = transform.position;
		}

		protected override void OnGameResetObject()
		{
			playerVelocity = 0f;
			zoom = 0f;

			look = Vector2.zero;
			rawDestination = Vector2.zero;

			cameraPosition = startPos;
			transform.position = startPos;

			cam.orthographicSize = orthoDefault;
		}

		private void FixedUpdate()
		{
			if (Globals.onPause) {
				return;
			}

			if (player != null) {
				//+ (Vector2)(
				// 	RSMath.GetDirVector(RSMath.Direction(cameraPosition, player.position)) *
				// 	Mathf.Min(
				// 		maxSpeed,
				// 		Vector2.Distance(cameraPosition, player.position)
				// 	)
				// );

				playerVelocity = player.velocity.magnitude;

				rawDestination = player.position + new Vector2(player.velocity.x, 0f);
			} else {
				playerVelocity = 0f;
				look = Vector2.zero;
			}

			playerLookAtStick = Vector2.Lerp(playerLookAtStick, 2f * look, 0.25f);

			zoom = Mathf.Lerp(zoom, playerVelocity / Constants.maxVelocity, 0.01f);
			cam.orthographicSize = orthoDefault + orthoDefault * zoom;


			#region Camera Bounds
			// Debug.Log($"{Globals.levelData.bounds} | {levelBounds}");

			if (cameraSize.x > levelBounds.width) {
				rawDestination.x = Globals.levelData.bounds.center.x;
			} else {
				rawDestination.x = Mathf.Clamp(rawDestination.x, levelBounds.xMin, levelBounds.xMax);
			}

			if (cameraSize.y > levelBounds.height) {
				rawDestination.y = Globals.levelData.bounds.center.y;
			} else {
				rawDestination.y = Mathf.Clamp(rawDestination.y, levelBounds.yMin, levelBounds.yMax);
			}
			#endregion

			cameraPosition += (rawDestination - cameraPosition) / 30f;


			transform.position = (Vector3)(
					cameraPosition +
					(1f + zoom) * ((Globals.shakeForce * 0.5f * Random.insideUnitCircle) + playerLookAtStick)
				) +
				new Vector3(0f, 0f, -10f);
		}
	}
}
