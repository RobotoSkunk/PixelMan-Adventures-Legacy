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
	public class Block : GameHandlerBehaviourExtended
	{
		[Header("Generic Properties")]
		public InGameObjectBehaviour behaviour;
		public SpriteRenderer spriteRenderer;
		public BoxCollider2D boxCollider;

		// [Header("Sprite renderer and data")]
		// public Sprite defaultSprite;
		// public BlockData[] data = new BlockData[47];


		// public struct BlockData
		// {
		// 	public Sprite sprite;
		// 	public float angle;
		// }


		private void Start()
		{
			if (!behaviour.properties.isFake) {
				Destroy(this);
				return;
			}

			boxCollider.enabled = false;
		}

		private void Update()
		{
			if (behaviour.properties.isFake) {
				// Fade out the block if the player is close
				GameObject nearestPlayer = NearestPlayer();

				if (nearestPlayer != null) {
					float distance = Vector2.Distance(nearestPlayer.transform.position, transform.position);


					Color currentColor = spriteRenderer.color;

					if (distance < 5f) {
						currentColor.a = Mathf.Lerp(currentColor.a, 0f, RSTime.delta * 0.5f);
					} else {
						currentColor.a = Mathf.Lerp(currentColor.a, 1f, RSTime.delta * 0.5f);
					}

					spriteRenderer.color = currentColor;
				}
			}
		}
	}
}
