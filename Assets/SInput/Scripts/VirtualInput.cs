using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems {

	public static class VirtualInputs {
		private static List<VirtualInput> inputs = new List<VirtualInput>();

		public static void AddInput(string virtualInputName) {
			for (int i=0; i<inputs.Count; i++) {
				if (inputs[i].name == virtualInputName) return;
			}
			inputs.Add(new VirtualInput(virtualInputName));
		}

		public static float GetVirtualAxis(string virtualInputName) {
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].name == virtualInputName) return inputs[i].axisValue;
			}
			Debug.Log("Virtual input \"" + virtualInputName + "\" not found.");
			return 0f;
		}

		public static bool GetVirtualButton(string virtualInputName) {
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].name == virtualInputName) return inputs[i].held;
			}
			Debug.Log("Virtual input \"" + virtualInputName + "\" not found.");
			return false;
		}

		public static bool GetDeltaPreference(string virtualInputName) {
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].name == virtualInputName) return inputs[i].preferDeltaUse;
			}
			Debug.Log("Virtual input \"" + virtualInputName + "\" not found.");
			return false;
		}

		public static void SetVirtualAxis(string virtualInputName, float newAxisValue) {
			Sinput.SinputUpdate(); //make sure sinput is set up, so any bound virtual inputs have been instantiated
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].name == virtualInputName) {
					inputs[i].SetAxisValue(newAxisValue);
					return;
				}
			}
			Debug.Log("Virtual input \"" + virtualInputName + "\" not found.");
		}

		public static void SetVirtualButton(string virtualInputName, bool held) {
			Sinput.SinputUpdate(); //make sure sinput is set up, so any bound virtual inputs have been instantiated
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].name == virtualInputName) {
					inputs[i].UpdateButtonState(held);
					return;
				}
			}
			Debug.Log("Virtual input \"" + virtualInputName + "\" not found.");
		}

		public static void SetDeltaPreference(string virtualInputName, bool preferFrameDelta) {
			Sinput.SinputUpdate(); //make sure sinput is set up, so any bound virtual inputs have been instantiated
			for (int i = 0; i < inputs.Count; i++) {
				if (inputs[i].name == virtualInputName) {
					inputs[i].preferDeltaUse = preferFrameDelta;
					return;
				}
			}
			Debug.Log("Virtual input \"" + virtualInputName + "\" not found.");
		}
	}

	class VirtualInput {
		public string name;

		public bool preferDeltaUse = true;
		public float axisValue = 0f;
		public bool held = false;
		//public ButtonAction buttonState = ButtonAction.NOTHING;

		public VirtualInput(string virtualInputName) {
			name = virtualInputName;
		}

		public void UpdateButtonState(bool heldState) {
			axisValue = 0f;
			if (heldState) axisValue = 1f;

			held = heldState;

		}

		public void SetAxisValue(float newValue) {
			if (newValue > 0.4f) {
				held = true;
			} else {
				held = false;
			}

			axisValue = newValue;
		}

	}
}