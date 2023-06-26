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

using UnityEngine;

using RobotoSkunk.PixelMan.Utils;
using RobotoSkunk.PixelMan.Events;
using RobotoSkunk.PixelMan.LevelEditor.IO;


namespace RobotoSkunk.PixelMan.Gameplay
{
	public class GameController : GameObjectBehaviour
	{
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[SerializeField] PlayerCamera playerCamera;
		[SerializeField] LevelIO.TransformContainers transformContainers;
		[SerializeField] SceneReferenceHandler mainMenuSceneHandler;
		#pragma warning restore IDE0044


		private void Awake()
		{
			Globals.onPause = true;
			Globals.onLoad = true;
			Globals.loadingText = Globals.languages.GetField("loading.load_objects");
			Globals.musicType = GameDirector.MusicClips.Type.IN_GAME;

			UniTask.Void(async () =>
			{
				await LevelIO.LoadLevel(false, transformContainers);

				GameObject player = GameObject.FindWithTag("Player");

				if (player) {
					Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
					Player playerScript = player.GetComponent<Player>();

					playerCamera.SetPlayer(playerBody);
					playerScript.SetCamera(playerCamera);

					PhysicsEventsHandler.GenerateCompositeGeometry();
					GameEventsHandler.InvokeLevelReady();

					Globals.onPause = false;
				} else {
					mainMenuSceneHandler.GoToScene();
				}
			});
		}
	}
}
