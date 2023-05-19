/*
	PixelMan Adventures, an open source platformer game.
	Copyright (C) 2022  RobotoSkunk <contact@robotoskunk.com>

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Affero General Public License as published
	by the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Affero General Public License for more details.

	You should have received a copy of the GNU Affero General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;
using UnityEngine.Events;

using RobotoSkunk.PixelMan.UI;


/*
	Tengo un pinche desmadre aquí.
	
	I tried to explain this the most I can, but I'm not sure if you can understand it.
	Good luck.


	This is the inspector for the objects in the level editor. The way it works it's pretty easy to understand (I hope).

	The inspector works with three classes:
	- ObjectInspector: This is the inspector itself. It's the one that shows the properties of the selected object.
	- ObjectInspector.Section: This is a section of the inspector. It's the one that shows the properties of a specific component.
	- ObjectInspector.Section.PropertyField: This is a property field, the user here can change the value of the property.

	When the MainEditor calls the inspector, it reloads, displays and hides all the sections and property fields.
	When the user changes a value, the inspector calls the MainEditor to change the value of the property.
*/


namespace RobotoSkunk.PixelMan.LevelEditor {
	public class ObjectInspector : MonoBehaviour {
		[System.Serializable]
		public class InspectorEvent : UnityEvent<Section, Section.PropertyField> { }

		public GameObject defaultText;
		public Section[] sections;

		[SerializeField] InspectorEvent onChange = new();

		// Here the inspector prepares itself to control the sections and property fields.
		private void Start() {
			for (int i = 0; i < sections.Length; i++) {
				sections[i].SetUp();
				sections[i].obj.SetActive(false);

				sections[i].onChange.AddListener((section, field) => SendChanges(section, field));
			}
		}

		// Sends the changes to the MainEditor... obviously.
		void SendChanges(Section section, Section.PropertyField field) => onChange.Invoke(section, field);

		// This disables all the sections and property fields.
		void DisableAll() {
			for (int i = 0; i < sections.Length; i++) {
				sections[i].obj.SetActive(false);
			}
		}

		// This function is called by the MainEditor to show the sections and property fields of the selected(s) object(s).
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

				// First the inspector checks which sections can be shown and which can't.
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

			// Then the inspector sets the values of the sections and property fields.
			defaultText.SetActive(nmb == 0);

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


		// Changes the selected object(s) render order.
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


			#region Setters
			// The setters calls each field's setter.

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
			#endregion

			#region Getters
			// The same as above, but for getting the values.

			public Vector2 GetVector2() {
				if (fields.Length == 1)
					return fields[0].GetFloat() * Vector2.one;

				if (fields.Length == 2) 
					return new(fields[0].GetFloat(), fields[1].GetFloat());

				return default;
			}

			public float GetFloat() => fields[0].GetFloat();
			public float GetInt() => fields[0].GetInt();
			#endregion

			#region Adders
			// You guessed it, the same as above, but for adding values.

			public void AddFloat(float value) {
				for (int i = 0; i < fields.Length; i++)
					fields[i].AddFloat(value);
			}
			public void AddInt(int value) {
				for (int i = 0; i < fields.Length; i++)
					fields[i].AddInt(value);
			}
			#endregion

			// Tells the Inspector to update the values.
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
				public float value;

				[System.NonSerialized]
				public PropertyEvent onChange = new();

				public void SetUp() {
					PropertyField tmp = this;

					if (slider != null) {
						slider.onValueChanged.AddListener((value) => {
							this.value = slider.value;
							Slider2Field();
							SendEvent();
						});
					}

					if (inputField != null) {
						inputField.onEndEdit.AddListener((value) => {
							this.value = value.ToSafeFloat();
							Field2Slider();
							SendEvent();
						});
					}

					if (toggle != null)
						toggle.onValueChanged.AddListener((value) => SendEvent());
				}


				#region Setters
				// The setters calls each field's setter to display the value.

				public void SetFloat(float value) {
					this.value = value;

					if (inputField != null)
						inputField.SetTextWithoutNotify(value.ToString());

					if (slider != null)
						slider.SetValueWithoutNotify(value);
				}
				public void SetBool(bool value) {
					if (toggle != null)
						toggle.SetIsOnWithoutNotify(value);
				}
				public void SetInt(int value) {
					this.value = value;
					SetFloat(value);
				}
				public void SetDiff() {
					value = 0f;

					if (inputField != null)
						inputField.SetTextWithoutNotify("- - - -");

					if (slider != null)
						slider.SetValueWithoutNotify(0f);

					if (toggle != null)
						toggle.SetIsOnWithoutNotify(false);
				}
				#endregion

				#region Getters
				public float GetFloat() => value;
				public int GetInt() => (int)value;
				public bool GetBool() {
					if (toggle != null)
						return toggle.isOn;

					return default;
				}
				#endregion

				#region Adders
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
				#endregion


				#region Converters
				public void Slider2Field() {
					if (inputField != null && slider != null)
						inputField.SetTextWithoutNotify(value.ToString());
				}
				public void Field2Slider() {
					if (inputField != null && slider != null)
						slider.SetValueWithoutNotify(inputField.text.ToFloat());
				}
				#endregion


				// Tells the section to tell the inspector to update the values... yeah, that's the way it is.
				void SendEvent() => onChange.Invoke(this);

				public enum Axis { x, y, z, w }
			}
		}
	}
}
