using UnityEngine;
using RobotoSkunk;

namespace RobotoSkunk.PixelMan.Physics {
	public class PMRigidbody : MonoBehaviour {
		public Rigidbody2D rb;

		//Vector2 directForce, indirectForce;

		public float hSpeed;

		float hSpeedIndirect = 0f, hSpeedResultant = 0f;

		private void FixedUpdate() {
			if (Globals.onPause) return;

			hSpeedIndirect = Mathf.Lerp(hSpeedIndirect, 0f, 0.04f + Mathf.Abs(RSMath.SafeDivision(hSpeed, rb.velocity.x)) * Time.fixedDeltaTime);

			hSpeedResultant = hSpeed + hSpeedIndirect;

			rb.velocity = new Vector2(hSpeedResultant, Mathf.Clamp(rb.velocity.y, -Constants.maxVelocity, Constants.maxVelocity));
		}

		public void ResetSpeed() {
			hSpeedIndirect = 0f;
			hSpeedResultant = 0f;
			rb.velocity = Vector2.zero;
		}

		public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
			hSpeedIndirect += force.x * rb.mass * (mode == ForceMode2D.Impulse ? 1f : Time.fixedDeltaTime);

			rb.AddForce(new Vector2(0f, force.y), mode);
		}
	}
}
