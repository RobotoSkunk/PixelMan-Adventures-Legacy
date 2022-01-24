using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

using System.Collections.Generic;

namespace RobotoSkunk.PixelMan.LevelEditor {
	[System.Serializable]
	public class PanelStruct {
		public RectTransform rectTransforms;
		public Image switchImage;
		public Selectable defaultSelected, outSelected;
		public List<Selectable> content;
		public GameObject container;
		public float size;
		public Internal internals;

		[System.Serializable]
		public struct Internal {
			public float position, nextPosition, deltaPosition;
			public bool enabled, wasEnabled, wasOpen;
		}
	}

	public class MainEditor : MonoBehaviour {
		[Header("Components")]
		public Camera cam;
		public MeshRenderer grids;
		public Image virtualCursor;
		public InputSystemUIInputModule inputModule;
		public SpriteRenderer objectPreview;

		[Header("Properties")]
		public Vector2 zoomLimits;
		public float defaultOrtho, navigationSpeed, cursorSpeed;
		[Range(0f, 1f)] public float zoomSpeedGamepad, zoomSpeedKeyboard;
		public ContactFilter2D guiFilter;
		public Sprite[] panelSwitchSprite = new Sprite[2];
		public Button[] buttons;
		public Image[] gamepadIndications;

		[Space(25)]
		public PanelStruct[] panels;

		[Header("Game inspector")]
		public ScrollRect inspector;
		public RectTransform[] inspectorSections;


		const float panelLimit = 0.9f;

		readonly List<RaycastResult> guiResults = new();


		Material g_Material;
		Vector2 navSpeed, dragOrigin, mousePosition;
		float zoom = 1f, newZoom = 1f, zoomSpeed;
		bool onDragNavigation, somePanelEnabled;
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

			foreach (PanelStruct panel in panels) {
				panel.internals.wasOpen = true;
			}

			ChangeInspectorSection(0);
		}

		private void Update() {
			Vector2 trnsSpd = navSpeed;

			#region Panels controller
			for (int i = 0; i < panels.Length; i++) {
				panels[i].internals.position = Mathf.Lerp(panels[i].internals.position, panels[i].internals.nextPosition + panels[i].internals.deltaPosition, 0.5f * RSTime.delta);

				panels[i].rectTransforms.anchoredPosition = new(-panels[i].size + panels[i].internals.position * panels[i].size, panels[i].rectTransforms.anchoredPosition.y);

				panels[i].internals.enabled = panels[i].internals.position > panelLimit;

				panels[i].switchImage.sprite = panelSwitchSprite[(!panels[i].internals.enabled).ToInt()];
			}
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
					Globals.Editor.hoverUI = panels[0].internals.enabled || panels[1].internals.enabled;
					if (Globals.Editor.hoverUI) navSpeed = Vector2.zero;

					Globals.Editor.cursorPos += cursorSpeed * RSTime.delta * navSpeed;
					Globals.Editor.cursorPos = RSMath.Clamp(Globals.Editor.cursorPos, Vector2.zero, Globals.screen);

					if (Globals.Editor.cursorPos.x > 0f && Globals.Editor.cursorPos.x < Screen.width) trnsSpd.x = 0f;
					if (Globals.Editor.cursorPos.y > 0f && Globals.Editor.cursorPos.y < Screen.height) trnsSpd.y = 0f;

					virtualCursor.rectTransform.position = Globals.Editor.cursorPos;
					virtualCursor.enabled = !Globals.Editor.hoverUI;

					#region Panels buttons
					if (buttons[0].navigation.mode != Navigation.Mode.Explicit)
						buttons.SetNavigation(Navigation.Mode.Explicit);

					if (buttons[0].interactable != Globals.Editor.hoverUI)
						buttons.SetInteractable(Globals.Editor.hoverUI);

					foreach (PanelStruct panel in panels) {
						if (!somePanelEnabled && panel.internals.enabled) {
							somePanelEnabled = true;

							panel.defaultSelected.Select();
						}
						
						if (Globals.Editor.hoverUI && panel.internals.wasOpen != panel.internals.enabled && !panel.internals.enabled) {
							for (int i = 0; i < panel.content.Count; i++) {
								if (panel.content[i].GetInstanceID() == Globals.buttonSelected) {
									panel.outSelected.Select();
									break;
								}
							}
						}

						if (panel.content[0].navigation.mode != Navigation.Mode.Explicit)
							panel.content.SetNavigation(Navigation.Mode.Explicit);
					}

					if (somePanelEnabled && !Globals.Editor.hoverUI) somePanelEnabled = false;
					#endregion
					break;
				case InputType.KeyboardAndMouse:
					Globals.Editor.cursorPos = mousePosition;
					virtualCursor.enabled = false;

					if (!buttons[0].interactable)
						buttons.SetInteractable(true);

					if (buttons[0].navigation.mode != Navigation.Mode.None)
						buttons.SetNavigation(Navigation.Mode.None);

					foreach (PanelStruct panel in panels) {
						if (panel.content[0].navigation.mode != Navigation.Mode.None)
							panel.content.SetNavigation(Navigation.Mode.None);
					}
					break;
			}

