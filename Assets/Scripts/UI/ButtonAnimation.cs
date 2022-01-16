using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace RobotoSkunk.PixelMan.UI {

	[RequireComponent(typeof(Button))][RequireComponent(typeof(Image))]
	public class ButtonAnimation : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {
		public float speed = 0.5f;
		public ButtonStyle pressed = new() {
			scale = Vector2.one,
			color = Color.white
		}, disabled = new() {
			scale = Vector2.one,
			color = Color.white
		};

		[Header("Requires")]
		public Image target;
		public Button button;

		ButtonStyle defaultStyle, currentStyle;
		Vector2 startPos;
		bool hover;


		[System.Serializable]
		public struct ButtonStyle {
			public Vector2 position, scale;
			public Color color;
		}

		private void Start() {
			startPos = target.rectTransform.anchoredPosition;

			defaultStyle = new() {
				scale = target.rectTransform.localScale,
				color = target.color
			};

			currentStyle = button.interactable ? defaultStyle : disabled;
		}

		private void Update() {
			if (!button.interactable) currentStyle = disabled;
			else {
				switch (Globals.inputType) {
					case InputType.KeyboardAndMouse:
						currentStyle = (Mouse.current.leftButton.isPressed && hover) ? pressed : defaultStyle;
						break;

					case InputType.Gamepad:
						currentStyle = (!Gamepad.current.aButton.isPressed && hover) ? pressed : defaultStyle;
						break;
				}
			}

			target.rectTransform.anchoredPosition = Vector2.Lerp(target.rectTransform.anchoredPosition, startPos + currentStyle.position, speed * RSTime.delta);
			target.rectTransform.localScale = Vector3.Lerp(target.rectTransform.localScale, (Vector3)currentStyle.scale + Vector3.forward, speed * RSTime.delta);
			target.color = Color.Lerp(target.color, currentStyle.color, speed * RSTime.delta);
		}

		public void OnSelect(BaseEventData ev) => hover = true;
		public void OnPointerEnter(PointerEventData ev) => hover = true;
		private void OnMouseDown() => hover = true;

		public void OnDeselect(BaseEventData ev) => hover = false;
		public void OnPointerExit(PointerEventData ev) => hover = false;
		private void OnMouseUpAsButton() => hover = false;
	}
}
