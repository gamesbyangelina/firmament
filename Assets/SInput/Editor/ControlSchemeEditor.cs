using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using SinputSystems;

[CustomEditor(typeof(ControlScheme))]
public class ControlSchemeEditor : Editor {

	int currentPanel = 0;

	//List<bool> controlFoldouts = new List<bool>();
	List<UnityEditor.AnimatedValues.AnimBool> controlFoldoutAnims = new List<UnityEditor.AnimatedValues.AnimBool>();
	List<UnityEditor.AnimatedValues.AnimBool> smartControlFoldouts = new List<UnityEditor.AnimatedValues.AnimBool>();

	//GUIStyle guiStyle;
	GUIStyle guiBGStyleeee;

	private string[] controlNames;
	private List<int> positiveControlIndices = new List<int>();
	private List<int> negativeControlIndices = new List<int>();
	private bool controlNamesChanged = true;

	string activeControlName = "";

	private Texture2D MakeTex(int width, int height, Color col) {
		Color[] pix = new Color[width * height];
		for (int i = 0; i < pix.Length; ++i) {
			pix[i] = col;
		}
		Texture2D result = new Texture2D(width, height);
		result.SetPixels(pix);
		result.Apply();
		return result;
	}

	public override void OnInspectorGUI(){

		ControlScheme controlScheme = (ControlScheme)target;
		EditorGUI.BeginChangeCheck();

		if (null == guiBGStyleeee) {
			guiBGStyleeee = new GUIStyle();
			guiBGStyleeee.normal.background = MakeTex(1, 1, new Color(0.0f, 0.0f, 0.0f, 0.07f));
		}
		//if (null == guiStyle) {
		//	guiBGStyle = new GUIStyle();
		//}

		string[] strs = new string[] { "Controls", "Smart Controls" };//,"Settings"};
		currentPanel = GUILayout.Toolbar(currentPanel, strs);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

		if (currentPanel==0){
			//show controls list

			if (controlFoldoutAnims.Count != controlScheme.controls.Count) {
				//controlFoldouts = new List<bool>();
				controlFoldoutAnims = new List<UnityEditor.AnimatedValues.AnimBool>();
				for (int i = 0; i < controlScheme.controls.Count; i++) {
					//controlFoldouts.Add(false);
					controlFoldoutAnims.Add(new UnityEditor.AnimatedValues.AnimBool(false));
					controlFoldoutAnims[controlFoldoutAnims.Count - 1].valueChanged.AddListener(Repaint);
				}
			}

			ControlScheme.ControlSetup activeControl = new ControlScheme.ControlSetup();
			for (int i=0; i<controlScheme.controls.Count; i++){
				bool deleteControl = false;
				bool moveUp = false;
				bool moveDown = false;

				EditorGUILayout.BeginHorizontal();
				controlFoldoutAnims[i].target = EditorGUILayout.Foldout(controlFoldoutAnims[i].target,controlScheme.controls[i].name, true);
				//EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
				if (GUILayout.Button("↑", EditorStyles.miniButton, GUILayout.Width(20))) moveUp = true;
				if (GUILayout.Button("↓", EditorStyles.miniButton, GUILayout.Width(20))) moveDown = true;
				if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30))) deleteControl = true;
				//EditorGUILayout.EndHorizontal();
				EditorGUILayout.EndHorizontal();
				

