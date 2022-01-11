using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class PlayerCamera : GameHandler {
		[Header("Components")]
		public Camera cam;
		public Rigidbody2D player;

		[Header("Properties")]
		public float orthoDefault, maxSpeed;

		[Header("Shared")]
		public Vector2 look;

		Vector2 vTo, camPos, lookAt;
		Vector3 startPos;
		float velocity, zoom;

		private void Start() {
			transform.parent = null;
			camPos = startPos = transform.position;
		}

		protected override void OnGameResetObject() {
			velocity = zoom = 0f;
			look = vTo = Vector2.zero;
			camPos = transform.position = startPos;
			cam.orthographicSize = orthoDefault;
		}

		private void FixedUpdate() {
			Vector2 targetPos = player.position;

			if (player) {
				vTo = targetPos + (Vector2)(RSMath.GetDirVector(RSMath.Direction(camPos, targetPos)) * Mathf.Min(maxSpeed, Vector2.Distance(camPos, targetPos)));

				if (!Globals.onPause)
					velocity = player.velocity.magnitude;
			} else {
				velocity = 0f;
				look = Vector2.zero;
			}

			lookAt = Vector2.Lerp(lookAt, 2f * look, 0.25f);

			zoom = Mathf.Lerp(zoom, velocity / Constants.maxVelocity, 0.01f);
			cam.orthographicSize = orthoDefault + zoom * orthoDefault;

			camPos += (vTo - camPos) / 30f;

			transform.position = (Vector3)camPos + (1f + zoom) * ((Vector3)(Globals.shakeForce * 0.5f * Random.insideUnitCircle) + (Vector3)lookAt) + new Vector3(0f, 0f, -10f);
		}
	}
}
