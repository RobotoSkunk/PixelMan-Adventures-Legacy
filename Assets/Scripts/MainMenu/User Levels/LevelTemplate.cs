using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class LevelTemplate : CustomLevelButton {
		public TextMeshProUGUI nameText, idText, dateText;
		public Image syncImage;


		[HideInInspector] public string path, lvlName, id;
		[HideInInspector] public bool isSynced;
		[HideInInspector] public long date;
		[HideInInspector] public UserLevelsController controller;


		protected override void Start() {
			if (!Application.isPlaying) return;

			nameText.text = UnityEngine.Random.Range(0, 1000).ToString();
			idText.text = id == null ? "" : '#' + id;
			dateText.text = DateTime.FromFileTimeUtc(date).ToString("yyyy/MM/dd - HH:mm:ss");
			syncImage.gameObject.SetActive(isSynced);
		}

		public void OnClick() => controller.OpenLevel(path);
		public void Test() => Debug.Log("Test");
	}
}