				if (EditorGUILayout.BeginFadeGroup(controlFoldoutAnims[i].faded)) {
					EditorGUILayout.BeginVertical(guiBGStyleeee);
					EditorGUILayout.Space();
					//guiCol = GUI.backgroundColor;
					//GUI.backgroundColor = Color.black;

					//if (controlFoldouts[i]){
					activeControl = controlScheme.controls[i];

					//EditorGUILayout.BeginHorizontal();


					activeControlName = activeControl.name;
					activeControl.name = EditorGUILayout.TextField("Control Name",activeControl.name);
					activeControl.name = SinputSystems.SinputFileIO.SanitiseStringForSaving(activeControl.name);
					if (activeControlName != activeControl.name) {
						//control name changed, lets apply this change to any smart controls that reference this control
						for (int k=0; k<controlScheme.smartControls.Count; k++) {
							if (controlScheme.smartControls[k].positiveControl == activeControlName) {
								ControlScheme.SmartControlSetup activeSmartControl = controlScheme.smartControls[k];
								activeSmartControl.positiveControl = activeControl.name;
								controlScheme.smartControls[k] = activeSmartControl;
							}
							if (controlScheme.smartControls[k].negativeControl == activeControlName) {
								ControlScheme.SmartControlSetup activeSmartControl = controlScheme.smartControls[k];
								activeSmartControl.negativeControl = activeControl.name;
								controlScheme.smartControls[k] = activeSmartControl;
							}
						}
					}
					//if (GUILayout.Button("↑", EditorStyles.miniButton)) moveUp = true;
					//if (GUILayout.Button("↓", EditorStyles.miniButton)) moveDown = true;
					//if (GUILayout.Button("X", EditorStyles.miniButton)) deleteControl = true;
					//EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();

					//keyboard inputs
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();
					//column a
					EditorGUILayout.LabelField("Keyboard Inputs:");

					EditorGUILayout.EndVertical();
					EditorGUILayout.BeginVertical();
					//column b
					for (int k=0; k<activeControl.keyboardInputs.Count; k++){
						EditorGUILayout.BeginHorizontal();

						activeControl.keyboardInputs[k] = (KeyboardInputType)EditorGUILayout.EnumPopup(activeControl.keyboardInputs[k]);
						if (GUILayout.Button("x", EditorStyles.miniButton)){
							activeControl.keyboardInputs.RemoveAt(k);
							k--;
						}
						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("+", GUILayout.Width(40))){
						KeyboardInputType newKeycode = KeyboardInputType.None;
						activeControl.keyboardInputs.Add(newKeycode);
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();


					//gamepad inputs
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();
					//column a
					EditorGUILayout.LabelField("Gamepad Inputs:");

					EditorGUILayout.EndVertical();
					EditorGUILayout.BeginVertical();
					//column b
					for (int k=0; k<activeControl.gamepadInputs.Count; k++){
						EditorGUILayout.BeginHorizontal();

						activeControl.gamepadInputs[k] = (CommonGamepadInputs)EditorGUILayout.EnumPopup(activeControl.gamepadInputs[k]);
						if (GUILayout.Button("x", EditorStyles.miniButton)){
							activeControl.gamepadInputs.RemoveAt(k);
							k--;
						}
						EditorGUILayout.EndHorizontal();

					}
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("+", GUILayout.Width(40))){
						CommonGamepadInputs newGamepadInput = CommonGamepadInputs.NOBUTTON;
						activeControl.gamepadInputs.Add(newGamepadInput);
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();


					//mouse inputs
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();
					//column a
					EditorGUILayout.LabelField("Mouse Inputs:");

					EditorGUILayout.EndVertical();
					EditorGUILayout.BeginVertical();
					//column b
					for (int k=0; k<activeControl.mouseInputs.Count; k++){
						EditorGUILayout.BeginHorizontal();

						activeControl.mouseInputs[k] = (MouseInputType)EditorGUILayout.EnumPopup(activeControl.mouseInputs[k]);
						if (GUILayout.Button("x", EditorStyles.miniButton)){
							activeControl.mouseInputs.RemoveAt(k);
							k--;
						}
						EditorGUILayout.EndHorizontal();

					}
					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("+", GUILayout.Width(40))){
						activeControl.mouseInputs.Add(MouseInputType.Mouse0);
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();


					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.Space();

					//virtual inputs
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.BeginVertical();
					//column a
					EditorGUILayout.LabelField("Virtual Inputs:");

					EditorGUILayout.EndVertical();
					EditorGUILayout.BeginVertical();
					//column b
					for (int k=0; k<activeControl.virtualInputs.Count; k++){
						EditorGUILayout.BeginHorizontal();
						activeControl.virtualInputs[k] = EditorGUILayout.TextField(activeControl.virtualInputs[k]);
						activeControl.virtualInputs[k] = SinputSystems.SinputFileIO.SanitiseStringForSaving(activeControl.virtualInputs[k]);
						if (GUILayout.Button("x", EditorStyles.miniButton)){
							activeControl.virtualInputs.RemoveAt(k);
							k--;
						}
						EditorGUILayout.EndHorizontal();
					}

					EditorGUILayout.BeginHorizontal();
					if (GUILayout.Button("+", GUILayout.Width(40))){
						activeControl.virtualInputs.Add("");
					}
					GUILayout.FlexibleSpace();
					EditorGUILayout.EndHorizontal();

					EditorGUILayout.EndVertical();
					EditorGUILayout.EndHorizontal();


					controlScheme.controls[i] = activeControl;

					EditorGUILayout.Space();
					//EditorGUILayout.Space();
					//EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndFadeGroup();

				if (moveUp && i>0) {
					controlScheme.controls.Insert(i - 1, controlScheme.controls[i]);
					controlScheme.controls.RemoveAt(i + 1);

					controlFoldoutAnims.Insert(i - 1, controlFoldoutAnims[i]);
					controlFoldoutAnims.RemoveAt(i + 1);
				}
				if (moveDown && i< controlScheme.controls.Count-1) {
					controlScheme.controls.Insert(i + 2, controlScheme.controls[i]);
					controlScheme.controls.RemoveAt(i);

					controlFoldoutAnims.Insert(i + 2, controlFoldoutAnims[i]);
					controlFoldoutAnims.RemoveAt(i);
				}

				if (deleteControl){
					controlScheme.controls.RemoveAt(i);
					//controlFoldouts.RemoveAt(i);
					controlFoldoutAnims.RemoveAt(i);
					//controlKeyboardInputNames.RemoveAt(i);
					i--;
				}


			}
			if (GUILayout.Button("+")){
				ControlScheme.ControlSetup newControl = new ControlScheme.ControlSetup();
				newControl.name = "New Control";
				newControl.keyboardInputs = new List<KeyboardInputType>();
				newControl.gamepadInputs = new List<CommonGamepadInputs>();
				newControl.mouseInputs = new List<MouseInputType>();
				newControl.virtualInputs = new List<string>();
				controlScheme.controls.Add(newControl);
				//controlFoldouts.Add(true);
				controlFoldoutAnims.Add(new UnityEditor.AnimatedValues.AnimBool(false));
				controlFoldoutAnims[controlFoldoutAnims.Count - 1].target = true;
				controlFoldoutAnims[controlFoldoutAnims.Count-1].valueChanged.AddListener(Repaint);
				//controlKeyboardInputNames.Add("");
				controlNamesChanged = true;
			}
			controlNamesChanged = true;
		}

