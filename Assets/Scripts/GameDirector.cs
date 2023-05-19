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

using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using RobotoSkunk.PixelMan.Events;
using RobotoSkunk.PixelMan.LevelEditor;

using Eflatun.SceneReference;

using TMPro;


using RobotoSkunk;


namespace RobotoSkunk.PixelMan {
	namespace UI {
		[Serializable]
		public struct IntelliNav {
			[Tooltip("When enabled, the script will find automatically some available selectable if selectable field is null.")]
			public bool useAutomatic;
			public Selectable selectable;
			public Vector3 addRotation;
		}
	}



	public static class Constants {
		public const float worldLimit = 1000f,
			trampolineForce = 25f,
			maxVelocity = 40f,
			pixelToUnit = 1f / 16f;
		public const int orderLimit = 3200;
		public const int chunkSize = 32;

		public static float worldHypotenuse {
			get {
				float ww = worldLimit * worldLimit;

				return Mathf.Sqrt(ww * 2f);
			}
		}

		public static class InternalIDs {
			public const uint player = 1;
		}
		public static class Colors {
			readonly static int __green = 0x52F540, __orange = 0xFF8836;

			public static Color orange {
				get => new Color().FromInt(__orange);
			}
			public static Color green {
				get => new Color().FromInt(__green);
			}
		}
	}


	// Globals class be like: https://i.imgur.com/lclhxpR.jpeg
	public static class Globals {
		#region Common static variables
		public static bool onPause = true, onEditField = false, doIntro = true, openSettings = false, onLoad = false, gotCoin = false;
		public static uint attempts = 0u, respawnAttempts = 0u;
		public static PlayerCharacters[] playerCharacters;
		public static Settings settings = new();
		public static PlayerData playerData = new();
		public static float gmVolume = 1f, shakeForce = 0f, loadProgress = 0f;
		public static Vector2 respawnPoint;
		public static int checkpointId = 0, buttonSelected, mainMenuSection = 0;
		public static InputType inputType;
		public static List<InGameObject> objects;
		public static GameDirector director;
		public static Dictionary<string, RSBehaviour> __behaviours = new();
		public static string creditsText, loadingText;

		public static Settings.Languages languages;


		public static class Editor {
			public static bool snap = true, handleLocally = false;

			public static InternalUserScene currentScene;
			public static Level.UserMetadata currentLevel;
		}
		#endregion

		#region Setters and Getters
		static bool __isDead = false;
		static GameDirector.MusicClips.Type __musicType = GameDirector.MusicClips.Type.NONE;

		public static bool isDead {
			get { return __isDead; }
			set {
				if (value && !__isDead) {
					GameEventsHandler.InvokePlayerDeath();
					GeneralEventsHandler.SetShake(0.25f, 0.15f);
					attempts++;
				}

				__isDead = value;
			}
		}

		public static GameDirector.MusicClips.Type musicType {
			get { return __musicType; }
			set {
				if (value != __musicType) GeneralEventsHandler.ChangeMusic(value);
				__musicType = value;
			}
		}


		public static float musicVolume { get => settings.volume.music * settings.volume.master * gmVolume; }
		public static float fxVolume { get => settings.volume.fx * settings.volume.master; }

		public static Vector2 screen { get => new(Screen.width, Screen.height); }
		public static Rect screenRect { get => new(Vector2.zero, screen); }
		#endregion


		[Serializable]
		public class Settings {
			public General general;
			public Volume volume;
			public Editor editor;

