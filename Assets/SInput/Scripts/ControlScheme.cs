using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems{
	[CreateAssetMenu(fileName = "New Control Scheme", menuName = "Sinput/Control Scheme", order = 0)]
	public class ControlScheme : ScriptableObject {
		
		public List<ControlSetup> controls = new List<ControlSetup>();
		public List<SmartControlSetup> smartControls = new List<SmartControlSetup>();

		//settings
		//public string name = "";
		//public bool exposeMouseSensitivityOption = true;
		//public bool mouseAndKeyboardAreDistinct = false;
		//public List<string> 
		
		public ControlScheme() {
			//add all the default controls
			ControlSetup newControl = NewControlSetup("Left");
			newControl.keyboardInputs.Add(KeyboardInputType.A);
			newControl.keyboardInputs.Add(KeyboardInputType.LeftArrow);
			newControl.gamepadInputs.Add(CommonGamepadInputs.LSTICK_LEFT);
			newControl.gamepadInputs.Add(CommonGamepadInputs.DPAD_LEFT);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Right");
			newControl.keyboardInputs.Add(KeyboardInputType.D);
			newControl.keyboardInputs.Add(KeyboardInputType.RightArrow);
			newControl.gamepadInputs.Add(CommonGamepadInputs.LSTICK_RIGHT);
			newControl.gamepadInputs.Add(CommonGamepadInputs.DPAD_RIGHT);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Up");
			newControl.keyboardInputs.Add(KeyboardInputType.W);
			newControl.keyboardInputs.Add(KeyboardInputType.UpArrow);
			newControl.gamepadInputs.Add(CommonGamepadInputs.LSTICK_UP);
			newControl.gamepadInputs.Add(CommonGamepadInputs.DPAD_UP);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Down");
			newControl.keyboardInputs.Add(KeyboardInputType.S);
			newControl.keyboardInputs.Add(KeyboardInputType.DownArrow);
			newControl.gamepadInputs.Add(CommonGamepadInputs.LSTICK_DOWN);
			newControl.gamepadInputs.Add(CommonGamepadInputs.DPAD_DOWN);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Jump");
			newControl.keyboardInputs.Add(KeyboardInputType.Space);
			newControl.gamepadInputs.Add(CommonGamepadInputs.A);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Fire1");
			newControl.keyboardInputs.Add(KeyboardInputType.LeftControl);
			newControl.gamepadInputs.Add(CommonGamepadInputs.RT);
			newControl.mouseInputs.Add(MouseInputType.Mouse0);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Fire2");
			newControl.keyboardInputs.Add(KeyboardInputType.LeftAlt);
			newControl.gamepadInputs.Add(CommonGamepadInputs.RB);
			newControl.mouseInputs.Add(MouseInputType.Mouse1);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Fire3");
			newControl.keyboardInputs.Add(KeyboardInputType.LeftShift);
			newControl.gamepadInputs.Add(CommonGamepadInputs.LB);
			newControl.mouseInputs.Add(MouseInputType.Mouse2);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Look Left");
			newControl.gamepadInputs.Add(CommonGamepadInputs.RSTICK_LEFT);
			newControl.mouseInputs.Add(MouseInputType.MouseMoveLeft);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Look Right");
			newControl.gamepadInputs.Add(CommonGamepadInputs.RSTICK_RIGHT);
			newControl.mouseInputs.Add(MouseInputType.MouseMoveRight);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Look Up");
			newControl.gamepadInputs.Add(CommonGamepadInputs.RSTICK_UP);
			newControl.mouseInputs.Add(MouseInputType.MouseMoveUp);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Look Down");
			newControl.gamepadInputs.Add(CommonGamepadInputs.RSTICK_DOWN);
			newControl.mouseInputs.Add(MouseInputType.MouseMoveDown);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Submit");
			newControl.keyboardInputs.Add(KeyboardInputType.Space);
			newControl.gamepadInputs.Add(CommonGamepadInputs.A);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Cancel");
			newControl.keyboardInputs.Add(KeyboardInputType.Escape);
			newControl.gamepadInputs.Add(CommonGamepadInputs.B);
			this.controls.Add(newControl);

			newControl = NewControlSetup("Join");
			newControl.keyboardInputs.Add(KeyboardInputType.Return);
			newControl.gamepadInputs.Add(CommonGamepadInputs.START);
			this.controls.Add(newControl);

			//now we create the default smart controls
			SmartControlSetup newSmartControl = NewSmartControlSetup("Horizontal", "Left", "Right");
			this.smartControls.Add(newSmartControl);

			newSmartControl = NewSmartControlSetup("Vertical", "Down", "Up");
			this.smartControls.Add(newSmartControl);

			newSmartControl = NewSmartControlSetup("Look Horizontal", "Look Left", "Look Right");
			this.smartControls.Add(newSmartControl);

			newSmartControl = NewSmartControlSetup("Look Vertical", "Look Down", "Look Up");
			this.smartControls.Add(newSmartControl);
			
		}
	


		[System.Serializable]
		public struct ControlSetup{
			public string name;// = "New Control";
			public List<KeyboardInputType> keyboardInputs;// = new List<InputSetup>();
			public List<CommonGamepadInputs> gamepadInputs;
			public List<MouseInputType> mouseInputs;
			public List<string> virtualInputs;
		}
		[System.Serializable]
		public struct SmartControlSetup{
			public string name;// = "New Mix Control";
			public string positiveControl;
			public string negativeControl;

			public float deadzone;//=0.001f;

			public float gravity;//=3;
			public float speed;//=3;
			public bool snap;//=true;

			public float scale;// =1f;
			public bool invert;//default inversion setting
		}

		private ControlSetup NewControlSetup(string name) {
			ControlSetup newControl = new ControlSetup {
				name = name,
				keyboardInputs = new List<KeyboardInputType>(),
				gamepadInputs = new List<CommonGamepadInputs>(),
				mouseInputs = new List<MouseInputType>(),
				virtualInputs = new List<string>()
			};
			return newControl;
		}
		private SmartControlSetup NewSmartControlSetup(string name, string negativeControl, string positiveControl) {
			SmartControlSetup newSmartControl = new SmartControlSetup {
				name = name,
				positiveControl = positiveControl,
				negativeControl = negativeControl,
				deadzone = 0.001f,
				gravity = 3f,
				speed = 3f,
				snap = false,
				scale = 1f,
				invert = false
			};
			return newSmartControl;
		}

	}

}