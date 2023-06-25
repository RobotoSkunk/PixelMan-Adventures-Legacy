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

using System;
using System.IO;
using System.Collections.Generic;

using Cysharp.Threading.Tasks;

using UnityEngine;

using RobotoSkunk.PixelMan.LevelEditor;
using RobotoSkunk.PixelMan.LevelEditor.IO;


namespace RobotoSkunk.PixelMan.UI.MainMenu
{
	public class UserStageMenuController : MonoBehaviour
	{
		[Header("Model")]
		public StageLevel levelPrefab;

		[Header("Menu stuff")]
		public MenuController menu;
		public int menuIndex;
		public UserLevelsController userLevelsController;
		public Popup popup;

		[Header("UI")]
		public RSInputField lvlName;
		public RSInputField description;
		public Transform levelsParent;

		bool isBusy;


		private void Awake() => menu.OnMenuChange += UpdateInfo;
		private void OnDestroy() => menu.OnMenuChange -= UpdateInfo;

		private void UpdateInfo()
		{
			if (Globals.mainMenuSection != menuIndex) {
				return;
			}
			InternalUserScene scene = Globals.Editor.currentScene;

			lvlName.SetTextWithoutNotify(scene.data.name);
			description.SetTextWithoutNotify(scene.data.description);
			LoadLevels();
		}

		public void LoadLevels()
		{
			InternalUserScene scene = Globals.Editor.currentScene;


			for (int i = 0; i < levelsParent.childCount; i++) {
				Destroy(levelsParent.GetChild(i).gameObject);
			}

			for (int i = 0; i < scene.data.levels.Count; i++) {
				StageLevel level = Instantiate(levelPrefab, levelsParent);
				level.data = scene.data.levels[i];
				level.controller = this;
			}
		}

		public void SetName(string name)
		{
			string filteredName = LevelFileSystem.FilterLevelName(name);

			Globals.Editor.currentScene.data.name = filteredName;
			lvlName.SetTextWithoutNotify(filteredName);
		}

		public void SetDescription(string description)
		{
			if (description.Length > 256) {
				description = description[..256];
			}

			Globals.Editor.currentScene.data.description = description;
			this.description.SetTextWithoutNotify(description);

			Debug.Log(Globals.Editor.currentScene.data);
		}

		public void SetLevelName(string uuid, string name)
		{
			InternalUserScene scene = Globals.Editor.currentScene;
			List<Level.UserMetadata> levels = scene.data.levels;

			for (int i = 0; i < scene.data.levels.Count; i++) {
				if (levels[i].uuid == uuid) {
					Level.UserMetadata level = levels[i];
					level.name = name;

					levels[i] = level;
					break;
				}
			}

			Save();
		}

		public void DeleteLevel()
		{
			UniTask.Void(async () =>
			{
				InternalUserScene scene = Globals.Editor.currentScene;
				List<Level.UserMetadata> levels = scene.data.levels;

				for (int i = 0; i < scene.data.levels.Count; i++) {
					if (levels[i].uuid == Globals.Editor.currentLevel.uuid) {
						levels.RemoveAt(i);
						break;
					}
				}

				await SaveAsync();
				LoadLevels();
				Files.DeleteFileFromZip(scene.file.FullName, Globals.Editor.currentLevel.uuid + ".json");
			});
		}

		public void MoveLevelUp()
		{
			if (isBusy) {
				return;
			}

			UniTask.Void(async () =>
			{
				InternalUserScene scene = Globals.Editor.currentScene;
				List<Level.UserMetadata> levels = scene.data.levels;

				for (int i = 0; i < scene.data.levels.Count; i++) {
					if (levels[i].uuid == Globals.Editor.currentLevel.uuid) {
						if (i == 0) return;

						(levels[i - 1], levels[i]) = (levels[i], levels[i - 1]);
						break;
					}
				}

				await SaveAsync();
				LoadLevels();
			});
		}

		public void MoveLevelDown()
		{
			if (isBusy) {
				return;
			}

			UniTask.Void(async () =>
			{
				InternalUserScene scene = Globals.Editor.currentScene;
				List<Level.UserMetadata> levels = scene.data.levels;

				for (int i = 0; i < scene.data.levels.Count; i++) {
					if (levels[i].uuid == Globals.Editor.currentLevel.uuid) {
						if (i == levels.Count - 1) return;

						(levels[i + 1], levels[i]) = (levels[i], levels[i + 1]);
						break;
					}
				}

				await SaveAsync();
				LoadLevels();
			});
		}

		public void DeleteStage()
		{
			File.Delete(Globals.Editor.currentScene.file.FullName);

			userLevelsController.ForceReload();
		}


		public async UniTask SaveAsync()
		{
			if (isBusy) {
				return;
			}
			isBusy = true;

			InternalUserScene scene = Globals.Editor.currentScene;

			await LevelFileSystem.WriteMetadata(scene.file.FullName, scene.data);
			isBusy = false;
		}

		public void Save()
		{
			UniTask.Void(async () => await SaveAsync());
		}


		public void SaveAndReload()
		{
			UniTask.Void(async () =>
			{
				await SaveAsync();
				userLevelsController.ForceReload();
			});
		}

		public void CreateLevel(string name)
		{
			InternalUserScene scene = Globals.Editor.currentScene;

			if (scene.data.levels.Count >= 5) {
				return;
			}

			name = LevelFileSystem.FilterLevelName(name);


			Level.UserMetadata metadata = new() {
				uuid = LevelIO.GenerateUUID(),
				name = name,
				createdAt = RSTime.ToUnixTimestamp(DateTime.Now),
				timeSpent = 0
			};

			Level level = new() {
				objects = new(),
				size = new(50, 30)
			};

			scene.data.levels.Add(metadata);
			LoadLevels();

			UniTask.Void(async () =>
			{
				await LevelFileSystem.WriteLevel(scene.file.FullName, level, metadata.uuid);
				await SaveAsync();
			});
		}

		public void TogglePopup(bool toggle) => popup.open = toggle;
		public void SetPopupIndex(int index) => popup.index = index;
	}
}
