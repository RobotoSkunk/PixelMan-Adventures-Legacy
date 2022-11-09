using UnityEngine;
using UnityEditor;

using TMPro;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class FolderTemplate : CustomLevelButton {
		public TextMeshProUGUI nameText;

		public string path, folderName;


		protected override void Start() {
			base.Start();
			if (!Application.isPlaying) return;

			nameText.text = folderName;
		}
	}

	[CustomEditor(typeof(FolderTemplate))]
	[CanEditMultipleObjects]
	public class FolderTemplateEditor : CustomLevelButtonEditor {
		SerializedProperty m_nameText;

		protected override void OnEnable() {
			base.OnEnable();

			m_nameText = serializedObject.FindProperty("nameText");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();

			serializedObject.Update();


			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_nameText);


			serializedObject.ApplyModifiedProperties();
		}
	}
}
