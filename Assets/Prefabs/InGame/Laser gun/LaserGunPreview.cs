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


namespace RobotoSkunk.PixelMan.LevelEditor {
	public class LaserGunPreview : PhysicsHandler {
		[Header("Components")]
		public SpriteRenderer laser;
		public SpriteRenderer gun;

		public ContactFilter2D contactFilter;


		readonly List<RaycastHit2D> hits = new();
		bool isEditor, hasChanged;
		float lineSize;

		private void FixedUpdate() {
			if (isEditor && hasChanged) {
				int count = GetRaycastCount();
				lineSize = Constants.worldHypotenuse;

				if (count > 0) {
					foreach (RaycastHit2D hit in hits) {
						if (hit.distance < lineSize)
							lineSize = hit.distance;
					}
				}

				hasChanged = false;
				laser.size = new Vector2(lineSize, laser.size.y);
			}
		}

		private int GetRaycastCount() => Physics2D.Raycast(transform.position, RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad), contactFilter, hits, Constants.worldHypotenuse);

		public void SetIsEditor(bool isEditor) {
			laser.enabled = this.isEditor = isEditor;
			hasChanged = true;
		}

		protected override void OnGenerateCompositeGeometry() => hasChanged = true;
	}
}
