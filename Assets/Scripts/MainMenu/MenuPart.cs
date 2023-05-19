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


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class MenuPart : MonoBehaviour {
		public Vector2 posOnClosed;
		public bool usePercentage;
		[Range(0f, 1f)] public float delta = 0.3f;

		[HideInInspector] public bool isOpen = false;
		[HideInInspector] public RectTransform rectTransform;
		[HideInInspector] public Vector2 startPos;


		public Vector2 positionOnClosed {
			get {
				if (!usePercentage) return posOnClosed;
				return posOnClosed * rectTransform.rect.size;
			}
		}
		public Vector2 nextPos {
			get {
				if (isOpen) return startPos;
				return startPos + positionOnClosed;
			}
		}


		private void Awake() {
			rectTransform = GetComponent<RectTransform>();
			startPos = rectTransform.anchoredPosition;
		}
	}
}
