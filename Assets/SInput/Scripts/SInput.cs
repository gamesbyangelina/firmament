using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using SinputSystems;


public static class Sinput {

	//Fixed number of gamepad things unity can handle, used mostly by GamepadDebug and InputManagerReplacementGenerator.
	//Sinput can handle as many of these as you want to throw at it buuuuut, unty can only handle so many and Sinput is wrapping unity input for now
	//You can try bumping up the range of these but you might have trouble
	//(EG, you can probably get axis of gamepads in slots over 8, but maybe not buttons?)
	public static int MAXCONNECTEDGAMEPADS { get {return 11; } }
	public static int MAXAXISPERGAMEPAD { get {return 28; } }
	public static int MAXBUTTONSPERGAMEPAD { get {return 20; } }


	//are keyboard & mouse used by two seperate players (distinct=true) or by a single player (distinct=false)
	private static bool keyboardAndMouseAreDistinct = false;

	//how many devices can be connected
	private static int _totalPossibleDeviceSlots;
	/// <summary>
	/// Total possible device slots that Sinput may detect. (Including keyboard, mouse, virtual, and any slots)
	/// </summary>
	public static int totalPossibleDeviceSlots { get { return _totalPossibleDeviceSlots; } }

	//overall mouse sensitivity
	/// <summary>
	/// Overall mouse sensitivity (effects all Controls bound to mouse movements)
	/// </summary>
	public static float mouseSensitivity = 1f;

	/// <summary>
	/// Name of control scheme used when saving/loading custom control schemes
	/// <para>unless you're doing fancy stuff like switching between various control schemes, this is probably best left alone.</para>
	/// </summary>
	public static string controlSchemeName = "ControlScheme";

	//the control scheme, set it with SetControlScheme()
	private static Control[] _controls;
	/// <summary>
	/// Returns a copy of the current Sinput control list
	/// <para>Note: This is not the fastest thing so don't go calling it in a loop every frame, make yourself a local copy.</para>
	/// </summary>
	public static Control[] controls {
		get {
			Init();

			//make a copy of the controls so we're definitely not returning something that will effect _controls
			Control[] returnControlList = new Control[_controls.Length];
			for (int i = 0; i < _controls.Length; i++) {
				returnControlList[i] = new Control(_controls[i].name);
				for (int k = 0; k < _controls[i].commonMappings.Count; k++) {
					returnControlList[i].commonMappings.Add(_controls[i].commonMappings[k]);
				}

				returnControlList[i].inputs = new List<DeviceInput>();
				for (int k = 0; k < _controls[i].inputs.Count; k++) {
					returnControlList[i].inputs.Add(_controls[i].inputs[k]);
				}
			}

			return returnControlList;
		}
		//set { Init(); _controls = value; }
	}

	private static SmartControl[] _smartControls;
	public static SmartControl[] smartControls {
		get { Init(); return _smartControls; }
	}

	//gamepads list is checked every GetButton/GetAxis call, when it updates all common mapped inputs are reapplied appropriately
	static int nextGamepadCheck=-99;
	private static string[] _gamepads = new string[0];
	/// <summary>
	/// List of connected gamepads that Sinput is aware of.
	/// </summary>
	public static string[] gamepads { get { CheckGamepads(); return _gamepads; } }
	/// <summary>
	/// Number of connected gamepads that Sinput is aware of.
	/// </summary>
	public static int connectedGamepads = 0;

	//init
	private static bool initialised = false;
	static void Init(){
		//Debug.Log("init!");
		if (initialised) return;
		initialised = true;

		_totalPossibleDeviceSlots = System.Enum.GetValues(typeof(InputDeviceSlot)).Length;

		zeroInputWaits = new float[_totalPossibleDeviceSlots];
		zeroInputs = new bool[_totalPossibleDeviceSlots];
	}

