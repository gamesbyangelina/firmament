using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using SinputSystems;

namespace SinputSystems{
	[CreateAssetMenu(fileName = "New Common Mapping", menuName = "Sinput/Common Mapping", order = 1)]
	public class CommonMapping : ScriptableObject {

		

		public List<string> names = new List<string>();//names of gamepads that this mapping can apply to
		public List<string> partialNames = new List<string>();//if an exact name match isn't found for a pad, use this mapping if it has a partial match with this

		public bool isDefault = false; //if no common mapping or saved custom binding is found, are we going to use this mapping

		public OSFamily os = OSFamily.Windows;

		public List<GamepadButtonInput> buttons = new List<GamepadButtonInput>();
		public List<GamepadAxisInput> axis = new List<GamepadAxisInput>();



		[System.Serializable]
		public struct GamepadButtonInput{
			public CommonGamepadInputs buttonType;
			public int buttonNumber;
			public string displayName;
		}

		[System.Serializable]
		public struct GamepadAxisInput{
			public CommonGamepadInputs buttonType;
			public int axisNumber;
			public bool invert;
			public bool clamp; //applied AFTER invert, to keep input result between 0 and 1

			//for using the axis as a button
			public bool compareGreater;//true is ([axisVal]>compareVal), false is ([axisVal]<compareVal)
			public float compareVal;//how var does have to go to count as "pressed" as a button

			public bool rescaleAxis;
			public float rescaleAxisMin;
			public float rescaleAxisMax;

			public float defaultVal; //all GetAxis() checks will return default value until a measured change occurs, since readings before then can be wrong


			public string displayName;
		}
	}
}
