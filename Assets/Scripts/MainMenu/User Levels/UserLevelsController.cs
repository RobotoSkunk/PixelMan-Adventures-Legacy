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

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.IO.Compression;
using System.IO;

using UnityEngine;
using UnityEngine.UI;

using RobotoSkunk.PixelMan.LevelEditor;
using RobotoSkunk.PixelMan.LevelEditor.IO;


using TMPro;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserLevelsController : MonoBehaviour {
		[Header("References")]
		public LevelTemplate levelTemplate;
		public FolderTemplate folderTemplate;
		public FolderGoBack folderGoBack;

		[Header("Properties")]
		public float loadingBarSpeed = 200f;

		[Header("Components")]
		public CanvasGroup contentGroup;
		public RectTransform content;
		public Transform hoverParent;
		public Popup popup;

		[Header("UI")]
		public GameObject loadingIndicator;
		public RectTransform loadingBar;
		public GameObject levelCreator, folderCreator;
		public Image levelInput, folderInput;
		public RSButton levelButton, folderButton;
		public TextMeshProUGUI currentPathText;
		public Image sortModeImage;

		[Header("Important")]
		public MenuController menuController;
		public int overviewMenu;


		readonly List<FileInfo> files = new();
		readonly List<DirectoryInfo> folders = new();
		readonly List<InternalUserScene> scenes = new();
		readonly System.Func<FileInfo, bool> fileFilter = delegate(FileInfo file) { return file.Extension == ".pml"; };

		DirectoryInfo root, current;
		SortType sortBy = SortType.Name;
		float waitDelta = 0f;
		bool invertedSort = false;
		FolderTemplate currentFolderTemplate;
		LevelTemplate currentLevelTemplate;

		public enum SortType { Name, Date, Edited, Size }

		bool __isBusy;

		public bool isBusy {
			get => __isBusy;
			set {
				loadingIndicator.SetActive(value);
				contentGroup.alpha = (!value).ToInt();
				contentGroup.interactable = !value;

				__isBusy = value;
			}
		}


		private void Start() {
			root = new(Files.Directories.levels);
			isBusy = false;

			LoadPath(root.FullName);
		}

		private void Update() {
			if (isBusy) {
				waitDelta += Time.deltaTime * loadingBarSpeed;
				if (waitDelta >= 360f) waitDelta = 0f;

				float delta = Mathf.Sin(waitDelta * Mathf.Deg2Rad);
				loadingBar.anchoredPosition = new(delta * loadingBar.rect.size.x, 0);
			}
		}


		public void LoadPath(string path) {
			UniTask.Void(async () => {
				isBusy = true;

				#region Get and sort info
				DirectoryInfo dir = new(path);
				foreach (Transform child in content) Destroy(child.gameObject);

				string currentPath = dir.FullName.Replace(root.FullName, "");

				if (currentPath.Length > 0)
					currentPath = currentPath[1..].Replace("\\", "/").Replace("/", " > ");

				currentPathText.text = currentPath;


				if (current != dir) {
					await Files.GetFilesAndDirectories(path, files, folders, fileFilter);
					scenes.Clear();

					foreach (FileInfo file in files) {
						try {
							UserScene scene = await LevelFileSystem.GetMetadata(file.FullName);
							InternalUserScene data = new() {
								file = file,
								data = scene
							};

							scenes.Add(data);
						} catch (System.Exception e) { Debug.LogError(e); }
					}

					current = dir;
				}

				await UniTask.RunOnThreadPool(() => {
					switch (sortBy) {
						case SortType.Date:
							scenes.Sort((a, b) => invertedSort ? a.data.createdAt.CompareTo(b.data.createdAt) : b.data.createdAt.CompareTo(a.data.createdAt));
							folders.Sort((a, b) => invertedSort ? a.CreationTime.CompareTo(b.CreationTime) : b.CreationTime.CompareTo(a.CreationTime));
							break;
						case SortType.Edited:
							scenes.Sort((a, b) => invertedSort ? a.data.lastModified.CompareTo(b.data.lastModified) : b.data.lastModified.CompareTo(a.data.lastModified));
							folders.Sort((a, b) => invertedSort ? a.LastWriteTime.CompareTo(b.LastWriteTime) : b.LastWriteTime.CompareTo(a.LastWriteTime));
							break;
						case SortType.Size:
							scenes.Sort((a, b) => invertedSort ? a.file.Length.CompareTo(b.file.Length) : b.file.Length.CompareTo(a.file.Length));
							folders.Sort((a, b) => invertedSort ? a.GetFiles().Length.CompareTo(b.GetFiles().Length) : b.GetFiles().Length.CompareTo(a.GetFiles().Length));
							break;
						case SortType.Name:
						default:
							scenes.Sort((a, b) => invertedSort ? a.data.name.CompareTo(b.data.name) : b.data.name.CompareTo(a.data.name));
							folders.Sort((a, b) => invertedSort ? a.Name.CompareTo(b.Name) : b.Name.CompareTo(a.Name));
							break;
					}
				});

				#endregion

				#region Create UI
				try {
					if (Files.CheckIfDirectoryIsChildOf(root, dir)) {
						var go = Instantiate(folderGoBack, content);
						go.info = dir.Parent;

						go.onClick.AddListener(() => LoadPath(go.info.FullName));
					}
				} catch (System.Exception e) { Debug.LogError(e); }

				foreach (DirectoryInfo folder in folders) {
					try {
						FolderTemplate _tmp = Instantiate(folderTemplate, content);
						_tmp.hoveringParent = hoverParent;
						_tmp.controller = this;
						_tmp.info = folder;

						_tmp.onClick.AddListener(() => LoadPath(folder.FullName));
						_tmp.deleteButton.onClick.AddListener(() => {
							popup.open = true;
							popup.index = 3;

							currentFolderTemplate = _tmp;
						});
					} catch (System.Exception e) { Debug.LogError(e); }
				}

				foreach(InternalUserScene scene in scenes) {
					try {
						LevelTemplate _tmp = Instantiate(levelTemplate, content);
						_tmp.hoveringParent = hoverParent;
						_tmp.controller = this;
						_tmp.info = scene;

						_tmp.deleteButton.onClick.AddListener(() => {
							popup.open = true;
							popup.index = 2;

							currentLevelTemplate = _tmp;
						});
						_tmp.onClick.AddListener(() => {
							Globals.Editor.currentScene = scene;
							menuController.SetMenu(overviewMenu);
						});

					} catch (System.Exception e) { Debug.LogError(e); }
				}
				#endregion

				isBusy = false;
			});
		}
		public void ForceLoadPath(string path) {
			current = null;
			LoadPath(path);
		}
		public void ForceReload() => LoadPath(current.FullName);


		public void OpenLevel(InternalUserScene scene) {
			UniTask.Void(async () => {
				InternalUserScene metadata = new() {
					data = await LevelFileSystem.GetMetadata(scene.file.FullName),
					file = scene.file
				};

				Globals.Editor.currentScene = metadata;
				menuController.SetMenu(1);
			});
		}


		public void CreateLevel(string name) {
			UniTask.Void(async () => {
				name = LevelFileSystem.FilterLevelName(name);

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
				await writer.WriteAsync(data);
				writer.Close();

				archive.Dispose();
				stream.Close();

				ForceReload();
			});
		}

		public void CreateFolder(string name) {
			name = LevelFileSystem.FilterDirectoryName(name);

			Directory.CreateDirectory(Path.Combine(current.FullName, name));
			ForceReload();
		}

		public void DeleteFolder() {
			if (currentFolderTemplate == null) return;

			Directory.Delete(currentFolderTemplate.info.FullName, true);
			ForceReload();
		}

		public void DeleteLevel() {
			if (currentLevelTemplate == null) return;

			File.Delete(currentLevelTemplate.info.file.FullName);
			ForceReload();
		}



		// public void CheckFolderName(string name) {
		// 	string[] invalidChars = new string[] { "\\", "/", ":", "*", "?", "\"", "<", ">", "|" };
		// 	name = name.Trim();

		// 	if (name.Length == 0 || name.Length > 200) {
		// 		folderButton.interactable = false;
		// 		folderInput.color = Color.red;
		// 		return;
		// 	}

		// 	foreach (string invalidChar in invalidChars)
		// 		if (name.Contains(invalidChar)) {
		// 			folderButton.interactable = false;
		// 			folderInput.color = Color.red;
		// 			return;
		// 		}

		// 	folderButton.interactable = true;
		// 	folderInput.color = Color.white;
		// }
		// public void CheckLevelName(string name) {
			// name = name.Trim();

			// if (name.Length == 0 || name.Length > 32) {
			// 	levelButton.interactable = false;
			// 	levelInput.color = Color.red;
			// 	return;
			// }

			// levelButton.interactable = true;
			// levelInput.color = Color.white;
		// }

		public void TogglePopup(bool toggle) => popup.open = toggle;
		public void SetPopupIndex(int index) => popup.index = index;



		public void SetSortingOrder(int order) {
			int max = System.Enum.GetValues(typeof(SortType)).Length - 1;
			order = Mathf.Clamp(order, 0, max);

			sortBy = (SortType)order;
			LoadPath(current.FullName);
		}
		public void ToggleSortingMode() {
			invertedSort = !invertedSort;
			sortModeImage.transform.localScale = new Vector3(1, invertedSort ? -1 : 1, 1);

			LoadPath(current.FullName);
		}
	}
}
