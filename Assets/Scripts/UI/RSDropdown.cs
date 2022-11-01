using UnityEngine;
using UnityEngine.UI;

using UnityEditor;
using UnityEditor.UI;

using TMPro;

namespace RobotoSkunk.PixelMan.UI {
	[AddComponentMenu("UI/RobotoSkunk - Dropdown")]
	public class RSDropdown : TMP_Dropdown {
		public IntelliNav selectOnUp, selectOnDown, selectOnLeft, selectOnRight;

		public override Selectable FindSelectableOnUp() {
			if ((navigation.mode & Navigation.Mode.Vertical) == 0) return null;

			if (!selectOnUp.useAutomatic) return selectOnUp.selectable;

			return FindSelectable(transform.rotation * Vector3.up);
		}

		public override Selectable FindSelectableOnDown() {
			if ((navigation.mode & Navigation.Mode.Vertical) == 0) return null;

			if (!selectOnDown.useAutomatic) return selectOnDown.selectable;

			return FindSelectable(transform.rotation * Vector3.down);
		}

		public override Selectable FindSelectableOnLeft() {
			if ((navigation.mode & Navigation.Mode.Horizontal) == 0) return null;

			if (!selectOnLeft.useAutomatic) return selectOnLeft.selectable;

			return FindSelectable(transform.rotation * Vector3.left);
		}

		public override Selectable FindSelectableOnRight() {
			if ((navigation.mode & Navigation.Mode.Horizontal) == 0) return null;

			if (!selectOnRight.useAutomatic) return selectOnRight.selectable;

			return FindSelectable(transform.rotation * Vector3.right);
		}
	}

	[CustomEditor(typeof(RSDropdown), true)]
	[CanEditMultipleObjects]
	public class RSDropdownEditor : TMPro.EditorUtilities.DropdownEditor {
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