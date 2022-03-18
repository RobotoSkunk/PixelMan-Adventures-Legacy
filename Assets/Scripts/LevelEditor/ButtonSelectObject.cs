using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using RobotoSkunk.PixelMan.UI;


namespace RobotoSkunk.PixelMan.LevelEditor {
	public class ButtonSelectObject : MonoBehaviour, ISelectHandler {
		[System.Serializable] public class BtnEvent : UnityEvent { }

		public Image preview;
		public RSButton button;
		public BtnEvent onSelect = new();

		public void OnSelect(BaseEventData ev) => onSelect.Invoke();
	}
}
