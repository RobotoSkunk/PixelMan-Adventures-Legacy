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


namespace RobotoSkunk.PixelMan.UI {
	public class Popup : MonoBehaviour {
		[Header("Data")]
		public GameObject[] content;
		public bool open;

		[Header("Layout")]
		public CanvasGroup group;
		public RectTransform upperBar, lowerBar;
		public Image bg;
		public GameObject[] children;


		int _index;
		float delta;

		public int index {
			get => _index;
			set {
				if (value < 0 || value >= content.Length) return;
				_index = value;
				for (int i = 0; i < content.Length; i++) {
					content[i].SetActive(i == _index);
				}
			}
		}

		private void Awake() => Update();
		private void Update() {
			delta = Mathf.Lerp(delta, open.ToInt(), 0.2f * RSTime.delta);

			group.blocksRaycasts = open;
			bg.raycastTarget = open;
			group.alpha = delta;

			bg.color = new Color(0f, 0f, 0f, 0.8f * delta);
			upperBar.anchoredPosition = new((1f - delta) * upperBar.rect.size.x, upperBar.anchoredPosition.y);
			lowerBar.anchoredPosition = new((1f - delta) * -lowerBar.rect.size.x, lowerBar.anchoredPosition.y);

			for (int i = 0; i < children.Length; i++)
				children[i].SetActive(delta > 0.01f);
		}
	}
}
