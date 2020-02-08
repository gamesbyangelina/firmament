using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems {
	public static class CommonGamepadMappings {


		static List<CommonMapping> commonMappings;
		static MappingSlots[] mappingSlots;

		public static void ReloadCommonMaps() {
			//called when gamepads are plugged in or removed, also when Sinput is first called

			//Debug.Log("Loading common mapping");

			OSFamily thisOS = OSFamily.Other;
			if (Application.platform == RuntimePlatform.OSXEditor) thisOS = OSFamily.MacOSX;
			if (Application.platform == RuntimePlatform.OSXPlayer) thisOS = OSFamily.MacOSX;
			if (Application.platform == RuntimePlatform.WindowsEditor) thisOS = OSFamily.Windows;
			if (Application.platform == RuntimePlatform.WindowsPlayer) thisOS = OSFamily.Windows;
			if (Application.platform == RuntimePlatform.LinuxEditor) thisOS = OSFamily.Linux;
			if (Application.platform == RuntimePlatform.LinuxPlayer) thisOS = OSFamily.Linux;
			if (Application.platform == RuntimePlatform.Android) thisOS = OSFamily.Android;
			if (Application.platform == RuntimePlatform.IPhonePlayer) thisOS = OSFamily.IOS;
			if (Application.platform == RuntimePlatform.PS4) thisOS = OSFamily.PS4;
			if (Application.platform == RuntimePlatform.PSP2) thisOS = OSFamily.PSVita;
			if (Application.platform == RuntimePlatform.XboxOne) thisOS = OSFamily.XboxOne;
			if (Application.platform == RuntimePlatform.Switch) thisOS = OSFamily.Switch;

			System.Object[] commonMappingAssets = Resources.LoadAll("", typeof(CommonMapping));
			commonMappings = new List<CommonMapping>();
			string[] gamepads = Sinput.gamepads;
			int defaultMappingIndex = -1;
			for (int i = 0; i < commonMappingAssets.Length; i++) {
				if (((CommonMapping)commonMappingAssets[i]).os == thisOS) {
					bool gamepadConnected = false;
					bool partialMatch = false;
					for (int k = 0; k < ((CommonMapping)commonMappingAssets[i]).names.Count; k++) {
						for (int g = 0; g < gamepads.Length; g++) {
							if (((CommonMapping)commonMappingAssets[i]).names[k].ToUpper() == gamepads[g]) gamepadConnected = true;
						}
					}

					for (int k = 0; k < ((CommonMapping)commonMappingAssets[i]).partialNames.Count; k++) {
						for (int g = 0; g < gamepads.Length; g++) {
							if (gamepads[g].Contains(((CommonMapping)commonMappingAssets[i]).partialNames[k].ToUpper())) partialMatch = true;
						}
					}

					if (gamepadConnected) commonMappings.Add((CommonMapping)commonMappingAssets[i]);
					if (partialMatch && !gamepadConnected) commonMappings.Add((CommonMapping)commonMappingAssets[i]);
					if (!partialMatch && !gamepadConnected && ((CommonMapping)commonMappingAssets[i]).isDefault) commonMappings.Add((CommonMapping)commonMappingAssets[i]);

					if (((CommonMapping)commonMappingAssets[i]).isDefault) defaultMappingIndex = commonMappings.Count - 1;
				}
			}



			//for each common mapping, find which gamepad slots it applies to
			//inputs built from common mappings will only check slots which match
			mappingSlots = new MappingSlots[commonMappings.Count];
			for (int i = 0; i < mappingSlots.Length; i++) {
				mappingSlots[i].slots = new List<int>();
			}
			//string[] gamepads = Sinput.GetGamepads();
			for (int i = 0; i < commonMappings.Count; i++) {
				for (int k = 0; k < commonMappings[i].names.Count; k++) {
					for (int g = 0; g < gamepads.Length; g++) {
						if (gamepads[g] == commonMappings[i].names[k].ToUpper()) {
							mappingSlots[i].slots.Add(g);
						}
					}
				}
			}

			//find out if there are any connected gamepads that dont match anything in mappingSlots
			//then find 
			for (int g = 0; g < gamepads.Length; g++) {
				bool mappingMatch = false;
				for (int b = 0; b < mappingSlots.Length; b++) {
					for (int s = 0; s < mappingSlots[b].slots.Count; s++) {
						if (mappingSlots[b].slots[s] == g) mappingMatch = true;
					}
				}
				if (!mappingMatch) {
					//check for partial name matches with this gamepad slot
					for (int i = 0; i < commonMappings.Count; i++) {
						for (int k = 0; k < commonMappings[i].partialNames.Count; k++) {
							if (!mappingMatch && gamepads[g].Contains(commonMappings[i].partialNames[k])) {
								mappingMatch = true;
								mappingSlots[i].slots.Add(g);
							}
						}
					}
					if (!mappingMatch && defaultMappingIndex != -1) {
						//apply default common mapping to this slot
						mappingSlots[defaultMappingIndex].slots.Add(g);
					}
				}
			}

		}
		struct MappingSlots {
			public List<int> slots;
		}


		public static List<DeviceInput> GetApplicableMaps(CommonGamepadInputs t, string[] connectedGamepads) {
			//builds input mapping of type t for all known connected gamepads


			List<DeviceInput> applicableInputs = new List<DeviceInput>();


			for (int i = 0; i < commonMappings.Count; i++) {

				//add any applicable button mappings
				for (int k = 0; k < commonMappings[i].buttons.Count; k++) {
					if (commonMappings[i].buttons[k].buttonType == t) {
						//add this button input
						DeviceInput newInput = new DeviceInput(InputDeviceType.GamepadButton);
						newInput.gamepadButtonNumber = commonMappings[i].buttons[k].buttonNumber;
						newInput.commonMappingType = t;
						newInput.displayName = commonMappings[i].buttons[k].displayName;

						newInput.allowedSlots = mappingSlots[i].slots.ToArray();

						applicableInputs.Add(newInput);
					}
				}
				//add any applicable axis bingings
				for (int k = 0; k < commonMappings[i].axis.Count; k++) {
					if (commonMappings[i].axis[k].buttonType == t) {
						//add this axis input
						DeviceInput newInput = new DeviceInput(InputDeviceType.GamepadAxis);
						newInput.gamepadAxisNumber = commonMappings[i].axis[k].axisNumber;
						newInput.commonMappingType = t;
						newInput.displayName = commonMappings[i].axis[k].displayName;
						newInput.invertAxis = commonMappings[i].axis[k].invert;
						newInput.clampAxis = commonMappings[i].axis[k].clamp;
						newInput.axisButtoncompareVal = commonMappings[i].axis[k].compareVal;
						newInput.defaultAxisValue = commonMappings[i].axis[k].defaultVal;

						newInput.allowedSlots = mappingSlots[i].slots.ToArray();

						if (commonMappings[i].axis[k].rescaleAxis) {
							newInput.rescaleAxis = true;
							newInput.rescaleAxisMin = commonMappings[i].axis[k].rescaleAxisMin;
							newInput.rescaleAxisMax = commonMappings[i].axis[k].rescaleAxisMax;
						}

						applicableInputs.Add(newInput);
					}
				}

			}



			return applicableInputs;
		}

	}
}