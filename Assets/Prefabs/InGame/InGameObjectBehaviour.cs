using UnityEngine;
using UnityEngine.Events;

namespace RobotoSkunk.PixelMan {
	public class InGameObjectBehaviour : EditorHandler {
		[System.Serializable]
		public class IGOEvent : UnityEvent { }

		public InGameObjectProperties properties {
			set => __prop = value;
			get {
				InGameObjectProperties tmp = __prop;

				tmp.position = transform.position;
				tmp.scale = transform.localScale;
				tmp.rotation = transform.rotation.eulerAngles.z;

				return tmp;
			}
		}
		public MonoBehaviour[] scripts;

		[SerializeField]
		IGOEvent onEditorCallback = new(), onNotEditorCallback = new();

		string __tag;
		int __layer;
		InGameObjectProperties __prop;

		private void Awake() {
			__tag = gameObject.tag;
			__layer = gameObject.layer;
		}

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
