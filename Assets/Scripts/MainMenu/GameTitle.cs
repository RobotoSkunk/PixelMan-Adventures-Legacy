using UnityEngine;

namespace RobotoSkunk.PixelMan.UI.MainMenu {
	public class GameTitle : MonoBehaviour {
		public float speed, maxCurve, sizeDelta;
		
		float __t = 0f;
		float minSize { get => 1.8f - sizeDelta; }


		private void Update() {
			__t += Time.deltaTime * speed;
			if (__t > 360f) __t -= 360f;

			float c = Mathf.Sin(__t * Mathf.Deg2Rad), c2 = Mathf.Sin(__t * 2f * Mathf.Deg2Rad);

			transform.localRotation = Quaternion.Euler(0f, 0f, c * maxCurve);
			transform.localScale = (Vector3)(minSize * Vector2.one) + (Vector3)(sizeDelta * c2 * Vector2.one) + Vector3.forward;
		}
	}
}
