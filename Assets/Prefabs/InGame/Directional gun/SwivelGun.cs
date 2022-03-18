using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class SwivelGun : GameHandler {
		[Header("Components")]
		public SpriteRenderer spriteRenderer;
		public AudioSource audioSource;

		[Header("Properties")]
		public ContactFilter2D lineFilter;
		public Bullet bullet;

		[Header("Shared")]
		public float reloadTime;

		float time = 1f, ang;
		readonly List<RaycastHit2D> lineResults = new();
		readonly List<GameObject> players = new();

		protected override void OnGameReady() {
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");

			foreach (GameObject g in gameObjects) players.Add(g);
		}

		protected override void OnGameResetObject() {
			time = 1f;
			ang = 0f;
			transform.rotation = Quaternion.Euler(0f, 0f, 0f);
		}

		private void FixedUpdate() {
			if (Globals.onPause) return;

			bool onCount = false;

			if (!Globals.isDead) {
				GameObject target = null;
				float b = Constants.worldLimit;

				foreach (GameObject c in players) {
					float d = Vector2.Distance(transform.position, c.transform.position);

					if (d < b) {
						target = c;
						b = d;
					}
				}

				if (target) {
					int lineBuffer = Physics2D.Linecast(transform.position, target.transform.position, lineFilter, lineResults);

					if (lineBuffer == 0) {
						float __z = RSMath.Direction(transform.position, target.transform.position) * Mathf.Rad2Deg;
						ang += Mathf.Sin((__z - ang) * Mathf.Deg2Rad) * 10f;

						transform.rotation = Quaternion.Euler(0f, 0f, ang);

						onCount = true;

						if (time > 0f) time -= Time.fixedDeltaTime;
						else {
							Bullet newObj = Instantiate(
								bullet,
								transform.position + RSMath.GetDirVector(ang * Mathf.Deg2Rad),
								Quaternion.Euler(0f, 0f, ang)
							);
							newObj.transform.localScale = transform.localScale;
							newObj.rb.velocity = 15f * transform.localScale.x * RSMath.GetDirVector(ang * Mathf.Deg2Rad);
							newObj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;

							time = reloadTime;
							audioSource.Play();
						}
					}
				}
			}

			if (!onCount) time = 1f;
		}
	}
}
