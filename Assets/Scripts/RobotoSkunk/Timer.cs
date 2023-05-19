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

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System;


namespace RobotoSkunk {
	public class Timer {
		private double __timer, __timerBuffer, __lastTime;
		private bool __onTick;

		public double time {
			get {
				if (isActive) __timer = __timerBuffer + Time.time - __lastTime;

				return __timer;
			}
			set {
				__timerBuffer = value;
				__lastTime = Time.time;
			}
		}
		public bool isActive {
			get {
				return __onTick;
			}
		}

		public Timer() {
			__onTick = false;
			__timer = __timerBuffer = __lastTime = 0d;
		}

		public void Start() {
			if (!__onTick) {
				__lastTime = Time.time;
				__onTick = true;
			}
		}

		public void Stop() {
			if (__onTick) {
				__timerBuffer = __timer;
				__onTick = false;
			}
		}

		public void Reset() => time = 0d;
	}
}
