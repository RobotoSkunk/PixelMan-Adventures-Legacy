using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;

using UnityEngine;
using RobotoSkunk.PixelMan.LevelEditor;
using RobotoSkunk.PixelMan.LevelEditor.IO;



namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserLevelsController : MonoBehaviour {
		[Header("References")]
		public LevelTemplate levelTemplate;
		public FolderTemplate folderTemplate;
		public FolderGoBack folderGoBack;

		[Header("Components")]
		public RectTransform content;
		public Transform hoverParent;

		[Header("Important")]
		public MenuController menuController;
		public int activeMenu;


		readonly List<FileInfo> files = new();
		readonly List<DirectoryInfo> folders = new();
		readonly System.Func<FileInfo, bool> fileFilter = delegate(FileInfo file) { return file.Extension == ".pml"; };

		DirectoryInfo root;


		private void Start() {
			root = new(Files.Directories.User.levels);

			LoadPath(root.FullName);
		}

		public void LoadPath(string path) {
			UniTask.Void(async () => {
				DirectoryInfo dir = new(path);
				foreach (Transform child in content) Destroy(child.gameObject);

				await Files.GetFilesAndDirectories(path, files, folders, fileFilter);

				try {
					if (Files.CheckIfDirectoryIsChildOf(root, dir)) {
						var go = Instantiate(folderGoBack, content);
						go.path = dir.Parent.FullName;

						go.onClick.AddListener(() => {
							LoadPath(go.path);
						});
					}
				} catch (System.Exception e) { Debug.LogError(e); }


				foreach (DirectoryInfo folder in folders) {
					try {
						FolderTemplate _tmp = Instantiate(folderTemplate, content);
						_tmp.folderName = folder.Name;
						_tmp.path = folder.FullName;
						_tmp.hoveringParent = hoverParent;

						_tmp.onClick.AddListener(() => {
							LoadPath(folder.FullName);
						});

					} catch (System.Exception e) { Debug.LogError(e); }
				}

				foreach (FileInfo file in files) {
					try {
						UserScene scene = await LevelFileSystem.GetMetadata(file.FullName);

						LevelTemplate _tmp = Instantiate(levelTemplate, content);
						_tmp.path = file.FullName;
						_tmp.lvlName = scene.name;
						_tmp.date = scene.createdAt;
						_tmp.controller = this;
						_tmp.hoveringParent = hoverParent;

						// _tmp.lvlName = file.Name;
						// _tmp.path = file.FullName;
						// _tmp.id = Random.Range(0, 1000);
						// _tmp.date = file.LastWriteTimeUtc.ToFileTimeUtc();
						// _tmp.isSynced = false;
						// _tmp.controller = this;
						// _tmp.hoveringParent = hoverParent;

					} catch (System.Exception e) { Debug.LogError(e); }
				}
			});
		}


		public void OpenLevel(string path) {
			Globals.levelPath = path;
			menuController.SetMenu(1);
		}
	}
}
