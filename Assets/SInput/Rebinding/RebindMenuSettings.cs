using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SinputSystems.Rebinding {
	[CreateAssetMenu(fileName = "RebindMenuSettings", menuName = "Sinput/Rebind Menu Settings", order = 2)]
	public class RebindMenuSettings : ScriptableObject {

		public bool showSettings = true;

		public bool showMouseSensitivity = true;
		public float minMouseSens = 0.01f;
		public float maxMouseSens = 4f;

		public List<string> toggleableControls = new List<string>();
		public List<string> invertableSmartControls = new List<string>();
		public List<scalable> scalables = new List<scalable>();

		[System.Serializable]
		public struct scalable {
			public string scalableName;
			public List<string> scalableSmartControls;
			public float minScale;
			public float maxScale;
		}

	}
}
