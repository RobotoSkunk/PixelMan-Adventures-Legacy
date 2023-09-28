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

using UnityEngine;

using TMPro;


namespace RobotoSkunk.PixelMan.Utils
{
	public class LevelsSceneManager : MonoBehaviour
	{
		public bool isLocked;
		public GameObject lockedPanel;
		public LevelButton[] buttons;

		public TextMeshProUGUI stageNameText;

		public string stageName;
		public int stageIndex;

		bool __locked;


		void Start()
		{
			stageNameText.text = stageName;

			int countLevels = Globals.currentWorld.scenes[stageIndex].levels.Length;

			for (int i = 0; i < buttons.Length; i++) {
				if (i < countLevels) {
					buttons[i].levelIndex = i;
					buttons[i].stageIndex = stageIndex;
				} else {
					buttons[i].gameObject.SetActive(false);
				}
			}
		}


		private void Update()
		{
			if (isLocked != __locked) {
				__locked = isLocked;
				
				lockedPanel.SetActive(__locked);

				for (int i = 0; i < buttons.Length; i++) {
					buttons[i].button.interactable = !__locked;
				}
			}
		}
	}
}

