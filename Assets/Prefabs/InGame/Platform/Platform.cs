using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Platform : GameHandler {
		[Header("Properties")]
		public Rigidbody2D rb;
		public BoxCollider2D col;
		public LayerMask layer;
		//public SpriteRenderer spr;

		[Header("Shared")]
		public float speed;
		public bool goPositive = true;
		public Direction dir;
		public Vector2 lastPosition;

		float time = 0f;
		bool goPosStart;
		Vector2 lastPos, startPos, velocity;

		public enum Direction { HORIZONTAL, VERTICAL }

		private void Start() {
			if (dir == Direction.VERTICAL)
				rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
			
			startPos = transform.position;
			goPosStart = goPositive;
		}

		private void FixedUpdate() {
			if (Globals.onPause) {
				rb.velocity = Vector2.zero;
				return;
			}

			if (time > 0f) {
				time -= Time.fixedDeltaTime;
				velocity = Vector2.zero;

				lastPos += Vector2.one;
			} else {
				if (dir == Direction.HORIZONTAL)
					velocity = new Vector2(speed * (goPositive ? 1f : -1f), 0f);
				else
					velocity = new Vector2(0f, speed * (goPositive ? 1f : -1f));

				if (Vector2.Distance(transform.position, lastPos) < 0.05f) {
					goPositive = !goPositive;
					time = 0.25f;
				}

				lastPos = transform.position;
			}

			rb.velocity = velocity;
			lastPosition = transform.position;
		}

		protected override void OnGameResetObject() {
			transform.position = startPos;
			goPositive = goPosStart;
			rb.velocity = velocity = Vector2.zero;
		}
	}
}
