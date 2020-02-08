using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems{
	public class Control{
		//name of control
		public string name;

		//is this control a hold or a toggle type
		public bool isToggle = false;

		//list of inputs we will check when the control is polled
		public List<DeviceInput> inputs;

		public List<CommonGamepadInputs> commonMappings = new List<CommonGamepadInputs>();

		//control constructor
		public Control(string controlName){
			name = controlName;
			inputs = new List<DeviceInput>();
		}

		private ControlState[] controlStates;
		//called no more than once a frame from Sinput.SinputUpdate
		bool checkslot = false;
		public void Update() {

			if (null == controlStates) {
				controlStates = new ControlState[Sinput.totalPossibleDeviceSlots];
				for (int i = 0; i < controlStates.Length; i++) {
					controlStates[i] = new ControlState();
				}
				ResetControlStates();
			}

			//do update here
			checkslot = false;
			//int connectedGamepads = Sinput.gamepads.Length;
			for (int i = 1; i < controlStates.Length; i++) {
				checkslot = false;
				if (i <= Sinput.connectedGamepads) checkslot = true; // slot represents a connected pad

				if (i >= 17) checkslot = true; //slot represents keyboard,mouse, or virtual slot

				if (checkslot) {
					//this is a (probably) connected device so lets update it
					UpdateControlState(i, (InputDeviceSlot)i);
				} else {
					//this device isn't connected & shouldn't be influencing this control
					//reset it
					controlStates[i].value = 0f;
					controlStates[i].axisAsButtonHeld = false;
					controlStates[i].held = false;
					controlStates[i].released = false;
					controlStates[i].pressed = false;
					controlStates[i].repeatPressed = false;
					controlStates[i].valuePrefersDeltaUse = true;
					controlStates[i].holdTime = 0f;
					controlStates[i].repeatTime = 0f;
					controlStates[i].toggleHeld = false;
					controlStates[i].togglePressed = false;
					controlStates[i].toggleReleased = false;
				}
				
			}
			UpdateAnyControlState();//checked other slots, now check the 'any' slot
			//UpdateControlState(0, InputDeviceSlot.any);//checked other slots, now check the 'any' slot
		}

		float v;
		bool wasHeld;
		void UpdateControlState(int i, InputDeviceSlot slot) {

			controlStates[i].value = 0f;
			controlStates[i].valuePrefersDeltaUse = true;
			controlStates[i].axisAsButtonHeld = false;
			
			for (int inpt = 0; inpt < inputs.Count; inpt++) {
				v = inputs[inpt].AxisCheck(slot);

				//update axis-as-button state
				if (inputs[inpt].inputType == InputDeviceType.GamepadAxis) {
					if (v > inputs[inpt].axisButtoncompareVal) controlStates[i].axisAsButtonHeld = true;
				}
				if (inputs[inpt].inputType == InputDeviceType.Mouse) {
					if (Mathf.Abs(v) > 0.5f) controlStates[i].axisAsButtonHeld = true;
				}

				if (Mathf.Abs(v) > controlStates[i].value) {
					//this is the value we're going with
					controlStates[i].value = v;
					//now find out if what set this value was something we shouldn't multiply by deltaTime
					controlStates[i].valuePrefersDeltaUse = true;
					if (inputs[inpt].inputType == InputDeviceType.Mouse) {
						if (inputs[inpt].mouseInputType == MouseInputType.MouseMoveLeft || inputs[inpt].mouseInputType == MouseInputType.MouseMoveRight ||
							inputs[inpt].mouseInputType == MouseInputType.MouseMoveUp || inputs[inpt].mouseInputType == MouseInputType.MouseMoveDown ||
							inputs[inpt].mouseInputType == MouseInputType.MouseHorizontal || inputs[inpt].mouseInputType == MouseInputType.MouseVertical ||
							inputs[inpt].mouseInputType == MouseInputType.MouseScrollUp || inputs[inpt].mouseInputType == MouseInputType.MouseScrollDown ||
							inputs[inpt].mouseInputType == MouseInputType.MouseScroll) {
							controlStates[i].valuePrefersDeltaUse = false;
						}
					}
				}
			}
			//controlStates[i].value = AxisCheck(slot, out controlStates[i].valuePrefersDeltaUse, out controlStates[i].axisAsButtonHeld);

			//check if this control is held
			wasHeld = controlStates[i].held;
			controlStates[i].held = false;
			for (int inpt = 0; inpt < inputs.Count; inpt++) {
				if (inputs[inpt].ButtonHeldCheck(slot)) controlStates[i].held = true;
			}
			//controlStates[i].pressed = ButtonCheck(ButtonAction.DOWN, slot);
			//controlStates[i].released = ButtonCheck(ButtonAction.UP, slot);

			//held state
			if (wasHeld) {
				controlStates[i].pressed = false;
				if (controlStates[i].axisAsButtonHeld || controlStates[i].held) {
					controlStates[i].held = true;
					controlStates[i].released = false;
				} else {
					controlStates[i].held = false;
					controlStates[i].released = true;
				}
			} else {
				controlStates[i].released = false;
				if (controlStates[i].axisAsButtonHeld || controlStates[i].held) {
					controlStates[i].held = true;
					controlStates[i].pressed = true;

				} else {
					controlStates[i].held = false;
					controlStates[i].pressed = false;
				}
			}

			//toggled state
			controlStates[i].toggleReleased = false;
			controlStates[i].togglePressed = false;
			if (controlStates[i].pressed) {
				if (controlStates[i].toggleHeld) {
					controlStates[i].toggleHeld = false;
					controlStates[i].toggleReleased = true;
				} else {
					controlStates[i].toggleHeld = true;
					controlStates[i].togglePressed = true;
				}
			}

			
			//repeating press state
			controlStates[i].repeatPressed = false;
			if (controlStates[i].pressed) controlStates[i].repeatPressed = true;//repeat press returns true on first frame down
			if (controlStates[i].held) {
				controlStates[i].holdTime += Time.deltaTime;
				controlStates[i].repeatTime -= Time.deltaTime;
				if (controlStates[i].holdTime>Sinput.buttonRepeatWait && controlStates[i].repeatTime < 0f) {
					controlStates[i].repeatTime = Sinput.buttonRepeat;
					controlStates[i].repeatPressed = true;
				}
			} else {
				controlStates[i].holdTime = 0f;
				controlStates[i].repeatTime = 0f;
			}
		}

		void UpdateAnyControlState() {

			controlStates[0].value = 0f;
			controlStates[0].axisAsButtonHeld = false;

			for (int i = 1; i < controlStates.Length; i++) {
				v = controlStates[i].value;

				if (Mathf.Abs(v) > controlStates[0].value) {
					//this is the value we're going with
					controlStates[0].value = v;
					//now find out if what set this value was something we shouldn't multiply by deltaTime
					controlStates[0].valuePrefersDeltaUse = controlStates[i].valuePrefersDeltaUse;
				}
			}

			//check if this control is held
			wasHeld = controlStates[0].held;
			controlStates[0].held = false;
			for (int i = 1; i < controlStates.Length; i++) {
				if (controlStates[i].held) controlStates[0].held = true;
			}

			//held state
			if (wasHeld) {
				controlStates[0].pressed = false;
				if (controlStates[0].held) {
					controlStates[0].released = false;
				} else {
					controlStates[0].released = true;
				}
			} else {
				controlStates[0].released = false;
				if (controlStates[0].held) {
					controlStates[0].pressed = true;
				} else {
					controlStates[0].pressed = false;
				}
			}

			//toggled state
			controlStates[0].toggleReleased = false;
			controlStates[0].togglePressed = false;
			if (controlStates[0].pressed) {
				if (controlStates[0].toggleHeld) {
					controlStates[0].toggleHeld = false;
					controlStates[0].toggleReleased = true;
				} else {
					controlStates[0].toggleHeld = true;
					controlStates[0].togglePressed = true;
				}
			}


			//repeating press state
			controlStates[0].repeatPressed = false;
			if (controlStates[0].pressed) controlStates[0].repeatPressed = true;//repeat press returns true on first frame down
			if (controlStates[0].held) {
				controlStates[0].holdTime += Time.deltaTime;
				controlStates[0].repeatTime -= Time.deltaTime;
				if (controlStates[0].holdTime > Sinput.buttonRepeatWait && controlStates[0].repeatTime < 0f) {
					controlStates[0].repeatTime = Sinput.buttonRepeat;
					controlStates[0].repeatPressed = true;
				}
			} else {
				controlStates[0].holdTime = 0f;
				controlStates[0].repeatTime = 0f;
			}
		}

		public void ResetControlStates() {
			//set all values for this control to 0
			for (int i = 0; i < controlStates.Length; i++) {
				controlStates[i].value = 0f;
				controlStates[i].axisAsButtonHeld = false;
				controlStates[i].held = false;
				controlStates[i].released = false;
				controlStates[i].pressed = false;
				controlStates[i].repeatPressed = false;
				controlStates[i].valuePrefersDeltaUse = true;
				controlStates[i].holdTime = 0f;
				controlStates[i].repeatTime = 0f;
				controlStates[i].toggleHeld = false;
				controlStates[i].togglePressed = false;
				controlStates[i].toggleReleased = false;
			}
		}

		//button checks
		public bool GetButtonState(ButtonAction bAction, InputDeviceSlot slot, bool getRaw) {

			if (!getRaw && isToggle) {
				if (bAction == ButtonAction.HELD) return controlStates[(int)slot].toggleHeld;
				if (bAction == ButtonAction.DOWN) return controlStates[(int)slot].togglePressed;
				if (bAction == ButtonAction.UP) return controlStates[(int)slot].toggleReleased;
			} else { 
				if (bAction == ButtonAction.HELD) return controlStates[(int)slot].held;
				if (bAction == ButtonAction.DOWN) {
					if (null==controlStates) Debug.Log("yup");
					return controlStates[(int)slot].pressed;

				}
				if (bAction == ButtonAction.UP) return controlStates[(int)slot].released;
			}
			if (bAction == ButtonAction.REPEATING) return controlStates[(int)slot].repeatPressed;

			return false;
		}
	

		//axis checks
		public float GetAxisState(InputDeviceSlot slot) {
			return controlStates[(int)slot].value;
		}
		public bool GetAxisStateDeltaPreference(InputDeviceSlot slot) {
			return controlStates[(int)slot].valuePrefersDeltaUse;
		}


		public void AddKeyboardInput(KeyCode keyCode){
			DeviceInput input = new DeviceInput(InputDeviceType.Keyboard);
			input.keyboardKeyCode = keyCode;
			input.commonMappingType = CommonGamepadInputs.NOBUTTON;//don't remove this input when gamepads are unplugged/replugged
			inputs.Add(input);
		}

		public void AddGamepadInput(CommonGamepadInputs gamepadButtonOrAxis){ AddGamepadInput(gamepadButtonOrAxis, true); }
		private void AddGamepadInput(CommonGamepadInputs gamepadButtonOrAxis, bool isNewBinding){
			Sinput.CheckGamepads();

			if (isNewBinding) commonMappings.Add(gamepadButtonOrAxis);
			List<DeviceInput> applicableMapInputs = CommonGamepadMappings.GetApplicableMaps( gamepadButtonOrAxis, Sinput.gamepads );

			string[] gamepads = Sinput.gamepads;

			//find which common mapped inputs apply here, but already have custom binding loaded, and disregard those common mappings
			for (int ai=0; ai<applicableMapInputs.Count; ai++){
				bool samePad = false;
				for (int i=0; i<inputs.Count; i++){
					if (inputs[i].inputType == InputDeviceType.GamepadAxis || inputs[i].inputType == InputDeviceType.GamepadButton){
						if (inputs[i].isCustom){
							for (int ais=0; ais<applicableMapInputs[ai].allowedSlots.Length; ais++){
								for (int toomanyints=0; toomanyints<inputs[i].allowedSlots.Length; toomanyints++){
									if (applicableMapInputs[ai].allowedSlots[ais] == inputs[i].allowedSlots[toomanyints]) samePad = true;
								}
								if (gamepads[applicableMapInputs[ai].allowedSlots[ais]] == inputs[i].deviceName.ToUpper()) samePad = true;
							}
							if (samePad){
								//if I wanna be copying input display names, here's the place to do it
								//TODO: decide if I wanna do this
								//pro: it's good if the common mapping is accurate but the user wants to rebind
								//con: it's bad if the common mapping is bad or has a generic gamepad name and so it mislables different inputs
								//maybe I should do this, but with an additional check so it's not gonna happen with say, a device labelled "wireless controller"?
							}
						}
					}
				}
				if (samePad){
					//we already have a custom bound control for this input, we don't need more
					applicableMapInputs.RemoveAt(ai);
					ai--;
				}
			}

			//add whichever common mappings still apply
			for (int i=0; i<applicableMapInputs.Count; i++){
				inputs.Add(applicableMapInputs[i]);
			}
		}

		public void AddMouseInput(MouseInputType mouseInputType){
			DeviceInput input = new DeviceInput(InputDeviceType.Mouse);
			input.mouseInputType = mouseInputType;
			input.commonMappingType = CommonGamepadInputs.NOBUTTON;
			inputs.Add(input);
		}

		public void AddVirtualInput(string virtualInputID){
			DeviceInput input = new DeviceInput(InputDeviceType.Virtual);
			input.virtualInputID = virtualInputID;
			input.commonMappingType = CommonGamepadInputs.NOBUTTON;
			inputs.Add(input);
			VirtualInputs.AddInput(virtualInputID);
		}

		public void ReapplyCommonBindings(){
			//connected gamepads have changed, so we want to remove all old common bindings, and replace them now new mapping information has been loaded
			for (int i=0; i<inputs.Count; i++){
				if (inputs[i].commonMappingType != CommonGamepadInputs.NOBUTTON){
					inputs.RemoveAt(i);
					i--;
				}
			}



			for (int i=0; i<commonMappings.Count; i++){
				AddGamepadInput(commonMappings[i], false);
			}

			//also recheck allowed slots for custom bound pads (their inputs have a device name, common bound stuff don't)
			//need to do this anyway so we can check if common & custom bindings are about to match on the same slot
			string[] gamepads= Sinput.gamepads;
			for (int i=0; i<inputs.Count; i++){
				if (inputs[i].deviceName!=""){
					List<int> allowedSlots = new List<int>();
					for (int g=0; g<gamepads.Length; g++){
						if (gamepads[g] == inputs[i].deviceName.ToUpper()) allowedSlots.Add(i);
					}
					inputs[i].allowedSlots = allowedSlots.ToArray();
				}
			}
		}

		public void SetAllowedInputSlots() {
			//custom gamepad inputs need to know which gamepad slots they can look at to match the gamepad they are for
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].isCustom) {
					if (inputs[i].inputType == InputDeviceType.GamepadAxis || inputs[i].inputType == InputDeviceType.GamepadButton) {
						//Debug.Log("Finding slot for gamepad: " + controls[c].inputs[i].displayName + " of " + controls[c].inputs[i].deviceName);
						//find applicable gamepad slots for this device
						List<int> allowedSlots = new List<int>();
						for (int g = 0; g < Sinput.connectedGamepads; g++) {
							if (Sinput.gamepads[g] == inputs[i].deviceName.ToUpper()) {
								allowedSlots.Add(g);
							}
						}
						inputs[i].allowedSlots = allowedSlots.ToArray();
					}
				}
			}
		}


	}

	//state of control, for a frame, for one slot
	class ControlState {
		//basic cacheing of all relevant inputs for this slot
		public float value = 0f;
		public bool axisAsButtonHeld = false;
		public bool held = false;
		public bool released = false;
		public bool pressed = false;

		//for toggle checks
		public bool toggleHeld = false;
		public bool toggleReleased = false;
		public bool togglePressed = false;

		//for checking if the value is something that should be multiplied by deltaTime or not
		public bool valuePrefersDeltaUse = true;

		//for Sinput.ButtonPressRepeat() checks
		public bool repeatPressed = false;
		public float repeatTime = 0f;
		public float holdTime = 0f;
	}
}
