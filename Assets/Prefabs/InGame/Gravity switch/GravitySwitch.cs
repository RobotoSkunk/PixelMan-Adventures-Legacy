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
	public class GravitySwitch : GameObjectBehaviour
	{
		[Header("Components")]
		public CircleCollider2D circleCollider;
		public SpriteRenderer spriteParticles;
		public SpriteRenderer loadingSprite;
		public AudioSource audioSource;
		public InGameObjectBehaviour gravityBehaviour;

		float ang;
		float time;
		float rotFactor;

		readonly float maxSize = 1.25f;


		private void Start()
		{
			rotFactor = RSRandom.UnionRange(-8f, -5f, 5f, 8f);
		}

		protected override void OnGameResetObject()
		{
			time = ang = 0f;
			spriteParticles.gameObject.transform.rotation = default;
			loadingSprite.size = maxSize * Vector2.one;
			loadingSprite.transform.localPosition = default;
			spriteParticles.enabled = true;
		}


		private void Update()
		{
			if (Globals.onPause) {
				return;
			}

			if (spriteParticles.isVisible) {
				ang += rotFactor * RSTime.delta;
				spriteParticles.gameObject.transform.rotation = Quaternion.Euler(0, 0, ang);
			}

			if (loadingSprite.isVisible) {
				loadingSprite.size = maxSize * new Vector2(
					1f,
					Mathf.Clamp01(
						(gravityBehaviour.properties.safeReloadTime - time) /
						gravityBehaviour.properties.safeReloadTime
					)
				);

				loadingSprite.transform.localPosition = maxSize / 2f *
														(time / gravityBehaviour.properties.safeReloadTime) *
														Vector2.down;
			}

			if (time > 0f) {
				time -= Time.deltaTime;
			}

			circleCollider.enabled = spriteParticles.enabled = time <= 0f;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (time > 0) {
				return;
			}

			bool effectsApplied = false;

			if (collision.CompareTag("Player")) {
				Player player = collision.GetComponent<Player>();
				player.InvertGravity();

				effectsApplied = true;
			}


			if (effectsApplied) {
				time = gravityBehaviour.properties.safeReloadTime;
				audioSource.Play();
			}
		}
	}
}
