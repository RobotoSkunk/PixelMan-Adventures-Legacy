using UnityEngine;

using RobotoSkunk.PixelMan.Misc;
using RobotoSkunk.PixelMan.Gameplay;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class MenuDeco : MonoBehaviour {
		[Header("Components")]
		public Animator animator;
		public GameObject ground;
		public PlayerColor playerSprite;

		[Header("Properties")]
		public float groundSpeed;


		readonly Player.State __playerState = Player.State.RUNNING;
		int __lastPlayerID;


		private void Awake() => SetPlayerSkin();

		private void Update() {
			ground.transform.Translate(groundSpeed * Time.deltaTime * Vector3.left);

			if (ground.transform.position.x < -5f)
				ground.transform.position += new Vector3(10f, 0f, 0f);

			if ((int)Globals.playerData.skinIndex != __lastPlayerID) SetPlayerSkin();
			playerSprite.color = Globals.playerData.color;
		}


		void SetPlayerSkin() {
			__lastPlayerID = (int)Globals.playerData.skinIndex;
			Globals.PlayerCharacters ps = Globals.playerCharacters.ClampIndex(__lastPlayerID);
			animator.runtimeAnimatorController = ps.controller;

			animator.SetFloat("Speed", 1f);
			animator.SetFloat("State", (int)__playerState);
		}
	}
}
