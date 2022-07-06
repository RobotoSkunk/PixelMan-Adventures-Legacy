using UnityEngine;
using UnityEngine.Events;

using RobotoSkunk.PixelMan.UI;


namespace RobotoSkunk.PixelMan.LevelEditor {
	public class ObjectInspector : MonoBehaviour {
		[System.Serializable]
		public class InspectorEvent : UnityEvent<Section, Section.PropertyField> { }

		public Section[] sections;

		[SerializeField] InspectorEvent onChange = new();

		private void Start() {
			for (int i = 0; i < sections.Length; i++) {
				sections[i].SetUp();
				sections[i].obj.SetActive(false);

				sections[i].onChange.AddListener((section, field) => SendChanges(section, field));
			}
		}


		void SendChanges(Section section, Section.PropertyField field) => onChange.Invoke(section, field);

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

					// I'm sorry you have to see this horrible comparison written in the worst possible way, but I couldn't think of another way to compare two structures and pass it to an enum.
					if (__lst.position != __act.position) isDifferent |= PropertiesEnum.Position;
					if (__lst.scale != __act.scale) isDifferent |= PropertiesEnum.FreeScale;
					if (__lst.scale.x != __act.scale.x) isDifferent |= PropertiesEnum.Scale;

					if (__lst.rotation != __act.rotation) isDifferent |= PropertiesEnum.Rotation;

					if (__lst.renderOrder != __act.renderOrder) isDifferent |= PropertiesEnum.RenderOrder;
					if (__lst.speed != __act.speed) isDifferent |= PropertiesEnum.Speed;
					if (__lst.wakeTime != __act.wakeTime) isDifferent |= PropertiesEnum.wakeTime;
					if (__lst.reloadTime != __act.reloadTime) isDifferent |= PropertiesEnum.ReloadTime;

