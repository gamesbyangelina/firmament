using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using SinputSystems;
namespace SinputSystems.Rebinding{
	public class SinputRebinder : MonoBehaviour {

		public SinputMonitor inputMonitor;

		private Control[] controls;
		private Control[] controlsDefaults;
		
		public RectTransform rebindMenuContainer;

		public GameObject devicePanelPrefab;
		public GameObject ControlPanelPrefab;
		public GameObject inputPanelPrefab;

		private List<devicePanel> devicePanels = new List<devicePanel>();

		public RebindMenuSettings rebindMenuSettings;
		public GameObject settingsPanelContainer;
		public GameObject settingsListPanel;

		public GameObject sensitivityPanelPrefab;
		public GameObject togglePanelPrefab;
		public GameObject invertPanelPrefab;

		

		// Use this for initialization
		void Start() {
			Init();
		}

		void Init () {
			Sinput.LoadControlScheme("MainControlScheme", false);
			controlsDefaults = Sinput.controls;
			
			Sinput.LoadControlScheme("MainControlScheme", true);
			controls = Sinput.controls;
			
			InitSettingsPanels();
			
			recordedPads = Sinput.gamepads;
			
			BuildRebindingPanels();
		}

		
		private bool rebinding = false;
		private int rebindingFrames=0;
		private int rebindingControlIndex=-1;
		private int rebindingInputIndex=-1;
		private string rebindingDevice;
		private Text rebindInputText;

		public ScrollRect scrollRect;
		private Vector2 autoScrollBuffer = new Vector2(200f, 100f);
		private Vector2 targetScrollPos = Vector2.zero;
		//public RectTransform contentPanel;
		public void ScrollTheMenu() {
			

			Canvas.ForceUpdateCanvases();

			bool autoscroll = false;

			RectTransform target;
			if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
				target = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.GetComponent<RectTransform>();
				if (target.gameObject.GetComponent<Scrollbar>()==null) autoscroll = true;
				if (!target.IsChildOf(rebindMenuContainer)) autoscroll = false;
				

				Vector2 targetLocalPosition = scrollRect.transform.InverseTransformPoint(target.position);
				Rect scrollRectRect = scrollRect.GetComponent<RectTransform>().rect;

				targetScrollPos = rebindMenuContainer.anchoredPosition;

				float amountAbove = targetLocalPosition.y - scrollRectRect.height*0.5f + autoScrollBuffer.y;
				if (amountAbove > 0f) {
					targetScrollPos.y -= amountAbove;
				}

				float amountBelow = targetLocalPosition.y + scrollRectRect.height*0.5f - autoScrollBuffer.y;
				if (amountBelow < 0f) {
					targetScrollPos.y -= amountBelow;
				}

				float amountleft = targetLocalPosition.x + scrollRectRect.width * 0.5f - autoScrollBuffer.x;
				if (amountleft < 0f) {
					targetScrollPos.x -= amountleft;
				}

				float amountRight = targetLocalPosition.x - scrollRectRect.width * 0.5f + autoScrollBuffer.x;
				if (amountRight > 0f) {
					targetScrollPos.x -= amountRight;
				}
			}

			if (Input.GetKey(KeyCode.Mouse0)) autoscroll = false;

			if (!autoscroll) { 
				targetScrollPos = rebindMenuContainer.anchoredPosition;
			}
			

			rebindMenuContainer.anchoredPosition = Vector2.Lerp(rebindMenuContainer.anchoredPosition, targetScrollPos, Time.deltaTime * 5f);


		}

