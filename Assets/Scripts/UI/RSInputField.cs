using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using UnityEditor;
using UnityEditor.UI;

using TMPro;
using TMPro.EditorUtilities;

namespace RobotoSkunk.PixelMan.UI {
	[AddComponentMenu("UI/RobotoSkunk - Input Field")]
	public class RSInputField : TMP_InputField {
		public IntelliNav selectOnUp, selectOnDown, selectOnLeft, selectOnRight;

		protected override void Awake() {
			base.Awake();

			onSelect.AddListener((value) => Globals.onEditField = true);
			onEndEdit.AddListener((value) => Globals.onEditField = false);
			onDeselect.AddListener((value) => Globals.onEditField = false);
		}

		public override Selectable FindSelectableOnUp() {
			if ((navigation.mode & Navigation.Mode.Vertical) == 0) return null;

			if (!selectOnUp.useAutomatic) return selectOnUp.selectable;

			return FindSelectable(Quaternion.Euler(transform.eulerAngles + selectOnUp.addRotation) * Vector3.up);
		}

		public override Selectable FindSelectableOnDown() {
			if ((navigation.mode & Navigation.Mode.Vertical) == 0) return null;

			if (!selectOnDown.useAutomatic) return selectOnDown.selectable;

			return FindSelectable(Quaternion.Euler(transform.eulerAngles + selectOnDown.addRotation) * Vector3.down);
		}

		public override Selectable FindSelectableOnLeft() {
			if ((navigation.mode & Navigation.Mode.Horizontal) == 0) return null;

			if (!selectOnLeft.useAutomatic) return selectOnLeft.selectable;

			return FindSelectable(Quaternion.Euler(transform.eulerAngles + selectOnLeft.addRotation) * Vector3.left);
		}

		public override Selectable FindSelectableOnRight() {
			if ((navigation.mode & Navigation.Mode.Horizontal) == 0) return null;

			if (!selectOnRight.useAutomatic) return selectOnRight.selectable;

			return FindSelectable(Quaternion.Euler(transform.eulerAngles + selectOnRight.addRotation) * Vector3.right);
		}
	}

	[CustomEditor(typeof(RSInputField), true)]
	[CanEditMultipleObjects]
	public class RSInputFieldEditor : TMP_InputFieldEditor {
		SerializedProperty m_selectOnUp, m_selectOnDown, m_selectOnLeft, m_selectOnRight;

		protected override void OnEnable() {
			base.OnEnable();

			m_selectOnUp    = serializedObject.FindProperty("selectOnUp");
			m_selectOnDown  = serializedObject.FindProperty("selectOnDown");
			m_selectOnLeft  = serializedObject.FindProperty("selectOnLeft");
			m_selectOnRight = serializedObject.FindProperty("selectOnRight");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();

			serializedObject.Update();


			EditorGUILayout.PropertyField(m_selectOnUp);
			EditorGUILayout.PropertyField(m_selectOnDown);
			EditorGUILayout.PropertyField(m_selectOnLeft);
			EditorGUILayout.PropertyField(m_selectOnRight);


			serializedObject.ApplyModifiedProperties();
		}
	}
}
