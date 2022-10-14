using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;



namespace RobotoSkunk.PixelMan.UI {
	[AddComponentMenu("UI/RobotoSkunk - Radial Slider")]
	public class RSRadialSlider : MonoBehaviour, IDragHandler  {
		[System.Serializable]
		public class SliderEvent : UnityEvent<float> { }

		public RectTransform handler;
		public RectTransform rectTransform;
		public SliderEvent onValueChanged = new();

		// Min value = 0, max value = 360
		float __value;


		public float value {
			get => __value;
			set {
				SetValueWithoutNotify(value);
				onValueChanged.Invoke(__value);
			}
		}

		public void SetValueWithoutNotify(float value) {
			__value = value;
			handler.localRotation = Quaternion.Euler(0, 0, value);
		}

		public void OnDrag(PointerEventData ev) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, ev.position, ev.pressEventCamera, out Vector2 localPoint);

			float f = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
			value = f < 0 ? f + 360 : f;
		}
	}
}
