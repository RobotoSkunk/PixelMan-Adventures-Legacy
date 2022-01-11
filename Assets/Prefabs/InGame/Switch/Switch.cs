using System.Collections.Generic;
using UnityEngine;

using RobotoSkunk.PixelMan.Events;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Switch : GameHandler {
		[Header("Components")]
		public Animator anim;
		public AudioSource audioSource;

		[Header("Shared")]
		public bool switched;

		private void OnTriggerEnter2D(Collider2D collision) {
			if (!switched && collision.CompareTag("Player")) {
				switched = true;
				audioSource.Play();
				GameEventsHandler.InvokeSwitchTouched();
				anim.SetBool("Switched", true);
			}
		}

		protected override void OnGameResetObject() {
			switched = false;
			anim.SetBool("Switched", false);
		}
	}
}
