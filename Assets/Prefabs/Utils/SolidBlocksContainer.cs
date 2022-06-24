using UnityEngine;

using RobotoSkunk.PixelMan.Events;


namespace RobotoSkunk.PixelMan.Physics {
	public class SolidBlocksContainer : PhysicsHandler {
		public CompositeCollider2D untagged, iceBlock;

		protected override void OnGenerateCompositeGeometry() {
			untagged.GenerateGeometry();
			iceBlock.GenerateGeometry();
		}
	}
}
