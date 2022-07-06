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