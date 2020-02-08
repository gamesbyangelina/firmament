using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems{

	public class SmartControl{
		//InputControls combine various inputs, and can behave as buttons or 1-dimensional axis
		//SmartControls combine various InputControls or other SmartControls, and can have a bunch of extra behaviour like normal InputManager smoothing
		//These won't be exposed to players when rebinding because they are built on other controls (and it'd be a headache to present anyway)

		public string name;
		public string displayName;

		//control constructor
		public SmartControl(string controlName){
			name = controlName;
		}

		//values for each slot's input
		private float[] rawValues;
		private float[] controlValues;
		private bool[] valuePrefersDeltaUse;

		public string positiveControl;
		public string negativeControl;


		public float deadzone=0.001f; //clip values less than this

		public float gravity=3; //how quickly the value shifts to zero
		public float speed=3; //how quickly does the value shift towards it's target
		public bool snap=false;//if value is negative and input is positive, snaps to zero

		//public float scale =1f;
		public float[] scales;

		public bool[] inversion;

		public void Init(){
			//prepare to record values for all gamepads AND keyboard & mouse inputs
			//int possibleInputDeviceCount = System.Enum.GetValues(typeof(InputDeviceSlot)).Length;
			rawValues = new float[Sinput.totalPossibleDeviceSlots];
			controlValues = new float[Sinput.totalPossibleDeviceSlots];
			valuePrefersDeltaUse = new bool[Sinput.totalPossibleDeviceSlots];
			ResetAllValues(InputDeviceSlot.any);

			
		}

		public void ResetAllValues(InputDeviceSlot slot) {
			//set all values for this control to 0
			if (slot == InputDeviceSlot.any) {
				for (int i = 0; i < controlValues.Length; i++) {
					rawValues[i] = 0f;
					controlValues[i] = 0f;
					valuePrefersDeltaUse[i] = true;
				}
			} else {
				rawValues[(int)slot] = 0f;
				controlValues[(int)slot] = 0f;
				valuePrefersDeltaUse[(int)slot] = true;
			}
		}

		private int lastUpdateFrame = -10;
		public void Update(){
			if (Time.frameCount == lastUpdateFrame) return;
			lastUpdateFrame = Time.frameCount;

			
			for (int slot=0; slot<rawValues.Length; slot++){
				
				rawValues[slot] = Sinput.GetAxis(positiveControl, (InputDeviceSlot)slot) - Sinput.GetAxis(negativeControl, (InputDeviceSlot)slot);

				if (inversion[slot]) rawValues[slot] *= -1f;

				valuePrefersDeltaUse[slot] = true;
				if (!Sinput.PrefersDeltaUse(positiveControl, (InputDeviceSlot)slot) || !Sinput.PrefersDeltaUse(negativeControl, (InputDeviceSlot)slot)) {
					//the rawvalue this frame is from a framerate independent input like a mouse movement, so we don't want it smoothed and wanna force getAxis checks to return raw
					valuePrefersDeltaUse[slot] = false;
				}
			}

			for (int slot=0; slot<controlValues.Length; slot++){
				if (!valuePrefersDeltaUse[slot]) {
					//we're forcing things to be unsmoothed for now, zero smoothed input now so when we stop smoothing, it doesn't seem weird
					controlValues[slot] = 0f;
				} else { 
					//shift to zero
					if (gravity > 0f) {
						if (rawValues[slot] == 0f || (rawValues[slot] < controlValues[slot] && controlValues[slot] > 0f) || (rawValues[slot] > controlValues[slot] && controlValues[slot] < 0f)) {
							if (controlValues[slot] > 0f) {
								controlValues[slot] -= gravity * Time.deltaTime;
								if (controlValues[slot] < 0f) controlValues[slot] = 0f;
								if (controlValues[slot] < rawValues[slot]) controlValues[slot] = rawValues[slot];
							} else if (controlValues[slot] < 0f) {
								controlValues[slot] += gravity * Time.deltaTime;
								if (controlValues[slot] > 0f) controlValues[slot] = 0f;
								if (controlValues[slot] > rawValues[slot]) controlValues[slot] = rawValues[slot];
							}
						}
					}

					//snapping
					if (snap) {
						if (rawValues[slot] > 0f && controlValues[slot] < 0f) controlValues[slot] = 0f;
						if (rawValues[slot] < 0f && controlValues[slot] > 0f) controlValues[slot] = 0f;
					}

					//move value towards target value
					if (rawValues[slot] < 0f) {
						if (controlValues[slot] > rawValues[slot]) {
							controlValues[slot] -= speed * Time.deltaTime;
							if (controlValues[slot] < rawValues[slot]) controlValues[slot] = rawValues[slot];
						}
					}
					if (rawValues[slot] > 0f) {
						if (controlValues[slot] < rawValues[slot]) {
							controlValues[slot] += speed * Time.deltaTime;
							if (controlValues[slot] > rawValues[slot]) controlValues[slot] = rawValues[slot];
						}
					}

					if (speed == 0f) controlValues[slot] = rawValues[slot];
				}
			}

		}

		//return current value
		public float GetValue() { return GetValue(InputDeviceSlot.any, false); }
		public float GetValue(InputDeviceSlot slot) { return GetValue(slot, false); }
		public float GetValue(InputDeviceSlot slot, bool getRawValue){
			if ((int)slot>=controlValues.Length) return 0f; //not a slot we have any input info for

			//if this input is checking a framerate independent input like a mouse, return the raw value regardless of getRawValue
			if (!valuePrefersDeltaUse[(int)slot]) return rawValues[(int)slot] * scales[(int)slot];

			//deadzone clipping
			if (Mathf.Abs(controlValues[(int)slot]) < deadzone) return 0f;

			if (getRawValue) {
				//return the raw value
				return rawValues[(int)slot] * scales[(int)slot];
			}

			//return the smoothed value
			return controlValues[(int)slot]*scales[(int)slot];
		}

		//button check
		public bool ButtonCheck(ButtonAction bAction){ return ButtonCheck(bAction, InputDeviceSlot.any); }
		public bool ButtonCheck(ButtonAction bAction, InputDeviceSlot slot){
			if (bAction == ButtonAction.DOWN && Sinput.GetButtonDown(positiveControl, slot)) return true;
			if (bAction == ButtonAction.DOWN && Sinput.GetButtonDown(negativeControl, slot)) return true;
			if (bAction == ButtonAction.HELD && Sinput.GetButton(positiveControl, slot)) return true;
			if (bAction == ButtonAction.HELD && Sinput.GetButton(negativeControl, slot)) return true;
			if (bAction == ButtonAction.UP && Sinput.GetButtonUp(positiveControl, slot)) return true;
			if (bAction == ButtonAction.UP && Sinput.GetButtonUp(negativeControl, slot)) return true;
			return false;
		}

	}

}
