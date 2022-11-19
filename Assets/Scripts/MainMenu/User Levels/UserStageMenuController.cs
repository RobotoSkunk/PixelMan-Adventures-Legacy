using Cysharp.Threading.Tasks;

using UnityEngine;

using RobotoSkunk.PixelMan.LevelEditor;
using RobotoSkunk.PixelMan.LevelEditor.IO;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserStageMenuController : MonoBehaviour {
		[Header("Menu stuff")]
		public MenuController menu;
		public int menuIndex;
		public UserLevelsController userLevelsController;

		[Header("Model")]
		public StageLevel levelPrefab;

		[Header("UI")]
		public RSInputField lvlName;
		public RSInputField description;
		public Transform levelsParent;


		private void Awake() => menu.OnMenuChange += UpdateInfo;

		private void UpdateInfo() {
			if (Globals.mainMenuSection != menuIndex) return;
			InternalUserScene scene = Globals.Editor.currentScene;

			lvlName.text = scene.data.name;
			description.text = scene.data.description;

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
	}
}
