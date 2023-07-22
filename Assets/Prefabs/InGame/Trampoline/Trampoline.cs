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


		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.gameObject.CompareLayers(layerMask)) {
				Impulse();
			}
		}

		private void OnParticleCollision(GameObject other)
		{
			if (other.CompareLayers(layerMask)) {
				Impulse(other.GetComponent<Rigidbody>());
			}

			Debug.Log("Collision");
		}


		void Impulse(Rigidbody rigidbody = null)
		{
			audioSource.Play();
			animator.Play("Default", 0, 0f);

			if (rigidbody) {
				rigidbody.AddForce(
					RSMath.GetDirVector((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad)
					* Constants.trampolineForce, ForceMode.Impulse
				);
			}
		}
	}
}
