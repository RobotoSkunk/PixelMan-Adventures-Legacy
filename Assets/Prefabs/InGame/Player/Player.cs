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
using UnityEngine.InputSystem;

using RobotoSkunk.PixelMan.Physics;
using RobotoSkunk.PixelMan.Events;
using RobotoSkunk.PixelMan.LevelEditor;


namespace RobotoSkunk.PixelMan.Gameplay
{
	public class Player : GameObjectBehaviour
	{
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[Header("Components")]
		[SerializeField] new Rigidbody2D rigidbody;
		[SerializeField] PMRigidbody pixelManRigidbody;
		[SerializeField] SpriteRenderer spriteRenderer;
		[SerializeField] BoxCollider2D boxCollider;
		[SerializeField] AudioSource audioSource;
		[SerializeField] Animator animator;
		[SerializeField] ParticleSystem deathParticles, runParticles;
		[SerializeField] InGameObjectBehaviour playerBehaviour;

		[Header("Properties")]
		[SerializeField] Vector2 speed;
		[SerializeField] float maxSpeed, maxJumpBuffer, maxHangCount;
		[SerializeField] ContactFilter2D groundFilter;
		[SerializeField] AudioClip[] sounds;
		#pragma warning restore IDE0044

		[Header("Shared")]
		public Vector2 lastPublicPos; // TODO: Change this to lastPublicPosition


		// Floats
		float axis = 0f;
		float jumpBuffer = 0f;
		float hangCount = 0f;
		float horizontalSpeed = 0f;
		float platformSpeed = 0f;
		float speedFactor = 0.4f;

		// Booleans
		bool onGround = false;
		bool canControlJump = false;
		bool canJump = true;
		bool isOverPlatform = false;
		bool invertedGravity = false;
		bool wasOnPause = false;
		bool respawnGravity;

		// Bidimensional vectors
		Vector2 startPos;
		Vector2 lastPos;
		Vector2 lastVelocity;

		// Player states
		State currentPlayerState = State.IDLE;
		State lastState = State.IDLE;

		// Lists
		readonly List<Platforms> platforms = new();
		readonly List<Collider2D> groundOverlap = new();
		// readonly List<Collider2D> stuckResult = new();

		// Others
		PlayerCamera playerCamera;


		// Definitions
		protected class Platforms
		{
			public GameObject gameObject;
			public bool wasCollided;
			public Platform code;
		}

		public enum State {
			IDLE,
			RUNNING,
			JUMPING,
			FALLING,
		}

		bool fellFromTheWorld {
			get
			{
				if (!invertedGravity) {
					return transform.position.y < Globals.levelData.bounds.yMin - 1f;
				} else {
					return transform.position.y > Globals.levelData.bounds.yMax + 1f;
				}
			}
		}

		bool outOfBounds {
			get
			{
				return transform.position.y < Globals.levelData.bounds.yMin - 1f
					|| transform.position.y > Globals.levelData.bounds.yMax + 1f;
			}
		}



		// Start of Unity methods

		private void Awake()
		{
			// Globals.playerData.Color.a = 1f;
			spriteRenderer.color = Globals.playerData.Color;

			Globals.PlayerCharacters ps = Globals.playerCharacters.ClampIndex((int)Globals.playerData.skinIndex);
			spriteRenderer.sprite = ps.display;
			animator.runtimeAnimatorController = ps.controller;

			ParticleSystem.MainModule particleMain = deathParticles.main;
			particleMain.startColor = Globals.playerData.Color;

			rigidbody.velocity = Vector2.zero;
			rigidbody.gravityScale = 0f;
			rigidbody.bodyType = RigidbodyType2D.Static;
		}

