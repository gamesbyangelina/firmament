using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//class that watches all Sinput compatible input devices and looks for any specific changes

namespace SinputSystems.Rebinding{

	public class SinputMonitor : MonoBehaviour {

		public bool changeFound = false;

		[HideInInspector]
		public KeyCode changedKey = KeyCode.None;
		[HideInInspector]
		public gamepadStateChange changedPadAxis = new gamepadStateChange();
		[HideInInspector]
		public gamepadStateChange changedPadButton = new gamepadStateChange();
		[HideInInspector]
		public MouseInputType changedMouse = MouseInputType.None;


		// Use this for initialization
		void Start () {
			
		}

		int mouseMovementMin = 50;
		public void ResetMouseListening() {
			mouseHorizontalTotal = 0f;
			mouseVerticalTotal = 0f;
			mouseScrollTotal = 0f;
		}

		// Update is called once per frame
		void Update() {
			changeFound = false;
			changedKey = KeyCode.None;

			changedPadAxis = new gamepadStateChange();
			changedPadAxis.padIndex = -1;
			changedPadAxis.inputIndex = -1;
			changedPadAxis.axisMotionIsPositive = true;
			changedPadAxis.restingValue = 0f;
			changedPadAxis.minRecordedValue = 0f;
			changedPadAxis.maxRecordedValue = 0f;

			changedPadButton = new gamepadStateChange();
			changedPadButton.padIndex = -1;
			changedPadButton.inputIndex = -1;

			changedMouse = MouseInputType.None;





			//keyboard check
			foreach (KeyboardInputType keycode in Enum.GetValues(typeof(KeyboardInputType))) {
				KeyCheck((KeyCode)Enum.Parse(typeof(KeyCode), keycode.ToString()));
			}

			//gamepad checks
			UpdateAxisMonitoring();

			//mouse checks
			if (AcceptChangesFromSlot(-1)) { 
				mouseHorizontalTotal += Input.GetAxis("Mouse Horizontal");
				mouseVerticalTotal += Input.GetAxis("Mouse Vertical");
				mouseScrollTotal += Input.GetAxis("Mouse Scroll");
				if (mouseHorizontalTotal > mouseMovementMin) changedMouse = MouseInputType.MouseMoveRight;
				if (mouseHorizontalTotal < -mouseMovementMin) changedMouse = MouseInputType.MouseMoveLeft;
				if (mouseVerticalTotal > mouseMovementMin) changedMouse = MouseInputType.MouseMoveUp;
				if (mouseVerticalTotal < -mouseMovementMin) changedMouse = MouseInputType.MouseMoveDown;
				if (mouseScrollTotal >= 1) changedMouse = MouseInputType.MouseScrollUp;
				if (mouseScrollTotal <= -1) changedMouse = MouseInputType.MouseScrollDown;
				if (changedMouse != MouseInputType.None) changeFound = true;
			}

			foreach (MouseInputType mouseInput in Enum.GetValues(typeof(MouseInputType))){
				MouseCheck( mouseInput );
			}

			/*if (changeFound){
				if (changedKey != KeyCode.None){
					//change was keyboard input
					Debug.Log(changedKey.ToString());
				}
				if (changedPadAxis.padIndex >=0){
					//change was a gamepad axis
					Debug.Log("Found change in gamepad " + (changedPadAxis.padIndex+1).ToString() + " axis " + (changedPadAxis.inputIndex+1).ToString());
				}
				if (changedPadButton.padIndex >=0){
					//change was a gamepad button
					Debug.Log("Found change in gamepad " + (changedPadButton.padIndex+1).ToString() + " button " + changedPadButton.inputIndex.ToString());
				}
				if (changedMouse != MouseInputType.None){
					//change was mouse click
					Debug.Log(changedMouse.ToString());
				}
			}*/
		}

		private float mouseHorizontalTotal = 0f;
		private float mouseVerticalTotal = 0f;
		private float mouseScrollTotal = 0f;

		private gamepadState[] allGamepadAxis;
		public struct gamepadState{
			public float[] axisValues;
			public bool[] buttonValues;

