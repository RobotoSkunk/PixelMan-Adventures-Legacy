/*
	PixelMan Adventures, an open source platformer game.
	Copyright (C) 2022  RobotoSkunk <contact@robotoskunk.com>

	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU Affero General Public License as published
	by the Free Software Foundation, either version 3 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU Affero General Public License for more details.

	You should have received a copy of the GNU Affero General Public License
	along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using UnityEngine;

using RobotoSkunk;
using RobotoSkunk.PixelMan;
using RobotoSkunk.PixelMan.LevelEditor;


namespace RobootSkunk.PixelMan.LevelEditor
{
	public class DragBoxResizer : MonoBehaviour
	{
		#pragma warning disable IDE0044
		// Excuse: The inspector can't show the variables if they are readonly.

		[SerializeField] RectTransform rectTransform;
		public bool clampToGridBorder = true;
		[SerializeField] Vector2 minSize = new(1f, 1f);

		public bool isResizing { get; private set; } = false;
		public Rect rect => resizeArea;

		#pragma warning restore IDE0044


		Vector2 resizeReferencePoint;
		Rect resizeArea;
		Rect lastResizeArea;


		public void Start() {
			SetRect(rectTransform.rect);
		}

		public void SetRect(Rect rect)
		{
			lastResizeArea = resizeArea = rect;
			rectTransform.transform.position = resizeArea.center;
			rectTransform.sizeDelta = resizeArea.size;
		}


		public void StartResizing(EditorDragArea handler)
		{
			isResizing = true;

			resizeReferencePoint = handler.resizePoint switch {
				ResizePoints.TOP          => new(0f, lastResizeArea.yMin),
				ResizePoints.RIGHT        => new(lastResizeArea.xMin, 0f),
				ResizePoints.BOTTOM       => new(0f, lastResizeArea.yMax),
				ResizePoints.LEFT         => new(lastResizeArea.xMax, 0f),

				ResizePoints.TOP_LEFT     => new(lastResizeArea.xMax, lastResizeArea.yMin),
				ResizePoints.TOP_RIGHT    => new(lastResizeArea.xMin, lastResizeArea.yMin),
				ResizePoints.BOTTOM_LEFT  => new(lastResizeArea.xMax, lastResizeArea.yMax),
				ResizePoints.BOTTOM_RIGHT => new(lastResizeArea.xMin, lastResizeArea.yMax),

				_ => new()
			};
		}

		public void UpdateResizing(EditorDragArea handler)
		{
			Vector4 a = resizeArea.MinMaxToVec4();
			Vector2 B = Globals.Editor.virtualMousePosition;

			if (clampToGridBorder) {
				B += new Vector2(
					Globals.Editor.virtualMousePosition.x < resizeReferencePoint.x ? 0.5f : -0.5f,
					Globals.Editor.virtualMousePosition.y < resizeReferencePoint.y ? 0.5f : -0.5f
				);
			}

			if (Mathf.Abs(Globals.Editor.virtualMousePosition.x - resizeReferencePoint.x) <= 0.5f) {
				B.x = resizeReferencePoint.x;
			}
			if (Mathf.Abs(Globals.Editor.virtualMousePosition.y - resizeReferencePoint.y) <= 0.5f) {
				B.y = resizeReferencePoint.y;
			}

			Vector4 newData = handler.resizePoint switch {
				ResizePoints.TOP          => new(a.x, a.y, a.z, B.y),
				ResizePoints.RIGHT        => new(a.x, a.y, B.x, a.w),
				ResizePoints.BOTTOM       => new(a.x, B.y, a.z, a.w),
				ResizePoints.LEFT         => new(B.x, a.y, a.z, a.w),

				ResizePoints.TOP_LEFT     => new(B.x, a.y, a.z, B.y),
				ResizePoints.TOP_RIGHT    => new(a.x, a.y, B.x, B.y),
				ResizePoints.BOTTOM_LEFT  => new(B.x, B.y, a.z, a.w),
				ResizePoints.BOTTOM_RIGHT => new(a.x, B.y, B.x, a.w),

				_ => new()
			};


			Rect rect = Rect.MinMaxRect(newData.x, newData.y, newData.z, newData.w);

			if (rect.width < minSize.x) {
				if (
					handler.resizePoint == ResizePoints.LEFT ||
					handler.resizePoint == ResizePoints.TOP_LEFT ||
					handler.resizePoint == ResizePoints.BOTTOM_LEFT
				) {
					rect.xMin = rect.xMax - minSize.x;
				} else {
					rect.xMax = rect.xMin + minSize.x;
				}
			}

			if (rect.height < minSize.y) {
				if (
					handler.resizePoint == ResizePoints.BOTTOM ||
					handler.resizePoint == ResizePoints.BOTTOM_LEFT ||
					handler.resizePoint == ResizePoints.BOTTOM_RIGHT
				) {
					rect.yMin = rect.yMax - minSize.y;
				} else {
					rect.yMax = rect.yMin + minSize.y;
				}
			}

			resizeArea = rect;

			rectTransform.transform.position = resizeArea.center;
			rectTransform.sizeDelta = resizeArea.size;
		}

		public void StopResizing()
		{
			isResizing = false;
			lastResizeArea = rectTransform.rect;
		}
	}
}
