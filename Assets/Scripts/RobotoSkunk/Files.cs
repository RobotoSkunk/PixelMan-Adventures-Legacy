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
					_root = home + "/.local/share/PixelMan Adventures";
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

			public static string userData = root + "/user-data.json", settings = root + "/settings.json";

			public static class User {
				public static string dir = root + "/user", levels = dir + "/levels", replays = dir + "/replays";
			}
			public static class Downloads {
				public static string dir = root + "/downloads", levels = dir + "/levels", cache = dir + "/cache";
			}

			public static async UniTask Prepare() {
				await UniTask.RunOnThreadPool(() => {
					if (!Directory.Exists(root)) Directory.CreateDirectory(root);

					if (!Directory.Exists(User.dir)) Directory.CreateDirectory(User.dir);
					if (!Directory.Exists(User.levels)) Directory.CreateDirectory(User.levels);
					if (!Directory.Exists(User.replays)) Directory.CreateDirectory(User.replays);

					if (!Directory.Exists(Downloads.dir)) Directory.CreateDirectory(Downloads.dir);
					if (!Directory.Exists(Downloads.levels)) Directory.CreateDirectory(Downloads.levels);
					if (!Directory.Exists(Downloads.cache)) Directory.CreateDirectory(Downloads.cache);

					if (!File.Exists(userData)) File.Create(userData);
					if (!File.Exists(settings)) File.Create(settings);
				});
			}
		}

		public static string B64ToStr(string data) {
			byte[] bits = Convert.FromBase64String(data);
			string result = Encoding.ASCII.GetString(bits);
			return result;
		}

		public static string StrToB64(string data) {
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
