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

namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class ColorBar : MonoBehaviour {
		public ColorChannel channel;
		public Slider slider;
		public ColorPicker colorPicker;
		public int ignoreSection;

		private void Awake() {
			colorPicker.onValueChangedInternal.AddListener((Color c, bool b) => HandleChanges(c, b));
		}

		public void HandleChanges(Color color, bool bypass) {
			if (ignoreSection == colorPicker.section && !bypass) return;

			if ((channel & ColorChannel.HSV) != 0) {
				Color.RGBToHSV(color, out float h, out float s, out float v);

				if ((channel & ColorChannel.H) != 0) slider.SetValueWithoutNotify(h);
				if ((channel & ColorChannel.S) != 0) slider.SetValueWithoutNotify(s);
				if ((channel & ColorChannel.V) != 0) slider.SetValueWithoutNotify(v);
			}

			if ((channel & ColorChannel.R) != 0) slider.SetValueWithoutNotify(color.r);
			if ((channel & ColorChannel.G) != 0) slider.SetValueWithoutNotify(color.g);
			if ((channel & ColorChannel.B) != 0) slider.SetValueWithoutNotify(color.b);
			
			if ((channel & ColorChannel.ALPHA) != 0) slider.SetValueWithoutNotify(color.a);
		}
	}

	[System.Flags]
	public enum ColorChannel {
		NONE = 0,

		ALPHA = 1 << 0,
		R = 1 << 1,
		G = 1 << 2,
		B = 1 << 3,
		H = 1 << 4,
		S = 1 << 5,
		V = 1 << 6,

		RGB = R | G | B,
		HSV = H | S | V,

		ALL = ~0
	}
}
