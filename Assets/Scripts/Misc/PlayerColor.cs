using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RobotoSkunk.PixelMan.Misc {
	[AddComponentMenu("RobotoSkunk - Misc/Player Color")]
	[RequireComponent(typeof(SpriteRenderer))]
	public class PlayerColor : MonoBehaviour {
		public SpriteRenderer spriteRenderer;
		[Range(0, 16)] public float outlineSize;

		Color __color;

		public Color color {
			get => __color;
			set {
				__color = value;
				spriteRenderer.color = __color;
				// UpdateOutline();
			}
		}

		private void Awake() {
			if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
			color = spriteRenderer.color;
		}

		private void UpdateOutline() {
			// spriteRenderer.material.SetColor("_Color", __color);
			// MaterialPropertyBlock __block = new();
			// spriteRenderer.GetPropertyBlock(__block);

			// __block.SetFloat("_Outline", 1f);
			// __block.SetColor("_OutlineColor", Color.blue);
			// __block.SetFloat("_OutlineSize", outlineSize);

			// spriteRenderer.SetPropertyBlock(__block);
		}
	}
}
