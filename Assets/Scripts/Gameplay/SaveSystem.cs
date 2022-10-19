using System;


namespace RobotoSkunk.PixelMan.Gameplay {
	[Serializable]
	public struct SaveData {
		public float time;
		public bool coin;

		public bool Won() => time > 0f;
		public bool WonInsideTimeLimit(float timeLimit) => time < timeLimit;
	}
}
