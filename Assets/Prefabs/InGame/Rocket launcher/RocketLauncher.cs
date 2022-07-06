using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class RocketLauncher : GameHandler {
		[Header("Components")]
		public SpriteRenderer spriteRenderer;
		public AudioSource audioSource;
		public InGameObjectBehaviour launcherBehaviour;

		[Header("Properties")]
		public ContactFilter2D lineFilter;
		public Rocket rocket;

		float time = 1f, ang, newAng;
		readonly List<RaycastHit2D> lineResults = new();
		readonly List<GameObject> players = new();

		protected override void OnGameReady() {
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Player");
			players.Clear();

			foreach (GameObject g in gameObjects) players.Add(g);
			time = launcherBehaviour.properties.safeReloadTime;
		}

		protected override void OnGameResetObject() {
			time = launcherBehaviour.properties.safeReloadTime;
			ang = newAng = 0f;
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
						newAng = RSMath.Direction(transform.position, target.transform.position) * Mathf.Rad2Deg;

						onCount = true;

						if (time > 0f) time -= Time.fixedDeltaTime;
						else {
							Rocket newObj = Instantiate(
								rocket,
								transform.position + RSMath.GetDirVector(ang * Mathf.Deg2Rad),
								Quaternion.Euler(0f, 0f, ang)
							);
							newObj.transform.localScale = transform.localScale;
							newObj.spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
							newObj.target = target;

							time = launcherBehaviour.properties.safeReloadTime;
							audioSource.Play();
						}
					}
				}
			}

			if (!onCount) {
				time = 1f;
				newAng++;
			}
			ang += Mathf.Sin((newAng - ang) * Mathf.Deg2Rad) * 5f;

			transform.rotation = Quaternion.Euler(0f, 0f, ang);
		}
	}
}
