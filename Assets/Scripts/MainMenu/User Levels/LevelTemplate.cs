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

using System.IO;

using Cysharp.Threading.Tasks;

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

		protected override void OnDragEnd(CustomLevelButton target) {
			UniTask.Void(async () => {
				FolderTemplate folder = target.GetComponent<FolderTemplate>();
				FolderGoBack goBack = target.GetComponent<FolderGoBack>();
				if (!folder && !goBack) return;

				controller.isBusy = true;

				await UniTask.RunOnThreadPool(() => {
					DirectoryInfo targetDir = folder ? folder.info : goBack.info;
					info.file.MoveTo(Path.Combine(targetDir.FullName, info.file.Name));

				});

				controller.isBusy = false;
				Destroy(gameObject);
			});
		}
	}

	#if UNITY_EDITOR
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
	#endif
}
