using System.Security.Cryptography;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

using UnityEngine;


namespace RobotoSkunk.PixelMan.LevelEditor.IO {
	[Serializable]
	public struct Level {
		[SerializeField]
		public List<InGameObjectProperties> objects;
		public Vector2 size;

		[Serializable]
		public struct Metadata {
			public string hash, name;
			public float version;
			public DateTime createdAt, lastModified;
		}
	}

	[Serializable]
	public struct UserScene {
		public string name, description;
		public long author;
		public long[] contributors;
		public Level.Metadata[] levels;
		public DateTime createdAt;
	}


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
			Task.Run(async () => {
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
