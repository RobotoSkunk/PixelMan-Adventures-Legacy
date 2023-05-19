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

using RobotoSkunk.PixelMan.Utils;
using RobotoSkunk.PixelMan.Gameplay;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class MenuDeco : MonoBehaviour {
		[Header("Components")]
		public Animator animator;
		public GameObject ground;
		public PlayerColor playerSprite;

		[Header("Properties")]
		public float groundSpeed;


		readonly Player.State __playerState = Player.State.RUNNING;
		int __lastPlayerID;


		private void Awake() => SetPlayerSkin();

		private void Update() {
			ground.transform.Translate(groundSpeed * Time.deltaTime * Vector3.left);

			if (ground.transform.position.x < -5f)
				ground.transform.position += new Vector3(10f, 0f, 0f);

			if ((int)Globals.playerData.skinIndex != __lastPlayerID) SetPlayerSkin();
			playerSprite.color = Globals.playerData.Color;
		}


		void SetPlayerSkin() {
			__lastPlayerID = (int)Globals.playerData.skinIndex;
			Globals.PlayerCharacters ps = Globals.playerCharacters.ClampIndex(__lastPlayerID);
			animator.runtimeAnimatorController = ps.controller;

			animator.SetFloat("Speed", 1f);
			animator.SetFloat("State", (int)__playerState);
		}
	}
}
