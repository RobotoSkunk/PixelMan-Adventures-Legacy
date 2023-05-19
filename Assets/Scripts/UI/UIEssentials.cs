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
// using UnityEngine.UI;
using UnityEngine.Events;

// using UnityEditor;
// using UnityEditor.UI;



namespace RobotoSkunk.PixelMan.UI {
	public class UIEssentials : MonoBehaviour {
		[System.Serializable]
		public class EssentialsEvent<T> : UnityEvent<T> { }
		public EssentialsEvent<string> onButtonSubmit = new();

		public void OnButtonSubmit(RSInputField inputField) => onButtonSubmit.Invoke(inputField.text);
	}

	// [CustomEditor(typeof(UIEssentials))]
	// [CanEditMultipleObjects]
	// public class UIEssentialsEditor : Editor {
		
	// }
}
