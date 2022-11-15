using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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
			public string name;
			public TextAsset levelData;
		}

		[Serializable]
		public struct UserMetadata {
			public string uuid, name;
			public float version;
			public long createdAt, lastModified;
		}
	}

	[Serializable]
	public struct UserScene {
		public long id, cloudId;
		public string name, description;
		public string author;
		public string[] contributors;
		public Level.UserMetadata[] levels;
		public long createdAt, lastModified;
	}

	public struct InternalUserScene {
		public UserScene data;
		public string path;
		public long size;
	}

	[Serializable]
	public class Worlds {
		public string name, internalId;
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
			public static void Save(Level level, Action<bool> callback = null) {
				UniTask.Void(async () => {
					bool success = true;

					try {
						var json = await AsyncJson.ToJson(level);

						Debug.Log(json);
					} catch (Exception e) {
						Debug.LogError(e);
						success = false;
					}

					callback?.Invoke(success);
				});
			}
		}

		public static class LevelFileSystem {
			public static async UniTask<UserScene> GetMetadata(string path) {
				string data = await Files.ReadFileFromZip(path, "metadata.json");
				if (data == null) return default;

				return await AsyncJson.FromJson<UserScene>(data);
			}
		}
	}
}
