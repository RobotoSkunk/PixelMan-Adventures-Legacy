
using UnityEngine;
using UnityEngine.UI;
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

		[Header("Configuration")]
		public bool isDraggable = true;
		public float sensitivity = 10f;
		public float holdTime = 0.25f;

		readonly float hangDistance = 3f;


		float hangTime, delta;
		bool isHanging;
		Vector2 deltaPos;
		RectTransform scrollRectRT;


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

			scrollRectRT = scrollRect.GetComponent<RectTransform>();
		}

		private void Update() {
			if (!Application.isPlaying) return;
			if (!isDraggable) return;

			if (isHanging) {
				hangTime += Time.deltaTime;
			} else hangTime = 0f;



			delta = Mathf.Lerp(delta, (hangTime > holdTime).ToInt(), 0.2f * RSTime.delta);
			shadow.effectDistance = hangDistance * delta * new Vector2(1f, -1f);
			content.localPosition = hangDistance * delta * new Vector2(-1f, 1f);

			if (hangTime < holdTime) preview.localPosition = Vector3.Lerp(preview.localPosition, Vector3.zero, 0.2f * RSTime.delta);
			else {
				interactable = false;

				float scrollTo =
					Mathf.Clamp01((preview.position.y - scrollRectRT.rect.height * 2f) / (0.25f * scrollRectRT.rect.height))
					-
					(1f - Mathf.Clamp01(preview.position.y / (0.25f * scrollRectRT.rect.height)));

				scrollRect.verticalScrollbar.value += 2f * scrollTo * Time.deltaTime;
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
			} else {
				scrollRect.OnDrag(ev);
				isHanging = false;
				interactable = false;
			}
		}

		public override void OnPointerUp(PointerEventData ev) {
			base.OnPointerUp(ev);
			scrollRect.OnEndDrag(ev);
			if (!isDraggable) return;

			isHanging = false;
			preview.SetParent(transform);
			hangTime = 0f;

			interactable = true;
		}
	}



	[CustomEditor(typeof(CustomLevelButton), true)]
	[CanEditMultipleObjects]
	public class CustomLevelButtonEditor : RSButtonEditor {
		SerializedProperty m_Preview, m_Content, m_ScrollRect, m_HoveringParent, m_Shadow, m_IsDraggable, m_Sensitivity, m_HoldTime;

		protected override void OnEnable() {
			base.OnEnable();

			m_Preview = serializedObject.FindProperty("preview");
			m_Content = serializedObject.FindProperty("content");
			m_ScrollRect = serializedObject.FindProperty("scrollRect");
			m_HoveringParent = serializedObject.FindProperty("hoveringParent");
			m_Shadow = serializedObject.FindProperty("shadow");
			m_IsDraggable = serializedObject.FindProperty("isDraggable");
			m_Sensitivity = serializedObject.FindProperty("sensitivity");
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

			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_IsDraggable);
			EditorGUILayout.PropertyField(m_Sensitivity);
			EditorGUILayout.PropertyField(m_HoldTime);


			serializedObject.ApplyModifiedProperties();
		}
	}
}
