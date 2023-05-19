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
using UnityEditor;

using TMPro;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class FolderTemplate : CustomLevelButton {
		public TextMeshProUGUI nameText;
		public RSButton deleteButton;

		public DirectoryInfo info;
		public UserLevelsController controller;


		protected override void Start() {
			base.Start();
			if (!Application.isPlaying) return;

			nameText.text = info.Name;
		}

		protected override void OnDragEnd(CustomLevelButton target) {
			UniTask.Void(async () => {
				FolderTemplate folder = target.GetComponent<FolderTemplate>();
				FolderGoBack goBack = target.GetComponent<FolderGoBack>();
				if (!folder && !goBack) return;

				DirectoryInfo targetDir = folder ? folder.info : goBack.info;
				if (!Directory.Exists(targetDir.FullName)) return;

				controller.isBusy = true;
				bool success = true;

				await UniTask.RunOnThreadPool(() => {
					DirectoryInfo[] dirs = targetDir.GetDirectories(info.Name);

					if (dirs.Length > 0) {
						foreach (DirectoryInfo dir in dirs) {
							if (dir.Name == info.Name) {
								success = false;
								break;
							}
						}
					}

					if (success) info.MoveTo(Path.Combine(targetDir.FullName, info.Name));
				});

				controller.isBusy = false;
				if (success) Destroy(gameObject);
				else {
					controller.popup.index = 4;
					controller.popup.open = true;
				}
			});
		}
	}

	#if UNITY_EDITOR
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
	#endif
}