			//we dont record some stuff till after first axis change is measured (some pads LIE until a user moves the axis from it's resting value)
			public bool[] measuredFirstChange; 

			//for determining the resting value, count how long the axis is closest to -1, 0, or 1
			public float[] zeroTime;
			public float[] plusOneTime;
			public float[] minusOneTime;

			public float[] minRecordedValue;
			public float[] maxRecordedValue;
		}
		public struct gamepadStateChange{
			public int padIndex;
			public int inputIndex;

			//axis specifics
			public bool axisMotionIsPositive;//was the change making the axis go up or down in value
			public float restingValue; //what value when the user is not doing anything
			public float minRecordedValue; //how low have we seen this axis go
			public float maxRecordedValue; //how high have we seen this axis go

		}
		void UpdateAxisMonitoring(){
			if (null == allGamepadAxis){
				BuildAxisArray();
			}
			int padCount = Input.GetJoystickNames().Length;
			if (padCount != allGamepadAxis.Length) BuildAxisArray();

			for (int i=0; i<padCount; i++){
				for (int a=0; a<allGamepadAxis[i].axisValues.Length; a++){
					float presentValue = Input.GetAxisRaw("J_"+(i+1).ToString()+"_"+(a+1).ToString());
					if (AcceptChangesFromSlot(i+1) && allGamepadAxis[i].measuredFirstChange[a] && allGamepadAxis[i].axisValues[a] != presentValue){
						float restingValue = 0f;
						if (allGamepadAxis[i].zeroTime[a] >= allGamepadAxis[i].minusOneTime[a] && allGamepadAxis[i].zeroTime[a] >= allGamepadAxis[i].plusOneTime[a]){
							restingValue = 0f;
						}else if (allGamepadAxis[i].minusOneTime[a] > allGamepadAxis[i].plusOneTime[a]){
							restingValue = -1f;
						}else{
							restingValue = 1f;
						}
						if (presentValue > restingValue + 0.3f || presentValue < restingValue - 0.3f){
							//axis value has changed enough to register as an actual change and for us to have recorded enough properties to get a picture of how it works
							changeFound = true;
							changedPadAxis.padIndex = i;
							changedPadAxis.inputIndex = a+1;

							changedPadAxis.axisMotionIsPositive = presentValue > allGamepadAxis[i].axisValues[a];
							changedPadAxis.restingValue = restingValue;
							changedPadAxis.minRecordedValue=allGamepadAxis[i].minRecordedValue[a];
							changedPadAxis.maxRecordedValue=allGamepadAxis[i].maxRecordedValue[a];
						}
					}

					if (allGamepadAxis[i].measuredFirstChange[a]){
						if (presentValue > 0.4f){
							allGamepadAxis[i].plusOneTime[a] += Time.deltaTime;
						}else if (presentValue < -0.4f){
							allGamepadAxis[i].minusOneTime[a] += Time.deltaTime;
						}else{
							allGamepadAxis[i].zeroTime[a] += Time.deltaTime;
						}
					}

					allGamepadAxis[i].minRecordedValue[a] = Mathf.Min(allGamepadAxis[i].minRecordedValue[a], presentValue); 
					allGamepadAxis[i].maxRecordedValue[a] = Mathf.Max(allGamepadAxis[i].maxRecordedValue[a], presentValue);

					if (allGamepadAxis[i].axisValues[a] != presentValue){
						allGamepadAxis[i].measuredFirstChange[a] = true;
					}

					allGamepadAxis[i].axisValues[a] = presentValue;
				}
				for (int b=0; b<allGamepadAxis[i].buttonValues.Length; b++){
					KeyCode buttonCode = (KeyCode)Enum.Parse(typeof(KeyCode), string.Format("Joystick{0}Button{1}", (i+1), b));
					bool presentValue = Input.GetKeyDown(buttonCode);
					if (AcceptChangesFromSlot(i+1) && allGamepadAxis[i].buttonValues[b] != presentValue){
						changeFound = true;
						changedPadButton.padIndex = i;
						changedPadButton.inputIndex = b;
					}
					allGamepadAxis[i].buttonValues[b] = presentValue;
				}
			}
		}
		string listeningDevice = "";
		bool AcceptChangesFromSlot(int s){
			if (s==-1){
				return listeningDevice=="KeyboardMouse";
			}else{
				if (Input.GetJoystickNames()[s-1].ToUpper() == listeningDevice.ToUpper()) return true;
				//return true;
			}
			return false;
		}
		public void SetListeningDevice(string deviceName){
			listeningDevice = deviceName;
			ResetMouseListening();
		}
		void BuildAxisArray(){
			int padCount = Input.GetJoystickNames().Length;
			allGamepadAxis = new gamepadState[padCount];
			for (int i=0; i<padCount; i++){
				allGamepadAxis[i].axisValues = new float[Sinput.MAXAXISPERGAMEPAD];

				allGamepadAxis[i].zeroTime = new float[Sinput.MAXAXISPERGAMEPAD];
				allGamepadAxis[i].plusOneTime = new float[Sinput.MAXAXISPERGAMEPAD];
				allGamepadAxis[i].minusOneTime = new float[Sinput.MAXAXISPERGAMEPAD];
				allGamepadAxis[i].minRecordedValue = new float[Sinput.MAXAXISPERGAMEPAD];
				allGamepadAxis[i].maxRecordedValue = new float[Sinput.MAXAXISPERGAMEPAD];

				allGamepadAxis[i].measuredFirstChange = new bool[Sinput.MAXAXISPERGAMEPAD];


				for (int a=0; a<allGamepadAxis[i].axisValues.Length; a++){
					allGamepadAxis[i].measuredFirstChange[a]=false;

					allGamepadAxis[i].axisValues[a] = Input.GetAxisRaw("J_"+(i+1).ToString()+"_"+(a+1).ToString());

					allGamepadAxis[i].zeroTime[a] = 0f;
					allGamepadAxis[i].plusOneTime[a] = 0f;
					allGamepadAxis[i].minusOneTime[a] = 0f;
					allGamepadAxis[i].minRecordedValue[a] = allGamepadAxis[i].axisValues[a]; 
					allGamepadAxis[i].maxRecordedValue[a] = allGamepadAxis[i].axisValues[a];
				}

				allGamepadAxis[i].buttonValues = new bool[Sinput.MAXBUTTONSPERGAMEPAD];
				for (int b=0; b<allGamepadAxis[i].buttonValues.Length; b++){
					KeyCode buttonCode = (KeyCode)Enum.Parse(typeof(KeyCode), string.Format("Joystick{0}Button{1}", (i+1), b));
					allGamepadAxis[i].buttonValues[b] = Input.GetKeyDown(buttonCode);
				}
			}
		}

