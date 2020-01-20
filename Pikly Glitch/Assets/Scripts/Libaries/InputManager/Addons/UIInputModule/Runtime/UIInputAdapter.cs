using UnityEngine;
using UnityEngine.EventSystems;

namespace Luminosity.IO
{
	public class UIInputAdapter : BaseInput
	{
		public bool IsMouseEnabled { get; set; }

		protected override void Awake()
		{
			base.Awake();
			IsMouseEnabled = true;
		}

		public override string compositionString
		{
			get { return InputMgr.compositionString; }
		}

		public override IMECompositionMode imeCompositionMode
		{
			get { return InputMgr.imeCompositionMode; }
			set { InputMgr.imeCompositionMode = value; }
		}

		public override Vector2 compositionCursorPos
		{
			get { return InputMgr.compositionCursorPos; }
			set { InputMgr.compositionCursorPos = value; }
		}

		public override bool mousePresent
		{
			get { return InputMgr.mousePresent; }
		}

		public override bool GetMouseButtonDown(int button)
		{
			return IsMouseEnabled && InputMgr.GetMouseButtonDown(button);
		}

		public override bool GetMouseButtonUp(int button)
		{
			return IsMouseEnabled && InputMgr.GetMouseButtonUp(button);
		}

		public override bool GetMouseButton(int button)
		{
			return IsMouseEnabled && InputMgr.GetMouseButton(button);
		}

		public override Vector2 mousePosition
		{
			get { return IsMouseEnabled ? InputMgr.mousePosition : -Vector2.one; }
		}

		public override Vector2 mouseScrollDelta
		{
			get { return IsMouseEnabled ? InputMgr.mouseScrollDelta : Vector2.zero; }
		}

		public override bool touchSupported
		{
			get { return InputMgr.touchSupported; }
		}

		public override int touchCount
		{
			get { return InputMgr.touchCount; }
		}

		public override Touch GetTouch(int index)
		{
			return InputMgr.GetTouch(index);
		}

		public override float GetAxisRaw(string axisName)
		{
			return InputMgr.GetAxisRaw(axisName);
		}

		public override bool GetButtonDown(string buttonName)
		{
			return InputMgr.GetButtonDown(buttonName);
		}
	}
}