		private void FixedUpdate()
		{
			#region On pause
			if (Globals.onPause) {
				rigidbody.velocity = Vector2.zero;
				rigidbody.gravityScale = 0f;
				wasOnPause = true;
				animator.speed = 0;


				if (!runParticles.isPaused && runParticles.isPlaying) {
					runParticles.Pause();
				}
				if (!deathParticles.isPaused && deathParticles.isPlaying) {
					deathParticles.Pause();
				}

				return;
			} else if (wasOnPause) {
				wasOnPause = false;
				animator.speed = 1;

				if (!Globals.isDead) {
					rigidbody.velocity = lastVelocity;
				}

				if (runParticles.isPaused) {
					runParticles.Play();
				}
				if (deathParticles.isPaused) {
					deathParticles.Play();
				}
			}
			#endregion

			if (Globals.isDead) {
				rigidbody.velocity = Vector2.zero;
				return;
			}

			rigidbody.gravityScale = invertedGravity ? -1f : 1f;

			Vector2 spd = new Vector2(transform.position.x, transform.position.y) - lastPos;

			#region Jump buffers
			int groundBuffer = Physics2D.OverlapBox(
				transform.position - new Vector3(0f, boxCollider.size.y / 2f) * rigidbody.gravityScale,
				new Vector2(boxCollider.size.x + Constants.pixelToUnit, 0.1f),
				0f,
				groundFilter,
				groundOverlap
			);
			onGround = false;


			if (groundBuffer != 0) {
				onGround = true;
				speedFactor = 0.4f;

				switch (groundOverlap[0].tag) {
					case "Ignore": onGround = false; break;
					case "IceBlock": speedFactor = 0.025f; break;
					case "Platform": onGround = isOverPlatform; break;
				}
			}


			if (jumpBuffer > 0f) {
				jumpBuffer -= Time.fixedDeltaTime;
			}

			if (onGround) {
				canJump = true;
				canControlJump = true;
				hangCount = maxHangCount;
				if (Globals.settings.general.enableParticles) {
					if (spd.x != 0f && axis != 0f) {
						if (!runParticles.isPlaying) {
							runParticles.Play();
						}
					} else if (runParticles.isPlaying) {
						runParticles.Stop();
					}
				}
			} else {
				if (hangCount > 0f) {
					hangCount -= Time.fixedDeltaTime;
				}
				if (runParticles.isPlaying) {
					runParticles.Stop();
				}
			}

			if (jumpBuffer > 0f && hangCount > 0f && !Globals.isDead) {
				rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);
				rigidbody.AddForce(new Vector2(0f, speed.y) * rigidbody.gravityScale, ForceMode2D.Impulse);

				audioSource.PlayOneShot(sounds[0]);

				hangCount = -1f;
				jumpBuffer = -1f;
			}
			#endregion

			horizontalSpeed = Mathf.Lerp(horizontalSpeed, axis * speed.x, speedFactor);

			spriteRenderer.flipY = invertedGravity;
			runParticles.transform.localScale = new Vector3(1f, rigidbody.gravityScale, 1f);
			if (axis != 0f) spriteRenderer.flipX = axis < 0f;

			pixelManRigidbody.horizontalSpeed = horizontalSpeed + platformSpeed;

			#region Animation processing
			// I prefer doing all this by code than using Unity animatior transitions manually.

			float animSpeed = Mathf.Abs(horizontalSpeed) > 0f && Mathf.Abs(spd.x) >= 0.05f
							? Mathf.Abs(horizontalSpeed) / speed.x : 0f;

			float velocityYAxis = rigidbody.gravityScale * rigidbody.velocity.y;


			if (animSpeed <= 0.2f) {
				animSpeed = 0.2f;
			}


			// Styling excuse: It's not too readable if I follow the style guide.

			if (onGround && (Mathf.Abs(axis) == 0f || spd.x == 0f)) currentPlayerState = State.IDLE;
			else if (onGround && Mathf.Abs(axis) > 0f) currentPlayerState = State.RUNNING;
			else if (!onGround && velocityYAxis > 0f) currentPlayerState = State.JUMPING;
			else if (!onGround && velocityYAxis < 0f) currentPlayerState = State.FALLING;
			else currentPlayerState = State.IDLE;