	//public static ControlScheme controlScheme;
	private static bool schemeLoaded = false;
	/// <summary>
	/// Load a Control Scheme asset.
	/// </summary>
	/// <param name="schemeName"></param>
	/// <param name="loadCustomControls"></param>
	public static void LoadControlScheme(string schemeName, bool loadCustomControls) {
		schemeLoaded = false;
		//Debug.Log("load scheme name!");
		Init();
		UnityEngine.Object[] projectControlSchemes = Resources.LoadAll("", typeof(ControlScheme));

		int schemeIndex = -1;
		for (int i=0; i<projectControlSchemes.Length; i++){
			if (projectControlSchemes[i].name == schemeName) schemeIndex = i;
		}
		if (schemeIndex==-1){
			Debug.LogError("Couldn't find control scheme \"" + schemeName + "\" in project resources.");
			return;
		}
		//controlScheme = (ControlScheme)projectControlSchemes[schemeIndex];
		LoadControlScheme((ControlScheme)projectControlSchemes[schemeIndex], loadCustomControls);
	}
	/// <summary>
	/// Load a Control Scheme.
	/// </summary>
	/// <param name="scheme"></param>
	/// <param name="loadCustomControls"></param>
	public static void LoadControlScheme(ControlScheme scheme, bool loadCustomControls) {
		//Debug.Log("load scheme asset!");

		schemeLoaded = false;


		Init();

		//make sure we know what gamepads are connected
		//and load their common mappings if they are needed
		CheckGamepads(true);

		//Generate controls from controlScheme asset
		List<Control> loadedControls = new List<Control>();
		for (int i=0; i<scheme.controls.Count; i++){
			Control newControl = new Control(scheme.controls[i].name);

			for (int k=0; k<scheme.controls[i].keyboardInputs.Count; k++){
				newControl.AddKeyboardInput( (KeyCode)Enum.Parse(typeof(KeyCode), scheme.controls[i].keyboardInputs[k].ToString()) );
			}
			for (int k=0; k<scheme.controls[i].gamepadInputs.Count; k++){
				newControl.AddGamepadInput( scheme.controls[i].gamepadInputs[k] );
			}
			for (int k=0; k<scheme.controls[i].mouseInputs.Count; k++){
				newControl.AddMouseInput( scheme.controls[i].mouseInputs[k] );
			}
			for (int k=0; k<scheme.controls[i].virtualInputs.Count; k++){
				newControl.AddVirtualInput( scheme.controls[i].virtualInputs[k] );
			}

			loadedControls.Add(newControl);
		}
		_controls = loadedControls.ToArray();

		//Generate smartControls from controlScheme asset
		List<SmartControl> loadedSmartControls = new List<SmartControl>();
		for (int i=0; i<scheme.smartControls.Count; i++){
			SmartControl newControl = new SmartControl(scheme.smartControls[i].name);

			newControl.positiveControl = scheme.smartControls[i].positiveControl;
			newControl.negativeControl = scheme.smartControls[i].negativeControl;
			newControl.gravity = scheme.smartControls[i].gravity;
			newControl.deadzone = scheme.smartControls[i].deadzone;
			newControl.speed = scheme.smartControls[i].speed;
			newControl.snap = scheme.smartControls[i].snap;
			//newControl.scale = scheme.smartControls[i].scale;

			newControl.inversion = new bool[_totalPossibleDeviceSlots];
			newControl.scales = new float[_totalPossibleDeviceSlots];
			for (int k = 0; k < _totalPossibleDeviceSlots; k++) {
				newControl.inversion[k] = scheme.smartControls[i].invert;
				newControl.scales[k] = scheme.smartControls[i].scale;
			}

			loadedSmartControls.Add(newControl);
		}
		_smartControls = loadedSmartControls.ToArray();
		for (int i=0; i<_smartControls.Length; i++) _smartControls[i].Init();

		//now load any saved control scheme with custom rebound inputs
		if (loadCustomControls && SinputFileIO.SaveDataExists(controlSchemeName)){
			//Debug.Log("Found saved binding!");
			_controls = SinputFileIO.LoadControls( _controls, controlSchemeName);
		}

		//make sure controls have any gamepad-relevant stuff set correctly
		RefreshGamepadControls();

		schemeLoaded = true;
		lastUpdateFrame = -99;
	}

