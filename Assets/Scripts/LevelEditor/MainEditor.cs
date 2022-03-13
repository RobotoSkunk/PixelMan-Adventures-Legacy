using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

using System.Collections.Generic;
using System.Linq;

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

		public struct Internal {
			public float position, nextPosition, deltaPosition;
			public bool enabled, wasEnabled, wasOpen;
		}
	}

	[System.Serializable]
	public enum ResizePoints { TOP, BOTTOM, LEFT, RIGHT, TOP_LEFT, TOP_RIGHT, BOTTOM_LEFT, BOTTOM_RIGHT }

	public struct UndoRedo {
		public Type type;
		public List<GOHandler> objCache;

		public enum Type {
			Add,
			Remove,
			Position,
			Properties
		}

		public UndoRedo(Type type, List<InGameObjectBehaviour> objCache) {
			this.type = type;
			List<GOHandler> _tmp = new();

			for (int i = 0; i < objCache.Count; i++) {
				_tmp.Add(GOHandler.GetGOHandler(objCache[i]));
			}

			this.objCache = _tmp;
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
		public SpriteRenderer objectPreview, selectionArea;
		public Canvas dragArea;
		public RectTransform dragAreaRect;

		[Header("UI components")]
		public Image virtualCursorImg;
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

		[Header("GameObject containers")]
		public CompositeCollider2D[] composites;
		public Transform contGeneric, contBlockUntagged, contBlockIce;

		[Header("Game inspector")]
		public ScrollRect inspector;
		public RectTransform[] inspectorSections;


		const float panelLimit = 0.9f;

		#region Cache and buffers
		readonly List<RaycastResult> guiResults = new();
		readonly List<RaycastHit2D> raycastHits = new(), msHits = new();
		readonly List<UndoRedo> undoRedo = new();
		readonly List<InGameObjectBehaviour> objCache = new(), selected = new(), multiSelected = new();

		InGameObjectBehaviour[] lastMS = new InGameObjectBehaviour[0],
								msBuffer = new InGameObjectBehaviour[0],
								lastSelBuffer = new InGameObjectBehaviour[0],
								selectedBuffer = new InGameObjectBehaviour[0];

		InGameObjectBehaviour draggedObject;
		#endregion

		#region Temporal garbage
		Rect guiRect = new(15, 15, 250, 150), msDrag, lastResArea, newResArea;
		Material g_Material;
		Vector2 navSpeed, dragNavOrigin, cursorToWorld;
		float newZoom = 1f, zoomSpeed, msAlpha;
		bool somePanelEnabled;
		uint undoIndex, undoLimit;
		int sid;
		#endregion

		#region Common variables
		InputType inputType = InputType.KeyboardAndMouse;
		float zoom = 1f;
		Vector2 cursorPos, virtualCursor, mousePosition, dragOrigin;
		Bounds selectionBounds;

		bool onSubmit, onDelete;
		bool onDragNavigation, onMultiselection, onDrag, hoverAnObject, onResize;
		bool hoverUI, curInWindow, allowScroll;

		AnalysisData analysis = new();
		#endregion

		Vector2 cameraSize {
			get {
				float h = cam.orthographicSize * 2f, w = h * cam.aspect;
				return new(w, h);
			}
		}
		int selectedId {
			get => sid;
			set => sid = Mathf.Clamp(value, 0, Globals.objects.Count - 1);
		}
		int undoDiff {
			get => undoRedo.Count - (int)undoIndex;
		}
		InGameObject selectedObject {
			get => Globals.objects[selectedId];
		}
		bool userReady {
			get => !hoverUI && curInWindow;
		}
		bool userIsDragging {
			get => onResize || onDrag || onMultiselection || onDragNavigation;
		}


		#region Unity methods
		private void Start() {
			g_Material = grids.material;
			cursorPos = Globals.screen / 2f;
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

			GameObject[] objs = GameObject.FindGameObjectsWithTag("EditorObject");

			for (int i = 0; i < objs.Length; i++)
				objCache.Add(objs[i].GetComponent<InGameObjectBehaviour>());

			CollidersSetAutomatic(false);
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
					hoverUI = panels[0].internals.enabled || panels[1].internals.enabled;
					if (hoverUI) navSpeed = Vector2.zero;

					cursorPos += cursorSpeed * RSTime.delta * navSpeed;
					cursorPos = RSMath.Clamp(cursorPos, Vector2.zero, Globals.screen);

					if (cursorPos.x > 0f && cursorPos.x < Screen.width) trnsSpd.x = 0f;
					if (cursorPos.y > 0f && cursorPos.y < Screen.height) trnsSpd.y = 0f;

					virtualCursorImg.rectTransform.position = cursorPos;
					virtualCursorImg.enabled = !hoverUI;

					#region Panels buttons
					if (buttons[0].navigation.mode != Navigation.Mode.Automatic)
						buttons.SetNavigation(Navigation.Mode.Automatic);

					if (buttons[0].interactable != hoverUI)
						buttons.SetInteractable(hoverUI);

					foreach (PanelStruct panel in panels) {
						if (!somePanelEnabled && panel.internals.enabled) {
							somePanelEnabled = true;

							panel.defaultSelected.Select();
						}
						
						if (hoverUI && panel.internals.wasOpen != panel.internals.enabled && !panel.internals.enabled) {
							for (int i = 0; i < panel.content.Count; i++) {
								if (panel.content[i].GetInstanceID() == Globals.buttonSelected) {
									panel.outSelected.Select();
									break;
								}
							}
						}

						if (panel.content[0].navigation.mode != Navigation.Mode.Automatic)
							panel.content.SetNavigation(Navigation.Mode.Automatic);
					}

					if (somePanelEnabled && !hoverUI) somePanelEnabled = false;
					#endregion

					if (!curInWindow) curInWindow = true;
					break;
				case InputType.KeyboardAndMouse:
					cursorPos = mousePosition;
					virtualCursorImg.enabled = false;

					if (!buttons[0].interactable)
						buttons.SetInteractable(true);

					if (buttons[0].navigation.mode != Navigation.Mode.None)
						buttons.SetNavigation(Navigation.Mode.None);

					foreach (PanelStruct panel in panels) {
						if (panel.content[0].navigation.mode != Navigation.Mode.None)
							panel.content.SetNavigation(Navigation.Mode.None);
					}

					curInWindow = Globals.screenRect.Contains(cursorPos);
					break;
			}


			foreach (PanelStruct panel in panels) {
				if (panel.internals.wasOpen != panel.internals.enabled) {
					panel.internals.wasOpen = panel.internals.enabled;

					panel.content.SetInteractable(panel.internals.enabled);
				}
			}
			#endregion

			if (!onMultiselection && multiSelected.Count > 0) {
				for (int i = 0; i < multiSelected.Count; i++) {
					if (!selected.Contains(multiSelected[i]))
						selected.Add(multiSelected[i]);
				}

				multiSelected.Clear();
			}

			transform.position += navigationSpeed * zoom * RSTime.delta * (Vector3)trnsSpd;
			transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
		}

		private void LateUpdate() {
			cursorToWorld = cam.ScreenToWorldPoint(cursorPos);
			virtualCursor = Snapping.Snap(cursorToWorld, (Globals.Editor.snap ? 1f : Constants.pixelToUnit) * Vector2.one);

			#region Preview and selection area
			msAlpha = Mathf.Lerp(msAlpha, onMultiselection.ToInt(), 0.15f * RSTime.delta);
			if (onMultiselection) msDrag.size = cursorToWorld - msDrag.position;

			objectPreview.transform.position = virtualCursor;
			objectPreview.enabled = userReady && !userIsDragging;
			objectPreview.sprite = selectedObject.preview;

			selectionArea.transform.position = msDrag.position;
			selectionArea.transform.localScale = (Vector3)(zoom * Vector2.one) + Vector3.forward;
			selectionArea.size = msDrag.size / zoom;
			selectionArea.color = new Color(0.23f, 0.35f, 1f, msAlpha);
			selectionArea.enabled = msAlpha > 0.05f;
			#endregion

			#region Grids
			Vector2 gridScale = cameraSize / 10f;

			grids.transform.localScale = new(gridScale.x, 1f, gridScale.y);
			g_Material.SetFloat("_Thickness", zoom * (defaultOrtho * 2f / Screen.height) * 0.1f);
			g_Material.SetColor("_MainColor", Color.white);
			g_Material.SetColor("_SecondaryColor", new Color(1, 1, 1, 0.2f));
			#endregion

			#region Selection
			// Color
			for (int i = 0; i < selected.Count; i++)
				if (selected[i])
					selected[i].color = Constants.Colors.green;

			if (lastSelBuffer.Length != selected.Count) {
				selectedBuffer = lastSelBuffer.Except(selected).ToArray();

				for (int i = 0; i < selectedBuffer.Length; i++)
					if (selectedBuffer[i])
						selectedBuffer[i].color = Color.white;
				

				lastSelBuffer = selected.ToArray();

				selectionBounds = GetSelectionBounds();
			}

			// Multiselection buffer
			if (!onMultiselection) {
				if (multiSelected.Count > 0) multiSelected.Clear();
				if (lastMS.Length > 0) lastMS = new InGameObjectBehaviour[0];
			}

			// Dragging all
			if (onDrag) {
				Vector2 newPos = Snapping.Snap(cursorToWorld + draggedObject.dragOrigin, (Globals.Editor.snap ? 1f : Constants.pixelToUnit) * Vector2.one);
				draggedObject.transform.position = newPos;

				selectionBounds = GetSelectionBounds();

				for (int i = 0; i < selected.Count; i++)
					if (selected[i] != draggedObject)
						selected[i].SetPosition(newPos - selected[i].dist2Dragged);
			}

			// Interpreting selection area
			if ((Vector2)selectionBounds.size == Vector2.zero)
				dragArea.enabled = false;
			else {
				dragArea.enabled = true;
				dragAreaRect.transform.position = selectionBounds.center;
				dragAreaRect.sizeDelta = selectionBounds.size + new Vector3(1f, 1f, 0f);
			}
			#endregion
		}

		private void FixedUpdate() {
			if (onMultiselection) {
				int nmb = Physics2D.BoxCast(msDrag.center, RSMath.Abs(msDrag.size), 0f, Vector2.zero, contactFilter, msHits);

				multiSelected.Clear();

				for (int i = 0; i < nmb; i++) {
					InGameObjectBehaviour obj = msHits[i].collider.GetComponent<InGameObjectBehaviour>();

					if (!selected.Contains(obj)) {
						obj.color = Constants.Colors.green;

						multiSelected.Add(obj);
					}
				}

				msBuffer = lastMS.Except(multiSelected).ToArray();

				for (int i = 0; i < msBuffer.Length; i++)
					msBuffer[i].color = Color.white;

				lastMS = multiSelected.ToArray();


			} else if (userReady && !userIsDragging) {
				int nmb = Physics2D.Raycast(virtualCursor, Vector2.zero, contactFilter, raycastHits, 1f);


				switch (nmb) {
					case > 0:
						InGameObjectBehaviour pointedObject = raycastHits[0].collider.GetComponent<InGameObjectBehaviour>();

						if (hoverAnObject && selected.Contains(pointedObject) && onSubmit && dragOrigin != cursorToWorld) {
							onDrag = true;
							hoverAnObject = false;
							draggedObject = pointedObject;


							draggedObject.dragOrigin = (Vector2)draggedObject.transform.position - dragOrigin;

							for (int i = 0; i < selected.Count; i++)
								selected[i].dist2Dragged = draggedObject.transform.position - selected[i].transform.position;

						} else if (onDelete) {
							List<InGameObjectBehaviour> listBuffer = new();

							foreach (RaycastHit2D ray in raycastHits) {
								GameObject obj = ray.collider.gameObject;
								InGameObjectBehaviour scr = obj.GetComponent<InGameObjectBehaviour>();

								listBuffer.Add(scr);
								obj.SetActive(false);
							}

							UpdateUndoRedo(UndoRedo.Type.Remove, listBuffer);
						}
						break;
					case 0:
						if (onSubmit) {
							if (selectedId == Constants.InternalIDs.player && analysis.playersNumber >= 2) return;

							InGameObjectBehaviour newObj = CreateObject(selectedId, virtualCursor, Vector2.one, 0f);

							UpdateUndoRedo(UndoRedo.Type.Add, new List<InGameObjectBehaviour>() { newObj });
						}
						break;
				}
			}
		}

		private void OnGUI() {
			if (inputType != InputType.Gamepad) {
				PointerEventData evData = new(EventSystem.current) {
					position = cursorPos
				};
				EventSystem.current.RaycastAll(evData, guiResults);

				hoverUI = guiResults.Count > 0;

				allowScroll = !hoverUI;

				if (!allowScroll) {
					for (int i = 0; i < guiResults.Count; i++) {
						if (guiResults[i].gameObject.CompareTag("EnableScroll")) {
							allowScroll = true;
							break;
						}
					}
				}
			}

			// GUI.Box(guiRect, "");
			// GUI.Label(guiRect, $"Instances number: {analysis.objectsNumber}\nCurrent ID: {sid}");
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

			analysis = AnalizeObjects();
			selectionBounds = GetSelectionBounds();
		}

		void UpdateUndoRedo(UndoRedo.Type type, List<InGameObjectBehaviour> objs) {
			UndoRedo undoBuffer = new(type, objs);

			if (undoIndex > 0) {
				List<UndoRedo> rangeBuffer = undoRedo.GetRange(undoDiff, (int)undoIndex);
				
				for (int i = 0; i < rangeBuffer.Count; i++) {
				
					if (rangeBuffer[i].type == UndoRedo.Type.Add) {
						for (int j = 0; j < rangeBuffer[i].objCache.Count; j++) {
							InGameObjectBehaviour rff = rangeBuffer[i].objCache[j].reference;

							selected.Remove(rff);
							objCache.Remove(rff);

							Destroy(rff.gameObject);
						}
					}

				}

				undoRedo.RemoveRange(undoDiff, (int)undoIndex);

				undoIndex = 0;
			} else if (undoRedo.Count == undoLimit) undoRedo.RemoveAt(0);

			undoRedo.Add(undoBuffer);
			UpdateButtons();
		}

		AnalysisData AnalizeObjects() {
			AnalysisData analysis = new();

			for (int i = 0; i < objCache.Count; i++) {
				if (!objCache[i]) continue;
				if (!objCache[i].gameObject.activeInHierarchy) continue;

				analysis.objectsNumber++;
				if (objCache[i].properties.id == Constants.InternalIDs.player) analysis.playersNumber++;
			}

			return analysis;
		}

		Bounds GetSelectionBounds() {
			if (selected.Count > 0) {
				int i = 0;
				Bounds bounds = new();

				// First bound
				for ( ; i < selected.Count; i++) {
					if (!selected[i]) continue;
					if (!selected[i].gameObject.activeInHierarchy) continue;

					bounds = selected[i].bounds;
					break;
				}

				// The rest of the bounds
				for ( ; i < selected.Count; i++) {
					if (!selected[i]) continue;
					if (!selected[i].gameObject.activeInHierarchy) continue;

					bounds.Encapsulate(selected[i].bounds);
				}

				return bounds;
			}

			return new Bounds();
		}

		InGameObjectBehaviour CreateObject(int id, Vector2 pos, Vector2 sca, float rot) {
			InGameObjectBehaviour newObj = Globals.objects[id].Instantiate(virtualCursor, sca, 0f, false);

			Transform destCont = contGeneric;

			if (Globals.objects[id].category == InGameObject.Category.BLOCKS) {
				destCont = newObj.GetDefaultTag() switch {
					"IceBlock" => contBlockIce,
					_ => contBlockUntagged
				};
			}

			newObj.transform.SetParent(destCont);
			newObj.transform.localScale = (Vector3)sca + Vector3.forward;

			newObj.SetInternalId((uint)id);
			newObj.Prepare4Editor(true);


			objCache.Add(newObj);

			return newObj;
		}

		void CollidersSetAutomatic(bool enabled) {
			for (int i = 0; i < composites.Length; i++)
				composites[i].generationType = enabled ? CompositeCollider2D.GenerationType.Synchronous : CompositeCollider2D.GenerationType.Manual;
		}
		void UpdateColliders() {
			for (int i = 0; i < composites.Length; i++)
				composites[i].GenerateGeometry();
		}
		#endregion

		#region Public methods
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

			foreach (UndoRedo.GOHandler obj in undoBuffer.objCache) {
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

			foreach (UndoRedo.GOHandler obj in undoBuffer.objCache) {
				switch (undoBuffer.type) {
					case UndoRedo.Type.Add: obj.reference.gameObject.SetActive(true); break;
					case UndoRedo.Type.Remove: obj.reference.gameObject.SetActive(false); break;
				}
			}

			undoIndex--;
			UpdateButtons();
		}

		public void DeselectAll() {
			if (selected.Count == 0) return;

			selected.Clear();
		}
		public void DeleteSelected() {
			if (selected.Count == 0) return;

			List<InGameObjectBehaviour> listBuffer = new();

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) continue;
				if (!selected[i].gameObject.activeInHierarchy) continue;

				listBuffer.Add(selected[i]);
				selected[i].gameObject.SetActive(false);
			}

			selected.Clear();

			analysis = AnalizeObjects();
			UpdateUndoRedo(UndoRedo.Type.Remove, listBuffer);
		}

		public void StartResizing(ResizeDragArea handler) {
			onResize = true;

			lastResArea = newResArea = new Rect(selectionBounds.min, selectionBounds.size);

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) continue;

				selected[i].resSca = selected[i].transform.localScale;
				selected[i].resPos = newResArea.center - (Vector2)selected[i].transform.position;
			}
		}
		public void UpdateResizing(ResizeDragArea handler) {
			Vector4 a = newResArea.MinMaxToVec4();

			Vector4 newData = handler.resizePoint switch {
				ResizePoints.TOP          => new(a.x, a.y, a.z, virtualCursor.y - 0.5f),
				ResizePoints.BOTTOM       => new(a.x, virtualCursor.y + 0.5f, a.z, a.w),
				ResizePoints.LEFT         => new(virtualCursor.x + 0.5f, a.y, a.z, a.w),
				ResizePoints.RIGHT        => new(a.x, a.y, virtualCursor.x - 0.5f, a.w),

				ResizePoints.TOP_LEFT     => new(virtualCursor.x + 0.5f, a.y, a.z, virtualCursor.y - 0.5f),
				ResizePoints.TOP_RIGHT    => new(a.x, a.y, virtualCursor.x - 0.5f, virtualCursor.y - 0.5f),
				ResizePoints.BOTTOM_LEFT  => new(virtualCursor.x + 0.5f, virtualCursor.y + 0.5f, a.z, a.w),
				ResizePoints.BOTTOM_RIGHT => new(a.x, virtualCursor.y + 0.5f, virtualCursor.x - 0.5f, a.w),

				_ => new()
			};

			newResArea = Rect.MinMaxRect(newData.x, newData.y, newData.z, newData.w);

			Vector2 fixedScale = newResArea.size / lastResArea.size;

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) continue;

				selected[i].SetScale(RSMath.Abs(selected[i].resSca * fixedScale));
				selected[i].SetPosition(newResArea.center - selected[i].resPos * fixedScale);
			}

			selectionBounds = GetSelectionBounds();
		}
		public void EndResizing() {
			onResize = false;
			lastResArea = newResArea;
			UpdateColliders();
		}

		public void TestButton(string text) => Debug.Log(text);
		#endregion

		#region Input System
		public void OnMousePosition(InputAction.CallbackContext context) {
			Vector2 val = context.ReadValue<Vector2>();

			switch (inputType) {
				case InputType.Gamepad: if (!hoverUI) navSpeed = val; break;
				case InputType.KeyboardAndMouse:
					mousePosition = val;

					if (onDragNavigation) transform.position = (Vector3)dragNavOrigin - ((Vector3)cursorToWorld - transform.position) + 10f * Vector3.back;
					break;
			}
		}

		public void OnNavigation(InputAction.CallbackContext context) => navSpeed = context.ReadValue<Vector2>();

		public void OnDragNavigation(InputAction.CallbackContext context) {
			if (onDragNavigation || (!hoverUI && !onDragNavigation)) {
				onDragNavigation = context.ReadValue<float>() != 0f;

				if (onDragNavigation) dragNavOrigin = cursorToWorld;
			}
		}

		public void OnZoom(InputAction.CallbackContext context) {
			if (!allowScroll) return;

			float val = -context.ReadValue<Vector2>().y;

			zoomSpeed = val > 0f ? 1f : (val < 0f ? -1f : 0f);

			switch (inputType) {
				case InputType.Gamepad: zoomSpeed *= zoomSpeedGamepad; break;
				case InputType.KeyboardAndMouse: zoomSpeed *= zoomSpeedKeyboard; break;
			}
		}

		public void OnShowLeftPanel(InputAction.CallbackContext context) => PanelsTrigger(context.ReadValue<float>(), 0);
		public void OnShowRightPanel(InputAction.CallbackContext context) => PanelsTrigger(context.ReadValue<float>(), 1);

		public void SubmitEditor(InputAction.CallbackContext context) {
			bool isTrue = context.ReadValue<float>() > 0.5f;

			onSubmit = isTrue && userReady && context.action.phase == InputActionPhase.Performed;

			if (isTrue && userReady && context.action.phase == InputActionPhase.Started && raycastHits.Count > 0) {
				selected.Add(raycastHits[0].collider.GetComponent<InGameObjectBehaviour>());
				dragOrigin = cursorToWorld;
				hoverAnObject = true;
			}

			if (!isTrue && onDrag) {
				onDrag = false;
				UpdateColliders();
			}
		}
		public void DeleteEditor(InputAction.CallbackContext context) => onDelete = context.ReadValue<float>() > 0.5f && userReady && context.action.phase == InputActionPhase.Performed;
		public void OnMultiselection(InputAction.CallbackContext context) {
			if (onMultiselection || (userReady && !userIsDragging)) {
				onMultiselection = context.ReadValue<float>() != 0f;

				if (onMultiselection) msDrag.position = cursorToWorld;
			}
		}


		public void UndoAction(InputAction.CallbackContext context) {
			if (context.action.phase != InputActionPhase.Started) return;

			Undo();
		}
		public void RedoAction(InputAction.CallbackContext context) {
			if (context.action.phase != InputActionPhase.Started) return;

			Redo();
		}
		public void DeselectAllAction(InputAction.CallbackContext context) {
			if (context.action.phase != InputActionPhase.Started) return;

			DeselectAll();
		}
		public void DeleteSelectedAction(InputAction.CallbackContext context) {
			if (context.action.phase != InputActionPhase.Started) return;

			DeleteSelected();
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

		struct AnalysisData {
			public int playersNumber, objectsNumber;
		}
	}
}
