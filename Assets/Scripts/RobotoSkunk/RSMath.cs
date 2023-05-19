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


namespace RobotoSkunk {
	public static class RSMath {
		public static float Lengthdir_x(float x, float ang) => Mathf.Cos(ang) * x;
		public static float Lengthdir_y(float x, float ang) => Mathf.Sin(ang) * x;

		public static Vector3 GetDirVector(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

		public static float SafeDivision(float numerator, float denominator) => denominator == 0f ? 0f : numerator / denominator;

		public static float Direction(Vector2 from, Vector2 to) => Mathf.Atan2(to.y - from.y, to.x - from.x);

		public static int ToInt(this bool b) => b ? 1 : 0;

		public static Vector2 Rotate(Vector2 vector, float angle) => new(
			vector.x * Mathf.Cos(angle) - vector.y * Mathf.Sin(angle),
			vector.x * Mathf.Sin(angle) + vector.y * Mathf.Cos(angle)
		);

		public static Vector2 Clamp(Vector2 vector, Vector2 min, Vector2 max) => new(
			Mathf.Clamp(vector.x, min.x, max.x),
			Mathf.Clamp(vector.y, min.y, max.y)
		);

		public static Vector3 Clamp(Vector3 vector, Vector3 min, Vector3 max) => new(
			Mathf.Clamp(vector.x, min.x, max.x),
			Mathf.Clamp(vector.y, min.y, max.y),
			Mathf.Clamp(vector.z, min.z, max.z)
		);

		public static Vector2 Abs(Vector2 vector) => new(
			Mathf.Abs(vector.x),
			Mathf.Abs(vector.y)
		);

		public static Vector3 Abs(Vector3 vector) => new(
			Mathf.Abs(vector.x),
			Mathf.Abs(vector.y),
			Mathf.Abs(vector.z)
		);

		public static Vector2 Round(Vector2 vector) => new(
			Mathf.Round(vector.x),
			Mathf.Round(vector.y)
		);

		public static Vector3 Round(Vector3 vector) => new(
			Mathf.Round(vector.x),
			Mathf.Round(vector.y),
			Mathf.Round(vector.z)
		);
	}

}
