using UnityEngine;


namespace RobotoSkunk.PixelMan.Gameplay {
	public class Block : MonoBehaviour {
		public struct BlockData {
			public Sprite sprite;
			public float angle;
		}

		public InGameObjectBehaviour behaviour;
		public SpriteRenderer spriteRenderer;
		public Sprite defSpr;
		public BlockData[] data = new BlockData[47];
	}
}
