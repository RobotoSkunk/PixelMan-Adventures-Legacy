using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using RobotoSkunk.PixelMan.Physics;
using RobotoSkunk.PixelMan.Events;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Player : GameHandler {
		[Header("Components")]
		public Rigidbody2D rb;
		public PMRigidbody pmrb;
		public SpriteRenderer spr;
		public BoxCollider2D boxCol;
		public AudioSource audSource;
		public Animator anim;
		public ParticleSystem deathParticles, runParticles;
		public InGameObjectBehaviour playerBehaviour;

		[Header("Properties")]
		public Vector2 speed;
		public float maxSpeed, maxJumpBuffer, maxHangCount;
		public ContactFilter2D groundFilter;
		public AudioClip[] sounds;
		public PlayerCamera cam;

		[Header("Shared")]
		public Vector2 lastPublicPos;

		float axis = 0f, jumpBuffer = 0f, hangCount = 0f, hSpeed = 0f, pSpeed = 0f, speedFactor = 0.4f;
		bool onGround = false, canControlJump = false, canJump = true, overPlatform = false, invertedGravity = false, wasOnPause = false, respawnGravity;
		Vector2 startPos, lastPos, lastVelocity;
		readonly List<Platforms> platforms = new();
		State state = State.IDLE, lastState = State.IDLE;

		readonly List<Collider2D> stuckResult = new(), groundOverlap = new();

		protected class Platforms {
			public GameObject gameObject;
			public bool wasCollided;
			public Platform code;
		}

		public enum State { IDLE, RUNNING, JUMPING, FALLING }

		private void Awake() {
			Globals.playerData.color.a = 1f;
			spr.color = Globals.playerData.color;

			Globals.PlayerCharacters ps = Globals.playerCharacters.ClampIndex((int)Globals.playerData.skinIndex);
			spr.sprite = ps.display;
			anim.runtimeAnimatorController = ps.controller;

			ParticleSystem.MainModule particleMain = deathParticles.main;
			particleMain.startColor = Globals.playerData.color;

			rb.velocity = Vector2.zero;
			rb.gravityScale = 0f;
			rb.bodyType = RigidbodyType2D.Static;
		}

		private void FixedUpdate() {
			#region On pause
			if (Globals.onPause) {
				rb.velocity = Vector2.zero;
				rb.gravityScale = 0f;
				wasOnPause = true;
				anim.speed = 0;

				if (!runParticles.isPaused && runParticles.isPlaying) runParticles.Pause();
				if (!deathParticles.isPaused && deathParticles.isPlaying) deathParticles.Pause();
				return;
			} else if (wasOnPause) {
				wasOnPause = false;
				anim.speed = 1;

				if (!Globals.isDead) rb.velocity = lastVelocity;

				if (runParticles.isPaused) runParticles.Play();
				if (deathParticles.isPaused) deathParticles.Play();
			}
			#endregion

			if (Globals.isDead) {
				rb.velocity = Vector2.zero;
				return;
			}

			rb.gravityScale = invertedGravity ? -1f : 1f;

			Vector2 spd = new Vector2(transform.position.x, transform.position.y) - lastPos;

			#region Is stuck?
			int stuckBuffer = Physics2D.OverlapBox(transform.position, boxCol.size - new Vector2(0.05f, 0.05f), 0f, groundFilter, stuckResult);
			
			if (stuckBuffer != 0) {
				foreach (Collider2D stuck in stuckResult) {
					if (!stuck.CompareTag("Platform") && !stuck.CompareTag("Ignore")) Globals.isDead = true;
				}
			}
			#endregion

			#region Jump buffers
			int groundBuffer = Physics2D.OverlapBox(transform.position - new Vector3(0f, boxCol.size.y / 2f) * rb.gravityScale, new Vector2(0.3f, 0.1f), 0f, groundFilter, groundOverlap);
			onGround = false;

			if (groundBuffer != 0) {
				onGround = true;
				speedFactor = 0.4f;

				switch (groundOverlap[0].tag) {
					case "Ignore": onGround = false; break;
					case "IceBlock": speedFactor = 0.025f; break;
					case "Platform": onGround = overPlatform; break;
				}
			}

			if (jumpBuffer > 0f)
				jumpBuffer -= Time.fixedDeltaTime;

			if (onGround) {
				canJump = true;
				canControlJump = true;
				hangCount = maxHangCount;
				if (Globals.settings.general.enableParticles) {
					if (spd.x != 0f && axis != 0f) {
						if (!runParticles.isPlaying) runParticles.Play();
					} else if (runParticles.isPlaying) runParticles.Stop();
				}
			} else {
				if (hangCount > 0f) hangCount -= Time.fixedDeltaTime;
				if (runParticles.isPlaying) runParticles.Stop();
			}

			if (jumpBuffer > 0f && hangCount > 0f && !Globals.isDead) {
				rb.velocity = new Vector2(rb.velocity.x, 0f);
				rb.AddForce(new Vector2(0f, speed.y) * rb.gravityScale, ForceMode2D.Impulse);

				audSource.PlayOneShot(sounds[0]);

				hangCount = -1f;
				jumpBuffer = -1f;
			}
			#endregion

			hSpeed = Mathf.Lerp(hSpeed, axis * speed.x, speedFactor);

			spr.flipY = invertedGravity;
			runParticles.transform.localScale = new Vector3(1f, rb.gravityScale, 1f);
			if (axis != 0f) spr.flipX = axis < 0f;

			pmrb.hSpeed = hSpeed + pSpeed;

			#region Animation processing
			// I prefer doing all this by code than using Unity animatior transitions manually.
			float animSpeed = Mathf.Abs(hSpeed) > 0f && Mathf.Abs(spd.x) >= 0.05f ? Mathf.Abs(hSpeed) / speed.x : 0f,
				vY = rb.gravityScale * rb.velocity.y;

			if (animSpeed <= 0.2f) animSpeed = 0.2f;

			if (onGround && (Mathf.Abs(axis) == 0f || spd.x == 0f)) state = State.IDLE;
			else if (onGround && Mathf.Abs(axis) > 0f) state = State.RUNNING;
			else if (!onGround && vY > 0f) state = State.JUMPING;
			else if (!onGround && vY < 0f) state = State.FALLING;
			else state = State.IDLE;

			if (state != lastState) anim.Play("Default", 0, 0f);
			lastState = state;

			anim.SetFloat("State", (float)state);
			anim.SetFloat("Speed", state == State.RUNNING ? animSpeed : 1f);
			#endregion

			lastPos = transform.position;
			lastVelocity = rb.velocity;
		}

		#region Triggers
		private void OnTriggerEnter2D(Collider2D collision) {
			switch (collision.tag) {
				case "Trampoline":
					pmrb.ResetSpeed();
					pmrb.AddForce(RSMath.GetDirVector((collision.transform.eulerAngles.z + 90f) * Mathf.Deg2Rad) * Constants.trampolineForce, ForceMode2D.Impulse);
					lastVelocity = rb.velocity;
					canJump = false;
					break;
				case "GravitySwitch": invertedGravity = !invertedGravity;break;
				case "Finish": Debug.Log("Won!"); break;
				default:
					if (collision.gameObject.layer == LayerMask.NameToLayer("Killzone")) Globals.isDead = true;
					break;
			}
		}

		private void OnTriggerStay2D(Collider2D collision) {
			if (Globals.onPause) return;

			switch (collision.tag) {
				case "Trampoline": canJump = false; break;
			}
		}

		private void OnTriggerExit2D(Collider2D collision) {
			switch (collision.tag) {
				case "Trampoline": canJump = false; break;
			}
		}
		#endregion

		#region Collisions
		private void OnCollisionStay2D(Collision2D collision) {
			if (Globals.onPause) return;

			switch (collision.collider.tag) {
				case "Platform":
					Platforms p = platforms.Find(m => m.gameObject == collision.gameObject);

					float pTop = collision.transform.position.y + 0.27f,
						pBottom = collision.transform.position.y - 0.27f,
						selfTop = transform.position.y + boxCol.size.y / 2f,
						selfBottom = transform.position.y - boxCol.size.y / 2f;

					overPlatform = false;

					if (p != null && ((pTop < selfBottom && !invertedGravity) || (pBottom > selfTop && invertedGravity))) {
						pSpeed = (collision.transform.position.x - p.code.lastPosition.x) / Time.fixedDeltaTime;
						p.wasCollided = true;
						overPlatform = true;
					}
					break;
			}
		}

		private void OnCollisionExit2D(Collision2D collision) {
			switch (collision.collider.tag) {
				case "Platform":
					Platforms p = platforms.Find(m => m.gameObject == collision.gameObject);

					if (p != null) {
						if (p.wasCollided) {
							pmrb.AddForce(new Vector2(pSpeed, 0f), ForceMode2D.Impulse);
							pSpeed = 0f;

							p.wasCollided = false;
							overPlatform = false;
						}
					}
					break;
			}
		}
		#endregion

		#region New input system
		public void HorizontalMovement(InputAction.CallbackContext context) {
			axis = context.ReadValue<Vector2>().x;

			if (Globals.isDead) return;
		}

		public void JumpMovement(InputAction.CallbackContext context) {
			bool onJump = context.ReadValue<float>() > 0f,
				goesUp = !invertedGravity ? rb.velocity.y > 0f : rb.velocity.y < 0f;

			if (Globals.isDead) return;

			if (onJump && canJump) {
				jumpBuffer = maxJumpBuffer;
			} else if (!onJump && goesUp && canControlJump) {
				rb.velocity *= new Vector2(1f, 0.5f);
				canControlJump = false;
			}
		}

		public void LookUp(InputAction.CallbackContext context) => cam.look = context.ReadValue<Vector2>();
		#endregion

		#region Editor methods
		public void SetUpStartupVars() {
			startPos = transform.position;
			invertedGravity = playerBehaviour.properties.invertGravity;
			rb.bodyType = RigidbodyType2D.Dynamic;
		}
		public void SetUpTest(bool onTest) {
			if (onTest) {
				SetUpStartupVars();
				OnGameReady();
			} else {
				rb.bodyType = RigidbodyType2D.Static;
			}

			spr.flipX = false;

			if (runParticles.isPlaying) runParticles.Stop();
			anim.Play("Default", 0, 0f);

			anim.SetFloat("State", (float)State.IDLE);
			anim.SetFloat("Speed", 0f);

			rb.gravityScale = onTest.ToInt();
		}
		#endregion

		#region Custom events
		void DefaultReset(bool trigger) {
			boxCol.enabled = spr.enabled = pmrb.enabled = trigger;
			rb.velocity = lastVelocity = Vector2.zero;
			hangCount = jumpBuffer = -1f;
			pmrb.ResetSpeed();
			pmrb.hSpeed = 0f;
			hSpeed = 0f;
			rb.gravityScale = 0f;

			if (trigger) {
				if (deathParticles.isPlaying) deathParticles.Stop();
				deathParticles.Clear();
			}

			for (int i = 0; i < platforms.Count; i++) platforms[i].wasCollided = false;
			overPlatform = false;
			pSpeed = 0f;
			spr.flipY = playerBehaviour.properties.invertGravity;
		}

		protected override void OnGameReady() {
			GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Platform");
			platforms.Clear();

			foreach (GameObject g in gameObjects) {
				Platforms p = new() {
					gameObject = g,
					code = g.GetComponent<Platform>()
				};

				platforms.Add(p);
			}
		}

		protected override void OnGamePlayerDeath() {
			GeneralEventsHandler.PlayOnBackground(sounds[1]);
			DefaultReset(false);

			if (Globals.settings.general.enableParticles) deathParticles.Play();
			if (runParticles.isPlaying) runParticles.Stop();
		}

		protected override void OnGameResetObject() {
			transform.position = startPos;
			invertedGravity = playerBehaviour.properties.invertGravity;
			DefaultReset(true);
		}

		protected override void OnGameCheckpointEnabled() {
			respawnGravity = invertedGravity;
		}

		protected override void OnGameCheckpointRespawn() {
			transform.position = Globals.respawnPoint;
			invertedGravity = respawnGravity;
			DefaultReset(true);
		}
		#endregion
	}
}
