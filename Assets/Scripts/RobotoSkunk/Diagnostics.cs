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

using System.Diagnostics;
using System.IO;
using System;

namespace RobotoSkunk {
	public static class Diagnostics {
		public static string deviceData =
			$"Operating System: {SystemInfo.operatingSystem}\n" +
			$"     Device info: {SystemInfo.deviceModel} [{SystemInfo.deviceName}]\n" +
			$"     Device type: {SystemInfo.deviceType}\n" +
			$"       Game info: PixelMan Adventures x{(Environment.Is64BitProcess ? "64" : "86")}\n" +
			$"        Graphics: {SystemInfo.graphicsDeviceName} {SystemInfo.graphicsDeviceType} {SystemInfo.graphicsMemorySize} Mb\n" +
			$"Graphics version: {SystemInfo.graphicsDeviceVersion}\n" +
			$"       Processor: {SystemInfo.processorType} ({SystemInfo.processorCount} CPUs) {SystemInfo.processorFrequency} MHz\n" +
			$"          Memory: {SystemInfo.systemMemorySize} Mb\n" +
			$" Game files path: {Files.Directories.root}";

		public static class Sizes {
			public static float kilobyte = 1024f;
			public static float megabyte = Mathf.Pow(kilobyte, 2f);
			public static float gigabyte = Mathf.Pow(megabyte, 2f);
			public static float terabyte = Mathf.Pow(gigabyte, 2f);
		}

		public static float availableDiskSpace {
			get {
				DriveInfo[] drives = DriveInfo.GetDrives();
				float availableSpace = 0;

				foreach (DriveInfo d in drives) {
					if (d.RootDirectory.Name[0] == Files.Directories.root[0]) {
						availableSpace = d.AvailableFreeSpace / Sizes.megabyte;
						break;
					}
				}

				return availableSpace;
			}
		}

		public static string systemLanguage {
			get {
				return Application.systemLanguage switch {
					SystemLanguage.Afrikaans => "af",
					SystemLanguage.Arabic => "ar",
					SystemLanguage.Basque => "eu",
					SystemLanguage.Belarusian => "be",
					SystemLanguage.Bulgarian => "bg",
					SystemLanguage.Catalan => "ca",
					SystemLanguage.Chinese => "zh",
					SystemLanguage.Czech => "cs",
					SystemLanguage.Danish => "da",
					SystemLanguage.Dutch => "nl",
					SystemLanguage.English => "en",
					SystemLanguage.Estonian => "et",
					SystemLanguage.Faroese => "fo",
					SystemLanguage.Finnish => "fi",
					SystemLanguage.French => "fr",
					SystemLanguage.German => "de",
					SystemLanguage.Greek => "el",
					SystemLanguage.Hebrew => "he",
					SystemLanguage.Hungarian => "hu",
					SystemLanguage.Icelandic => "is",
					SystemLanguage.Indonesian => "id",
					SystemLanguage.Italian => "it",
					SystemLanguage.Japanese => "ja",
					SystemLanguage.Korean => "ko",
					SystemLanguage.Latvian => "lv",
					SystemLanguage.Lithuanian => "lt",
					SystemLanguage.Norwegian => "no",
					SystemLanguage.Polish => "pl",
					SystemLanguage.Portuguese => "pt",
					SystemLanguage.Romanian => "ro",
					SystemLanguage.Russian => "ru",
					SystemLanguage.Slovak => "sk",
					SystemLanguage.Slovenian => "sl",
					SystemLanguage.Spanish => "es",
					SystemLanguage.Swedish => "sv",
					SystemLanguage.Thai => "th",
					SystemLanguage.Turkish => "tr",
					SystemLanguage.Ukrainian => "uk",
					SystemLanguage.Vietnamese => "vo",
					_ => "en",
				};
			}
		}

		public static bool CheckCommandLine(string lineName) {
			string[] args = Environment.GetCommandLineArgs();

			foreach (string arg in args) {
				if (arg == lineName) return true;
			}

			return false;
		}

		public static bool CheckOpenProcess(string processName) {
			Process[] v = Process.GetProcessesByName(processName);

			return v.Length > 0;
		}

		public enum GameState {
			ALL_OK,
			WEAK_CPU,
			WEAK_GPU,
			WEAK_RAM,
			NO_ENOUGH_DISK_STORAGE,
			UNKWNOWN_DEVICE
		};

		public static GameState GetGameState() {
			if (SystemInfo.deviceType == DeviceType.Unknown) return GameState.UNKWNOWN_DEVICE;
			if (availableDiskSpace < 32f) return GameState.NO_ENOUGH_DISK_STORAGE;
			if (SystemInfo.processorCount < 2 || SystemInfo.processorFrequency < 1000) return GameState.WEAK_CPU;
			if (SystemInfo.systemMemorySize < 2048) return GameState.WEAK_RAM;
			if (SystemInfo.graphicsMemorySize < 1024) return GameState.WEAK_GPU;

			return GameState.ALL_OK;
		}
	}
}