					if (__lst.invertGravity != __act.invertGravity) isDifferent |= PropertiesEnum.InvertGravity;
					if (__lst.spawnSaw != __act.spawnSaw) isDifferent |= PropertiesEnum.SpawnSaw;
				}
			}

			if (nmb > 0) {
				for (int i = 0; i < sections.Length; i++) {
					bool toUse = (sections[i].purpose & allowed) == sections[i].purpose;

					sections[i].obj.SetActive(toUse);

					if (toUse && lastProp.HasValue) {
						bool diff = (sections[i].purpose & isDifferent) == sections[i].purpose;

						if (diff) {
							sections[i].SetDiff();
						} else {
							switch (sections[i].dataType) {
								case Section.DataType.Vector2D:
									Vector2 v = sections[i].purpose switch {
										PropertiesEnum.Position => lastProp.Value.position,
										PropertiesEnum.FreeScale => lastProp.Value.scale,
										_ => new()
									};

									sections[i].SetValue(v.x, v.y);
									break;
								case Section.DataType.Float:
									float f = sections[i].purpose switch {
										PropertiesEnum.Scale => lastProp.Value.scale.x,
										PropertiesEnum.Rotation => lastProp.Value.rotation,
										PropertiesEnum.Speed => lastProp.Value.speed,
										PropertiesEnum.wakeTime => lastProp.Value.wakeTime,
										PropertiesEnum.ReloadTime => lastProp.Value.reloadTime,
										_ => 0f
									};

									sections[i].SetValue(f);
									break;
								case Section.DataType.Integer:
									sections[i].SetValue(lastProp.Value.renderOrder);
									break;
								case Section.DataType.Boolean:
									bool b = sections[i].purpose switch {
										PropertiesEnum.InvertGravity => lastProp.Value.invertGravity,
										PropertiesEnum.SpawnSaw => lastProp.Value.spawnSaw,
										_ => false
									};

									sections[i].SetValue(b);
									break;
							}
						}

					}
				}
			} else DisableAll();
		}

		public void Add2RenderOrder(int value) {
			for (int i = 0; i < sections.Length; i++) {
				if (sections[i].purpose != PropertiesEnum.RenderOrder) continue;

				sections[i].AddInt(value);
			}
		}


		[System.Serializable]
		public class Section {
			[System.Serializable]
			public class SectionEvent : UnityEvent<Section, PropertyField> { }

			public string name;
			public PropertiesEnum purpose;
			public DataType dataType;
			public GameObject obj;
			public PropertyField[] fields;

			[System.NonSerialized]
			public SectionEvent onChange = new();


			public void SetUp() {
				for (int i = 0; i < fields.Length; i++) {
					fields[i].SetUp();

					fields[i].onChange.AddListener((field) => SendEvent(field));
				}
			}


			public void SetValue(bool b) => fields[0].SetBool(b);
			public void SetValue(float x) => SetValue(x, x);
			public void SetValue(float x, float y) {
				switch (dataType) {
					case DataType.Vector2D:
						fields[0].SetFloat(x);
						fields[1].SetFloat(y);
						break;

					case DataType.Float:
						fields[0].SetFloat(x);
						break;

					case DataType.Integer:
						fields[0].SetInt((int)x);
						break;
				}
			}

			public void SetDiff() {
				for (int i = 0; i < fields.Length; i++)
					fields[i].SetDiff();
			}

			public Vector2 GetVector2() {
				if (fields.Length == 1)
					return fields[0].GetFloat() * Vector2.one;

				if (fields.Length == 2) 
					return new(fields[0].GetFloat(), fields[1].GetFloat());

				return default;
			}

			public float GetFloat() => fields[0].GetFloat();
			public float GetInt() => fields[0].GetInt();

			public void AddFloat(float value) {
				for (int i = 0; i < fields.Length; i++)
					fields[i].AddFloat(value);
			}
			public void AddInt(int value) {
				for (int i = 0; i < fields.Length; i++)
					fields[i].AddInt(value);
			}


			void SendEvent(PropertyField field) => onChange.Invoke(this, field);


			public enum DataType {
				Float,
				Integer,
				Vector2D,
				Boolean
			}

			[System.Serializable]
			public class PropertyField {
				[System.Serializable]
				public class PropertyEvent : UnityEvent<PropertyField> { }

				public Axis axis;
				public RSInputField inputField;
				public RSSlider slider;
				public RSToggle toggle;

				[System.NonSerialized]
				public PropertyEvent onChange = new();

				public void SetUp() {
					PropertyField tmp = this;

					if (slider != null) {
						slider.onValueChanged.AddListener((value) => {
							Slider2Field();
							SendEvent();
						});
					}

					if (inputField != null) {
						inputField.onEndEdit.AddListener((value) => {
							Field2Slider();
							SendEvent();
						});
					}

					if (toggle != null)
						toggle.onValueChanged.AddListener((value) => SendEvent());
				}


				public void SetFloat(float value) {
					if (inputField != null)
						inputField.SetTextWithoutNotify(value.ToString());

					if (slider != null)
						slider.SetValueWithoutNotify(value);
				}
				public void SetBool(bool value) {
					if (toggle != null)
						toggle.SetIsOnWithoutNotify(value);
				}
				public void SetInt(int value) => SetFloat(value);
				public void SetDiff() {
					if (inputField != null)
						inputField.SetTextWithoutNotify("- - - -");

					if (slider != null)
						slider.SetValueWithoutNotify(0f);

					if (toggle != null)
						toggle.SetIsOnWithoutNotify(false);
				}


				public float GetFloat() {
					if (slider != null)
						return slider.value;

					if (inputField != null)
						return inputField.text.ToFloat();

					return default;
				}
				public int GetInt() {
					if (slider != null)
						return (int)slider.value;

					if (inputField != null)
						return inputField.text.ToInt();

					return default;
				}
				public bool GetBool() {
					if (toggle != null)
						return toggle.isOn;

					return default;
				}

				public void AddFloat(float value) {
					float f = GetFloat();
					f += value;

					SetFloat(f);
				}
				public void AddInt(int value) {
					int i = GetInt();
					i += value;

					SetInt(i);
				}


				public void Slider2Field() {
					if (inputField != null && slider != null)
						inputField.SetTextWithoutNotify(slider.value.ToString());
				}
				public void Field2Slider() {
					if (inputField != null && slider != null)
						slider.SetValueWithoutNotify(inputField.text.ToFloat());
				}


				void SendEvent() => onChange.Invoke(this);

				public enum Axis { x, y, z, w }
			}
		}
	}
}