		string[] recordedPads = new string[0];
		void Update(){
			//if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null) {
				ScrollTheMenu();
			//}
			
			//if the menu was rebuilt and what ~was~ selected was destroyed, select the nearest thing there is now
			if (reselectWait > 0) {
				reselectWait--;
				if (reselectWait == 0) {
					Selectable[] selectables = GameObject.FindObjectsOfType<Selectable>();
					float shortestDist = 99999f;
					int nearest = -1;
					for (int i = 0; i < selectables.Length; i++) {
						float dist = Vector3.Distance(reselectPosition, selectables[i].transform.position);
						if (dist < shortestDist) {
							nearest = i;
							shortestDist = dist;
						}
					}
					if (nearest != -1) selectables[nearest].Select();
					Canvas.ForceUpdateCanvases();
				}
			}

			bool gamepadsChanged = false;
			string[] gamepads = Sinput.gamepads;
			if (recordedPads.Length!=gamepads.Length) gamepadsChanged = true;
			if (!gamepadsChanged){
				for (int i=0; i<recordedPads.Length; i++){
					if (recordedPads[i].ToUpper() != gamepads[i].ToUpper()) gamepadsChanged = true;
				}
			}
			if (gamepadsChanged){
				//connected gamepads have changed
				rebinding = false;

				//Debug.Log("Gamepads changed!");

				recordedPads = Sinput.gamepads;
				Init();
				return;
			}


			if (!rebinding) return;
			rebindingFrames++;
			if (rebindingFrames<5) return;

			//we're ready to swap out an input now
			if (inputMonitor.changeFound){
				rebinding = false;
				rebindInputText.text = "?";
				//Debug.Log("CHANGE INPUT SETTING NOW!");

				
				InputDeviceType changedInputType = InputDeviceType.Keyboard;

				if (inputMonitor.changedKey != KeyCode.None){
					//change was keyboard input
					//Debug.Log(inputMonitor.changedKey.ToString());
					changedInputType = InputDeviceType.Keyboard;
				}
				if (inputMonitor.changedPadAxis.padIndex >=0){
					//change was a gamepad axis
					//Debug.Log("Found change in gamepad " + (inputMonitor.changedPadAxis.padIndex+1).ToString() + " axis " + (inputMonitor.changedPadAxis.inputIndex).ToString());
					changedInputType = InputDeviceType.GamepadAxis;
				}
				if (inputMonitor.changedPadButton.padIndex >=0){
					//change was a gamepad button
					//Debug.Log("Found change in gamepad " + (inputMonitor.changedPadButton.padIndex+1).ToString() + " button " + inputMonitor.changedPadButton.inputIndex.ToString());
					changedInputType = InputDeviceType.GamepadButton;
				}
				if (inputMonitor.changedMouse != MouseInputType.None){
					//change was mouse click
					//Debug.Log(inputMonitor.changedMouse.ToString());
					changedInputType = InputDeviceType.Mouse;
				}

				DeviceInput newInput = new DeviceInput(changedInputType);
				newInput.isCustom = true;
				newInput.deviceName = rebindingDevice;
				newInput.commonMappingType = CommonGamepadInputs.NOBUTTON;//don't remove this input when gamepads are unplugged/replugged

				if (changedInputType == InputDeviceType.Keyboard){
					newInput.keyboardKeyCode = inputMonitor.changedKey;
				}

				int padIndex = -1;
				if (changedInputType == InputDeviceType.GamepadButton){
					newInput.gamepadButtonNumber = inputMonitor.changedPadButton.inputIndex;
					newInput.displayName = "B" + inputMonitor.changedPadButton.inputIndex.ToString();

					padIndex = inputMonitor.changedPadButton.padIndex;
					List<int> slots = new List<int>();
					//string[] gamepads = Sinput.GetGamepads();
					for (int g=0;g<gamepads.Length; g++){
						if (gamepads[g].ToUpper() == rebindingDevice.ToUpper()) slots.Add(g);
					}
					newInput.allowedSlots = slots.ToArray();
				}
					
				if (changedInputType == InputDeviceType.GamepadAxis){
					SinputMonitor.gamepadStateChange axisChange = inputMonitor.changedPadAxis;

					newInput.gamepadAxisNumber = axisChange.inputIndex;
					newInput.displayName = "A" + axisChange.inputIndex.ToString();
					if (axisChange.axisMotionIsPositive) newInput.displayName += "+";
					if (!axisChange.axisMotionIsPositive) newInput.displayName += "-";

					newInput.invertAxis = false;
					if (!axisChange.axisMotionIsPositive) newInput.invertAxis = true;

					newInput.clampAxis = true;
					newInput.axisButtoncompareVal = 0.4f;
					newInput.defaultAxisValue = 0f;

					newInput.rescaleAxis = false;
					if (axisChange.restingValue != 0f){
						newInput.rescaleAxis = true;
						if (axisChange.restingValue<0f){
							newInput.rescaleAxisMin = -1f;
							newInput.rescaleAxisMax = 1f;
						}else{
							newInput.rescaleAxisMin = 1f;
							newInput.rescaleAxisMax = -1f;
						}
					}

					padIndex = axisChange.padIndex;
					List<int> slots = new List<int>();
					//string[] gamepads = Sinput.GetGamepads();
					for (int g=0;g<gamepads.Length; g++){
						if (gamepads[g].ToUpper() == rebindingDevice.ToUpper()) slots.Add(g);
					}
					newInput.allowedSlots = slots.ToArray();

				}

				if (changedInputType == InputDeviceType.GamepadAxis || changedInputType == InputDeviceType.GamepadButton){
					//lets also set all other inputs on this control with matching allowed slots to be custom and remove common binding
					//this should preserve stuff with the same common binding from being ignored when re reload common bindings
					for (int i=0; i<controls[rebindingControlIndex].inputs.Count; i++){
						bool sameDevice = false;
						if (controls[rebindingControlIndex].inputs[i].commonMappingType != CommonGamepadInputs.NOBUTTON){
							for (int k=0; k<controls[rebindingControlIndex].inputs[i].allowedSlots.Length; k++){
								if (controls[rebindingControlIndex].inputs[i].allowedSlots[k] == padIndex) sameDevice = true;
							}
						}
						if (sameDevice){
							controls[rebindingControlIndex].inputs[i].isCustom = true;
							controls[rebindingControlIndex].inputs[i].commonMappingType = CommonGamepadInputs.NOBUTTON;
							controls[rebindingControlIndex].inputs[i].deviceName = rebindingDevice;

							/*List<int> slots = new List<int>();
							string[] gamepads = Sinput.GetGamepads();
							for (int g=0;g<gamepads.Length; g++){
								if (gamepads[g].ToUpper() == rebindingDevice.ToUpper()) slots.Add(g);
							}
							newInput.allowedSlots = slots.ToArray();*/
						}
					}
				}

				if (changedInputType == InputDeviceType.Mouse){
					
					newInput.mouseInputType = inputMonitor.changedMouse;
				}

				controls[rebindingControlIndex].inputs[rebindingInputIndex] = newInput;

				rebindInputText.text = controls[rebindingControlIndex].inputs[rebindingInputIndex].GetDisplayName();

				
				//rebindInputText.transform.parent.GetC
				//BuildRebindingPanels();
			}
		}

