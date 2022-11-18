using UnityEngine;

using TMPro;


namespace RobotoSkunk.PixelMan.UI {
	[AddComponentMenu("UI/RobotoSkunk - Dropdown Language Handler")]
	[RequireComponent(typeof(RSDropdown))]
	public class DropdownLangHandler : MonoBehaviour {
		public RSDropdown dropdown;
		public string[] options;

		private void Awake() {
			if (!dropdown) dropdown = GetComponent<RSDropdown>();
			Events.GeneralEventsHandler.LangChanged += UpdateText;
		}

		private void Start() => UpdateText();
		private void OnDestroy() => Events.GeneralEventsHandler.LangChanged -= UpdateText;
		public void UpdateText() {
			dropdown.options.Clear();

			foreach (string option in options)
				dropdown.options.Add(new TMP_Dropdown.OptionData(Globals.languages.GetField(option)));
		}
	}
}
