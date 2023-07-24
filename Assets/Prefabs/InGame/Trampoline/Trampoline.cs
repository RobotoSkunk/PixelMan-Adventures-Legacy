/*
	PixelMan Adventures, an open source platformer game.
	Copyright (C) 2022  RobotoSkunk <contact@robotoskunk.com>

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Affero General Public License as published
	by the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Affero General Public License for more details.

	You should have received a copy of the GNU Affero General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;


namespace RobotoSkunk.PixelMan.Gameplay
{
	public class Trampoline : MonoBehaviour
	{
		[Header("Components")]
		public Animator animator;
		public AudioSource audioSource;

		[Header("Properties")]
		public LayerMask layerMask;

		private void Awake() => animator.Play("Default", 0, 5f);
		private void Update() => animator.speed = Globals.onPause ? 0 : 1;

		int colliderID = -1;
		Rigidbody2D otherRigidbody;


		private void OnTriggerEnter2D(Collider2D collision)
		{
			if ((1 << collision.gameObject.layer & layerMask.value) != 0) {

				if (collision.GetInstanceID() != colliderID) {
					colliderID = collision.GetInstanceID();
					otherRigidbody = collision.attachedRigidbody;
				}


				Impulse(otherRigidbody);
			}
		}


		void Impulse(Rigidbody2D rigidbody = null)
		{
			audioSource.Play();
			animator.Play("Default", 0, 0f);

			if (rigidbody) {
				float direction = (transform.eulerAngles.z + 90f) * Mathf.Deg2Rad;
				Vector2 directionVector = (Vector2)RSMath.GetDirVector(direction);

				rigidbody.velocity *= Vector2.one - RSMath.Abs(directionVector);

				rigidbody.AddForce(
					RSMath.GetDirVector(direction) * Constants.trampolineForce,
					ForceMode2D.Impulse
				);
			}
		}
	}
}
