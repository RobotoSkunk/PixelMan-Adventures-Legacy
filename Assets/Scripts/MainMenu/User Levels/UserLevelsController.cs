using UnityEngine;

namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserLevelsController : MonoBehaviour {
		public LevelTemplate levelTemplate;
		public FolderTemplate folderTemplate;
		public RectTransform content;

		[Header("Important")]
		public MenuController menuController;
		public int activeMenu;


		public void OpenLevel(string path) {
			Globals.levelPath = path;
			menuController.SetMenu(1);
		}
	}
}