			// End of styling excuse.

			if (currentPlayerState != lastState) {
				animator.Play("Default", 0, 0f);
			}
			lastState = currentPlayerState;

			animator.SetFloat("State", (float)currentPlayerState);
			animator.SetFloat("Speed", currentPlayerState == State.RUNNING ? animSpeed : 1f);
			#endregion

			lastPos = transform.position;
			lastVelocity = rigidbody.velocity;
		}

		private void Update()
		{
			if (Globals.isDead) {
				return;
			}


			bool killWhenFalling = Globals.levelData.IsOptionSet(Level.Options.KillPlayerWhenFallingOutOfLevel);

			if (fellFromTheWorld && killWhenFalling) {
				Globals.isDead = true;
			} else if (outOfBounds && !killWhenFalling) {
				float YPosition = transform.position.y;

				if (YPosition < Globals.levelData.bounds.yMin + 0.5f) {
					YPosition = Globals.levelData.bounds.yMax + 0.5f;
				} else if (YPosition > Globals.levelData.bounds.yMax - 0.5f) {
					YPosition = Globals.levelData.bounds.yMin - 0.5f;
				}

				transform.position = new Vector2(transform.position.x, YPosition);
			}
		}


		#region Triggers
		private void OnTriggerEnter2D(Collider2D collision)
		{
			switch (collision.tag) {
				case "Trampoline":
					pixelManRigidbody.ResetSpeed();

					pixelManRigidbody.AddForce(
						RSMath.GetDirVector(
							(collision.transform.eulerAngles.z + 90f) * Mathf.Deg2Rad
						) * Constants.trampolineForce,
						ForceMode2D.Impulse
					);

					lastVelocity = rigidbody.velocity;
					canJump = false;
					break;

				case "GravitySwitch": invertedGravity = !invertedGravity;break;
				case "Finish": Debug.Log("Won!"); break;
				// default:
				// 	if (collision.gameObject.layer == LayerMask.NameToLayer("Killzone")) {
				// 		Globals.isDead = true;
				// 	}
				// 	break;
			}
		}

