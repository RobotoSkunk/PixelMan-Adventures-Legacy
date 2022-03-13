using UnityEngine;
using UnityEngine.EventSystems;

using RobotoSkunk.PixelMan.UI;


namespace RobotoSkunk.PixelMan.LevelEditor {
	public class ResizeDragArea : DragHandler {
		public ResizePoints resizePoint;

		public Vector2 point { get => __pnt; }

		readonly Vector3[] corners = new Vector3[4];

		[SerializeField]
		RectTransform rectTransform;
		Vector2 __pnt;


		public override void OnBeginDrag(PointerEventData ev) {
			SetPoint();
			base.OnBeginDrag(ev);
		}

		public override void OnDrag(PointerEventData ev) {
			SetPoint();
			base.OnDrag(ev);
		}

		void SetPoint() {
			Rect rect = GetRect();

			__pnt = resizePoint switch {
				ResizePoints.LEFT => new(rect.xMin, rect.center.y),
				ResizePoints.TOP => new(rect.center.x, rect.yMax),
				ResizePoints.RIGHT => new(rect.xMax, rect.center.y),
				ResizePoints.BOTTOM => new(rect.center.x, rect.yMin),

				ResizePoints.TOP_LEFT => new(rect.xMin, rect.yMax),
				ResizePoints.TOP_RIGHT => new(rect.xMax, rect.yMax),
				ResizePoints.BOTTOM_RIGHT => new(rect.xMax, rect.yMin),
				ResizePoints.BOTTOM_LEFT => new(rect.xMin, rect.yMin),

				_ => new()
			};
		}

		public Rect GetRect() {
			rectTransform.GetWorldCorners(corners);

			return new Rect(corners[0], rectTransform.lossyScale * rectTransform.rect.size);
		}
	}
}
