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
