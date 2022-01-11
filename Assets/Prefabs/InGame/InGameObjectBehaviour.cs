using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan {
	public class InGameObjectBehaviour : GameHandler {
		public InGameObjectProperties properties;
		public MonoBehaviour[] scripts;
	}

	[System.Serializable]
	public struct InGameObjectProperties {
		[Header("Generic")]
		public uint id;
		public Vector2 position, scale;
		public float rotation;

		[Header("Advanced")]
		public int renderOrder;
		public uint skinIndex;
		public float speed, startupTime, reloadTime;

		public int orderInLayer {
			get {
				int order = Mathf.Clamp(renderOrder, -3000, 3000);

				return order * 10;
			}
		}
	}

	[System.Serializable]
	public struct InGameObject {
		public string name;
		public GameObject gameObject;
		public Sprite preview;
		public Category category;
		public InGameObjectProperties defaultProperties;
		public Options options;

		[System.Serializable]
		public struct Options {
			public bool
				allowRenderOrder,
				allowSkinIndex,
				allowSpeed,
				allowStartupTime,
				allowReloadTime;
		}

		public enum Category {
			BLOCKS,
			DECORATION,
			GAMEPLAY, 
			OBSTACLES
		}
	}
}
