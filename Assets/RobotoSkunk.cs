using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using System.IO.Compression;
using System.Diagnostics;
using System.Text;
using System.IO;
using System;


namespace RobotoSkunk {
	public static class Files {
		public static class Directories {
			private static string _root = null;

			public static string root {
				get {
					if (_root != null) return _root;

#if UNITY_ANDROID && !UNITY_EDITOR
					_root "/storage/emulated/0/Games/PixelMan Adventures";
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
					string home = Environment.GetEnvironmentVariable("HOME");
					_root = home + "/.local/share/PixelMan Adventures";
#else
					string folder = "/My Games/PixelMan Adventures", special = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

					if (special == "")
						special = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

					if (special == "")
						special = Environment.GetEnvironmentVariable("HOME") ?? Environment.GetEnvironmentVariable("HOMEPATH") ?? "";

					_root = special + folder;
#endif
					return _root;
				}
			}

			public static string userData = root + "/user-data.json", settings = root + "/settings.json";

			public static class User {
				public static string dir = root + "/user", levels = dir + "/levels", replays = dir + "/replays";
			}
			public static class Downloads {
				public static string dir = root + "/downloads", levels = dir + "/levels", cache = dir + "/cache";
			}

			public static async UniTask Prepare() {
				await UniTask.RunOnThreadPool(() => {
					if (!Directory.Exists(root)) Directory.CreateDirectory(root);

					if (!Directory.Exists(User.dir)) Directory.CreateDirectory(User.dir);
					if (!Directory.Exists(User.levels)) Directory.CreateDirectory(User.levels);
					if (!Directory.Exists(User.replays)) Directory.CreateDirectory(User.replays);

					if (!Directory.Exists(Downloads.dir)) Directory.CreateDirectory(Downloads.dir);
					if (!Directory.Exists(Downloads.levels)) Directory.CreateDirectory(Downloads.levels);
					if (!Directory.Exists(Downloads.cache)) Directory.CreateDirectory(Downloads.cache);

					if (!File.Exists(userData)) File.Create(userData);
					if (!File.Exists(settings)) File.Create(settings);
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

		public static async UniTask<string> ReadFile(string path) {
			await Directories.Prepare();

			StreamReader sr = new(path);
			string fileContent = await sr.ReadToEndAsync();
			sr.Close();

			return fileContent;
		}

		public static async UniTask WriteFile(string path, string data) {
			await Directories.Prepare();

			StreamWriter sw = new(path);
			await sw.WriteAsync(data);
			sw.Close();
		}

		public static async UniTask GetFilesAndDirectories(string path, List<FileInfo> files, List<DirectoryInfo> directories, Func<FileInfo, bool> fileFilter = null) {
			await Directories.Prepare();

			await UniTask.RunOnThreadPool(() => {
				DirectoryInfo dir = new(path);
				FileInfo[] __files = dir.GetFiles();
				DirectoryInfo[] __dirs = dir.GetDirectories();

				files.Clear();
				directories.Clear();

				foreach (DirectoryInfo directory in __dirs) {
					directories.Add(directory);
				}

				foreach (FileInfo file in __files) {
					if (fileFilter != null && !fileFilter(file)) continue;
					files.Add(file);
				}
			});
		}

		public static async UniTask<T> FromJson<T>(string data) {
			T __tmp = default;

			await UniTask.RunOnThreadPool(() => {
				__tmp = JsonUtility.FromJson<T>(data);
			});

			return __tmp;
		}
		
		public static async UniTask FromJsonOverwrite(string data, object objectToOverwrite) {
			await UniTask.RunOnThreadPool(() => {
				JsonUtility.FromJsonOverwrite(data, objectToOverwrite);
			});
		}

		public static async UniTask<string> ToJson(string json) {
			string __tmp = null;

			await UniTask.RunOnThreadPool(() => {
				__tmp = JsonUtility.ToJson(json);
			});

			return __tmp;
		}
	
		public static bool CheckIfDirectoryIsChildOf(DirectoryInfo parent, DirectoryInfo child) {
			bool isParent = false;

			while (child.Parent != null) {
				if (child.Parent.FullName == parent.FullName) {
					isParent = true;
					break;

				} else child = child.Parent;
			}

			return isParent;
		}

		public static async UniTask<string> ReadFileFromZip(string zipPath, string filePath) {
			await Directories.Prepare();
			ZipArchive archive = ZipFile.OpenRead(zipPath);

			ZipArchiveEntry entry = archive.GetEntry(filePath);
			if (entry == null) {
				archive.Dispose();
				return null;
			}

			StreamReader reader = new(entry.Open());
			string result = await reader.ReadToEndAsync();
			reader.Close();
			archive.Dispose();

			return result;
		}

		public static async UniTask<bool> WriteFileToZip(string zipPath, string filePath, string data) {
			await Directories.Prepare();
			ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

			ZipArchiveEntry entry = archive.GetEntry(filePath);
			if (entry == null) {
				entry = archive.CreateEntry(filePath);
			}

			StreamWriter writer = new(entry.Open());
			writer.BaseStream.SetLength(0);
			await writer.WriteAsync(data);
			writer.Close();
			archive.Dispose();

			return true;
		}

		public static async UniTask<bool> DeleteFileFromZip(string zipPath, string filePath) {
			await Directories.Prepare();
			ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

			ZipArchiveEntry entry = archive.GetEntry(filePath);
			if (entry == null) {
				archive.Dispose();
				return false;
			}

			entry.Delete();
			archive.Dispose();

			return true;
		}

		public static async UniTask<bool> CreateFileToZip(string zipPath, string filePath) {
			await Directories.Prepare();
			ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Update);

			ZipArchiveEntry entry = archive.GetEntry(filePath);
			if (entry != null) {
				archive.Dispose();
				return false;
			}

			archive.CreateEntry(filePath);
			archive.Dispose();

			return true;
		}
	}

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
				// else if (SystemInfo.deviceType == DeviceType.Handheld) Handheld.Vibrate();
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

	public static class AsyncJson {
		public static async UniTask<T> FromJson<T>(string json) {
			return await UniTask.RunOnThreadPool(() => JsonUtility.FromJson<T>(json));
		}

		public static async UniTask<string> ToJson<T>(T obj) {
			return await UniTask.RunOnThreadPool(() => JsonUtility.ToJson(obj));
		}

		public static async UniTask FromJsonOverwrite<T>(string json, T obj) {
			await UniTask.RunOnThreadPool(() => JsonUtility.FromJsonOverwrite(json, obj));
		}
	}



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

	public enum InputType {
		Gamepad,
		KeyboardAndMouse,
		Touch
	}
}
