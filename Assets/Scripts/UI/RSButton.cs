using UnityEngine;
using UnityEngine.UI;

namespace RobotoSkunk.PixelMan.UI {
	[System.Serializable]
	public struct IntelliNav {
		[Tooltip("When enabled, the script will find automatically some available selectable if selectable field is null.")]
		public bool useAutomatic;
		public Selectable selectable;
	}

	public class RSButton : Button {
		public IntelliNav selectOnUp;
		public IntelliNav selectOnDown;
		public IntelliNav selectOnLeft;
		public IntelliNav selectOnRight;
		public int asdasda;

		public override Selectable FindSelectableOnUp() {
			if (!selectOnUp.useAutomatic && navigation.mode != Navigation.Mode.None) return null;

			return selectOnUp.selectable != null ? selectOnUp.selectable : base.FindSelectableOnUp();
		}

		public override Selectable FindSelectableOnDown() {
			if (!selectOnDown.useAutomatic && navigation.mode != Navigation.Mode.None) return null;

			return selectOnUp.selectable != null ? selectOnUp.selectable : base.FindSelectableOnDown();
		}

		public override Selectable FindSelectableOnLeft() {
			if (!selectOnLeft.useAutomatic && navigation.mode != Navigation.Mode.None) return null;

			return selectOnUp.selectable != null ? selectOnUp.selectable : base.FindSelectableOnLeft();
		}

		public override Selectable FindSelectableOnRight() {
			if (!selectOnRight.useAutomatic && navigation.mode != Navigation.Mode.None) return null;

			return selectOnUp.selectable != null ? selectOnUp.selectable : base.FindSelectableOnRight();
		}
	}
}
