using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using XInputDotNetPure;

using RobotoSkunk.PixelMan.Events;



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
		public static bool onPause = true;
		public static uint attempts = 0u, respawnAttempts = 0u;
		public static PlayerCharacters[] playerCharacters;
		public static Settings settings = new();
		public static PlayerData playerData = new();
		public static Settings.LangWrapper langWrapper;
		public static float gmVolume = 1f, shakeForce = 0f;
		public static Vector2 respawnPoint;
		public static int checkpointId = 0, buttonSelected;
		public static InputType inputType;
		public static List<InGameObject> objects;

		public static class Editor {
			public static Vector2 cursorPos, virtualCursor;
			public static bool hoverUI, curInWindow, snap = true, onSubmit, onDelete;
		}

		static bool __isDead = false;
		static MainCore.MusicClips.Type __musicType = MainCore.MusicClips.Type.NONE;

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

		public static MainCore.MusicClips.Type musicType {
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
				public bool enableShake = true, enableDeviceVibration = true, enableControllerVibration = true, enableParticles = true;
				public string lang = "en";
			}
			[Serializable]
			public class Editor {
				public uint undoLimit;
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

	public class MainCore : MonoBehaviour {
		[Header("Startup properties")]
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

		bool onMusicFade = false;
		int fps;
		readonly Timer t = new();
		Coroutine musicRoutine, shakeRoutine;
		Rect guiRect = new(15, 65, 200, 100);

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

		private async void Awake() {
			// Set values
			Globals.objects = objects;
			Globals.playerCharacters = playerCharacters;
			Globals.settings = settings;
			Globals.playerData = playerData;
			Globals.langWrapper = languagesWrapper;
			Globals.settings.general.lang = Diagnostics.systemLanguage;

			// Clear memory
			settings = null;
			playerData = null;
			playerCharacters = null;
			languagesWrapper = null;

			// Suscribe events
			GameEventsHandler.PlayerDeath += () => {
				t.Stop();

				StartCoroutine(ResetObjects());
			};

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

				string langs = await Files.ReadFile($"{Files.Directories.root}/languages.json");

				if (!string.IsNullOrEmpty(langs)) {
					Globals.langWrapper = await Files.FromJson<Globals.Settings.LangWrapper>(langs);
				}
			} catch (Exception e) {
				Debug.LogWarning(e);
			}
		}

		private async void Start() {
			await Task.Delay(500);
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
			if (Globals.settings.general.enableDeviceVibration && leftMotor + rightMotor != 0f) Device.Vibrate(150);

			if (Globals.settings.general.enableControllerVibration) {
				PlayerIndex[] enums = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));

				foreach (PlayerIndex index in enums)
					GamePad.SetVibration(index, leftMotor, rightMotor);
			}
		}

		#region Coroutines
		IEnumerator ShakeEffect(float __force, float __time) {
			if (Globals.settings.general.enableShake) Globals.shakeForce = Mathf.Clamp01(__force);
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
