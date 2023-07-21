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
using UnityEngine.UI;


namespace RobotoSkunk.PixelMan.LevelEditor
{
	public class LevelOptionsHandler : EditorHandler
	{
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[SerializeField] Level.Options options;
		[SerializeField] Toggle toggle;

		#pragma warning restore IDE0044


		protected override void OnEditorReady()
		{
			toggle.SetIsOnWithoutNotify(Globals.levelData.IsOptionSet(options));
		}

		public void SetOption(bool value)
		{
			Globals.levelData.SetOption(options, value);
		}
	}
}
