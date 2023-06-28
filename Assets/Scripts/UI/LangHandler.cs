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

using TMPro;


namespace RobotoSkunk.PixelMan.UI
{
	[AddComponentMenu("UI/RobotoSkunk - Language Handler")]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class LangHandler : MonoBehaviour
	{
		public TextMeshProUGUI text;
		public string key;

		private void Awake()
		{
			if (!text) {
				text = GetComponent<TextMeshProUGUI>();
			}

			Events.GeneralEventsHandler.LangChanged += UpdateText;
		}

		private void Start()
		{
			UpdateText();
		}

		private void OnDestroy()
		{
			Events.GeneralEventsHandler.LangChanged -= UpdateText;
		}

		public void UpdateText()
		{
			text.text = Globals.languages.GetField(key);
		}

		public void UpdateText(string value)
		{
			try {
				text.text = Globals.languages.GetField(key, value);
			} catch (System.Exception) {
				Debug.LogWarning(gameObject.name);
			}
		}

		public void UpdateText(float value)
		{
			UpdateText(value.ToString());
		}

		public void UpdateText(int value)
		{
			UpdateText(value.ToString());
		}
	}
}
