using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;



namespace RobotoSkunk.PixelMan.LevelEditor {

	[AddComponentMenu("UI/RobotoSkunk - Drag Handler")]
	public class DragHandler : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {
		[System.Serializable] public class ClickEvent : UnityEvent { }

		[SerializeField] ClickEvent onDragBegin = new(), onDrag = new(), onDragEnd = new();
		[SerializeField] public EnumResizeHandler resHandler;
		Vector2 __point;
		RectTransform __rect;


		private void Awake() => __rect = GetComponent<RectTransform>();

		public void OnBeginDrag(PointerEventData ev) => onDragBegin.Invoke();
		public void OnDrag(PointerEventData ev) {
			UpdateHandledPoint();
			onDrag.Invoke();
		}
		public void OnEndDrag(PointerEventData ev) => onDragEnd.Invoke();

		public void UpdateHandledPoint() {
			Rect rect = __rect.rect;

			__point = resHandler.value switch {
				ResizePoints.LEFT => new(rect.xMin, rect.center.y)
			};
		}
	}
}