	static int lastUpdateFrame = -99;
	/// <summary>
	/// Update Sinput.
	/// <para>This is called by all other Sinput functions so it is not necessary for you to call it in most circumstances.</para>
	/// </summary>
	public static void SinputUpdate() {
		if (lastUpdateFrame == Time.frameCount) return;

		lastUpdateFrame = Time.frameCount;

		//make sure everything is set up
		Init();

		if (!schemeLoaded) LoadControlScheme("MainControlScheme", true);

		//check if connected gamepads have changed
		CheckGamepads();

		//update controls
		if (null != _controls) {
			for (int i = 0; i < _controls.Length; i++) {
				_controls[i].Update();//resetAxisButtonStates);
			}
		}

		//update our smart controls
		if (null != _smartControls) {
			for (int i = 0; i < _smartControls.Length; i++) {
				_smartControls[i].Update();
			}
		}

		//count down till we can stop zeroing inputs
		for (int i = 0; i < _totalPossibleDeviceSlots; i++) {
			if (zeroInputs[i]) {
				zeroInputWaits[i] -= Time.deltaTime;
				if (zeroInputWaits[i] <= 0f) zeroInputs[i] = false;
			}
		}
	}


	//tells sinput to return false/0f for any input checks until the wait time has passed
	static float[] zeroInputWaits;
	static bool[] zeroInputs;
	/// <summary>
	/// tells Sinput to return false/0f for any input checks until half a second has passed
	/// </summary>
	/// <param name="slot"></param>
	public static void ResetInputs(InputDeviceSlot slot = InputDeviceSlot.any) { ResetInputs(0.5f, slot); } //default wait is half a second
	/// <summary>
	/// tells Sinput to return false/0f for any input checks until the wait time has passed
	/// </summary>
	/// <param name="waitTime"></param>
	/// <param name="slot"></param>
	public static void ResetInputs(float waitTime, InputDeviceSlot slot=InputDeviceSlot.any) {
		SinputUpdate();
		
		if (slot == InputDeviceSlot.any) {
			//reset all slots' input
			for (int i=0; i<_totalPossibleDeviceSlots; i++) {
				zeroInputWaits[i] = waitTime;
				zeroInputs[i] = true;
			}
		} else {
			//reset only a specific slot's input
			zeroInputWaits[(int)slot] = waitTime;
			zeroInputs[(int)slot] = true;
		}
		
		//reset smartControl values
		if (_smartControls != null) {
			for (int i = 0; i < _smartControls.Length; i++) {
				_smartControls[i].ResetAllValues(slot);
			}
		}
	}
    

	//update gamepads
	static int lastCheckedGamepadRefreshFrame = -99;
	static string[] tempInputGamepads;//more optimal to have it defined here than inside the CheckGamepads function
	/// <summary>
	/// Checks whether connected gamepads have changed.
	/// <para>This is called before every input check so it is uneccesary for you to use it.</para>
	/// </summary>
	/// <param name="refreshGamepadsNow"></param>
	public static void CheckGamepads(bool refreshGamepadsNow = false){
		if (Time.frameCount == lastCheckedGamepadRefreshFrame && !refreshGamepadsNow) return;
		lastCheckedGamepadRefreshFrame = Time.frameCount;

		//Debug.Log("checking gamepads");

		Init();

		tempInputGamepads = Input.GetJoystickNames();
		if (connectedGamepads != tempInputGamepads.Length) refreshGamepadsNow = true; //number of connected gamepads has changed
		if (!refreshGamepadsNow && nextGamepadCheck < Time.frameCount){
			//this check is for the rare case gamepads get re-ordered in a single frame & the length of GetJoystickNames() stays the same
			nextGamepadCheck = Time.frameCount + 500;
			for (int i=0; i<connectedGamepads; i++){
				if (_gamepads[i] != tempInputGamepads[i].ToUpper()) refreshGamepadsNow = true;
			}
		}
		if (refreshGamepadsNow){
			//Debug.Log("Refreshing gamepads");

			//connected gamepads have changed, lets update them
			_gamepads = new string[tempInputGamepads.Length];
			for (int i=0; i<_gamepads.Length; i++){
				_gamepads[i] = tempInputGamepads[i].ToUpper();
			}
			connectedGamepads = _gamepads.Length;

			//reload common mapping information for any new gamepads
			CommonGamepadMappings.ReloadCommonMaps();
			
			//refresh control information relating to gamepads
			if (schemeLoaded) RefreshGamepadControls();

			refreshGamepadsNow = false;
		}
	}