			foreach (PanelStruct panel in panels) {
				if (panel.internals.wasOpen != panel.internals.enabled) {
					panel.internals.wasOpen = panel.internals.enabled;

					panel.content.SetInteractable(panel.internals.enabled);
				}
			}
			#endregion

			transform.position += navigationSpeed * zoom * RSTime.delta * (Vector3)trnsSpd;
		}

		private void LateUpdate() {
			objectPreview.transform.position = Snapping.Snap(cam.ScreenToWorldPoint(Globals.Editor.cursorPos), (Globals.Editor.snap ? 1f : Constants.pixelToUnit) * Vector2.one);
			objectPreview.enabled = !Globals.Editor.hoverUI;

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

		void PanelsTrigger(float val, int i) {
			if (val > panelLimit && !panels[i].internals.wasEnabled) {
				panels[i].internals.wasEnabled = true;
				panels[i].internals.nextPosition = !panels[i].internals.enabled ? 1f : panelLimit - 0.01f;

			} else if (val <= panelLimit) {
				panels[i].internals.wasEnabled = false;

				if (!panels[i].internals.enabled) panels[i].internals.nextPosition = val;
			}
		}

		#region Buttons
		public void ButtonResetZoom() => newZoom = 1f;
		public void ButtonChangeZoom(float x) => newZoom += x;
		public void SwitchPanel(int i) { panels[i].internals.nextPosition = (!panels[i].internals.enabled).ToInt(); panels[i].internals.deltaPosition = 0f; }
		public void OpenPanel(int i) => panels[i].internals.nextPosition = 1f;

		public void SwitchPointerEnter(int i) { if (inputType != InputType.Gamepad) panels[i].internals.deltaPosition = panels[i].internals.enabled ? -0.05f : 0.05f; }
		public void SwitchPointerExit(int i) => panels[i].internals.deltaPosition = 0f;

		public void ChangeInspectorSection(int i) {
			for (int a = 0; a < inspectorSections.Length; a++) {
				inspectorSections[a].gameObject.SetActive(a == i);

				if (a == i) inspector.content = inspectorSections[a];
			}
		}

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

		public void OnNavigation(InputAction.CallbackContext context) => navSpeed = context.ReadValue<Vector2>();

		public void OnDragNavigation(InputAction.CallbackContext context) {
			if (!Globals.Editor.hoverUI) {
				onDragNavigation = context.ReadValue<float>() != 0f;

				if (onDragNavigation) dragOrigin = cam.ScreenToWorldPoint(mousePosition);
			}
		}

		public void OnZoom(InputAction.CallbackContext context) {
			if (Globals.Editor.hoverUI) return;

			float val = -context.ReadValue<Vector2>().y;

			zoomSpeed = val > 0f ? 1f : (val < 0f ? -1f : 0f);

			switch (inputType) {
				case InputType.Gamepad: zoomSpeed *= zoomSpeedGamepad; break;
				case InputType.KeyboardAndMouse: zoomSpeed *= zoomSpeedKeyboard; break;
			}
		}

		public void OnShowLeftPanel(InputAction.CallbackContext context) => PanelsTrigger(context.ReadValue<float>(), 0);
		public void OnShowRightPanel(InputAction.CallbackContext context) => PanelsTrigger(context.ReadValue<float>(), 1);


		public void OnControlsChanged(PlayerInput input) {
			inputType = input.currentControlScheme switch {
				"Gamepad" => InputType.Gamepad,
				"Keyboard&Mouse" => InputType.KeyboardAndMouse,
				"Touch" => InputType.Touch,
				_ => InputType.KeyboardAndMouse,
			};


			Cursor.visible = inputType == InputType.KeyboardAndMouse;

			if (inputType != InputType.Gamepad) {
				panels[0].internals.nextPosition = panels[0].internals.enabled.ToInt();
				panels[1].internals.nextPosition = panels[1].internals.enabled.ToInt();
			}

			if (inputType != InputType.KeyboardAndMouse) {
				if (onDragNavigation) onDragNavigation = false;
				panels[0].internals.deltaPosition = panels[1].internals.deltaPosition = 0f;
			}

			if (gamepadIndications[0].enabled != (inputType == InputType.Gamepad)) {
				foreach (Image image in gamepadIndications) {
					image.enabled = inputType == InputType.Gamepad;
				}
			}
		}
		#endregion
	}
}
