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

using UnityEngine;

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.IO.Compression;
using System.Text;
using System.IO;
using System;


namespace RobotoSkunk {

	public static class Files {
		public static class Directories {
			private static string _root = null;

			public static string root {
				get {
					if (_root != null) return _root;

#if UNITY_ANDROID && !UNITY_EDITOR
					_root "/storage/emulated/0/Games/PixelMan Adventures";
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
					string home = Environment.GetEnvironmentVariable("HOME");
					_root = Path.Join(home, "/.local/share/PixelMan Adventures");
#else
					string folder = "/My Games/PixelMan Adventures", special = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

					if (special == "")
						special = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

					if (special == "")
						special = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("HOMEPATH") ?? "";

					_root = special + folder;
#endif
					return _root;
				}
			}

			public static string home {
				get => _root;
			}


			/// <summary>
			/// Common user data like username, account ID, etc.
			/// </summary>
			public static readonly string userData = Path.Join(root, "/user-data.json");

			/// <summary>
			/// Game settings file
			/// </summary>
			public static readonly string settings = Path.Join(root, "/settings.json");

			/// <summary>
			/// Downloaded levels
			/// </summary>
			public static readonly string downloads = Path.Join(root, "/downloads");

			/// <summary>
			/// All user's stored replays
			/// </summary>
			public static readonly string replays = Path.Join(root, "/replays");

			/// <summary>
			/// User's levels
			/// </summary>
			public static readonly string levels = Path.Join(root, "/levels");


			public static async UniTask Prepare()
			{
				await UniTask.RunOnThreadPool(() =>
				{
					// If root doesn't exist, create it recursively

					if (!Directory.Exists(root)) {
						string[] dirs = root.Replace('\\', '/').Split('/');
						string path = "";

						foreach (string dir in dirs) {
							path += dir + "/";
							if (!Directory.Exists(path)) {
								Directory.CreateDirectory(path);
							}
						}
					}

					string[] directories = {
						downloads,
						replays,
						levels
					};

					string[] files = {
						userData,
						settings
					};

					foreach (string dir in directories) {
						if (!Directory.Exists(dir)) {
							Directory.CreateDirectory(dir);
						}
					}

					foreach (string file in files) {
						if (!File.Exists(file)) {
							File.Create(file);
						}
					}
				});
			}
		}

		public static string Base64ToString(string data) {
			byte[] bits = Convert.FromBase64String(data);
			string result = Encoding.ASCII.GetString(bits);
			return result;
		}

		public static string StringToBase64(string data) {
			byte[] bits = Encoding.ASCII.GetBytes(data);
			string result = Convert.ToBase64String(bits);
			return result;
		}

		public static async UniTask<string> ReadFile(string path) {
			await Directories.Prepare();

			StreamReader sr = new(path);
			string fileContent = await sr.ReadToEndAsync();
			sr.Close();

			return fileContent;
		}

		public static async UniTask WriteFile(string path, string data) {
			await Directories.Prepare();

			StreamWriter sw = new(path);
			await sw.WriteAsync(data);
			sw.Close();
		}

		public static async UniTask GetFilesAndDirectories(string path, List<FileInfo> files, List<DirectoryInfo> directories, Func<FileInfo, bool> fileFilter = null) {
			await Directories.Prepare();

			await UniTask.RunOnThreadPool(() => {
				DirectoryInfo dir = new(path);
				FileInfo[] __files = dir.GetFiles();
				DirectoryInfo[] __dirs = dir.GetDirectories();

				files.Clear();
				directories.Clear();

				foreach (DirectoryInfo directory in __dirs) {
					directories.Add(directory);
				}

				foreach (FileInfo file in __files) {
					if (fileFilter != null && !fileFilter(file)) continue;
					files.Add(file);
				}
			});
		}

		public static async UniTask<T> FromJson<T>(string data) {
			T __tmp = default;

			await UniTask.RunOnThreadPool(() => {
				__tmp = JsonUtility.FromJson<T>(data);
			});

			return __tmp;
		}
		
		public static async UniTask FromJsonOverwrite(string data, object objectToOverwrite) {
			await UniTask.RunOnThreadPool(() => {
				JsonUtility.FromJsonOverwrite(data, objectToOverwrite);
			});
		}

		public static async UniTask<string> ToJson(string json) {
			string __tmp = null;

			await UniTask.RunOnThreadPool(() => {
				__tmp = JsonUtility.ToJson(json);
			});

			return __tmp;
		}
	
		public static bool CheckIfDirectoryIsChildOf(DirectoryInfo parent, DirectoryInfo child) {
			bool isParent = false;

			while (child.Parent != null) {
				if (child.Parent.FullName == parent.FullName) {
					isParent = true;
					break;

				} else child = child.Parent;
			}

			return isParent;
		}

		public static async UniTask<string> ReadFileFromZip(string zipPath, string filePath) {
			await Directories.Prepare();
			ZipArchive archive = ZipFile.OpenRead(zipPath);

			ZipArchiveEntry entry = archive.GetEntry(filePath);
			if (entry == null) {
				archive.Dispose();
				return null;
			}

			StreamReader reader = new(entry.Open());
			string result = await reader.ReadToEndAsync();
			reader.Close();
			archive.Dispose();

			return result;
		}

		public static async UniTask<bool> WriteFileToZip(string zipPath, string filePath, string data) {
			await Directories.Prepare();
			ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

			ZipArchiveEntry entry = archive.GetEntry(filePath);
			if (entry == null) {
				entry = archive.CreateEntry(filePath);
			}

			StreamWriter writer = new(entry.Open());
			writer.BaseStream.SetLength(0);
			await writer.WriteAsync(data);
			writer.Close();
			archive.Dispose();

			return true;
		}

		public static async UniTask<bool> DeleteFileFromZip(string zipPath, string filePath) {
			await Directories.Prepare();
			ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

			ZipArchiveEntry entry = archive.GetEntry(filePath);
			if (entry == null) {
				archive.Dispose();
				return false;
			}

			entry.Delete();
			archive.Dispose();

			return true;
		}

		public static async UniTask<bool> CreateFileToZip(string zipPath, string filePath) {
			await Directories.Prepare();
			ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

			ZipArchiveEntry entry = archive.GetEntry(filePath);
			if (entry != null) {
				archive.Dispose();
				return false;
			}

			archive.CreateEntry(filePath);
			archive.Dispose();

			return true;
		}
	}
}
