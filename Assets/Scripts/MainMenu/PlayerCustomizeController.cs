using System;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using RobotoSkunk.PixelMan.Gameplay;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class PlayerCustomizeController : MonoBehaviour {
		[Header("Components")]
		public ColorPicker colorPicker;
		public Image playerPreview;
		public SkinSelector skinSelectorButton;
		public Transform buttonsContainer;
		public TextMeshProUGUI avatarName;
		public Outline avatarOutline;

		[Header("Garbage")]
		public SpriteRenderer playerSprite;
		public Animator playerAnimator;


		private Player.State __state, __lastState;
		private readonly float maxPrevSize = 17f;


		private void Start() {
			playerPreview.color = colorPicker.color = Globals.playerData.color;

			SetState(Player.State.IDLE);
			SetPreviewPlayer();
			SetPreviewSprite();

			for (int i = 0; i < Globals.playerCharacters.Length; i++) {
				SkinSelector sks = Instantiate(skinSelectorButton, buttonsContainer);
				Sprite spr = Globals.playerCharacters[i].display;

				sks.playerPreview.sprite = spr;
				sks.playerPreview.SetNativeSize();
				
				float max = Mathf.Max(spr.rect.size.x, spr.rect.size.y);
				if (max > maxPrevSize) {
					Vector3 newSize = maxPrevSize / max * sks.playerPreview.rectTransform.localScale;
					newSize.z = 1;

					sks.playerPreview.rectTransform.localScale = newSize;
				}

				int x = i;
				sks.button.onClick.AddListener(() => SetSkin(x));
			}
		}

		private void Update() {
			if (playerPreview.sprite != playerSprite.sprite) SetPreviewSprite();
			Globals.playerData.color = playerPreview.color = colorPicker.color;

			Color.RGBToHSV(colorPicker.color, out float h, out float s, out float v);
			avatarOutline.effectColor = Color.HSVToRGB(h, s, 1f - v);


			if (__state != __lastState) {
				ResetAnimation();
				__lastState = __state;
			}
		}

		void SetPreviewSprite() {
			if (playerSprite.sprite == null) return;

			playerPreview.sprite = playerSprite.sprite;
			playerPreview.SetNativeSize();
			playerPreview.rectTransform.pivot = playerSprite.sprite.pivot / playerSprite.sprite.rect.size;
		}

		void SetPreviewPlayer() {
			Globals.PlayerCharacters ps = Globals.playerCharacters.ClampIndex((int)Globals.playerData.skinIndex);
			playerAnimator.runtimeAnimatorController = ps.controller;
			ResetAnimation();

			avatarName.text = ps.name;
		}

		void ResetAnimation() {
			playerAnimator.SetFloat("State", (int)__state);
			playerAnimator.Play("Default", 0, 0f);
			playerAnimator.SetFloat("Speed", 1f);
		}

		public void AddState(int x) {
			int newX = (int)__state + x,
				max = Enum.GetValues(typeof(Player.State)).Length - 1;

			newX = newX < 0 ? max : newX > max ? 0 : newX;
			SetState((Player.State)newX);
		}
		public void SetState(Player.State state) => __state = state;

		public void SetSkin(int index) {
			Globals.playerData.skinIndex = (uint)index;
			SetPreviewPlayer();
		}
	}
}
