using UnityEngine;

namespace RobotoSkunk.PixelMan.LevelEditor {
	public class ObjectInspector : MonoBehaviour {
		public PropertiesChanges properties;
	}

	[System.Flags]
	public enum PropertiesChanges {
		None = 0,

		Position = 1 << 0,
		ScaleX = 1 << 1,
		ScaleY = 1 << 2,
		Scale = ScaleX | ScaleY,
		Rotation = 1 << 3,
		RenderOrder = 1 << 4,
		Speed = 1 << 5,
		StartupTime = 1 << 6,
		ReloadTime = 1 << 7,

		All = ~0
	}
}
