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

using System.Collections.Generic;

using UnityEngine;

using Eflatun.SceneReference;


namespace RobotoSkunk.PixelMan {
	namespace Events {
		public static class GameEventsHandler {
			public delegate void GameEvent();

			public static event GameEvent ResetObject = delegate { };
			public static event GameEvent PlayerDeath = delegate { };
			public static event GameEvent LevelReady = delegate { };
			public static event GameEvent SwitchTouched = delegate { };
			public static event GameEvent NewCheckpoint = delegate { };
			public static event GameEvent BackToCheckpoint = delegate { };
			public static event GameEvent PlayerWon = delegate { };

			public static void InvokeResetObject() => ResetObject();
			public static void InvokePlayerDeath() => PlayerDeath();
			public static void InvokeLevelReady() => LevelReady();
			public static void InvokeSwitchTouched() => SwitchTouched();
			public static void InvokeNewCheckpoint() => NewCheckpoint();
			public static void InvokeBackToCheckpoint() => BackToCheckpoint();
			public static void InvokePlayerWon() => PlayerWon();
		}

		public static class EditorEventsHandler {
			public delegate void EditorEvent();

			public static event EditorEvent StartTesting = delegate { };
			public static event EditorEvent EndTesting = delegate { };
			public static event EditorEvent OnReady = delegate { };

			public static void InvokeStartTesting() => StartTesting();
			public static void InvokeEndTesting() => EndTesting();
			public static void InvokeOnReady() => OnReady();
		}

		public static class GeneralEventsHandler {
			public delegate void Default();
			public delegate void AudioEvent(AudioClip clip);
			public delegate void MusicEvent(GameDirector.MusicClips.Type type);
			public delegate void ShakeEvent(float force, float time);
			public delegate void SceneEvent(SceneReference scene);

			public static event AudioEvent PlayOnBG = delegate { };
			public static event MusicEvent ChgMusic = delegate { };
			public static event ShakeEvent ShakeFx = delegate { };
			public static event Default LangChanged = delegate { };
			public static event SceneEvent SceneChanged = delegate { };
			public static event Default SettingsLoaded = delegate { };

			public static void PlayOnBackground(AudioClip clip) => PlayOnBG(clip);
			public static void ChangeMusic(GameDirector.MusicClips.Type type) => ChgMusic(type);
			public static void SetShake(float force, float time) => ShakeFx(force, time);
			public static void InvokeLanguageChanged() => LangChanged();
			public static void ChangeScene(SceneReference scene) => SceneChanged(scene);
			public static void InvokeSettingsLoaded() => SettingsLoaded();
		}

		public static class PhysicsEventsHandler {
			public delegate void PhysicsEvent();

			public static event PhysicsEvent CompGeo = delegate { };

			public static void GenerateCompositeGeometry() => CompGeo();
		}
	}

	public class GameObjectBehaviour : MonoBehaviour {
		private void OnEnable() => EnableEvents(true);
		private void OnDisable() => EnableEvents(false);
		private void OnDestroy() => EnableEvents(false);

		private void EnableEvents(bool enable) {
			if (enable) {
				Events.GameEventsHandler.ResetObject += OnGameResetObject;
				Events.GameEventsHandler.PlayerDeath += OnGamePlayerDeath;
				Events.GameEventsHandler.LevelReady += OnGameReady;
				Events.GameEventsHandler.SwitchTouched += OnGameSwitchTouched;
				Events.GameEventsHandler.NewCheckpoint += OnGameCheckpointEnabled;
				Events.GameEventsHandler.BackToCheckpoint += OnGameCheckpointRespawn;
				Events.GameEventsHandler.PlayerWon += OnGamePlayerWon;
			} else {
				Events.GameEventsHandler.ResetObject -= OnGameResetObject;
				Events.GameEventsHandler.PlayerDeath -= OnGamePlayerDeath;
				Events.GameEventsHandler.LevelReady -= OnGameReady;
				Events.GameEventsHandler.SwitchTouched -= OnGameSwitchTouched;
				Events.GameEventsHandler.NewCheckpoint -= OnGameCheckpointEnabled;
				Events.GameEventsHandler.BackToCheckpoint -= OnGameCheckpointRespawn;
				Events.GameEventsHandler.PlayerWon -= OnGamePlayerWon;
			}
		}

		/// <summary>
		/// Is called when a game level is fully loaded.
		/// </summary>
		protected virtual void OnGameReady() { }

		/// <summary>
		/// Is called when the object needs to be restarted.
		/// </summary>
		protected virtual void OnGameResetObject() { }

		/// <summary>
		/// Is called when a player dies in game.
		/// </summary>
		protected virtual void OnGamePlayerDeath() { }

		/// <summary>
		/// Is called when a switch was turned on.
		/// </summary>
		protected virtual void OnGameSwitchTouched() { }

		/// <summary>
		/// Is called when the player collides with a new checkpoint.
		/// </summary>
		protected virtual void OnGameCheckpointEnabled() { }

		/// <summary>
		/// Is called when the player dies and can back to some checkpoint.
		/// </summary>
		protected virtual void OnGameCheckpointRespawn() { }

		/// <summary>
		/// Is called when the player wins the level.
		/// </summary>
		protected virtual void OnGamePlayerWon() { }
	}

	public class GameObjectBehaviourExtended : GameObjectBehaviour {
		protected readonly List<GameObject> players = new();

		protected override void OnGameReady() {
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
			players.Clear();

			foreach (GameObject g in gameObjects) players.Add(g);
		}


		protected GameObject NearestPlayer() {
			GameObject target = null;
			float distance = Constants.worldLimit;

			foreach (GameObject player in players) {
				float playerDistance = Vector2.Distance(transform.position, player.transform.position);

				if (playerDistance < distance) {
					target = player;
					distance = playerDistance;
				}
			}

			return target;
		}
	}


	public class EditorHandler : MonoBehaviour {
		private void OnEnable() => EnableEvents(true);
		private void OnDisable() => EnableEvents(false);
		private void OnDestroy() => EnableEvents(false);

		private void EnableEvents(bool enable) {
			if (enable) {
				Events.EditorEventsHandler.StartTesting += OnStartTest;
				Events.EditorEventsHandler.EndTesting += OnEndTest;
				Events.EditorEventsHandler.OnReady += OnEditorReady;
			} else {
				Events.EditorEventsHandler.StartTesting -= OnStartTest;
				Events.EditorEventsHandler.EndTesting -= OnEndTest;
				Events.EditorEventsHandler.OnReady += OnEditorReady;
			}
		}

		/// <summary>
		/// Is called when the editor starts testing a level.
		/// </summary>
		protected virtual void OnStartTest() { }

		/// <summary>
		/// Is called when the editor ends testing a level.
		/// </summary>
		protected virtual void OnEndTest() { }

		/// <summary>
		/// Is called when the editor loaded a level.
		/// </summary>
		protected virtual void OnEditorReady() { }
	}

	public class PhysicsHandler : MonoBehaviour {
		private void OnEnable() => EnableEvents(true);
		private void OnDisable() => EnableEvents(false);
		private void OnDestroy() => EnableEvents(false);

		private void EnableEvents(bool enable) {
			if (enable) {
				Events.PhysicsEventsHandler.CompGeo += OnGenerateCompositeGeometry;
			} else {
				Events.PhysicsEventsHandler.CompGeo -= OnGenerateCompositeGeometry;
			}
		}

		/// <summary>
		/// Is called when the editor starts testing a level.
		/// </summary>
		protected virtual void OnGenerateCompositeGeometry() { }
	}
}
