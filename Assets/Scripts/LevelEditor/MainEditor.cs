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

	public struct UndoRedo {
		public Type type;
		public List<GOHandler> objects;

		public enum Type {
			Add,
			Remove,
			Position,
			Properties
		}

		public UndoRedo(Type type, List<InGameObjectBehaviour> objects) {
			this.type = type;
			List<GOHandler> _tmp = new();

			for (int i = 0; i < objects.Count; i++) {
				_tmp.Add(GOHandler.GetGOHandler(objects[i]));
			}

			this.objects = _tmp;
		}

		public struct GOHandler {
			public InGameObjectBehaviour reference;
			public InGameObjectProperties properties;

			public static GOHandler GetGOHandler(InGameObjectBehaviour obj) => new() {
				reference = obj,
				properties = obj.properties
			};
		}
	}

	public class MainEditor : MonoBehaviour {
		[Header("Components")]
		public Camera cam;
		public MeshRenderer grids;
		public InputSystemUIInputModule inputModule;
		public SpriteRenderer objectPreview;

		[Header("UI components")]
		public Image virtualCursor;
		public Image undoImg, redoImg;

		[Header("Properties")]
		public Vector2 zoomLimits;
		public float defaultOrtho, navigationSpeed, cursorSpeed;
		[Range(0f, 1f)] public float zoomSpeedGamepad, zoomSpeedKeyboard;
		public ContactFilter2D contactFilter;
		public Sprite[] panelSwitchSprite = new Sprite[2];
		public Button[] buttons;
		public Image[] gamepadIndications;
		public Toggle[] optionsToggles;

		[Space(25)]
		public PanelStruct[] panels;

		[Header("Game inspector")]
		public ScrollRect inspector;
		public RectTransform[] inspectorSections;


		const float panelLimit = 0.9f;

		readonly List<RaycastResult> guiResults = new();
		readonly List<RaycastHit2D> raycastHits = new();
		readonly List<UndoRedo> undoRedo = new();
		readonly List<InGameObjectBehaviour> objects = new();


		Rect guiRect = new(15, 15, 250, 150);
		InputType inputType = InputType.KeyboardAndMouse;
		Material g_Material;
		Vector2 navSpeed, dragOrigin, mousePosition;
		float zoom = 1f, newZoom = 1f, zoomSpeed;
		bool onDragNavigation, somePanelEnabled;
		uint undoIndex, undoLimit;
		int sid;

		Vector2 cameraSize {
			get {
				float h = cam.orthographicSize * 2f, w = h * cam.aspect;
				return new(w, h);
			}
		}
		int undoDiff {
			get {
				return undoRedo.Count - (int)undoIndex;
			}
		}
		int selectedId {
			get => sid;
			set => sid = Mathf.Clamp(value, 0, Globals.objects.Count - 1);
		}
		InGameObject selectedObject {
			get => Globals.objects[selectedId];
		}


		#region Public methods
		private void Start() {
			g_Material = grids.material;
			Globals.Editor.cursorPos = Globals.screen / 2f;
			// Globals.musicType = MainCore.MusicClips.Type.EDITOR;

			foreach (PanelStruct panel in panels) {
				panel.internals.wasOpen = true;
			}

			undoLimit = Globals.settings.editor.undoLimit;
			undoIndex = 0;
			ChangeInspectorSection(0);
			UpdateButtons();

			Globals.onPause = true;

			bool[] vals = { Globals.Editor.snap };

			for (int i = 0; i < vals.Length; i++) {
				if (i < optionsToggles.Length) optionsToggles[i].SetIsOnWithoutNotify(vals[i]);
			}
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

					if (!Globals.Editor.curInWindow) Globals.Editor.curInWindow = true;
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

					Globals.Editor.curInWindow = Globals.screenRect.Contains(Globals.Editor.cursorPos);
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
			Globals.Editor.virtualCursor = Snapping.Snap(cam.ScreenToWorldPoint(Globals.Editor.cursorPos), (Globals.Editor.snap ? 1f : Constants.pixelToUnit) * Vector2.one);

			objectPreview.transform.position = Globals.Editor.virtualCursor;
			objectPreview.enabled = !Globals.Editor.hoverUI;
			objectPreview.sprite = selectedObject.preview;

			Vector2 gridScale = cameraSize / 10f;

			grids.transform.localScale = new(gridScale.x, 1f, gridScale.y);
			g_Material.SetFloat("_Thickness", zoom * (defaultOrtho * 2f / Screen.height) * 0.1f);
			g_Material.SetColor("_MainColor", Color.white);
			g_Material.SetColor("_SecondaryColor", new Color(1, 1, 1, 0.2f));
		}

		private void FixedUpdate() {
			if (!Globals.Editor.hoverUI && Globals.Editor.curInWindow && !onDragNavigation) {
				int nmb = Physics2D.Raycast(Globals.Editor.virtualCursor, Vector2.zero, contactFilter, raycastHits, 1f);

				if (nmb == 0 && Globals.Editor.onSubmit) {
					InGameObjectBehaviour newObj = Instantiate(Globals.objects[selectedId].gameObject, Globals.Editor.virtualCursor, Quaternion.Euler(0f, 0f, 0f));
					newObj.Prepare4Editor(true);

					UpdateUndoRedo(UndoRedo.Type.Add, new List<InGameObjectBehaviour>() { newObj });
				} else if (nmb > 0 && Globals.Editor.onDelete) {
					List<InGameObjectBehaviour> listBuffer = new();

					foreach (RaycastHit2D ray in raycastHits) {
						GameObject obj = ray.collider.gameObject;

						listBuffer.Add(obj.GetComponent<InGameObjectBehaviour>());
						obj.SetActive(false);
					}

					UpdateUndoRedo(UndoRedo.Type.Remove, listBuffer);
				}
			}
		}

		private void OnGUI() {
			if (inputType != InputType.Gamepad) {
				PointerEventData evData = new(EventSystem.current) {
					position = Globals.Editor.cursorPos
				};
				EventSystem.current.RaycastAll(evData, guiResults);

				Globals.Editor.hoverUI = guiResults.Count > 0;
			}

			GUI.Box(guiRect, "");
			GUI.Label(guiRect, $"Current ID: {sid}");
		}
		#endregion

		#region Private methods
		void PanelsTrigger(float val, int i) {
			if (val > panelLimit && !panels[i].internals.wasEnabled) {
				panels[i].internals.wasEnabled = true;
				panels[i].internals.nextPosition = !panels[i].internals.enabled ? 1f : panelLimit - 0.01f;

			} else if (val <= panelLimit) {
				panels[i].internals.wasEnabled = false;

				if (!panels[i].internals.enabled) panels[i].internals.nextPosition = val;
			}
		}

		void UpdateButtons() {
			undoImg.color = undoRedo.Count == undoIndex ? Color.grey : Color.white;
			redoImg.color = undoIndex == 0 ? Color.grey : Color.white;
		}

		List<InGameObjectBehaviour> GetAllFromHistorial() {
			List<InGameObjectBehaviour> undoOutter = new();
		
			for (int i = 0; i < undoRedo.Count; i++) {
				for (int j = 0; j < undoRedo[i].objects.Count; j++) {
					if (undoRedo[i].objects[j].reference.gameObject.activeSelf) {
						undoOutter.Add(undoRedo[i].objects[j].reference);
					}
				}
			}
		
			return undoOutter;
		}

		void UpdateUndoRedo(UndoRedo.Type type, List<InGameObjectBehaviour> objects) {
			UndoRedo undoBuffer = new(type, objects);

			if (undoIndex > 0) {
				List<UndoRedo> rangeBuffer = undoRedo.GetRange(undoDiff, (int)undoIndex);
				
				for (int i = 0; i < rangeBuffer.Count; i++) {
				
					if (rangeBuffer[i].type == UndoRedo.Type.Add) {
						for (int j = 0; j < rangeBuffer[i].objects.Count; j++) {
							Destroy(rangeBuffer[i].objects[j].reference.gameObject);
						}
					}

				}

				undoRedo.RemoveRange(undoDiff, (int)undoIndex);

				undoIndex = 0;
			} else if (undoRedo.Count == undoLimit) undoRedo.RemoveAt(0);

			undoRedo.Add(undoBuffer);
			UpdateButtons();
		}
		#endregion

		#region Buttons
		public void ButtonResetZoom() => newZoom = 1f;
		public void ButtonChangeZoom(float x) => newZoom += x;
		public void SwitchPanel(int i) { panels[i].internals.nextPosition = (!panels[i].internals.enabled).ToInt(); panels[i].internals.deltaPosition = 0f; }
		public void OpenPanel(int i) => panels[i].internals.nextPosition = 1f;
		public void SetObjectId(int i) => selectedId = i;

		public void SwitchPointerEnter(int i) { if (inputType != InputType.Gamepad) panels[i].internals.deltaPosition = panels[i].internals.enabled ? -0.05f : 0.05f; }
		public void SwitchPointerExit(int i) => panels[i].internals.deltaPosition = 0f;

		public void ChangeInspectorSection(int i) {
			for (int a = 0; a < inspectorSections.Length; a++) {
				inspectorSections[a].gameObject.SetActive(a == i);

				if (a == i) inspector.content = inspectorSections[a];
			}
		}


		public void Undo() {
			if (undoRedo.Count == undoIndex) return;
			UndoRedo undoBuffer = undoRedo[undoDiff - 1];

			foreach (UndoRedo.GOHandler obj in undoBuffer.objects) {
				switch (undoBuffer.type) {
					case UndoRedo.Type.Add: obj.reference.gameObject.SetActive(false); break;
					case UndoRedo.Type.Remove: obj.reference.gameObject.SetActive(true); break;
				}
			}

			undoIndex++;
			UpdateButtons();
		}
		public void Redo() {
			if (undoIndex == 0) return;
			UndoRedo undoBuffer = undoRedo[undoDiff];

			foreach (UndoRedo.GOHandler obj in undoBuffer.objects) {
				switch (undoBuffer.type) {
					case UndoRedo.Type.Add: obj.reference.gameObject.SetActive(true); break;
					case UndoRedo.Type.Remove: obj.reference.gameObject.SetActive(false); break;
				}
			}

			undoIndex--;
			UpdateButtons();
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
			if (onDragNavigation || (!Globals.Editor.hoverUI && !onDragNavigation)) {
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

		public void SubmitEditor(InputAction.CallbackContext context) => Globals.Editor.onSubmit = context.ReadValue<float>() > 0.5f;
		public void DeleteEditor(InputAction.CallbackContext context) => Globals.Editor.onDelete = context.ReadValue<float>() > 0.5f;

		public void UndoAction(InputAction.CallbackContext context) {
			if (context.action.phase != InputActionPhase.Started) return;

			Undo();
		}
		public void RedoAction(InputAction.CallbackContext context) {
			if (context.action.phase != InputActionPhase.Started) return;

			Redo();
		}
		public void SetSnap(bool value) => Globals.Editor.snap = value;



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
