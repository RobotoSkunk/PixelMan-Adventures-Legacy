using UnityEngine;

namespace RobotoSkunk.PixelMan.Utils {

	[ExecuteInEditMode]
	[AddComponentMenu("RobotoSkunk - Misc/Player Color")]
	[RequireComponent(typeof(SpriteRenderer))]
	public class PlayerColor : MonoBehaviour {
		[Header("Components and properties")]
		public SpriteRenderer spriteRenderer;
		[Range(0, 16)] public int outlineSize = 1;
		[Range(0, 1)] public float sensitivity = 0.5f;

		[Header("Shared")]
		public bool customOutlineColor = false;
		public Color outlineColor = Color.black;

		Color __color;

		public Color color {
			get => __color;
			set {
				__color = value;
				spriteRenderer.color = __color;

				if (customOutlineColor) UpdateOutline(outlineColor);
				else {
					Color.RGBToHSV(__color, out float h, out float s, out float v);
					v = 1 - v;

					Color __tmp = Color.HSVToRGB(h, s, v);
					__tmp.a = __color.a;

					UpdateOutline(__tmp);
				}
			}
		}

		private void Awake() {
			if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
			color = spriteRenderer.color;
		}

		private void Update() {
			if (Application.isPlaying) return;

			if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
			color = spriteRenderer.color;
		}

		private void UpdateOutline(Color c) {
			MaterialPropertyBlock __block = new();
			spriteRenderer.GetPropertyBlock(__block);

			__block.SetFloat("_Outline", (c.a > 0).ToInt());
			__block.SetColor("_OutlineColor", c);
			__block.SetInteger("_OutlineSize", outlineSize);
			__block.SetFloat("_OutlineSensitivity", sensitivity);

			spriteRenderer.SetPropertyBlock(__block);
		}
	}
}