	private static void RefreshGamepadControls() {
		//if (null != _controls) {
			for (int i = 0; i < _controls.Length; i++) {
				//reapply common bindings
				_controls[i].ReapplyCommonBindings();

				//reset axis button states
				//_controls[i].ResetAxisButtonStates();

				//make sure inputs are linked to correct gamepad slots
				_controls[i].SetAllowedInputSlots();
			}
		//}
		//if (null != smartControls) {
			for (int i = 0; i < _smartControls.Length; i++) {
				_smartControls[i].Init();
			}
		//}
	}



	/// <summary>
	/// like GetButtonDown() but returns ~which~ keyboard/gamepad input slot pressed the control
	/// <para>will return InputDeviceSlot.any if no device pressed the button this frame</para>
	/// <para>use it for 'Pres A to join!' type multiplayer, and instantiate a player for the returned slot (if it isn't DeviceSlot.any)</para>
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static InputDeviceSlot GetSlotPress(string controlName){
		//like GetButtonDown() but returns ~which~ keyboard/gamepad input slot pressed the control
		//use it for 'Pres A to join!' type multiplayer, and instantiate a player for the returned slot (if it isn't DeviceSlot.any)

		SinputUpdate();

		if (keyboardAndMouseAreDistinct){
			if (GetButtonDown(controlName, InputDeviceSlot.keyboard)) return InputDeviceSlot.keyboard;
			if (GetButtonDown(controlName, InputDeviceSlot.mouse)) return InputDeviceSlot.mouse;
		}else{
			if (GetButtonDown(controlName, InputDeviceSlot.keyboardAndMouse)) return InputDeviceSlot.keyboardAndMouse;
			if (!zeroInputs[(int)InputDeviceSlot.keyboardAndMouse] && GetButtonDown(controlName, InputDeviceSlot.keyboard)) return InputDeviceSlot.keyboardAndMouse;
			if (!zeroInputs[(int)InputDeviceSlot.keyboardAndMouse] && GetButtonDown(controlName, InputDeviceSlot.mouse)) return InputDeviceSlot.keyboardAndMouse;
		}
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad1)) return InputDeviceSlot.gamepad1;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad2)) return InputDeviceSlot.gamepad2;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad3)) return InputDeviceSlot.gamepad3;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad4)) return InputDeviceSlot.gamepad4;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad5)) return InputDeviceSlot.gamepad5;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad6)) return InputDeviceSlot.gamepad6;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad7)) return InputDeviceSlot.gamepad7;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad7)) return InputDeviceSlot.gamepad7;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad9)) return InputDeviceSlot.gamepad9;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad10)) return InputDeviceSlot.gamepad10;
		if (GetButtonDown(controlName, InputDeviceSlot.gamepad11)) return InputDeviceSlot.gamepad11;
		
		if (GetButtonDown(controlName, InputDeviceSlot.virtual1)) return InputDeviceSlot.virtual1;

		return InputDeviceSlot.any;
	}


	//Button control checks
	private static bool controlFound = false;//used in control checks to tell whether a control was found or not
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control is Held this frame
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool GetButton(string controlName){ return ButtonCheck(controlName, InputDeviceSlot.any, ButtonAction.HELD); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control is Held this frame
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static bool GetButton(string controlName, InputDeviceSlot slot){ return ButtonCheck(controlName, slot, ButtonAction.HELD); }

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool GetButtonDown(string controlName){ return ButtonCheck(controlName, InputDeviceSlot.any, ButtonAction.DOWN); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static bool GetButtonDown(string controlName, InputDeviceSlot slot){ return ButtonCheck(controlName, slot, ButtonAction.DOWN); }

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Released this frame
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool GetButtonUp(string controlName){ return ButtonCheck(controlName, InputDeviceSlot.any, ButtonAction.UP); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Released this frame
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static bool GetButtonUp(string controlName, InputDeviceSlot slot){ return ButtonCheck(controlName, slot, ButtonAction.UP); }

	/// <summary>
	/// Returns true if a Sinput Control or Smart Control is Held this frame, regardless of the Control's toggle setting.
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool GetButtonRaw(string controlName) { return ButtonCheck(controlName, InputDeviceSlot.any, ButtonAction.HELD, true); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control is Held this frame, regardless of the Control's toggle setting.
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static bool GetButtonRaw(string controlName, InputDeviceSlot slot) { return ButtonCheck(controlName, slot, ButtonAction.HELD, true); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame, regardless of the Control's toggle setting.
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool GetButtonDownRaw(string controlName) { return ButtonCheck(controlName, InputDeviceSlot.any, ButtonAction.DOWN, true); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame, regardless of the Control's toggle setting.
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static bool GetButtonDownRaw(string controlName, InputDeviceSlot slot) { return ButtonCheck(controlName, slot, ButtonAction.DOWN, true); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Released this frame, regardless of the Control's toggle setting.
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool GetButtonUpRaw(string controlName) { return ButtonCheck(controlName, InputDeviceSlot.any, ButtonAction.UP, true); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Released this frame, regardless of the Control's toggle setting.
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static bool GetButtonUpRaw(string controlName, InputDeviceSlot slot) { return ButtonCheck(controlName, slot, ButtonAction.UP, true); }

	//repeating button checks
	/// <summary>
	/// How long a Control must be held before GetButtonDownRepeating() starts repeating
	/// </summary>
	public static float buttonRepeatWait = 0.75f;
	/// <summary>
	/// How quickly GetButtonDownRepeating() will repeat.
	/// </summary>
	public static float buttonRepeat = 0.1f;
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame, or if it has been held long enough to start repeating.
	/// <para>Use this for menu scrolling inputs</para>
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool GetButtonDownRepeating(string controlName) { return ButtonCheck(controlName, InputDeviceSlot.any, ButtonAction.REPEATING); }
	/// <summary>
	/// Returns true if a Sinput Control or Smart Control was Pressed this frame, or if it has been held long enough to start repeating.
	/// <para>Use this for menu scrolling inputs</para>
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static bool GetButtonDownRepeating(string controlName, InputDeviceSlot slot) { return ButtonCheck(controlName, slot, ButtonAction.REPEATING); }

	static bool ButtonCheck(string controlName, InputDeviceSlot slot, ButtonAction bAction, bool getRawValue = false){
		
		SinputUpdate();
		if (zeroInputs[(int)slot]) return false;

		controlFound = false;

		for (int i=0; i<_controls.Length; i++){
			if (_controls[i].name == controlName){
				controlFound=true;
				if (_controls[i].GetButtonState(bAction, slot, getRawValue)) return true;
			}
		}

		for (int i=0; i<_smartControls.Length; i++){
			if (_smartControls[i].name == controlName){
				controlFound=true;
				if (_smartControls[i].ButtonCheck(bAction, slot)) return true;
			}
		}

		if (!controlFound) Debug.LogError("Sinput Error: Control \"" + controlName + "\" not found in list of controls or SmartControls.");

		return false;
	}


	//Axis control checks
	private static float returnV; //used in axis checks to hold the return value
	private static float v;

	/// <summary>
	/// Returns the value of a Sinput Control or Smart Control.
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns>float value</returns>
	public static float GetAxis(string controlName) { return AxisCheck(controlName, InputDeviceSlot.any); }
	/// <summary>
	/// Returns the value of a Sinput Control or Smart Control.
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static float GetAxis(string controlName, InputDeviceSlot slot ) { return AxisCheck(controlName, slot); }


	/// <summary>
	/// Returns the raw value of a Sinput Control or Smart Control
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static float GetAxisRaw(string controlName) { return AxisCheck(controlName, InputDeviceSlot.any, true); }
	/// <summary>
	/// Returns the raw value of a Sinput Control or Smart Control
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static float GetAxisRaw(string controlName, InputDeviceSlot slot) { return AxisCheck(controlName, slot, true); }

	static float AxisCheck(string controlName, InputDeviceSlot slot, bool getRawValue=false){

		SinputUpdate();
		if (zeroInputs[(int)slot]) return 0f;

		controlFound = false;

		if (controlName=="") return 0f;

		returnV = 0f;
		for (int i=0; i<_controls.Length; i++){
			if (_controls[i].name == controlName){
				controlFound=true;
				v = _controls[i].GetAxisState(slot);
				if (Mathf.Abs(v) > returnV) returnV = v;
			}
		}

		for (int i=0; i<_smartControls.Length; i++){
			if (_smartControls[i].name == controlName){
				controlFound=true;
				v = _smartControls[i].GetValue(slot, getRawValue);
				if (Mathf.Abs(v) > returnV) returnV = v;
			}
		}

		if (!controlFound) Debug.LogError("Sinput Error: Control \"" + controlName + "\" not found in list of Controls or SmartControls.");

		return returnV;
	}

	//vector checks
	private static Vector2 returnVec2;
	/// <summary>
	/// Returns a Vector2 made with GetAxis() values applied to x and y
	/// </summary>
	/// <param name="controlNameA"></param>
	/// <param name="controlNameB"></param>
	/// <returns></returns>
	public static Vector2 GetVector(string controlNameA, string controlNameB) { return Vector2Check(controlNameA, controlNameB, InputDeviceSlot.any, true); }
	/// <summary>
	/// Returns a Vector2 made with GetAxis() values applied to x and y
	/// </summary>
	/// <param name="controlNameA"></param>
	/// <param name="controlNameB"></param>
	/// <returns></returns>
	public static Vector2 GetVector(string controlNameA, string controlNameB, bool normalClip) { return Vector2Check(controlNameA, controlNameB, InputDeviceSlot.any, normalClip); }
	/// <summary>
	/// Returns a Vector2 made with GetAxis() values applied to x and y
	/// </summary>
	/// <param name="controlNameA"></param>
	/// <param name="controlNameB"></param>
	/// <returns></returns>
	public static Vector2 GetVector(string controlNameA, string controlNameB, InputDeviceSlot slot) { return Vector2Check(controlNameA, controlNameB, slot, true); }
	/// <summary>
	/// Returns a Vector2 made with GetAxis() values applied to x and y
	/// </summary>
	/// <param name="controlNameA"></param>
	/// <param name="controlNameB"></param>
	/// <returns></returns>
	public static Vector2 GetVector(string controlNameA, string controlNameB, InputDeviceSlot slot, bool normalClip) { return Vector2Check(controlNameA, controlNameB, slot, normalClip); }

	static Vector2 Vector2Check(string controlNameA, string controlNameB, InputDeviceSlot slot, bool normalClip){

		SinputUpdate();

		returnVec2 = Vector2.zero;
		returnVec2.x = AxisCheck(controlNameA, slot);
		returnVec2.y = AxisCheck(controlNameB, slot);

		if (normalClip && returnVec2.magnitude > 1f) {
			returnVec2.Normalize();
		}

		return returnVec2;
	}

	private static Vector3 returnVec3;
	/// <summary>
	/// Returns a Vector3 made with GetAxis() values applied to x, y, and z
	/// </summary>
	/// <param name="controlNameA"></param>
	/// <param name="controlNameB"></param>
	/// <returns></returns>
	public static Vector3 GetVector(string controlNameA, string controlNameB, string controlNameC) { return Vector3Check(controlNameA, controlNameB, controlNameC, InputDeviceSlot.any, true); }
	/// <summary>
	/// Returns a Vector3 made with GetAxis() values applied to x, y, and z
	/// </summary>
	/// <param name="controlNameA"></param>
	/// <param name="controlNameB"></param>
	/// <returns></returns>
	public static Vector3 GetVector(string controlNameA, string controlNameB, string controlNameC, bool normalClip) { return Vector3Check(controlNameA, controlNameB, controlNameC, InputDeviceSlot.any, normalClip); }
	/// <summary>
	/// Returns a Vector3 made with GetAxis() values applied to x, y, and z
	/// </summary>
	/// <param name="controlNameA"></param>
	/// <param name="controlNameB"></param>
	/// <returns></returns>
	public static Vector3 GetVector(string controlNameA, string controlNameB, string controlNameC, InputDeviceSlot slot) { return Vector3Check(controlNameA, controlNameB, controlNameC, slot, true); }
	/// <summary>
	/// Returns a Vector3 made with GetAxis() values applied to x, y, and z
	/// </summary>
	/// <param name="controlNameA"></param>
	/// <param name="controlNameB"></param>
	/// <returns></returns>
	public static Vector3 GetVector(string controlNameA, string controlNameB, string controlNameC, InputDeviceSlot slot, bool normalClip) { return Vector3Check(controlNameA, controlNameB, controlNameC, slot, normalClip); }

	static Vector3 Vector3Check(string controlNameA, string controlNameB, string controlNameC, InputDeviceSlot slot, bool normalClip){

		SinputUpdate();

		returnVec3 = Vector3.zero;
		returnVec3.x = AxisCheck(controlNameA, slot);
		returnVec3.y = AxisCheck(controlNameB, slot);
		returnVec3.z = AxisCheck(controlNameC, slot);

		if (normalClip && returnVec3.magnitude > 1f) {
			returnVec3.Normalize();
		}

		return returnVec3;
	}
	
	//frame delta preference
	private static bool preferDelta;
	/// <summary>
	/// Returns false if the value returned by GetAxis(controlName) on this frame should NOT be multiplied by delta time.
	/// <para>For example, this will return true for gamepad stick values, false for mouse movement values</para>
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool PrefersDeltaUse(string controlName) { return PrefersDeltaUse(controlName, InputDeviceSlot.any); }
	/// <summary>
	/// Returns false if the value returned by GetAxis(controlName, slot) on this frame should NOT be multiplied by delta time.
	/// <para>For example, this will return true for gamepad stick values, false for mouse movement values</para>
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool PrefersDeltaUse(string controlName, InputDeviceSlot slot) {

		SinputUpdate();

		preferDelta = true;

		controlFound = false;

		if (controlName == "") return false;
		returnV = 0f;
		for (int i = 0; i < _controls.Length; i++) {
			if (_controls[i].name == controlName) {
				controlFound = true;
				v = _controls[i].GetAxisState(slot);
				if (Mathf.Abs(v) > returnV) {
					returnV = v;
					preferDelta = _controls[i].GetAxisStateDeltaPreference(slot);
				}
			}
		}

		//now check smart controls for framerate independence
		for (int i = 0; i < _smartControls.Length; i++) {
			if (_smartControls[i].name == controlName) {
				controlFound = true;
				v = _smartControls[i].GetValue(slot, true);
				if (Mathf.Abs(v) > returnV) {
					returnV = v;

					if (!PrefersDeltaUse(_smartControls[i].positiveControl, slot) || !PrefersDeltaUse(_smartControls[i].negativeControl, slot)) preferDelta = false;
				}
			}
		}

		if (!controlFound) Debug.LogError("Sinput Error: Control \"" + controlName + "\" not found in list of Controls or SmartControls.");

		return preferDelta;
	}

	
	/// <summary>
	/// sets whether a control treats GetButton() calls with press or with toggle behaviour
	/// </summary>
	/// <param name="controlName"></param>
	/// <param name="toggle"></param>
	public static void SetToggle(string controlName, bool toggle) {
		SinputUpdate();
		controlFound = false;
		for (int i = 0; i < _controls.Length; i++) {
			if (_controls[i].name == controlName) {
				controlFound = true;
				_controls[i].isToggle = toggle;
			}
		}
		if (!controlFound) Debug.LogError("Sinput Error: Control \"" + controlName + "\" not found in list of Controls or SmartControls.");
	}
	
	/// <summary>
	/// returns true if a control treats GetButton() calls with toggle behaviour
	/// </summary>
	/// <param name="controlName"></param>
	/// <returns></returns>
	public static bool GetToggle(string controlName) {
		SinputUpdate();
		for (int i = 0; i < _controls.Length; i++) {
			if (_controls[i].name == controlName) {
				return _controls[i].isToggle;
			}
		}
		Debug.LogError("Sinput Error: Control \"" + controlName + "\" not found in list of Controls or SmartControls.");
		return false;
	}

	
	/// <summary>
	/// set a smart control to be inverted or not
	/// </summary>
	/// <param name="smartControlName"></param>
	/// <param name="invert"></param>
	/// <param name="slot"></param>
	public static void SetInverted(string smartControlName, bool invert, InputDeviceSlot slot=InputDeviceSlot.any) {
		SinputUpdate();
		controlFound = false;
		for (int i = 0; i < _smartControls.Length; i++) {
			if (_smartControls[i].name == smartControlName) {
				controlFound = true;
				if (slot == InputDeviceSlot.any) {
					for (int k=0; k<_totalPossibleDeviceSlots; k++) {
						_smartControls[i].inversion[k] = invert;
					}
				} else {
					_smartControls[i].inversion[(int)slot] = invert;
				}
			}
		}
		if (!controlFound) Debug.LogError("Sinput Error: Smart Control \"" + smartControlName + "\" not found in list of SmartControls.");
	}
	
	/// <summary>
	/// returns true if a smart control is inverted
	/// </summary>
	/// <param name="smartControlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static bool GetInverted(string smartControlName, InputDeviceSlot slot = InputDeviceSlot.any) {
		SinputUpdate();
		for (int i = 0; i < _smartControls.Length; i++) {
			if (_smartControls[i].name == smartControlName) {
				return _smartControls[i].inversion[(int)slot];
			}
		}
		Debug.LogError("Sinput Error: Smart Control \"" + smartControlName + "\" not found in list of SmartControls.");
		return false;
	}

	
	/// <summary>
	/// sets scale ("sensitivity") of a smart control
	/// </summary>
	/// <param name="smartControlName"></param>
	/// <param name="scale"></param>
	/// <param name="slot"></param>
	public static void SetScale(string smartControlName, float scale, InputDeviceSlot slot = InputDeviceSlot.any) {
		SinputUpdate();
		controlFound = false;
		for (int i = 0; i < _smartControls.Length; i++) {
			if (_smartControls[i].name == smartControlName) {
				controlFound = true;
				if (slot == InputDeviceSlot.any) {
					for (int k = 0; k < _totalPossibleDeviceSlots; k++) {
						_smartControls[i].scales[k] = scale;
					}
				} else {
					_smartControls[i].scales[(int)slot] = scale;
				}
			}
		}
		if (!controlFound) Debug.LogError("Sinput Error: Smart Control \"" + smartControlName + "\" not found in list of SmartControls.");
	}
	
	/// <summary>
	/// gets scale of a smart control
	/// </summary>
	/// <param name="smartControlName"></param>
	/// <param name="slot"></param>
	/// <returns></returns>
	public static float GetScale(string smartControlName, InputDeviceSlot slot = InputDeviceSlot.any) {
		for (int i = 0; i < _smartControls.Length; i++) {
			if (_smartControls[i].name == smartControlName) {
				return _smartControls[i].scales[(int)slot];
			}
		}
		Debug.LogError("Sinput Error: Smart Control \"" + smartControlName + "\" not found in list of SmartControls.");
		return 1f;
	}

}


