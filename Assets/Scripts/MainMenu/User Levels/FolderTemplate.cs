using System.IO;

using UnityEngine;
using UnityEditor;

using TMPro;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class FolderTemplate : CustomLevelButton {
		public TextMeshProUGUI nameText;
		public RSButton deleteButton;

		public DirectoryInfo info;


		protected override void Start() {
			base.Start();
			if (!Application.isPlaying) return;

			nameText.text = info.Name;
		}
	}

	[CustomEditor(typeof(FolderTemplate))]
	[CanEditMultipleObjects]
	public class FolderTemplateEditor : CustomLevelButtonEditor {
		SerializedProperty m_nameText, m_deleteButton;

		protected override void OnEnable() {
			base.OnEnable();

			m_nameText = serializedObject.FindProperty("nameText");
			m_deleteButton = serializedObject.FindProperty("deleteButton");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();

			serializedObject.Update();


			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_nameText);
			EditorGUILayout.PropertyField(m_deleteButton);


			serializedObject.ApplyModifiedProperties();
		}
	}
}
