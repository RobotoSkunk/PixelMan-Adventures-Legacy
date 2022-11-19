using Cysharp.Threading.Tasks;
using System;

using UnityEngine;

using RobotoSkunk.PixelMan.LevelEditor;
using RobotoSkunk.PixelMan.LevelEditor.IO;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserStageMenuController : MonoBehaviour {
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


		private void Awake() => menu.OnMenuChange += UpdateInfo;
		private void OnDestroy() => menu.OnMenuChange -= UpdateInfo;

		private void UpdateInfo() {
			if (Globals.mainMenuSection != menuIndex) return;
			InternalUserScene scene = Globals.Editor.currentScene;

			lvlName.text = scene.data.name;
			description.text = scene.data.description;
			LoadLevels();
		}
		public void LoadLevels() {
			InternalUserScene scene = Globals.Editor.currentScene;

			for (int i = 0; i < levelsParent.childCount; i++) Destroy(levelsParent.GetChild(i).gameObject);
			for (int i = 0; i < scene.data.levels.Count; i++) {
				StageLevel level = Instantiate(levelPrefab, levelsParent);
				level.data = scene.data.levels[i];
			}
		}

		public void SetName(string name) => Globals.Editor.currentScene.data.name = name;
		public void SetDescription(string description) => Globals.Editor.currentScene.data.description = description;

		public void Save() {
			UniTask.Void(async () => {
				InternalUserScene scene = Globals.Editor.currentScene;

				await LevelFileSystem.WriteMetadata(scene.file.FullName, scene.data);
				userLevelsController.ForceReload();
			});
		}
		public void CreateLevel(string name) {
			InternalUserScene scene = Globals.Editor.currentScene;
			if (scene.data.levels.Count >= 5) return;

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

			UniTask.Void(async () => {
				await LevelFileSystem.WriteLevel(scene.file.FullName, level, metadata.uuid);
				Save();
			});
		}

		public void TogglePopup(bool toggle) => popup.open = toggle;
		public void SetPopupIndex(int index) => popup.index = index;
	}
}
