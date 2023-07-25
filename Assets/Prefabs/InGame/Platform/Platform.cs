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


namespace RobotoSkunk.PixelMan.Gameplay
{
	public class Platform : GameObjectBehaviour
	{
		#region Inspector variables
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[Header("Properties")]
		[SerializeField] Rigidbody2D rigidbody2d;
		[SerializeField] InGameObjectBehaviour platformBehaviour;

		// [HideInInspector] public Vector2 lastPublicPosition;

		#pragma warning restore IDE0044
		#endregion


		float delayToMove = 0f;

		bool goToPositiveStartValue;
		bool goToPositive;

		Vector2 lastPosition;
		Vector2 startPosition;

		Direction direction;

		public enum Direction {
			HORIZONTAL,
			VERTICAL,
		}


		Vector2 directionVector {
			get {
				if (direction == Direction.HORIZONTAL) {
					return Vector2.right;
				}

				return Vector2.up;
			}
		}



		override protected void OnGameReady()
		{
			InGameObjectProperties.Direction _direction = platformBehaviour.properties.direction;

			if (
				_direction == InGameObjectProperties.Direction.Left ||
				_direction == InGameObjectProperties.Direction.Right
			) {
				direction = Direction.HORIZONTAL;
			} else {
				direction = Direction.VERTICAL;
			}


			if (direction == Direction.VERTICAL) {
				rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionX;
			} else {
				rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionY;
			}

			rigidbody2d.constraints |= RigidbodyConstraints2D.FreezeRotation;


			goToPositive = _direction == InGameObjectProperties.Direction.Right
						|| _direction == InGameObjectProperties.Direction.Up;


			startPosition = transform.position;
			goToPositiveStartValue = goToPositive;
		}

		private void FixedUpdate()
		{
			if (Globals.onPause) {
				rigidbody2d.velocity = Vector2.zero;
				return;
			}


			if (delayToMove > 0f) {
				delayToMove -= Time.fixedDeltaTime;
				rigidbody2d.velocity = Vector2.zero;

				lastPosition = Mathf.Infinity * Vector2.one;
			} else {
				rigidbody2d.velocity = platformBehaviour.properties.speed * (goToPositive ? 1f : -1f) * directionVector;

				if (Vector2.Distance(transform.position, lastPosition) < 0.05f) {
					goToPositive = !goToPositive;
					delayToMove = 0.25f;
				}

				lastPosition = transform.position;
			}
		}

		protected override void OnGameResetObject()
		{
			transform.position = startPosition;

			goToPositive = goToPositiveStartValue;

			rigidbody2d.velocity = Vector2.zero;
		}
	}
}
