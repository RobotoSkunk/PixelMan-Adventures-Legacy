using UnityEngine;


namespace RobotoSkunk.PixelMan.Gameplay {
	public class Block : MonoBehaviour {
		public InGameObjectBehaviour behaviour;
		public SpriteRenderer spriteRenderer;
		public Sprite defSpr;


		public void SetSkin(int index) {
			if (index >= 0 && index < behaviour.options.skins.Length)
				spriteRenderer.sprite = behaviour.options.skins[index];
			else
				spriteRenderer.sprite = defSpr;
		}
	}
}
