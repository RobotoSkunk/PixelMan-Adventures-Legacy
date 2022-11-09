using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;

using UnityEngine;

namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserLevelsController : MonoBehaviour {
		public LevelTemplate levelTemplate;
		public FolderTemplate folderTemplate;
		public RectTransform content;
		public Transform hoverParent;

		[Header("Important")]
		public MenuController menuController;
		public int activeMenu;


		readonly List<FileInfo> files = new();
		readonly List<DirectoryInfo> folders = new();
		readonly System.Func<FileInfo, bool> fileFilter = delegate(FileInfo file) { return file.Extension == ".pml"; };

		string root;


		private void Start() {
			root = Files.Directories.User.levels;

			LoadPath(root);
		}

		public void LoadPath(string path) {
			UniTask.Void(async () => {
				foreach (Transform child in content) Destroy(child.gameObject);

				await Files.GetFilesAndDirectories(path, files, folders, fileFilter);


				foreach (DirectoryInfo folder in folders) {
					FolderTemplate _tmp = Instantiate(folderTemplate, content);
					_tmp.folderName = folder.Name;
					_tmp.path = folder.FullName;
					_tmp.hoveringParent = hoverParent;

					_tmp.onClick.AddListener(() => {
						LoadPath(folder.FullName);
					});
				}

				foreach (FileInfo file in files) {
					LevelTemplate _tmp = Instantiate(levelTemplate, content);
					_tmp.lvlName = file.Name;
					_tmp.path = file.FullName;
					_tmp.id = Random.Range(0, 1000);
					_tmp.date = file.LastWriteTimeUtc.ToFileTimeUtc();
					_tmp.isSynced = false;
					_tmp.controller = this;
					_tmp.hoveringParent = hoverParent;
				}
			});
		}


		public void OpenLevel(string path) {
			Globals.levelPath = path;
			menuController.SetMenu(1);
		}
	}
}
