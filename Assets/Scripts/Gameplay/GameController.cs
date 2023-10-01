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

using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

using RobotoSkunk.PixelMan.Utils;
using RobotoSkunk.PixelMan.Events;
using RobotoSkunk.PixelMan.LevelEditor.IO;

using TMPro;
using Eflatun.SceneReference;
using System.Linq;
using RobotoSkunk.PixelMan.LevelEditor;

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
		[SerializeField] GameObject victoryUI;
		[SerializeField] GameObject victoryContainer;
		[SerializeField] RectTransform victoryLeftPanel;
		[SerializeField] RectTransform victoryRightPanel;
		[SerializeField] TextMeshProUGUI victoryPhrase;
		[SerializeField] TextMeshProUGUI attemptsText;
		[SerializeField] TextMeshProUGUI timeText;
		[SerializeField] Image[] achievement = new Image[3];

		[Header("Level Editor")]
		[SerializeField] GameObject goToEditorButton;
		[SerializeField] Image nextLevelImage;
		[SerializeField] Image goToEditorImage;

		[Header("Properties")]
		[SerializeField] SceneReference selfScene;
		[SerializeField] SceneReference levelEditorScene;
		[SerializeField] SceneReference menuScene;
		#pragma warning restore IDE0044

		readonly Timer timer = new();

		float pauseMenuDelta = 1f;
		bool victoryPanelOpen = false;
		bool loadingNextLevel = false;
		uint attempts = 1u;


		Coroutine victoryPanelCoroutine;


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
			Globals.gotCoin = false;
			Globals.respawnAttempts = 0;

			victoryUI.SetActive(false);
			goToEditorButton.SetActive(!Globals.levelIsBuiltIn);

			nextLevelImage.gameObject.SetActive(Globals.levelIsBuiltIn);
			goToEditorImage.gameObject.SetActive(!Globals.levelIsBuiltIn);


			Globals.loadingText = Globals.languages.GetField("loading.load_objects");
			Globals.musicType = GameDirector.MusicClips.Type.IN_GAME;


			// Load level
			if (Globals.Editor.currentScene.file != null || Globals.levelIsBuiltIn) {
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


			if (Keyboard.current?.escapeKey.wasPressedThisFrame == true && !Globals.openSettings) {
				Globals.onPause = !Globals.onPause;
			}


			if (victoryPanelCoroutine == null) {
				timer.SetActive(!Globals.onPause);

				return;
			}


			float deltaSize = victoryPanelOpen ? 150 : 0;

			float newMax = Mathf.Lerp(victoryLeftPanel.offsetMax.x, -deltaSize, 0.4f * RSTime.delta);
			victoryLeftPanel.offsetMax = new(newMax, victoryLeftPanel.offsetMax.y);

			float newMin = Mathf.Lerp(victoryRightPanel.offsetMin.x, deltaSize, 0.4f * RSTime.delta);
			victoryRightPanel.offsetMin = new(newMin, victoryRightPanel.offsetMin.y);
		}


		/// <summary>
		/// Prepares the level setting up the last details.
		/// </summary>
		/// <returns>True if the level is ready to be played.</returns>
		void PrepareLevel()
		{
			GameObject player = GameObject.FindWithTag("Player");

			if (player) {
				Player playerScript = player.GetComponent<Player>();

				playerCamera.SetPlayer(playerScript);
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

		public void RestartLevel(bool fromScratch = false)
		{
			ResetObjects(true);
			SetPauseState(false);

			victoryUI.SetActive(false);

			timer.Reset();

			if (victoryPanelCoroutine != null) {
				StopCoroutine(victoryPanelCoroutine);
				victoryPanelCoroutine = null;
			}

			if (fromScratch) {
				attempts = 1u;
			}
		}

		public void Pause(InputAction.CallbackContext context)
		{
			if (context.started && !Globals.openSettings) {
				Globals.onPause = !Globals.onPause;
			}
		}

		public void GoToNext()
		{
			if (loadingNextLevel) {
				return;
			}

			loadingNextLevel = true;

			if (Globals.levelIsBuiltIn) {
				Globals.levelIndex++;

				if (Globals.levelIndex >= Globals.currentGameScene.levels.Count())
				{
					GeneralEventsHandler.ChangeScene(menuScene);
					return;
				}
			}

			GeneralEventsHandler.ChangeScene(Globals.levelIsBuiltIn ? selfScene : levelEditorScene);
		}

		protected override void OnGamePlayerDeath()
		{
			StartCoroutine(ResetObjectsCoroutine());
		}

		protected override void OnGamePlayerWon()
		{
			if (victoryPanelCoroutine != null) {
				return;
			}

			victoryPanelCoroutine = StartCoroutine(VictoryPanelCoroutine());
		}

		void ResetObjects(bool wholeLevel = false)
		{
			if (Globals.respawnAttempts > 0 && !wholeLevel) {
				GameEventsHandler.InvokeBackToCheckpoint();
			} else {
				attempts++;
				GameEventsHandler.InvokeResetObject();

				// This looks redundant, but it's not.
				Globals.respawnAttempts = 0;
			}

			Globals.isDead = false;
		}


		IEnumerator ResetObjectsCoroutine()
		{
			yield return new WaitForSeconds(1f);

			ResetObjects(false);
		}

		IEnumerator VictoryPanelCoroutine()
		{
			float halfScreenWidth = Globals.screen.x / 2;

			victoryLeftPanel.offsetMax = new(-halfScreenWidth, victoryLeftPanel.offsetMax.y);
			victoryRightPanel.offsetMin = new(halfScreenWidth, victoryRightPanel.offsetMin.y);

			victoryPanelOpen = false;
			victoryContainer.SetActive(false);

			timer.Stop();


			List<string> phrases = new() {
				"That wasn't so hard, was it?",
				"Good job!",
				"You made it!",
				"WOOO HOOO!",
				"*insert obligatory victory phrase here*",
				"I'm lazy to translate this, I'll do it later!",
			};

			if (attempts == 1) {
				phrases = new() {
					"At first try!",
					"Well, that was easy!",
					"Wow, you're good!",
				};
			} else if (attempts > 10) {
				phrases = new() {
					"Uhm... sure...",
					"Maybe you should try another game?",
					"It really was that hard?",
					"Oh, come on, you can do better than that!",
					"Are you sure you're not cheating?",
					"It should be easier than Give Up...",
				};
			}

			victoryPhrase.text = phrases[Random.Range(0, phrases.Count)];
			attemptsText.text = "x" + attempts.ToString();
			timeText.text = timer.ToString();


			// Achievements
			achievement[0].color = attempts == 1 ? Color.white : Color.gray;
			achievement[1].color = Globals.gotCoin ? Color.white : Color.gray;
			achievement[2].color = timer.time < 60 ? Color.white : Color.gray;


			victoryUI.SetActive(true);

			yield return new WaitForSeconds(0.25f);

			victoryPanelOpen = true;
			victoryContainer.SetActive(true);
		}
	}
}
