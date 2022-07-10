using System.Collections;
using UnityEngine;


using RobotoSkunk.PixelMan.Events;


namespace RobotoSkunk.PixelMan.Gameplay {
	public class GameController : MonoBehaviour {
		IEnumerator StartGame() {
			yield return new WaitForSeconds(1f);
			EditorEventsHandler.InvokeStartTesting();
			GameEventsHandler.InvokeLevelReady();
		}
	}
}
