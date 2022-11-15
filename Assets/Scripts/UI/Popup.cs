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
