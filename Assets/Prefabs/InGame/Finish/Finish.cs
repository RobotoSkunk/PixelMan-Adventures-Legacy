using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Finish : GameHandler {
		[Header("Components")]
		public Animator anim;
		public BoxCollider2D col;
		public AudioSource audioSource;

		[Header("Shared")]
		public bool isOpen;

		readonly List<Switch> switches = new();

		protected override void OnGameReady() {
			GameObject[] tmp = GameObject.FindGameObjectsWithTag("Switch");
			switches.Clear();

			foreach (GameObject go in tmp) {
				if (!go) continue;
				if (!go.activeInHierarchy) continue;

				switches.Add(go.GetComponent<Switch>());
			}
		}

		protected override void OnGameSwitchTouched() {
			int turnedOn = 0;

			foreach (Switch sw in switches) {
				if (sw.switched) turnedOn++;
			}

			if (turnedOn == switches.Count) isOpen = true;

			if (isOpen) {
				col.enabled = true;
				anim.SetBool("IsOpen", true);
				audioSource.Play();
			}
		}

		protected override void OnGameResetObject() {
			isOpen = false;
			col.enabled = false;
			anim.SetBool("IsOpen", isOpen);
		}
	}
}
