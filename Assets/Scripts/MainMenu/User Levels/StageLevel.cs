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

using System;
using UnityEngine;

using TMPro;

using RobotoSkunk.PixelMan.LevelEditor;


namespace RobotoSkunk.PixelMan.UI.MainMenu
{
	public class StageLevel : MonoBehaviour
	{
		[Header("Level Data")]
		public RSInputField lvlName;
		public TextMeshProUGUI createdAt, timeSpent;

		[HideInInspector] public Level.UserMetadata data;
		[HideInInspector] public UserStageMenuController controller;


		private void Start()
		{
			lvlName.SetTextWithoutNotify(data.name);
			createdAt.text = RSTime.FromUnixTimestamp(data.createdAt).ToString("yyyy-MM-dd HH:mm:ss");
			timeSpent.text = TimeSpan.FromMilliseconds(data.timeSpent).ToString(@"hh\:mm\:ss");
		}

		public void SetCurrentLevel()
		{
			Globals.levelIsBuiltIn = false;
			Globals.Editor.currentLevel = data;
		}

		public void DeleteLevel()
		{
			SetCurrentLevel();
			controller.TogglePopup(true);
			controller.SetPopupIndex(1);
		}


		public void SetName(string name)
		{
			controller.SetLevelName(data.uuid, name);
		}

		public void MoveUp()
		{
			SetCurrentLevel();
			controller.MoveLevelUp();
		}

		public void MoveDown()
		{
			SetCurrentLevel();
			controller.MoveLevelDown();
		}
	}
}
