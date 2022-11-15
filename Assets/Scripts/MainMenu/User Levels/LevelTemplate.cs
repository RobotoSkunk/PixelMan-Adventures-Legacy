using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

using TMPro;


using RobotoSkunk.PixelMan.LevelEditor;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class LevelTemplate : CustomLevelButton {
		public TextMeshProUGUI nameText, idText, dateText;
		public Image syncImage;
		public RSButton deleteButton;

		public InternalUserScene info;
		public UserLevelsController controller;


		protected override void Start() {
			base.Start();
			if (!Application.isPlaying) return;

			nameText.text = info.data.name;
			idText.text = info.data.id == 0 ? "" : '#' + info.data.id.ToString();
			dateText.text = RSTime.FromUnixTimestamp(info.data.createdAt).ToString("yyyy/MM/dd HH:mm:ss");
			syncImage.gameObject.SetActive(info.data.cloudId != 0);
		}

		public void OnClick() => controller.OpenLevel(info);
		public void Test() => Debug.Log("Test");
	}

	[CustomEditor(typeof(LevelTemplate))]
	[CanEditMultipleObjects]
	public class LevelTemplateEditor : CustomLevelButtonEditor {
		SerializedProperty m_nameText, m_idText, m_dateText, m_syncImage, m_deleteButton;

		protected override void OnEnable() {
			base.OnEnable();

			m_nameText = serializedObject.FindProperty("nameText");
			m_idText = serializedObject.FindProperty("idText");
			m_dateText = serializedObject.FindProperty("dateText");
			m_syncImage = serializedObject.FindProperty("syncImage");
			m_deleteButton = serializedObject.FindProperty("deleteButton");
		}

		public override void OnInspectorGUI() {
			base.OnInspectorGUI();
			EditorGUILayout.Space();

			serializedObject.Update();


			EditorGUILayout.Separator();
			EditorGUILayout.PropertyField(m_nameText);
			EditorGUILayout.PropertyField(m_idText);
			EditorGUILayout.PropertyField(m_dateText);
			EditorGUILayout.PropertyField(m_syncImage);
			EditorGUILayout.PropertyField(m_deleteButton);


			serializedObject.ApplyModifiedProperties();
		}
	}
}
