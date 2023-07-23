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

using System.Collections.Generic;

using UnityEngine;

using RobotoSkunk.PixelMan.Events;


namespace RobotoSkunk.PixelMan.Gameplay
{
	public class LaserGun : GameObjectBehaviour
	{
		[Header("Components")]
		public AudioSource audioSource;

		public SpriteRenderer gun;
		public SpriteRenderer dotedLine;
		public SpriteRenderer normalLine;
		public SpriteRenderer laser;
		public SpriteRenderer outline;

		public BoxCollider2D laserCollider;
		public InGameObjectBehaviour laserBehaviour;

		[Header("Properties")]
		public ContactFilter2D contactFilter;
		public LayerMask solidLayer;
		public AudioClip detectedClip, shootClip;
		public Sprite[] laserSprites, gunSprites, dotedLineSprites;


		readonly List<RaycastHit2D> hits = new();
		readonly float laserHeight = 0.4375f;

		enum LaserState {
			None,
			Detected,
			Shoot,
			Reload,
		}

		bool onReset = false;

		float lineSize;
		float reload = 0f;
		float dotedLineTime = 0f;

		int laserFrame = 0;
		int outlineFrame = 0;

		LaserState laserState = LaserState.None;

		private void Awake()
		{
			laserCollider.enabled = false;
			laser.size = new Vector2(0f, 0f);
			normalLine.enabled = false;
			outline.enabled = false;
			gun.sprite = gunSprites[^1];
		}


		protected override void OnGameResetObject()
		{
			audioSource.Stop();
			onReset = true;
			reload = 0f;
			laserState = LaserState.None;
			Awake();
		}

		private void FixedUpdate()
		{
			if (Globals.onPause) {
				return;
			}

			#region Evaluate line distance and detect player
			if (!onReset) {
				lineSize = Constants.worldHypotenuse;

				int count = GetRaycastCount();

				if (count > 0) {
					float playerDistance = lineSize + 1f;

					foreach (RaycastHit2D hit in hits) {
						if (hit.collider.CompareTag("Player") && hit.distance < playerDistance) {
							playerDistance = hit.distance;

						} else if (1 << (hit.collider.gameObject.layer & solidLayer) != 0 && hit.distance < lineSize) {
							lineSize = hit.distance;
						}
					}

					if (laserState == LaserState.None && playerDistance <= lineSize) {
						laserState = LaserState.Detected;
					}
				}
			} else onReset = false;

			dotedLine.size = normalLine.size = new(lineSize, dotedLine.size.y);
			laser.size = new(lineSize, laser.size.y);
			#endregion

			#region Check laser state
			switch (laserState) {
				case LaserState.None:
					laserCollider.enabled = false;
					gun.sprite = gunSprites[^1];

					dotedLine.enabled = true;
					outline.enabled = false;
					normalLine.enabled = false;
					laser.enabled = false;

					if (dotedLine.isVisible) {
						dotedLineTime += Time.fixedDeltaTime * 2f;

						dotedLine.sprite = dotedLineSprites[
							(int) (dotedLineTime * dotedLineSprites.Length) % dotedLineSprites.Length
						];

						if (dotedLineTime >= 1f) {
							dotedLineTime = 0f;
						}
					}
					break;

				case LaserState.Detected:
					if (!audioSource.isPlaying && audioSource.clip != detectedClip) {
						audioSource.clip = detectedClip;
						audioSource.Play();

						normalLine.enabled = true;
						dotedLine.enabled = false;

						outlineFrame = 0;
					}

					if (RSTime.fixedFrameCount % 3 == 0 && (outline.isVisible || normalLine.isVisible)) {
						outlineFrame = outlineFrame == 1 ? 0 : 1;

						outline.enabled = outlineFrame == 1;
						normalLine.color = outlineFrame == 1 ? Color.white : Color.clear;
					}

					if (!audioSource.isPlaying && audioSource.clip == detectedClip) {
						laserState = LaserState.Shoot;
						audioSource.clip = shootClip;
						audioSource.Play();

						GeneralEventsHandler.SetShake(0.5f, 0.25f);
						laser.size = new(lineSize, laserHeight);
						gun.sprite = gunSprites[0];
						outline.enabled = false;
						normalLine.color = new Color(1f, 1f, 1f, 0.5f);
					}
					break;

				case LaserState.Shoot:
					float currentHeight = laser.size.y;
					laser.enabled = true;

					if (currentHeight > 0f) {
						currentHeight -= Time.fixedDeltaTime;
						laser.size = new(lineSize, currentHeight);

						laserCollider.enabled = laser.size.y >= laserHeight / 2f;
					} else {
						laserState = LaserState.Reload;
					}

					if (RSTime.fixedFrameCount % 3 == 0 && laser.isVisible) {
						laserFrame = laserFrame == 1 ? 0 : 1;
						laser.sprite = laserSprites[laserFrame];
					}
					break;

				case LaserState.Reload:
					reload += Time.fixedDeltaTime / laserBehaviour.properties.safeReloadTime;
					laser.enabled = false;

					if (gun.isVisible) {
						gun.sprite = gunSprites[(int) (reload * gunSprites.Length) % gunSprites.Length];
					}

					if (reload >= 1f) {
						reload = 0f;
						laserState = LaserState.None;
						gun.sprite = gunSprites[^1];
					}
					break;
			}
			#endregion
		}

		private int GetRaycastCount()
		{
			return Physics2D.Raycast(
				transform.position,
				RSMath.GetDirVector(transform.eulerAngles.z * Mathf.Deg2Rad),
				contactFilter,
				hits,
				Constants.worldHypotenuse
			);
		}

		public void SetUpTesting(bool isTesting)
		{
			if (!isTesting) {
				dotedLine.size = normalLine.size = new(0f, dotedLine.size.y);
				laser.size = new(0f, laser.size.y);
			}
		}
	}
}
