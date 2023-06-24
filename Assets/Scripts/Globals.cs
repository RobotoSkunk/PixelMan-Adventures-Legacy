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
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using RobotoSkunk.PixelMan.Events;
using RobotoSkunk.PixelMan.LevelEditor;


namespace RobotoSkunk.PixelMan
{

	namespace UI
	{
		[Serializable]
		public struct IntelliNav
		{
			[Tooltip("When enabled, the script will find automatically some available selectable if selectable field is null.")]
			public bool useAutomatic;
			public Selectable selectable;
			public Vector3 addRotation;
		}
	}



	public static class Constants
	{
		public const float worldLimit = 1000f;
		public const float trampolineForce = 25f;
		public const float maxVelocity = 40f;
		public const float pixelToUnit = 1f / 16f;

		public const int orderLimit = 3200;
		public const int chunkSize = 32;


		public static float worldHypotenuse
		{
			get {
				float ww = worldLimit * worldLimit;

				return Mathf.Sqrt(ww * 2f);
			}
		}

		public static class InternalIDs
		{
			public const uint player = 1;
		}

		public static class Colors
		{
			readonly static int __green = 0x52F540, __orange = 0xFF8836;


			public static Color orange
			{
				get => new Color().FromInt(__orange);
			}

			public static Color green
			{
				get => new Color().FromInt(__green);
			}
		}
	}


	// Globals class be like: https://i.imgur.com/lclhxpR.jpeg
	public static class Globals {
		#region Common static variables
		/// <summary>
		/// If true, the game is paused.
		/// </summary>
		public static bool onPause = true;
		public static bool onEditField = false;
		public static bool doIntro = true;
		public static bool openSettings = false;
		public static bool onLoad = false;
		public static bool gotCoin = false;

		public static uint attempts = 0u;
		public static uint respawnAttempts = 0u;

		public static float gmVolume = 1f;
		public static float shakeForce = 0f;
		public static float loadProgress = 0f;

		public static int checkpointId = 0;
		public static int buttonSelected;
		public static int mainMenuSection = 0;

		public static string creditsText;
		public static string loadingText;


		public static PlayerCharacters[] playerCharacters;
		public static Settings settings = new();
		public static PlayerData playerData = new();

		public static Vector2 respawnPoint;
		public static InputType inputType;
		public static List<InGameObject> objects;
		public static GameDirector director;
		// public static Dictionary<string, RSBehaviour> __behaviours = new();

		public static Settings.Languages languages;


		public static class Editor
		{
			public static bool snap = true, handleLocally = false;

			public static InternalUserScene currentScene;
			public static Level.UserMetadata currentLevel;
		}
		#endregion

		#region Setters and Getters
		static bool __isDead = false;
		static bool __filesReady = false;

		static GameDirector.MusicClips.Type __musicType = GameDirector.MusicClips.Type.NONE;


		public static void SetFilesReady()
		{
			__filesReady = true;
		}

		public static bool filesReady
		{
			get => __filesReady;
		}


		public static bool isDead
		{
			get => __isDead;
			set {
				if (value && !__isDead) {
					GameEventsHandler.InvokePlayerDeath();
					GeneralEventsHandler.SetShake(0.25f, 0.15f);
					attempts++;
				}

				__isDead = value;
			}
		}

