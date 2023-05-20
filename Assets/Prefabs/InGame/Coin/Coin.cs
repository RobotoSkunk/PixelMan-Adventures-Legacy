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



namespace RobotoSkunk.PixelMan.Gameplay {
	public class Coin : GameObjectBehaviour {
		public Animator animator;
		public SpriteRenderer spriteRenderer;
		public ParticleSystem particles;

		bool collected = false;
		float delta = 0f;


		public void OnGameState(bool isEditor) {
			if (isEditor) particles.Stop();
			else particles.Play();

			if (isEditor) {
				animator.speed = 0f;
				animator.Play("Coin", 0, 0f);
			}
		}

		protected override void OnGameResetObject() {
			collected = false;
			Globals.gotCoin = false;

			UpdateCoin(true);
		}

		private void Update() {
			animator.speed = (!Globals.onPause).ToInt();

			UpdateCoin();
		}

		private void OnTriggerEnter2D(Collider2D other) {
			if (other.gameObject.CompareTag("Player")) {
				collected = true;
				Globals.gotCoin = true;
			}
		}

		void UpdateCoin(bool force = false) {
			if (collected) {
				delta += Time.deltaTime;
				delta = Mathf.Clamp01(delta);

				animator.SetBool("Collected", true);

				if (delta >= 0.5f && !particles.isStopped) particles.Stop();
				return;
			}

			if (!force) return;

			spriteRenderer.color = Color.white;
			animator.speed = 1f;
			delta = 0f;
			particles.Play();
			animator.SetBool("Collected", false);
			animator.Play("Coin", 0, 0f);
		}
	}
}