		//functions called by UI
		public void SetDefaults(){
			//PlayerPrefs.DeleteAll();
			SinputFileIO.DeleteSavedControls(Sinput.controlSchemeName);
			Init();
			SettingsToDefault();
		}
		public void Apply(){
			ApplySettings();
			SinputFileIO.SaveControls(controls, Sinput.controlSchemeName);
			Init();
		}

		public void CollapseControlPanel(GameObject panel){
			if (rebinding) return;
			panel.SetActive(!panel.activeSelf);
		}

		public void BeginRebindInput(int controlIndex, int inputIndex, string deviceName, Text text){
			if (rebinding) return;
			//Debug.Log("Begind rebind for : " + controls[controlIndex].name + " input " + controls[controlIndex].inputs[inputIndex].GetDisplayName() + " of " + deviceName);
			rebinding = true;
			rebindingFrames = 0;
			rebindingControlIndex = controlIndex;
			rebindingInputIndex = inputIndex;
			rebindInputText = text;
			rebindInputText.text = "...";
			rebindingDevice = deviceName;

			inputMonitor.SetListeningDevice(rebindingDevice);

			

		}

		public void DeleteInput(int controlIndex, int inputIndex, string deviceName, Transform transformToDelete){
			if (rebinding) return;

			
			//Debug.Log("Delete input : " + controls[controlIndex].name + " input " + controls[controlIndex].inputs[inputIndex].GetDisplayName() + " of " + deviceName);
			controls[controlIndex].inputs.RemoveAt(inputIndex);

			//select UI element to the right
			transformToDelete.Find("InputDelete").GetComponent<Button>().FindSelectableOnRight().Select();


			//remove input panel
			for (int d=0; d<devicePanels.Count; d++) {
				for (int c=0; c<devicePanels[d].controlPanels.Count; c++) {
					for (int i=0; i<devicePanels[d].controlPanels[c].inputPanels.Count; i++) {
						if (devicePanels[d].controlPanels[c].inputPanels[i].inputPanelObj == transformToDelete.gameObject) {
							devicePanels[d].controlPanels[c].inputPanels.RemoveAt(i);
							i--;
						}
					}
				}
			}

			//remove object that actually is showing the thing
			Destroy(transformToDelete.gameObject);

			

			//BuildRebindingPanels();
		}
		public void ResetControlInputs(int controlIndex, string deviceName, InputDeviceType deviceType, int deviceSlotIndex){
			if (rebinding) return;
			

			//Debug.Log("reset inputs for : " + controls[controlIndex].name + " of " + deviceName);

			int padIndex = -1;
			string[] padNames = Input.GetJoystickNames();
			for (int i=0;i<padNames.Length; i++){
				if (padNames[i].ToUpper()==deviceName.ToUpper()) padIndex = i;
			}
				

			//remove current inputs for this control from this device
			for (int i=0; i<controls[controlIndex].inputs.Count; i++){
				bool removeControl = false;
				if (deviceName == "KeyboardMouse" && controls[controlIndex].inputs[i].inputType == InputDeviceType.Keyboard) removeControl = true;
				if (deviceName == "KeyboardMouse" && controls[controlIndex].inputs[i].inputType == InputDeviceType.Mouse) removeControl = true;
				if (deviceName != "KeyboardMouse" && controls[controlIndex].inputs[i].inputType == InputDeviceType.GamepadAxis) removeControl = true;
				if (deviceName != "KeyboardMouse" && controls[controlIndex].inputs[i].inputType == InputDeviceType.GamepadButton) removeControl = true;
				if (controls[controlIndex].inputs[i].inputType == InputDeviceType.Virtual) removeControl = false; //don't remove virtual inputs

				//make sure we only remove inputs for this gamepad
				if (removeControl && deviceName != "KeyboardMouse"){
					bool matchingPad = false;
					for (int k=0; k<controls[controlIndex].inputs[i].allowedSlots.Length; k++){
						if (controls[controlIndex].inputs[i].allowedSlots[k] == padIndex) matchingPad = true;
					}
					if (!matchingPad) removeControl = false;
				}

				if (removeControl){
					Debug.Log("Removing control - " + deviceName + " - " + controls[controlIndex].inputs[i].GetDisplayName());
					controls[controlIndex].inputs.RemoveAt(i);
					i--;
				}
			}

			//add default inputs
			for (int i=0; i<controlsDefaults[controlIndex].inputs.Count; i++){
				bool addControl = false;
				if (deviceName == "KeyboardMouse" && controlsDefaults[controlIndex].inputs[i].inputType == InputDeviceType.Keyboard) addControl = true;
				if (deviceName == "KeyboardMouse" && controlsDefaults[controlIndex].inputs[i].inputType == InputDeviceType.Mouse) addControl = true;
				if (deviceName != "KeyboardMouse" && controlsDefaults[controlIndex].inputs[i].inputType == InputDeviceType.GamepadAxis) addControl = true;
				if (deviceName != "KeyboardMouse" && controlsDefaults[controlIndex].inputs[i].inputType == InputDeviceType.GamepadButton) addControl = true;
				if (controlsDefaults[controlIndex].inputs[i].inputType == InputDeviceType.Virtual) addControl = false; //don't add virtual inputs

				//make sure we only add inputs for this gamepad
				if (addControl && deviceName != "KeyboardMouse"){
					bool matchingPad = false;
					for (int k=0; k<controlsDefaults[controlIndex].inputs[i].allowedSlots.Length; k++){
						if (controlsDefaults[controlIndex].inputs[i].allowedSlots[k] == padIndex) matchingPad = true;
					}
					if (!matchingPad) addControl = false;
				}

				if (addControl){
					Debug.Log("Adding control - " + deviceName + controlsDefaults[controlIndex].inputs[i].GetDisplayName());
					controls[controlIndex].inputs.Add(controlsDefaults[controlIndex].inputs[i]);
				}
			}

			//now we rebuild this part of the menu
			//first we remove any old input panels for this control
			int devicePanelIndex = -1;
			for (int d=0; d<devicePanels.Count; d++) {
				if (devicePanels[d].deviceName == deviceName) devicePanelIndex = d;
			}
			int controlPanelIndex = -1;
			for (int c =0; c< devicePanels[devicePanelIndex].controlPanels.Count; c++) {
				if (devicePanels[devicePanelIndex].controlPanels[c].controlIndex == controlIndex) controlPanelIndex = c;
			}
			//Debug.Log(devicePanelIndex);
			//Debug.Log(controlPanelIndex);
			Debug.Log("Device Panel - " + devicePanelIndex.ToString() + " - " + deviceName);
			for (int i=0; i< devicePanels[devicePanelIndex].controlPanels[controlPanelIndex].inputPanels.Count; i++) {
				Debug.Log("I happen: " + devicePanels[devicePanelIndex].controlPanels[controlPanelIndex].inputPanels[i].inputButtonText.text);
				Destroy(devicePanels[devicePanelIndex].controlPanels[controlPanelIndex].inputPanels[i].inputPanelObj);
				devicePanels[devicePanelIndex].controlPanels[controlPanelIndex].inputPanels.RemoveAt(i);
				i--;
			}

			//then add the new input panels
			devicePanels[devicePanelIndex].controlPanels[controlPanelIndex] = AddInputPanels(controlIndex, devicePanels[devicePanelIndex].controlPanels[controlPanelIndex], deviceType, deviceName, deviceSlotIndex, devicePanels[devicePanelIndex].controlPanels[controlPanelIndex].controlPanelObj);

			//then set the index of the add/reset buttons so they are at the end again
			devicePanels[devicePanelIndex].controlPanels[controlPanelIndex].addInputButton.transform.SetSiblingIndex(98);
			devicePanels[devicePanelIndex].controlPanels[controlPanelIndex].resetInputsButton.transform.SetSiblingIndex(99);

			//BuildRebindingPanels();

			

		}
		public void AddControlInput(int controlIndex, string deviceName, controlPanel cp){
			if (rebinding) return;
			//Debug.Log("Add new input for : " + controls[controlIndex].name + " of " + deviceName);

			

			InputDeviceType t = InputDeviceType.Keyboard;
			if (deviceName != "KeyboardMouse") t = InputDeviceType.GamepadButton;
			DeviceInput newInput = new DeviceInput(t);

			newInput.isCustom = true;
			newInput.deviceName = deviceName;
			newInput.commonMappingType = CommonGamepadInputs.NOBUTTON;//don't remove this input when gamepads are unplugged/replugged

			if (t == InputDeviceType.Keyboard){
				newInput.keyboardKeyCode = KeyCode.None;
			}

			if (t == InputDeviceType.GamepadButton){
				newInput.gamepadButtonNumber = 18;
				newInput.displayName = "B?";

				string[] padNames = Input.GetJoystickNames();
				List<int> allowedSlots = new List<int>();
				for (int i=0;i<padNames.Length; i++){
					if (padNames[i].ToUpper()==deviceName.ToUpper()) allowedSlots.Add(i);
				}
				newInput.allowedSlots = allowedSlots.ToArray();
			}
				

			controls[controlIndex].inputs.Add(newInput);

			//BuildRebindingPanels();


			///newer input adding code
			GameObject newInputPanelObj = (GameObject)GameObject.Instantiate(inputPanelPrefab);
			newInputPanelObj.transform.SetParent(cp.controlPanelObj.transform);
			newInputPanelObj.transform.localScale = Vector3.one;
			inputPanel newInputPanel = new inputPanel();

			newInputPanel.inputPanelObj = newInputPanelObj;
			newInputPanel.inputButton = newInputPanelObj.transform.Find("Input").GetComponent<Button>();
			newInputPanel.deleteButton = newInputPanelObj.transform.Find("InputDelete").GetComponent<Button>();
			newInputPanel.inputButtonText = newInputPanel.inputButton.transform.Find("Text").GetComponent<Text>();

			int inputIndex = cp.inputPanels.Count;
			inputIndex = controls[controlIndex].inputs.Count - 1;
			//Debug.Log("INPUT INDEX: " + inputIndex.ToString());
			newInputPanel.inputButton.onClick.AddListener(delegate { BeginRebindInput(controlIndex, inputIndex, deviceName, newInputPanel.inputButtonText); });
			newInputPanel.deleteButton.onClick.AddListener(delegate { DeleteInput(controlIndex, inputIndex, deviceName, newInputPanelObj.transform); });

			newInputPanel.inputButtonText.text = controls[controlIndex].inputs[controls[controlIndex].inputs.Count-1].GetDisplayName();

			cp.inputPanels.Add(newInputPanel);

			//select the new input button
			newInputPanel.inputButton.Select();

			//make sure input buttons aren't before add/reset buttons
			cp.addInputButton.transform.SetSiblingIndex(98);
			cp.resetInputsButton.transform.SetSiblingIndex(99);

			//begin rebind immediately
			BeginRebindInput(controlIndex, inputIndex, deviceName, newInputPanel.inputButtonText);

			
		}


