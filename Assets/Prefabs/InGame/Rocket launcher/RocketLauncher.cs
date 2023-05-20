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


namespace RobotoSkunk.PixelMan.Gameplay
{
	public class RocketLauncher : GameObjectBehaviourExtended
	{
		[Header("Components")]
		public SpriteRenderer spriteRenderer;
		public AudioSource audioSource;
		public InGameObjectBehaviour launcherBehaviour;

		[Header("Properties")]
		public ContactFilter2D lineFilter;
		public Rocket rocket;

		float time = 1f, ang, newAng;
		readonly List<RaycastHit2D> lineResults = new();


		protected override void OnGameReady()
		{
			base.OnGameReady();
			time = launcherBehaviour.properties.safeReloadTime;
		}

		protected override void OnGameResetObject()
		{
			time = launcherBehaviour.properties.safeReloadTime;
			ang = newAng = 0f;
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}

		private void FixedUpdate()
		{
			if (Globals.onPause) {
				return;
			}

			bool onCount = false;

			if (!Globals.isDead) {
				GameObject target = NearestPlayer();

				if (target) {
					int lineBuffer = Physics2D.Linecast(
						transform.position,
						target.transform.position,
						lineFilter,
						lineResults
					);

					if (lineBuffer == 0) {
						newAng = RSMath.Direction(transform.position, target.transform.position) * Mathf.Rad2Deg;

						onCount = true;

						if (time > 0f) time -= Time.fixedDeltaTime;
						else {
							Rocket newObj = Instantiate(
								rocket,
								transform.position + RSMath.GetDirVector(ang * Mathf.Deg2Rad),
								Quaternion.Euler(0f, 0f, ang)
							);
							newObj.transform.localScale = transform.localScale;
							newObj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
							newObj.target = target;

							time = launcherBehaviour.properties.safeReloadTime;
							audioSource.Play();
						}
					}
				}
			}

			if (!onCount) {
				time = 1f;
				newAng++;
			}
			ang += Mathf.Sin((newAng - ang) * Mathf.Deg2Rad) * 5f;

			transform.rotation = Quaternion.Euler(0f, 0f, ang);
		}
	}
}
