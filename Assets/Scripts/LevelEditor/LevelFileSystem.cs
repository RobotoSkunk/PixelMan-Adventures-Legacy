using System.Security.Cryptography;
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
			public string hash, name;
			public float version;
			public long createdAt, lastModified;
		}
	}

	[Serializable]
	public struct UserScene {
		public string name, description;
		public string author;
		public string[] contributors;
		public Level.UserMetadata[] levels;
		public long createdAt;
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
			public static string GenerateSHA1() {
				byte[] bytes = new byte[32];
				RNGCryptoServiceProvider rng = new();
				rng.GetBytes(bytes);

				SHA1Managed sha1 = new();

				byte[] hash = sha1.ComputeHash(bytes);
				return BitConverter.ToString(hash).Replace("-", "");
			}

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
	}
}
