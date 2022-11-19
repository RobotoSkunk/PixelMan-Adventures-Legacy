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
			if (!dragging) content.anchoredPosition += new Vector2(0f, speed);
			float pos = content.anchoredPosition.y;

			if (pos > content.rect.height) content.anchoredPosition = new(0f, -viewport.rect.height);
			else if (pos < -viewport.rect.height) content.anchoredPosition = new Vector2(0f, content.rect.height);
		}


		public void OnBeginDrag(PointerEventData eventData) => dragging = true;
		public void OnEndDrag(PointerEventData eventData) => dragging = false;
	}
}
