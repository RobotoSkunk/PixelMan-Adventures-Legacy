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
		#region Inspector variables
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[Header("Components")]
		[SerializeField] new Rigidbody2D rigidbody;
		[SerializeField] PMRigidbody pixelManRigidbody;
		[SerializeField] SpriteRenderer spriteRenderer;
		[SerializeField] BoxCollider2D boxCollider;
		[SerializeField] AudioSource audioSource;
		[SerializeField] Animator animator;
		[SerializeField] ParticleSystem deathParticles;
		[SerializeField] ParticleSystem runParticles;
		[SerializeField] InGameObjectBehaviour playerBehaviour;

		[Header("Properties")]
		[SerializeField] Vector2 speed;
		[SerializeField] float maxSpeed;
		[SerializeField] float maxJumpBufferTime;
		[SerializeField] float maxHangCount;
		[SerializeField] ContactFilter2D groundFilter;
		[SerializeField] AudioClip[] sounds;
		#pragma warning restore IDE0044
		#endregion

		// Public variables that are hidden in the inspector
		[HideInInspector] public Vector2 lastPublicPosition;

		#region Private variables
		/// <summary>
		/// The horizontal input axis.
		/// </summary>
		float horizontalAxis = 0f;

		/// <summary>
		/// The time before the player can jump.
		/// </summary>
		float jumpBufferTime = 0f;

		/// <summary>
		/// The time that the player can hang on the air.
		/// </summary>
		float hangCount = 0f;

		/// <summary>
		/// The horizontal speed of the player.
		/// </summary>
		float horizontalSpeed = 0f;

		/// <summary>
		/// The speed of the platform.
		/// </summary>
		float platformSpeed = 0f;

		/// <summary>
		/// The speed delay of the player to run.
		/// </summary>
		float speedFactor = 0.4f;


		/// <summary>
		/// If the player is on the ground.
		/// </summary>
		bool onGround = false;

		/// <summary>
		/// If the player can control the jump in the air.
		/// </summary>
		bool canControlJump = false;

		/// <summary>
		/// If the player can jump.
		/// </summary>
		bool canJump = true;

		/// <summary>
		/// If the player is over a platform.
		/// </summary>
		bool isOverPlatform = false;

		/// <summary>
		/// If the player's gravity is inverted.
		/// </summary>
		bool invertedGravity = false;

		/// <summary>
		/// If the game was on pause.
		/// </summary>
		bool wasOnPause = false;

		/// <summary>
		/// If the player's gravity was inverted before reaching the last checkpoint.
		/// </summary>
		bool respawnGravity;


		// Positions
		// I don't think I have to explain this.
		Vector2 startPosition;
		Vector2 lastPosition;
		Vector2 lastRigidbodyVelocity;

		// Player states
		State currentPlayerState = State.IDLE;
		State lastState = State.IDLE;

		// Lists
		readonly List<Platforms> platforms = new();
		readonly List<Collider2D> groundOverlap = new();

		// Others
		PlayerCamera playerCamera;
		#endregion


		#region Definitions
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

		/// <summary>
		/// Has the player fell from the world?
		/// </summary>
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

		/// <summary>
		/// Is the player out of level bounds? (vertical only)
		/// </summary>
		bool outOfBounds {
			get
			{
				return transform.position.y < Globals.levelData.bounds.yMin - 1f
					|| transform.position.y > Globals.levelData.bounds.yMax + 1f;
			}
		}

		/// <summary>
		/// Is the player going up?
		/// </summary>
		bool goesUp {
			get
			{
				return !invertedGravity ? rigidbody.velocity.y > 0f : rigidbody.velocity.y < 0f;
			}
		}

		/// <summary>
		/// The gravity multiplier.
		/// </summary>
		float gravityMultiplier {
			get
			{
				return invertedGravity ? -1f : 1f;
			}
		}

		/// <summary>
		/// The actual speed of the player ignoring the Rigidbody.
		/// </summary>
		Vector2 actualSpeed {
			get
			{
				return new Vector2(transform.position.x, transform.position.y) - lastPosition;
			}
		}
		#endregion


		#region Unity Methods

		private void Awake()
		{
			spriteRenderer.color = Globals.playerData.Color;

			Globals.PlayerCharacters playerData = Globals.playerCharacters.ClampIndex(
																			(int)Globals.playerData.skinIndex);
			spriteRenderer.sprite = playerData.display;
			animator.runtimeAnimatorController = playerData.controller;


			ParticleSystem.MainModule particleMain = deathParticles.main;
			particleMain.startColor = Globals.playerData.Color;

			FreezeRigidbody(true);
		}

		private void FixedUpdate()
		{
			#region On pause
			if (Globals.onPause) {
				FreezeRigidbody(true);
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
					FreezeRigidbody(false);
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
				FreezeRigidbody(true);
				return;
			}

			rigidbody.gravityScale = gravityMultiplier;

			#region Jump buffers

			// Imaginary box to check if the player is on the ground.
			int groundBuffer = Physics2D.OverlapBox(
				transform.position - new Vector3(0f, boxCollider.size.y / 2f) * gravityMultiplier,
				new Vector2(boxCollider.size.x + Constants.pixelToUnit, 0.1f),
				0f,
				groundFilter,
				groundOverlap
			);

			onGround = false;


			// Check if the player is on the ground or not.
			if (groundBuffer != 0) {
				onGround = true;
				speedFactor = 0.4f;

				switch (groundOverlap[0].tag) {
					case "Ignore":
						onGround = false;
						break;

					case "IceBlock":
						speedFactor = 0.025f;
						break;

					case "Platform":
						onGround = isOverPlatform;
						break;

					// No default
				}
			}


			if (jumpBufferTime > 0f) {
				jumpBufferTime -= Time.fixedDeltaTime;
			}

			if (onGround) {
				canJump = true;
				canControlJump = true;
				hangCount = maxHangCount;

				if (Globals.settings.general.enableParticles) {

					if (actualSpeed.x != 0f && horizontalAxis != 0f) {

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


			// Apply the jump buffer.
			if (jumpBufferTime > 0f && hangCount > 0f && !Globals.isDead) {

				rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f);

				rigidbody.AddForce(
					new Vector2(0f, speed.y) * gravityMultiplier,
					ForceMode2D.Impulse
				);


				audioSource.PlayOneShot(sounds[0]);

				hangCount = -1f;
				jumpBufferTime = -1f;
			}
			#endregion


			// Apply the horizontal speed.
			horizontalSpeed = Mathf.Lerp(horizontalSpeed, horizontalAxis * speed.x, speedFactor);

			// Minor changes to renderers and particles.
			spriteRenderer.flipY = invertedGravity;
			runParticles.transform.localScale = new Vector3(1f, gravityMultiplier, 1f);

			if (horizontalAxis != 0f) {
				spriteRenderer.flipX = horizontalAxis < 0f;
			}

			pixelManRigidbody.horizontalSpeed = horizontalSpeed + platformSpeed;


			#region Animation processing
			// I prefer doing all this by code than using Unity animator transitions manually.

			float animationSpeed = Mathf.Abs(horizontalSpeed) > 0f && Mathf.Abs(actualSpeed.x) >= 0.05f
								 ? Mathf.Abs(horizontalSpeed) / speed.x : 0f;

			float velocityYhorizontalAxis = rigidbody.gravityScale * rigidbody.velocity.y;

			if (animationSpeed <= 0.2f) {
				animationSpeed = 0.2f;
			}


			if (onGround && (Mathf.Abs(horizontalAxis) == 0f || actualSpeed.x == 0f)) {
				currentPlayerState = State.IDLE;

			} else if (onGround && Mathf.Abs(horizontalAxis) > 0f) {
				currentPlayerState = State.RUNNING;

			} else if (!onGround && velocityYhorizontalAxis > 0f) {
				currentPlayerState = State.JUMPING;

			} else if (!onGround && velocityYhorizontalAxis < 0f) {
				currentPlayerState = State.FALLING;

			} else {
				currentPlayerState = State.IDLE;
			}


			// End of styling excuse.

			if (currentPlayerState != lastState) {
				animator.Play("Default", 0, 0f);
			}

			animator.SetFloat("State", (float)currentPlayerState);
			animator.SetFloat("Speed", currentPlayerState == State.RUNNING ? animationSpeed : 1f);
			#endregion


			lastState = currentPlayerState;
			lastPosition = transform.position;
			lastRigidbodyVelocity = rigidbody.velocity;
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
			if (collision.CompareTag("Trampoline")) {
				pixelManRigidbody.ResetSpeed();

				pixelManRigidbody.AddForce(
					RSMath.GetDirVector(
						(collision.transform.eulerAngles.z + 90f) * Mathf.Deg2Rad
					) * Constants.trampolineForce,
					ForceMode2D.Impulse
				);

				lastRigidbodyVelocity = rigidbody.velocity;
				canJump = false;
				canControlJump = false;
			
			} else if (collision.CompareTag("GravitySwitch")) {
				invertedGravity = !invertedGravity;

			} else if (collision.CompareTag("Finish")) {
				Debug.Log("Won!");
			}
		}

		private void OnTriggerStay2D(Collider2D collision)
		{
			if (Globals.onPause) {
				return;
			}

			if (collision.CompareTag("Trampoline")) {
				canControlJump = false;
				canJump = false;
			}
		}

		private void OnTriggerExit2D(Collider2D collision)
		{
			if (collision.CompareTag("Trampoline")) {
				canControlJump = false;
				canJump = false;
			}
		}
		#endregion


		#region Collisions
		private void OnCollisionStay2D(Collision2D collision)
		{
			if (Globals.onPause) {
				return;
			}

			if (collision.collider.CompareTag("Platform")) {
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
					platformSpeed = (collision.transform.position.x - platform.code.lastPosition.x)
									/ Time.fixedDeltaTime;
					platform.wasCollided = true;
					isOverPlatform = true;
				}
			}
		}

		private void OnCollisionExit2D(Collision2D collision)
		{
			if (collision.collider.CompareTag("Platform")) {
				Platforms platform = platforms.Find(m => m.gameObject == collision.gameObject);

				if (platform != null) {
					if (platform.wasCollided) {
						pixelManRigidbody.AddForce(new Vector2(platformSpeed, 0f), ForceMode2D.Impulse);
						platformSpeed = 0f;

						platform.wasCollided = false;
						isOverPlatform = false;
					}
				}
			}
		}
		#endregion


		#endregion


		#region New input system
		public void HorizontalMovement(InputAction.CallbackContext context)
		{
			if (Globals.isDead) {
				return;
			}

			horizontalAxis = context.ReadValue<Vector2>().x;
		}

		public void JumpMovement(InputAction.CallbackContext context)
		{
			if (Globals.isDead || Globals.onPause) {
				return;
			}

			bool onJump = context.ReadValue<float>() > 0f;


			if (onJump && canJump) {
				jumpBufferTime = maxJumpBufferTime;

			} else if (!onJump && goesUp && canControlJump) {
				rigidbody.velocity *= new Vector2(1f, 0.5f);
				canControlJump = false;
			}
		}

		public void LookUp(InputAction.CallbackContext context)
		{
			if (Globals.onPause) {
				return;
			}

			if (playerCamera != null) {
				playerCamera.look = context.ReadValue<Vector2>();
			}
		}
		#endregion

		#region Editor methods
		public void SetUpStartupVars()
		{
			startPosition = transform.position;
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
			boxCollider.enabled = trigger;
			spriteRenderer.enabled = trigger;
			pixelManRigidbody.enabled = trigger;

			rigidbody.velocity = Vector2.zero;
			lastRigidbodyVelocity = Vector2.zero;

			hangCount = -1f;
			jumpBufferTime = -1f;

			pixelManRigidbody.ResetSpeed();
			pixelManRigidbody.horizontalSpeed = 0f;
			horizontalSpeed = 0f;
			rigidbody.gravityScale = 0f;


			if (trigger) {
				if (deathParticles.isPlaying) {
					deathParticles.Stop();
				}

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
			transform.position = startPosition;
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


		void FreezeRigidbody(bool freeze)
		{
			rigidbody.velocity = freeze ? Vector2.zero : lastRigidbodyVelocity;
			rigidbody.gravityScale = freeze ? 0f : gravityMultiplier;
			rigidbody.bodyType = freeze ? RigidbodyType2D.Static : RigidbodyType2D.Dynamic;
		}


		public void SetCamera(PlayerCamera camera)
		{
			playerCamera = camera;
		}
	}
}
