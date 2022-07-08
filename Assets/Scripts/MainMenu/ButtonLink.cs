using UnityEngine;


namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class ButtonLink : MonoBehaviour {
		public string url;
		public void OnClick() => Application.OpenURL(url);
	}
}
