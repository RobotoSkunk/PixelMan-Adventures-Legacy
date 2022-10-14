using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class Credits : MonoBehaviour, IBeginDragHandler, IEndDragHandler {
		public ScrollRect scrollRect;
		public TextMeshProUGUI credits;
		public float speed, padding;
		bool dragging = false;


		float contentHeight { get => scrollRect.content.rect.height + padding; }
		float delta { get => scrollRect.viewport.rect.height / contentHeight; }

		float bottom { get => -contentHeight * delta; }
		float top { get => contentHeight; }


		private void Awake() => credits.text = Globals.creditsText;
		private void Start() => SetContentPos(bottom);

		private void Update() {
			if (delta >= 1f) {
				scrollRect.verticalNormalizedPosition = 0.5f;
				return;
			}
			if (!dragging) scrollRect.verticalNormalizedPosition += Time.deltaTime * speed * delta;

			if (scrollRect.content.anchoredPosition.y > top) SetContentPos(bottom);
			else if (scrollRect.content.anchoredPosition.y < bottom) SetContentPos(top);
		}

		void SetContentPos(float y) {
			Vector2 pos = scrollRect.content.anchoredPosition; pos.y = y;
			scrollRect.content.anchoredPosition = pos;
		}


		public void OnBeginDrag(PointerEventData eventData) => dragging = true;
		public void OnEndDrag(PointerEventData eventData) => dragging = false;
	}
}