			[Serializable] public class Volume {
				[Range(0f, 1f)]
				public float music = 1f, fx = 1f, master = 1f;
			}
			[Serializable] public class General {
				public string lang = "en";
				public float shakeStrenght = 0.5f;
				public Options options;

				[Flags]
				public enum Options {
					None = 0,
					EnableDeviceVibration = 1 << 0,
					EnableControllerVibration = 1 << 1,
					EnableParticles = 1 << 2,
					EnableVSync = 1 << 3,
					EnableFullscreen = 1 << 4,
					DebugMode = 1 << 5
				}

				public bool enableDeviceVibration {
					get => (options & Options.EnableDeviceVibration) == Options.EnableDeviceVibration;
					set => options = (options & ~Options.EnableDeviceVibration) | (value ? Options.EnableDeviceVibration : 0);
				}
				public bool enableControllerVibration {
					get => (options & Options.EnableControllerVibration) == Options.EnableControllerVibration;
					set => options = (options & ~Options.EnableControllerVibration) | (value ? Options.EnableControllerVibration : 0);
				}
				public bool enableParticles {
					get => (options & Options.EnableParticles) == Options.EnableParticles;
					set => options = (options & ~Options.EnableParticles) | (value ? Options.EnableParticles : 0);
				}
				public bool enableVSync {
					get => (options & Options.EnableVSync) == Options.EnableVSync;
					set => options = (options & ~Options.EnableVSync) | (value ? Options.EnableVSync : 0);
				}
				public bool enableFullscreen {
					get => (options & Options.EnableFullscreen) == Options.EnableFullscreen;
					set => options = (options & ~Options.EnableFullscreen) | (value ? Options.EnableFullscreen : 0);
				}

				public bool debugMode {
					get => (options & Options.DebugMode) == Options.DebugMode;
					set => options = (options & ~Options.DebugMode) | (value ? Options.DebugMode : 0);
				}
			}
			[Serializable] public class Editor {
				public uint undoLimit, lineLength = 500;
			}

			public async UniTask Save() {
				string json = await AsyncJson.ToJson(settings);
				await Files.WriteFile(Files.Directories.settings, json);
			}


			[Serializable] public class Languages {
				[Serializable] public class Properties {
					[Serializable] public class Values {
						public string code = "en";						
						[TextArea(1, 5)] public string value = "Example";
					}

					[TextArea(1, 1)] public string field = "default.test";
					public List<Values> values = new();

					public string GetField(string code) {
						Values _val = values.Find(m => m.code == code);
						if (_val != null) return _val.value;

						string _defCode = code.Split('-')[0];
						Values _secondTry = values.Find(m => m.code.StartsWith(_defCode));

						if (_secondTry != null) return _secondTry.value;

						return "[LANG_DEF_ERROR]";
					}
				}

				public List<Properties.Values> available = new();
				public List<Properties> properties = new();

				string GetLang() {
					string _lang = "en";

					if (settings != null) {
						if (settings.general != null)
							_lang = settings.general.lang;
					}

					if (string.IsNullOrEmpty(_lang)) _lang = "en";
					return _lang;
				}

				public int GetLanguageIndex(string code) {
					for (int i = 0; i < available.Count; i++)
						if (available[i].code == code) return i;

					return 0;
				}
				public string GetCurrentLangName() {
					string _lang = GetLang();
					for (int i = 0; i < available.Count; i++)
						if (available[i].code == _lang) return available[i].value;

					return "English";
				}

				public string GetField(string fieldName) {
					Properties _prop = properties.Find(m => m.field.Trim() == fieldName.Trim());
					if (_prop != null) return _prop.GetField(GetLang());

					return "[LANG_FIELD_ERROR]";
				}
				public string GetField(string fieldName, string[] args) {
					Properties _prop = properties.Find(m => m.field == fieldName);

					if (_prop != null) {
						string tmp = _prop.GetField(GetLang());

						for (int i = 0; i < args.Length; i++)
							tmp = tmp.Replace($"{{{i}}}", args[i]);

						return tmp;
					}

					return "[LANG_ERROR]";
				}

			}
		}

		#region Non-Static classes and structs
		[Serializable]
		public class PlayerData {
			public string displayName = "Player", accessToken = "";
			// public Color color = Color.white;
			public uint color = 0xFFFFFF, skinIndex = 0u;
			public int fun = 0;

			public Color Color {
				get => new Color().FromInt((int)color);
				set => color = (uint)value.ToInt();
			}

