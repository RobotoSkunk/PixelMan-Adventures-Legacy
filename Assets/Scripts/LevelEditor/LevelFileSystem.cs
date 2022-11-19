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
	public class Worlds {
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
				await Files.CreateFileToZip(path, uuid + ".json");
				await Files.WriteFileToZip(path, uuid + ".json", json);
			}

			public static async UniTask<Level> GetLevel(string path, string uuid) {
				string data = await Files.ReadFileFromZip(path, uuid + ".json");
				if (data == null) return default;

				return await AsyncJson.FromJson<Level>(data);
			}
		}
	}
}
