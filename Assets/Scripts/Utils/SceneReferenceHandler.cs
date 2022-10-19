using UnityEngine;
using Eflatun.SceneReference;
using RobotoSkunk.PixelMan.Events;



namespace RobotoSkunk.PixelMan.Utils {
	public class SceneReferenceHandler : MonoBehaviour {
		public SceneReference scene;

		public void GoToScene() => GeneralEventsHandler.ChangeScene(scene);
	}
}