			public async UniTask Save() {
				string json = await AsyncJson.ToJson(playerData);
				await Files.WriteFile(Files.Directories.userData, json);
			}
		}

		[Serializable]
		public struct PlayerCharacters {
			public string name;
			public Sprite display;
			public RuntimeAnimatorController controller;
		}
		#endregion
	}






	public class GameDirector : MonoBehaviour {
		#region Variables
		public bool testOnLoad = false;
		[Range(0f, 1f)] public float loadProgress = 0f;

		[Header("Default properties")]
		public Globals.Settings settings;
		public Globals.PlayerData playerData;

		[Header("Game properties")]
		public float audioTransitionFactor = 0.25f;
		public Globals.PlayerCharacters[] playerCharacters;
		public List<InGameObject> objects;
		public List<MusicClips> musicClips;
		public Globals.Settings.Languages languages;
		public Worlds[] worlds;

		[Header("Components")]
		public AudioSource bgAudio;
		public AudioSource musicAudio;
		public Canvas loadingCanvas;
		public RectTransform loadingBar;
		public RectTransform upperCover, lowerCover;
		public float loadingBarSpeed = 200f;
		public TextMeshProUGUI loadingText;

		// Follow up the order of the components in the inspector
		[Header("Configuration components")]
		public TextMeshProUGUI langText;
		public Toggle[] optionsToggles;
		public Slider[] optionsSliders;
		public RectTransform[] confPanels;
		public GameObject[] confSections;
		public ScrollRect optionsScrollRect;


		bool onMusicFade = false;
		int fps;
		Coroutine musicRoutine, shakeRoutine, saveRoutine;
		Rect guiRect = new(10, 10, 200, 100);
		float openDelta = 1f, waitDelta = 0f, loadDelta = 0f, coversDelta = 1f;
		#endregion




		[Serializable]
		public class MusicClips {
			public string name;
			public Type type;
			public AudioClip[] clips;

			public AudioClip GetClip() {
				if (clips.Length != 0) return clips[UnityEngine.Random.Range(0, clips.Length)];
				else return null;
			}

			public enum Type {
				NONE,
				IN_GAME,
				MAIN_MENU,
				EDITOR
			}
		}


		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		public static void OnRuntimeStart() {
			TextAsset _credits = Resources.Load<TextAsset>("Credits");
			Globals.creditsText = _credits.text;

			GameDirector _director = Instantiate(Resources.Load<GameDirector>("GameDirector"));
			_director.name = "GameDirector";
			Globals.director = _director;
		}


