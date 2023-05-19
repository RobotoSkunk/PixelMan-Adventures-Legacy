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

using RobotoSkunk.PixelMan;


namespace RobotoSkunk {
	public class RSBehaviour : MonoBehaviour {
		public Flags flags;

		[System.Flags]
		public enum Flags {
			NONE              = 0,

			Player            = 1 << 0,
			Trampoline        = 1 << 1,
			GravitySwitch     = 1 << 2,
			Switch            = 1 << 3,
			Killzone          = 1 << 4,
			Platform          = 1 << 5,
			Ignore            = 1 << 6,
			IceBLock          = 1 << 7,
			Laser             = 1 << 8,
			Bullet            = 1 << 9,
			Rocket            = 1 << 10,
			IntelligentRocket = 1 << 11,
			EditorObject      = 1 << 12,
			Coin              = 1 << 13,

			ALL               = ~0
		}

		private void Awake() {
			string uid = gameObject.GetInstanceID().ToString();
			Globals.__behaviours.Add(uid, this);
		}

		private void OnDestroy() {
			string uid = gameObject.GetInstanceID().ToString();
			Globals.__behaviours.Remove(uid);
		}
	}

	public static class RSBehaviourExtensions {
		public static bool CompareFlags(this GameObject gameObject, RSBehaviour.Flags flags) {
			RSBehaviour b = Globals.__behaviours[gameObject.GetInstanceID().ToString()];

			if (b == null) 
				throw new System.Exception("Current gameObject has no RSBehaviour component");

			return (b.flags & flags) == flags;
		}

		public static bool HasFlags(this GameObject gameObject, RSBehaviour.Flags flags) {
			RSBehaviour b = Globals.__behaviours[gameObject.GetInstanceID().ToString()];

			if (b == null) 
				throw new System.Exception("Current gameObject has no RSBehaviour component");

			return (b.flags & flags) != 0;
		}
	}
}
