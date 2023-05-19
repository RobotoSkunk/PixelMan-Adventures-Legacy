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

using System.Collections.Generic;
using System;
using UnityEngine.UI;


namespace RobotoSkunk {
	public static class Extensions {
		public static bool CompareLayers(this GameObject gameObject, LayerMask layerMask) => layerMask == (layerMask | (1 << gameObject.layer));
		
		public static T ClampIndex<T>(this T[] array, int x) => array[x < 0 ? 0 : (x >= array.Length ? array.Length : x)];

		public static void SetInteractable(this List<Selectable> list, bool enabled) {
			foreach (Selectable button in list)
				if (button)
					button.interactable = enabled;
		}
		public static void SetInteractable(this Selectable[] array, bool enabled) {
			foreach (Selectable button in array)
				if (button)
					button.interactable = enabled;
		}

		public static void SetNavigation(this List<Selectable> list, Navigation.Mode mode) {
			foreach (Selectable button in list) {
				if (button) {
					Navigation navigation = button.navigation;
					navigation.mode = mode;

					button.navigation = navigation;
				}
			}
		}
		public static void SetNavigation(this Selectable[] array, Navigation.Mode mode) {
			foreach (Selectable button in array) {
				if (button) {
					Navigation navigation = button.navigation;
					navigation.mode = mode;

					button.navigation = navigation;
				}
			}
		}

		public static void SetActive(this GameObject[] array, bool enabled) {
			foreach (GameObject gameObject in array)
				if (gameObject)
					gameObject.SetActive(enabled);
		}
		public static void SetActive(this List<GameObject> list, bool enabled) {
			foreach (GameObject gameObject in list)
				if (gameObject)
					gameObject.SetActive(enabled);
		}

		public static void SetActive(this Component[] array, bool enabled) {
			foreach (Component component in array)
				if (component)
					component.gameObject.SetActive(enabled);
		}
		public static void SetActive(this List<Component> list, bool enabled) {
			foreach (Component component in list)
				if (component)
					component.gameObject.SetActive(enabled);
		}

		public static Vector4 MinMaxToVec4(this Rect rect) => new(rect.xMin, rect.yMin, rect.xMax, rect.yMax);

		public static Color FromInt(this Color _, int color) => new((color >> 16 & 0xFF) / 255f, (color >> 8 & 0xFF) / 255f, (color & 0xFF) / 255f, 1f);
		public static Color FromInt4Bytes(this Color _, int color) => new((color >> 24 & 0xFF) / 255f, (color >> 16 & 0xFF) / 255f, (color >> 8 & 0xFF) / 255f, (color & 0xFF) / 255f);
		public static int ToInt(this Color color) => (int) (color.r * 255) << 16 | (int) (color.g * 255) << 8 | (int) (color.b * 255);
		public static int ToInt4Bytes(this Color color) => (int) (color.r * 255) << 24 | (int) (color.g * 255) << 16 | (int) (color.b * 255) << 8 | (int) (color.a * 255);


		public static float ToSafeFloat(this string str) {
			if (float.TryParse(str, out float result))
				return result;

			return 0f;
		}
		public static int ToSafeInt(this string str) {
			if (int.TryParse(str, out int result))
				return result;

			return 0;
		}


		public static float ToFloat(this string str) {
			try {
				return float.Parse(str);
			} catch (Exception) { }

			return 0f;
		}

		public static int ToInt(this string str) {
			try {
				return int.Parse(str);
			} catch (Exception) { }

			return 0;
		}
	}
}
