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
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class Credits : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
		public ScrollRect scrollRect;
		public float speed;

		bool dragging = false;
		RectTransform content { get => scrollRect.content; }
		RectTransform viewport { get => scrollRect.viewport; }


		private void Start() => content.anchoredPosition = new Vector2(0f, -viewport.rect.height);

		private void Update() {
			if (!dragging) content.anchoredPosition += new Vector2(0f, speed * RSTime.delta);
			float pos = content.anchoredPosition.y;

			if (pos > content.rect.height) content.anchoredPosition = new(0f, -viewport.rect.height);
			else if (pos < -viewport.rect.height) content.anchoredPosition = new Vector2(0f, content.rect.height);
		}


		public void OnBeginDrag(PointerEventData eventData) => dragging = true;
		public void OnEndDrag(PointerEventData eventData) => dragging = false;
	}
}
