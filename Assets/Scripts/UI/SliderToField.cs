using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace RobotoSkunk.PixelMan.UI {
	public class SliderToField : MonoBehaviour {
		public Slider from;
		public TMP_InputField to;


		public void DoBridge() => to.text = from.value.ToString();
		public void DoBridgeWithoutNotify() => to.SetTextWithoutNotify(from.value.ToString());
	}
}
