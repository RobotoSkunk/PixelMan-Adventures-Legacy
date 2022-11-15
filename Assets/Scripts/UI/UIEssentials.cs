using UnityEngine;
// using UnityEngine.UI;
using UnityEngine.Events;

// using UnityEditor;
// using UnityEditor.UI;



namespace RobotoSkunk.PixelMan.UI {
	public class UIEssentials : MonoBehaviour {
		[System.Serializable]
		public class EssentialsEvent<T> : UnityEvent<T> { }
		public EssentialsEvent<string> onButtonSubmit = new();

		public void OnButtonSubmit(RSInputField inputField) => onButtonSubmit.Invoke(inputField.text);
	}

	// [CustomEditor(typeof(UIEssentials))]
	// [CanEditMultipleObjects]
	// public class UIEssentialsEditor : Editor {
		
	// }
}
