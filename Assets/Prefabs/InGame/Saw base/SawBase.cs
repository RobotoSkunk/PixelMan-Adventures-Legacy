using UnityEngine;


namespace RobotoSkunk.PixelMan.Gameplay {
	public class SawBase : MonoBehaviour {
		[Header("Components")]
		public SpriteRenderer sprRend;
		public GameObject children;

		[Header("Shared")]
		public bool spawnChildren = true;

		private void Start() {
			if (!spawnChildren) children.SetActive(false);
		}

		public void Update() {
			if (!Globals.onPause && sprRend.isVisible) {
				sprRend.transform.localPosition = Random.insideUnitCircle * 0.1f;
			}
		}
	}
}
