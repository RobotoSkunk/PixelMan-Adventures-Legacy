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

using Cysharp.Threading.Tasks;

using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

using RobotoSkunk.PixelMan.Utils;
using RobotoSkunk.PixelMan.Events;
using RobotoSkunk.PixelMan.LevelEditor;
using RobotoSkunk.PixelMan.LevelEditor.IO;


namespace RobotoSkunk.PixelMan.Gameplay
{
	public class GameController : GameObjectBehaviour
	{
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[Header("Components")]
		[SerializeField] PlayerCamera playerCamera;
		[SerializeField] LevelIO.TransformContainers transformContainers;
		[SerializeField] SceneReferenceHandler mainMenuSceneHandler;
		[SerializeField] RectTransform levelBounds;

		[Header("UI")]
		[SerializeField] RectTransform[] pauseMenuPanels;
		#pragma warning restore IDE0044

		float pauseMenuDelta = 1f;

		bool pauseMenuOpen {
			get {
				return Globals.onPause && !Globals.openSettings;
			}
		}


		private void Awake()
		{
			Globals.onPause = true;
			Globals.onLoad = true;
			Globals.isDead = false;
			Globals.respawnAttempts = 0;

			Globals.loadingText = Globals.languages.GetField("loading.load_objects");
			Globals.musicType = GameDirector.MusicClips.Type.IN_GAME;

			if (Globals.Editor.currentScene.file != null) {
				UniTask.Void(async () =>
				{
					await LevelIO.LoadLevel(false, transformContainers);

					levelBounds.sizeDelta = Globals.levelData.bounds.size;
					levelBounds.transform.position = Globals.levelData.bounds.center;

					PrepareLevel();
				});
			} else {
				PrepareLevel();
			}
		}

		private void Update()
		{
			pauseMenuDelta = Mathf.Lerp(pauseMenuDelta, (!pauseMenuOpen).ToInt(), 0.2f * RSTime.delta);

			pauseMenuPanels.SetActive(pauseMenuDelta < 0.99f);
			pauseMenuPanels[0].anchoredPosition = new(-pauseMenuDelta * (pauseMenuPanels[0].rect.width + 10), 0);
			pauseMenuPanels[1].anchoredPosition = new(pauseMenuDelta * (pauseMenuPanels[1].rect.width + 10), 0);
		}


		/// <summary>
		/// Prepares the level setting up the last details.
		/// </summary>
		/// <returns>True if the level is ready to be played.</returns>
		void PrepareLevel()
		{
			GameObject player = GameObject.FindWithTag("Player");

			if (player) {
				Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
				Player playerScript = player.GetComponent<Player>();

				playerCamera.SetPlayer(playerBody);
				playerScript.SetCamera(playerCamera);

				GameEventsHandler.InvokeLevelReady();
				Globals.onPause = false;
			} else {
				mainMenuSceneHandler.GoToScene();
			}


			Globals.onLoad = false;
		}


		public void SetPauseState(bool state)
		{
			Globals.onPause = state;
		}

		public void OpenSettings()
		{
			Globals.openSettings = true;
		}

		public void RestartLevel()
		{
			ResetObjects(true);
			SetPauseState(false);
		}

		public void Pause(InputAction.CallbackContext context)
		{
			if (context.started && !Globals.openSettings) {
				Globals.onPause = !Globals.onPause;
			}
		}

		protected override void OnGamePlayerDeath()
		{
			StartCoroutine(ResetObjectsCoroutine());
		}

		void ResetObjects(bool wholeLevel = false)
		{
			if (Globals.respawnAttempts > 0 && !wholeLevel) {
				GameEventsHandler.InvokeBackToCheckpoint();
			} else {
				Globals.attempts++;
				GameEventsHandler.InvokeResetObject();
			}

			Globals.isDead = false;
		}


		IEnumerator ResetObjectsCoroutine() {
			yield return new WaitForSeconds(1f);

			ResetObjects(false);
		}
	}
}