		//stuff for building UI
		private int reselectWait = 0;
		private Vector3 reselectPosition = Vector3.zero;
		void BuildRebindingPanels(){

			//find which thing is currently selected, se we know where it is, and if we need to select something new after the rebuild
			GameObject currentlySelected = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
			reselectPosition = Vector3.zero;
			bool currentlySelectedDestroyed = false;
			if (currentlySelected == null) currentlySelectedDestroyed = true;
			if (!currentlySelectedDestroyed) reselectPosition = currentlySelected.transform.position;


			//clear any existing panels
			for (int i=0; i<devicePanels.Count; i++){
				if (!currentlySelectedDestroyed && currentlySelected.transform.IsChildOf(devicePanels[i].devicePanelObj.transform)) currentlySelectedDestroyed = true;
				GameObject.Destroy(devicePanels[i].devicePanelObj);
			}

			if (UnityEngine.EventSystems.EventSystem.current.currentInputModule == null) currentlySelectedDestroyed = false;

			devicePanels = new List<devicePanel>();

			//add keyboard/mouse cdevice panel
			AddDevicePanel(InputDeviceType.Keyboard, "KeyboardMouse", -1);

			//add gamepad device panels
			string[] pads = Sinput.gamepads;
			for (int p=0; p<pads.Length; p++){
				bool deviceAlreadyListed = false;
				for (int d=0; d<devicePanels.Count; d++){
					if (devicePanels[d].deviceName == pads[p]) deviceAlreadyListed = true;
				}

				if (!deviceAlreadyListed && pads[p] != "") AddDevicePanel(InputDeviceType.GamepadAxis, pads[p], p);
			}


			if (currentlySelectedDestroyed) {
				//Debug.Log("Reselect needed");
				reselectWait = 3;
				//the menu item that was selected has been destroyed, let's select something nearby a couple frames from now (selecting stuff the same frame seems a problem)
				
			}
			
		}
	

