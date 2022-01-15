using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using System.Collections.Generic;

namespace RobotoSkunk.PixelMan.LevelEditor {
	public class MainEditor : MonoBehaviour {
		[Header("Components")]
		public Camera cam;
		public MeshRenderer grids;
		public Image virtualCursor;

		[Header("Properties")]
		public Vector2 zoomLimits;
		public float defaultOrtho, navigationSpeed, cursorSpeed;
		[Range(0f, 1f)] public float zoomSpeedGamepad, zoomSpeedKeyboard;

		[Header("Panels")]
		public RectTransform menuLeft;
		public RectTransform menuRight;
		public GameObject leftContainer, rightContainer;
		public float menuLeftSize, menuRightSize;
		public List<Button> leftButtons, rightButtons;


		Material g_Material;
		Vector2 navSpeed, dragOrigin, mousePosition;
		InputType inputType;
		float zoom = 1f, newZoom = 1f, zoomSpeed, mL, nmL, mR, nmR;
		bool leftButtonsEnabled, rightButtonsEnabled, onDragNavigation;

		Vector2 cameraSize {
			get {
				float h = cam.orthographicSize * 2f, w = h * cam.aspect;
				return new(w, h);
			}
		}


		private void Start() {
			g_Material = grids.material;
			Globals.Editor.cursorPos = Globals.screen / 2f;
			// Globals.musicType = MainCore.MusicClips.Type.EDITOR;
		}

		private void Update() {
			Vector2 trnsSpd = navSpeed;

			if (inputType == InputType.Gamepad) {
				Globals.Editor.cursorPos += cursorSpeed * RSTime.delta * navSpeed;
				Globals.Editor.cursorPos = RSMath.Clamp(Globals.Editor.cursorPos, Vector2.zero, Globals.screen);

				if (Globals.Editor.cursorPos.x > 0f && Globals.Editor.cursorPos.x < Screen.width) trnsSpd.x = 0f;
				if (Globals.Editor.cursorPos.y > 0f && Globals.Editor.cursorPos.y < Screen.height) trnsSpd.y = 0f;

				virtualCursor.rectTransform.position = Globals.Editor.cursorPos;
			}

			virtualCursor.enabled = inputType == InputType.Gamepad;

			transform.position += navigationSpeed * zoom * RSTime.delta * (Vector3)trnsSpd;

			#region Zoom
			newZoom += zoomSpeed * RSTime.delta;

			newZoom = Mathf.Clamp(newZoom, zoomLimits.x, zoomLimits.y);
			zoom = Mathf.Lerp(zoom, newZoom, 0.3f * RSTime.delta);

			cam.orthographicSize = zoom * defaultOrtho;
			#endregion

			#region Panels controller
			mL = Mathf.Lerp(mL, nmL, 0.5f * RSTime.delta);
			mR = Mathf.Lerp(mR, nmR, 0.5f * RSTime.delta);

			menuLeft.anchoredPosition = new(-menuLeftSize + mL * menuLeftSize, menuLeft.anchoredPosition.y);
			menuRight.anchoredPosition = new(-menuRightSize + mR * menuRightSize, menuLeft.anchoredPosition.y);

			leftButtonsEnabled = mL > 0.5f;
			rightButtonsEnabled = mR > 0.5f;
			#endregion
		}

		private void LateUpdate() {
			Vector2 gridScale = cameraSize / 10f;

			grids.transform.localScale = new(gridScale.x, 1f, gridScale.y);
			g_Material.SetFloat("_Thickness", zoom * (defaultOrtho * 2f / Screen.height) * 0.1f);
			g_Material.SetColor("_MainColor", Color.white);
			g_Material.SetColor("_SecondaryColor", new Color(1, 1, 1, 0.2f));
		}


		#region Buttons
		public void ButtonResetZoom() => newZoom = 1f;
		public void ButtonChangeZoom(float x) => newZoom += x;
		#endregion

		#region Input System
		public void OnMousePosition(InputAction.CallbackContext context) {
			Vector2 val = context.ReadValue<Vector2>();

			switch (inputType) {
				case InputType.Gamepad: navSpeed = val; break;
				case InputType.KeyboardAndMouse:
					mousePosition = val;

					if (onDragNavigation) transform.position = (Vector3)dragOrigin - (cam.ScreenToWorldPoint(mousePosition) - transform.position) + 10f * Vector3.back;
					break;
			}
		}

		public void OnNavigation(InputAction.CallbackContext context) {
			navSpeed = context.ReadValue<Vector2>();
		}

		public void OnDragNavigation(InputAction.CallbackContext context) {
			onDragNavigation = context.ReadValue<float>() != 0f;

			if (onDragNavigation) dragOrigin = cam.ScreenToWorldPoint(mousePosition);
		}

		public void OnZoom(InputAction.CallbackContext context) {
			float val = -context.ReadValue<Vector2>().y;

			zoomSpeed = val > 0f ? 1f : (val < 0f ? -1f : 0f);

			switch (inputType) {
				case InputType.Gamepad: zoomSpeed *= zoomSpeedGamepad; break;
				case InputType.KeyboardAndMouse: zoomSpeed *= zoomSpeedKeyboard; break;
			}
		}

		public void OnShowLeftPanel(InputAction.CallbackContext context) {
			nmL = context.ReadValue<float>();
		}
		public void OnShowRightPanel(InputAction.CallbackContext context) {
			nmR = context.ReadValue<float>();
		}


		public void OnControlsChanged(PlayerInput input) {
			inputType = input.currentControlScheme switch {
				"Gamepad" => InputType.Gamepad,
				"Keyboard&Mouse" => InputType.KeyboardAndMouse,
				"Touch" => InputType.Touch,
				_ => InputType.KeyboardAndMouse,
			};

			Cursor.visible = inputType == InputType.KeyboardAndMouse;

			if (inputType != InputType.Gamepad) {
				nmL = leftButtonsEnabled ? 1f : 0f;
				nmR = rightButtonsEnabled ? 1f : 0f;
			}

			if (inputType != InputType.KeyboardAndMouse) {
				if (onDragNavigation) onDragNavigation = false;
			}
		}
		#endregion
	}
}
