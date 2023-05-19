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
using Cysharp.Threading.Tasks;
using System.IO.Compression;
using System.Text;
using System.IO;
using System;
using UnityEngine.UI;


namespace RobotoSkunk {
	public static class RSRandom {
		public static int Choose(params int[] values) => values[UnityEngine.Random.Range(0, values.Length)];
		public static float Choose(params float[] values) => values[UnityEngine.Random.Range(0, values.Length)];
		public static int Sign() => Choose(1, -1);

		public static int UnionRange(params int[] values) {
			if (values.Length % 2 != 0 || values.Length == 0)
				throw new ArgumentException("Invalid number of arguments, can't be odd or zero.");

			int rnd = UnityEngine.Random.Range(0, values.Length / 2);

			return UnityEngine.Random.Range(rnd, rnd + 1);
		}

		public static float UnionRange(params float[] values) {
			if (values.Length % 2 != 0 || values.Length == 0)
				throw new ArgumentException("Invalid number of arguments, can't be odd or zero.");

			int rnd = UnityEngine.Random.Range(0, values.Length / 2) * 2;

			return UnityEngine.Random.Range(values[rnd], values[rnd + 1]);
		}
	}
}
