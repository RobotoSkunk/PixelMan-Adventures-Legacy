using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;



namespace RobotoSkunk.PixelMan.UI {

	[AddComponentMenu("UI/RobotoSkunk - Drag Handler")]
	public class DragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
		[System.Serializable] public class ClickEvent : UnityEvent { }

		[SerializeField] ClickEvent onDragBegin = new(), onDrag = new(), onDragEnd = new();


		public virtual void OnBeginDrag(PointerEventData ev) => onDragBegin.Invoke();
		public virtual void OnDrag(PointerEventData ev) => onDrag.Invoke();
		public virtual void OnEndDrag(PointerEventData ev) => onDragEnd.Invoke();
	}
}
