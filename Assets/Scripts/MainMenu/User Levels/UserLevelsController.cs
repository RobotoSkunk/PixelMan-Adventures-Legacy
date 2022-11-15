using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.IO.Compression;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using RobotoSkunk.PixelMan.LevelEditor;
using RobotoSkunk.PixelMan.LevelEditor.IO;



namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserLevelsController : MonoBehaviour {
		[Header("References")]
		public LevelTemplate levelTemplate;
		public FolderTemplate folderTemplate;
		public FolderGoBack folderGoBack;

		[Header("Properties")]
		public float loadingBarSpeed = 200f;

		[Header("Components")]
		public RectTransform content;
		public Transform hoverParent;

		[Header("UI")]
		public GameObject loadingIndicator;
		public RectTransform loadingBar;
		public GameObject levelCreator, folderCreator;
		public Image levelInput, folderInput;
		public RSButton levelButton, folderButton;

		[Header("Important")]
		public MenuController menuController;
		public int activeMenu;


		readonly List<FileInfo> files = new();
		readonly List<DirectoryInfo> folders = new();
		readonly System.Func<FileInfo, bool> fileFilter = delegate(FileInfo file) { return file.Extension == ".pml"; };

		DirectoryInfo root, current;
		List<InternalUserScene> scenes = new();
		SortType sortBy = SortType.Name;
		float waitDelta = 0f;

		public enum SortType { Name, Date, Edited, Size }


		private void Start() {
			root = new(Files.Directories.User.levels);
			ToggleCreateLevel(false);
			ToggleCreateFolder(false);
			loadingIndicator.SetActive(false);

			LoadPath(root.FullName);
		}

		private void Update() {
			waitDelta += Time.deltaTime * loadingBarSpeed;
			if (waitDelta >= 360f) waitDelta = 0f;

			float delta = Mathf.Sin(waitDelta * Mathf.Deg2Rad);
			loadingBar.anchoredPosition = new(delta * loadingBar.rect.size.x, 0);
		}


		public void LoadPath(string path) {
			UniTask.Void(async () => {
				loadingIndicator.SetActive(true);

				#region Get and sort info
				DirectoryInfo dir = new(path);
				foreach (Transform child in content) Destroy(child.gameObject);


				if (current != dir) {
					await Files.GetFilesAndDirectories(path, files, folders, fileFilter);
					scenes.Clear();

					foreach (FileInfo file in files) {
						try {
							UserScene scene = await LevelFileSystem.GetMetadata(file.FullName);
							InternalUserScene data = new() {
								path = file.FullName,
								data = scene,
								size = file.Length
							};

							scenes.Add(data);
						} catch (System.Exception e) { Debug.LogError(e); }
					}

					current = dir;
				}

				switch (sortBy) {
					case SortType.Date:
						scenes.Sort((a, b) => a.data.createdAt.CompareTo(b.data.createdAt));
						break;
					case SortType.Edited:
						scenes.Sort((a, b) => a.data.lastModified.CompareTo(b.data.lastModified));
						break;
					case SortType.Size:
						scenes.Sort((a, b) => a.size.CompareTo(b.size));
						break;
					case SortType.Name:
					default:
						scenes.Sort((a, b) => a.data.name.CompareTo(b.data.name));
						break;
				}
				#endregion

				loadingIndicator.SetActive(false);


				#region Create UI
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

				foreach(InternalUserScene scene in scenes) {
					try {
						LevelTemplate _tmp = Instantiate(levelTemplate, content);
						_tmp.info = scene;
						_tmp.controller = this;
						_tmp.hoveringParent = hoverParent;

					} catch (System.Exception e) { Debug.LogError(e); }
				}
				#endregion
			});
		}
		public void ForceLoadPath(string path) {
			current = null;
			LoadPath(path);
		}


		public void OpenLevel(InternalUserScene scene) {
			Globals.Editor.currentScene = scene;
			menuController.SetMenu(1);
		}

		public void CreateLevel(string name) {
			UniTask.Void(async () => {
				name = name.Trim();
				if (name.Length == 0 || name.Length > 32) return;

				UserScene scene = new() {
					createdAt = RSTime.ToUnixTimestamp(System.DateTime.Now),
					name = name
				};

				string data = await AsyncJson.ToJson(scene);
				string path = Path.Combine(current.FullName, LevelIO.GenerateUUID() + ".pml");

				FileStream stream = File.Create(path);
				ZipArchive archive = new(stream, ZipArchiveMode.Create, true);

				ZipArchiveEntry entry = archive.CreateEntry("metadata.json");

				StreamWriter writer = new(entry.Open());
				writer.Write(data);
				writer.Close();

				archive.Dispose();
				stream.Close();

				ForceLoadPath(current.FullName);
			});
		}
		public void CreateFolder(string name) {
			string[] invalidChars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
			name = name.Trim();

			if (name.Length == 0 || name.Length > 200) return;
			foreach (string invalidChar in invalidChars)
				if (name.Contains(invalidChar)) return;


			Directory.CreateDirectory(Path.Combine(current.FullName, name));
			ForceLoadPath(current.FullName);
		}
		public void DeleteFolder(string path) {
			Directory.Delete(path, true);
			ForceLoadPath(current.FullName);
		}
		public void DeleteLevel(string path) {
			File.Delete(path);
			ForceLoadPath(current.FullName);
		}



		public void CheckFolderName(string name) {
			string[] invalidChars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
			name = name.Trim();

			if (name.Length == 0 || name.Length > 200) {
				folderButton.interactable = false;
				folderInput.color = Color.red;
				return;
			}

			foreach (string invalidChar in invalidChars)
				if (name.Contains(invalidChar)) {
					folderButton.interactable = false;
					folderInput.color = Color.red;
					return;
				}

			folderButton.interactable = true;
			folderInput.color = Color.white;
		}
		public void CheckLevelName(string name) {
			name = name.Trim();

			if (name.Length == 0 || name.Length > 32) {
				levelButton.interactable = false;
				levelInput.color = Color.red;
				return;
			}

			levelButton.interactable = true;
			levelInput.color = Color.white;
		}

		public void ToggleCreateLevel(bool toggle) => levelCreator.SetActive(toggle);
		public void ToggleCreateFolder(bool toggle) => folderCreator.SetActive(toggle);

		public void SetSortingOrder(int order) {
			int max = System.Enum.GetValues(typeof(SortType)).Length - 1;
			order = Mathf.Clamp(order, 0, max);

			sortBy = (SortType)order;
			LoadPath(current.FullName);
		}
	}
}
