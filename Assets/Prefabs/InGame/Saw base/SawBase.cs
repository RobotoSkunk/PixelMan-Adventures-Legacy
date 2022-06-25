using UnityEngine;


namespace RobotoSkunk.PixelMan.Gameplay {
	public class SawBase : GameHandler {
		[Header("Components")]
		public SpriteRenderer sprRend;
		public GameObject children;
		public InGameObjectBehaviour sawBehaviour;

		[Header("Shared")]
		public bool spawnChildren = true;

		private void Start() => SpawnSaw();
		protected override void OnGameReady() => SpawnSaw();
		public void OnSetProperties() => SpawnSaw();

		public void Update() {
			if (!Globals.onPause && sprRend.isVisible) {
				sprRend.transform.localPosition = Random.insideUnitCircle * 0.1f;
			}
		}

		protected override void OnGameResetObject() => sprRend.transform.localPosition = default;
		void SpawnSaw() { if (!spawnChildren) children.SetActive(false); }
	}
}