		private void Awake() {
			// Avoid destroy on scenes load
			DontDestroyOnLoad(gameObject);

			// Set values
			Globals.objects = objects;
			Globals.playerCharacters = playerCharacters;
			Globals.languages = languages;

			Globals.playerData = playerData;
			Globals.settings = settings;
			Globals.settings.general.lang = Diagnostics.systemLanguage;

			#region Parse Credits.txt
			Globals.creditsText = Globals.creditsText.Replace("\r", "");
			string[] _lines = Globals.creditsText.Split("\n");
			string __tmp = "", __lang = "", __sample = "";

			Globals.Settings.Languages.Properties _dictionary = new() {
				field = "menu.credits",
				values = new() {
					new() { code = "en", value = "" },
					new() { code = "es", value = "" },
					new() { code = "pt", value = "" }
				}
			};


			for (int i = 0; i < _lines.Length; i++) {
				if (_lines[i].StartsWith("#lang=")) __lang = _lines[i].Replace("#lang=", "");
				else if (_lines[i].StartsWith("#end")) {
					if (Globals.languages.available.Find(m => m.code == __lang) != null) {
						_dictionary.values.Find(m => m.code == __lang).value = __tmp;

						__sample = __tmp;
						__tmp = "";
					}
				} else __tmp += _lines[i] + "\n";
			}

			Globals.languages.properties.Add(_dictionary);
			#endregion

			#region Small easter egg
			Globals.languages.available.Add(new() { code = "mrrrr", value = "Mrrrrrr!" });

			for (int i = 0; i < Globals.languages.properties.Count; i++) {
				if (Globals.languages.properties[i].field == "menu.credits") {
					string _mrrrr = "";
					string[] _sampleLines = __sample.Split("\n");

					for (int j = 0; j < _sampleLines.Length; j++) {
						if (_sampleLines[j].StartsWith("<b>")) {
							_mrrrr += "<b>M" + new string('r', UnityEngine.Random.Range(10, 25)) + "</b>\n";
							continue;
						}

						if (_sampleLines[j].Length > 0) {
							_mrrrr += "M" + new string('r', UnityEngine.Random.Range(10, 25)) + "\n";
							continue;
						}

						_mrrrr += "\n";
					}


					Globals.languages.properties[i].values.Add(new() {
						code = "mrrrr",
						value = _mrrrr
					});

					continue;
				}

				Globals.languages.properties[i].values.Add(new() {
					code = "mrrrr",
					value = "M" + new string('r', UnityEngine.Random.Range(5, 10)) + "!"
				});
			}
			#endregion

			GeneralEventsHandler.SettingsLoaded += () => {
				SetLanguage(Globals.languages.GetLanguageIndex(Globals.settings.general.lang));
				SetFullScreenInternal(Globals.settings.general.enableFullscreen);
				SetVSyncInternal(Globals.settings.general.enableVSync);

				bool[] _options = {
					Globals.settings.general.enableFullscreen,
					Globals.settings.general.enableVSync,
					Globals.settings.general.enableDeviceVibration,
					Globals.settings.general.enableControllerVibration,
					Globals.settings.general.enableParticles
				};
				float[] _sliders = {
					Globals.settings.general.shakeStrenght,
					Globals.settings.volume.master,
					Globals.settings.volume.music,
					Globals.settings.volume.fx,
					Globals.settings.editor.undoLimit,
					Globals.settings.editor.lineLength
				};
				langText.text = Globals.languages.GetCurrentLangName();

				for (int i = 0; i < optionsToggles.Length; i++)
					if (i < _options.Length) optionsToggles[i].SetIsOnWithoutNotify(_options[i]);

				for (int i = 0; i < optionsSliders.Length; i++)
					if (i < _sliders.Length) optionsSliders[i].SetValueWithoutNotify(_sliders[i]);
			};
			OpenPanel(0);

			// Clear memory
			playerData = null;
			playerCharacters = null;
			languages = null;
			settings = null;
			Globals.creditsText = null;


			#region Suscribe events
			GeneralEventsHandler.PlayOnBG += (AudioClip clip) => bgAudio.PlayOneShot(clip);

			GeneralEventsHandler.ChgMusic += (MusicClips.Type type) => {
				MusicClips mc = musicClips.Find(m => m.type == type);

				if (mc != null) {
					if (musicRoutine != null) StopCoroutine(musicRoutine);
					musicRoutine = StartCoroutine(ChangeMusic(mc.GetClip()));
				}
			};

			GeneralEventsHandler.ShakeFx += (float __force, float __time) => {
				if (shakeRoutine != null) StopCoroutine(shakeRoutine);
					shakeRoutine = StartCoroutine(ShakeEffect(__force, __time));
			};

			GeneralEventsHandler.SceneChanged += (SceneReference scene) => {
				if (Globals.onLoad) return;
				if (!scene.IsSafeToUse) return;

				Globals.onLoad = true;
				Globals.loadProgress = 0f;
				loadDelta = waitDelta = 0f;

				int rnd = UnityEngine.Random.Range(1, 7);
				Globals.loadingText = Globals.languages.GetField("loading.phrase." + rnd, new string[] { Environment.UserName });

				UniTask.Void(async () => {
					await UniTask.Delay(TimeSpan.FromSeconds(0.5f));

					await SceneManager.LoadSceneAsync(scene.BuildIndex);
					Globals.onLoad = false;
				});
			};

			// GeneralEventsHandler.LangChanged += () => {
			// 	Debug.Log(Globals.languages.GetField("default.test.1"));
			// 	Debug.Log(Globals.languages.GetField("default.test.2", new string[1]{ "mrrrr" }));
			// };


			// Application.logMessageReceived += (string condition, string stackTrace, LogType type) => {
			// 	if (!Globals.settings.general.debugMode) return;

			// 	if (type == LogType.Error || type == LogType.Exception) {
					
			// 	}
			// };
			#endregion
		}

