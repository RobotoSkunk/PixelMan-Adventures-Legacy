/*
	PixelMan Adventures, an open source platformer game.
	Copyright (C) 2022  RobotoSkunk <contact@robotoskunk.com>

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Affero General Public License as published
	by the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Affero General Public License for more details.

	You should have received a copy of the GNU Affero General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

using Cysharp.Threading.Tasks;

using System.Collections.Generic;
using System.Collections;
using System.Linq;

using RobotoSkunk.PixelMan.Events;
using RobotoSkunk.PixelMan.Gameplay;
using RobootSkunk.PixelMan.LevelEditor;
using RobotoSkunk.PixelMan.LevelEditor.IO;


namespace RobotoSkunk.PixelMan.LevelEditor
{
	[System.Serializable]
	public enum ResizePoints {
		TOP,
		BOTTOM,
		LEFT,
		RIGHT,
		TOP_LEFT,
		TOP_RIGHT,
		BOTTOM_LEFT,
		BOTTOM_RIGHT,
	}

	public struct UndoRedo
	{
		public Type type;
		public List<GOHandler> objCache;

		public enum Type
		{
			Add,
			Remove,
			Properties,
		}

		public UndoRedo(Type type, List<InGameObjectBehaviour> objCache)
		{
			this.type = type;
			List<GOHandler> _tmp = new();

			for (int i = 0; i < objCache.Count; i++) {
				_tmp.Add(GOHandler.GetGOHandler(objCache[i]));
			}

			this.objCache = _tmp;
		}

		/// <summary>
		/// Stores information about a GameObject and its properties.
		/// </summary>
		public struct GOHandler
		{
			public InGameObjectBehaviour reference;
			public InGameObjectProperties properties;
			public InGameObjectProperties lastProperties;

			public static GOHandler GetGOHandler(InGameObjectBehaviour obj) => new()
			{
				reference = obj,
				properties = obj.properties,
				lastProperties = obj.lastProperties
			};
		}
	}



	public class MainEditor : MonoBehaviour
	{
		#region Public attributes
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[Header("Components")]
		public Camera cam;
		public MeshRenderer grids;
		public SpriteRenderer objectPreview;
		public SpriteRenderer selectionArea;
		public Canvas dragArea;
		public RectTransform dragAreaRect;
		public RectTransform resRectArea;
		public RectTransform rotRectArea;
		public PlayerInput playerInput;
		public DragBoxResizer levelBounds;

		[Header("Input system")]
		[SerializeField] InputSystemUIInputModule inputModule;
		[SerializeField] InputActionAsset inputActions;

		[Header("UI components")]
		public CanvasScaler canvasScaler;
		public Image virtualCursorImg;
		public Image undoImg;
		public Image redoImg;
		public Image testButtonImg;
		public Button[] buttons;
		public Image[] gamepadIndications;
		public Panel[] panels;

		[Header("I don't know what name should I use")]
		public LineRenderer[] lineRenderers;

		[Header("Properties")]
		public Vector2 zoomLimits;
		public float defaultOrtho;
		public float navigationSpeed;
		public float cursorSpeed;
		[Range(0f, 1f)] public float zoomSpeedGamepad;
		[Range(0f, 1f)] public float zoomSpeedKeyboard;
		public ContactFilter2D contactFilter;
		// public Sprite[] panelSwitchSprite = new Sprite[2];
		public Sprite[] sprTestBtn;
		public Toggle[] optionsToggles;

		// [Space(25)]
		// public PanelStruct[] panels;

		[Space(25)]
		public LevelIO.TransformContainers containers;

		[Header("Game inspector")]
		public ScrollRect inspector;
		public ObjectInspector inspectorProcessor;
		public RectTransform[] inspectorSections;
		public ButtonSelectObject bso;
		public RectTransform BSOGameplay;
		public RectTransform BSOBlocks;
		public RectTransform BSOObstacles;
		public RectTransform BSODecoration;

		#pragma warning restore IDE0044
		#endregion


		const float panelLimit = 0.9f;

		#region Cache and buffers
		readonly List<RaycastResult> guiResults = new();

		readonly List<RaycastHit2D> raycastHits = new();
		readonly List<RaycastHit2D> msHits = new();
		readonly List<RaycastHit2D> virtualHits = new();

		readonly List<UndoRedo> undoRedo = new();

		readonly List<InGameObjectBehaviour> objCache = new();
		readonly List<InGameObjectBehaviour> selected = new();
		readonly List<InGameObjectBehaviour> multiSelected = new();

		public List<InGameObjectProperties> copiedObjCache = new();

		readonly List<Player> players = new();

		InGameObjectBehaviour[] lastMS = new InGameObjectBehaviour[0];
		InGameObjectBehaviour[] msBuffer = new InGameObjectBehaviour[0];
		InGameObjectBehaviour[] lastSelBuffer = new InGameObjectBehaviour[0];
		InGameObjectBehaviour[] selectedBuffer = new InGameObjectBehaviour[0];

		InGameObjectBehaviour draggedObject;

		List<Vector3>[] playersLineRenderers;

		Coroutine saveCoroutine = null;
		Coroutine collidersCoroutine = null;
		#endregion

		#region Temporal garbage
		Rect msDrag;
		Rect lastResArea;
		Rect newResArea;
		Rect rotArea;

		Material g_Material;

		Vector2 navSpeed;
		Vector2 dragNavOrigin;
		Vector2 cursorToWorld;
		Vector2 resRefPnt;

		float newZoom = 1f;
		float zoomSpeed;
		float msAlpha;
		float rotHandle;
		float uiScale;

		bool somePanelEnabled;
		bool wasMultiselecting;
		bool isOnTest;
		bool wasEditing;
		bool editorIsBusy = true;

		uint undoIndex;

		int sid;

		Coroutine inspectorCoroutine;
		// GameObject[] players;
		#endregion

		#region Commonly used variables
		InputType inputType = InputType.KeyboardAndMouse;

		float zoom = 1f;

		Vector2 cursorPosition;
		// Vector2 Globals.Editor.virtualMousePosition;
		Vector2 mousePosition;
		Vector2 dragOrigin;
		Vector2 virtualPosition;
		Vector2 testPos;

		Bounds selectionBounds;

		// Input events
		bool onSubmit;
		bool onDelete;

		// Editor tools events
		bool onDragNavigation;
		bool onMultiselection;
		bool onDrag;
		bool hoverAnObject;
		bool onResize;
		bool onRotate;

		// UI events
		bool hoverUI;
		bool cursorInWindow;
		bool allowScroll;

		bool historialAvailable = true;

		/// <summary>
		/// Stores a resume of all the information about the objects in the scene.
		/// </summary>
		AnalysisData analysis = new();
		#endregion

		#region Hotdog
		/// <summary>
		/// The actual size of the viewport in world units.
		/// </summary>
		Vector2 cameraSize {
			get {
				float h = cam.orthographicSize * 2f;
				float w = h * cam.aspect;

				return new(w, h);
			}
		}

		/// <summary>
		/// The current selected object ID.
		/// </summary>
		int selectedId {
			get => sid;
			set => sid = Mathf.Clamp(value, 0, Globals.objects.Count - 1);
		}

		/// <summary>
		/// The actual index of the undoRedo list.
		/// </summary>
		int undoDiff {
			get => undoRedo.Count - (int)undoIndex;
		}

		/// <summary>
		/// The current selected object.
		/// </summary>
		InGameObject selectedObject {
			get => Globals.objects[selectedId];
		}

		/// <summary>
		/// If true, the user is ready to interact with the editor.
		/// </summary>
		bool userReady {
			get => !hoverUI && cursorInWindow && !editorIsBusy && !Globals.openSettings;
		}

		/// <summary>
		/// If true, the user is dragging an object or the selection area.
		/// </summary>
		bool userIsDragging {
			get => onRotate || onResize || onDrag || onMultiselection || onDragNavigation;
		}
		#endregion

		#region Unity methods
		private void OnEnable() => GameEventsHandler.PlayerDeath += OnPlayerDeathInTest;
		private void OnDisable() => GameEventsHandler.PlayerDeath -= OnPlayerDeathInTest;


		private void Start()
		{
			#region Reset global variables and set panels default states
			Globals.onLoad = true;
			Globals.onPause = true;

			Globals.loadingText = Globals.languages.GetField("loading.load_objects");

			g_Material = grids.material;
			cursorPosition = Globals.screen / 2f;
			Globals.musicType = GameDirector.MusicClips.Type.EDITOR;


			// Reset undo/redo list
			undoIndex = 0;
			ChangeInspectorSection(0);
			UpdateUndoRedoButtons();
			#endregion

			// foreach (PanelStruct panel in panels) {
			// 	panel.internals.wasOpen = true;
			// }

			#region Add game objects buttons
			float baseSize = 22f;

			for (int i = 0; i < Globals.objects.Count; i++) {

				// Ignore objects that are not supposed to be in the editor
				if (Globals.objects[i].category == InGameObject.Category.IGNORE) {
					continue;
				}

				// Select correct parent for the button based on the object category
				RectTransform parent = Globals.objects[i].category switch {
					InGameObject.Category.GAMEPLAY => BSOGameplay,
					InGameObject.Category.BLOCKS => BSOBlocks,
					InGameObject.Category.OBSTACLES => BSOObstacles,
					InGameObject.Category.DECORATION => BSODecoration,
					_ => null
				};


				// Create button
				ButtonSelectObject newButton = Instantiate(bso, parent);

				Sprite spritePreview = Globals.objects[i].preview;
				newButton.preview.sprite = spritePreview;
				newButton.preview.SetNativeSize();


				float maxSize = Mathf.Max(spritePreview.rect.size.x, spritePreview.rect.size.y);

				if (maxSize > baseSize) {
					Vector3 newSize = baseSize / maxSize * newButton.preview.rectTransform.localScale;
					newSize.z = 1;

					newButton.preview.rectTransform.localScale = newSize;
				}


				int x = i;

				newButton.button.onClick.AddListener(() => SetObjectId(x));
				// newButton.onSelect.AddListener(() => OpenPanel(1));
				// panels[1].content.Add(tmp.button);
			}
			#endregion

			#region Set toggle buttons values
			bool[] toggleValues = {
				Globals.Editor.snap,
				Globals.Editor.handleLocally
			};

			for (int i = 0; i < toggleValues.Length; i++) {
				if (i < optionsToggles.Length) {
					optionsToggles[i].SetIsOnWithoutNotify(toggleValues[i]);
				}
			}
			#endregion


			// GameObject[] objs = GameObject.FindGameObjectsWithTag("EditorObject");

			// for (int i = 0; i < objs.Length; i++) {
			// 	objCache.Add(objs[i].GetComponent<InGameObjectBehaviour>());
			// }

			#region Set up players line renderers
			playersLineRenderers = new List<Vector3>[lineRenderers.Length];

			for (int i = 0; i < playersLineRenderers.Length; i++) {
				playersLineRenderers[i] = new List<Vector3>();
			}
			#endregion

			#region Load level from file
			if (Globals.Editor.currentScene.file != null) {
				UniTask.Void(async () =>
				{
					await LevelIO.LoadLevel(
						true,
						containers,
						objCache
					);

					editorIsBusy = false;

					levelBounds.SetRect(Globals.levelData.bounds);
					EditorEventsHandler.InvokeOnReady();
				});
			} else {
				Globals.onLoad = editorIsBusy = false;
				Globals.loadProgress = 0f;
				Globals.levelData = new Level();

				EditorEventsHandler.InvokeOnReady();
			}
			#endregion
		}

		private void Update()
		{
			Vector2 transformSpeed = navSpeed;

			if (uiScale != Globals.settings.editor.uiScale) {
				float newUIScale = 2f - Mathf.Clamp(Globals.settings.editor.uiScale, 0.15f, 1.35f);

				canvasScaler.referenceResolution = Constants.referenceResolution * newUIScale;

				uiScale = Globals.settings.editor.uiScale;
			}

			// TODO: Replace this with a new Panel system
			// #region Panels controller
			// for (int i = 0; i < panels.Length; i++) {
			// 	panels[i].internals.position = Mathf.Lerp(panels[i].internals.position, panels[i].internals.nextPosition + panels[i].internals.deltaPosition, 0.5f * RSTime.delta);

			// 	panels[i].rectTransforms.anchoredPosition = new(-panels[i].size + panels[i].internals.position * panels[i].size, panels[i].rectTransforms.anchoredPosition.y);

			// 	panels[i].internals.enabled = panels[i].internals.position > panelLimit;

			// 	panels[i].switchImage.sprite = panelSwitchSprite[(!panels[i].internals.enabled).ToInt()];
			// }
			// #endregion

			#region Zoom
			newZoom += zoomSpeed * RSTime.delta;

			newZoom = Mathf.Clamp(newZoom, zoomLimits.x, zoomLimits.y);
			zoom = Mathf.Lerp(zoom, newZoom, 0.3f * RSTime.delta);

			cam.orthographicSize = zoom * defaultOrtho;
			#endregion

			#region Controls processor

			if (inputType == InputType.KeyboardAndMouse) {
				cursorPosition = mousePosition;
				virtualCursorImg.enabled = false;

				cursorInWindow = Globals.screenRect.Contains(cursorPosition);

				inputActions.FindAction("UI/Navigate").Disable();
				// inputModule.move
			} else {
				inputActions.FindAction("UI/Navigate").Enable();
			}

			/*
			switch (inputType) {
				case InputType.Gamepad:
					// hoverUI = panels[0].internals.enabled || panels[1].internals.enabled;
					// if (hoverUI) {
					// 	navSpeed = Vector2.zero;
					// }

					// cursorPosition += cursorSpeed * RSTime.delta * navSpeed;
					// cursorPosition = RSMath.Clamp(cursorPosition, Vector2.zero, Globals.screen);

					// if (cursorPosition.x > 0f && cursorPosition.x < Screen.width) {
					// 	transformSpeed.x = 0f;
					// }
					// if (cursorPosition.y > 0f && cursorPosition.y < Screen.height) {
					// 	transformSpeed.y = 0f;
					// }

					// virtualCursorImg.rectTransform.position = cursorPosition;
					// virtualCursorImg.enabled = !hoverUI;

					// TODO: Replace this with a easier way to do it
					// #region Panels buttons
					// if (buttons[0].navigation.mode != Navigation.Mode.Automatic) {
					// 	buttons.SetNavigation(Navigation.Mode.Automatic);
					// }

					// if (buttons[0].interactable != hoverUI) {
					// 	buttons.SetInteractable(hoverUI);
					// }

					// foreach (PanelStruct panel in panels) {
					// 	if (!somePanelEnabled && panel.internals.enabled) {
					// 		somePanelEnabled = true;

					// 		panel.defaultSelected.Select();
					// 	}
						
					// 	if (hoverUI && panel.internals.wasOpen != panel.internals.enabled && !panel.internals.enabled) {
					// 		for (int i = 0; i < panel.content.Count; i++) {
					// 			if (panel.content[i].GetInstanceID() == Globals.buttonSelected) {
					// 				panel.outSelected.Select();
					// 				break;
					// 			}
					// 		}
					// 	}

					// 	if (panel.content[0].navigation.mode != Navigation.Mode.Automatic) {
					// 		panel.content.SetNavigation(Navigation.Mode.Automatic);
					// 	}
					// }

					// if (somePanelEnabled && !hoverUI) {
					// 	somePanelEnabled = false;
					// }
					// #endregion

					// if (!cursorInWindow) {
					// 	cursorInWindow = true;
					// }
					// break;
				case InputType.KeyboardAndMouse:
					cursorPosition = mousePosition;
					virtualCursorImg.enabled = false;

				// 	// TODO: Replace this with a easier way to do it... again
				// 	// if (!buttons[0].interactable) {
				// 	// 	buttons.SetInteractable(true);
				// 	// }

				// 	// if (buttons[0].navigation.mode != Navigation.Mode.None) {
				// 	// 	buttons.SetNavigation(Navigation.Mode.None);
				// 	// }

				// 	// foreach (PanelStruct panel in panels) {
				// 	// 	if (panel.content[0].navigation.mode != Navigation.Mode.None) {
				// 	// 		panel.content.SetNavigation(Navigation.Mode.None);
				// 	// 	}
				// 	// }

					cursorInWindow = Globals.screenRect.Contains(cursorPosition);
					break;
			}


			// foreach (PanelStruct panel in panels) {
			// 	if (panel.internals.wasOpen != panel.internals.enabled) {
			// 		panel.internals.wasOpen = panel.internals.enabled;

			// 		panel.content.SetInteractable(panel.internals.enabled);
			// 	}
			// }
			*/
			#endregion

			// If user finished selecting objects, move the selected objects to the perma-selected list
			if (!onMultiselection && multiSelected.Count > 0) {
				for (int i = 0; i < multiSelected.Count; i++) {
					if (!selected.Contains(multiSelected[i])) {
						selected.Add(multiSelected[i]);
					}
				}

				multiSelected.Clear();
			}


			if (!isOnTest) {
				transform.position += navigationSpeed * zoom * RSTime.delta * (Vector3)transformSpeed;
				transform.position = new Vector3(transform.position.x, transform.position.y, -10f);
			}
		}

		private void LateUpdate()
		{
			cursorToWorld = cam.ScreenToWorldPoint(cursorPosition);
			Globals.Editor.virtualMousePosition = Snapping.Snap(cursorToWorld, (Globals.Editor.snap ? 1f : Constants.pixelToUnit) * Vector2.one);
			virtualPosition = Globals.Editor.snap ? Snapping.Snap(transform.position, Vector2.one) : transform.position;

			#region Preview and selection area
			if (!isOnTest) {
				msAlpha = Mathf.Lerp(msAlpha, onMultiselection.ToInt(), 0.15f * RSTime.delta);

				if (onMultiselection) {
					msDrag.size = cursorToWorld - msDrag.position;
				}

				objectPreview.transform.position = Globals.Editor.virtualMousePosition;
				objectPreview.enabled = userReady && !userIsDragging;
				objectPreview.sprite = selectedObject.preview;

				selectionArea.transform.position = msDrag.position;
				selectionArea.transform.localScale = (Vector3)(zoom * Vector2.one) + Vector3.forward;
				selectionArea.size = msDrag.size / zoom;
				selectionArea.color = new Color(0.23f, 0.35f, 1f, msAlpha);
				selectionArea.enabled = msAlpha > 0.05f;
			}
			#endregion

			#region Grids
			Vector2 gridScale = cameraSize / 10f;
			float pixelSize = zoom * (defaultOrtho * 2f / Screen.height) * 0.1f;

			grids.transform.localScale = new(gridScale.x, 1f, gridScale.y);
			g_Material.SetFloat("_Thickness", pixelSize);
			g_Material.SetColor("_MainColor", Color.white);
			g_Material.SetColor("_SecondaryColor", new Color(1, 1, 1, 0.2f));
			#endregion

			for (int i = 0; i < lineRenderers.Length; i++) {
				lineRenderers[i].widthMultiplier = zoom * 0.025f;
			}

			#region Selection
			// Multiselection buffer
			if (!onMultiselection) {
				if (multiSelected.Count > 0) {
					multiSelected.Clear();
				}
				if (lastMS.Length > 0) {
					lastMS = new InGameObjectBehaviour[0];
				}

				if (wasMultiselecting) {
					wasMultiselecting = false;
					analysis = AnalizeObjects(AnalysisData.Area.SELECTED);
					ScanSelectedObjects();
				}
			}

			// Dragging all
			if (onDrag) {
				Vector2 newPos = Snapping.Snap(
					cursorToWorld + draggedObject.dragOrigin,
					(Globals.Editor.snap ? 1f : Constants.pixelToUnit) * Vector2.one
				);
				draggedObject.transform.position = newPos;

				selectionBounds = GetSelectionBounds();

				for (int i = 0; i < selected.Count; i++) {
					if (selected[i] != draggedObject) {
						selected[i].SetPosition(newPos - selected[i].dist2Dragged);
					}
				}
			}

			// Interpreting selection area
			if (analysis.selectedNumber != 0 && !isOnTest) {
				dragArea.enabled = true;
				dragAreaRect.transform.position = selectionBounds.center;
				dragAreaRect.transform.localScale = (Vector3)(zoom * Vector2.one) + Vector3.forward;
				dragAreaRect.sizeDelta = (selectionBounds.size + new Vector3(1f, 1f, 0f)) / zoom;
			} else {
				dragArea.enabled = false;
			}
			#endregion
		}

		private void FixedUpdate()
		{
			if (isOnTest) {
				if (players.Count > 0) {
					Vector3 minPos = new(), maxPos = new();
					int j = 0;
					bool minMaxDefined = false;

					for (int i = 0; i < players.Count; i++) {
						if (!players[i]) {
							continue;
						}

						if (!minMaxDefined) {
							minPos = players[i].transform.position;
							maxPos = players[i].transform.position;
							minMaxDefined = true;
						} else {
							minPos = Vector3.Min(minPos, players[i].transform.position);
							maxPos = Vector3.Max(maxPos, players[i].transform.position);
						}


						if (j < lineRenderers.Length) {
							Vector2 diff = (Vector2)players[i].transform.position - players[i].lastPublicPos;

							if (diff.magnitude > 0.05f) {
								if (playersLineRenderers[j].Count > Globals.settings.editor.lineLength) {
									playersLineRenderers[j].RemoveAt(0);
								}
								playersLineRenderers[j].Add(players[i].transform.position);

								lineRenderers[j].positionCount = playersLineRenderers[j].Count;
								lineRenderers[j].SetPositions(playersLineRenderers[j].ToArray());
							}

							j++;
							players[i].lastPublicPos = players[i].transform.position;
						}
					}

					Vector3 center = (minPos + maxPos) * 0.5f;
					testPos = Vector2.Lerp(testPos, center, 0.3f);

					transform.position = (Vector3)(testPos + (1f + zoom) * Globals.shakeForce * 0.5f * Random.insideUnitCircle) + Vector3.forward * -10f;
				}
			}

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

				for (int i = 0; i < msBuffer.Length; i++) {
					msBuffer[i].color = Color.white;
				}

				lastMS = multiSelected.ToArray();


			} else if (userReady && !userIsDragging) {
				int nmb = Physics2D.Raycast(cursorToWorld, Vector2.zero, contactFilter, raycastHits, 1f);
				Physics2D.Raycast(Globals.Editor.virtualMousePosition, Vector2.zero, contactFilter, virtualHits, 1f);


				switch (nmb) {
					case > 0:
						InGameObjectBehaviour pointedObject = raycastHits[0].collider.GetComponent<InGameObjectBehaviour>();

						if (hoverAnObject && selected.Contains(pointedObject) && onSubmit && dragOrigin != cursorToWorld) {
							onDrag = true;
							hoverAnObject = false;
							draggedObject = pointedObject;


							draggedObject.dragOrigin = (Vector2)draggedObject.transform.position - dragOrigin;

							for (int i = 0; i < selected.Count; i++) {
								selected[i].SetLastProperties();
								selected[i].dist2Dragged = draggedObject.transform.position - selected[i].transform.position;
							}

							wasEditing = true;
						} else if (onDelete) {
							List<InGameObjectBehaviour> listBuffer = new();

							foreach (RaycastHit2D ray in raycastHits) {
								GameObject obj = ray.collider.gameObject;
								InGameObjectBehaviour scr = obj.GetComponent<InGameObjectBehaviour>();

								listBuffer.Add(scr);
								obj.SetActive(false);
							}

							UpdateUndoRedoList(UndoRedo.Type.Remove, listBuffer);
						}
						break;
					case 0:
						if (onSubmit && virtualHits.Count == 0) {
							if (selectedId == Constants.InternalIDs.player && analysis.playersNumber >= 2) {
								return;
							}

							InGameObjectBehaviour newObj = CreateObject(selectedId, Globals.Editor.virtualMousePosition, Vector2.one, 0f);

							wasEditing = true;
							UpdateUndoRedoList(UndoRedo.Type.Add, newObj);
						}
						break;
				}
			}
		}

		private void OnGUI()
		{
			if (inputType != InputType.Gamepad) {
				PointerEventData evData = new(EventSystem.current) {
					position = cursorPosition
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
		// TODO: Deprecate this and reemplaze it with a new Panel system
		/// <summary>
		/// Opens or closes a panel.
		/// </summary>
		void PanelsTrigger(float value, int index)
		{
			// if (val > panelLimit && !panels[i].internals.wasEnabled) {
			// 	panels[i].internals.wasEnabled = true;
			// 	panels[i].internals.nextPosition = !panels[i].internals.enabled ? 1f : panelLimit - 0.01f;

			// } else if (val <= panelLimit) {
			// 	panels[i].internals.wasEnabled = false;

			// 	if (!panels[i].internals.enabled) {
			// 		panels[i].internals.nextPosition = val;
			// 	}
			// }
			// panels[index].SetOpen(value);
		}

		/// <summary>
		/// Updates undo and redo buttons to show if there are any actions to undo or redo.
		/// </summary>
		void UpdateUndoRedoButtons()
		{
			undoImg.color = undoRedo.Count == undoIndex ? Color.grey : Color.white;
			redoImg.color = undoIndex == 0 ? Color.grey : Color.white;

			analysis = AnalizeObjects();
			ScanSelectedObjects();
		}

		/// <summary>
		/// Changes the undo/redo history adding new entries and removing old ones.
		/// </summary>
		void UpdateUndoRedoList(UndoRedo.Type type, List<InGameObjectBehaviour> objs)
		{
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
			} else if (undoRedo.Count == Globals.settings.editor.historialSize) {
				undoRedo.RemoveAt(0);
			}

			undoRedo.Add(undoBuffer);
			UpdateUndoRedoButtons();
			UpdateColliders();
		}

		/// <summary>
		/// Changes the undo/redo history adding a new entry and removing old ones.
		/// </summary>
		void UpdateUndoRedoList(UndoRedo.Type type, InGameObjectBehaviour obj)
		{
			UpdateUndoRedoList(type, new List<InGameObjectBehaviour>() { obj });
		}

		/// <summary>
		/// Stores changes of the current selected objects in the undo/redo history.
		/// </summary>
		void StoreSelectedObjectsInUndoRedoList()
		{
			List<InGameObjectBehaviour> tmp = new();

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				tmp.Add(selected[i]);
			}

			UpdateUndoRedoList(UndoRedo.Type.Properties, tmp);
		}

		/// <summary>
		/// Scans selected objects and sends their information to the inspector.
		/// </summary>
		void ScanSelectedObjects()
		{
			// Update colors
			for (int i = 0; i < selected.Count; i++) {
				if (selected[i]) {
					selected[i].color = Constants.Colors.green;
				}
			}

			// Prepare buffer
			selectedBuffer = lastSelBuffer.Except(selected).ToArray();

			for (int i = 0; i < selectedBuffer.Length; i++) {
				if (selectedBuffer[i]) {
					selectedBuffer[i].ResetColor();
				}
			}


			lastSelBuffer = selected.ToArray();

			// Update selection bounds
			selectionBounds = GetSelectionBounds();

			// Send them to the inspector
			inspectorProcessor.PrepareInspector(lastSelBuffer);
		}

		/// <summary>
		/// Analizes all the objects in the scene and returns a resume of the information.
		/// </summary>
		AnalysisData AnalizeObjects(AnalysisData.Area area = AnalysisData.Area.ALL)
		{
			AnalysisData analysis = new();

			for (int i = 0; i < objCache.Count; i++) {
				if (!objCache[i]) {
					continue;
				}
				if (!objCache[i].gameObject.activeInHierarchy) {
					continue;
				}


				if ((area & AnalysisData.Area.OBJECTS) != 0) {
					analysis.objectsNumber++;
				}


				if (
					(area & AnalysisData.Area.PLAYERS) != 0 &&
					objCache[i].properties.id == Constants.InternalIDs.player
				) {
					analysis.playersNumber++;
				}


				if (
					(area & AnalysisData.Area.SELECTED) != 0 &&
					selected.Contains(objCache[i])
				) {
					analysis.selectedNumber++;
				}
			}

			return analysis;
		}

		/// <summary>
		/// Scans all selected objects and updates the selection bounds.
		/// </summary>
		Bounds GetSelectionBounds()
		{
			if (selected.Count > 0) {
				Bounds bounds = new();
				bool isVariableAssigned = false; // Unity Technologies eats crayons


				for (int i = 0; i < selected.Count; i++) {
					if (!selected[i]) {
						continue;
					}
					if (!selected[i].gameObject.activeInHierarchy) {
						continue;
					}


					if (!isVariableAssigned) {
						bounds = selected[i].bounds;

						isVariableAssigned = true;
					} else {
						bounds.Encapsulate(selected[i].bounds);
					}
				}

				return bounds;
			}

			return new Bounds();
		}

		/// <summary>
		/// Creates a new game object in the scene.
		/// </summary>
		InGameObjectBehaviour CreateObject(int id, Vector2 position, Vector2 scale, float rotation)
		{
			InGameObjectBehaviour newObject = LevelIO.CreateObject(
				id,
				position,
				scale,
				rotation,
				true,
				containers
			);

			objCache.Add(newObject);

			return newObject;
		}

		/// <summary>
		/// Updates colliders composite geometry.
		/// </summary>
		void UpdateColliders(bool lazyUpdate = true)
		{
			if (lazyUpdate) {
				if (collidersCoroutine != null) {
					StopCoroutine(collidersCoroutine);
				}
				collidersCoroutine = StartCoroutine(LazyUpdateColliders());
			} else {
				PhysicsEventsHandler.GenerateCompositeGeometry();
			}
		}

		/// <summary>
		/// Starts or stops the test mode.
		/// </summary>
		void SetUpTestingState(bool enabled)
		{
			Globals.onPause = !enabled;
			Globals.isDead = false;
			Globals.respawnAttempts = 0;


			// for (int i = 0; i < panels.Length; i++) {
			// 	panels[i].group.interactable = !enabled;
			// 	panels[i].group.blocksRaycasts = !enabled;
			// 	panels[i].group.alpha = (!enabled).ToInt();
			// }


			if (enabled) {
				Globals.levelData.bounds = levelBounds.rect;
				UpdateColliders(false);

				foreach (Panel panel in panels) {
					panel.SetOpen(false);
				}

				EditorEventsHandler.InvokeStartTesting();
				GameEventsHandler.InvokeLevelReady();
				testPos = transform.position;

				GameObject[] plyrs = GameObject.FindGameObjectsWithTag("Player");

				for (int i = 0; i < plyrs.Length; i++) {
					players.Add(plyrs[i].GetComponent<Player>());
				}

				for (int i = 0; i < lineRenderers.Length; i++) {
					lineRenderers[i].positionCount = 0;
				}
				for (int i = 0; i < playersLineRenderers.Length; i++) {
					playersLineRenderers[i].Clear();
				}
			} else {
				GameEventsHandler.InvokeResetObject();
				EditorEventsHandler.InvokeEndTesting();

				players.Clear();
			}

			testButtonImg.sprite = sprTestBtn[enabled.ToInt()];
			// playerInput.enabled = !enabled;
		}

		/// <summary>
		/// Is triggered when the player dies in test mode.
		/// </summary>
		void OnPlayerDeathInTest()
		{
			if (!isOnTest) {
				return;
			}

			StartCoroutine(ResetTestObjects());
		}

		/// <summary>
		/// Returns an object that has not been selected yet under raycastHits.
		/// </summary>
		InGameObjectBehaviour FindNotSelectedHoveredObject()
		{
			for (int i = 0; i < raycastHits.Count; i++) {
				InGameObjectBehaviour tmp = raycastHits[i].collider.GetComponent<InGameObjectBehaviour>();

				if (!selected.Contains(tmp)) {
					return tmp;
				}
			}

			return null;
		}

		/// <summary>
		/// Returns true if the user is hovering an object that has been selected.
		/// </summary>
		bool IsHoverSelectedObject()
		{
			for (int i = 0; i < raycastHits.Count; i++) {
				InGameObjectBehaviour tmp = raycastHits[i].collider.GetComponent<InGameObjectBehaviour>();

				if (selected.Contains(tmp)) {
					return true;
				}
			}

			return false;
		}
		#endregion

		#region Public methods
		public void ButtonResetZoom()
		{
			newZoom = 1f;
		}

		public void ButtonChangeZoom(float toAdd)
		{
			newZoom += toAdd;
		}


		public void SetObjectId(int index)
		{
			selectedId = index;
		}


		public void ChangeInspectorSection(int i)
		{
			for (int a = 0; a < inspectorSections.Length; a++) {
				inspectorSections[a].gameObject.SetActive(a == i);

				if (a == i) {
					inspector.content = inspectorSections[a];
				}
			}
		}

		public void HandleInspectorChanges(ObjectInspector.Section section, ObjectInspector.Section.PropertyField field)
		{
			bool storeLast = inspectorCoroutine == null;

			if (inspectorCoroutine != null) {
				StopCoroutine(inspectorCoroutine);
			}

			inspectorCoroutine = StartCoroutine(Inspector2Historial());


			historialAvailable = false;

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				if (storeLast) {
					selected[i].SetLastProperties();
				}

				InGameObjectProperties prop = selected[i].properties;

				switch (section.dataType) {
					case ObjectInspector.Section.DataType.Vector2D:
						bool isX = field.axis == ObjectInspector.Section.PropertyField.Axis.x;

						switch (section.purpose) {
							case PropertiesEnum.Position:
								if (isX) {
									prop.position.x = field.GetFloat();
								} else {
									prop.position.y = field.GetFloat();
								}
								break;
							case PropertiesEnum.FreeScale:
								if (isX) {
									prop.scale.x = field.GetFloat();
								} else {
									prop.scale.y = field.GetFloat();
								}
								break;

							// No default
						}
						break;

					case ObjectInspector.Section.DataType.Float:
						switch (section.purpose) {
							case PropertiesEnum.Scale:
								prop.scale = field.GetFloat() * Vector2.one;
								break;

							case PropertiesEnum.Rotation:
								prop.rotation = field.GetFloat();
								break;

							case PropertiesEnum.wakeTime:
								prop.wakeTime = field.GetFloat();
								break;

							case PropertiesEnum.ReloadTime:
								prop.reloadTime = field.GetFloat();
								break;

							case PropertiesEnum.Speed:
								prop.speed = field.GetFloat();
								break;

							// No default
						}
						break;

					case ObjectInspector.Section.DataType.Integer:
						prop.renderOrder = field.GetInt();
						break;

					case ObjectInspector.Section.DataType.Boolean:
						switch (section.purpose) {
							case PropertiesEnum.InvertGravity:
								prop.invertGravity = field.GetBool();
								break;
							case PropertiesEnum.SpawnSaw:
								prop.spawnSaw = field.GetBool();
								break;

							// No default
						}
						break;

					// No default
				}

				selected[i].properties = prop;
			}

			ScanSelectedObjects();
		}


		public void Undo()
		{
			if (undoRedo.Count == undoIndex || !historialAvailable || isOnTest) {
				return;
			}
			UndoRedo undoBuffer = undoRedo[undoDiff - 1];

			foreach (UndoRedo.GOHandler obj in undoBuffer.objCache) {
				switch (undoBuffer.type) {
					case UndoRedo.Type.Add:
						obj.reference.gameObject.SetActive(false);
						break;

					case UndoRedo.Type.Remove:
						obj.reference.gameObject.SetActive(true);
						break;

					case UndoRedo.Type.Properties:
						obj.reference.properties = obj.lastProperties;
						break;

					// No default
				}
			}

			undoIndex++;
			UpdateUndoRedoButtons();
			UpdateColliders();
		}

		public void Redo()
		{
			if (undoIndex == 0 || !historialAvailable || isOnTest) {
				return;
			}
			UndoRedo undoBuffer = undoRedo[undoDiff];

			foreach (UndoRedo.GOHandler obj in undoBuffer.objCache) {
				switch (undoBuffer.type) {
					case UndoRedo.Type.Add:
						obj.reference.gameObject.SetActive(true);
						break;

					case UndoRedo.Type.Remove:
						obj.reference.gameObject.SetActive(false);
						break;

					case UndoRedo.Type.Properties:
						obj.reference.properties = obj.properties;
						break;

					// No default
				}
			}

			undoIndex--;
			UpdateUndoRedoButtons();
			UpdateColliders();
		}


		public void DeselectAll()
		{
			if (selected.Count == 0) {
				return;
			}
			selected.Clear();

			ScanSelectedObjects();

			analysis.selectedNumber = 0;
		}

		public void DeleteSelected()
		{
			if (selected.Count == 0) {
				return;
			}

			List<InGameObjectBehaviour> listBuffer = new();

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				listBuffer.Add(selected[i]);
				selected[i].gameObject.SetActive(false);
			}

			selected.Clear();
			ScanSelectedObjects();

			analysis = AnalizeObjects();
			UpdateUndoRedoList(UndoRedo.Type.Remove, listBuffer);
		}


		public void StartResizing(EditorDragArea handler)
		{
			onResize = true;
			rotRectArea.gameObject.SetActive(false);

			lastResArea = newResArea = new Rect(selectionBounds.min, selectionBounds.size);

			resRefPnt = handler.resizePoint switch {
				ResizePoints.TOP          => new(0f, lastResArea.yMin),
				ResizePoints.RIGHT        => new(lastResArea.xMin, 0f),
				ResizePoints.BOTTOM       => new(0f, lastResArea.yMax),
				ResizePoints.LEFT         => new(lastResArea.xMax, 0f),

				ResizePoints.TOP_LEFT     => new(lastResArea.xMax, lastResArea.yMin),
				ResizePoints.TOP_RIGHT    => new(lastResArea.xMin, lastResArea.yMin),
				ResizePoints.BOTTOM_LEFT  => new(lastResArea.xMax, lastResArea.yMax),
				ResizePoints.BOTTOM_RIGHT => new(lastResArea.xMin, lastResArea.yMax),

				_ => new()
			};

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				selected[i].SetLastProperties();
				selected[i].dist2Dragged = newResArea.center - (Vector2)selected[i].transform.position;
			}
		}

		public void UpdateResizing(EditorDragArea handler)
		{
			Vector4 a = newResArea.MinMaxToVec4();
			Vector2 B = Globals.Editor.virtualMousePosition + new Vector2(
				Globals.Editor.virtualMousePosition.x < resRefPnt.x ? 0.5f : -0.5f,
				Globals.Editor.virtualMousePosition.y < resRefPnt.y ? 0.5f : -0.5f
			);

			if (Mathf.Abs(Globals.Editor.virtualMousePosition.x - resRefPnt.x) <= 0.5f) {
				B.x = resRefPnt.x;
			}
			if (Mathf.Abs(Globals.Editor.virtualMousePosition.y - resRefPnt.y) <= 0.5f) {
				B.y = resRefPnt.y;
			}


			// Hola, soy RobotoSkunk del 2022, y ahora que veo esta reverenda porquería, comienzo a replantear si mi intelecto es el suficiente para continuar con esto.
			// Apenas en Febrero o Marzo del 2022 descubrí que existen las máscaras de bits, por lo que esta parte del código es redundante como la m...
			// Si a alguien le interesa mejorar la calidad de esta pieza de código, pues adelante, porque por mi parte tengo tanta pereza que ni siquiera traduje esto al inglés xD
			Vector4 newData = handler.resizePoint switch {
				ResizePoints.TOP          => new(a.x, a.y, a.z, B.y),
				ResizePoints.RIGHT        => new(a.x, a.y, B.x, a.w),
				ResizePoints.BOTTOM       => new(a.x, B.y, a.z, a.w),
				ResizePoints.LEFT         => new(B.x, a.y, a.z, a.w),

				ResizePoints.TOP_LEFT     => new(B.x, a.y, a.z, B.y),
				ResizePoints.TOP_RIGHT    => new(a.x, a.y, B.x, B.y),
				ResizePoints.BOTTOM_LEFT  => new(B.x, B.y, a.z, a.w),
				ResizePoints.BOTTOM_RIGHT => new(a.x, B.y, B.x, a.w),

				_ => new()
			};

			newResArea = Rect.MinMaxRect(newData.x, newData.y, newData.z, newData.w);
			selectionBounds.SetMinMax(new Vector2(newData.x, newData.y), new Vector2(newData.z, newData.w));

			selectionBounds.extents = RSMath.Abs(selectionBounds.extents);

			Vector2 fixedScale = new(
				lastResArea.size.x <= Mathf.Epsilon ? newResArea.size.x : newResArea.size.x / lastResArea.size.x,
				lastResArea.size.y <= Mathf.Epsilon ? newResArea.size.y : newResArea.size.y / lastResArea.size.y
			);

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}


				selected[i].SetScale(RSMath.Abs(selected[i].lastProperties.scale * fixedScale));

				if (!Globals.Editor.handleLocally) {
					selected[i].SetPosition(newResArea.center - selected[i].dist2Dragged * fixedScale);
				}
			}
		}

		public void EndResizing()
		{
			onResize = false;
			rotRectArea.gameObject.SetActive(true);

			StoreSelectedObjectsInUndoRedoList();

			selectionBounds = GetSelectionBounds();
			UpdateColliders();
		}


		public void StartRotating()
		{
			onRotate = true;
			resRectArea.gameObject.SetActive(false);

			rotArea = new Rect(selectionBounds.min, selectionBounds.size);

			rotHandle = RSMath.Direction(rotArea.center, cursorToWorld) * Mathf.Rad2Deg;

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				selected[i].SetLastProperties();
				selected[i].dist2Dragged = rotArea.center - (Vector2)selected[i].transform.position;
			}
		}

		public void UpdateRotating()
		{
			float newRot = Snapping.Snap(
				rotHandle - RSMath.Direction(rotArea.center, cursorToWorld) * Mathf.Rad2Deg,
				Globals.Editor.snap ? 15f : 0f
			);
			float alpha = -newRot * Mathf.Deg2Rad;

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				selected[i].SetRotation(selected[i].lastProperties.rotation - newRot);

				if (!Globals.Editor.handleLocally) {
					selected[i].SetPosition(rotArea.center - RSMath.Rotate(selected[i].dist2Dragged, alpha));
				}
			}

			rotRectArea.transform.eulerAngles = new Vector3(0f, 0f, -newRot);
		}

		public void EndRotating()
		{
			resRectArea.gameObject.SetActive(true);
			rotRectArea.transform.eulerAngles = Vector3.zero;

			onRotate = false;
			StoreSelectedObjectsInUndoRedoList();

			selectionBounds = GetSelectionBounds();
			UpdateColliders();
		}


		public void CopySelection()
		{
			if (selected.Count == 0 || isOnTest) {
				return;
			}
			copiedObjCache.Clear();

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				InGameObjectProperties __tmp = selected[i].properties;
				__tmp.position -= (Vector2)selectionBounds.center;

				copiedObjCache.Add(__tmp);
			}
		}

		public void PasteSelection()
		{
			if (copiedObjCache.Count == 0 || isOnTest) {
				return;
			}
			List<InGameObjectBehaviour> tmpCache = new();

			for (int i = 0; i < copiedObjCache.Count; i++) {
				InGameObjectProperties __tmp = copiedObjCache[i];
				__tmp.position += (Vector2)virtualPosition;

				tmpCache.Add(CreateObject((int)__tmp.id, __tmp.position, __tmp.scale, __tmp.rotation));
			}

			selected.Clear();
			selected.AddRange(tmpCache);
			ScanSelectedObjects();
			
			analysis = AnalizeObjects();
			UpdateUndoRedoList(UndoRedo.Type.Add, tmpCache);
		}

		public void DuplicateSelection()
		{
			if (selected.Count == 0 || isOnTest) {
				return;
			}
			List<InGameObjectBehaviour> tmpCache = new();

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				InGameObjectProperties __tmp = selected[i].properties;
				tmpCache.Add(CreateObject((int)__tmp.id, __tmp.position, __tmp.scale, __tmp.rotation));
			}

			selected.Clear();
			selected.AddRange(tmpCache);
			ScanSelectedObjects();
			
			analysis = AnalizeObjects();
			UpdateUndoRedoList(UndoRedo.Type.Add, tmpCache);
		}

		public void CutSelection()
		{
			if (selected.Count == 0 || isOnTest) {
				return;
			}

			copiedObjCache.Clear();
			List<InGameObjectBehaviour> tmpCache = new();

			for (int i = 0; i < selected.Count; i++) {
				if (!selected[i]) {
					continue;
				}
				if (!selected[i].gameObject.activeInHierarchy) {
					continue;
				}

				InGameObjectProperties __tmp = selected[i].properties;
				__tmp.position -= (Vector2)selectionBounds.center;

				tmpCache.Add(selected[i]);
				selected[i].gameObject.SetActive(false);
				copiedObjCache.Add(__tmp);
			}

			selected.Clear();
			ScanSelectedObjects();
			
			analysis = AnalizeObjects();
			UpdateUndoRedoList(UndoRedo.Type.Remove, tmpCache);
		}


		public void ToggleTestingState()
		{
			isOnTest = !isOnTest;
			SetUpTestingState(isOnTest);
		}

		public void TestButton(string text)
		{
			Debug.Log(text);
		}

		public void SaveLevel()
		{
			saveCoroutine ??= StartCoroutine(SaveLvlRoutine());
		}

		public void OpenSettings()
		{
			Globals.openSettings = true;
		}
		#endregion

		#region Input System
		public void OnMousePosition(InputAction.CallbackContext context)
		{
			Vector2 val = context.ReadValue<Vector2>();

			switch (inputType) {
				case InputType.Gamepad:
					if (!hoverUI) {
						navSpeed = val;
					}
					break;

				case InputType.KeyboardAndMouse:
					mousePosition = val;

					if (onDragNavigation) {
						transform.position =
							(Vector3)dragNavOrigin -
							((Vector3)cursorToWorld - transform.position) + 10f * Vector3.back;
					}
					break;

				// No default
			}
		}

		public void OnNavigation(InputAction.CallbackContext context)
		{
			if (Keyboard.current?.ctrlKey.isPressed ?? false) {
				return;
			}

			navSpeed = Globals.onEditField ? Vector2.zero : context.ReadValue<Vector2>();
		}

		public void OnDragNavigation(InputAction.CallbackContext context)
		{
			if (isOnTest) {
				return;
			}

			if (onDragNavigation || (!hoverUI && !onDragNavigation)) {
				onDragNavigation = context.ReadValue<float>() != 0f;

				if (onDragNavigation) {
					dragNavOrigin = cursorToWorld;
				}
			}
		}

		public void OnZoom(InputAction.CallbackContext context)
		{
			if (!allowScroll) {
				return;
			}

			float val = -context.ReadValue<Vector2>().y;

			zoomSpeed = val > 0f ? 1f : (val < 0f ? -1f : 0f);

			switch (inputType) {
				case InputType.Gamepad:
					zoomSpeed *= zoomSpeedGamepad;
					break;

				case InputType.KeyboardAndMouse:
					zoomSpeed *= zoomSpeedKeyboard;
					break;

				// No default
			}
		}


		public void OnShowLeftPanel(InputAction.CallbackContext context)
		{
			PanelsTrigger(context.ReadValue<float>(), 0);
		}

		public void OnShowRightPanel(InputAction.CallbackContext context)
		{
			PanelsTrigger(context.ReadValue<float>(), 1);
		}


		public void SubmitEditor(InputAction.CallbackContext context)
		{
			if (isOnTest || editorIsBusy) {
				return;
			}

			bool isTrue = context.ReadValue<float>() > 0.5f;

			onSubmit = isTrue && userReady && context.action.phase == InputActionPhase.Performed;

			if (isTrue && userReady && raycastHits.Count > 0 && context.action.phase == InputActionPhase.Started) {
				if (IsHoverSelectedObject()) {
					dragOrigin = cursorToWorld;
				}

				hoverAnObject = true;
			}


			if (!isTrue) {
				if (onDrag) {

					onDrag = false;
					StoreSelectedObjectsInUndoRedoList();
					UpdateColliders();

				} else if (!wasEditing && userReady) {

					InGameObjectBehaviour tmp = FindNotSelectedHoveredObject();

					if (!IsHoverSelectedObject() && tmp != null) {
						DeselectAll();
						selected.Add(tmp);

						analysis.selectedNumber = 1;
						ScanSelectedObjects();
					}
				}

				wasEditing = false;
			}
		}

		public void DeleteEditor(InputAction.CallbackContext context)
		{
			if (isOnTest) {
				return;
			}

			onDelete = context.ReadValue<float>() > 0.5f && userReady && context.action.phase == InputActionPhase.Performed;
		}

		public void OnMultiselection(InputAction.CallbackContext context)
		{
			if (isOnTest) {
				return;
			}

			if (onMultiselection || (userReady && !userIsDragging)) {
				onMultiselection = context.ReadValue<float>() != 0f;

				if (onMultiselection) {
					msDrag.position = cursorToWorld;

					if (!wasMultiselecting) {
						wasMultiselecting = true;
					}
				}
			}
		}


		public void UndoAction(InputAction.CallbackContext context)
		{
			if (
				isOnTest ||
				editorIsBusy ||
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			Undo();
		}

		public void RedoAction(InputAction.CallbackContext context)
		{
			if (
				isOnTest ||
				editorIsBusy ||
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			Redo();
		}

		public void DeselectAllAction(InputAction.CallbackContext context)
		{
			if (
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			DeselectAll();
		}

		public void DeleteSelectedAction(InputAction.CallbackContext context)
		{
			if (
				isOnTest ||
				editorIsBusy ||
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			DeleteSelected();
		}

		public void SetSnap(bool value)
		{
			Globals.Editor.snap = value;
		}

		public void SetHandleLocally(bool value)
		{
			Globals.Editor.handleLocally = value;
		}


		public void SaveAction(InputAction.CallbackContext context)
		{
			if (
				isOnTest ||
				editorIsBusy ||
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			SaveLevel();
		}

		public void CopyAction(InputAction.CallbackContext context)
		{
			if (
				isOnTest ||
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			CopySelection();
		}

		public void PasteAction(InputAction.CallbackContext context)
		{
			if (
				isOnTest ||
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			PasteSelection();
		}

		public void DuplicateAction(InputAction.CallbackContext context)
		{
			if (
				isOnTest ||
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			DuplicateSelection();
		}

		public void CutAction(InputAction.CallbackContext context)
		{
			if (
				isOnTest ||
				Globals.onEditField ||
				context.action.phase != InputActionPhase.Started
			) {
				return;
			}

			CutSelection();
		}


		public void OnControlsChanged(PlayerInput input)
		{
			inputType = input.currentControlScheme switch {
				"Gamepad" => InputType.Gamepad,
				"Keyboard&Mouse" => InputType.KeyboardAndMouse,
				"Touch" => InputType.Touch,
				_ => InputType.KeyboardAndMouse,
			};


			Cursor.visible = inputType == InputType.KeyboardAndMouse;

			// if (inputType != InputType.Gamepad) {
			// 	panels[0].internals.nextPosition = panels[0].internals.enabled.ToInt();
			// 	panels[1].internals.nextPosition = panels[1].internals.enabled.ToInt();
			// }

			if (inputType != InputType.KeyboardAndMouse) {
				if (onDragNavigation) {
					onDragNavigation = false;
				}

				// panels[0].internals.deltaPosition = panels[1].internals.deltaPosition = 0f;
			}

			if (gamepadIndications[0].enabled != (inputType == InputType.Gamepad)) {
				foreach (Image image in gamepadIndications) {
					image.enabled = inputType == InputType.Gamepad;
				}
			}
		}
		#endregion

		#region Coroutines
		IEnumerator Inspector2Historial()
		{
			yield return new WaitForSeconds(0.5f);

			StoreSelectedObjectsInUndoRedoList();

			inspectorCoroutine = null;
			historialAvailable = true;
		}

		IEnumerator SaveLvlRoutine()
		{
			Globals.onLoad = true;
			Globals.loadProgress = 0f;
			Globals.loadingText = Globals.languages.GetField("loading.saving_level");
			editorIsBusy = true;

			Globals.levelData.objects.Clear();
			Globals.levelData.bounds = levelBounds.rect;

			int chunkIndex = 0;

			for (int i = 0; i < objCache.Count; i++) {
				if (!objCache[i]) {
					continue;
				}

				if (!objCache[i].gameObject.activeInHierarchy) {
					continue;
				}


				Globals.levelData.objects.Add(objCache[i].properties);
				chunkIndex++;

				if (chunkIndex >= Constants.chunkSize) {
					chunkIndex = 0;
					yield return new WaitForSeconds(0.1f);
				}

				Globals.loadProgress = (float) i / objCache.Count;
			}

			UniTask.Void(async () => {
				Globals.loadProgress = 0f;
				Globals.loadingText = Globals.languages.GetField("loading.almost_done");

				bool success = await LevelIO.Save(Globals.levelData);

				await UniTask.Delay(1000);

				saveCoroutine = null;

				Globals.onLoad = false;
				editorIsBusy = false;
			});

			yield return null;
		}

		IEnumerator LazyUpdateColliders()
		{
			yield return new WaitForSeconds(0.5f);

			PhysicsEventsHandler.GenerateCompositeGeometry();
			collidersCoroutine = null;
		}

		IEnumerator ResetTestObjects()
		{
			yield return new WaitForSeconds(1f);

			if (Globals.respawnAttempts > 0) {
				GameEventsHandler.InvokeBackToCheckpoint();
				Globals.isDead = false;
			}
		}
		#endregion

		struct AnalysisData
		{
			public int playersNumber;
			public int objectsNumber;
			public int selectedNumber;

			public enum Area {
				ALL = 0x7,

				PLAYERS = 0x1,
				OBJECTS = 0x2,
				SELECTED = 0x4,
			}
		}
	}
}
