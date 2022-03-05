using UnityEngine;
using UnityEngine.Events;

namespace RobotoSkunk.PixelMan {
	public class InGameObjectBehaviour : EditorHandler {
		[System.Serializable] public class IGOEvent : UnityEvent { }

		public MonoBehaviour[] scripts;
		public SpriteRenderer[] renderers;
		[System.NonSerialized] public Vector2 dist2Dragged, dragOrigin;

		public Color color {
			set {
				/*if (value == Color.clear) __color = Color.white;
				else __color = value;*/

				for (int i = 0; i < renderers.Length; i++)
					renderers[i].color = value;
			}
			// get => __color;
		}
		public InGameObjectProperties properties {
			set {
				__prop = value;

				for (int i = 0; i < renderers.Length; i++)
					renderers[i].sortingOrder = __prop.orderInLayer - i;
			}
			get {
				InGameObjectProperties tmp = __prop;

				tmp.id = __id;
				tmp.position = transform.position;
				tmp.scale = transform.localScale;
				tmp.rotation = transform.rotation.eulerAngles.z;

				return tmp;
			}
		}

		[SerializeField]
		IGOEvent onEditorCallback = new(), onNotEditorCallback = new();

		string __tag;
		int __layer;
		uint __id;
		InGameObjectProperties __prop;

		private void Awake() {
			__tag = gameObject.tag;
			__layer = gameObject.layer;

			for (int i = 0; i < renderers.Length; i++)
				renderers[i].sortingOrder = __prop.orderInLayer - i;
		}

		/*private void Update() {
			if (renderers.Length == 0) return;
			if (!renderers[0].isVisible) return;

			for (int i = 0; i < renderers.Length; i++) {
				renderers[i].color = color;
				renderers[i].sortingOrder = properties.orderInLayer - i;
			}
		}*/

		protected override void OnStartTest() => Prepare4Editor(true);
		protected override void OnEndTest() => Prepare4Editor(false);

		void EnableScripts(bool trigger) {
			foreach (var script in scripts)
				script.enabled = trigger;
		}

		public void Prepare4Editor(bool trigger) {
			gameObject.tag = trigger ? "EditorObject" : __tag;
			gameObject.layer = trigger ? 9 : __layer;

			EnableScripts(!trigger);

			if (trigger) onEditorCallback.Invoke();
			else onNotEditorCallback.Invoke();
		}

		public void SetInternalId(uint id) => __id = id;
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
		public InGameObjectBehaviour gameObject, staticGameObject;
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
