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
			[Tooltip(
				"When enabled, the script will find automatically" +
				"some available selectable if selectable field is null."
			)]
			public bool useAutomatic;
			public Selectable selectable;
			public Vector3 addRotation;
		}
	}


	/// <summary>
	/// A set of constants for the game.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// The maximum world size in X and Y axis.
		/// </summary>
		public const float worldLimit = 1000f;

		/// <summary>
		/// The impulse force of trampolines.
		/// </summary>
		public const float trampolineForce = 27f;

		/// <summary>
		/// The maximum velocity of the player.
		/// </summary>
		public const float maxVelocity = 40f;

		/// <summary>
		/// A simple constant to convert pixels to units.
		/// </summary>
		public const float pixelToUnit = 1f / 16f;

		/// <summary>
		/// The maximum range of sprite sorting in Infinite and -Infinite.
		/// </summary>
		public const int orderLimit = 3200;

		/// <summary>
		/// The maximum chunk loading size.
		/// </summary>
		public const int chunkSize = 32;

		/// <summary>
		/// The hypotenuse of the world.
		/// </summary>
		public static float worldHypotenuse
		{
			get {
				float ww = worldLimit * worldLimit;

				return Mathf.Sqrt(ww * 2f);
			}
		}

		/// <summary>
		/// The reference resolution of the game.
		/// </summary>
		public static Vector2 referenceResolution
		{
			get => new(640f, 360f);
		}

		/// <summary>
		/// The default size of a level.
		/// </summary>
		public static Vector2 levelDefaultSize
		{
			get => new(21, 11);
		}

		/// <summary>
		/// A set of constants IDs.
		/// </summary>
		public static class InternalIDs
		{
			public const uint player = 1;
		}

		/// <summary>
		/// Converts static hexadecimal colors to Unity's Color class.
		/// </summary>
		public static class Colors
		{
			readonly static int __green = 0x52F540;
			readonly static int __orange = 0xFF8836;


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

	/// <summary>
	/// A set of global variables for the game.
	/// </summary>
	public static class Globals {
		#region Common static variables

		#region Boolean variables
		/// <summary>
		/// If true, the game is paused.
		/// </summary>
		public static bool onPause = true;

		/// <summary>
		/// Tells if the user is editing a field.
		/// </summary>
		public static bool onEditField = false;

		/// <summary>
		/// If true, the main menu should execute the intro animation.
		/// </summary>
		public static bool doIntro = true;

		/// <summary>
		/// Opens the settings menu.
		/// </summary>
		public static bool openSettings = false;

		/// <summary>
		/// If true, the game is loading a scene.
		/// </summary>
		public static bool onLoad = false;

		/// <summary>
		/// The player has got the coin?
		/// </summary>
		public static bool gotCoin = false;
		#endregion

		#region Unsigned integer variables
		/// <summary>
		/// The attempt counter.
		/// </summary>
		public static uint attempts = 0u;

		/// <summary>
		/// The respawn attempt counter.
		/// </summary>
		public static uint respawnAttempts = 0u;
		#endregion

		#region Float variables
		/// <summary>
		/// The game volume set by the game itself (for example transitions).
		/// </summary>
		public static float gmVolume = 1f;

		/// <summary>
		/// The camera's shake force.
		/// </summary>
		public static float shakeForce = 0f;

		/// <summary>
		/// The load progress of the scene or anything else. It's used by the loading screen.
		/// </summary>
		/// <remarks>
		/// The value is between 0 and 1.
		/// If the value is 0, the loading screen will display a generic loading bar.
		/// </remarks>
		public static float loadProgress = 0f;
		#endregion

		#region Integer variables
		/// <summary>
		/// The ID of the active checkpoint.
		/// </summary>
		public static int checkpointId = 0;

		/// <summary>
		/// The ID of the current selected button.
		/// </summary>
		public static int buttonSelected;

		/// <summary>
		/// The index of the main menu active section.
		/// </summary>
		public static int mainMenuSection = 0;
		#endregion

		#region String variables
		/// <summary>
		/// The whole credits text.
		/// </summary>
		/// <remarks>
		/// This variable is temporary and only loaded once.
		/// </remarks>
		public static string creditsText;

		/// <summary>
		/// A text display for the loading screen.
		/// </summary>
		public static string loadingText;
		#endregion

		#region Spaguetti variables
		/// <summary>
		/// All available player characters.
		/// </summary>
		public static PlayerCharacters[] playerCharacters;

		/// <summary>
		/// The whole settings object.
		/// </summary>
		public static Settings settings = new();

		/// <summary>
		/// The whole player data object.
		/// </summary>
		public static PlayerData playerData = new();

		/// <summary>
		/// The coordinates of the respawn point.
		/// </summary>
		public static Vector2 respawnPoint;

		/// <summary>
		/// What is the current input type?
		/// </summary>
		public static InputType inputType;

		/// <summary>
		/// All the available objects for the game.
		/// </summary>
		public static List<InGameObject> objects;

		/// <summary>
		/// The director object of the game.
		/// </summary>
		public static GameDirector director;

		/// <summary>
		/// All the available languages in the game.
		/// </summary>
		public static Settings.Languages languages;

		/// <summary>
		/// The current level's data (without game objects).
		/// </summary>
		public static Level levelData;
		#endregion


		/// <summary>
		/// Editor's only variables.
		/// </summary>
		public static class Editor
		{
			/// <summary>
			/// If true, all the objects will be snapped to the grid.
			/// </summary>
			public static bool snap = true;

			/// <summary>
			/// If true, all the selected objects transformations will be applied with their own pivot.
			/// </summary>
			public static bool handleLocally = false;

			/// <summary>
			/// The current's stage metadata.
			/// </summary>
			public static InternalUserScene currentScene;

			/// <summary>
			/// The current's level metadata.
			/// </summary>
			public static Level.UserMetadata currentLevel;

			/// <summary>
			/// The current's virtual mouse position.
			/// </summary>
			public static Vector2 virtualMousePosition;
		}
		#endregion

		#region Setters and Getters
		static bool __isDead = false;
		static bool __filesReady = false;

		static GameDirector.MusicClips.Type __musicType = GameDirector.MusicClips.Type.NONE;


		/// <summary>
		/// Tells the game that the files system is ready.
		/// </summary>
		/// <remarks>
		/// This method is called by the <see cref="GameDirector"/> class once.
		/// </remarks>
		public static void SetFilesReady()
		{
			__filesReady = true;
		}

		/// <summary>
		/// Is the files system ready?
		/// </summary>
		public static bool filesReady
		{
			get => __filesReady;
		}

		/// <summary>
		/// Is the player dead?
		/// </summary>
		public static bool isDead
		{
			get => __isDead;
			set {
				if (__isDead != value && value) {
					GameEventsHandler.InvokePlayerDeath();
					GeneralEventsHandler.SetShake(0.25f, 0.15f);
					// attempts++;
				}

				__isDead = value;
			}
		}

		/// <summary>
		/// The current music type.
		/// </summary>
		public static GameDirector.MusicClips.Type musicType
		{
			get => __musicType;
			set {
				if (value != __musicType) GeneralEventsHandler.ChangeMusic(value);
				__musicType = value;
			}
		}

		/// <summary>
		/// The current music volume.
		/// </summary>
		public static float musicVolume { get => settings.volume.music * settings.volume.master * gmVolume; }

		/// <summary>
		/// The current FX volume.
		/// </summary>
		public static float fxVolume { get => settings.volume.fx * settings.volume.master; }

		/// <summary>
		/// The client's screen size.
		/// </summary>
		public static Vector2 screen { get => new(Screen.width, Screen.height); }

		/// <summary>
		/// The client's screen rectangle.
		/// </summary>
		public static Rect screenRect { get => new(Vector2.zero, screen); }
		#endregion


		/// <summary>
		/// A set of settings for the game.
		/// </summary>
		[Serializable]
		public sealed class Settings
		{
			public General general;
			public Volume volume;
			public Editor editor;

			[Serializable] public sealed class Volume
			{
				[Range(0f, 1f)] public float music = 1f;
				[Range(0f, 1f)] public float fx = 1f;
				[Range(0f, 1f)] public float master = 1f;
			}

			[Serializable] public sealed class General
			{
				public string lang = "en";
				public float shakeStrenght = 0.5f;
				public Options options;

				[Flags]
				public enum Options {
					EnableDeviceVibration = 1 << 0,
					EnableControllerVibration = 1 << 1,
					EnableParticles = 1 << 2,
					EnableVSync = 1 << 3,
					EnableFullscreen = 1 << 4,
					DebugMode = 1 << 5,
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

			[Serializable] public sealed class Editor
			{
				/// <summary>
				/// The maximum size for the undo/redo history.
				/// </summary>
				public uint historialSize;

				/// <summary>
				/// The maximum points for the player's path.
				/// </summary>
				public uint lineLength = 500;

				/// <summary>
				/// A list of options for the editor.
				/// </summary>
				public Options options;

				/// <summary>
				/// The user-defined UI scale.
				/// </summary>
				public float uiScale = 1f;

				[Flags]
				public enum Options {
					UseCtrlToZoom = 1 << 0,
					UseCustomUIScale = 1 << 1,
					DisplayHelpLines = 1 << 2,
				}

				/// <summary>
				/// If true, the editor zoom will only be enabled when the user holds the Ctrl key.
				/// </summary>
				public bool useCtrlToZoom
				{
					get => (options & Options.UseCtrlToZoom) == Options.UseCtrlToZoom;
					set => options = (options & ~Options.UseCtrlToZoom) | (value ? Options.UseCtrlToZoom : 0);
				}

				/// <summary>
				/// If true, the editor will use a custom UI scale instead of the system's one.
				/// </summary>
				public bool useCustomUIScale
				{
					get => (options & Options.UseCustomUIScale) == Options.UseCustomUIScale;
					set => options = (options & ~Options.UseCustomUIScale) | (value ? Options.UseCustomUIScale : 0);
				}

				/// <summary>
				/// If true, the editor will display help lines.
				/// </summary>
				public bool displayHelpLines
				{
					get => (options & Options.DisplayHelpLines) == Options.DisplayHelpLines;
					set => options = (options & ~Options.DisplayHelpLines) | (value ? Options.DisplayHelpLines : 0);
				}
			}


			/// <summary>
			/// Saves the current settings to the disk.
			/// </summary>
			public async UniTask Save()
			{
				string json = await AsyncJson.ToJson(settings);
				await Files.WriteFile(Files.Directories.settings, json);
			}

			/// <summary>
			/// Controls the language settings.
			/// </summary>
			[Serializable] public class Languages
			{
				public List<Properties.Values> available = new();
				public List<Properties> properties = new();


				/// <summary>
				/// A set of properties for the language.
				/// </summary>
				[Serializable] public class Properties
				{
					[TextArea(1, 1)] public string field = "default.test";
					public List<Values> values = new();
	

					/// <summary>
					/// A set of values for the language.
					/// </summary>
					[Serializable] public class Values
					{
						public string code = "en";						
						[TextArea(1, 5)] public string value = "Example";
					}


					/// <summary>
					/// Gets the translation of a field.
					/// </summary>
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


				/// <summary>
				/// Gets the current language code.
				/// </summary>
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

				/// <summary>
				/// Gets the current language index.
				/// </summary>
				public int GetLanguageIndex(string code)
				{
					for (int i = 0; i < available.Count; i++) {
						if (available[i].code == code) {
							return i;
						}
					}

					return 0;
				}

				/// <summary>
				/// Gets the current language human-readable name.
				/// </summary>
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

				/// <summary>
				/// Gets the translation of a field.
				/// </summary>
				public string GetField(string fieldName)
				{
					Properties _prop = properties.Find(m => m.field.Trim() == fieldName.Trim());

					if (_prop != null) {
						return _prop.GetField(GetLang());
					}

					return "[LANG_FIELD_ERROR]";
				}

				/// <summary>
				/// Gets the translation of a field with arguments.
				/// </summary>
				public string GetField(string fieldName, params string[] args)
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
