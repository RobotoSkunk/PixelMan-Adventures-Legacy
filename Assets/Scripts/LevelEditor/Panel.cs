/*
	PixelMan Adventures, an open source platformer game.
	Copyright (C) 2023  RobotoSkunk <contact@robotoskunk.com>

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

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace RobotoSkunk.PixelMan.LevelEditor
{
	public sealed class Panel : MonoBehaviour
	{
		
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[Header("Components")]
		[SerializeField] CanvasGroup canvasGroup;
		[SerializeField] Image buttonSwitchImage;
		[SerializeField] GameObject container;

		[Header("Properties")]
		[SerializeField] Vector2 openPanelPosition;
		[SerializeField] Vector2 closedPanelPosition;

		#pragma warning restore IDE0044


		/// <summary>
		/// The delta panel phase.
		/// </summary>
		float delta;

		/// <summary>
		/// The last delta panel phase.
		/// </summary>
		float lastDelta;

		/// <summary>
		/// The panel's position based on the delta.
		/// </summary>
		Vector2 deltaPosition {
			get {
				return Vector2.Lerp(openPanelPosition, closedPanelPosition, delta);
			}
		}

		RectTransform rectTransform;


		private void Awake() {
			rectTransform = GetComponent<RectTransform>();
		}


		private void Update() {
			if (lastDelta != delta) {
				lastDelta = delta;
				rectTransform.anchoredPosition = deltaPosition;
			}
		}
	}
}
