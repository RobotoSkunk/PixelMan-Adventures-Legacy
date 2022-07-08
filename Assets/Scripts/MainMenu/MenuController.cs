using UnityEngine;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	[System.Serializable]
	public class Menu {
		public MenuPart[] parts;
		readonly float minDist = 1.5f;

		public void SetActive(bool active) {
			for (int i = 0; i < parts.Length; i++) parts[i].isOpen = active;
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
		public Menu[] menus;


		private void Start() {
			Globals.musicType = MainCore.MusicClips.Type.MAIN_MENU;
			UpdateActiveMenu();
			UpdatePositions(true);
		}

		private void Update() {
			UpdatePositions();
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
			UpdateActiveMenu();
		}
	}
}
