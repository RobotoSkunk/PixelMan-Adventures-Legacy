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
	public class PlayerStuckHandler : MonoBehaviour {
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[SerializeField] LayerMask killzoneLayer;
		[SerializeField] string[] ignoreTags;

		#pragma warning restore IDE0044


		private void OnTriggerEnter2D(Collider2D other)
		{
			CheckIfHasToKill(other);
		}

		private void OnCollisionEnter2D(Collision2D other)
		{
			CheckIfHasToKill(other.collider);
		}


		void CheckIfHasToKill(Collider2D other)
		{
			if ((other.gameObject.layer & killzoneLayer) != 0) {

				foreach (string tag in ignoreTags) {
					if (other.CompareTag(tag)) {
						return;
					}
				}

				Globals.isDead = true;
			}
		}
	}
}
