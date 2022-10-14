using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;



namespace RobotoSkunk.PixelMan.UI {
	[AddComponentMenu("UI/RobotoSkunk - Rect Slider")]
	public class RSRectSlider : MonoBehaviour, IDragHandler  {
		[System.Serializable]
		public class SliderEvent : UnityEvent<Vector2> { }

		public RectTransform handler;
		public RectTransform rectTransform;
		public SliderEvent onValueChanged = new();

		Vector2 __value;


		public Vector2 value {
			get => __value;
			set {
				SetValueWithoutNotify(value);
				onValueChanged.Invoke(__value);
			}
		}


		public void SetValueWithoutNotify(Vector2 value) {
			__value = RSMath.Clamp(value, Vector2.zero, Vector2.one);
			handler.anchoredPosition = value * rectTransform.rect.size;
		}

		public void OnDrag(PointerEventData ev) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, ev.position, ev.pressEventCamera, out Vector2 localPoint);
			value = localPoint / rectTransform.rect.size;
		}
	}
}
