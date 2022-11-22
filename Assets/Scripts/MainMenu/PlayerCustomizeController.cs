using System;
using System.Collections;

using Cysharp.Threading.Tasks;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using RobotoSkunk.PixelMan.Events;
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

		Coroutine saveRoutine;


		private void Start() {
			playerPreview.color = Globals.playerData.Color;
			colorPicker.SetValueWithoutNotify(Globals.playerData.Color);

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

			if (__state != __lastState) {
				ResetAnimation();
				__lastState = __state;
			}
		}

		private void OnEnable() => GeneralEventsHandler.SettingsLoaded += SettingsLoaded;
		private void OnDestroy() => GeneralEventsHandler.SettingsLoaded -= SettingsLoaded;

		void SettingsLoaded() {
			SetSkin((int)Globals.playerData.skinIndex);
			SetColor(Globals.playerData.Color);

			colorPicker.SetValueWithoutNotify(Globals.playerData.Color);
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
			SaveMiddleware();
		}
		public void SetColor(Color color) {
			Globals.playerData.Color = playerPreview.color = color;

			Color.RGBToHSV(color, out float h, out float s, out float v);
			avatarOutline.effectColor = Color.HSVToRGB(h, s, 1f - v);

			SaveMiddleware();
		}

		public void SaveMiddleware() {
			if (saveRoutine != null) StopCoroutine(saveRoutine);
			saveRoutine = StartCoroutine(Save());
		}

		IEnumerator Save() {
			yield return new WaitForSeconds(0.5f);

			UniTask.Void(async () => {
				await Globals.playerData.Save();

				saveRoutine = null;
			});
		}
	}
}
