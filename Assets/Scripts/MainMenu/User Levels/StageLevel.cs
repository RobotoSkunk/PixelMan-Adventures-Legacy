using System;
using UnityEngine;

using TMPro;

using RobotoSkunk.PixelMan.LevelEditor;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class StageLevel : MonoBehaviour {
		[Header("Level Data")]
		public RSInputField lvlName;
		public TextMeshProUGUI createdAt, timeSpent;

		[HideInInspector] public Level.UserMetadata data;
		[HideInInspector] public UserStageMenuController controller;


		private void Start() {
			lvlName.SetTextWithoutNotify(data.name);
			createdAt.text = RSTime.FromUnixTimestamp(data.createdAt).ToString("yyyy-MM-dd HH:mm:ss");
			timeSpent.text = TimeSpan.FromMilliseconds(data.timeSpent).ToString(@"hh\:mm\:ss");
		}

		public void SetCurrentLevel() => Globals.Editor.currentLevel = data;
		public void DeleteLevel() {
			SetCurrentLevel();
			controller.TogglePopup(true);
			controller.SetPopupIndex(1);
		}

		public void SetName(string name) => controller.SetLevelName(data.uuid, name);
		public void MoveUp() {
			SetCurrentLevel();
			controller.MoveLevelUp();
		}
		public void MoveDown() {
			SetCurrentLevel();
			controller.MoveLevelDown();
		}
	}
}
