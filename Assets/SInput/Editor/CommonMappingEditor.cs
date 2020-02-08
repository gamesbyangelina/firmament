using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using SinputSystems;

[CustomEditor(typeof(CommonMapping))]
public class CommonMappingEditor : Editor {

	int currentPanel = 0;

	List<bool> axisEditFoldouts = new List<bool>();

	public override void OnInspectorGUI(){
		
		CommonMapping padMapping = (CommonMapping)target;
		EditorGUI.BeginChangeCheck();

		string[] strs = new string[]{"Gamepad","Buttons","Axis"};
		currentPanel = GUILayout.Toolbar(currentPanel, strs);
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


		if (currentPanel==0){
			//Gamepad general menu
			padMapping.os = (OSFamily)EditorGUILayout.EnumPopup("Operating System:", padMapping.os);
			EditorGUILayout.Space();
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			EditorGUILayout.PrefixLabel("Gamepad Names:");
			EditorGUILayout.LabelField("Which gamepads will this mapping apply to?");
			EditorGUILayout.LabelField("(Case insensitive, but needs to match what unity detects)");


			for (int i=0; i<padMapping.names.Count; i++){
				EditorGUILayout.BeginHorizontal();
				padMapping.names[i] = EditorGUILayout.TextField(padMapping.names[i]);
				if (GUILayout.Button("x")){
					//remove gamepad name
					padMapping.names.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("+")){
				//add gamepad name here
				padMapping.names.Add("GAMEPAD_NAME_HERE");
			}
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			EditorGUILayout.Space();
			EditorGUILayout.PrefixLabel("Partial Names:");
			EditorGUILayout.LabelField("If a gamepad can't find a match on the above names,");
			EditorGUILayout.LabelField("check if it contains any of the following.");


			for (int i = 0; i < padMapping.partialNames.Count; i++) {
				EditorGUILayout.BeginHorizontal();
				padMapping.partialNames[i] = EditorGUILayout.TextField(padMapping.partialNames[i]);
				if (GUILayout.Button("x")) {
					//remove gamepad name
					padMapping.partialNames.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("+")) {
				//add gamepad name here
				padMapping.partialNames.Add("PARTIAL_NAME_HERE");
			}
			EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

			EditorGUILayout.Space();
			EditorGUILayout.PrefixLabel("Default pad:");
			EditorGUILayout.LabelField("If a common or saved binding isn't found,");
			EditorGUILayout.LabelField("is this the default mapping that will be loaded?");
			bool wasDefault = padMapping.isDefault;
			padMapping.isDefault = EditorGUILayout.Toggle("Is Default", padMapping.isDefault);
			if (padMapping.isDefault && !wasDefault) {
				//we set this mapping to default, lets unset any other common mappings of the same OS from being default
				System.Object[] commonMappingAssets = Resources.LoadAll("", typeof(CommonMapping));
				for (int i = 0; i < commonMappingAssets.Length; i++) {
					if (((CommonMapping)commonMappingAssets[i]).os == padMapping.os) {
						((CommonMapping)commonMappingAssets[i]).isDefault = false;
						EditorUtility.SetDirty((CommonMapping)commonMappingAssets[i]);
					}
				}
				padMapping.isDefault = true;
			}
		}

		if (currentPanel==1){
			//button mapping menu
			if (padMapping.buttons.Count>0){
				//EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField("Button Type / Button ID / Display Name");
				//EditorGUILayout.EndHorizontal();
			}

			CommonMapping.GamepadButtonInput activeButton = new CommonMapping.GamepadButtonInput();
			for (int i=0; i<padMapping.buttons.Count; i++){
				EditorGUILayout.BeginHorizontal();
				activeButton = padMapping.buttons[i];
				activeButton.buttonType = (CommonGamepadInputs)EditorGUILayout.EnumPopup(padMapping.buttons[i].buttonType);
				activeButton.buttonNumber = EditorGUILayout.IntField( padMapping.buttons[i].buttonNumber);
				activeButton.displayName = EditorGUILayout.TextField( activeButton.displayName);
				activeButton.displayName = SinputFileIO.SanitiseStringForSaving(activeButton.displayName);
				padMapping.buttons[i] = activeButton;
				if (GUILayout.Button("x")){
					//remove button
					padMapping.buttons.RemoveAt(i);
				}
				EditorGUILayout.EndHorizontal();
			}
			if (GUILayout.Button("+")){
				//add button mapping name here
				CommonMapping.GamepadButtonInput newButtonInput = new CommonMapping.GamepadButtonInput();
				newButtonInput.buttonType = CommonGamepadInputs.NOBUTTON;
				newButtonInput.displayName = "[?]";
				newButtonInput.buttonNumber = 0;
				padMapping.buttons.Add(newButtonInput);
			}

		}

		if (currentPanel==2){
			//axis mapping menu
			if (axisEditFoldouts.Count != padMapping.axis.Count){
				axisEditFoldouts = new List<bool>();
				for (int i=0; i<padMapping.axis.Count; i++) axisEditFoldouts.Add(false);
			}

			CommonMapping.GamepadAxisInput activeAxis = new CommonMapping.GamepadAxisInput();
			bool delete = false;
			for (int i=0; i<padMapping.axis.Count; i++){
				axisEditFoldouts[i] = EditorGUILayout.Foldout(axisEditFoldouts[i],padMapping.axis[i].buttonType.ToString(), true);
				if (axisEditFoldouts[i]){
					delete = false;
					activeAxis = padMapping.axis[i];
					EditorGUILayout.BeginHorizontal();
					activeAxis.buttonType = (CommonGamepadInputs)EditorGUILayout.EnumPopup(activeAxis.buttonType);
					if (GUILayout.Button("x")) delete = true;
					EditorGUILayout.EndHorizontal();

					activeAxis.axisNumber = EditorGUILayout.IntField("Axis ID", activeAxis.axisNumber);

					activeAxis.displayName = EditorGUILayout.TextField("Display name", activeAxis.displayName);
					activeAxis.displayName = SinputFileIO.SanitiseStringForSaving(activeAxis.displayName);
					EditorGUILayout.Space();
					activeAxis.defaultVal=EditorGUILayout.FloatField("Default Value", activeAxis.defaultVal);
					activeAxis.invert = EditorGUILayout.Toggle("Invert", activeAxis.invert);
					activeAxis.clamp = EditorGUILayout.Toggle("Clamp to [0-1]", activeAxis.clamp);

					if (!activeAxis.rescaleAxis){
						activeAxis.rescaleAxis=EditorGUILayout.Toggle("Rescale to [0-1]", activeAxis.rescaleAxis);
					}else{
						EditorGUILayout.BeginHorizontal();
						activeAxis.rescaleAxis=EditorGUILayout.Toggle("Rescale to [0-1]", activeAxis.rescaleAxis);
						activeAxis.rescaleAxisMin=EditorGUILayout.FloatField(activeAxis.rescaleAxisMin);
						activeAxis.rescaleAxisMax=EditorGUILayout.FloatField(activeAxis.rescaleAxisMax);

						EditorGUILayout.EndHorizontal();
					}
					EditorGUILayout.Space();

					//EditorGUILayout.LabelField("Counts as pressed button when:");
					EditorGUILayout.BeginHorizontal();
					string compareStr = "Pressed if >";
					if (!activeAxis.compareGreater) compareStr = "Pressed if <";
					if (GUILayout.Button(compareStr)) activeAxis.compareGreater = !activeAxis.compareGreater;
					activeAxis.compareVal=EditorGUILayout.FloatField(activeAxis.compareVal);

					EditorGUILayout.EndHorizontal();

					padMapping.axis[i] = activeAxis;

					if (delete){
						//remove axis
						padMapping.axis.RemoveAt(i);
						axisEditFoldouts.RemoveAt(i);
						i--;
					}
					EditorGUILayout.Space();
				}

				//EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
			}
			EditorGUILayout.Space();
			if (GUILayout.Button("+")){
				//add axis mapping name here
				CommonMapping.GamepadAxisInput newAxisInput = new CommonMapping.GamepadAxisInput();
				newAxisInput.buttonType = CommonGamepadInputs.NOBUTTON;

				newAxisInput.axisNumber = 1;
				newAxisInput.invert = false;
				newAxisInput.clamp = false; //applied AFTER invert, to keep input result between 0 and 1

				//for using the axis as a button
				newAxisInput.compareGreater=true;//true is ([axisVal]>compareVal), false is ([axisVal]<compareVal)
				newAxisInput.compareVal=0.4f;//how var does have to go to count as "pressed" as a button

				newAxisInput.rescaleAxis=false;
				newAxisInput.rescaleAxisMin=0f;
				newAxisInput.rescaleAxisMax=1f;

				newAxisInput.defaultVal = 0f; //all GetAxis() checks will return default value until a measured change occurs, since readings before then can be wrong


				newAxisInput.displayName = "[?]";

				padMapping.axis.Add(newAxisInput);
				axisEditFoldouts.Add(true);
			}

		}


		if (EditorGUI.EndChangeCheck()){
			//something was changed
			EditorUtility.SetDirty(padMapping);
		}

	}
}
