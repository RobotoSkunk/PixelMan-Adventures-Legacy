using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using System.IO;
using System;

namespace RobotoSkunk {
	public static class Files {
		public static class Directories {
			public static string root {
				get {
#if UNITY_ANDROID && !UNITY_EDITOR
					return "/storage/emulated/0/RobotoSkunk/PixelMan Adventures";
#else
					string folder = "/RobotoSkunk/PixelMan Adventures", special = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

					if (special == "")
						special = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

					if (special == "")
						special = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("HOMEPATH") ?? "";

					return special + folder;
#endif
				}
			}

			public static string userData = $"{root}/user-data.json", settings = $"{root}/settings.json";

			public static class User {
				public static string dir = $"{root}/user", levels = $"{dir}/levels", replays = $"{dir}/replays";
			}
			public static class Downloads {
				public static string dir = $"{root}/downloads", levels = $"{dir}/levels", cache = $"{dir}/cache";
			}

			public static async Task Prepare() {
				await Task.Run(() => {
					Directory.CreateDirectory($"{root}/");

					Directory.CreateDirectory($"{User.dir}/");
					Directory.CreateDirectory($"{User.levels}/");
					Directory.CreateDirectory($"{User.replays}/");

					Directory.CreateDirectory($"{Downloads.dir}/");
					Directory.CreateDirectory($"{Downloads.levels}/");
					Directory.CreateDirectory($"{Downloads.cache}/");

					File.Create(userData);
					File.Create(settings);
				});
			}
		}

		public static string B64ToStr(string data) {
			byte[] bits = Convert.FromBase64String(data);
			string result = Encoding.ASCII.GetString(bits);
			return result;
		}

		public static string StrToB64(string data) {
			byte[] bits = Encoding.ASCII.GetBytes(data);
			string result = Convert.ToBase64String(bits);
			return result;
		}

		public static async Task<string> ReadFile(string path) {
			await Directories.Prepare();

			StreamReader sr = new StreamReader(path);
			string fileContent = await sr.ReadToEndAsync();
			sr.Close();

			return fileContent;
		}

		public static async Task WriteFile(string path, string data) {
			await Directories.Prepare();

			StreamWriter sw = new StreamWriter(path);
			await sw.WriteAsync(data);
			sw.Close();
		}

		public static async Task<T> FromJson<T>(string data) {
			T __tmp = default;

			await Task.Run(() => {
				__tmp = JsonUtility.FromJson<T>(data);
			});

			return __tmp;
		}
		
		public static async Task FromJsonOverwrite(string data, object objectToOverwrite) {
			await Task.Run(() => {
				JsonUtility.FromJsonOverwrite(data, objectToOverwrite);
			});
		}

		public static async Task<string> ToJson(string json) {
			string __tmp = null;

			await Task.Run(() => {
				__tmp = JsonUtility.ToJson(json);
			});

			return __tmp;
		}
	}

	public static class RSTime {
		private static double __lastTime;
		private static int __currFps, __fpsFactor;

		public static int fixedFrameCount {
			get => Mathf.RoundToInt(Time.fixedTime / Time.fixedDeltaTime);
		}

		public static float delta {
			get => Time.deltaTime / Time.fixedDeltaTime;
		}

		public static float fixedFrameRate {
			get {
				return 1f / Time.fixedDeltaTime;
			}
		}

		public static int fps {
			get {
				double currTime = Time.realtimeSinceStartupAsDouble,
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
				return fixedFrameRate / delta;
			}
		}
	}

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
				else if (SystemInfo.deviceType == DeviceType.Handheld) Handheld.Vibrate();
			} catch (Exception err) {
				UnityEngine.Debug.LogWarning(err);
			}
		}
	}

	public static class RSMath {
		public static float Lengthdir_x(float x, float ang) => Mathf.Cos(ang) * x;
		public static float Lengthdir_y(float x, float ang) => Mathf.Sin(ang) * x;

		public static Vector3 GetDirVector(float angle) => new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

		public static float SafeDivision(float numerator, float denominator) => denominator == 0f ? 0f : numerator / denominator;

		public static float Direction(Vector2 from, Vector2 to) => Mathf.Atan2(to.y - from.y, to.x - from.x);

		public static int ToInt(this bool b) => b ? 1 : 0;

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
	}

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
	}



	public class Timer {
		private double __timer, __timerBuffer, __lastTime;
		private bool __onTick;

		public double time {
			get {
				if (isActive) __timer = __timerBuffer + Time.realtimeSinceStartupAsDouble - __lastTime;

				return __timer;
			}
			set {
				__timerBuffer = value;
				__lastTime = Time.realtimeSinceStartupAsDouble;
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
				__lastTime = Time.realtimeSinceStartupAsDouble;
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

	public enum InputType {
		Gamepad,
		KeyboardAndMouse,
		Touch
	}
}