		void AddDevicePanel(InputDeviceType deviceType, string deviceName, int deviceSlotIndex){
			GameObject newDevicePanelObj = (GameObject)GameObject.Instantiate(devicePanelPrefab);
			newDevicePanelObj.name = deviceName;
			newDevicePanelObj.transform.SetParent(rebindMenuContainer);
			newDevicePanelObj.transform.localScale = Vector3.one;
			devicePanel newDevicePanel = new devicePanel();
			newDevicePanel.devicePanelObj = newDevicePanelObj;

			newDevicePanel.deviceName = deviceName;

			newDevicePanel.foldoutButton = newDevicePanelObj.transform.Find("DeviceNameFoldout").Find("FoldoutButton").GetComponent<Button>();
			newDevicePanel.foldoutButton.enabled = false;//lets not have foldouts for now, they get reset all the time anyway
			newDevicePanel.deviceNameText = newDevicePanel.foldoutButton.transform.Find("DeviceNameText").GetComponent<Text>();

			newDevicePanel.foldoutButton.onClick.AddListener(delegate {CollapseControlPanel(newDevicePanelObj.transform.Find("ControlsPanel").gameObject); });

			newDevicePanel.deviceNameText.text = "Keyboard/Mouse:";
			if (deviceType == InputDeviceType.GamepadAxis || deviceType == InputDeviceType.GamepadButton){
				newDevicePanel.deviceNameText.text = "Gamepad: \"" + deviceName + "\"";
			}

			newDevicePanel.controlPanels = new List<controlPanel>();

			//int controlPanelIndex = 0;

			for (int c=0; c<controls.Length; c++){
				GameObject newControlPanelObj = (GameObject)GameObject.Instantiate(ControlPanelPrefab);
				newControlPanelObj.name = controls[c].name;
				newControlPanelObj.transform.SetParent(newDevicePanelObj.transform.Find("ControlsPanel").transform);
				newControlPanelObj.transform.localScale = Vector3.one;
				controlPanel newControlPanel = new controlPanel();
				newControlPanel.controlPanelObj = newControlPanelObj;

				newControlPanel.controlNameText = newControlPanelObj.transform.Find("ControlName").Find("ControlNameText").GetComponent<Text>();
				newControlPanel.addInputButton = newControlPanelObj.transform.Find("AddInput").GetComponent<Button>();
				newControlPanel.resetInputsButton = newControlPanelObj.transform.Find("ResetControl").GetComponent<Button>();

				newControlPanel.controlNameText.text = controls[c].name;
				//Debug.Log(c);
				int controlIndex = c;
				newControlPanel.controlIndex = controlIndex;
				newControlPanel.addInputButton.onClick.AddListener(delegate {AddControlInput(controlIndex,deviceName, newControlPanel); });
				//Debug.Log(newDevicePanel.controlPanels.Count);
				newControlPanel.resetInputsButton.onClick.AddListener(delegate {ResetControlInputs(controlIndex,deviceName, deviceType, deviceSlotIndex); });


				newControlPanel.inputPanels = new List<inputPanel>();

				newControlPanel = AddInputPanels(c, newControlPanel, deviceType, deviceName, deviceSlotIndex, newControlPanelObj);

				newDevicePanel.controlPanels.Add(newControlPanel);
			}


			devicePanels.Add(newDevicePanel);
		}

