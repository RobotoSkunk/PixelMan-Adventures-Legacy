using UnityEngine;
using UnityEngine.UI;

using UnityEditor;

using TMPro;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class LevelTemplate : CustomLevelButton {
		public TextMeshProUGUI nameText, idText, dateText;
		public Image syncImage;


		public string path, lvlName;
		public bool isSynced;
		public long date, id;
		public UserLevelsController controller;


		protected override void Start() {
			base.Start();
			if (!Application.isPlaying) return;

			nameText.text = lvlName;
			idText.text = id == 0 ? "" : '#' + id.ToString();
			dateText.text = RSTime.FromUnixTimestamp(date).ToString("yyyy/MM/dd HH:mm:ss");
			syncImage.gameObject.SetActive(isSynced);
		}

		public void OnClick() => controller.OpenLevel(path);
		public void Test() => Debug.Log("Test");
	}

	[CustomEditor(typeof(LevelTemplate))]
	[CanEditMultipleObjects]
	public class LevelTemplateEditor : CustomLevelButtonEditor {
		SerializedProperty m_nameText, m_idText, m_dateText, m_syncImage;

		protected override void OnEnable() {
			base.OnEnable();

			m_nameText = serializedObject.FindProperty("nameText");
			m_idText = serializedObject.FindProperty("idText");
			m_dateText = serializedObject.FindProperty("dateText");
			m_syncImage = serializedObject.FindProperty("syncImage");
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


			serializedObject.ApplyModifiedProperties();
		}
	}
}
