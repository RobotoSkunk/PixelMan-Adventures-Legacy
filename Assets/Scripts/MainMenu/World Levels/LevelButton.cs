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
using Cysharp.Threading.Tasks;


namespace RobotoSkunk.PixelMan.Utils {
	public class LevelButton : MonoBehaviour {
		[Header("Components")]
		public Image preview;
		public Image[] symbols = new Image[3];
		public bool won, insideTimeLimit, coin;


		Texture2D __texture;


		private void Awake() {
			UniTask.Create(() => {
				Vector2 __size = preview.rectTransform.rect.size / 2f;
				__texture = new((int)__size.x, (int)__size.y) { filterMode = FilterMode.Point };
				__texture.SetColor(Color.black);

				__texture.DrawLine(Vector2.zero, __size, Color.white);
				__texture.DrawLine(new(0f, __size.y), new(__size.x, 0f), Color.white);

				__texture.Apply();


				preview.sprite = Sprite.Create(__texture, new(0, 0, __texture.width, __texture.height), new(0.5f, 0.5f));
				preview.color = Color.white;

				return UniTask.CompletedTask;
			}).Forget();
		}

		private void Update() {
			symbols[0].color = won ? Color.white : default;
			symbols[1].color = insideTimeLimit ? Color.white : default;
			symbols[2].color = coin ? Color.white : default;
		}
	}
}
