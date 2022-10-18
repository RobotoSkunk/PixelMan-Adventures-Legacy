using UnityEngine;


namespace RobotoSkunk.PixelMan.Utils {
	public static class DrawUtils {
		public static void DrawSquare(this Texture2D tex, Vector2 pos, Vector2 size, Color32 color) {
			int x = (int)pos.x;
			int y = (int)pos.y;
			int w = (int)size.x;
			int h = (int)size.y;

			for (int i = 0; i < h; i++)
				for (int j = 0; j < w; j++)
					tex.SetPixel(x + j, y + i, color);
		}

		public static void DrawCircle(this Texture2D tex, Vector2 pos, int radius, Color32 color) {
			int x = (int)pos.x;
			int y = (int)pos.y;

			for (int u = x - radius; u < x + radius + 1; u++)
				for (int v = y - radius; v < y + radius + 1; v++)
					if (Mathf.Pow(u - x, 2) + Mathf.Pow(v - y, 2) <= Mathf.Pow(radius, 2))
						tex.SetPixel(u, v, color);
		}

		public static void DrawLine(this Texture2D tex, Vector2 p1, Vector2 p2, Color32 color) {
			// Bresenham's line algorithm
			int x0 = (int)p1.x;
			int y0 = (int)p1.y;
			int x1 = (int)p2.x;
			int y1 = (int)p2.y;

			int dx = Mathf.Abs(x1 - x0);
			int sx = x0 < x1 ? 1 : -1;
			int dy = -Mathf.Abs(y1 - y0);
			int sy = y0 < y1 ? 1 : -1;
			int error = dx + dy;

			while (true) {
				tex.SetPixel(x0, y0, color);
				if (x0 == x1 && y0 == y1)
					break;
				int e2 = 2 * error;
				if (e2 >= dy) {
					if (x0 == x1)
						break;
					error += dy;
					x0 += sx;
				}
				if (e2 <= dx) {
					if (y0 == y1)
						break;
					error += dx;
					y0 += sy;
				}
			}
		}

		public static void SetColor(this Texture2D tex, Color32 color) {
			for (int i = 0; i < tex.width; i++)
				for (int j = 0; j < tex.height; j++)
					tex.SetPixel(i, j, color);
		}
	}
}