		if (currentPanel==1){
			//show smart controls list
			if (smartControlFoldouts.Count != controlScheme.smartControls.Count){
				smartControlFoldouts = new List<UnityEditor.AnimatedValues.AnimBool>();
				for (int i = 0; i < controlScheme.smartControls.Count; i++) {
					smartControlFoldouts.Add(new UnityEditor.AnimatedValues.AnimBool(false));
					smartControlFoldouts[smartControlFoldouts.Count - 1].valueChanged.AddListener(Repaint);
				}
			}
			

			if (controlNames == null || controlNamesChanged) {
				controlNamesChanged = true;

				List<string> controlNamesList = new List<string>();
				controlNamesList.Add("");
				for (int i=0; i<controlScheme.controls.Count; i++) {
					controlNamesList.Add(controlScheme.controls[i].name);
				}
				controlNames = controlNamesList.ToArray();
				
			}

			if (positiveControlIndices.Count != controlScheme.smartControls.Count || negativeControlIndices.Count != controlScheme.smartControls.Count || controlNamesChanged) {
				positiveControlIndices = new List<int>();
				negativeControlIndices = new List<int>();
				for (int i = 0; i < controlScheme.smartControls.Count; i++) {
					positiveControlIndices.Add(GetControlIndex(controlScheme.smartControls[i].positiveControl));
					negativeControlIndices.Add(GetControlIndex(controlScheme.smartControls[i].negativeControl));
				}
			}

			controlNamesChanged = false;

			int controlIndex = 0;

			ControlScheme.SmartControlSetup activeSmartControl = new ControlScheme.SmartControlSetup();
			for (int i=0; i<controlScheme.smartControls.Count; i++){
				EditorGUILayout.BeginHorizontal();
				smartControlFoldouts[i].target = EditorGUILayout.Foldout(smartControlFoldouts[i].target,controlScheme.smartControls[i].name, true);

				//EditorGUILayout.LabelField("");
				bool deleteControl = false;
				bool moveUp = false;
				bool moveDown = false;

				//EditorGUILayout.BeginHorizontal(GUILayout.Width(90));
				if (GUILayout.Button("↑", EditorStyles.miniButton, GUILayout.Width(20))) moveUp = true;
				if (GUILayout.Button("↓", EditorStyles.miniButton, GUILayout.Width(20))) moveDown = true;
				if (GUILayout.Button("X", EditorStyles.miniButton, GUILayout.Width(30))) deleteControl = true;
				//EditorGUILayout.EndHorizontal();
				//if (GUILayout.Button("X")) deleteControl = true;
				EditorGUILayout.EndHorizontal();

				activeSmartControl = controlScheme.smartControls[i];

				if (EditorGUILayout.BeginFadeGroup(smartControlFoldouts[i].faded)) {
				//if (smartControlFoldouts[i]){
					activeSmartControl.name = EditorGUILayout.TextField("Name", activeSmartControl.name);
					activeSmartControl.name = SinputSystems.SinputFileIO.SanitiseStringForSaving(activeSmartControl.name);

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Positive Control");
					controlIndex = EditorGUILayout.Popup(positiveControlIndices[i], controlNames);
					if (controlIndex != positiveControlIndices[i]) {
						positiveControlIndices[i] = controlIndex;
						activeSmartControl.positiveControl = controlNames[positiveControlIndices[i]];
					}
					EditorGUILayout.EndHorizontal();
					//activeSmartControl.positiveControl = EditorGUILayout.TextField("Positive Control", activeSmartControl.positiveControl);

					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.PrefixLabel("Negative Control");
					controlIndex = EditorGUILayout.Popup(negativeControlIndices[i], controlNames);
					if (controlIndex != negativeControlIndices[i]) {
						negativeControlIndices[i] = controlIndex;
						activeSmartControl.negativeControl = controlNames[negativeControlIndices[i]];
					}
					EditorGUILayout.EndHorizontal();
					//activeSmartControl.negativeControl = EditorGUILayout.TextField("Negative Control", activeSmartControl.negativeControl);

					activeSmartControl.deadzone = EditorGUILayout.FloatField("Deadzone", activeSmartControl.deadzone);
					activeSmartControl.deadzone = Mathf.Clamp(activeSmartControl.deadzone, 0f, 1f);

					activeSmartControl.gravity = EditorGUILayout.FloatField("Gravity", activeSmartControl.gravity);
					activeSmartControl.gravity = Mathf.Clamp(activeSmartControl.gravity, 0f, float.MaxValue);

					activeSmartControl.speed = EditorGUILayout.FloatField("Speed", activeSmartControl.speed);
					activeSmartControl.speed = Mathf.Clamp(activeSmartControl.speed, 0f, float.MaxValue);

					activeSmartControl.snap = EditorGUILayout.Toggle("Snap", activeSmartControl.snap);
					activeSmartControl.scale = EditorGUILayout.FloatField("Scale", activeSmartControl.scale);
					activeSmartControl.invert = EditorGUILayout.Toggle("Invert", activeSmartControl.invert);
					EditorGUILayout.Space();

				}
				EditorGUILayout.EndFadeGroup();

				controlScheme.smartControls[i] = activeSmartControl;
				if (deleteControl){
					controlScheme.smartControls.RemoveAt(i);
					smartControlFoldouts.RemoveAt(i);
					positiveControlIndices.RemoveAt(i);
					negativeControlIndices.RemoveAt(i);
					i--;
				}

				if (moveUp && i > 0) {
					controlScheme.smartControls.Insert(i - 1, controlScheme.smartControls[i]);
					controlScheme.smartControls.RemoveAt(i + 1);

					smartControlFoldouts.Insert(i - 1, smartControlFoldouts[i]);
					smartControlFoldouts.RemoveAt(i + 1);

					positiveControlIndices.Insert(i - 1, positiveControlIndices[i]);
					positiveControlIndices.RemoveAt(i + 1);

					negativeControlIndices.Insert(i - 1, negativeControlIndices[i]);
					negativeControlIndices.RemoveAt(i + 1);
				}
				if (moveDown && i < controlScheme.smartControls.Count - 1) {
					controlScheme.smartControls.Insert(i + 2, controlScheme.smartControls[i]);
					controlScheme.smartControls.RemoveAt(i);

					smartControlFoldouts.Insert(i + 2, smartControlFoldouts[i]);
					smartControlFoldouts.RemoveAt(i);

					positiveControlIndices.Insert(i +2, positiveControlIndices[i]);
					positiveControlIndices.RemoveAt(i);

					negativeControlIndices.Insert(i +2, negativeControlIndices[i]);
					negativeControlIndices.RemoveAt(i);
				}

			}


			if (GUILayout.Button("+")){
				ControlScheme.SmartControlSetup newSmartControl = new ControlScheme.SmartControlSetup();
				newSmartControl.name = "New Control";
				newSmartControl.positiveControl = "";
				newSmartControl.negativeControl = "";

				newSmartControl.deadzone = 0.001f;
				newSmartControl.gravity = 3f;
				newSmartControl.speed = 3f;
				newSmartControl.snap = false;
				newSmartControl.scale = 1f;

				controlScheme.smartControls.Add(newSmartControl);
				smartControlFoldouts.Add(new UnityEditor.AnimatedValues.AnimBool(false));
				smartControlFoldouts[smartControlFoldouts.Count - 1].target = true;
				smartControlFoldouts[smartControlFoldouts.Count - 1].valueChanged.AddListener(Repaint);

				positiveControlIndices.Add(0);
				negativeControlIndices.Add(0);
			}
		}

		/*if (currentPanel == 2) {
			//settings panel
			controlScheme.name = EditorGUILayout.TextField("Control Scheme Name", controlScheme.name);
			controlScheme.exposeMouseSensitivityOption = EditorGUILayout.Toggle("Expose mouse sensitivity on rebind screen", controlScheme.exposeMouseSensitivityOption);
			controlScheme.mouseAndKeyboardAreDistinct = EditorGUILayout.Toggle("Mouse & Keyboard are distinct", controlScheme.mouseAndKeyboardAreDistinct);
		}*/

		if (EditorGUI.EndChangeCheck()){
			//something was changed
			EditorUtility.SetDirty(controlScheme);
		}

		//if there are any foldouts/fades still animating, repaint
		bool repaint = false;
		for (int i = 0; i < controlFoldoutAnims.Count; i++) {
			if (controlFoldoutAnims[i].isAnimating) repaint = true;
		}
		for (int i = 0; i < smartControlFoldouts.Count; i++) {
			if (smartControlFoldouts[i].isAnimating) repaint = true;
		}
		if (repaint) Repaint();

	}

	int GetControlIndex(string s) {
		for (int i=0; i<controlNames.Length; i++) {
			if (controlNames[i] == s) return i;
		}
		return 0;
	}
}
