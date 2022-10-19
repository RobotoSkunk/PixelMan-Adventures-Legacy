using UnityEngine;

using RobotoSkunk.PixelMan.UI;


namespace RobotoSkunk.PixelMan.Utils {
	public class LevelsSceneManager : MonoBehaviour {
		public bool isLocked;
		public GameObject lockedPanel;
		public RSButton[] buttons;

		bool __locked;

		private void Update() {
			if (isLocked != __locked) {
				__locked = isLocked;
				
				lockedPanel.SetActive(__locked);
				buttons.SetInteractable(!__locked);
			}
		}
	}
}

