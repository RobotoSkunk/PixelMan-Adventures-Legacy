using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

using RobotoSkunk.PixelMan.UI.MainMenu;


namespace RobotoSkunk.PixelMan.UI {
	[System.Serializable]
	public struct ColorPickerSection {
		public Sprite enabled, disabled;
		public Image image;
		public GameObject gameObject;
		public bool isHex;

		private bool __enabled;


		public bool isOn {
			get => __enabled;
			set {
				__enabled = value;
				image.sprite = value ? enabled : disabled;
				gameObject.SetActive(value);
			}
		}
	}

	[System.Serializable]
	public struct SliderRGBComponent {
		public Image left, right;
	}



	public class ColorPicker : MonoBehaviour {
		[System.Serializable]
		public class ColorChangedEvent : UnityEvent<Color> { }
		public class ColorChangedEventInternal : UnityEvent<Color, bool> { }

		[Header("Components")]
		public ColorPickerSection[] sections;
		public GameObject[] alphaBars;
		public RSInputField inputField;

		[Header("Bars")]
		public Slider sliderH;
		public Image saturationChannelBar, valueChannelBar, svPicker;
		public Image[] alphaChannelBars;
		public SliderRGBComponent[] rgbSliders;

		[Header("Circular HSV Picker")]
		public Image saturationValueChannelSquare;
		public Image svPickerCircle;
		public RectTransform huePicker, svContainer;

		[Header("Properties")]
		public bool allowAlpha;
		public ColorChangedEvent onValueChanged = new();

		[HideInInspector] public int section;
		[HideInInspector] public ColorChangedEventInternal onValueChangedInternal = new();


		float hue, saturation, value;

		private Color __color;
		public Color color {
			get => __color;
			set {
				SetValueWithoutNotify(value);
				InvokeChangedEvent(true);
				UpdateHex();
			}
		}



		private void SetSlidersColors() {
			// Color.RGBToHSV(color, out hue, out saturation, out value);
			Color c = __color;
			c.a = 1f;

			Color colH = Color.HSVToRGB(sliderH.value, 1f, 1f);
			Color inverted = Color.white - c;
			inverted.a = 1f;

			#region HSV Color Channel bars
			saturationChannelBar.color = colH;
			valueChannelBar.color = Color.HSVToRGB(hue, saturation, 1f);

			saturationValueChannelSquare.color = colH;
			svPickerCircle.rectTransform.anchoredPosition = new Vector2(saturation, value) * svContainer.sizeDelta;
			huePicker.localRotation = Quaternion.Euler(0f, 0f, hue * 360f);
			#endregion

			#region RGB Color Channel bars
			rgbSliders[0].left.color = new Color(0f, c.g, c.b);
			rgbSliders[0].right.color = new Color(1f, c.g, c.b);

			rgbSliders[1].left.color = new Color(c.r, 0f, c.b);
			rgbSliders[1].right.color = new Color(c.r, 1f, c.b);

			rgbSliders[2].left.color = new Color(c.r, c.g, 0f);
			rgbSliders[2].right.color = new Color(c.r, c.g, 1f);
			#endregion

			for (int i = 0; i < alphaChannelBars.Length; i++) alphaChannelBars[i].color = c;
		}


		private void Start() {
			UpdateButtons(0);

			foreach (GameObject alphaBar in alphaBars)
				alphaBar.SetActive(allowAlpha);
		}

		public void SetValueWithoutNotify(Color color, bool ignoreHSV2RGB = false) {
			if (!ignoreHSV2RGB) Color.RGBToHSV(color, out hue, out saturation, out value);
			__color = color;
			SetSlidersColors();

			if (!sections[section].isHex) UpdateHex();
		}

		public void UpdateButtons(int section) {
			for (int i = 0; i < sections.Length; i++) {
				sections[i].isOn = i == section;
				sections[i].image.SetNativeSize();
			}

			this.section = section;
		}

		public void GetSliderValue(ColorBar bar) {
			Color c = color;
			float f = bar.slider.value;

			if ((ColorChannel.HSV & bar.channel) != 0) {
				if ((ColorChannel.H & bar.channel) != 0) {
					Color.RGBToHSV(c, out hue, out _, out _);

					c = Color.HSVToRGB(f, saturation, value);
				}

				if ((ColorChannel.S & bar.channel) != 0) {
					Color.RGBToHSV(c, out _, out saturation, out _);

					c = Color.HSVToRGB(hue, f, value);
				}

				if ((ColorChannel.V & bar.channel) != 0) {
					Color.RGBToHSV(c, out _, out _, out value);

					c = Color.HSVToRGB(hue, saturation, f);
				}
			} else {
				if ((ColorChannel.R & bar.channel) != 0) c.r = f;
				if ((ColorChannel.G & bar.channel) != 0) c.g = f;
				if ((ColorChannel.B & bar.channel) != 0) c.b = f;

				Color.RGBToHSV(c, out hue, out saturation, out value);
			}
			c.a = allowAlpha ? f : 1f;

			SetValueWithoutNotify(c, true);
			InvokeChangedEvent(false);
			UpdateHex();
		}

		public void GetHexValue(string hex) {
			if (hex.StartsWith("#")) hex = hex[1..];
			if (hex.Length != 6 && hex.Length != 8) return;
			if (!Regex.IsMatch(hex, "[0-9a-fA-F]")) return;
			if (hex.Length == 6) hex += "FF";

			int rgba = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

			Color c = new Color32(
				(byte)((rgba >> 24) & 0xff),
				(byte)((rgba >> 16) & 0xff),
				(byte)((rgba >> 8) & 0xff),
				(byte)(rgba & 0xff)
			);

			if (!allowAlpha) c.a = 1f;

			SetValueWithoutNotify(c, false);
			InvokeChangedEvent(false);
		}

		void InvokeChangedEvent(bool bypass) {
			onValueChanged.Invoke(color);
			
			onValueChangedInternal.Invoke(color, bypass);
		}


		void UpdateHex() {
			string hex = allowAlpha ? ColorUtility.ToHtmlStringRGBA(__color) : ColorUtility.ToHtmlStringRGB(__color);
			inputField.SetTextWithoutNotify(string.Concat("#", hex));
		}
	}
}
