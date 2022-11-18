using UnityEngine;

using TMPro;


namespace RobotoSkunk.PixelMan.UI {
	[AddComponentMenu("UI/RobotoSkunk - Language Handler")]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class LangHandler : MonoBehaviour {
		public TextMeshProUGUI text;
		public string key;

		private void Awake() {
			if (!text) text = GetComponent<TextMeshProUGUI>();
			Events.GeneralEventsHandler.LangChanged += UpdateText;
		}

		private void Start() => UpdateText();
		private void OnDestroy() => Events.GeneralEventsHandler.LangChanged -= UpdateText;
		public void UpdateText() => text.text = Globals.languages.GetField(key);
	}
}
