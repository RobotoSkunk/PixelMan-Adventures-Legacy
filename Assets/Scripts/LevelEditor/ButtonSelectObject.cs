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
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using RobotoSkunk.PixelMan.UI;


namespace RobotoSkunk.PixelMan.LevelEditor {
	public class ButtonSelectObject : MonoBehaviour, ISelectHandler {
		[System.Serializable] public class BtnEvent : UnityEvent { }

		public Image preview;
		public RSButton button;
		public BtnEvent onSelect = new();

		public void OnSelect(BaseEventData ev) => onSelect.Invoke();
	}
}
