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


namespace RobotoSkunk {
	public static class Device {
		public enum Type {
			Windows,
			MacOSX,
			Linux,
			Android,
			iOS,
			Unknown
		}

		public static Type type {
			get {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
				return Type.Windows;

#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
				return Type.MacOSX;

#elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
				return Type.Linux;

#elif !UNITY_EDITOR && UNITY_ANDROID
				return Type.Android;

#elif !UNITY_EDITOR && UNITY_IOS
				return Type.iOS;
#else
				return Type.Unknown;
#endif
			}
		}


		// Credits to aVolpe: https://gist.github.com/aVolpe/707c8cf46b1bb8dfb363
#if UNITY_ANDROID && !UNITY_EDITOR
		readonly private static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		readonly private static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
		readonly private static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
		readonly private static AndroidJavaClass unityPlayer;
		readonly private static AndroidJavaObject currentActivity, vibrator;
#endif

		public static void Vibrate(long milliseconds) {
			try {
				if (type == Type.Android) vibrator.Call("vibrate", milliseconds);
				// else if (SystemInfo.deviceType == DeviceType.Handheld) Handheld.Vibrate();
			} catch (Exception err) {
				UnityEngine.Debug.LogWarning(err);
			}
		}
	}
}
