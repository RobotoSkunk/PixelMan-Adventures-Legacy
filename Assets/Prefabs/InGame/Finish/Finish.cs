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

using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Finish : GameHandler {
		[Header("Components")]
		public Animator anim;
		public BoxCollider2D col;
		public AudioSource audioSource;

		[Header("Shared")]
		public bool isOpen;

		readonly List<Switch> switches = new();

		protected override void OnGameReady() {
			GameObject[] tmp = GameObject.FindGameObjectsWithTag("Switch");
			switches.Clear();

			foreach (GameObject go in tmp) {
				if (!go) continue;
				if (!go.activeInHierarchy) continue;

				switches.Add(go.GetComponent<Switch>());
			}
		}

		protected override void OnGameSwitchTouched() {
			int turnedOn = 0;

			foreach (Switch sw in switches) {
				if (sw.switched) turnedOn++;
			}

			if (turnedOn == switches.Count) isOpen = true;

			if (isOpen) {
				col.enabled = true;
				anim.SetBool("IsOpen", true);
				audioSource.Play();
			}
		}

		protected override void OnGameResetObject() {
			isOpen = false;
			col.enabled = false;
			anim.SetBool("IsOpen", isOpen);
		}
	}
}