		private void Update() {
			fps = RSTime.fps;
			if (!onMusicFade) musicAudio.volume = Globals.musicVolume;

			openDelta = Mathf.Lerp(openDelta, (!Globals.openSettings).ToInt(), 0.2f * RSTime.delta);

			confPanels.SetActive(openDelta < 0.99f);
			confPanels[0].anchoredPosition = new(-openDelta * (confPanels[0].rect.width + 10), 0);
			confPanels[1].anchoredPosition = new(openDelta * (confPanels[1].rect.width + 10), 0);


			if (Globals.onLoad) {
				if (loadingText.text != Globals.loadingText) loadingText.text = Globals.loadingText;

				if (Globals.loadProgress == 0f) {
					waitDelta += Time.deltaTime * loadingBarSpeed;
					if (waitDelta >= 360f) waitDelta = 0f;

					float delta = Mathf.Sin(waitDelta * Mathf.Deg2Rad);
					loadingBar.anchoredPosition = new(delta * loadingBar.rect.size.x, 0);
				} else {
					loadDelta = Mathf.Lerp(loadDelta, Globals.loadProgress, 0.2f * RSTime.delta);
					loadingBar.anchoredPosition = new(-loadingBar.rect.size.x + loadDelta * loadingBar.rect.size.x, 0);
				}
			}

			coversDelta = Mathf.Lerp(coversDelta, (!Globals.onLoad).ToInt(), 0.25f * RSTime.delta);

			loadingCanvas.gameObject.SetActive(coversDelta < 0.99f);
			upperCover.anchoredPosition = 1.5f * new Vector2(0, upperCover.rect.height * coversDelta);
			lowerCover.anchoredPosition = 1.5f * new Vector2(0, -lowerCover.rect.height * coversDelta);
			
		}

		private void OnGUI() {
			if (!Globals.settings.general.debugMode) return;

			guiRect = GUI.Window(-1, guiRect, DebugWindow, "Debugger");
		}

		void DebugWindow(int id) {
			GUILayout.Label($"<b>FPS:</b> {fps}\n" +
				$"<b>Real FPS:</b> {RSTime.realFps}\n" +
				$"<b>Uptime</b>: {Time.realtimeSinceStartup}");
			GUI.DragWindow(new Rect(0, 0, Screen.width * 2, Screen.height * 2));
		}


		void SetVibration(float leftMotor, float rightMotor) {
			if (Globals.settings.general.enableControllerVibration) {
				foreach (Gamepad pad in Gamepad.all) {
					pad.SetMotorSpeeds(leftMotor, rightMotor);
				}
			}
		}


		#region Settings methods
		public void SetFullscreen(bool value) {
			Globals.settings.general.enableFullscreen = value;
			SetFullScreenInternal(value);
			SaveSettingsMiddleware();
		}
		public void SetVSync(bool value) {
			SetVSyncInternal(value);
			SaveSettingsMiddleware();
		}
		public void SetDeviceVibration(bool value) {
			Globals.settings.general.enableDeviceVibration = value;
			SaveSettingsMiddleware();
		}
		public void SetControllerVibration(bool value) {
			Globals.settings.general.enableControllerVibration = value;
			SaveSettingsMiddleware();
		}
		public void SetParticles(bool value) {
			Globals.settings.general.enableParticles = value;
			SaveSettingsMiddleware();
		}
		public void SetDebugMode(bool value) {
			Globals.settings.general.debugMode = value;
			SaveSettingsMiddleware();
		}


