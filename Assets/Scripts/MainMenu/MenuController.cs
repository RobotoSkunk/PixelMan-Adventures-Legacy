using System.Collections;

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

		[Header("Intro stuff")]
		public float introTextSpeed;
		public RectTransform text1, text2;
		public GameObject introPanel;

		bool moveTexts;
		float introTimer;


		private void Start() {
			bool intro = Globals.musicType == MainCore.MusicClips.Type.NONE;

			if (!intro) {
				introPanel.SetActive(false);
				Globals.musicType = MainCore.MusicClips.Type.MAIN_MENU;
			} else StartCoroutine(DoIntro());

			UpdateActiveMenu();
			UpdatePositions(true);
		}

		private void Update() {
			UpdatePositions();

			if (moveTexts) {
				text1.anchoredPosition -= Time.deltaTime * new Vector2(introTextSpeed, 0f);
				text2.anchoredPosition += Time.deltaTime * new Vector2(introTextSpeed, 0f);
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
			UpdateActiveMenu();
		}


		IEnumerator DoIntro() {
			yield return new WaitForSeconds(2.5f);
			Globals.musicType = MainCore.MusicClips.Type.MAIN_MENU;

			yield return new WaitForSeconds(1.8f);
			moveTexts = true;
			
			yield return new WaitForSeconds(1.8f);
			moveTexts = false;
			introPanel.SetActive(false);
		}
	}
}
