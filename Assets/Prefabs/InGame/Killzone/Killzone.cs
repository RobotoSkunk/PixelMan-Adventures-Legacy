using System.Collections.Generic;
using UnityEngine;

namespace RobotoSkunk.PixelMan.Gameplay {
	public class Killzone : MonoBehaviour {
		[Header("Components")]
		public SpriteRenderer sprRend;

		[Header("Properties")]
		public ContactFilter2D ContactFilter2D;

		[Header("Shared")]
		public float radius = 5f;
		public bool enableEffects = true;

		float delta = 0f;
		readonly List<Collider2D> overlapResult = new();

		private void Start() {
			if (!enableEffects) sprRend.color = Color.clear;
		}

		private void FixedUpdate() {
			if (enableEffects) {
				int buffer = Physics2D.OverlapCircle(transform.position, radius, ContactFilter2D, overlapResult);
				float lastDistance = radius + 1f;

				if (buffer != 0) {
					foreach (Collider2D col in overlapResult) {
						float distance = Vector2.Distance(transform.position, col.transform.position);

						if (distance < lastDistance) lastDistance = distance;
					}
				}

				delta = Mathf.Lerp(delta, Mathf.Clamp01((radius - lastDistance) / radius), 0.3f);
				sprRend.color = new Color(1f, 1f, 1f, 0.2f + 0.8f * delta);
			}
		}
	}
}
