using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

using RobotoSkunk.PixelMan.Gameplay;


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