		public void SetShakeStrenght(float value) {
			Globals.settings.general.shakeStrenght = value;
			SaveSettingsMiddleware();
		}
		public void SetMasterVolume(float value) {
			Globals.settings.volume.master = value;
			SaveSettingsMiddleware();
		}
		public void SetMusicVolume(float value) {
			Globals.settings.volume.music = value;
			SaveSettingsMiddleware();
		}
		public void SetFxVolume(float value) {
			Globals.settings.volume.fx = value;
			SaveSettingsMiddleware();
		}


		public void OpenPanel(int i) {
			for (int j = 0; j < confSections.Length; j++) confSections[j].SetActive(j == i);
			optionsScrollRect.content.anchoredPosition = new(optionsScrollRect.content.anchoredPosition.x, -50);
		}
		public void ToggleSettings(bool value) => Globals.openSettings = value;


		public void SetLanguage(int i) {
			int current = Globals.languages.GetLanguageIndex(Globals.settings.general.lang);
			
			current += i;
			if (current < 0) current = Globals.languages.available.Count - 1;
			else if (current >= Globals.languages.available.Count) current = 0;

			Globals.settings.general.lang = Globals.languages.available[current].code;
			langText.text = Globals.languages.GetCurrentLangName();
			Events.GeneralEventsHandler.InvokeLanguageChanged();
		}

		public void SaveSettingsMiddleware() {
			if (saveRoutine != null) StopCoroutine(saveRoutine);
			saveRoutine = StartCoroutine(SaveSettings());
		}

		public void SetVSyncInternal(bool value) => QualitySettings.vSyncCount = value.ToInt();
		public void SetFullScreenInternal(bool value) {
			Screen.fullScreen = value;
			if (value) Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
		}
		#endregion



		#region Coroutines
		IEnumerator ShakeEffect(float __force, float __time) {
			Globals.shakeForce = Mathf.Clamp01(__force * Globals.settings.general.shakeStrenght);
			if (Globals.settings.general.enableDeviceVibration) Device.Vibrate((long)(__time * 1000f));
			SetVibration(__force, __force);

			yield return new WaitForSeconds(__time);

			SetVibration(0f, 0f);
			Globals.shakeForce = 0f;
		}

		IEnumerator ResetObjects() {
			float time = 1f;

			while (time > 0) {
				if (!Globals.onPause)
					time -= Time.fixedDeltaTime;

				yield return new WaitForFixedUpdate();
			}

			Globals.isDead = false;

			if (Globals.respawnAttempts > 0) GameEventsHandler.InvokeBackToCheckpoint();
			else GameEventsHandler.InvokeResetObject();
		}

		IEnumerator ChangeMusic(AudioClip __clip) {
			onMusicFade = true;

			if (musicAudio.clip != null) {
				while (musicAudio.volume >= 0.05f) {
					musicAudio.volume = Mathf.Lerp(musicAudio.volume, 0f, audioTransitionFactor);

					yield return new WaitForFixedUpdate();
				}
			}

			musicAudio.clip = __clip;
			musicAudio.time = 0f;

			if (musicAudio.clip != null) {
				musicAudio.Play();

				while (musicAudio.volume <= Globals.musicVolume - 0.05f) {
					musicAudio.volume = Mathf.Lerp(musicAudio.volume, Globals.musicVolume, audioTransitionFactor);

					yield return new WaitForFixedUpdate();
				}
			} 

			onMusicFade = false;
		}
		
		IEnumerator SaveSettings() {
			yield return new WaitForSeconds(0.5f);

			UniTask.Void(async () => {
				await Globals.settings.Save();

				saveRoutine = null;
			});
		}
		#endregion



		public void OnControlsChanged(PlayerInput obj) {
			Globals.inputType = obj.currentControlScheme switch {
				"Gamepad" => InputType.Gamepad,
				"Keyboard&Mouse" => InputType.KeyboardAndMouse,
				"Touch" => InputType.Touch,
				_ => InputType.KeyboardAndMouse,
			};

			Debug.Log(Globals.inputType);
		}
	}
}