		controlPanel AddInputPanels(int c, controlPanel newControlPanel, InputDeviceType deviceType, string deviceName, int deviceSlotIndex, GameObject cpObj) {

			for (int i = 0; i < controls[c].inputs.Count; i++) {
				bool applicableInput = false;
				if (deviceType == InputDeviceType.GamepadAxis || deviceType == InputDeviceType.GamepadButton) {
					if (null != controls[c].inputs[i].allowedSlots) {
						for (int s = 0; s < controls[c].inputs[i].allowedSlots.Length; s++) {
							if (controls[c].inputs[i].allowedSlots[s] == deviceSlotIndex) applicableInput = true;
						}
					}
					if (controls[c].inputs[i].deviceName == deviceName) applicableInput = true;
				}
				if (deviceType == InputDeviceType.Keyboard || deviceType == InputDeviceType.Mouse) {
					if (controls[c].inputs[i].inputType == InputDeviceType.Keyboard) applicableInput = true;
					if (controls[c].inputs[i].inputType == InputDeviceType.Mouse) applicableInput = true;
				}
				if (applicableInput) {
					//this input is referring to this device
					GameObject newInputPanelObj = (GameObject)GameObject.Instantiate(inputPanelPrefab);
					newInputPanelObj.transform.SetParent(cpObj.transform);
					newInputPanelObj.transform.localScale = Vector3.one;
					inputPanel newInputPanel = new inputPanel();

					newInputPanel.inputPanelObj = newInputPanelObj;

					newInputPanel.inputButton = newInputPanelObj.transform.Find("Input").GetComponent<Button>();
					newInputPanel.deleteButton = newInputPanelObj.transform.Find("InputDelete").GetComponent<Button>();
					newInputPanel.inputButtonText = newInputPanel.inputButton.transform.Find("Text").GetComponent<Text>();

					int inputIndex = i;
					newInputPanel.inputButton.onClick.AddListener(delegate { BeginRebindInput(c, inputIndex, deviceName, newInputPanel.inputButtonText); });
					newInputPanel.deleteButton.onClick.AddListener(delegate { DeleteInput(c, inputIndex, deviceName, newInputPanelObj.transform); });

					newInputPanel.inputButtonText.text = controls[c].inputs[i].GetDisplayName();

					newControlPanel.inputPanels.Add(newInputPanel);
				}
			}

			newControlPanel.addInputButton.transform.SetSiblingIndex(98);
			newControlPanel.resetInputsButton.transform.SetSiblingIndex(99);

			newControlPanel.addInputButton.transform.SetSiblingIndex(98);
			newControlPanel.resetInputsButton.transform.SetSiblingIndex(99);
			return newControlPanel;
		}

		public struct devicePanel{
			public GameObject devicePanelObj;
			public Button foldoutButton;
			public Text deviceNameText;
			public string deviceName;

			public List<controlPanel> controlPanels;
		}
		public struct controlPanel{
			public GameObject controlPanelObj;

			public int controlIndex;
			public Text controlNameText;
			public Button addInputButton;
			public Button resetInputsButton;
			public List<inputPanel> inputPanels;
		}
		public struct inputPanel{
			public GameObject inputPanelObj;
			public Button inputButton;
			public Text inputButtonText;
			public Button deleteButton;
		}



		private List<settingPanel> settingPanels;
		public struct settingPanel {
			public bool isMouseSensitivity;

			public bool isToggle;
			public string toggleControl;

			public bool isInvert;
			public string invertSmartControl;

			public bool isScale;
			public string scaleSettingName;
			public List<string> scaleSmartControls;

			//ui vars
			public Text label;
			public Slider slider;
			public Text sliderText;
			public Toggle toggle;
			public Text toggleText;
			public Button resetButton;

			//actual setting values
			public float val;
			public float defaultVal;
			public bool settingBool;
			public bool defaultSettingBool;

			public float minVal;
			public float maxVal;
		}

