using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Saw : GameHandler {
		[Header("Components")]
		public SpriteRenderer sprRend;

		float speed = 0f, ang = 0f;

		private void Start() {
			speed = Random.Range(15f, 25f) * RSRandom.Sign();
		}

		protected override void OnGameResetObject() {
			sprRend.transform.rotation = default;
			sprRend.transform.localPosition = default;
			ang = 0f;
		}

		public void Update() {
			if (!Globals.onPause && sprRend.isVisible) {
				ang += speed * RSTime.delta;

				sprRend.transform.localPosition = Random.insideUnitCircle * 0.1f;
				sprRend.transform.rotation = Quaternion.Euler(0, 0, ang);
			}
		}
	}
}
