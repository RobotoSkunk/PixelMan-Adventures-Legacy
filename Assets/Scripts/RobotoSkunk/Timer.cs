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


namespace RobotoSkunk
{
	public class Timer
	{
		private double timerBuffer;
		private double lastTime;

		private bool onTick;


		/// <summary>
		/// The time in seconds.
		/// </summary>
		public double time {
			get {
				if (isActive) {
					return timerBuffer + Time.time - lastTime;
				}

				return timerBuffer;
			}

			set {
				timerBuffer = value;
				lastTime = Time.time;
			}
		}

		public bool isActive {
			get {
				return onTick;
			}
		}


		public Timer()
		{
			onTick = false;
			timerBuffer = lastTime = 0d;
		}

		public void Start()
		{
			if (!onTick) {
				lastTime = Time.time;
				onTick = true;
			}
		}

		public void Stop()
		{
			if (onTick) {
				timerBuffer = time;
				onTick = false;
			}
		}

		public void Reset()
		{
			time = 0d;
		}

		public void SetActive(bool active)
		{
			if (active) {
				Start();
			} else {
				Stop();
			}
		}


		public override string ToString()
		{
			double hours = time / 3600d;
			double minutes = time % 3600d / 60d;
			double seconds = time % 60d;


			if (hours < 1d) {
				return minutes.ToString("00") + ":" + seconds.ToString("00");
			}

			return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
		}
	}
}