		void KeyCheck(KeyCode k){
			if (!AcceptChangesFromSlot(-1)) return;

			if (Input.GetKeyDown(k)){
				changeFound = true;
				changedKey = k;
			}
			//if (Input.GetKeyUp(k)){
			//	changeFound = true;
			//	changedKey = k;
			//}
		}


		void MouseCheck(MouseInputType m){
			if (!AcceptChangesFromSlot(-1)) return;
			bool change = false;
			if (m == MouseInputType.Mouse0 && Input.GetKeyDown( KeyCode.Mouse0 )) change = true;
			if (m == MouseInputType.Mouse1 && Input.GetKeyDown( KeyCode.Mouse1 )) change = true;
			if (m == MouseInputType.Mouse2 && Input.GetKeyDown( KeyCode.Mouse2 )) change = true;
			if (m == MouseInputType.Mouse3 && Input.GetKeyDown( KeyCode.Mouse3 )) change = true;
			if (m == MouseInputType.Mouse4 && Input.GetKeyDown( KeyCode.Mouse4 )) change = true;
			if (m == MouseInputType.Mouse5 && Input.GetKeyDown( KeyCode.Mouse5 )) change = true;
			if (m == MouseInputType.Mouse6 && Input.GetKeyDown( KeyCode.Mouse6 )) change = true;
			if (change){
				changeFound = true;
				changedMouse = m;
			}
		}
	}
}