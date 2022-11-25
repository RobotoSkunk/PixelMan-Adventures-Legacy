using UnityEngine;
using UnityEngine.SceneManagement;

using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;


using RobotoSkunk.PixelMan.Events;


namespace RobotoSkunk.PixelMan {
	public class RSLogo : MonoBehaviour {
		[Header("Components")]
		public Animator animator;
		public SceneReference mainMenuScene;

		private void Start() {
			UniTask.Void(async () => {
				animator.SetBool("Loaded", false);
				await UniTask.Delay(1000);


				string settingsJson = await Files.ReadFile(Files.Directories.settings);
				string userDataJson = await Files.ReadFile(Files.Directories.userData);

				if (!string.IsNullOrEmpty(settingsJson))
					Globals.settings = await AsyncJson.FromJson<Globals.Settings>(settingsJson);

				if (!string.IsNullOrEmpty(userDataJson))
					Globals.playerData = await AsyncJson.FromJson<Globals.PlayerData>(userDataJson);

				
				GeneralEventsHandler.InvokeSettingsLoaded();

				animator.SetBool("Loaded", true);
				await UniTask.Delay(1000);

				SceneManager.LoadScene(mainMenuScene.BuildIndex);
			});
		}
	}
}
