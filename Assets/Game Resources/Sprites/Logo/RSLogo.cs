using UnityEngine;
using UnityEngine.SceneManagement;

using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;


public class RSLogo : MonoBehaviour {
	[Header("Components")]
	public Animator animator;
	public SceneReference mainMenuScene;

	private void Start() {
		UniTask.Void(async () => {
			animator.SetBool("Loaded", false);
			await UniTask.Delay(1000);

			animator.SetBool("Loaded", true);
			await UniTask.Delay(1000);

			SceneManager.LoadScene(mainMenuScene.BuildIndex);
		});
	}
}
