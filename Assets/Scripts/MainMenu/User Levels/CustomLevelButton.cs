using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityEditor;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	[AddComponentMenu("UI/RobotoSkunk/PixelMan - Custom Level Button")]
	public class CustomLevelButton : RSButton, IDragHandler, IPointerDownHandler, IPointerUpHandler {
		[Header("Layout stuff")]
		public RectTransform preview;
		public RectTransform content;
		public ScrollRect scrollRect;
		public Transform hoveringParent;
		public Shadow shadow;
		public Image hoveringIndicator;

		[Header("Configuration")]
		public bool isDraggable = true;
		public bool canHover = true;
		public float holdTime = 0.25f;


		public bool isHovering;
		readonly float hangDistance = 3f;
		readonly List<RaycastResult> results = new();


		float hangTime, delta;
		bool isHanging;
		Vector2 deltaPos;
		RectTransform scrollRectRT;
		GraphicRaycaster raycaster;


		float hangDist {
			get {
				return hangDistance * delta;
			}
		}

		protected override void Awake() {
			base.Awake();

			if (!Application.isPlaying) return;
			if (!scrollRect) scrollRect = GetComponentInParent<ScrollRect>();
			if (!shadow) shadow = GetComponentInChildren<Shadow>();
			if (!raycaster) raycaster = GetComponentInParent<GraphicRaycaster>();

			scrollRectRT = scrollRect.GetComponent<RectTransform>();
			if (!canHover) hoveringIndicator.color = Color.clear;
		}

		private void Update() {
			if (!Application.isPlaying) return;

			if (canHover) hoveringIndicator.color = isHovering ? new Color(1f, 1f, 1f, 0.5f) : Color.clear;
			if (!isDraggable) return;

			if (isHanging) hangTime += Time.deltaTime;
			else hangTime = 0f;



			delta = Mathf.Lerp(delta, (hangTime > holdTime).ToInt(), 0.2f * RSTime.delta);
			shadow.effectDistance = hangDistance * delta * new Vector2(1f, -1f);
			content.localPosition = hangDistance * delta * new Vector2(-1f, 1f);

			if (hangTime < holdTime) preview.localPosition = Vector3.Lerp(preview.localPosition, Vector3.zero, 0.2f * RSTime.delta);
			else {
				interactable = false;


				if (preview.parent != transform) {
					float scrollTo =
						Mathf.Clamp01((preview.localPosition.y - scrollRectRT.rect.height * 0.75f) / (0.3f * scrollRectRT.rect.height))
						-
						(1f - Mathf.Clamp01(preview.localPosition.y / (0.3f * scrollRectRT.rect.height)));

					scrollRect.verticalScrollbar.value += 2f * scrollTo * Time.deltaTime;
				}
			}
		}


		public override void OnPointerDown(PointerEventData ev) {
			base.OnPointerDown(ev);
			scrollRect.OnBeginDrag(ev);
			if (!isDraggable) return;

			isHanging = true;
			deltaPos = (Vector2)preview.position - ev.position;
		}
		public void OnDrag(PointerEventData ev) {
			if (hangTime > holdTime) {
				preview.position = ev.position + deltaPos;
				preview.SetParent(hoveringParent);


				SetHoveringState(results, false);
				results.Clear();
				ev.position = preview.position;
				raycaster.Raycast(ev, results);
				SetHoveringState(results, true);
			} else {
				scrollRect.OnDrag(ev);
				isHanging = false;
				interactable = false;
			}
		}

		public override void OnPointerUp(PointerEventData ev) {
			base.OnPointerUp(ev);
			scrollRect.OnEndDrag(ev);
			interactable = true;

			if (!isDraggable) return;
			if (hangTime >= holdTime && results.Count > 0) {

				for (int i = 0; i < results.Count; i++) {
					if (!results[i].gameObject) continue;
					if (results[i].gameObject == gameObject) continue;

					CustomLevelButton button = results[i].gameObject.GetComponent<CustomLevelButton>();
					if (button) {
						if (!button.canHover) continue;

						OnDragEnd(button);
						break;
					}
				}
			}

			isHanging = false;
			preview.SetParent(transform);
			hangTime = 0f;

			SetHoveringState(results, false);
		}

		protected virtual void OnDragEnd(CustomLevelButton button) { }


		void SetHoveringState(List<RaycastResult> results, bool toggle) {
			if (results.Count > 0) {
				for (int i = 0; i < results.Count; i++) {
					if (!results[i].gameObject) continue;
					if (results[i].gameObject == gameObject) continue;

					CustomLevelButton button = results[i].gameObject.GetComponent<CustomLevelButton>();
					if (button) button.isHovering = toggle;
				}
			}
		}
	}



	[CustomEditor(typeof(CustomLevelButton), true)]
	[CanEditMultipleObjects]
	public class CustomLevelButtonEditor : RSButtonEditor {
		SerializedProperty m_Preview, m_Content, m_ScrollRect, m_HoveringParent, m_Shadow, m_hoveringIndicator, m_IsDraggable, m_CanHover, m_HoldTime;

		protected override void OnEnable() {
			base.OnEnable();

			m_Preview = serializedObject.FindProperty("preview");
			m_Content = serializedObject.FindProperty("content");
			m_ScrollRect = serializedObject.FindProperty("scrollRect");
			m_HoveringParent = serializedObject.FindProperty("hoveringParent");
			m_Shadow = serializedObject.FindProperty("shadow");
			m_hoveringIndicator = serializedObject.FindProperty("hoveringIndicator");

			m_IsDraggable = serializedObject.FindProperty("isDraggable");
			m_CanHover = serializedObject.FindProperty("canHover");
			m_HoldTime = serializedObject.FindProperty("holdTime");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();

			serializedObject.Update();


			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_Preview);
			EditorGUILayout.PropertyField(m_Content);
			EditorGUILayout.PropertyField(m_ScrollRect);
			EditorGUILayout.PropertyField(m_HoveringParent);
			EditorGUILayout.PropertyField(m_Shadow);
			EditorGUILayout.PropertyField(m_hoveringIndicator);

			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_IsDraggable);
			EditorGUILayout.PropertyField(m_CanHover);
			EditorGUILayout.PropertyField(m_HoldTime);


			serializedObject.ApplyModifiedProperties();
		}
	}
}
