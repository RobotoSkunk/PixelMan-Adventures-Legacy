using UnityEngine;

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

			public static void InvokeResetObject() => ResetObject();
			public static void InvokePlayerDeath() => PlayerDeath();
			public static void InvokeLevelReady() => LevelReady();
			public static void InvokeSwitchTouched() => SwitchTouched();
			public static void InvokeNewCheckpoint() => NewCheckpoint();
			public static void InvokeBackToCheckpoint() => BackToCheckpoint();
		}

		public static class GeneralEventsHandler {
			public delegate void AudioEvent(AudioClip clip);
			public delegate void MusicEvent(MainCore.MusicClips.Type type);
			public delegate void ShakeEvent(float force, float time);

			public static event AudioEvent PlayOnBG = delegate { };
			public static event MusicEvent ChgMusic = delegate { };
			public static event ShakeEvent ShakeFx = delegate { };

			public static void PlayOnBackground(AudioClip clip) => PlayOnBG(clip);
			public static void ChangeMusic(MainCore.MusicClips.Type type) => ChgMusic(type);
			public static void SetShake(float force, float time) => ShakeFx(force, time);
		}
	}

	public class GameHandler : MonoBehaviour {
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
			} else {
				Events.GameEventsHandler.ResetObject -= OnGameResetObject;
				Events.GameEventsHandler.PlayerDeath -= OnGamePlayerDeath;
				Events.GameEventsHandler.LevelReady -= OnGameReady;
				Events.GameEventsHandler.SwitchTouched -= OnGameSwitchTouched;
				Events.GameEventsHandler.NewCheckpoint -= OnGameCheckpointEnabled;
				Events.GameEventsHandler.BackToCheckpoint -= OnGameCheckpointRespawn;
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
	}
}
