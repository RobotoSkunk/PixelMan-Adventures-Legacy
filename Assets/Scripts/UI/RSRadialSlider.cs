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
using UnityEngine.EventSystems;



namespace RobotoSkunk.PixelMan.UI {
	[AddComponentMenu("UI/RobotoSkunk - Radial Slider")]
	public class RSRadialSlider : MonoBehaviour, IDragHandler  {
		[System.Serializable]
		public class SliderEvent : UnityEvent<float> { }

		public RectTransform handler;
		public RectTransform rectTransform;
		public SliderEvent onValueChanged = new();

		// Min value = 0, max value = 360
		float __value;


		public float value {
			get => __value;
			set {
				SetValueWithoutNotify(value);
				onValueChanged.Invoke(__value);
			}
		}

		public void SetValueWithoutNotify(float value) {
			__value = value;
			handler.localRotation = Quaternion.Euler(0, 0, value);
		}

		public void OnDrag(PointerEventData ev) {
			RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, ev.position, ev.pressEventCamera, out Vector2 localPoint);

			float f = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
			value = f < 0 ? f + 360 : f;
		}
	}
}
