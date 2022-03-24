using RobotoSkunk.PixelMan.LevelEditor;
using UnityEngine;
using UnityEngine.Events;

namespace RobotoSkunk.PixelMan {
	public class InGameObjectBehaviour : EditorHandler {
		[System.Serializable] public class IGOEvent : UnityEvent { }

		public MonoBehaviour[] scripts;
		public SpriteRenderer[] renderers;
		public Collider2D editorCollider;
		[System.NonSerialized] public Vector2 dist2Dragged, dragOrigin;

		readonly Vector2 limit = Constants.worldLimit * Vector2.one;

		public Color color {
			set {
				for (int i = 0; i < renderers.Length; i++)
					renderers[i].color = value;
			}
		}
		public Bounds bounds {
			get {
				if (renderers.Length == 0) return new Bounds();
				Bounds tmp = renderers[0].bounds;

				for (int i = 1; i < renderers.Length; i++)
					tmp.Encapsulate(renderers[i].bounds);

				return tmp;
			}
		}
		public InGameObjectProperties properties {
			set {
				SetPropertiesWithoutTransform(value);

				SetPosition(value.position);
				SetScale(value.scale);
				SetRotation(value.rotation);
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
		public InGameObjectProperties lastProperties { get => __lastProp; }
		public InGameObject.Options options { get => __opt; }

		[SerializeField]
		IGOEvent onEditorCallback = new(), onNotEditorCallback = new();

		string __tag;
		int __layer;
		uint __id;
		Color[] __defCol;
		InGameObjectProperties __prop, __lastProp;
		InGameObject.Options __opt;

		private void Awake() {
			__tag = gameObject.tag;
			__layer = gameObject.layer;

			if (editorCollider != null)
				editorCollider.enabled = false;

			__defCol = new Color[renderers.Length];

			for (int i = 0; i < renderers.Length; i++)
				__defCol[i] = renderers[i].color;
		}

		protected override void OnStartTest() => Prepare4Editor(false);
		protected override void OnEndTest() => Prepare4Editor(true);

		void EnableScripts(bool trigger) {
			foreach (var script in scripts)
				script.enabled = trigger;
		}

		public void Prepare4Editor(bool trigger) {
			gameObject.tag = trigger ? "EditorObject" : __tag;
			gameObject.layer = trigger ? 9 : __layer;

			if (editorCollider != null)
				editorCollider.enabled = trigger;

			EnableScripts(!trigger);

			if (trigger) onEditorCallback.Invoke();
			else onNotEditorCallback.Invoke();
		}

		public string GetDefaultTag() => __tag;
		public int GetDefaultLayer() => __layer;

		public void ResetColor() {
			for (int i = 0; i < renderers.Length; i++)
				renderers[i].color = __defCol[i];
		}
		public void SetInternalId(uint id) => __id = id;
		public void SetInternalOptions(InGameObject.Options options) => __opt = options;
		public void SetLastProperties() => __lastProp = properties;
		public void SetPropertiesWithoutTransform(InGameObjectProperties prop) {
			if (prop.renderOrder != __prop.renderOrder)
				for (int i = 0; i < renderers.Length; i++)
					renderers[i].sortingOrder = __prop.orderInLayer - i;

			__prop = prop;
		}

		public void SetPosition(Vector2 value) => transform.position = RSMath.Clamp(value, -limit, limit);
		public void SetScale(Vector2 value) {
			if ((__opt.allowed & (PropertiesEnum.Scale | PropertiesEnum.FreeScale)) == 0) return;

			if (value.x < 0f) value.x = 0f;
			if (value.y < 0f) value.y = 0f;

			if ((__opt.allowed & PropertiesEnum.FreeScale) == 0)
				value = (value.x + value.y) / 2f * Vector2.one;

			transform.localScale = (Vector3)value + Vector3.forward;
		}
		public void SetRotation(float value) {
			if ((__opt.allowed & PropertiesEnum.Rotation) == 0) return;

			transform.localEulerAngles = new Vector3(0f, 0f, value);
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
				int order = Mathf.Clamp(renderOrder, -Constants.orderLimit, Constants.orderLimit);

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
			public PropertiesEnum allowed;
		}

		public enum Category {
			IGNORE,
			BLOCKS,
			DECORATION,
			GAMEPLAY, 
			OBSTACLES
		}

		public InGameObjectBehaviour Instantiate(Vector2 position, Vector2 scale, float rotation, bool doStatic = true) {
			InGameObjectBehaviour newObj = Object.Instantiate(doStatic && staticGameObject != null ? staticGameObject : gameObject);
			newObj.SetInternalOptions(options);

			newObj.SetPropertiesWithoutTransform(defaultProperties);
			newObj.SetPosition(position);
			newObj.SetScale(scale);
			newObj.SetRotation(rotation);

			return newObj;
		}
	}
}
