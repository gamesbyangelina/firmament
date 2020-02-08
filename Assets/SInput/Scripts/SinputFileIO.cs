using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace SinputSystems{
	public static  class SinputFileIO {

		private static bool forceUsePlayerPrefs = false; //set to true to force sinput to use player prefs on all platforms

		private static bool usePlayerPrefs = false;

		private static string savePath;//folder we save the file to
		private static string saveFileName = "controls_";//prefix to look for when loading control schemes
		private static string saveExtension = ".dat";//files without an extension are ugly

		private static bool initialised = false;
		static void Init() {
			if (initialised) return;
			initialised = true;

			savePath = Application.persistentDataPath;

			usePlayerPrefs = true;
			if (Application.platform == RuntimePlatform.LinuxEditor || Application.platform == RuntimePlatform.LinuxPlayer ||
				Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer ||
				Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) usePlayerPrefs = false;

			if (forceUsePlayerPrefs) usePlayerPrefs = true;
		}


		public static bool SaveDataExists(string schemeName){
			Init();

			//string schemeName = "ControlScheme";
			if (usePlayerPrefs) {
				if (PlayerPrefs.GetString("snpt_" + saveFileName + schemeName, "") != "") return true;
			} else {
				if (!Directory.Exists(savePath)) return false;

				string[] files = Directory.GetFiles(savePath);
				for (int i = 0; i < files.Length; i++) {
					if (files[i].Contains(saveFileName + schemeName + saveExtension)) {
						if (File.Exists(files[i])) {
							return true;
						}
					}
				}
			}
			return false;
		}

		static string LoadString(string schemeName) {
			
			if (usePlayerPrefs) {
				return PlayerPrefs.GetString("snpt_" + saveFileName + schemeName, "");
			} else {
				if (!Directory.Exists(savePath)) return "";

				string[] files = Directory.GetFiles(savePath);
				for (int i = 0; i < files.Length; i++) {
					if (files[i].Contains(saveFileName + schemeName + saveExtension)) {
						if (!File.Exists(files[i])) return "";
						//Debug.Log("Loady load");
						return File.ReadAllText(files[i]);
					}
				}
			}

			return "";
		}

		//loading stuff
		static string activeControlName = "";
		static string[] joysticks;
		public static Control[] LoadControls(Control[] schemeToReplace, string schemeName){
			//we pass the existing control scheme so that info on needed common bindings can be kept
			Init();


			string loadString = LoadString(schemeName);

			return LoadFromString(schemeToReplace, loadString);
		}

		public static Control[] LoadFromString(Control[] schemeToReplace, string loadString) {
			//we pass the existing control scheme so that info on needed common bindings can be kept

			//create our new controls list, and add any common bindings we might need to load later when pads are swapped in/out
			List<Control> controls = new List<Control>();
			for (int c = 0; c < schemeToReplace.Length; c++) {
				controls.Add(new Control(schemeToReplace[c].name));
				controls[c].commonMappings = new List<CommonGamepadInputs>();
				for (int b = 0; b < schemeToReplace[c].commonMappings.Count; b++) {
					controls[c].commonMappings.Add(schemeToReplace[c].commonMappings[b]);
				}
			}

			//now add saved data to the list (and to other places)
			string[] loadLines = loadString.Split('\n');
			joysticks = Input.GetJoystickNames();
			for (int l = 0; l < loadLines.Length; l++) {
				string[] thisLine = loadLines[l].Split(seperator[0]);

				if (thisLine[0] == "controls") {
					int savedControlCount = 0;
					if (int.TryParse(thisLine[1], out savedControlCount)) {

						for (int c = 0; c < savedControlCount; c++) {
							l++;
							//get the string that describes this control
							thisLine = loadLines[l].Split(seperator[0]);
							//build the control
							//Control currentControl = new Control(thisLine[0]);
							//currentControl.isToggle = thisLine[1] == "True";
							

							int currentControlIndex = -1;
							for (int cc=0; cc<controls.Count; cc++) {
								if (controls[cc].name == thisLine[0]) currentControlIndex = cc;
							}
							if (currentControlIndex == -1) {
								controls.Add(new Control(thisLine[0]));
								currentControlIndex = controls.Count - 1;
							}
							controls[currentControlIndex].isToggle = thisLine[1] == "True";

							int savedInputCount = int.Parse(thisLine[2]);

							//add inputs

							for (int i = 0; i < savedInputCount; i++) {
								l++;
								//get the string that describes this input
								thisLine = loadLines[l].Split(seperator[0]);
								//build the input
								DeviceInput currentInput = new DeviceInput((InputDeviceType)Enum.Parse(typeof(InputDeviceType), thisLine[0]));

								currentInput.displayName = thisLine[1];
								currentInput.isCustom = thisLine[2] == "True";
								currentInput.deviceName = thisLine[3];

								if (currentInput.inputType == InputDeviceType.Keyboard) {
									currentInput.keyboardKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), thisLine[4]);
								}
								if (currentInput.inputType == InputDeviceType.Mouse) {
									currentInput.mouseInputType = (MouseInputType)Enum.Parse(typeof(MouseInputType), thisLine[4]);
								}
								if (currentInput.inputType == InputDeviceType.GamepadButton) {
									currentInput.gamepadButtonNumber = int.Parse(thisLine[4]);
								}
								if (currentInput.inputType == InputDeviceType.GamepadAxis) {
									currentInput.gamepadAxisNumber = int.Parse(thisLine[4]);
									currentInput.invertAxis = thisLine[5] == "True";
									currentInput.clampAxis = thisLine[6] == "True";
									currentInput.rescaleAxis = thisLine[7] == "True";
									currentInput.rescaleAxisMin = float.Parse(thisLine[8]);
									currentInput.rescaleAxisMax = float.Parse(thisLine[9]);
								}
								if (currentInput.inputType == InputDeviceType.Virtual) {
									currentInput.virtualInputID = thisLine[4];
								}

								//lets not forget stuff that isn't saved, but needed anyway
								currentInput.commonMappingType = CommonGamepadInputs.NOBUTTON;
								currentInput.defaultAxisValue = 0f;
								if (currentInput.inputType == InputDeviceType.GamepadAxis || currentInput.inputType == InputDeviceType.GamepadButton) {
									List<int> allowedSlots = new List<int>();
									for (int j = 0; j < joysticks.Length; j++) {
										if (joysticks[j].ToUpper() == currentInput.deviceName.ToUpper()) allowedSlots.Add(j);
									}
									currentInput.allowedSlots = allowedSlots.ToArray();
								}


								//add the input to the control's input list
								controls[currentControlIndex].inputs.Add(currentInput);
							}

							//add this control to the list
							//controls.Add(currentControl);
						}

					} else {
						Debug.LogError("Can't read number of saved controls.");
					}
				}

				if (thisLine[0] == "smartcontrols") {
					int savedSmartControlCount = 0;
					if (int.TryParse(thisLine[1], out savedSmartControlCount)) {

						for (int c = 0; c < savedSmartControlCount; c++) {
							l++;
							string currentSmartControl = loadLines[l];

							//get the inversion settings
							l++;
							thisLine = loadLines[l].Split(seperator[0]);
							for (int i = 0; i < thisLine.Length; i++) {
								Sinput.SetInverted(currentSmartControl, thisLine[i] == "True", (InputDeviceSlot)i);
							}

							//get the scale settings
							l++;
							thisLine = loadLines[l].Split(seperator[0]);
							for (int i = 0; i < thisLine.Length; i++) {
								Sinput.SetScale(currentSmartControl, float.Parse(thisLine[i]), (InputDeviceSlot)i);
							}

						}

					} else {
						Debug.LogError("Can't read number of saved smart controls.");
					}
				}

				if (thisLine[0] == "mouseSensitivity") {
					Sinput.mouseSensitivity = float.Parse(thisLine[1]);
				}
			}


			return controls.ToArray();
		}

		

		static void SaveControlsToFile(string schemeName, string saveString) {

			if (!File.Exists(savePath)) {
				Directory.CreateDirectory(savePath);
			}
			File.WriteAllText(savePath + "\\" + saveFileName + schemeName + saveExtension, saveString);
			//Debug.Log("saved to " + savePath + "\\" + saveFileName + schemeName + saveExtension);
		}
		static void SaveControlsToPlayerPrefs(string schemeName, string saveString) {

			PlayerPrefs.SetString("snpt_" + saveFileName + schemeName, saveString);
		}

		//saving stuff
		public static void SaveControls(Control[] controls, string schemeName){
			Init();

			DeleteSavedControls(schemeName);//delete existing controls so we dont have stray inputs saved that have since been removed

			if (usePlayerPrefs) {
				SaveControlsToPlayerPrefs(schemeName, GenerateSaveString(controls));
				
			} else {
				SaveControlsToFile(schemeName, GenerateSaveString(controls));
			}
			
		}

		

		public static string SanitiseStringForSaving(string s) {
			if (!s.Contains(seperator) && !s.Contains("\n")) return s;

			string retStr = "";
			string substr = "";
			for (int i=0; i<s.Length; i++) {
				substr = s.Substring(i, 1);
				if (substr != "\n" && substr != seperator) retStr += substr;
			}

			return retStr;
		}

		static string seperator = "|";
		static string GenerateSaveString(Control[] controls) {
			string saveStr = "";

			//save controls to the string
			saveStr += "controls" + seperator + controls.Length.ToString() + "\n";
			for (int c=0; c<controls.Length; c++) {
				saveStr += controls[c].name + seperator + controls[c].isToggle.ToString() + seperator + controls[c].inputs.Count.ToString() + "\n";
				for (int i = 0; i < controls[c].inputs.Count; i++) {
					saveStr += controls[c].inputs[i].inputType.ToString() + seperator;
					saveStr += controls[c].inputs[i].displayName + seperator;
					saveStr += controls[c].inputs[i].isCustom.ToString() + seperator;
					saveStr += controls[c].inputs[i].deviceName + seperator;

					//keyboard input specifics
					if (controls[c].inputs[i].inputType == InputDeviceType.Keyboard) {
						saveStr += controls[c].inputs[i].keyboardKeyCode.ToString() + seperator;
					}

					//mouse input specifics
					if (controls[c].inputs[i].inputType == InputDeviceType.Mouse) {
						saveStr += controls[c].inputs[i].mouseInputType.ToString() + seperator;
					}

					//gamepad button input specifics
					if (controls[c].inputs[i].inputType == InputDeviceType.GamepadButton) {
						saveStr += controls[c].inputs[i].gamepadButtonNumber.ToString() + seperator;
					}

					//gamepad axis input specifics
					if (controls[c].inputs[i].inputType == InputDeviceType.GamepadAxis) {
						saveStr += controls[c].inputs[i].gamepadAxisNumber.ToString() + seperator;
						saveStr += controls[c].inputs[i].invertAxis.ToString() + seperator;
						saveStr += controls[c].inputs[i].clampAxis.ToString() + seperator;
						saveStr += controls[c].inputs[i].rescaleAxis.ToString() + seperator;
						saveStr += controls[c].inputs[i].rescaleAxisMin.ToString() + seperator;
						saveStr += controls[c].inputs[i].rescaleAxisMax.ToString() + seperator;
					}

					//virtual input specifics
					if (controls[c].inputs[i].inputType == InputDeviceType.Virtual) {
						saveStr += controls[c].inputs[i].virtualInputID.ToString() + seperator;
					}

					saveStr += "\n";
				}
			}

			//save smart controls to the string
			saveStr += "smartcontrols" + seperator + Sinput.smartControls.Length.ToString() + "\n";
			for (int s = 0; s < Sinput.smartControls.Length; s++) {
				saveStr += Sinput.smartControls[s].name + "\n";
				//inversion settings for each slot
				for (int i = 0; i < Sinput.totalPossibleDeviceSlots; i++) {
					saveStr += Sinput.smartControls[s].inversion[i].ToString();
					if (i < Sinput.totalPossibleDeviceSlots - 1) saveStr += seperator;
				}
				saveStr += "\n";

				//scale settings for each slot
				for (int i = 0; i < Sinput.totalPossibleDeviceSlots; i++) {
					saveStr += Sinput.smartControls[s].scales[i].ToString();
					if (i < Sinput.totalPossibleDeviceSlots - 1) saveStr += seperator;
				}
				saveStr += "\n";
			}

			//save other settings to the string
			saveStr += "mouseSensitivity" + seperator + Sinput.mouseSensitivity.ToString();

			return saveStr;
		}
		

		//deleting stuff
		public static void DeleteSavedControls(string schemeName){

			Init();

			//string schemeName = "ControlScheme";
			if (usePlayerPrefs) {
				PlayerPrefs.DeleteKey("snpt_" + saveFileName + schemeName);
			} else {
				if (!File.Exists(savePath)) {
					Directory.CreateDirectory(savePath);
				}
				if (File.Exists(savePath + "\\" + saveFileName + schemeName + saveExtension)) {
					File.Delete(savePath + "\\" + saveFileName + schemeName + saveExtension);
				}
			}

			return;

			
		}
		

	}
}