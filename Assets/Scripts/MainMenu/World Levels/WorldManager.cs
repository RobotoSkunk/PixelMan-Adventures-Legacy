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
	public class WorldManager : MonoBehaviour
	{
		public GameObject stagesContainer;
		public LevelsSceneManager levelsSceneManagerPrefab;

		public TextMeshProUGUI worldNameText;


		private void Start()
		{
			Globals.levelIsBuiltIn = false;
			LoadWorld();
		}

		public void LoadWorld()
		{
			if (Globals.currentWorld == null) {
				return;
			}

			Globals.levelIsBuiltIn = true;

			// Delete all childs in the stages container.
			foreach (Transform child in stagesContainer.transform) {
				Destroy(child.gameObject);
			}

			worldNameText.text = Globals.currentWorld.name;

			// Create a new LevelsSceneManager for each stage.
			for (int i = 0; i < Globals.currentWorld.scenes.Length; i++) {
				LevelsSceneManager __levelsSceneManager = Instantiate(levelsSceneManagerPrefab, stagesContainer.transform);
				__levelsSceneManager.stageName = Globals.currentWorld.scenes[i].name;
				__levelsSceneManager.stageIndex = i;
			}
		}
	}
}

