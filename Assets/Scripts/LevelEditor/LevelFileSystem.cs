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
using System.IO;
using System;

using UnityEngine;
using Eflatun.SceneReference;

namespace RobotoSkunk.PixelMan.LevelEditor {
	[Serializable]
	public struct Level {
		[SerializeField]
		public List<InGameObjectProperties> objects;
		public Vector2 size;

		[Serializable]
		public struct Metadata {
			public string uuid, name;
			public TextAsset levelData;
		}

		[Serializable]
		public struct UserMetadata {
			public string uuid, name;
			public float version;
			public long createdAt, lastModified, timeSpent;
		}
	}

	[Serializable]
	public struct UserScene {
		public long id, cloudId;
		public string name, description;
		public string author;
		public string[] contributors;
		public List<Level.UserMetadata> levels;
		public long createdAt, lastModified;
	}

	public struct InternalUserScene {
		public UserScene data;
		public FileInfo file;
	}

	[Serializable]
	public sealed class Worlds {
		public string name, uuid;
		public SceneReference bossScene;
		public GameScene[] scenes;

		[Serializable]
		public class GameScene {
			public string name;
			public Level.Metadata[] levels;
		}
	}


	namespace IO {
		public static class LevelIO {
			public static string GenerateUUID() => Guid.NewGuid().ToString();
			public static async UniTask<bool> Save(Level level) {
				bool success = true;

				try {
					string path = Globals.Editor.currentScene.file.FullName;
					string uuid = Globals.Editor.currentLevel.uuid;

					await LevelFileSystem.WriteLevel(path, level, uuid);
				} catch (Exception e) {
					Debug.LogError(e);
					success = false;
				}

				return success;
			}
		}

		public static class LevelFileSystem {
			public static async UniTask<UserScene> GetMetadata(string path) {
				string data = await Files.ReadFileFromZip(path, "metadata.json");
				if (data == null) return default;

				return await AsyncJson.FromJson<UserScene>(data);
			}

			public static async UniTask WriteMetadata(string path, UserScene data) {
				string json = await AsyncJson.ToJson(data);
				await Files.WriteFileToZip(path, "metadata.json", json);
			}

			public static async UniTask WriteLevel(string path, Level level, string uuid) {
				string json = await AsyncJson.ToJson(level);
				Files.CreateFileToZip(path, uuid + ".json");
				await Files.WriteFileToZip(path, uuid + ".json", json);
			}

			public static async UniTask<Level> GetLevel(string path, string uuid) {
				string data = await Files.ReadFileFromZip(path, uuid + ".json");
				if (data == null) return default;

				return await AsyncJson.FromJson<Level>(data);
			}


			/// <summary>
			/// Filters out invalid characters from a level or stage name.
			/// </summary>
			public static string FilterLevelName(string name)
			{
				// Verify if the user is being a smartass
				if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) {
					return "Unnamed";
				}

				// Remove unnecessary whitespace
				name = name.Trim();

				// Limit the name to 32 characters
				if (name.Length > 32) {
					name = name[..32];
				}

				return name;
			}

			/// <summary>
			/// Filters out invalid characters from a directory name.
			/// </summary>
			public static string FilterDirectoryName(string name)
			{
				// Repeat the same as above
				name = FilterLevelName(name);

				// Remove invalid characters
				#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				string[] invalidChars = {
					"<", ">", ":", "\"", "/", "\\", "|", "?", "*"
				};
				#else
				string[] invalidChars = {
					"/"
				};
				#endif

				foreach (string c in invalidChars) {
					name = name.Replace(c, "");
				}

				if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) {
					return "Unnamed";
				}

				return name;
			}
		}
	}
}
