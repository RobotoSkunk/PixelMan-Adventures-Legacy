using UnityEngine;
using UnityEngine.InputSystem;

namespace RobotoSkunk.PixelMan.LevelEditor {
	public class MainEditor : MonoBehaviour {
		[Header("Components")]
		public Camera cam;
		public MeshRenderer grids;

		[Header("Properties")]
		public float navigationSpeed;

		Vector2 cameraSize {
			get {
				float h = cam.orthographicSize * 2f, w = h * cam.aspect;
				return new(w, h);
			}
		}

		Material g_Material;
		Vector2 navSpeed;
		InputType inputType;

		private void Start() {
			g_Material = grids.material;
			// Globals.musicType = MainCore.MusicClips.Type.EDITOR;
		}

		private void Update() {
			transform.position += navigationSpeed * RSTime.delta * (Vector3)navSpeed;
		}

		private void LateUpdate() {
			grids.transform.localScale = (Vector3)(cameraSize / 10f) + Vector3.forward;
			g_Material.SetFloat("_Thickness", (10f / Screen.height) * 0.1f);
			g_Material.SetColor("_MainColor", Color.white);
			g_Material.SetColor("_SecondaryColor", new Color(1, 1, 1, 0.2f));
		}



		public void OnMousePosition(InputAction.CallbackContext context) {
			Vector2 val = context.ReadValue<Vector2>();

			switch (inputType) {
				case InputType.Gamepad: navSpeed = val; break;
			}
		}

		public void OnNavigation(InputAction.CallbackContext context) {
			navSpeed = context.ReadValue<Vector2>();
		}

		public void OnDragNavigation(InputAction.CallbackContext context) {
			bool isActive = context.ReadValue<float>() != 0f;
		}

		public void OnControlsChanged(PlayerInput input) {
			inputType = input.currentControlScheme switch {
				"Gamepad" => InputType.Gamepad,
				"Keyboard&Mouse" => InputType.KeyboardAndMouse,
				"Touch" => InputType.Touch,
				_ => InputType.KeyboardAndMouse,
			};
		}
	}
}
