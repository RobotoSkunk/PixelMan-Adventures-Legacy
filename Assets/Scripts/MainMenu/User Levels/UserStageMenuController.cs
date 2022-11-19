using UnityEngine;

namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class UserStageMenuController : MonoBehaviour {
		[Header("Menu stuff")]
		public MenuController menu;
		public int menuIndex;

		[Header("UI")]
		public RSInputField lvlName;
		public RSInputField description;


		private void Awake() => menu.OnMenuChange += UpdateInfo;

		private void UpdateInfo() {
			if (Globals.mainMenuSection != menuIndex) return;

			
		}
	}
}
