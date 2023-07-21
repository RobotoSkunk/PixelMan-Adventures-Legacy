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
	[System.Serializable]
	public sealed class Panel : MonoBehaviour
	{
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[Header("Components")]
		[SerializeField] Image[] buttonSwitchImages;
		[SerializeField] CanvasGroup canvasGroup;
		[SerializeField] GameObject container;
		[SerializeField] Sprite[] switchSprites = new Sprite[2];

		[Header("Properties")]
		[SerializeField] Vector2 openPanelPosition;
		[SerializeField] Vector2 closedPanelPosition;

		#pragma warning restore IDE0044


		/// <summary>
		/// The delta panel phase.
		/// </summary>
		float delta;

		/// <summary>
		/// The next delta panel phase.
		/// </summary>
		float newDelta;

		/// <summary>
		/// Returns true if the panel is open.
		/// </summary>
		bool _isOpen = false;

		/// <summary>
		/// Returns true if the panel is open.
		/// </summary>
		bool isOpen {
			get {
				return _isOpen;
			}
			set {
				if (value) {
					delta = 1f;
				} else {
					delta = 0f;
				}

				_isOpen = value;
			}
		}

		/// <summary>
		/// The rect transform of the panel.
		/// </summary>
		RectTransform rectTransform;


		private void Awake()
		{
			rectTransform = GetComponent<RectTransform>();
		}

		private void Update()
		{
			newDelta = Mathf.Lerp(newDelta, delta, 0.3f * RSTime.delta);
			rectTransform.anchoredPosition = DeltaToPosition(newDelta);


			bool needsToBeActive = newDelta >= 0.05f;

			container.SetActive(needsToBeActive);
			canvasGroup.interactable = needsToBeActive;
			canvasGroup.blocksRaycasts = needsToBeActive;

			Sprite sprite = switchSprites[needsToBeActive.ToInt()];

			if (buttonSwitchImages.Length > 0 && buttonSwitchImages[0].sprite != sprite) {

				foreach (Image image in buttonSwitchImages) {
					image.sprite = sprite;
				}
			}
		}


		public void SetOpen(bool open)
		{
			isOpen = open;
		}

		public void ToggleOpen()
		{
			SetOpen(!isOpen);
		}

		public void SetDeltaValue(float value, bool instant = false)
		{
			if (instant) {
				delta = value;
				newDelta = value;
				return;
			}

			delta = value;
		}


		Vector2 DeltaToPosition(float delta)
		{
			return Vector2.Lerp(closedPanelPosition, openPanelPosition, delta);
		}
	}
}