		bool initialisedSettings = false;
		bool showSettings = false;
		void InitSettingsPanels() {
			if (initialisedSettings) return;
			initialisedSettings = true;

			showSettings = false;
			
			settingPanels = new List<settingPanel>();
			

			if (rebindMenuSettings == null || !rebindMenuSettings.showSettings) {
				settingsPanelContainer.SetActive(false);
				return;
			}

			//mouse sensitivity
			if (rebindMenuSettings.showMouseSensitivity) {
				settingPanel newSettingPanel = new settingPanel();

				newSettingPanel.isMouseSensitivity = true;
				newSettingPanel.minVal = rebindMenuSettings.minMouseSens;
				newSettingPanel.maxVal = rebindMenuSettings.maxMouseSens;
				newSettingPanel.defaultVal = 1f;
				newSettingPanel.val = Sinput.mouseSensitivity;

				GameObject newPanelObj = (GameObject)GameObject.Instantiate(sensitivityPanelPrefab);
				newPanelObj.transform.SetParent(settingsListPanel.transform);
				newPanelObj.transform.localScale = Vector3.one;

				newSettingPanel.label = newPanelObj.transform.Find("SettingName").Find("Text").GetComponent<Text>();
				newSettingPanel.slider = newPanelObj.transform.Find("Slider").GetComponent<Slider>();
				newSettingPanel.sliderText = newSettingPanel.slider.transform.Find("Handle Slide Area").Find("Handle").Find("Text").GetComponent<Text>();
				newSettingPanel.resetButton = newPanelObj.transform.Find("ResetControl").GetComponent<Button>();

				newSettingPanel.label.text = "Mouse Sensitivity";
				newSettingPanel.slider.minValue = newSettingPanel.minVal;
				newSettingPanel.slider.maxValue = newSettingPanel.maxVal;
				newSettingPanel.slider.value = newSettingPanel.val;
				newSettingPanel.sliderText.text = ClipStr(newSettingPanel.val.ToString(), 5);

				int settingIndex = settingPanels.Count;
				newSettingPanel.slider.onValueChanged.AddListener(delegate { SliderSettingChange(settingIndex); });
				newSettingPanel.resetButton.onClick.AddListener(delegate { SettingReset(settingIndex); });

				


				settingPanels.Add(newSettingPanel);
				showSettings = true;
			}

			//toggles
			for (int i = 0; i < rebindMenuSettings.toggleableControls.Count; i++) {
				settingPanel newSettingPanel = new settingPanel();

				newSettingPanel.isToggle = true;
				newSettingPanel.toggleControl = rebindMenuSettings.toggleableControls[i];
				newSettingPanel.defaultSettingBool = false;
				newSettingPanel.settingBool = Sinput.GetToggle(newSettingPanel.toggleControl); ;

				GameObject newPanelObj = (GameObject)GameObject.Instantiate(togglePanelPrefab);
				newPanelObj.transform.SetParent(settingsListPanel.transform);
				newPanelObj.transform.localScale = Vector3.one;

				newSettingPanel.label = newPanelObj.transform.Find("SettingName").Find("Text").GetComponent<Text>();
				newSettingPanel.toggle = newPanelObj.transform.Find("Toggle").GetComponent<Toggle>();
				newSettingPanel.toggleText = newSettingPanel.toggle.transform.Find("Label").GetComponent<Text>();
				newSettingPanel.resetButton = newPanelObj.transform.Find("ResetControl").GetComponent<Button>();

				newSettingPanel.label.text = "Toggle " + newSettingPanel.toggleControl;
				newSettingPanel.toggle.isOn = newSettingPanel.settingBool;
				if (newSettingPanel.settingBool) {
					newSettingPanel.toggleText.text = "Toggle " + newSettingPanel.toggleControl;
				} else {
					newSettingPanel.toggleText.text = "Hold " + newSettingPanel.toggleControl;
				}

				int settingIndex = settingPanels.Count;
				newSettingPanel.toggle.onValueChanged.AddListener(delegate { ToggleSettingChange(settingIndex); });
				newSettingPanel.resetButton.onClick.AddListener(delegate { SettingReset(settingIndex); });

				settingPanels.Add(newSettingPanel);
				showSettings = true;
			}

			//inverts
			for (int i = 0; i < rebindMenuSettings.invertableSmartControls.Count; i++) {
				settingPanel newSettingPanel = new settingPanel();

				newSettingPanel.isInvert = true;
				newSettingPanel.invertSmartControl = rebindMenuSettings.invertableSmartControls[i];
				newSettingPanel.defaultSettingBool = false;
				newSettingPanel.settingBool = Sinput.GetInverted(newSettingPanel.invertSmartControl);

				GameObject newPanelObj = (GameObject)GameObject.Instantiate(invertPanelPrefab);
				newPanelObj.transform.SetParent(settingsListPanel.transform);
				newPanelObj.transform.localScale = Vector3.one;

				newSettingPanel.label = newPanelObj.transform.Find("SettingName").Find("Text").GetComponent<Text>();
				newSettingPanel.toggle = newPanelObj.transform.Find("Toggle").GetComponent<Toggle>();
				newSettingPanel.resetButton = newPanelObj.transform.Find("ResetControl").GetComponent<Button>();

				newSettingPanel.label.text = "Invert " + newSettingPanel.invertSmartControl;
				newSettingPanel.toggle.isOn = newSettingPanel.settingBool;

				int settingIndex = settingPanels.Count;
				newSettingPanel.toggle.onValueChanged.AddListener(delegate { ToggleSettingChange(settingIndex); });
				newSettingPanel.resetButton.onClick.AddListener(delegate { SettingReset(settingIndex); });

				settingPanels.Add(newSettingPanel);
				showSettings = true;
			}

			//scalables
			for (int i = 0; i < rebindMenuSettings.scalables.Count; i++) {
				settingPanel newSettingPanel = new settingPanel();

				newSettingPanel.isScale = true;
				newSettingPanel.scaleSettingName = rebindMenuSettings.scalables[i].scalableName;
				newSettingPanel.scaleSmartControls = new List<string>();
				for (int k=0; k<rebindMenuSettings.scalables[i].scalableSmartControls.Count; k++) {
					newSettingPanel.scaleSmartControls.Add(rebindMenuSettings.scalables[i].scalableSmartControls[k]);
				}
				newSettingPanel.minVal = rebindMenuSettings.scalables[i].minScale;
				newSettingPanel.maxVal = rebindMenuSettings.scalables[i].maxScale;
				newSettingPanel.defaultVal = 1f;
				newSettingPanel.val = Sinput.GetScale(newSettingPanel.scaleSmartControls[0]);

				GameObject newPanelObj = (GameObject)GameObject.Instantiate(sensitivityPanelPrefab);
				newPanelObj.transform.SetParent(settingsListPanel.transform);
				newPanelObj.transform.localScale = Vector3.one;

				newSettingPanel.label = newPanelObj.transform.Find("SettingName").Find("Text").GetComponent<Text>();
				newSettingPanel.slider = newPanelObj.transform.Find("Slider").GetComponent<Slider>();
				newSettingPanel.sliderText = newSettingPanel.slider.transform.Find("Handle Slide Area").Find("Handle").Find("Text").GetComponent<Text>();
				newSettingPanel.resetButton = newPanelObj.transform.Find("ResetControl").GetComponent<Button>();

				newSettingPanel.label.text = newSettingPanel.scaleSettingName + " Sensitivity";
				newSettingPanel.slider.minValue = newSettingPanel.minVal;
				newSettingPanel.slider.maxValue = newSettingPanel.maxVal;
				newSettingPanel.slider.value = newSettingPanel.val;
				newSettingPanel.sliderText.text = ClipStr(newSettingPanel.val.ToString(), 5);

				int settingIndex = settingPanels.Count;
				newSettingPanel.slider.onValueChanged.AddListener(delegate { SliderSettingChange(settingIndex); });
				newSettingPanel.resetButton.onClick.AddListener(delegate { SettingReset(settingIndex); });

				settingPanels.Add(newSettingPanel);
				showSettings = true;
			}

			if (!showSettings) {
				settingsPanelContainer.SetActive(false);
			}
		}

