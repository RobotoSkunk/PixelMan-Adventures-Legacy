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
