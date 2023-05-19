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
	public class StaticGun : GameHandler {
		[Header("Components")]
		public SpriteRenderer spriteRenderer;
		public AudioSource audioSource;
		public InGameObjectBehaviour gunBehaviour;

		[Header("Properties")]
		public Bullet bullet;

		float time;

		void ResetTime() => time = gunBehaviour.properties.wakeTime;

		protected override void OnGameReady() => ResetTime();
		protected override void OnGameResetObject() => ResetTime();

		private void FixedUpdate() {
			if (Globals.onPause) return;

			if (time > 0f) time -= Time.fixedDeltaTime;
			else {
				Bullet newObj = Instantiate(
					bullet,
					transform.position + 0.15f * RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad),
					Quaternion.Euler(0f, 0f, transform.eulerAngles.z)
				);
				newObj.transform.localScale = transform.localScale;
				newObj.rb.velocity = 15f * transform.localScale.x * RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad);
				newObj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

				time = gunBehaviour.properties.safeReloadTime;
				audioSource.Play();
			}
		}
	}
}
