using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace RobotoSkunk.PixelMan.UI {

	[AddComponentMenu("UI/RobotoSkunk - Button Animation")]
	[RequireComponent(typeof(Selectable))][RequireComponent(typeof(Image))]
	public class ButtonAnimation : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
		public float speed = 0.5f;
		public bool usePosition = true, useScale = true, useColor = true;
		public ButtonStyle pressed = new() { scale = 1.2f * Vector2.one, color = Color.white },
			disabled = new() { scale = Vector2.one, color = Color.white };

		[Header("Requires")]
		[SerializeField] Image target;
		[SerializeField] Selectable button;

		ButtonStyle defaultStyle, currentStyle;
		Vector2 startPos;
		bool isSelected, isPressed, pointerDown;


		[System.Serializable]
		public struct ButtonStyle {
			public Vector2 position, scale;
			public Color color;
		}

		private void Awake() {
			if (!target) target = GetComponent<Image>();
			if (!button) button = GetComponent<Button>();
		}

		void Start() {
			startPos = target.rectTransform.anchoredPosition;

			defaultStyle = new() {
				scale = target.rectTransform.localScale,
				color = target.color
			};

			currentStyle = button.interactable ? defaultStyle : disabled;
		}

		private void Update() {
			bool onSubmit = (Gamepad.current?.aButton.isPressed ?? false) || (Keyboard.current?.enterKey.isPressed ?? false);

			if (!button.interactable) currentStyle = disabled;
			else if (isPressed && !onSubmit) currentStyle = pressed;
			else currentStyle = defaultStyle;

			if (usePosition) target.rectTransform.anchoredPosition = Vector2.Lerp(target.rectTransform.anchoredPosition, startPos + currentStyle.position, speed * RSTime.delta);
			if (useScale) target.rectTransform.localScale = Vector3.Lerp(target.rectTransform.localScale, (Vector3)currentStyle.scale + Vector3.forward, speed * RSTime.delta);
			if (useColor) target.color = Color.Lerp(target.color, currentStyle.color, speed * RSTime.delta);
		}

		public void OnSelect(BaseEventData ev) {
			isSelected = isPressed = true;
			Globals.buttonSelected = button.GetInstanceID();
		}
		public void OnPointerEnter(PointerEventData ev) => isPressed = isSelected && pointerDown;
		public void OnPointerDown(PointerEventData ev) => pointerDown = isPressed = true;

		public void OnDeselect(BaseEventData ev) => isSelected = isPressed = false;
		public void OnPointerExit(PointerEventData ev) => isPressed = false;
		public void OnPointerUp(PointerEventData ev) => pointerDown = isPressed = false;
	}
}
