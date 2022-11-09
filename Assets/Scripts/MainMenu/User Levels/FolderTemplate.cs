using System;

using UnityEngine;
using UnityEngine.UI;

using TMPro;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class FolderTemplate : CustomLevelButton {
		public TextMeshProUGUI nameText;

		public string path, folderName;


		protected override void Start() {
			base.Start();
			if (!Application.isPlaying) return;

			nameText.text = UnityEngine.Random.Range(0, 1000).ToString();
		}
	}
}
