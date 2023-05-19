
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

using System;


namespace RobotoSkunk {
	public static class RSTime {
		private static double __lastTime;
		private static int __currFps, __fpsFactor;

		public const float defaultFPS = 60f;
		public const float wantedFrameRate = 1f / defaultFPS;

		public static int fixedFrameCount {
			get => Mathf.RoundToInt(Time.fixedTime / wantedFrameRate);
		}

		public static float delta {
			get => Time.deltaTime / wantedFrameRate;
		}

		public static int fps {
			get {
				double currTime = Time.realtimeSinceStartup,
					timeDiff = currTime - __lastTime;


				if (timeDiff >= 1f && __fpsFactor != 0) {
					__currFps = __fpsFactor;
					__fpsFactor = 1;

					__lastTime = currTime;
				} else {
					__fpsFactor++;
				}

				return __currFps;
			}
		}

		public static double realFps {
			get {
				return defaultFPS / delta;
			}
		}

		public static DateTime FromUnixTimestamp(long unixTimeStamp) {
			DateTime date = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

			return date.AddMilliseconds(unixTimeStamp).ToLocalTime();
		}

		public static long ToUnixTimestamp(DateTime date) {
			return (long) (date - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
		}
	}
}
