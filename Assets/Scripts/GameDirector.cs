using System;
using System.Collections;
using System.Collections.Generic;
// using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using XInputDotNetPure;

using RobotoSkunk.PixelMan.Events;
using TMPro;



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
			public static Color green = new(0.32f, 0.96f, 0.25f);
		}
	}



	public static class Globals {
		public static bool onPause = true, onEditField = false, doIntro = true, openSettings = false;
		public static uint attempts = 0u, respawnAttempts = 0u;
		public static PlayerCharacters[] playerCharacters;
		public static Settings settings = new();
		public static PlayerData playerData = new();
		public static Settings.LangWrapper langWrapper;
		public static float gmVolume = 1f, shakeForce = 0f;
		public static Vector2 respawnPoint;
		public static int checkpointId = 0, buttonSelected, mainMenuSection = 0;
		public static InputType inputType;
		public static List<InGameObject> objects;
		public static GameDirector director;
		public static Dictionary<string, RSBehaviour> __behaviours = new();
		public static string creditsText;


		public static class Editor {
			public static bool snap = true, handleLocally = false;
		}

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

		[Serializable]
		public class Settings {
			public General general;
			public Volume volume;
			public Editor editor;

			[Serializable]
			public class Volume {
				public float music = 1f, fx = 1f, master = 1f;
			}
			[Serializable]
			public class General {
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
					EnableFullscreen = 1 << 4
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
			}
			[Serializable]
			public class Editor {
				public uint undoLimit, lineLength = 500;
			}

			[Serializable]
			public class Languages {
				public string name = "English", tag = "en";
				public List<Properties> properties = new();

				[Serializable]
				public class Properties {
					public string field = "default.test", value = "";
				}

				public string GetField(string fieldName) {
					Properties _prop = properties.Find(m => m.field == fieldName);
					if (_prop != null) return _prop.value;
					return null;
				}
				public string GetField(string fieldName, string[] args) {
					Properties _prop = properties.Find(m => m.field == fieldName);

					if (_prop != null) {
						string tmp = _prop.value;

						for (int i = 0; i < args.Length; i++)
							tmp = tmp.Replace($"{{{i}}}", args[i]);

						return tmp;
					}

					return null;
				}
			}

			[Serializable]
			public class LangWrapper {
				public List<Languages> languages;

				public string GetValue(string fieldName) {
					Languages _lang = languages.Find(m => m.tag == settings.general.lang);

					if (_lang != null) return _lang.GetField(fieldName);
					return languages[0].GetField(fieldName);
				}
				public string GetValue(string fieldName, params string[] args) {
					Languages _lang = languages.Find(m => m.tag == settings.general.lang);

					if (_lang != null) return _lang.GetField(fieldName, args);
					return languages[0].GetField(fieldName, args);
				}
				public string GetCurrentLangName() {
					Languages _lang = languages.Find(m => m.tag == settings.general.lang);

					if (_lang != null) return _lang.name;
					return languages[0].name;
				}
			}
		}

		[Serializable]
		public class PlayerData {
			public string displayName = "Player", accessToken = "";
			public Color color = Color.white;
			public uint skinIndex = 0u;
			public int fun = 0;
		}

		[Serializable]
		public struct PlayerCharacters {
			public string name;
			public Sprite display;
			public RuntimeAnimatorController controller;
		}
	}






	public class GameDirector : MonoBehaviour {
		[Header("Default properties")]
		public Globals.Settings settings;
		public Globals.PlayerData playerData;

		[Header("Game properties")]
		public float audioTransitionFactor = 0.25f;
		public Globals.PlayerCharacters[] playerCharacters;
		public List<InGameObject> objects;
		public List<MusicClips> musicClips;
		public Globals.Settings.LangWrapper languagesWrapper;

		[Header("Components")]
		public AudioSource bgAudio;
		public AudioSource musicAudio;

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
		readonly Timer t = new();
		Coroutine musicRoutine, shakeRoutine;
		Rect guiRect = new(15, 65, 200, 100);
		float openDelta = 1f;




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
			GameDirector _director = Instantiate(Resources.Load<GameDirector>("GameDirector"));
			_director.name = "GameDirector";
			Globals.director = _director;

			TextAsset _credits = Resources.Load<TextAsset>("Credits");
			Globals.creditsText = _credits.text;
		}


		private async void Awake() {
			// Set values
			Globals.objects = objects;
			Globals.playerCharacters = playerCharacters;
			Globals.langWrapper = languagesWrapper;

			Globals.settings = settings;
			Globals.playerData = playerData;
			Globals.settings.general.lang = Diagnostics.systemLanguage;

			// Clear memory
			settings = null;
			playerData = null;
			playerCharacters = null;
			languagesWrapper = null;

			#region Set settings to the UI
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
			langText.text = Globals.langWrapper.GetCurrentLangName();

			for (int i = 0; i < optionsToggles.Length; i++)
				if (i < _options.Length) optionsToggles[i].SetIsOnWithoutNotify(_options[i]);

			for (int i = 0; i < optionsSliders.Length; i++)
				if (i < _sliders.Length) optionsSliders[i].SetValueWithoutNotify(_sliders[i]);

			OpenPanel(0);
			#endregion


			// Suscribe events
			/*GameEventsHandler.PlayerDeath += () => {
				t.Stop();

				StartCoroutine(ResetObjects());
			};*/

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

			// Avoid destroy on scenes load
			DontDestroyOnLoad(gameObject);

			// Prepare directories
			try {
				await Files.Directories.Prepare();

				// foreach (var p in typeof(GameDirector).GetProperties()) {
				// 	Debug.Log($"{p.Name} | {p.PropertyType}");
				// }
			} catch (Exception e) {
				Debug.LogWarning(e);
			}
		}

		private async void Start() {
			await UniTask.Delay(500);
			GameEventsHandler.InvokeLevelReady();
			Globals.onPause = false;
			t.Start();
		}

		private void Update() {
			if (Keyboard.current.rKey.wasPressedThisFrame)
				GameEventsHandler.InvokeResetObject();

			if (Keyboard.current.kKey.wasPressedThisFrame)
				Globals.isDead = true;

			if (Keyboard.current.tKey.wasPressedThisFrame)
				Globals.onPause = !Globals.onPause;

			fps = RSTime.fps;

			openDelta = Mathf.Lerp(openDelta, (!Globals.openSettings).ToInt(), 0.2f);

			confPanels.SetActive(openDelta < 0.99f);
			confPanels[0].anchoredPosition = new(-openDelta * (confPanels[0].rect.width + 10), 0);
			confPanels[1].anchoredPosition = new(openDelta * (confPanels[1].rect.width + 10), 0);
		}

		private void FixedUpdate() {
			if (!onMusicFade) musicAudio.volume = Globals.musicVolume;
		}

		// private void OnGUI() {
		// 	GUI.Box(guiRect, "");
		// 	GUI.Label(guiRect, 
		// 		$"<b>Default frame rate:</b> {RSTime.fixedFrameRate}\n" +
		// 		$"<b>FPS:</b> {fps}\n" +
		// 		$"<b>Real FPS:</b> {RSTime.realFps}\n" +
		// 		$"<b>Timer</b>: {t.time}");
		// }

		void SetVibration(float leftMotor, float rightMotor) {
			if (Globals.settings.general.enableControllerVibration) {
				PlayerIndex[] enums = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));

				foreach (PlayerIndex index in enums)
					GamePad.SetVibration(index, leftMotor, rightMotor);
			}
		}


		#region Settings methods
		public void SetFullscreen(bool value) {
			Globals.settings.general.enableFullscreen = value;
			Screen.fullScreen = value;
		}
		public void SetVSync(bool value) {
			Globals.settings.general.enableVSync = value;
			QualitySettings.vSyncCount = value ? 1 : 0;
		}
		public void SetDeviceVibration(bool value) => Globals.settings.general.enableDeviceVibration = value;
		public void SetControllerVibration(bool value) => Globals.settings.general.enableControllerVibration = value;
		public void SetParticles(bool value) => Globals.settings.general.enableParticles = value;


		public void SetShakeStrenght(float value) => Globals.settings.general.shakeStrenght = value;
		public void SetMasterVolume(float value) => Globals.settings.volume.master = value;
		public void SetMusicVolume(float value) => Globals.settings.volume.music = value;
		public void SetFxVolume(float value) => Globals.settings.volume.fx = value;


		public void OpenPanel(int i) {
			for (int j = 0; j < confSections.Length; j++) confSections[j].SetActive(j == i);
			optionsScrollRect.content.anchoredPosition = new(optionsScrollRect.content.anchoredPosition.x, -50);
		}
		public void ToggleSettings(bool value) => Globals.openSettings = value;


		// public void SetLanguage(int i) {
		// 	Globals.settings.general.language = (Language)i;

		// 	Globals.langWrapper.languages

		// 	Globals.settings.general.language = (Language)i;
		// 	LocalizationManager.Instance.ChangeLanguage((Language)i);
		// }
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
			t.Reset();
			t.Start();

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
