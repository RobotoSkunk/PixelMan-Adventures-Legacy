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
	public class Block : GameObjectBehaviour
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


		bool playerIsNear = false;


		private void Start()
		{
			if (!behaviour.properties.isFake) {
				Destroy(this);
				return;
			}

			boxCollider.isTrigger = true;
		}

		private void Update()
		{
			if (spriteRenderer.isVisible && behaviour.properties.isFake) {
				Color currentColor = spriteRenderer.color;

				currentColor.a = Mathf.Lerp(currentColor.a, playerIsNear ? 0.5f : 0f, Time.deltaTime * 5f);

				spriteRenderer.color = currentColor;
			}
		}


		private void OnTriggerEnter2D(Collider2D other)
		{
			if (other.CompareTag("Player")) {
				playerIsNear = true;
			}
		}

		private void OnTriggerExit2D(Collider2D other)
		{
			if (other.CompareTag("Player")) {
				playerIsNear = false;
			}
		}
	}
}