		void SettingsToDefault() {
			for (int i=0; i<settingPanels.Count; i++) {
				settingPanel thisPanel = settingPanels[i];
				thisPanel.val = settingPanels[i].defaultVal;
				thisPanel.settingBool = settingPanels[i].defaultSettingBool;

				if (thisPanel.isMouseSensitivity || thisPanel.isScale) {
					thisPanel.slider.value = thisPanel.val;
				}
				if (thisPanel.isToggle || thisPanel.isInvert) {
					thisPanel.toggle.isOn = thisPanel.settingBool;
					if (thisPanel.isToggle) {
						if (thisPanel.settingBool) {
							thisPanel.toggleText.text = "Toggle " + thisPanel.toggleControl;
						} else {
							thisPanel.toggleText.text = "Hold " + thisPanel.toggleControl;
						}
					}
				}

				settingPanels[i] = thisPanel;
			}
		}

		void ApplySettings() {
			for (int i = 0; i < settingPanels.Count; i++) {

				if (settingPanels[i].isMouseSensitivity) {
					Sinput.mouseSensitivity = settingPanels[i].val;
				}
				if (settingPanels[i].isToggle) {
					for (int k=0; k<controls.Length; k++) {
						if (controls[k].name == settingPanels[i].toggleControl) controls[k].isToggle = settingPanels[i].settingBool;
					}

					/*Debug.Log("Setting " + settingPanels[i].toggleControl + " toggle to " + settingPanels[i].settingBool.ToString());
					Sinput.SetToggle(settingPanels[i].toggleControl, settingPanels[i].settingBool);
					Debug.Log("Setting " + settingPanels[i].toggleControl + " is toggle is " + Sinput.GetToggle(settingPanels[i].toggleControl).ToString());*/
				}
				if (settingPanels[i].isInvert) {
					Sinput.SetInverted(settingPanels[i].invertSmartControl, settingPanels[i].settingBool);
				}
				if (settingPanels[i].isScale) {
					for (int k = 0; k < settingPanels[i].scaleSmartControls.Count; k++) {
						Sinput.SetScale(settingPanels[i].scaleSmartControls[k], settingPanels[i].val);
					}
				}


			}
		}

		public void SliderSettingChange(int i) {
			//Debug.Log(i);
			settingPanel s = settingPanels[i];
			s.val = s.slider.value;
			s.sliderText.text = ClipStr(s.val.ToString(),5);

			//be able to navigate to right when slider is at max value
			Navigation sliderNav = new Navigation();
			Selectable aboveSelectable = s.slider.FindSelectableOnUp();
			Selectable belowSelectable = s.slider.FindSelectableOnDown();
			sliderNav.mode = Navigation.Mode.Explicit;
			if (s.slider.value == s.slider.maxValue) sliderNav.selectOnRight = s.resetButton.GetComponent<Selectable>();
			sliderNav.selectOnUp = aboveSelectable;
			sliderNav.selectOnDown = belowSelectable;
			s.slider.navigation = sliderNav;

			settingPanels[i] = s;


			
		}
		public void ToggleSettingChange(int i) {
			settingPanel s = settingPanels[i];
			s.settingBool = s.toggle.isOn;
			if (s.isToggle) {
				if (s.settingBool) {
					s.toggleText.text = "Toggle " + s.toggleControl;
				} else {
					s.toggleText.text = "Hold " + s.toggleControl;
				}
			}
			settingPanels[i] = s;
		}
		public void SettingReset(int i) {
			settingPanel s = settingPanels[i];
			s.val = s.defaultVal;
			s.settingBool = s.defaultSettingBool;

			if (s.isMouseSensitivity || s.isScale) {
				s.slider.value = s.val;
				s.sliderText.text = s.val.ToString();
			}
			if (s.isToggle || s.isInvert) {
				s.toggle.isOn = s.settingBool;
			}
			if (s.isToggle) {
				if (s.settingBool) {
					s.toggleText.text = "Toggle " + s.toggleControl;
				} else {
					s.toggleText.text = "Hold " + s.toggleControl;
				}
			}

			settingPanels[i] = s;
		}

		string ClipStr(string s, int maxLen) {
			if (s.Length < maxLen) return s;
			return s.Substring(0, maxLen);
		}

	}
}