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

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif


namespace RobotoSkunk.PixelMan.UI {
	[AddComponentMenu("UI/RobotoSkunk - Slider")]
	public class RSSlider : Slider {
		[MinAttribute(0f)] public float snapValue = 0f;
		public IntelliNav selectOnUp, selectOnDown, selectOnLeft, selectOnRight;

		public SliderEvent onEndValueChange = new();

		public override float value {
			get => (snapValue > 0f) ? Mathf.Round(base.value / snapValue) * snapValue : base.value;
			set => base.value = value;
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

	#if UNITY_EDITOR
	[CustomEditor(typeof(RSSlider), true)]
	[CanEditMultipleObjects]
	public class RSSliderEditor : SliderEditor {
		SerializedProperty m_selectOnUp, m_selectOnDown, m_selectOnLeft, m_selectOnRight, m_snapValue;

		protected override void OnEnable() {
			base.OnEnable();

			m_selectOnUp    = serializedObject.FindProperty("selectOnUp");
			m_selectOnDown  = serializedObject.FindProperty("selectOnDown");
			m_selectOnLeft  = serializedObject.FindProperty("selectOnLeft");
			m_selectOnRight = serializedObject.FindProperty("selectOnRight");
			m_snapValue     = serializedObject.FindProperty("snapValue");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();

			serializedObject.Update();


			EditorGUILayout.PropertyField(m_snapValue);
			EditorGUILayout.PropertyField(m_selectOnUp);
			EditorGUILayout.PropertyField(m_selectOnDown);
			EditorGUILayout.PropertyField(m_selectOnLeft);
			EditorGUILayout.PropertyField(m_selectOnRight);


			serializedObject.ApplyModifiedProperties();
		}
	}
	#endif
}