		private void OnTriggerStay2D(Collider2D collision)
		{
			if (Globals.onPause) {
				return;
			}

			switch (collision.tag) {
				case "Trampoline":
					canJump = false;
					break;
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			switch (collision.tag) {
				case "Trampoline":
					canJump = false;
					break;
			}
		}
		#endregion

		#region Collisions
		private void OnCollisionStay2D(Collision2D collision)
		{
			if (Globals.onPause) {
				return;
			}

			switch (collision.collider.tag) {
				case "Platform":
					Platforms platform = platforms.Find(m => m.gameObject == collision.gameObject);

					float platformTop = collision.transform.position.y + 0.27f;
					float platformBottom = collision.transform.position.y - 0.27f;
					float selfTop = transform.position.y + boxCollider.size.y / 2f;
					float selfBottom = transform.position.y - boxCollider.size.y / 2f;

					isOverPlatform = false;


					if (
						platform != null && ((platformTop < selfBottom && !invertedGravity)
						||
						(platformBottom > selfTop && invertedGravity))
					) {
						platformSpeed = (collision.transform.position.x - platform.code.lastPosition.x) / Time.fixedDeltaTime;
						platform.wasCollided = true;
						isOverPlatform = true;
					}
					break;
			}
		}

		private void OnCollisionExit2D(Collision2D collision)
		{
			switch (collision.collider.tag) {
				case "Platform":
					Platforms platform = platforms.Find(m => m.gameObject == collision.gameObject);

					if (platform != null) {
						if (platform.wasCollided) {
							pixelManRigidbody.AddForce(new Vector2(platformSpeed, 0f), ForceMode2D.Impulse);
							platformSpeed = 0f;

							platform.wasCollided = false;
							isOverPlatform = false;
						}
					}
					break;
			}
		}
		#endregion

		#region New input system
		public void HorizontalMovement(InputAction.CallbackContext context)
		{
			axis = context.ReadValue<Vector2>().x;

			if (Globals.isDead) {
				return;
			}
		}

		public void JumpMovement(InputAction.CallbackContext context)
		{
			bool onJump = context.ReadValue<float>() > 0f;
			bool goesUp = !invertedGravity ? rigidbody.velocity.y > 0f : rigidbody.velocity.y < 0f;

			if (Globals.isDead) {
				return;
			}

			if (onJump && canJump) {
				jumpBuffer = maxJumpBuffer;
			} else if (!onJump && goesUp && canControlJump) {
				rigidbody.velocity *= new Vector2(1f, 0.5f);
				canControlJump = false;
			}
		}

		public void LookUp(InputAction.CallbackContext context)
		{
			if (playerCamera != null) {
				playerCamera.look = context.ReadValue<Vector2>();
			}
		}
		#endregion

		#region Editor methods
		public void SetUpStartupVars()
		{
			startPos = transform.position;
			invertedGravity = playerBehaviour.properties.invertGravity;
			rigidbody.bodyType = RigidbodyType2D.Dynamic;
		}

		public void SetUpTest(bool onTest)
		{
			if (onTest) {
				SetUpStartupVars();
				OnGameReady();
			} else {
				rigidbody.bodyType = RigidbodyType2D.Static;
			}

			spriteRenderer.flipX = false;

			if (runParticles.isPlaying) {
				runParticles.Stop();
			}
			animator.Play("Default", 0, 0f);

			animator.SetFloat("State", (float)State.IDLE);
			animator.SetFloat("Speed", 0f);

			rigidbody.gravityScale = onTest.ToInt();
		}
		#endregion

		#region Custom events
		void DefaultReset(bool trigger)
		{
			boxCollider.enabled = spriteRenderer.enabled = pixelManRigidbody.enabled = trigger;
			rigidbody.velocity = lastVelocity = Vector2.zero;
			hangCount = jumpBuffer = -1f;
			pixelManRigidbody.ResetSpeed();
			pixelManRigidbody.horizontalSpeed = 0f;
			horizontalSpeed = 0f;
			rigidbody.gravityScale = 0f;

			if (trigger) {
				if (deathParticles.isPlaying) deathParticles.Stop();
				deathParticles.Clear();
			}

			for (int i = 0; i < platforms.Count; i++) {
				platforms[i].wasCollided = false;
			}

			isOverPlatform = false;
			platformSpeed = 0f;
			spriteRenderer.flipY = playerBehaviour.properties.invertGravity;
		}

		protected override void OnGameReady()
		{
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Platform");
			platforms.Clear();

			foreach (GameObject g in gameObjects) {
				Platforms platform = new() {
					gameObject = g,
					code = g.GetComponent<Platform>()
				};

				platforms.Add(platform);
			}

			// if (!isEditor) {
			// 	playerCamera.cam.enabled = true;
			// }
		}

		protected override void OnGamePlayerDeath()
		{
			GeneralEventsHandler.PlayOnBackground(sounds[1]);
			DefaultReset(false);

			if (Globals.settings.general.enableParticles) {
				deathParticles.Play();
			}
			if (runParticles.isPlaying) {
				runParticles.Stop();
			}
		}

		protected override void OnGameResetObject()
		{
			transform.position = startPos;
			invertedGravity = playerBehaviour.properties.invertGravity;
			DefaultReset(true);
		}

		protected override void OnGameCheckpointEnabled()
		{
			respawnGravity = invertedGravity;
		}

		protected override void OnGameCheckpointRespawn()
		{
			transform.position = Globals.respawnPoint;
			invertedGravity = respawnGravity;
			DefaultReset(true);
		}
		#endregion

		public void SetCamera(PlayerCamera camera) {
			playerCamera = camera;
		}
	}
}
