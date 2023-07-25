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
		public float horizontalOffset;


		[HideInInspector] public Vector2 look;

		Vector2 rawDestination;
		Vector2 cameraPosition;
		Vector2 playerLookAtStick;

		Vector3 startPos;
		Rigidbody2D player;

		float playerVelocity = 0f;
		float playerDirection = 0f;
		float zoom = 0f;
		float horizontalOffsetToAdd = 0f;



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

		Vector2 missingSize {
			get {
				return new Vector2(
					1f - cameraSize.x / (Globals.levelData.bounds.width + 2f),
					1f - cameraSize.y / (Globals.levelData.bounds.height + 2f)
				);
			}
		}

		enum AdjustZoomTo {
			None,
			Width = 1 << 0,
			Height = 1 << 1,
			Both = Width | Height
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

			// look = Vector2.zero;
			// rawDestination = Vector2.zero;

			cameraPosition = startPos;
			transform.position = startPos;

			// cam.orthographicSize = orthoDefault;
		}

		private void FixedUpdate()
		{
			if (Globals.onPause) {
				return;
			}

			if (player != null) {
				playerVelocity = player.velocity.magnitude;


				if (Mathf.Abs(player.velocity.x) > 0.1f) {
					playerDirection = Mathf.Sign(player.velocity.x);
				}

				horizontalOffsetToAdd = Mathf.Lerp(horizontalOffsetToAdd, playerDirection * horizontalOffset, 0.07f);


				rawDestination = player.position + new Vector2(horizontalOffsetToAdd, 0f);
			} else {
				playerVelocity = 0f;
				look = Vector2.zero;
			}

			playerLookAtStick = Vector2.Lerp(playerLookAtStick, 2f * look, 0.25f);


			#region Camera Bounds
			AdjustZoomTo adjustZoomTo = AdjustZoomTo.None;

			if (cameraSize.x > levelBounds.width) {
				rawDestination.x = Globals.levelData.bounds.center.x;
				
				adjustZoomTo |= AdjustZoomTo.Width;
			} else {
				rawDestination.x = Mathf.Clamp(rawDestination.x, levelBounds.xMin, levelBounds.xMax);
			}

			if (cameraSize.y > levelBounds.height) {
				rawDestination.y = Globals.levelData.bounds.center.y;

				adjustZoomTo |= AdjustZoomTo.Height;
			} else {
				rawDestination.y = Mathf.Clamp(rawDestination.y, levelBounds.yMin, levelBounds.yMax);
			}
			#endregion


			switch (adjustZoomTo) {
				case AdjustZoomTo.Both:
					if (cameraSize.x > cameraSize.y) {
						zoom = missingSize.x;
					} else {
						zoom = missingSize.y;
					}
					break;

				case AdjustZoomTo.Width:
					zoom = missingSize.x;
					break;

				case AdjustZoomTo.Height:
					zoom = missingSize.y;
					break;

				default:
					zoom = Mathf.Lerp(zoom, playerVelocity / Constants.maxVelocity, 0.01f);
					break;
			}

			cam.orthographicSize = orthoDefault + orthoDefault * zoom;
			cameraPosition += (rawDestination - cameraPosition) / 15f;


			transform.position = (Vector3)(
					cameraPosition +
					(1f + zoom) * ((Globals.shakeForce * 0.5f * Random.insideUnitCircle) + playerLookAtStick)
				) +
				new Vector3(0f, 0f, -10f);
		}
	}
}
