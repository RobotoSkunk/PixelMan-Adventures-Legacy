using UnityEngine;
using UnityEngine.Events;

using RobotoSkunk.PixelMan.UI;


namespace RobotoSkunk.PixelMan.LevelEditor {
	public class ObjectInspector : MonoBehaviour {
		[System.Serializable]
		public class InspectorEvent : UnityEvent<PropertiesEnum, InGameObjectProperties> { }

		public Section[] sections;

		PropertiesEnum __chg;
		InGameObjectProperties __prop;

		[SerializeField] InspectorEvent onChange = new();

		private void Start() => DisableAll();


		void SendChanges() {
			onChange.Invoke(__chg, __prop);

			__prop = default;
			__chg = 0;
		}

		void DisableAll() {
			for (int i = 0; i < sections.Length; i++) {
				sections[i].obj.SetActive(false);
			}
		}

		public void PrepareInspector(InGameObjectBehaviour[] objs) {
			PropertiesEnum allowed = PropertiesEnum.All;

			int nmb = 0;
			InGameObjectProperties? lastProp = null;
			PropertiesEnum isDifferent = 0;

			for (int i = 0; i < objs.Length; i++) {
				if (!objs[i]) continue;
				if (!objs[i].gameObject.activeInHierarchy) continue;

				allowed &= objs[i].options.allowed | PropertiesEnum.Position;
				nmb++;

				if (!lastProp.HasValue) lastProp = objs[i].properties;
				else {
					InGameObjectProperties __lst = lastProp.Value, __act = objs[i].properties;

					if (__lst.position != __act.position) isDifferent |= PropertiesEnum.Position;
					if (__lst.scale != __act.scale) isDifferent |= PropertiesEnum.Scale;

					if (__lst.rotation != __act.rotation) isDifferent |= PropertiesEnum.Rotation;

					if (__lst.renderOrder != __act.renderOrder) isDifferent |= PropertiesEnum.RenderOrder;
					if (__lst.speed != __act.speed) isDifferent |= PropertiesEnum.Speed;
					if (__lst.startupTime != __act.startupTime) isDifferent |= PropertiesEnum.StartupTime;
					if (__lst.reloadTime != __act.reloadTime) isDifferent |= PropertiesEnum.ReloadTime;
				}
			}

			if (nmb > 0)
				for (int i = 0; i < sections.Length; i++) {
					bool toUse = (sections[i].purpose & allowed) == sections[i].purpose;

					sections[i].obj.SetActive(toUse);

					if (toUse && lastProp.HasValue) {
						bool diff = (sections[i].purpose & isDifferent) == sections[i].purpose;

						if (diff) {
							sections[i].SetValue("- - - -");
						} else {
							switch (sections[i].purpose) {
								case PropertiesEnum.Position:
									sections[i].SetValue(lastProp.Value.position.x.ToString(), lastProp.Value.position.y.ToString());
									break;
								case PropertiesEnum.FreeScale:
									sections[i].SetValue(lastProp.Value.scale.x.ToString(), lastProp.Value.scale.y.ToString());
									break;
								case PropertiesEnum.Scale:
									sections[i].SetValue(lastProp.Value.scale.x.ToString());
									break;
								case PropertiesEnum.Rotation:
									sections[i].SetValue(lastProp.Value.rotation.ToString());
									break;
								case PropertiesEnum.RenderOrder:
									sections[i].SetValue(lastProp.Value.renderOrder.ToString());
									break;
								case PropertiesEnum.Speed:
									sections[i].SetValue(lastProp.Value.speed.ToString());
									break;
								case PropertiesEnum.StartupTime:
									sections[i].SetValue(lastProp.Value.startupTime.ToString());
									break;
								case PropertiesEnum.ReloadTime:
									sections[i].SetValue(lastProp.Value.reloadTime.ToString());
									break;
							}
						}

					}
				}
			else DisableAll();
		}

		[System.Serializable]
		public struct Section {
			public string name;
			public PropertiesEnum purpose;
			public DataType dataType;
			public GameObject obj;
			public RSInputField[] fields;

			float GetFloat(string str) {
				try {
					return float.Parse(str);
				} catch (System.Exception) { }

				return 0f;
			}

			int GetInt(string str) {
				try {
					return int.Parse(str);
				} catch (System.Exception) { }

				return 0;
			}

			public void SetValue(string x) => SetValue(x, x);

			public void SetValue(string x, string y) {
				switch (dataType) {
					case DataType.Vector2D:
						fields[0].SetTextWithoutNotify(x);
						fields[1].SetTextWithoutNotify(y);
						break;
					case DataType.Float:
					case DataType.Integer:
						fields[0].SetTextWithoutNotify(x);
						break;
				}
			}

			public Vector2 GetVector2() {
				if (fields.Length == 1)
					return GetFloat(fields[0].text) * Vector2.one;

				if (fields.Length == 2) 
					return new(GetFloat(fields[0].text), GetFloat(fields[1].text));

				return default;
			}

			public float GetFloat() => GetFloat(fields[0].text);
			public float GetInt() => GetInt(fields[0].text);

			public enum DataType {
				Float,
				Integer,
				Vector2D,
				Boolean
			}
		}
	}

	[System.Flags]
	public enum PropertiesEnum {
		None = 0,

		Position = 1 << 0,
		FreeScale = 1 << 1,
		Scale = 1 << 2,
		Rotation = 1 << 3,
		RenderOrder = 1 << 4,
		Speed = 1 << 5,
		StartupTime = 1 << 6,
		ReloadTime = 1 << 7,

		All = ~0
	}
}