		public static GameDirector.MusicClips.Type musicType
		{
			get => __musicType;
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
		public class Settings
		{
			public General general;
			public Volume volume;
			public Editor editor;

			[Serializable] public class Volume
			{
				[Range(0f, 1f)]
				public float music = 1f, fx = 1f, master = 1f;
			}

			[Serializable] public class General
			{
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


				public bool enableDeviceVibration
				{
					get => (options & Options.EnableDeviceVibration) == Options.EnableDeviceVibration;
					set => options = (options & ~Options.EnableDeviceVibration) | (value ? Options.EnableDeviceVibration : 0);
				}

				public bool enableControllerVibration
				{
					get => (options & Options.EnableControllerVibration) == Options.EnableControllerVibration;
					set => options = (options & ~Options.EnableControllerVibration) | (value ? Options.EnableControllerVibration : 0);
				}

				public bool enableParticles
				{
					get => (options & Options.EnableParticles) == Options.EnableParticles;
					set => options = (options & ~Options.EnableParticles) | (value ? Options.EnableParticles : 0);
				}

				public bool enableVSync
				{
					get => (options & Options.EnableVSync) == Options.EnableVSync;
					set => options = (options & ~Options.EnableVSync) | (value ? Options.EnableVSync : 0);
				}

				public bool enableFullscreen
				{
					get => (options & Options.EnableFullscreen) == Options.EnableFullscreen;
					set => options = (options & ~Options.EnableFullscreen) | (value ? Options.EnableFullscreen : 0);
				}

				public bool debugMode
				{
					get => (options & Options.DebugMode) == Options.DebugMode;
					set => options = (options & ~Options.DebugMode) | (value ? Options.DebugMode : 0);
				}
			}

			[Serializable] public class Editor
			{
				public uint undoLimit, lineLength = 500;
			}

			public async UniTask Save()
			{
				string json = await AsyncJson.ToJson(settings);
				await Files.WriteFile(Files.Directories.settings, json);
			}


			[Serializable] public class Languages
			{
				[Serializable] public class Properties
				{
					[Serializable] public class Values
					{
						public string code = "en";						
						[TextArea(1, 5)] public string value = "Example";
					}

					[TextArea(1, 1)] public string field = "default.test";
					public List<Values> values = new();

					public string GetField(string code)
					{
						Values _val = values.Find(m => m.code == code);

						if (_val != null) {
							return _val.value;
						}


						string _defCode = code.Split('-')[0];
						Values _secondTry = values.Find(m => m.code.StartsWith(_defCode));

						if (_secondTry != null) {
							return _secondTry.value;
						}

						return "[LANG_DEF_ERROR]";
					}
				}

				public List<Properties.Values> available = new();
				public List<Properties> properties = new();

				string GetLang()
				{
					string _lang = "en";

					if (settings != null && settings.general != null) {
						_lang = settings.general.lang;
					}

					if (string.IsNullOrEmpty(_lang)) {
						_lang = "en";
					}

					return _lang;
				}

				public int GetLanguageIndex(string code)
				{
					for (int i = 0; i < available.Count; i++) {
						if (available[i].code == code) {
							return i;
						}
					}

					return 0;
				}

				public string GetCurrentLangName()
				{
					string _lang = GetLang();

					for (int i = 0; i < available.Count; i++) {
						if (available[i].code == _lang) {
							return available[i].value;
						}
					}

					return "English";
				}

				public string GetField(string fieldName)
				{
					Properties _prop = properties.Find(m => m.field.Trim() == fieldName.Trim());

					if (_prop != null) {
						return _prop.GetField(GetLang());
					}

					return "[LANG_FIELD_ERROR]";
				}

				public string GetField(string fieldName, string[] args)
				{
					Properties _prop = properties.Find(m => m.field == fieldName);

					if (_prop != null) {
						string tmp = _prop.GetField(GetLang());

						for (int i = 0; i < args.Length; i++) {
							tmp = tmp.Replace($"{{{i}}}", args[i]);
						}

						return tmp;
					}

					return "[LANG_ERROR]";
				}

			}
		}

		#region Non-Static classes and structs
		[Serializable]
		public class PlayerData
		{
			public string displayName = "Player";
			public string accessToken = "";

			public uint color = 0xFFFFFF;
			public uint skinIndex = 0u;

			public int fun = 0;


			public Color Color
			{
				get => new Color().FromInt((int)color);
				set => color = (uint)value.ToInt();
			}

			public async UniTask Save()
			{
				string json = await AsyncJson.ToJson(playerData);
				await Files.WriteFile(Files.Directories.userData, json);
			}
		}


		[Serializable]
		public struct PlayerCharacters
		{
			public string name;
			public Sprite display;
			public RuntimeAnimatorController controller;
		}
		#endregion
	}
}
