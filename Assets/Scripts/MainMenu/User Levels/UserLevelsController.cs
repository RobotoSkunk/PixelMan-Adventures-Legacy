using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;

using UnityEngine;

namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserLevelsController : MonoBehaviour {
		public LevelTemplate levelTemplate;
		public FolderTemplate folderTemplate;
		public RectTransform content;

		[Header("Important")]
		public MenuController menuController;
		public int activeMenu;


		readonly List<FileInfo> files = new();
		readonly List<DirectoryInfo> folders = new();
		readonly System.Func<FileInfo, bool> fileFilter = delegate(FileInfo file) { return file.Extension == ".pml"; };



		private void Start() {
			LoadPath(Files.Directories.User.levels);
		}

		public void LoadPath(string path) {

			UniTask.Void(async () => {
				Debug.Log("LoadPath: " + path);

				await Files.GetFilesAndDirectories(path, files, folders, fileFilter);

				foreach (DirectoryInfo folder in folders) {
					Debug.Log("Folder: " + folder.Name);
				}

				foreach (FileInfo file in files) {
					Debug.Log("File: " + file.Name);
				}
			});
		}


		public void OpenLevel(string path) {
			Globals.levelPath = path;
			menuController.SetMenu(1);
		}
	}
}
