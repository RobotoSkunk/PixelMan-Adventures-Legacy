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

namespace RobotoSkunk.PixelMan.Gameplay {
	public class SwivelGun : GameObjectBehaviour {
		[Header("Components")]
		public SpriteRenderer spriteRenderer;
		public AudioSource audioSource;
		public InGameObjectBehaviour gunBehaviour;

		[Header("Properties")]
		public ContactFilter2D lineFilter;
		public Bullet bullet;

		float time = 1f, ang;
		readonly List<RaycastHit2D> lineResults = new();
		readonly List<GameObject> players = new();

		protected override void OnGameReady() {
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
			players.Clear();

			foreach (GameObject g in gameObjects) players.Add(g);
			time = gunBehaviour.properties.safeReloadTime;
		}

		protected override void OnGameResetObject() {
			time = gunBehaviour.properties.safeReloadTime;
			ang = 0f;
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}

		private void FixedUpdate() {
			if (Globals.onPause) return;

			bool onCount = false;

			if (!Globals.isDead) {
				GameObject target = null;
				float b = Constants.worldLimit;

				foreach (GameObject c in players) {
					float d = Vector2.Distance(transform.position, c.transform.position);

					if (d < b) {
						target = c;
						b = d;
					}
				}

				if (target) {
					int lineBuffer = Physics2D.Linecast(transform.position, target.transform.position, lineFilter, lineResults);

					if (lineBuffer == 0) {
						float __z = RSMath.Direction(transform.position, target.transform.position) * Mathf.Rad2Deg;
						ang += Mathf.Sin((__z - ang) * Mathf.Deg2Rad) * 10f;

						transform.rotation = Quaternion.Euler(0f, 0f, ang);

						onCount = true;

						if (time > 0f) time -= Time.fixedDeltaTime;
						else {
							Bullet newObj = Instantiate(
								bullet,
								transform.position + RSMath.GetDirVector(ang * Mathf.Deg2Rad),
								Quaternion.Euler(0f, 0f, ang)
							);
							newObj.transform.localScale = transform.localScale;
							newObj.rb.velocity = 15f * transform.localScale.x * RSMath.GetDirVector(ang * Mathf.Deg2Rad);
							newObj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

							time = gunBehaviour.properties.safeReloadTime;
							audioSource.Play();
						}
					}
				}
			}

			if (!onCount) time = gunBehaviour.properties.safeReloadTime;
		}
	}
}
