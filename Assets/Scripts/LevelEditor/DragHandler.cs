using System.Collections.Generic;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;



namespace RobotoSkunk.PixelMan.LevelEditor {

	[AddComponentMenu("UI/RobotoSkunk - Drag Handler")]
	public class DragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
		[System.Serializable] public class ClickEvent : UnityEvent { }


		[SerializeField]
		ClickEvent onDragBegin = new(), onDrag = new(), onDragEnd = new();

		public void OnBeginDrag(PointerEventData ev) => onDragBegin.Invoke();
		public void OnDrag(PointerEventData ev) => onDrag.Invoke();
		public void OnEndDrag(PointerEventData ev) => onDragEnd.Invoke();
	}
}
