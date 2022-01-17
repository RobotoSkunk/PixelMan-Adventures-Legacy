using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

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
		public ContactFilter2D guiFilter;
		public Sprite[] panelSwitchSprite = new Sprite[2];

		[Header("Panels")]
		public RectTransform[] panels = new RectTransform[2];
		public GameObject[] panelsContainer = new GameObject[2];
		public float[] panelsSize = new float[2];
		public Image[] panelsSwitches;
		public List<Button> leftButtons, rightButtons;


		Material g_Material;
		Vector2 navSpeed, dragOrigin, mousePosition;
		float zoom = 1f, newZoom = 1f, zoomSpeed;
		bool onDragNavigation;
		float[] panelPos = { 0f, 0f }, newPanelPos = { 0f, 0f }, deltaPanelPos = { 0f, 0f };
		bool[] panelEnabled = { false, false };
		List<RaycastResult> guiResults = new();
		InputType inputType = InputType.KeyboardAndMouse;

		Vector2 cameraSize {
			get {
				float h = cam.orthographicSize * 2f, w = h * cam.aspect;
				return new(w, h);
			}
		}


		private void Start() {
			g_Material = grids.material;
			Globals.Editor.cursorPos = Globals.screen / 2f;
			//Globals.musicType = MainCore.MusicClips.Type.EDITOR;
		}

		private void Update() {
			Vector2 trnsSpd = navSpeed;

			#region Panels controller
			panelPos[0] = Mathf.Lerp(panelPos[0], newPanelPos[0] + deltaPanelPos[0], 0.5f * RSTime.delta);
			panelPos[1] = Mathf.Lerp(panelPos[1], newPanelPos[1] + deltaPanelPos[1], 0.5f * RSTime.delta);

			panels[0].anchoredPosition = new(-panelsSize[0] + panelPos[0] * panelsSize[0], panels[0].anchoredPosition.y);
			panels[1].anchoredPosition = new(-panelsSize[1] + panelPos[1] * panelsSize[1], panels[1].anchoredPosition.y);

			panelEnabled[0] = panelPos[0] > 0.5f;
			panelEnabled[1] = panelPos[1] > 0.5f;

			panelsSwitches[0].sprite = panelSwitchSprite[(!panelEnabled[0]).ToInt()];
			panelsSwitches[1].sprite = panelSwitchSprite[(!panelEnabled[1]).ToInt()];
			#endregion

			#region Zoom
			newZoom += zoomSpeed * RSTime.delta;

			newZoom = Mathf.Clamp(newZoom, zoomLimits.x, zoomLimits.y);
			zoom = Mathf.Lerp(zoom, newZoom, 0.3f * RSTime.delta);

			cam.orthographicSize = zoom * defaultOrtho;
			#endregion

			#region Controls processor
			switch (inputType) {
				case InputType.Gamepad:
					Globals.Editor.hoverUI = panelEnabled[0] || panelEnabled[1];
					if (Globals.Editor.hoverUI) navSpeed = Vector2.zero;

					Globals.Editor.cursorPos += cursorSpeed * RSTime.delta * navSpeed;
					Globals.Editor.cursorPos = RSMath.Clamp(Globals.Editor.cursorPos, Vector2.zero, Globals.screen);

					if (Globals.Editor.cursorPos.x > 0f && Globals.Editor.cursorPos.x < Screen.width) trnsSpd.x = 0f;
					if (Globals.Editor.cursorPos.y > 0f && Globals.Editor.cursorPos.y < Screen.height) trnsSpd.y = 0f;

					virtualCursor.rectTransform.position = Globals.Editor.cursorPos;
					virtualCursor.enabled = !Globals.Editor.hoverUI;
					break;
				case InputType.KeyboardAndMouse:
					Globals.Editor.cursorPos = mousePosition;
					virtualCursor.enabled = false;
					break;
			}
			#endregion

			transform.position += navigationSpeed * zoom * RSTime.delta * (Vector3)trnsSpd;
		}

		private void LateUpdate() {
			Vector2 gridScale = cameraSize / 10f;

			grids.transform.localScale = new(gridScale.x, 1f, gridScale.y);
			g_Material.SetFloat("_Thickness", zoom * (defaultOrtho * 2f / Screen.height) * 0.1f);
			g_Material.SetColor("_MainColor", Color.white);
			g_Material.SetColor("_SecondaryColor", new Color(1, 1, 1, 0.2f));
		}

		private void OnGUI() {
			if (inputType != InputType.Gamepad) {
				PointerEventData evData = new(EventSystem.current) {
					position = Globals.Editor.cursorPos
				};
				EventSystem.current.RaycastAll(evData, guiResults);

				Globals.Editor.hoverUI = guiResults.Count > 0;
			}
		}

		#region Buttons
		public void ButtonResetZoom() => newZoom = 1f;
		public void ButtonChangeZoom(float x) => newZoom += x;
		public void SwitchPanel(int index) => newPanelPos[index] = (!panelEnabled[index]).ToInt();
		public void OpenRightPanel() => newPanelPos[1] = 1f;

		public void SwitchPointerEnter(int index) => deltaPanelPos[index] = panelEnabled[index] ? -0.1f : 0.1f;
		public void SwitchPointerExit(int index) { if (inputType != InputType.Gamepad) deltaPanelPos[index] = 0f; }

		public void TestButton(string text) => Debug.Log(text);
		#endregion

		#region Input System
		public void OnMousePosition(InputAction.CallbackContext context) {
			Vector2 val = context.ReadValue<Vector2>();

			switch (inputType) {
				case InputType.Gamepad: if (!Globals.Editor.hoverUI) navSpeed = val; break;
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
			if (!Globals.Editor.hoverUI) {
				onDragNavigation = context.ReadValue<float>() != 0f;

				if (onDragNavigation) dragOrigin = cam.ScreenToWorldPoint(mousePosition);
			}
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
			newPanelPos[0] = context.ReadValue<float>();
		}
		public void OnShowRightPanel(InputAction.CallbackContext context) {
			newPanelPos[1] = context.ReadValue<float>();
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
				newPanelPos[0] = panelEnabled[0].ToInt();
				newPanelPos[1] = panelEnabled[1].ToInt();
			}

			if (inputType != InputType.KeyboardAndMouse) {
				if (onDragNavigation) onDragNavigation = false;
				deltaPanelPos[0] = deltaPanelPos[1] = 0f;
			}
		}
		#endregion
	}
}
