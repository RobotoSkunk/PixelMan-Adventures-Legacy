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

using RobotoSkunk.PixelMan.Events;


namespace RobotoSkunk.PixelMan.Gameplay
{
	public class Bullet : GameObjectBehaviour
	{
		[Header("Components")]
		public Rigidbody2D rb;
		public SpriteRenderer spriteRenderer;
		public ParticleSystem explosionParticles;

		[Header("Properties")]
		public AudioClip audioClip;
		public LayerMask layerMask;
		public LayerMask importantLayers;
		public Sprite[] sprites;
		public List<string> tags;

		int index;
		Vector2 speed;


		private void Start() => speed = rb.velocity;

		private void FixedUpdate()
		{
			rb.velocity = !Globals.onPause ? speed : Vector2.zero;

			if (spriteRenderer.isVisible && !Globals.onPause && RSTime.fixedFrameCount % 3 == 0) {
				index = index == 0 ? 1 : 0;
				spriteRenderer.sprite = sprites[index];
			}
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (explosionParticles && (1 << collision.gameObject.layer & layerMask) != 0) {
				if ((1 << collision.gameObject.layer & importantLayers) != 0 || tags.Contains(collision.tag)) {
					explosionParticles.transform.parent = null;
					explosionParticles.Play();

					GeneralEventsHandler.PlayOnBackground(audioClip);

					if (collision.gameObject.CompareTag("Player")) {
						Globals.isDead = true;
					}

					DestroyMyself();
				}
			}
		}

		protected override void OnGameResetObject() => DestroyMyself();

		void DestroyMyself()
		{
			rb.velocity = Vector2.zero;
			Destroy(gameObject, Time.fixedDeltaTime);
		}
	}
}
