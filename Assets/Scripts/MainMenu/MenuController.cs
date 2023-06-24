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

using System.Collections;

using UnityEngine;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	[System.Serializable]
	public class Menu {
		public string name;
		public MenuPart[] parts;
		public GameObject[] controllers;
		readonly float minDist = 1.5f;


		public void SetActive(bool active) {
			for (int i = 0; i < parts.Length; i++) parts[i].isOpen = active;
			for (int i = 0; i < controllers.Length; i++) controllers[i].SetActive(active);
		}

		public void SetPositions(bool ignoreDelta = false) {
			for (int i = 0; i < parts.Length; i++) {
				parts[i].rectTransform.anchoredPosition = Vector2.Lerp(parts[i].rectTransform.anchoredPosition, parts[i].nextPos, ignoreDelta ? 1f : RSTime.delta * parts[i].delta);

				Vector2 diff = parts[i].rectTransform.anchoredPosition - (parts[i].startPos + parts[i].positionOnClosed);

				bool setActive = diff.sqrMagnitude > minDist * minDist;
				parts[i].gameObject.SetActive(setActive);
			}
		}
	}

	public class MenuController : MonoBehaviour {
		public delegate void MenuEvent();

		public Menu[] menus;
		public CanvasGroup group;
		public Popup popup;

		[Header("Intro stuff")]
		public RectTransform introPanel;
		public RectTransform text1, text2;

		public event MenuEvent OnMenuChange = delegate { };

		bool moveTexts;


		private void Start() {
			if (!Globals.doIntro) {
				introPanel.gameObject.SetActive(false);
				Globals.musicType = GameDirector.MusicClips.Type.MAIN_MENU;

				UpdateActiveMenu();
				UpdatePositions(true);
				OnMenuChange();
			} else {
				StartCoroutine(DoIntro());
				group.interactable = false;
			}
		}

		private void Update() {
			UpdatePositions();

			if (moveTexts) {
				float introSpeed = introPanel.sizeDelta.x / 2f * Time.deltaTime;

				text1.anchoredPosition -= introSpeed * Vector2.right;
				text2.anchoredPosition += introSpeed * Vector2.right;
			}
		}


		void UpdateActiveMenu() {
			for (int i = 0; i < menus.Length; i++)
				menus[i].SetActive(Globals.mainMenuSection == i);
		}
		void UpdatePositions(bool ignoreDelta = false) {
			for (int i = 0; i < menus.Length; i++)
				menus[i].SetPositions(ignoreDelta);
		}
		
		public void SetMenu(int menu) {
			Globals.mainMenuSection = menu;
			OnMenuChange();

			UpdateActiveMenu();
		}

		public void OpenSettings() => Globals.openSettings = true;



		IEnumerator DoIntro() {
			yield return new WaitForSeconds(1f);
			Globals.musicType = GameDirector.MusicClips.Type.MAIN_MENU;

			yield return new WaitForSeconds(1.8f);
			moveTexts = true;
			
			yield return new WaitForSeconds(1.8f);
			moveTexts = false;
			introPanel.gameObject.SetActive(false);
			group.interactable = true;
			UpdateActiveMenu();
			UpdatePositions(true);

			Globals.doIntro = false;
		}

		public void CloseGame() => Application.Quit();
		public void OpenPopup(int index) {
			popup.index = index;
			popup.open = true;
		}
		public void ClosePopup() => popup.open = false;
	}
}
